//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public class ReleaseReferenceHelper
    {
        private const int c_defaultObjectStackCapacity = 100;
        private const int c_defaultArrayStackCapacity = 10;

        private UIntPtr[] m_objectStack;
        private int       m_objectStackPos;

        // Helper struct to store the tracking information when traversing through
        // the arrays.
        struct ArrayInfo
        {
            private UIntPtr m_address;
            private uint m_elementSize;
            private uint m_numOfElementsLeft;
            private TS.VTable m_vTableElement;

            // Populate the struct with the specified array
            public unsafe void Push( ArrayImpl array, uint elementSize, uint numOfElements, TS.VTable vTableElement )
            {
                m_address = new UIntPtr( array.GetEndDataPointer( ) );
                m_elementSize = elementSize;
                m_numOfElementsLeft = numOfElements;
                m_vTableElement = vTableElement;
            }

            // Clear the struct and return the address of the array's ObjectHeader 
            public UIntPtr Pop( )
            {
                BugCheck.Assert( m_numOfElementsLeft == 0, BugCheck.StopCode.InvalidOperation );

                // Need to calculate the address of the array's ObjectHeader to return
                UIntPtr arrayImplAddress = AddressMath.Decrement(m_address, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(ArrayImpl)));
                Object arrayObject = ObjectImpl.CastAsObject(arrayImplAddress);
                ObjectHeader arrayObjectHeader = ObjectHeader.Unpack(arrayObject);
                UIntPtr arrayObjectHeaderPtr = arrayObjectHeader.ToPointer();

                m_address = UIntPtr.Zero;
                m_vTableElement = null;

                return arrayObjectHeaderPtr;
            }

            // Get the next set of element to visit. Return true when we've reached the last one.
            public bool GetNext( out UIntPtr address, out TS.VTable vTable )
            {
                // Move to the previous element
                m_address = AddressMath.Decrement( m_address, m_elementSize );
                m_numOfElementsLeft--;

                address = m_address;
                vTable = m_vTableElement;

                return m_numOfElementsLeft == 0;
            }
        }

        private ArrayInfo[] _arrayStack;
        private int _arrayStackPos;

        public ReleaseReferenceHelper( )
            : this( c_defaultObjectStackCapacity, c_defaultArrayStackCapacity )
        { }

        public ReleaseReferenceHelper( int objectStackCapacity, int arrayStackCapacity )
        {
            m_objectStack = new UIntPtr[ objectStackCapacity ];
            m_objectStackPos = 0;

            _arrayStack = new ArrayInfo[ arrayStackCapacity ];
            _arrayStackPos = 0;
        }

        public bool ReleaseReference( ObjectHeader oh )
        {
            Log( "ReleaseReference 0x%x", (int)oh.ToPointer( ).ToUInt32( ) );

            // DecrementRefCount returns true when the ref count of oh reaches 0
            if(DecrementRefCount( oh ))
            {
                while(true)
                {
                    if(!this.IsObjectStackEmpty)
                    {
                        VisitObjectStack( );
                        continue;
                    }

                    if(!this.IsArrayStackEmpty)
                    {
                        VisitArrayStack( );
                        continue;
                    }

                    break;
                }

                return true;
            }

            return false;
        }

        // Helper that decrement the ref count of oh, and if necessary, 
        // Add the zombie objects to the appropriate stack.
        // Return true if the ref count reaches 0.
        private bool DecrementRefCount( ObjectHeader oh )
        {
            if(oh.ExtensionKind == ObjectHeader.ExtensionKinds.ReferenceCount &&
                oh.DecrementReferenceCount())
            {
                if(oh.VirtualTable.IsArray)
                {
                    PushToArrayStack( oh );
                }
                else
                {
                    PushToObjectStack( oh.ToPointer( ) );
                }

                return true;
            }

            return false;
        }

        // Helper that goes through each of the interesting field and decrement
        // their ref counts. Note that the baseAddress is at the start of the fields.
        private unsafe void DecrementFieldsRefCount( UIntPtr baseAddress, TS.VTable vTable )
        {
            TS.GCInfo.Pointer[] pointers = vTable.GCInfo.Pointers;
            int numOfPointers = (pointers != null) ? pointers.Length : 0;
            UIntPtr* fieldAddress = (UIntPtr*)baseAddress.ToPointer();

            Log( "DecrementFieldsRefCount baseAddress:0x%x, numPointers:%d", (int)baseAddress.ToUInt32( ), numOfPointers );

            for(int i = 0; i < numOfPointers; i++)
            {
                TS.GCInfo.Pointer pointer = pointers[i];
                UIntPtr referenceAddress = fieldAddress[pointer.OffsetInWords];

                Log( "Pointer %d: kind:%d, offset:%d, ref:0x%x",
                    i, (int)pointer.Kind, pointer.OffsetInWords, (int)referenceAddress.ToUInt32( ) );

                if(referenceAddress != UIntPtr.Zero && pointer.Kind == TS.GCInfo.Kind.Heap)
                {
                    DecrementRefCount( ObjectHeader.CastAsObjectHeader( referenceAddress ) );
                }
            }
        }

        // Helper that calls the finalizer and free up the memory.
        private void DeleteObject( UIntPtr objectHeaderAddress )
        {
            ObjectHeader oh = ObjectHeader.CastAsObjectHeader(objectHeaderAddress);

            Log( "Releasing oh:0x%x from memory.", (int)objectHeaderAddress.ToUInt32( ) );
            ( (ObjectImpl)oh.Pack( ) ).FinalizeImpl( );
            MemoryManager.Instance.Release( objectHeaderAddress );
        }

        // Helper called by the main loop to iterate through the object stack.
        private void VisitObjectStack( )
        {
            ObjectHeader oh = ObjectHeader.CastAsObjectHeader(PopFromObjectStack());

            Log( "VisitObjectStack address:0x%x", (int)oh.ToPointer( ).ToUInt32( ) );

            UIntPtr baseAddress = ((ObjectImpl)oh.Pack()).CastAsUIntPtr();
            DecrementFieldsRefCount( baseAddress, oh.VirtualTable );
            DeleteObject( oh.ToPointer( ) );
        }

        // Helper called by the main loop to iterate through the array stack.
        private unsafe void VisitArrayStack( )
        {
            UIntPtr address;
            TS.VTable vTable;
            bool isLast = _arrayStack[_arrayStackPos - 1].GetNext(out address, out vTable);

            Log( "VisitArrayStack address:0x%x", (int)address.ToUInt32( ) );

            if(vTable != null)
            {
                // address points to a struct, so visit each field of the struct and 
                // decrement the ref count accordingly
                DecrementFieldsRefCount( address, vTable );
            }
            else
            {
                // address points to an object, so make sure it's not null and decrement
                // its ref count
                UIntPtr* ptr = (UIntPtr*)address.ToPointer();
                UIntPtr obj = ptr[0];

                if(obj != UIntPtr.Zero)
                {
                    DecrementRefCount( ObjectHeader.CastAsObjectHeader( obj ) );
                }
            }

            if(isLast)
            {
                // When we've gone through all the elements in this array, we can safely delete
                // array object from memory
                DeleteObject( PopFromArrayStack( ) );
            }
        }

        private void PushToObjectStack( UIntPtr objectHeaderPtr )
        {
            Log( "Adding 0x%x to delete object stack at pos:%d", (int)objectHeaderPtr.ToUInt32( ), m_objectStackPos );

            BugCheck.Assert( m_objectStackPos < m_objectStack.Length - 1, BugCheck.StopCode.NoMarkStack );

            m_objectStack[ m_objectStackPos++ ] = objectHeaderPtr;
        }

        private void PushToArrayStack( ObjectHeader oh )
        {
            ArrayImpl array = ArrayImpl.CastAsArray(oh.Pack());
            uint numOfElements = (uint)array.Length;

            if(numOfElements == 0)
            {
                // Empty array, we can just delete the array object itself and return.
                DeleteObject( oh.ToPointer( ) );
                return;
            }

            TS.VTable vTableElement = oh.VirtualTable.TypeInfo.ContainedType.VirtualTable;

            if(vTableElement.IsValueType)
            {
                if(vTableElement.GCInfo.Pointers == null)
                {
                    // It's an array of value types with no pointers, no need to push it.
                    // Just delete the array object and return.
                    DeleteObject( oh.ToPointer( ) );
                    return;
                }

                // This is an array of structs.
            }
            else
            {
                // This is an array of objects, which has its own vTable embedded in its ObjectHeader,
                // so set vTableElement to null to indicate such.
                vTableElement = null;
            }

            Log( "Adding 0x%x (len %d) to delete array stack at pos:%d", (int)oh.ToPointer( ).ToUInt32( ), (int)numOfElements, _arrayStackPos );

            BugCheck.Assert( _arrayStackPos < _arrayStack.Length - 1, BugCheck.StopCode.NoMarkStack );

            _arrayStack[ _arrayStackPos++ ].Push( array, oh.VirtualTable.ElementSize, numOfElements, vTableElement );
        }

        private UIntPtr PopFromObjectStack( )
        {
            UIntPtr objectHeaderPtr = m_objectStack[--m_objectStackPos];

            Log( "Removing 0x%x from delete object stack at pos:%d", (int)objectHeaderPtr.ToUInt32( ), m_objectStackPos );

            return objectHeaderPtr;
        }

        private UIntPtr PopFromArrayStack( )
        {
            UIntPtr objectHeaderPtr = _arrayStack[--_arrayStackPos].Pop();

            Log( "Removing 0x%x from delete array stack at pos:%d", (int)objectHeaderPtr.ToUInt32( ), _arrayStackPos );

            return objectHeaderPtr;
        }

        private bool IsObjectStackEmpty
        {
            get
            {
                return m_objectStackPos == 0;
            }
        }

        private bool IsArrayStackEmpty
        {
            get
            {
                return _arrayStackPos == 0;
            }
        }

#if RELEASEREFERENCEHELPER_LOG
        private static void Log( string format )
        {
            Log( format );
        }

        private static void Log( string format, int p1 )
        {
            Log( format, p1 );
        }

        private static void Log( string format, int p1, int p2 )
        {
            Log( format, p1, p2 );
        }

        private static void Log( string format, int p1, int p2, int p3 )
        {
            Log( format, p1, p2, p3 );
        }

        private static void Log( string format, int p1, int p2, int p3, int p4 )
        {
            Log( format, p1, p2, p3, p4 );
        }

        private static void Log( string format, int p1, int p2, int p3, int p4, int p5 )
        {
            Log( format, p1, p2, p3, p4, p5 );
        }
#else
        private static void Log( string format )
        {
        }

        private static void Log( string format, int p1 )
        {
        }

        private static void Log( string format, int p1, int p2 )
        {
        }

        private static void Log( string format, int p1, int p2, int p3 )
        {
        }

        private static void Log( string format, int p1, int p2, int p3, int p4 )
        {
        }

        private static void Log( string format, int p1, int p2, int p3, int p4, int p5 )
        {
        }
#endif

    }
}

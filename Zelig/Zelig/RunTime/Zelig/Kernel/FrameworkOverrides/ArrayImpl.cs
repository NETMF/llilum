//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections.Generic;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Array), NoConstructors=true)]
    public class ArrayImpl
    {
        //
        // State
        //

        [TS.WellKnownField( "ArrayImpl_m_numElements" )]
        internal uint m_numElements;

        //
        // Constructor Methods
        //

        private ArrayImpl()
        {
            m_numElements = 0;
        }

        //--//

        //
        // Helper Methods
        //

        public int GetUpperBound( int dimension )
        {
            TS.VTable vTable = TS.VTable.Get( this );

            TS.TypeRepresentation ts = vTable.TypeInfo;

            if(ts is TS.SzArrayReferenceTypeRepresentation)
            {
                return (int)(m_numElements - 1);
            }

            TS.MultiArrayReferenceTypeRepresentation ts2 = (TS.MultiArrayReferenceTypeRepresentation)ts;

            return (int)ts2.Dimensions[dimension].m_upperBound;
        }

        public int GetLowerBound( int dimension )
        {
            TS.VTable vTable = TS.VTable.Get( this );

            TS.TypeRepresentation ts = vTable.TypeInfo;

            if(ts is TS.SzArrayReferenceTypeRepresentation)
            {
                return 0;
            }

            TS.MultiArrayReferenceTypeRepresentation ts2 = (TS.MultiArrayReferenceTypeRepresentation)ts;

            return (int)ts2.Dimensions[dimension].m_lowerBound;
        }

        public static unsafe void Clear( ArrayImpl array  ,
                                         int       index  ,
                                         int       length )
        {
            if(index  < 0 ||
               length < 0  )
            {
                throw new IndexOutOfRangeException();
            }

            int indexEnd = index + length;
            if(indexEnd > array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            TS.VTable vTable = TS.VTable.Get( array );

            void* voidPtr    = array.GetPointerToElement( (uint)index    );
            void* voidPtrEnd = array.GetPointerToElement( (uint)indexEnd );

            if((vTable.ElementSize & 3) == 0)
            {
                //
                // Word aligned.
                //
                uint* ptr    = (uint*)voidPtr;
                uint* ptrEnd = (uint*)voidPtrEnd;

                while(ptr < ptrEnd)
                {
                    *ptr++ = 0;
                }
            }
            else
            {
                byte* ptr    = (byte*)voidPtr;
                byte* ptrEnd = (byte*)voidPtrEnd;

                while(ptr < ptrEnd)
                {
                    *ptr++ = 0;
                }
            }
        }

        internal static unsafe void Copy( ArrayImpl sourceArray      ,
                                          int       sourceIndex      ,
                                          ArrayImpl destinationArray ,
                                          int       destinationIndex ,
                                          int       length           ,
                                          bool      reliable         )
        {
            if(sourceIndex      < 0 ||
               destinationIndex < 0 ||
               length           < 0  )
            {
                throw new IndexOutOfRangeException();
            }

            int sourceIndexEnd = sourceIndex + length;
            if(sourceIndexEnd > sourceArray.Length)
            {
                throw new IndexOutOfRangeException();
            }

            int destinationIndexEnd = destinationIndex + length;
            if(destinationIndexEnd > destinationArray.Length)
            {
                throw new IndexOutOfRangeException();
            }

            TS.VTable vTableSource      = TS.VTable.Get( sourceArray      );
            TS.VTable vTableDestination = TS.VTable.Get( destinationArray );

            if(vTableSource != vTableDestination)
            {
                throw new NotSupportedException();
            }

            void* voidSourcePtr      = sourceArray     .GetPointerToElement( (uint)sourceIndex      );
            void* voidDestinationPtr = destinationArray.GetPointerToElement( (uint)destinationIndex );

            if(voidSourcePtr != voidDestinationPtr)
            {
                BufferImpl.InternalMemoryMove( (byte*)voidSourcePtr, (byte*)voidDestinationPtr, length * (int)vTableSource.ElementSize );
            }
        }

        //--//

        //
        // This is used to cast between an object and and ArrayImpl, which is not possible in C#.
        //
        [TS.GenerateUnsafeCast]
        public extern static ArrayImpl CastAsArray( object target );

        //
        // This is used to cast between an object and and ArrayImpl, which is not possible in C#.
        //
        [TS.GenerateUnsafeCast]
        public extern Array CastThisAsArray();

        //
        // This is used to get the pointer to the data, which is not possible in C#.
        //
        [Inline]
        public unsafe uint* GetDataPointer()
        {
            fixed(uint* ptr = &m_numElements)
            {
                return &ptr[1];
            }
        }

        //
        // This is used to get the pointer to the data, which is not possible in C#.
        //
        public unsafe void* GetPointerToElement( uint index )
        {
            byte* ptr = (byte*)GetDataPointer();

            return &ptr[index * this.ElementSize];
        }

        //
        // This is used to get the pointer to the data, which is not possible in C#.
        //
        public unsafe uint* GetEndDataPointer()
        {
            return (uint*)GetPointerToElement( m_numElements );
        }

        //--//

        internal void SetLength( uint numElements )
        {
            m_numElements = numElements;
        }

        //--//

        [NoInline]
        [NoReturn]
        internal static void Throw_FixedSizeCollection()
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( "NotSupported_FixedSizeCollection" );
#else
            throw new NotSupportedException();
#endif
        }

        [NoInline]
        [NoReturn]
        internal static void Throw_ReadOnlyCollection()
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( "NotSupported_ReadOnlyCollection" );
#else
            throw new NotSupportedException();
#endif
        }

        [NoInline]
        internal static void EnsureSZArray( Array array )
        {
            if(array != null && array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( "Rank_MultiDimNotSupported" );
#else
                throw new ArgumentException();
#endif
            }
        }

        //

        //--//

        //
        // Access Methods
        //

        public int Length
        {
            [Inline]
            [TS.WellKnownMethod( "ArrayImpl_get_Length" )]
            get
            {
                return (int)m_numElements;
            }
        }

        public uint Size
        {
            [Inline]
            get
            {
                return m_numElements * this.ElementSize;
            }
        }

        public uint ElementSize
        {
            [Inline]
            get
            {
                return TS.VTable.Get( this ).ElementSize;
            }
        }

        public int Rank
        {
            get
            {
                TS.VTable vTable = TS.VTable.Get( this );

                TS.TypeRepresentation ts = vTable.TypeInfo;

                if(ts is TS.SzArrayReferenceTypeRepresentation)
                {
                    return 1;
                }

                TS.MultiArrayReferenceTypeRepresentation ts2 = (TS.MultiArrayReferenceTypeRepresentation)ts;

                return (int)ts2.Dimensions.Length;
            }
        }

        //--//
        
        public static uint[] InitializeFromRawMemory( UIntPtr baseAddress, uint sizeInBytes )
        {
            BugCheck.Assert( sizeInBytes % 4 == 0, BugCheck.StopCode.IncorrectArgument );
            
            TS.VTable vTable        = TS.VTable.GetFromType( typeof(uint[]) );
            uint      numOfElements = (sizeInBytes - MemoryFreeBlock.FixedSize() ) / sizeof( uint ); 
            
            uint[] externalRepresentation = (uint[])TypeSystemManager.Instance.InitializeArray( baseAddress, vTable, numOfElements, referenceCounting: false );

            ObjectHeader oh = ObjectHeader.Unpack( externalRepresentation );

            oh.MultiUseWord = (int)(ObjectHeader.GarbageCollectorFlags.FreeBlock | ObjectHeader.GarbageCollectorFlags.Unmarked);

            return externalRepresentation;
        }
    }

    //----------------------------------------------------------------------------------------
    // ! READ THIS BEFORE YOU WORK ON THIS CLASS.
    //
    // This class is needed to allow an SZ array of type T[] to expose IList<T>,
    // IList<T.BaseType>, etc., etc. all the way up to IList<Object>. When the following call is
    // made:
    //
    //   ((IList<T>) (new U[n])).SomeIListMethod()
    //
    // the interface stub dispatcher treats this as a special case, loads up SZArrayHelper,
    // finds the corresponding generic method (matched simply by method name), instantiates
    // it for type <T> and executes it.
    //
    // The "T" will reflect the interface used to invoke the method. The actual runtime "this" will be
    // array that is castable to "T[]" (i.e. for primitivs and valuetypes, it will be exactly
    // "T[]" - for orefs, it may be a "U[]" where U derives from T.)
    //----------------------------------------------------------------------------------------
    [TS.WellKnownType( "Microsoft_Zelig_Runtime_SZArrayHelper" )]
    static class SZArrayHelper<T>
    {
        // -----------------------------------------------------------
        // ------- Implement IEnumerable<T> interface methods --------
        // -----------------------------------------------------------
        [Inline]
        internal static IEnumerator<T> GetEnumerator( T[] _this )
        {
            //! Warning: "this" is an array, not an SZArrayHelper. See comments above
            //! or you may introduce a security hole!
            return new SZGenericArrayEnumerator( _this );
        }
    
        // -----------------------------------------------------------
        // ------- Implement ICollection<T> interface methods --------
        // -----------------------------------------------------------
        [Inline]
        internal static void CopyTo( T[] _this ,
                                     T[] array ,
                                     int index )
        {
            ArrayImpl.EnsureSZArray( array );
    
            Array.Copy( _this, 0, array, index, _this.Length );
        }
    
        [Inline]
        internal static int get_Count( T[] _this )
        {
            return _this.Length;
        }
    
        // -----------------------------------------------------------
        // ---------- Implement IList<T> interface methods -----------
        // -----------------------------------------------------------
        [Inline]
        internal static T get_Item( T[] _this ,
                                    int index )
        {
////        if((uint)index >= (uint)_this.Length)
////        {
////            ThrowHelper.ThrowArgumentOutOfRangeException();
////        }
    
            return _this[index];
        }
    
        [Inline]
        internal static void set_Item( T[] _this ,
                                       int index ,
                                       T   value )
        {
////        if((uint)index >= (uint)_this.Length)
////        {
////            ThrowHelper.ThrowArgumentOutOfRangeException();
////        }
    
            _this[index] = value;
        }
    
        [Inline]
        internal static void Add( T[] _this ,
                                  T   value )
        {
            ArrayImpl.Throw_FixedSizeCollection();
        }
    
        [Inline]
        internal static bool Contains( T[] _this ,
                                       T   value )
        {
            return Array.IndexOf( _this, value ) != -1;
        }
    
        [Inline]
        internal static bool get_IsReadOnly( T[] _this )
        {
            return true;
        }
    
        [Inline]
        internal static void Clear( T[] _this )
        {
            ArrayImpl.Throw_ReadOnlyCollection();
        }
    
        [Inline]
        internal static int IndexOf( T[] _this ,
                                     T   value )
        {
            return Array.IndexOf( _this, value );
        }
    
        [Inline]
        internal static void Insert( T[] _this ,
                                     int index ,
                                     T   value )
        {
            // Not meaningful for arrays
            ArrayImpl.Throw_FixedSizeCollection();
        }
    
        [Inline]
        internal static bool Remove( T[] _this ,
                                     T   value )
        {
            // Not meaningful for arrays
            ArrayImpl.Throw_FixedSizeCollection();

            return false;
        }
    
        [Inline]
        internal static void RemoveAt( T[] _this ,
                                       int index )
        {
            // Not meaningful for arrays
            ArrayImpl.Throw_FixedSizeCollection();
        }
    
        // This is a normal generic Enumerator for SZ arrays. It doesn't have any of the "this" voodoo
        // that SZArrayHelper does.
        //
        [Serializable]
        private sealed class SZGenericArrayEnumerator : System.Collections.Generic.IEnumerator<T>
        {
            private T[] m_array;
            private int m_index;
            private int m_endIndex; // cache array length, since it's a little slow.
    
            internal SZGenericArrayEnumerator( T[] array )
            {
////            BCLDebug.Assert( array.Rank == 1 && array.GetLowerBound( 0 ) == 0, "SZArrayEnumerator<T> only works on single dimension arrays w/ a lower bound of zero." );
    
                m_array    = array;
                m_index    = -1;
                m_endIndex = array.Length;
            }
    
            public bool MoveNext()
            {
                if(m_index < m_endIndex)
                {
                    m_index++;
    
                    return (m_index < m_endIndex);
                }
                return false;
            }
    
            public T Current
            {
                get
                {
////                if(m_index < 0)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( ResId.InvalidOperation_EnumNotStarted ) );
////                }
////
////                if(m_index >= m_endIndex)
////                {
////                    throw new InvalidOperationException( Environment.GetResourceString( ResId.InvalidOperation_EnumEnded ) );
////                }
    
                    return m_array[m_index];
                }
            }
    
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
    
            void System.Collections.IEnumerator.Reset()
            {
                m_index = -1;
            }
    
            public void Dispose()
            {
            }
        }
    }
}

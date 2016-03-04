//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class TypeSystemManager
    {
        class EmptyManager : TypeSystemManager
        {
            [NoInline]
            public override Object AllocateObject(TS.VTable vTable)
            {
                return null;
            }

            [NoInline]
            public override Object AllocateReferenceCountingObject(TS.VTable vTable)
            {
                return null;
            }

            [NoInline]
            public override Object AllocateObjectWithExtensions(TS.VTable vTable)
            {
                return null;
            }

            [NoInline]
            public override Array AllocateArray(TS.VTable vTable,
                                                 uint length)
            {
                return null;
            }

            [NoInline]
            public override Array AllocateReferenceCountingArray(TS.VTable vTable,
                                                                  uint length)
            {
                return null;
            }

            [NoInline]
            public override Array AllocateArrayNoClear(TS.VTable vTable,
                                                        uint length)
            {
                return null;
            }

            [NoInline]
            public override String AllocateString(TS.VTable vTable,
                                                   int length)
            {
                return null;
            }

            [NoInline]
            public override String AllocateReferenceCountingString(TS.VTable vTable,
                                                                    int length)
            {
                return null;
            }
        }

        //
        // Helper Methods
        //

        public virtual void InitializeTypeSystemManager()
        {
            InvokeStaticConstructors();
        }

        [Inline]
        [TS.DisableAutomaticReferenceCounting]

        public object InitializeObject( UIntPtr memory,
                                        TS.VTable vTable,
                                        bool referenceCounting)
        {
            ObjectHeader oh = ObjectHeader.CastAsObjectHeader(memory);

            oh.VirtualTable = vTable;

            if (referenceCounting)
            {
                oh.MultiUseWord = (int)((1 << ObjectHeader.ReferenceCountShift) | (int)ObjectHeader.GarbageCollectorFlags.NormalObject | (int)ObjectHeader.GarbageCollectorFlags.Unmarked);
#if REFCOUNT_STAT
                ObjectHeader.s_RefCountedObjectsAllocated++;
#endif
#if DEBUG_REFCOUNT
                BugCheck.Log( "InitRC (0x%x), new count = 1 +", (int)oh.ToPointer( ) );
#endif
            }
            else
            {
                oh.MultiUseWord = (int)(ObjectHeader.GarbageCollectorFlags.NormalObject | ObjectHeader.GarbageCollectorFlags.Unmarked);
            }

            return oh.Pack();
        }

        [Inline]
        [TS.DisableAutomaticReferenceCounting]

        public object InitializeObjectWithExtensions(UIntPtr memory,
                                                      TS.VTable vTable)
        {
            ObjectHeader oh = ObjectHeader.CastAsObjectHeader(memory);

            oh.VirtualTable = vTable;
            oh.MultiUseWord = (int)(ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked);

            return oh.Pack();
        }


        [Inline]
        [TS.DisableAutomaticReferenceCounting]

        public Array InitializeArray( UIntPtr memory,
                                      TS.VTable vTable,
                                      uint length,
                                      bool referenceCounting)
        {
            object obj = InitializeObject(memory, vTable, referenceCounting);

            ArrayImpl array = ArrayImpl.CastAsArray(obj);

            array.m_numElements = length;

            return array.CastThisAsArray();
        }

        [Inline]
        [TS.DisableAutomaticReferenceCounting]

        public String InitializeString(UIntPtr memory,
                                        TS.VTable vTable,
                                        int length,
                                        bool referenceCounting)
        {
            object obj = InitializeObject(memory, vTable, referenceCounting);

            StringImpl str = StringImpl.CastAsString(obj);

            str.m_arrayLength = length;

            return str.CastThisAsString();
        }

        [TS.WellKnownMethod("TypeSystemManager_AllocateObject")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract Object AllocateObject(TS.VTable vTable);

        [TS.WellKnownMethod("TypeSystemManager_AllocateReferenceCountingObject")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract Object AllocateReferenceCountingObject(TS.VTable vTable);

        [TS.WellKnownMethod("TypeSystemManager_AllocateObjectWithExtensions")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract Object AllocateObjectWithExtensions(TS.VTable vTable);

        [TS.WellKnownMethod("TypeSystemManager_AllocateArray")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract Array AllocateArray(TS.VTable vTable, uint length);

        [TS.WellKnownMethod("TypeSystemManager_AllocateReferenceCountingArray")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract Array AllocateReferenceCountingArray(TS.VTable vTable, uint length);

        [TS.WellKnownMethod("TypeSystemManager_AllocateArrayNoClear")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract Array AllocateArrayNoClear(TS.VTable vTable, uint length);

        [TS.WellKnownMethod("TypeSystemManager_AllocateString")]
        [TS.DisableAutomaticReferenceCounting]
        public abstract String AllocateString(TS.VTable vTable, int length);

        [TS.DisableAutomaticReferenceCounting]
        public abstract String AllocateReferenceCountingString(TS.VTable vTable, int length);

        //--//

        [Inline]
        public static T AtomicAllocator<T>(ref T obj) where T : class, new()
        {
            if (obj == null)
            {
                return AtomicAllocatorSlow(ref obj);
            }

            return obj;
        }

        [NoInline]
        private static T AtomicAllocatorSlow<T>(ref T obj) where T : class, new()
        {
            T newObj = new T();

            System.Threading.Interlocked.CompareExchange(ref obj, newObj, default(T));

            return obj;
        }

        //--//

        public static System.Reflection.MethodInfo CodePointerToMethodInfo(TS.CodePointer ptr)
        {
            throw new NotImplementedException();
        }

        //--//

        [NoInline]
        [TS.WellKnownMethod("TypeSystemManager_InvokeStaticConstructors")]
        private void InvokeStaticConstructors()
        {
            //
            // WARNING! 
            // WARNING! Keep this method empty!!!!
            // WARNING! 
            //
            // We need a way to inject calls to the static constructors that are reachable.
            // This is the empty vessel for those calls.
            //
            // WARNING! 
            // WARNING! Keep this method empty!!!!
            // WARNING! 
            //
        }

        [TS.WellKnownMethod("TypeSystemManager_CastToType")]
        public static object CastToType(object obj, TS.VTable expected)
        {
            if (obj != null)
            {
                obj = CastToTypeNoThrow(obj, expected);
                if (obj == null)
                {
                    throw new InvalidCastException();
                }
            }

            return obj;
        }

        [TS.WellKnownMethod("TypeSystemManager_CastToTypeNoThrow")]
        public static object CastToTypeNoThrow(object obj, TS.VTable expected)
        {
            if (obj != null)
            {
                TS.VTable got = TS.VTable.Get(obj);

                if (expected.CanBeAssignedFrom(got) == false)
                {
                    return null;
                }
            }

            return obj;
        }

        //--//

        [TS.WellKnownMethod("TypeSystemManager_CastToSealedType")]
        public static object CastToSealedType(object obj, TS.VTable expected)
        {
            if (obj != null)
            {
                obj = CastToSealedTypeNoThrow(obj, expected);
                if (obj == null)
                {
                    throw new InvalidCastException();
                }
            }

            return obj;
        }

        [TS.WellKnownMethod("TypeSystemManager_CastToSealedTypeNoThrow")]
        public static object CastToSealedTypeNoThrow(object obj, TS.VTable expected)
        {
            if (obj != null)
            {
                TS.VTable got = TS.VTable.Get(obj);

                if (got != expected)
                {
                    return null;
                }
            }

            return obj;
        }

        //--//

        [TS.WellKnownMethod("TypeSystemManager_CastToInterface")]
        public static object CastToInterface(object obj, TS.VTable expected)
        {
            if (obj != null)
            {
                obj = CastToInterfaceNoThrow(obj, expected);
                if (obj == null)
                {
                    throw new InvalidCastException();
                }
            }

            return obj;
        }

        [TS.WellKnownMethod("TypeSystemManager_CastToInterfaceNoThrow")]
        public static object CastToInterfaceNoThrow(object obj, TS.VTable expected)
        {
            if (obj != null)
            {
                TS.VTable got = TS.VTable.Get(obj);

                if (got.ImplementsInterface(expected))
                {
                    return obj;
                }
            }

            return null;
        }

        //--//

        [NoReturn]
        [NoInline]
        [TS.WellKnownMethod("TypeSystemManager_Throw")]
        public virtual void Throw(Exception obj)
        {
            ThreadImpl.CurrentThread.CurrentException = obj;
            UIntPtr exception = Unwind.LLOS_AllocateException(obj, Unwind.ExceptionClass);
            Unwind.LLOS_Unwind_RaiseException(exception);
        }

        [NoReturn]
        [NoInline]
        [TS.WellKnownMethod("TypeSystemManager_Rethrow")]
        public virtual void Rethrow()
        {
            MemoryManager.Instance.DumpMemory( ); 

            Throw(ThreadImpl.CurrentThread.CurrentException);
        }

        //
        // Access Methods
        //

        public static extern TypeSystemManager Instance
        {
            [TS.WellKnownMethod("TypeSystemManager_get_Instance")]
            [SingletonFactory(Fallback = typeof(EmptyManager))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}

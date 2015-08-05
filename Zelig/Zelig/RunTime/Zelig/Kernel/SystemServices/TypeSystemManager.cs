//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ImplicitInstance]
    [ForceDevirtualization]
    [TS.DisableAutomaticReferenceCounting]
    public abstract class TypeSystemManager
    {
        class EmptyManager : TypeSystemManager
        {
            [NoInline]
            public override Object AllocateObject( TS.VTable vTable )
            {
                return null;
            }

            [NoInline]
            public override Object AllocateReferenceCountingObject( TS.VTable vTable )
            {
                return null;
            }

            [NoInline]
            public override Object AllocateObjectWithExtensions( TS.VTable vTable )
            {
                return null;
            }

            [NoInline]
            public override Array AllocateArray( TS.VTable vTable ,
                                                 uint      length )
            {
                return null;
            }

            [NoInline]
            public override Array AllocateReferenceCountingArray( TS.VTable vTable,
                                                                  uint      length )
            {
                return null;
            }

            [NoInline]
            public override Array AllocateArrayNoClear( TS.VTable vTable ,
                                                        uint      length )
            {
                return null;
            }

            [NoInline]
            public override String AllocateString( TS.VTable vTable ,
                                                   int       length )
            {
                return null;
            }

            [NoInline]
            public override String AllocateReferenceCountingString( TS.VTable vTable,
                                                                    int       length )
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
        public object InitializeObject( UIntPtr   memory ,
                                        TS.VTable vTable ,
                                        bool      referenceCounting )
        {
            ObjectHeader oh = ObjectHeader.CastAsObjectHeader( memory );

            oh.VirtualTable = vTable;

            if(referenceCounting)
            {
                oh.MultiUseWord = (int)( ( 1 << ObjectHeader.ReferenceCountShift ) | (int)ObjectHeader.GarbageCollectorFlags.NormalObject | (int)ObjectHeader.GarbageCollectorFlags.Unmarked );
#if REFCOUNT_STAT
                ObjectHeader.s_RefCountedObjectsAllocated++;
#endif
#if DEBUG_REFCOUNT
                BugCheck.Log( "InitRC (0x%x), new count = 1 +", (int)oh.ToPointer( ) );
#endif
            }
            else
            {
                oh.MultiUseWord = (int)( ObjectHeader.GarbageCollectorFlags.NormalObject | ObjectHeader.GarbageCollectorFlags.Unmarked );
            }

            return oh.Pack();
        }

        [Inline]
        public object InitializeObjectWithExtensions( UIntPtr   memory ,
                                                      TS.VTable vTable )
        {
            ObjectHeader oh = ObjectHeader.CastAsObjectHeader( memory );

            oh.VirtualTable = vTable;
            oh.MultiUseWord = (int)(ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked);

            return oh.Pack();
        }


        [Inline]
        public Array InitializeArray( UIntPtr   memory ,
                                      TS.VTable vTable ,
                                      uint      length ,
                                      bool      referenceCounting )
        {
            object obj = InitializeObject( memory, vTable, referenceCounting );

            ArrayImpl array = ArrayImpl.CastAsArray( obj );

            array.m_numElements = length;

            return array.CastThisAsArray();
        }

        [Inline]
        public String InitializeString( UIntPtr   memory ,
                                        TS.VTable vTable ,
                                        int       length ,
                                        bool      referenceCounting )
        {
            object obj = InitializeObject( memory, vTable, referenceCounting );

            StringImpl str = StringImpl.CastAsString( obj );

            str.m_arrayLength = length;

            return str.CastThisAsString();
        }

        [TS.WellKnownMethod( "TypeSystemManager_AllocateObject" )]
        public abstract Object AllocateObject( TS.VTable vTable );

        [TS.WellKnownMethod( "TypeSystemManager_AllocateReferenceCountingObject" )]
        public abstract Object AllocateReferenceCountingObject( TS.VTable vTable );

        [TS.WellKnownMethod( "TypeSystemManager_AllocateObjectWithExtensions" )]
        public abstract Object AllocateObjectWithExtensions( TS.VTable vTable );

        [TS.WellKnownMethod( "TypeSystemManager_AllocateArray" )]
        public abstract Array AllocateArray( TS.VTable vTable ,
                                             uint      length );

        [TS.WellKnownMethod( "TypeSystemManager_AllocateReferenceCountingArray" )]
        public abstract Array AllocateReferenceCountingArray( TS.VTable vTable,
                                                              uint      length );

        [TS.WellKnownMethod( "TypeSystemManager_AllocateArrayNoClear" )]
        public abstract Array AllocateArrayNoClear( TS.VTable vTable ,
                                                    uint      length );

        [TS.WellKnownMethod( "TypeSystemManager_AllocateString" )]
        public abstract String AllocateString( TS.VTable vTable ,
                                               int       length );

        public abstract String AllocateReferenceCountingString( TS.VTable vTable ,
                                                                int       length );

        //--//

        [Inline]
        public static T AtomicAllocator< T >( ref T obj ) where T : class, new()
        {
            if(obj == null)
            {
                return AtomicAllocatorSlow( ref obj );
            }

            return obj;
        }

        [NoInline]
        private static T AtomicAllocatorSlow<T>( ref T obj ) where T : class, new()
        {
            T newObj = new T();

            System.Threading.Interlocked.CompareExchange( ref obj, newObj, default(T) );

            return obj;
        }

        //--//

        public static System.Reflection.MethodInfo CodePointerToMethodInfo( TS.CodePointer ptr )
        {
            throw new NotImplementedException();
        }

        //--//

        [NoInline]
        [TS.WellKnownMethod( "TypeSystemManager_InvokeStaticConstructors" )]
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

        [TS.WellKnownMethod( "TypeSystemManager_CastToType" )]
        public static object CastToType( object    obj      ,
                                         TS.VTable expected )
        {
            if(obj != null)
            {
                obj = CastToTypeNoThrow( obj, expected );
                if(obj == null)
                {
                    throw new InvalidCastException();
                }
            }

            return obj;
        }

        [TS.WellKnownMethod( "TypeSystemManager_CastToTypeNoThrow" )]
        public static object CastToTypeNoThrow( object    obj      ,
                                                TS.VTable expected )
        {
            if(obj != null)
            {
                TS.VTable got = TS.VTable.Get( obj );

                if(expected.CanBeAssignedFrom( got ) == false)
                {
                    return null;
                }
            }

            return obj;
        }

        //--//

        [TS.WellKnownMethod( "TypeSystemManager_CastToSealedType" )]
        public static object CastToSealedType( object    obj      ,
                                               TS.VTable expected )
        {
            if(obj != null)
            {
                obj = CastToSealedTypeNoThrow( obj, expected );
                if(obj == null)
                {
                    throw new InvalidCastException();
                }
            }

            return obj;
        }

        [TS.WellKnownMethod( "TypeSystemManager_CastToSealedTypeNoThrow" )]
        public static object CastToSealedTypeNoThrow( object    obj      ,
                                                      TS.VTable expected )
        {
            if(obj != null)
            {
                TS.VTable got = TS.VTable.Get( obj );

                if(got != expected)
                {
                    return null;
                }
            }

            return obj;
        }

        //--//

        [TS.WellKnownMethod( "TypeSystemManager_CastToInterface" )]
        public static object CastToInterface( object    obj      ,
                                              TS.VTable expected )
        {
            if(obj != null)
            {
                obj = CastToInterfaceNoThrow( obj, expected );
                if(obj == null)
                {
                    throw new InvalidCastException();
                }
            }

            return obj;
        }

        [TS.WellKnownMethod( "TypeSystemManager_CastToInterfaceNoThrow" )]
        public static object CastToInterfaceNoThrow( object    obj      ,
                                                     TS.VTable expected )
        {
            if(obj != null)
            {
                TS.VTable got = TS.VTable.Get( obj );

                if(got.ImplementsInterface( expected ))
                {
                    return obj;
                }
            }

            return null;
        }

        //--//

        [NoReturn]
        [NoInline]
        [TS.WellKnownMethod( "TypeSystemManager_Throw" )]
        public virtual void Throw( Exception obj )
        {
            //
            // TODO: Capture stack dump.
            //

            //
            // Our LLVM port does not yet support throwing exceptions
            //
            
             BugCheck.Log( "!!!                       WARNING                             !!!" );
             BugCheck.Log( "!!! Throwing Exceptions is not yet supported for LLVM CodeGen !!!" );
             BugCheck.Log( "!!!                       WARNING                             !!!" );

             BugCheck.Raise( BugCheck.StopCode.InvalidOperation );

            DeliverException( obj );
        }

        [NoReturn]
        [NoInline]
        [TS.WellKnownMethod( "TypeSystemManager_Rethrow" )]
        public virtual void Rethrow()
        {
            DeliverException( ThreadImpl.GetCurrentException() );
        }

        [NoReturn]
        [NoInline]
        [TS.WellKnownMethod( "TypeSystemManager_Rethrow__Exception" )]
        public virtual void Rethrow( Exception obj )
        {
            DeliverException( obj );
        }

        //--//

        private void DeliverException( Exception obj )
        {
            //
            // TODO: LT72: Only RT.ThreadManager can implement this method correctly
            //
            ThreadImpl        thread = ThreadManager.Instance.CurrentThread;
            Processor.Context ctx    = thread.ThrowContext;

            thread.CurrentException = obj;

            ctx.Populate();

            while(true)
            {
                //
                // The PC points to the instruction AFTER the call, but the ExceptionMap could not cover it.
                //
                UIntPtr    pc = AddressMath.Decrement( ctx.ProgramCounter, sizeof(uint) );
                TS.CodeMap cm = TS.CodeMap.ResolveAddressToCodeMap( pc );

                if(cm != null && cm.ExceptionMap != null)
                {
                    TS.CodePointer cp = cm.ExceptionMap.ResolveAddressToHandler( pc, TS.VTable.Get( obj ) );

                    if(cp.IsValid)
                    {
                        ctx.ProgramCounter = new UIntPtr( (uint)cp.Target.ToInt32() );
                        ctx.SwitchTo();
                    }
                }

                if(ctx.Unwind() == false)
                {
                    break;
                }
            }

            BugCheck.Raise( BugCheck.StopCode.UnwindFailure );
        }

        //
        // Access Methods
        //

        public static extern TypeSystemManager Instance
        {
            [TS.WellKnownMethod( "TypeSystemManager_get_Instance" )]
            [SingletonFactory(Fallback=typeof(EmptyManager))]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}

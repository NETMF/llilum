//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;
    using System.Runtime.InteropServices;

    using RT = Microsoft.Zelig.Runtime;

    public abstract partial class ProcessorARMv7M : RT.Processor
    {
        //--//

        //
        // Part of Context is defined in the model for the targeted sub-system, e.g. Mbed or CMSIS-Core for ARM processors
        //
        
        public new abstract class Context : RT.Processor.Context
        {
            protected Delegate m_dlg;
            protected UIntPtr  m_nativeContext;

            //--//
                        
            //
            // Extensibility 
            //
            protected abstract UIntPtr CreateNativeContext( UIntPtr entryPoint, UIntPtr stack, int stackSize );

            protected abstract void SwitchToContext( UIntPtr thread );
            
            protected abstract void Yield( UIntPtr handle );
        
            protected abstract void Retire( UIntPtr handle );
            
            //
            // Overrides
            //
            public override void Populate()
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.InvalidOperation );
            }

            public override void Populate( RT.Processor.Context context )
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.InvalidOperation );
            }

            public unsafe override void PopulateFromDelegate( Delegate dlg,
                                                              uint[] stack)
            {
                m_dlg = dlg;

                RT.DelegateImpl dlgImpl   = (RT.DelegateImpl)(object)dlg;
                //RT.ArrayImpl    stackImpl = (RT.ArrayImpl   )(object)stack;
                RT.ObjectImpl   objImpl   = (RT.ObjectImpl  )(object)dlg.Target;
    
                UIntPtr PC    = new UIntPtr( dlgImpl.InnerGetCodePointer().Target.ToPointer() );
                //UIntPtr SP    = new UIntPtr( stackImpl.GetEndDataPointer()                    );

                //
                // we do not need to pass the 'this' pointer
                //
                //if(objImpl != null)
                //{
                //    this.Registers.R0 = new UIntPtr( objImpl.Unpack() );
                //}

                //
                // Create the native thread 
                //
                //m_nativeContext = CreateNativeContext( PC, SP, stack.Length ); 
                m_nativeContext = CreateNativeContext( PC, new UIntPtr( 0 ), 0 ); 
            }

            public override void SetupForExceptionHandling( uint mode )
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.InvalidOperation );
                throw new Exception("SetupForExceptionHandling not implemented");
            }

            public override bool Unwind()
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.InvalidOperation );
                throw new Exception("Unwind not implemented");
            }

            public override UIntPtr GetRegisterByIndex( uint idx ) 
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.InvalidOperation );
                throw new Exception("GetRegisterByIndex not implemented");
            }

            public override void SetRegisterByIndex( uint idx,
                                                     UIntPtr value)
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.InvalidOperation );
                throw new Exception("SetRegisterByIndex not implemented");
            }

            //
            // Access Methods
            //

            public override UIntPtr StackPointer
            {
                get { return (UIntPtr)0; }
                set { }
            }

            public override UIntPtr ProgramCounter
            {
                get { return (UIntPtr)0; }
                set { }
            }

            public override uint ScratchedIntegerRegisters
            {
                get { return 0; }
            }
        }
    }
}

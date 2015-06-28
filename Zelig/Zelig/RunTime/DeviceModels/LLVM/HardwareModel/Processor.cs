//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.LLVMHosted
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public class Processor : RT.TargetPlatform.LLVM.ProcessorLLVM
    {
        public new class Context : RT.Processor.Context
        {
            public override void Populate( )
            {

            }

            public override void Populate( RT.Processor.Context context )
            {

            }

            public override void PopulateFromDelegate( Delegate dlg,
                                                       uint[] stack )
            {
                throw new Exception( "PopulateFromDelegate not implemented" );
            }

            public override void SetupForExceptionHandling( uint mode )
            {
                throw new Exception( "SetupForExceptionHandling not implemented" );
            }

            public override bool Unwind( )
            {
                throw new Exception( "Unwind not implemented" );
            }

            public override void SwitchTo( )
            {
                throw new Exception( "SwitchTo not implemented" );
            }

            public override UIntPtr GetRegisterByIndex( uint idx )
            {
                throw new Exception( "GetRegisterByIndex not implemented" );
            }

            public override void SetRegisterByIndex( uint idx,
                                                     UIntPtr value )
            {
                throw new Exception( "SetRegisterByIndex not implemented" );
            }

            //
            // Access Methods
            //

            public override UIntPtr StackPointer
            {
                get { return ( UIntPtr )0; }
                set { }
            }

            public override UIntPtr ProgramCounter
            {
                get { return ( UIntPtr )0; }
                set { }
            }

            public override uint ScratchedIntegerRegisters
            {
                get { return 0; }
            }
        }


     

    }

}
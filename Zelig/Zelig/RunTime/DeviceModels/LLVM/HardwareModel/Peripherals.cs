//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.LLVMHosted
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public sealed unsafe class Peripherals : RT.Peripherals
    {
        //
        // Helper Methods
        //

        public override void Initialize( )
        {
        }

        public override void Activate( )
        {
        }

        public override void CauseInterrupt( )
        {
            throw new Exception( "CauseInterrupt not implemented" );
        }

        public override void ContinueUnderNormalInterrupt( Continuation dlg )
        {
            throw new Exception( "ContinueUnderNormalInterrupt not implemented" );
        }

        public override void WaitForInterrupt( )
        {
            throw new Exception( "WaitForInterrupt not implemented" );
        }

        public override void ProcessInterrupt( )
        {
            throw new Exception( "ProcessInterrupt not implemented" );
        }

        public override void ProcessFastInterrupt( )
        {
            throw new Exception( "ProcessFastInterrupt not implemented" );
        }

        //--//

        public override ulong GetPerformanceCounterFrequency( )
        {
            throw new Exception( "GetPerformanceCounterFrequency not implemented" );
        }

        public override uint ReadPerformanceCounter( )
        {
            throw new Exception( "ReadPerformanceCounter not implemented" );
        }

    }
}

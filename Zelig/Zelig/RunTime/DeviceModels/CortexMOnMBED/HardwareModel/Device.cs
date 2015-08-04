//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexMOnMBED
{
    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.DeviceModels.Chipset.CortexM;
    using RTOS    = Microsoft.Zelig.Support.mbed;

    public sealed class Device : RT.Device
    {

        public override void PreInitializeProcessorAndMemory( )
        {
            RT.BugCheck.Raise( RT.BugCheck.StopCode.FailedBootstrap );
        }

        public override void MoveCodeToProperLocation( )
        {
            RT.BugCheck.Raise( RT.BugCheck.StopCode.FailedBootstrap );
        }
        
        public override void ProcessBugCheck( RT.BugCheck.StopCode code )
        {
            RTOS.Utilities.Breakpoint( (uint)code ); 
        }
    }
}

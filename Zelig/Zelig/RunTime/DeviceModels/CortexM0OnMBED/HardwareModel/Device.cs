//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM0OnMBED
{
    using RT            = Microsoft.Zelig.Runtime;
    using RTOS          = Microsoft.Zelig.Support.mbed;
    using ChipsetModel  = Microsoft.CortexM0OnCMSISCore;


    public abstract class Device : ChipsetModel.Device
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
            m_bugCheckCode = code;

            RT.TargetPlatform.ARMv6.ProcessorARMv6M.Breakpoint( 0x42 ); 
        }

        public override unsafe void ProcessLog(string format)
        {
            fixed (char* pS = format)
            {
                uint length = (uint)format.Length;
                RTOS.Utilities.DebugLog0(pS, length);
            }
        }

        public override unsafe void ProcessLog(string format, int p1)
        {
            fixed (char* pS = format)
            {
                uint length = (uint)format.Length;
                RTOS.Utilities.DebugLog1(pS, length, p1);
            }
        }

        public override unsafe void ProcessLog(string format, int p1, int p2)
        {
            fixed (char* pS = format)
            {
                uint length = (uint)format.Length;
                RTOS.Utilities.DebugLog2(pS, length, p1, p2);
            }
        }

        public override unsafe void ProcessLog(string format, int p1, int p2, int p3)
        {
            fixed (char* pS = format)
            {
                uint length = (uint)format.Length;
                RTOS.Utilities.DebugLog3(pS, length, p1, p2, p3);
            }
        }

        public override unsafe void ProcessLog(string format, int p1, int p2, int p3, int p4)
        {
            fixed (char* pS = format)
            {
                uint length = (uint)format.Length;
                RTOS.Utilities.DebugLog4(pS, length, p1, p2, p3, p4);
            }
        }

        public override unsafe void ProcessLog(string format, int p1, int p2, int p3, int p4, int p5)
        {
            fixed (char* pS = format)
            {
                uint length = (uint)format.Length;
                RTOS.Utilities.DebugLog5(pS, length, p1, p2, p3, p4, p5);
            }
        }
    }
}

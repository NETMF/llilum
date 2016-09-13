//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnCMSISCore
{
    using System;

    using RT   = Microsoft.Zelig.Runtime;
    using PROC = Microsoft.Zelig.Runtime.TargetPlatform.ARMv6;


    public abstract class GarbageCollectionManager : RT.ConservativeMarkAndSweepCollector
    {

        public override uint Collect()
        {
            // Snapshot the registers on the current thread
            PROC.ProcessorARMv6M.RaiseSupervisorCall( PROC.ProcessorARMv6M.SVC_Code.SupervisorCall__SnapshotProcessModeRegisters );

            return base.Collect();
        }

        //--//

        protected override void WalkStackFrames()
        {
            // Mark the registers from the snapshot
            for (uint regNum = 0; regNum < 13; regNum++)
            {
                UIntPtr ptr = PROC.ProcessorARMv6MForLlvm.Snapshot.GetRegisterValue(regNum);

                if (ptr != UIntPtr.Zero)
                {
                    VisitInternalPointer(ptr);
                }
            }

            base.WalkStackFrames();
        }
    }
}

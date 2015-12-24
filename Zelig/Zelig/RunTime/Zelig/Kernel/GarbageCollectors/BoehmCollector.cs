//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define GC_PRECISE_PROFILING

#define DEBUG_PROCESSOR_SNAPSHOT


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS   = Microsoft.Zelig.Runtime.TypeSystem;
    using PROC = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;


    public abstract class BoehmCollector : MarkAndSweepCollector
    {
        //
        // State
        //


        //
        // Helper Methods
        //

        public unsafe override void InitializeGarbageCollectionManager()
        {
            base.InitializeGarbageCollectionManager( ); 
        }

        [Inline]
        public override void NotifyNewObject( UIntPtr ptr  ,
                                              uint    size )
        {
            base.NotifyNewObject( ptr, size );
        }

        public override ObjectImpl FindObject( UIntPtr interiorPtr )
        {
            return base.FindObject( interiorPtr ); 
        }

        public override uint Collect()
        {
            //
            // Jump to handler mode and snapshot the live registers on current frame
            //
            PROC.ProcessorARMv7M.RaiseSupervisorCall( PROC.ProcessorARMv7M.SVC_Code.SupervisorCall__SnapshotProcessModeRegisters );

#if DEBUG_PROCESSOR_SNAPSHOT
                        
            BugCheck.Log( "[Last Active Frame] EXC=0x%08x, PSR=0x%08x, PC=0x%08x",
                    (int)PROC.ProcessorARMv7M.Snapshot.SoftwareFrameRegisters.EXC_RETURN,
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.PSR.ToUInt32( ),
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.PC .ToUInt32( )
                    );

            BugCheck.Log( "[Last Active Frame] R0=0x%08x, R1=0x%08x, R2=0x%08x, R3=0x%08x, R12=0x%08x",
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.R0 .ToUInt32( ),
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.R1 .ToUInt32( ),
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.R2 .ToUInt32( ),
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.R3 .ToUInt32( ),
                    (int)PROC.ProcessorARMv7M.Snapshot.HardwareFrameRegisters.R12.ToUInt32( )
                    );
#endif

            return base.Collect();
        }

        private static int ToMilliseconds( long ticks )
        {
            return (int)(1000.0 * ticks / System.Diagnostics.Stopwatch.Frequency);
        }

        public override long GetTotalMemory()
        {
            return MemoryManager.Instance.AllocatedMemory;
        }

        public override void ThrowOutOfMemory( TS.VTable vTable )
        {
            base.ThrowOutOfMemory( vTable );
        }

        public override bool IsMarked( object obj )
        {
            return base.IsMarked( obj ); 
        }

        public override void ExtendMarking( object obj )
        {
            base.ExtendMarking( obj ); 
        }

        //--//

    }
}

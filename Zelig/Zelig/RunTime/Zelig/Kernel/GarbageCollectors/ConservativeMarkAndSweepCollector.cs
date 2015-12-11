//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using PROC = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

    public abstract class ConservativeMarkAndSweepCollector : MarkAndSweepCollector
    {
        class ConservativeStackWalker : MarkAndSweepStackWalker
        {
            ConservativeMarkAndSweepCollector m_owner;

            internal ConservativeStackWalker( ConservativeMarkAndSweepCollector owner )
            {
                m_owner = owner;
            }

            public unsafe void Process( Processor.Context ctx )
            {
                // Go through each entry in the stack
                UIntPtr* end = (UIntPtr*)ctx.BaseStackPointer.ToUInt32( );
                UIntPtr* current = (UIntPtr*)ctx.StackPointer.ToUInt32( );

                while(current < end)
                {
                    m_owner.VisitInternalPointer( *current );
                    current++;
                }
            }
        }

        //--//

        protected override int MarkStackForObjectsSize
        {
            get { return 256; }
        }
        protected override int MarkStackForArraysSize
        {
            get { return 32; }
        }

        //--//
        protected override MarkAndSweepStackWalker CreateStackWalker( )
        {
            return new ConservativeStackWalker( this );
        }

        public override uint Collect( )
        {
            // Snapshot the registers on the current thread
            PROC.ProcessorARMv7M.RaiseSupervisorCall( PROC.ProcessorARMv7M.SVC_Code.SupervisorCall__SnapshotProcessModeRegisters );

            return base.Collect( );
        }

        //--//

        protected override bool IsThisAGoodPlaceToStopTheWorld( )
        {
            return true;
        }

        protected override void WalkStackFrames( )
        {
            // Mark the registers from the snapshot
            for(uint regNum = 0; regNum < 13; regNum++)
            {
                UIntPtr ptr = PROC.ProcessorARMv7M.Snapshot.GetRegisterValue( regNum );

                if(ptr != UIntPtr.Zero)
                {
                    VisitInternalPointer( ptr );
                }
            }

            base.WalkStackFrames( );
        }
    }
}

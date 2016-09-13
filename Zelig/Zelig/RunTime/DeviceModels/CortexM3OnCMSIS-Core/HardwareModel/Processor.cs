//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM3OnCMSISCore
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.CortexM3;
    using CortexM      =  Microsoft.DeviceModels.Chipset.CortexM;
    using LLOS         = Zelig.LlilumOSAbstraction.HAL;

    public abstract class Processor : ChipsetModel.Processor
    {
        public abstract new class Context : RT.TargetPlatform.ARMv7.ProcessorARMv7MForLlvm.Context
        {
            //
            // Constructor Methods
            //
            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            //
            // Helper Methods
            //

            #region RTOS extensibility

            protected virtual UIntPtr CreateNativeContext( UIntPtr entryPoint, UIntPtr stack, int stackSize )
            {
                return (UIntPtr)0;
            }

            protected virtual void SwitchToContext( UIntPtr thread )
            {
            }

            protected virtual void Yield( UIntPtr handle )
            {
            }

            protected virtual void Retire( UIntPtr handle )
            {
            }

            #endregion

            //
            // Access Methods
            //
        }

        
        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            base.InitializeProcessor();
            
            //
            // Reset the priority grouping that we assume not used
            //
            CortexM.NVIC.SetPriorityGrouping( 0 );
        }

        protected override unsafe void RemapInterrupt(IRQn_Type IRQn, Action isr)
        {
            RT.DelegateImpl dlg = (RT.DelegateImpl)(object)isr;

            UIntPtr isrPtr = new UIntPtr(dlg.InnerGetCodePointer().Target.ToPointer());

            CortexM.NVIC.SetVector((int)IRQn, isrPtr.ToUInt32());
        }
    }

    //--//
    //--//
    //--//

    [RT.ExtendClass( typeof( Microsoft.Zelig.Runtime.Processor ) )]
    internal class ProcessorImpl
    {
        [RT.MergeWithTargetImplementation]
        internal ProcessorImpl()
        {
        }

        [RT.NoInline]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        public static int Delay( int count )
        {
            LLOS.Clock.LLOS_CLOCK_DelayCycles( (uint)count );
            return 0;
        }
    }
}

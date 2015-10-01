//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM4OnCMSISCore
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.CortexM4;


    public abstract class Processor : ChipsetModel.Processor
    {
        public abstract new class Context : RT.TargetPlatform.ARMv7.ProcessorARMv7M_VFP.Context
        {
            //
            // Constructor Methods
            //

            //
            // Helper Methods
            //
            
            #region RTOS extensiblity

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
        // Access methods
        //

    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnCMSISCore
{
    using System;

    using RT            = Microsoft.Zelig.Runtime;
    using TS            = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel  = Microsoft.DeviceModels.Chipset.CortexM3;


    public abstract class Processor : ChipsetModel.Processor
    {
        public abstract new class Context : RT.TargetPlatform.ARMv7.ProcessorARMv7M.Context
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

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexMOnMBED
{
    using System;
    using System.Runtime.InteropServices;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.CortexM;
    using RTOS         = Microsoft.Zelig.Support.mbed;

    public sealed class Processor : RT.TargetPlatform.ARMv7.ProcessorARMv7M
    {
        
        public new class Context : ChipsetModel.Processor.Context
        {
            //--//
                        
            protected override UIntPtr CreateNativeContext( UIntPtr entryPoint, UIntPtr stack, int stackSize )
            {
                return RTOS.Threading.CreateNativeContext( entryPoint, stack, stackSize );
            }

            protected override void SwitchToContext( UIntPtr nativeContext )
            {
                RTOS.Threading.SwitchToContext( nativeContext );
            }
            
            protected override void Yield( UIntPtr nativeContext )
            {
                RTOS.Threading.Yield( nativeContext );
            }
        
            protected override void Retire( UIntPtr nativeContext )
            {
                RTOS.Threading.Retire( nativeContext );
            }

            public override void SwitchTo()
            {
                RTOS.Threading.SwitchToContext( m_nativeContext ); 
            }
        }
        

        //--//

        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext()
        {
            return new Context();
        }        
    }
}

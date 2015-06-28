//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.ArmProcessor.VoxSoloFormFactor
{
    using System;
    using System.Collections.Generic;

    using EncDef       = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using ElementTypes = Microsoft.Zelig.MetaData.ElementTypes;

    //--//

    public static class Interop
    {
        //
        // WARNING: This interop has been disabled because we need to spin the idle thread in order to pump the DCC queues.
        //
        //[Simulator.Interop(Function="void Microsoft.VoxSoloFormFactor.Peripherals::WaitForInterrupt()")]
        class InteropHandler_WaitForInterrupt : Simulator.InteropHandler
        {
            protected override Hosting.Interop.CallbackResponse PerformInterop()
            {
                m_owner.SpinUntilInterrupts();

                return m_owner.Interop_GenericSkipCall();
            }
        }
    }
}

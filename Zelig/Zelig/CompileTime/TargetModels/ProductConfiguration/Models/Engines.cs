//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    [DisplayName("ARM emulator")]
    [HardwareModel(typeof(Emulation.ArmProcessor.Simulator), HardwareModelAttribute.Kind.Engine)]
    public sealed class ArmEmulator : EngineCategory
    {
        public override object Instantiate(InstructionSet iset)
        {
            return new Emulation.ArmProcessor.Simulator(iset);
        }
    }
}

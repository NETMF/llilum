//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    public abstract class EngineCategory : AbstractCategory
    {
        public abstract object Instantiate(InstructionSet iset);
    }
}

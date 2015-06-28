//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [DisplayName( "SED15E0 LCD controller"   )]
    [Defaults   ( "SizeInBytes", 0x00020000U )]
    [Defaults   ( "WordSize"   , 8           )]
    [Defaults   ( "WaitStates" , 2           )]
    [HardwareModel(typeof(Emulation.ArmProcessor.Display.SED15E0), HardwareModelAttribute.Kind.Memory)]
    public sealed class SED15E0 : DisplayCategory
    {
    }
}

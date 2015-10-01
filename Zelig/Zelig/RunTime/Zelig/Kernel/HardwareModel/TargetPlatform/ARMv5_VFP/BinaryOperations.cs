//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv5_VFP.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal static class BinaryOperations
    {
        [TS.WellKnownMethod( "SoftVFP_BinaryOperations_FloatRem" )]
        internal static float FloatRem( float left  ,
                                        float right )
        {
            return left - (int)(left / right) * right;
        }

        //--//

        [TS.WellKnownMethod( "SoftVFP_BinaryOperations_DoubleRem" )]
        internal static double DoubleRem( double left  ,
                                          double right )
        {
            return left - (long)(left / right) * right;
        }
    }
}


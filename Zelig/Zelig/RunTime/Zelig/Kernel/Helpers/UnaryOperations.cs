//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal static class UnaryOperations
    {
        [TS.WellKnownMethod( "SoftFP_UnaryOperations_FloatNeg" )]
        internal static float FloatNeg( float val )
        {
            FloatImplementation fiVal = new FloatImplementation( val );

            fiVal.Negate();

            return fiVal.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_UnaryOperations_FloatFinite" )]
        internal static float FloatFinite( float val )
        {
            FloatImplementation fiVal = new FloatImplementation( val );

            if(fiVal.IsFinite == false)
            {
                throw new NotFiniteNumberException();
            }

            return val;
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_UnaryOperations_DoubleNeg" )]
        internal static double DoubleNeg( double val )
        {
            DoubleImplementation diVal = new DoubleImplementation( val );

            diVal.Negate();

            return diVal.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_UnaryOperations_DoubleFinite" )]
        internal static double DoubleFinite( double val )
        {
            DoubleImplementation diVal = new DoubleImplementation( val );

            if(diVal.IsFinite == false)
            {
                throw new NotFiniteNumberException();
            }

            return val;
        }
    }
}


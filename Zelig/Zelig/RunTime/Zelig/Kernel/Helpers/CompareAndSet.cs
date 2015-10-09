//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal static class CompareAndSet
    {
        [TS.WellKnownMethod( "SoftFP_CompareAndSet_FloatEqual" )]
        internal static bool FloatEqual( float left  ,
                                         float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            return fiLeft.Compare( ref fiRight ) == 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_FloatGreaterOrEqual" )]
        internal static bool FloatGreaterOrEqual( float left  ,
                                                  float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            return fiLeft.Compare( ref fiRight ) >= 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_FloatGreater" )]
        internal static bool FloatGreater( float left  ,
                                           float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            return fiLeft.Compare( ref fiRight ) > 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_FloatLessOrEqual" )]
        internal static bool FloatLessOrEqual( float left  ,
                                               float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            return fiLeft.Compare( ref fiRight ) <= 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_FloatLess" )]
        internal static bool FloatLess( float left  ,
                                        float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            return fiLeft.Compare( ref fiRight ) < 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_FloatNotEqual" )]
        internal static bool FloatNotEqual( float left  ,
                                            float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            return fiLeft.Compare( ref fiRight ) != 0;
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_DoubleEqual" )]
        internal static bool DoubleEqual( double left  ,
                                          double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            return diLeft.Compare( ref diRight ) == 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_DoubleGreaterOrEqual" )]
        internal static bool DoubleGreaterOrEqual( double left  ,
                                                   double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            return diLeft.Compare( ref diRight ) >= 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_DoubleGreater" )]
        internal static bool DoubleGreater( double left  ,
                                            double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            return diLeft.Compare( ref diRight ) > 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_DoubleLessOrEqual" )]
        internal static bool DoubleLessOrEqual( double left  ,
                                                double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            return diLeft.Compare( ref diRight ) <= 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_DoubleLess" )]
        internal static bool DoubleLess( double left  ,
                                         double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            return diLeft.Compare( ref diRight ) < 0;
        }

        [TS.WellKnownMethod( "SoftFP_CompareAndSet_DoubleNotEqual" )]
        internal static bool DoubleNotEqual( double left  ,
                                             double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            return diLeft.Compare( ref diRight ) != 0;
        }
    }
}


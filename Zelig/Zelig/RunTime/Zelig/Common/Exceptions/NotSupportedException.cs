//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    public class FeatureNotSupportedException : Exception
    {
        //
        // Constructor Methods
        //

        private FeatureNotSupportedException( string reason ) : base( reason )
        {
        }

        public static FeatureNotSupportedException Create(        string   format ,
                                                           params object[] args   )
        {
            return new FeatureNotSupportedException( String.Format( format, args ) );
        }
    }
}

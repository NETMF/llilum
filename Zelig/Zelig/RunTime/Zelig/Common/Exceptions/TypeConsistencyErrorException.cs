//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    public class TypeConsistencyErrorException : Exception
    {
        //
        // Constructor Methods
        //

        private TypeConsistencyErrorException( string reason ) : base( reason )
        {
        }

        public static TypeConsistencyErrorException Create(        string   format ,
                                                            params object[] args   )
        {
            return new TypeConsistencyErrorException( String.Format( format, args ) );
        }
    }
}

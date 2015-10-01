//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    public class AssertionViolationException : Exception
    {
        //
        // Constructor Methods
        //

        private AssertionViolationException( string reason ) : base( reason )
        {
        }

        public static AssertionViolationException Create(        string   format ,
                                                          params object[] args   )
        {
            return new AssertionViolationException( String.Format( format, args ) );
        }
    }
}

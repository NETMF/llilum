//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    public class IncorrectEncodingException : Exception
    {
        //
        // Constructor Methods
        //

        private IncorrectEncodingException( string reason ) : base( reason )
        {
        }

        public static IncorrectEncodingException Create(        string   format ,
                                                         params object[] args   )
        {
            return new IncorrectEncodingException( String.Format( format, args ) );
        }
    }
}

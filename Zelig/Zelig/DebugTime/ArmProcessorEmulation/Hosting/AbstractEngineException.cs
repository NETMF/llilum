//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.Emulation.Hosting
{
    public class AbstractEngineException : Exception
    {
        public enum Kind
        {
            Initialization,
            Deployment    ,
            Shutdown      ,
        }

        //
        // State
        //

        public readonly Kind Reason;

        //
        // Constructor Methods
        //

        public AbstractEngineException( Kind   reason  ,
                                        string message ) : base( message )
        {
            this.Reason = reason;
        }

        public AbstractEngineException( Kind      reason         ,
                                        string    message        ,
                                        Exception innerException ) : base( message, innerException )
        {
            this.Reason = reason;
        }

        //
        // Helper Methods
        //

        public static AbstractEngineException Create(        Kind     reason ,
                                                             string   format ,
                                                      params object[] parms  )
        {
            return Create( reason, null, format, parms );
        }

        public static AbstractEngineException Create(        Kind      reason         ,
                                                             Exception innerException ,
                                                             string    format         ,
                                                      params object[]  parms          )
        {
            return new AbstractEngineException( reason, string.Format( format, parms ), innerException );
        }
    }
}

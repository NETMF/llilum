//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;


    [ExtendClass( typeof( System.Diagnostics.DefaultTraceListener ), NoConstructors = true )]
    public class DefaultTraceListenerImpl
    {
        public static void OutputDebugString( String message )
        {
            BugCheck.Log( message );
        }
    }
}

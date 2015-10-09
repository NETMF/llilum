//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Console))]
    public static class ConsoleImpl
    {
        //
        // Helper Methods
        //

        [NoInline]
        public static void WriteLine()
        {
            BugCheck.WriteLine( "" );
        }
    
        [NoInline]
        public static void WriteLine( String value )
        {
            BugCheck.WriteLine( value );
        }
    
    
        [NoInline]
        public static void WriteLine( String format ,
                                      Object arg0   )
        {
            BugCheck.WriteLineFormat( format, arg0 );
        }
    
        [NoInline]
        public static void WriteLine( String format ,
                                      Object arg0   ,
                                      Object arg1   )
        {
            BugCheck.WriteLineFormat( format, arg0, arg1 );
        }
    
        [NoInline]
        public static void WriteLine( String format ,
                                      Object arg0   ,
                                      Object arg1   ,
                                      Object arg2   )
        {
            BugCheck.WriteLineFormat( format, arg0, arg1, arg2 );
        }
    
        [NoInline]
        public static void WriteLine(        String   format ,
                                      params Object[] arg    )
        {
            BugCheck.WriteLineFormat( format, arg );
        }
    }
}

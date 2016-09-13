//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Diagnostics.Debugger), NoConstructors=true)]
    public class DebuggerImpl
    {
        [Inline]
        public static void Break()
        {
            Processor.Instance.Breakpoint();
        }
        
        private static bool IsDebuggerAttached()
        {
            var proc = Processor.Instance;

            if(proc is Microsoft.Zelig.Runtime.TargetPlatform.ARMv7.ProcessorARMv7M)
            {
                return Microsoft.Zelig.Runtime.TargetPlatform.ARMv7.ProcessorARMv7M.IsDebuggerConnected;
            }

            return false;
        }
        
        public static void Log( int level, String category, String message )
        {
            BugCheck.Log( "Level: " + level.ToString() + ", Category: " + category + ", message: " + message ); 
        }
    }
}


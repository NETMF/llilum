//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public static class BugCheck
    {
        public enum StopCode
        {
            InterruptsNotDisabled ,
            InterruptsNotEnabled  ,

            UnwindFailure         ,

            KernelNodeStillLinked ,
            NoCurrentThread       ,
            ExpectingReadyThread  ,

            NoFreeSyncBlock       ,
            SyncBlockCorruption   ,

            NegativeIndex         ,
            IncorrectArgument     ,

            NoMemory              ,
            NoMarkStack           ,
            NotAMemoryReference   ,
            HeapCorruptionDetected,

            FailedBootstrap       ,
            InvalidOperation      ,
        }

        //
        // Helper Methods
        //

        [Inline]
        public static void Assert( bool     condition ,
                                   StopCode code      )
        {
            if(!condition)
            {
                Raise( code );
            }
        }

        [NoReturn]
        [NoInline]
        [TS.WellKnownMethod( "BugCheck_Raise" )]
        public static void Raise( StopCode code )
        {
            Device.Instance.ProcessBugCheck( code );
        }

        //--//

        public static void AssertInterruptsOff()
        {
            Assert( Processor.Instance.AreInterruptsDisabled() == true, BugCheck.StopCode.InterruptsNotDisabled );
        }

        public static void AssertInterruptsOn()
        {
            Assert( Processor.Instance.AreInterruptsDisabled() == false, BugCheck.StopCode.InterruptsNotEnabled );
        }

        //--//

        static readonly System.Text.StringBuilder s_sb = new System.Text.StringBuilder();

        [NoInline]
        [TS.WellKnownMethod( "BugCheck_WriteLine" )]
        public static void WriteLine( string text )
        {
            s_sb.Append( text );
            s_sb.AppendLine();
        }

        public static void WriteLineFormat(        string   fmt   ,
                                            params object[] parms )
        {
            WriteLine( string.Format( fmt, parms ) );
        }
    }
}

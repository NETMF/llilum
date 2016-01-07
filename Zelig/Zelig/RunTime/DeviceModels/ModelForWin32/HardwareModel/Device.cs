//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Win32
{
    using LLOS = Zelig.LlilumOSAbstraction;
    using RT = Microsoft.Zelig.Runtime;

    public class Device : RT.Device
    {
        public override void MoveCodeToProperLocation( )
        {
        }

        public override void PreInitializeProcessorAndMemory( )
        {
        }

        public override void ProcessBugCheck( RT.BugCheck.StopCode code )
        {
            m_bugCheckCode = code;

            LLOS.Debug.LLOS_DEBUG_Break( (uint)code ); 
        }

        public override void ProcessLog(string format)
        {
            LogText( format );
        }

        public override unsafe void ProcessLog(string format, int p1)
        {
            LogText( format, p1 );
        }

        public override unsafe void ProcessLog(string format, int p1, int p2)
        {
            LogText( format, p1, p2 );
        }

        public override unsafe void ProcessLog(string format, int p1, int p2, int p3)
        {
            LogText( format, p1, p2, p3 );
        }

        public override unsafe void ProcessLog(string format, int p1, int p2, int p3, int p4)
        {
            LogText( format, p1, p2, p3, p4 );
        }

        public override unsafe void ProcessLog(string format, int p1, int p2, int p3, int p4, int p5)
        {
            LogText( format, p1, p2, p3, p4, p5 );
        }

        private unsafe void LogText(string format, params object[] args)
        {
            if(!string.IsNullOrEmpty( format ))
            {
                string text = args == null ? format : string.Format(format, args);

                fixed (char* pText = text)
                {
                    LLOS.Debug.LLOS_DEBUG_LogText( pText, text.Length );
                }
            }
        }
    }
}

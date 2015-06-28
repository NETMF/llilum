//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;


    public partial class OutputForm : Emulation.Hosting.Forms.BaseDebuggerForm
    {
        class OutputSinkImpl : Emulation.Hosting.OutputSink
        {
            const int c_RedirectLinesPerFile = 100000;

            //
            // State
            //

            OutputForm           m_owner;

            System.IO.TextWriter m_output;
            string               m_outputFile;
            bool                 m_outputEnabled = false;
            int                  m_outputLines;
            int                  m_outputNum;

            //
            // Constructor Methods
            //

            internal OutputSinkImpl( OutputForm owner )
            {
                m_owner = owner;

                m_owner.Host.RegisterService( typeof(Emulation.Hosting.OutputSink), this );
            }

            //
            // Helper Methods
            //

            public void CloseOutput()
            {
                if(m_output != null)
                {
                    m_output.Close();
                    m_output = null;
                }
            }

            private void OpenOutput()
            {
                if(c_RedirectLinesPerFile > 0)
                {
                    if(m_outputLines >= c_RedirectLinesPerFile)
                    {
                        m_output.Close();
                        m_output = null;
                    }
                }

                if(m_output == null)
                {
                    string file = m_outputFile;

                    if(c_RedirectLinesPerFile > 0)
                    {
                        file = String.Format( "{0}.{1:D8}", file, m_outputNum++ );
                    }

                    Directory.CreateDirectory( Path.GetDirectoryName( file ) );

                    m_output      = new System.IO.StreamWriter( file );
                    m_outputLines = 0;
                }
            }

            //--//

            public override void SetOutput( string file )
            {
                m_outputFile  = Environment.ExpandEnvironmentVariables( file );
                m_outputNum   = 0;
                m_outputLines = 0;

                CloseOutput();
            }

            public override void StartOutput()
            {
                m_outputEnabled = true;
            }

            public override void OutputLine(        string   format ,
                                             params object[] args   )
            {
                if(!m_outputEnabled) return;

                OpenOutput();

                try
                {
                    if(args.Length > 0)
                    {
                        m_output.WriteLine( format, args );
                    }
                    else
                    {
                        m_output.WriteLine( format );
                    }

                    m_output.Flush();

                    m_outputLines++;

                    m_owner.OutputLine( format, args );
                }
                catch
                {
                }
            }

            public override void OutputChar( char c )
            {
                if(!m_outputEnabled) return;

                OpenOutput();

                m_output.Write( "{0}", c );

                m_output.Flush();

                if(c == '\n')
                {
                    m_outputLines++;
                }

                m_owner.OutputChar( c );
            }
        }

        //
        // State
        //

        Queue< string > m_output;
        OutputSinkImpl  m_implOutputSink;

        //
        // Constructor Methods
        //

        public OutputForm( Emulation.Hosting.Forms.HostingSite site ) : base( site )
        {
            InitializeComponent();

            //--//

            m_output = new Queue<string>();

            //--//

            const string outputFile = @"%DEPOTROOT%\ZeligUnitTestResults\ArmEmulator\log.txt";

            textBoxOutputFile.Text = outputFile;

            m_implOutputSink = new OutputSinkImpl( this );

            m_implOutputSink.SetOutput( outputFile );

            //--//

            timer1.Tick += new EventHandler( UpdateTimerCallback );

            //--//

            site.RegisterView( this, Emulation.Hosting.Forms.HostingSite.PublicationMode.View );
        }

        //
        // Helper Methods
        //

        protected override void NotifyChangeInVisibility( bool fVisible )
        {
            if(fVisible)
            {
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        void OutputLine(        string   format ,
                         params object[] args   )
        {
            if(checkBoxFileOnly.Checked)
            {
                return;
            }

            string text = args.Length > 0 ? String.Format( format, args ) : format;

            OutputText( text + "\r\n" );
        }

        void OutputChar( char c )
        {
            OutputText( new string( c, 1 ) );
        }

        void OutputText( string text )
        {
            lock(this)
            {
                m_output.Enqueue( text );
            }
        }

        //
        // Access Methods
        //

        public override string ViewTitle
        {
            get
            {
                return "&Output";
            }
        }

        //
        // Event Methods
        //

        private void UpdateTimerCallback( object    sender ,
                                          EventArgs e      )
        {
            StringBuilder sb = null;

            lock(this)
            {
                if(m_output.Count > 0)
                {
                    sb = new StringBuilder();

                    while(m_output.Count > 0)
                    {
                        sb.Append( m_output.Dequeue() );
                    }
                }
            }

            if(sb != null)
            {
                richTextBoxOutput.AppendText( sb.ToString() );

                if(richTextBoxOutput.Focused == false)
                {
                    richTextBoxOutput.ScrollToCaret();
                }
            }
        }

        private void buttonLoggingClear_Click( object    sender ,
                                               EventArgs e      )
        {
            richTextBoxOutput.Clear();
        }

        private void buttonOutputBrowse_Click( object    sender ,
                                               EventArgs e      )
        {
            saveLoggingOutputDialog1.FileName = Environment.ExpandEnvironmentVariables( textBoxOutputFile.Text );

            if(saveLoggingOutputDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputFile.Text = saveLoggingOutputDialog1.FileName;

                Emulation.Hosting.OutputSink sink;
               
                if(this.Host.GetHostingService( out sink ))
                {
                    sink.SetOutput( textBoxOutputFile.Text );
                }
            }
        }

        private void checkBoxCalls_CheckedChanged( object    sender ,
                                                   EventArgs e      )
        {
            Emulation.Hosting.MonitorExecution svcME;

            if(this.Host.GetHostingService( out svcME ))
            {
                svcME.MonitorCalls = checkBoxCalls.Checked;
            }
        }

        private void checkBoxMemory_CheckedChanged( object    sender ,
                                                    EventArgs e      )
        {
            Emulation.Hosting.MonitorExecution svcME;

            if(this.Host.GetHostingService( out svcME ))
            {
                svcME.MonitorMemory = checkBoxMemory.Checked;
            }
        }

        private void checkBoxRegisters_CheckedChanged( object    sender ,
                                                       EventArgs e      )
        {
            Emulation.Hosting.MonitorExecution svcME;

            if(this.Host.GetHostingService( out svcME ))
            {
                svcME.MonitorRegisters = checkBoxRegisters.Checked;
            }
        }

        private void checkBoxInstructions_CheckedChanged( object    sender ,
                                                          EventArgs e      )
        {
            Emulation.Hosting.MonitorExecution svcME;

            if(this.Host.GetHostingService( out svcME ))
            {
                svcME.MonitorOpcodes = checkBoxInstructions.Checked;
            }
        }

        private void checkBoxInterrupts_CheckedChanged( object    sender ,
                                                        EventArgs e      )
        {
            Emulation.Hosting.MonitorExecution svcME;

            if(this.Host.GetHostingService( out svcME ))
            {
                svcME.MonitorInterrupts = checkBoxInterrupts.Checked;
            }
        }

        private void checkBoxNoSleep_CheckedChanged( object    sender ,
                                                     EventArgs e      )
        {
            Emulation.Hosting.MonitorExecution svcME;

            if(this.Host.GetHostingService( out svcME ))
            {
                svcME.NoSleep = checkBoxNoSleep.Checked;
            }
        }

        private void checkBoxCodeCoverage_CheckedChanged( object    sender ,
                                                          EventArgs e      )
        {
            bool fEnable = checkBoxCodeCoverage.Checked;

            Emulation.Hosting.CodeCoverage svc;

            if(this.Host.GetHostingService( out svc ))
            {
                svc.Enable = fEnable;
            }

            buttonDumpCodeCoverage .Enabled = fEnable;
            buttonResetCodeCoverage.Enabled = fEnable;
        }

        private void buttonResetCodeCoverage_Click( object    sender ,
                                                    EventArgs e      )
        {
            if(this.Host.IsIdle)
            {
                Emulation.Hosting.CodeCoverage svc;
                
                if(this.Host.GetHostingService( out svc ))
                {
                    svc.Reset();
                }
            }
        }

        private void buttonDumpCodeCoverage_Click( object    sender ,
                                                   EventArgs e      )
        {
            if(this.Host.IsIdle)
            {
                Emulation.Hosting.CodeCoverage svc;
                
                if(this.Host.GetHostingService( out svc ))
                {
                    svc.Dump();
                }
            }
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using IR  = Microsoft.Zelig.CodeGeneration.IR;
    using RT  = Microsoft.Zelig.Runtime;
    using TS  = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg = Microsoft.Zelig.Configuration.Environment;
    using Hst = Microsoft.Zelig.Emulation.Hosting;


    public partial class DebuggerMainForm : Form, IMainForm
    {
        class ViewEntry
        {
            //
            // State
            //

            internal readonly Hst.Forms.BaseDebuggerForm            Form;
            internal readonly Hst.Forms.HostingSite.PublicationMode Mode;
            internal readonly ToolStripMenuItem                     MenuItem;

            //
            // Constructor Methods
            //

            internal ViewEntry( Hst.Forms.BaseDebuggerForm            form ,
                                Hst.Forms.HostingSite.PublicationMode mode )

            {
                var item = new ToolStripMenuItem();

                this.Mode     = mode;
                this.Form     = form;
                this.MenuItem = item;

                //--//
         
                var image   = form.ViewImage;
                var title   = form.ViewTitle;

                item.Text         = (title != null) ? title : form.GetType().FullName;
                item.DisplayStyle = (image != null) ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Text;
                item.Image        = image;
                item.Tag          = this;
		        item.ImageAlign   = ContentAlignment.MiddleLeft;
			    item.TextAlign    = ContentAlignment.MiddleRight;

                item.Click += new System.EventHandler( delegate( object sender, EventArgs e )
                {
                    if(this.MenuItem.Checked)
                    {
                        this.Form.Hide();
                    }
                    else
                    {
                        this.Form.Show();
                    }
                } );
            }

            //
            // Helper Methods
            //

            internal static ViewEntry Find( List< ViewEntry >                     lst  ,
                                            Hst.Forms.BaseDebuggerForm            form ,
                                            Hst.Forms.HostingSite.PublicationMode mode )
            {
                foreach(var item in lst)
                {
                    if(item.Form == form && item.Mode == mode)
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        class HostingSiteImpl : Hst.Forms.HostingSite
        {
            //
            // State
            //

            DebuggerMainForm m_owner;

            //
            // Constructor Methods
            //

            internal HostingSiteImpl( DebuggerMainForm owner )
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            public override void ProcessKeyDownEvent( KeyEventArgs e )
            {
                m_owner.ProcessKeyDownEvent( e );
            }

            public override void ProcessKeyUpEvent( KeyEventArgs e )
            {
                m_owner.ProcessKeyUpEvent( e );
            }

            public override void ReportFormStatus( Hst.Forms.BaseDebuggerForm form    ,
                                                   bool                       fOpened )
            {
                m_owner.ReportFormStatus( form, fOpened );
            }

            public override void RegisterView( Hst.Forms.BaseDebuggerForm            form ,
                                               Hst.Forms.HostingSite.PublicationMode mode )
            {
                m_owner.RegisterView( form, mode );
            }

            public override void UnregisterView( Hst.Forms.BaseDebuggerForm            form ,
                                                 Hst.Forms.HostingSite.PublicationMode mode )
            {
                m_owner.UnregisterView( form, mode );
            }

            public override void VisualizeDebugInfo( Debugging.DebugInfo di )
            {
                m_owner.VisualizeDebugInfo( di );
            }

            public override object GetHostingService( Type t )
            {
                return m_owner.Host.GetHostingService( t );
            }

            public override void RegisterService( Type   type ,
                                                  object impl )
            {
                m_owner.Host.RegisterService( type, impl );
            }


            public override void UnregisterService( object impl )
            {
                m_owner.Host.UnregisterService( impl );
            }

            //
            // Access Methods
            // 

            public override bool IsIdle
            {
                get
                {
                    return m_owner.IsIdle;
                }
            }
        }

        class ApplicationArguments
        {
            //
            // State
            //

            internal string[]                           m_arguments;

            internal string                             m_sessionName;
            internal string                             m_sessionFile;
            internal string                             m_imageFile;
            internal List< string                     > m_breakpoints = new List< string                     >();
            internal List< System.Reflection.Assembly > m_asssemblies = new List< System.Reflection.Assembly >();
            internal List< Type                       > m_handlers    = new List< Type                       >();

            //
            // Constructor Methods
            //

            internal ApplicationArguments( string[] args )
            {
                m_arguments = args;
            }

            //
            // Helper Methods
            //

            internal bool Parse()
            {
                return Parse( m_arguments );
            }

            private bool Parse( string line )
            {
                List< string > args = new List< string >();

                for(int pos = 0; pos < line.Length; )
                {
                    char c = line[pos++];

                    switch(c)
                    {
                        case ' ':
                        case '\t':
                            break;

                        case '\'':
                        case '"':
                            {
                                StringBuilder sb   = new StringBuilder();
                                int           pos2 = pos;
                                bool          fAdd = false;

                                while(pos2 < line.Length)
                                {
                                    char c2 = line[pos2++];

                                    if(fAdd == false)
                                    {
                                        if(c2 == c)
                                        {
                                            break;
                                        }

                                        if(c2 == '\\')
                                        {
                                            fAdd = true;
                                            continue;
                                        }
                                    }

                                    sb.Append( c2 );
                                    fAdd = false;
                                }

                                pos = pos2;

                                args.Add( sb.ToString() );
                            }
                            break;

                        default:
                            {
                                StringBuilder sb   = new StringBuilder();
                                int           pos2 = pos;

                                sb.Append( c );

                                while(pos2 < line.Length)
                                {
                                    char c2 = line[pos2++];

                                    if(c2 == ' '  ||
                                       c2 == '\t'  )
                                    {
                                        break;
                                    }

                                    sb.Append( c2 );
                                }

                                pos = pos2;

                                args.Add( sb.ToString() );
                            }
                            break;
                    }
                }

                if(args.Count == 0)
                {
                    return true;
                }

                return Parse( args.ToArray() );
            }

            private bool Parse( string[] args )
            {
                if(args != null)
                {
                    for(int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i];

                        if(arg == string.Empty)
                        {
                            continue;
                        }

                        if(arg.StartsWith( "/" ) ||
                           arg.StartsWith( "-" )  )
                        {
                            string option = arg.Substring( 1 );

                            if(IsMatch( option, "Cfg" ))
                            {
                                string file;

                                if(!GetArgument( arg, args, true, ref i, out file ))
                                {
                                    return false;
                                }

                                using(System.IO.StreamReader stream = new System.IO.StreamReader( file ))
                                {
                                    string line;

                                    while((line = stream.ReadLine()) != null)
                                    {
                                        if(line.StartsWith( "#" ))
                                        {
                                            continue;
                                        }

                                        if(Parse( line ) == false)
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                            else if(IsMatch( option, "Session" ))
                            {
                                if(!GetArgument( arg, args, false, ref i, out m_sessionName ))
                                {
                                    return false;
                                }
                            }
                            else if(IsMatch( option, "SessionFile" ))
                            {
                                if(!GetArgument( arg, args, true, ref i, out m_sessionFile ))
                                {
                                    return false;
                                }
                            }
                            else if(IsMatch( option, "ImageFile" ))
                            {
                                if(!GetArgument( arg, args, true, ref i, out m_imageFile ))
                                {
                                    return false;
                                }
                            }
                            else if(IsMatch( option, "Breakpoint" ))
                            {
                                string str;

                                if(!GetArgument( arg, args, false, ref i, out str ))
                                {
                                    return false;
                                }

                                m_breakpoints.Add( str );
                            }
                            else if(IsMatch( option, "LoadAssembly" ))
                            {
                                string file;

                                if(!GetArgument( arg, args, true, ref i, out file ))
                                {
                                    return false;
                                }

                                try
                                {
                                    m_asssemblies.Add( System.Reflection.Assembly.LoadFrom( file ) );

                                    //
                                    // The plug-ins will come from a different directory structure.
                                    // But the loader won't resolve assembly names to assemblies outside the application directory structure,
                                    // even if the assemblies are already loaded in the current AppDomain.
                                    //
                                    // Let's register with the AppDomain, so we can check if an assembly has already been loaded, and
                                    // just override the loader policy.
                                    //
                                    if(m_asssemblies.Count == 1)
                                    {
                                        AppDomain.CurrentDomain.AssemblyResolve += delegate( object sender, ResolveEventArgs args2 )
                                        {
                                            foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                                            {
                                                if(asm.FullName == args2.Name)
                                                {
                                                    return asm;
                                                }
                                            }

                                            return null;
                                        };
                                    }
                                }
                                catch(Exception ex)
                                {
                                    MessageBox.Show( string.Format( "Exception caught while loading assembly from file {0}:\r\n{1}", file, ex ), "Type Load Error", MessageBoxButtons.OK );
                                    return false;
                                }
                            }
                            else if(IsMatch( option, "AddHandler" ))
                            {
                                string cls;

                                if(!GetArgument( arg, args, false, ref i, out cls ))
                                {
                                    return false;
                                }

                                Type t = Type.GetType( cls );
                                if(t == null)
                                {
                                    foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                                    {
                                        t = asm.GetType( cls );
                                        if(t != null)
                                        {
                                            break;
                                        }
                                    }
                                }

                                if(t == null)
                                {
                                    MessageBox.Show( string.Format( "Cannot find type for handler '{0}'", cls ), "Type Load Error", MessageBoxButtons.OK );
                                    return false;
                                }

                                m_handlers.Add( t );
                            }
////                        else if(IsMatch( option, "DumpIR" ))
////                        {
////                            m_fDumpIR = true;
////                        }
////                        else if(IsMatch( option, "DumpASM" ))
////                        {
////                            m_fDumpASM = true;
////                        }
////                        else if(IsMatch( option, "DumpHEX" ))
////                        {
////                            m_fDumpHEX = true;
////                        }
                            else
                            {
                                MessageBox.Show( string.Format( "Unrecognized option: {0}", option ) );
                                return false;
                            }
                        }
////                    else
////                    {
////                        arg = Expand( arg );
////                        if(File.Exists( arg ) == false)
////                        {
////                            Console.WriteLine( "Cannot find '{0}'", arg );
////                            return false;
////                        }
////    
////                        if(m_targetFile != null)
////                        {
////                            Console.WriteLine( "ERROR: Only one target file per compilation." );
////                        }
////    
////                        m_targetFile = arg;
////    
////                        m_searchOrder.Insert( 0, System.IO.Path.GetDirectoryName( arg ) );
////                    }
                    }

                    return true;
                }

                return false;
            }

            private static bool IsMatch( string arg ,
                                         string cmd )
            {
                return arg.ToUpper().CompareTo( cmd.ToUpper() ) == 0;
            }

            private static bool GetArgument(     string   arg           ,
                                                 string[] args          ,
                                                 bool     fExpandEnvVar ,
                                             ref int      i             ,
                                             out string   value         )
            {
                if(i + 1 < args.Length)
                {
                    i++;

                    value = args[i];

                    if(fExpandEnvVar)
                    {
                        value = Expand( value );
                    }

                    return true;
                }

                MessageBox.Show( string.Format( "Option '{0}' needs an argument", arg ) );

                value = null;
                return false;
            }

            private static string Expand( string str )
            {
                return Environment.ExpandEnvironmentVariables( str );
            }
        }

        //--//

        const int c_visualEffect_Depth_CurrentStatement = 1;
        const int c_visualEffect_Depth_OnTheStackTrace  = 2;
        const int c_visualEffect_Depth_Breakpoint       = 3;
        const int c_visualEffect_Depth_Disassembly      = 4;
        const int c_visualEffect_Depth_Profile          = 5;

        //
        // State
        //

        ApplicationArguments                 m_arguments;
        bool                                 m_fFullyInitialized;
                               
        ProfilerMainForm                     m_profilerForm;
        DisplayForm                          m_displayForm;
        OutputForm                           m_outputForm;
        EnvironmentForm                      m_environmentForm;
        SessionManagerForm                   m_sessionManagerForm;
        List< Hst.AbstractUIPlugIn >         m_plugIns = new List< Hst.AbstractUIPlugIn >();
                               
        Hst.Forms.HostingSite.ExecutionState m_currentState;
                                     
        HostingSiteImpl                      m_hostingSiteImpl;
        ProcessorHost                        m_processorHost;
        int                                  m_versionBreakpoints;
        ulong                                m_baseSample_clockTicks;
        ulong                                m_baseSample_nanoseconds;
        ulong                                m_currentSample_clockTicks;
        ulong                                m_currentSample_nanoseconds;
        ulong                                m_lastSample_clockTicks;
        ulong                                m_lastSample_nanoseconds;
                                     
        ImageInformation                     m_imageInformation;
                               
        int                                  m_versionStackTrace;
        List< ThreadStatus >                 m_threads;
        ThreadStatus                         m_activeThread;
        ThreadStatus                         m_selectedThread;
                               
        Thread                               m_processorWorker;
        AutoResetEvent                       m_processorWorkerSignal;
        Queue< WorkerWorkItem >              m_processorWorkerRequests;
        bool                                 m_processorWorkerExit;
                               
        DebugGarbageColllection              m_debugGC;

        //--//                 
                               
        List< VisualTreeInfo >               m_visualTreesList;
        List< ViewEntry      >               m_viewEntriesList;
                               
        //--//                 
                               
        Session                              m_currentSession;
        Cfg.Manager                          m_configurationManager;
                                     
        Brush                                m_brush_CurrentPC  = new SolidBrush( Color.FromArgb( 255, 238,  98 ) );
        Brush                                m_brush_PastPC     = new SolidBrush( Color.FromArgb( 180, 228, 180 ) );
        Brush                                m_brush_Breakpoint = new SolidBrush( Color.FromArgb( 150,  58,  70 ) );
        Pen                                  m_pen_Breakpoint   = new Pen       ( Color.FromArgb( 150,  58,  70 ) );
                               
        Icon                                 m_icon_Breakpoint         = Properties.Resources.Breakpoint;
        Icon                                 m_icon_BreakpointDisabled = Properties.Resources.BreakpointDisabled;
        Icon                                 m_icon_CurrentStatement   = Properties.Resources.CurrentStatement;
        Icon                                 m_icon_StackFrame         = Properties.Resources.StackFrame;

        //--//

        public bool DebugGC        = false;
        public bool DebugGCVerbose = false;

        //--//

        //
        // Constructor Methods
        //

        public DebuggerMainForm( string[] args )
        {
            InitializeComponent();

            //--//

            m_arguments = new ApplicationArguments( args );

            m_configurationManager = new Cfg.Manager();

            m_configurationManager.AddAllAssemblies( null );
            m_configurationManager.ComputeAllPossibleValuesForFields();

            //--//

            m_currentState            = Hst.Forms.HostingSite.ExecutionState.Invalid;
            m_threads                 = new List< ThreadStatus >();

            m_processorWorker         = new Thread( ProcessorWorker );
            m_processorWorkerSignal   = new AutoResetEvent( false );
            m_processorWorkerRequests = new Queue< WorkerWorkItem >();

            m_visualTreesList         = new List< VisualTreeInfo >();
            m_viewEntriesList         = new List< ViewEntry      >();

            //--//

            m_hostingSiteImpl         = new HostingSiteImpl( this );
            m_processorHost           = new ProcessorHost  ( this );
            
            m_profilerForm            = new ProfilerMainForm  ( m_hostingSiteImpl );
            m_displayForm             = new DisplayForm       ( m_hostingSiteImpl );
            m_outputForm              = new OutputForm        ( m_hostingSiteImpl );
            m_environmentForm         = new EnvironmentForm   ( this );
            m_sessionManagerForm      = new SessionManagerForm( this );

            //--//

            codeView1.DefaultHitSink = codeView1_HitSink;

            localsView1     .Link( this );
            stackTraceView1 .Link( this );
            threadsView1    .Link( this );
            registersView1  .Link( this );
            breakpointsView1.Link( this );
            memoryView1     .Link( this );
        }

        //
        // Helper Methods
        //

        private void FullyInitialized()
        {
            if(m_arguments.Parse() == false)
            {
                Application.Exit();
            }

            string name = m_arguments.m_sessionName;
            if(name != null)
            {
                m_currentSession = m_sessionManagerForm.FindSession( name );
                if(m_currentSession == null)
                {
                    MessageBox.Show( string.Format( "Cannot find session '{0}'", name ) );
                    Application.Exit();
                }
            }

            string file = m_arguments.m_sessionFile;
            if(file != null)
            {
                m_currentSession = m_sessionManagerForm.LoadSession( file, true );
                if(m_currentSession == null)
                {
                    MessageBox.Show( string.Format( "Cannot load session file '{0}'", file ) );
                    Application.Exit();
                }
            }

            if(m_currentSession != null)
            {
                m_sessionManagerForm.SelectSession( m_currentSession );

                if(m_arguments.m_imageFile != null)
                {
                    m_currentSession.ImageToLoad = m_arguments.m_imageFile;
                }
            }

            foreach(Type t in m_arguments.m_handlers)
            {
                if(t.IsSubclassOf( typeof(Hst.AbstractUIPlugIn) ))
                {
                    Hst.AbstractUIPlugIn plugIn = (Hst.AbstractUIPlugIn)Activator.CreateInstance( t, m_hostingSiteImpl );

                    m_plugIns.Add( plugIn );

                    plugIn.Start();
                }
            }

            if(m_currentSession == null)
            {
                m_currentSession = m_sessionManagerForm.SelectSession( true );
            }

            if(m_arguments.m_breakpoints.Count > 0)
            {
                m_hostingSiteImpl.NotifyOnExecutionStateChange += delegate( Hst.Forms.HostingSite host, Hst.Forms.HostingSite.ExecutionState oldState, Hst.Forms.HostingSite.ExecutionState newState )
                {
                    if(newState != Hst.Forms.HostingSite.ExecutionState.Loaded)
                    {
                        return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
                    }

                    tabControl_Data.SelectedTab = tabPage_Breakpoints;

                    foreach(string breakpoint in m_arguments.m_breakpoints)
                    {
                        breakpointsView1.Set( breakpoint );
                    }

                    return Hst.Forms.HostingSite.NotificationResponse.RemoveFromNotificationList;
                };
            }

            m_hostingSiteImpl.NotifyOnExecutionStateChange += delegate( Hst.Forms.HostingSite host, Hst.Forms.HostingSite.ExecutionState oldState, Hst.Forms.HostingSite.ExecutionState newState )
            {
                if(newState == Hst.Forms.HostingSite.ExecutionState.Loaded)
                {
                    Action_ResetAbsoluteTime();
                }

                if(newState == Hst.Forms.HostingSite.ExecutionState.Paused)
                {
                    if(m_processorHost.GetAbsoluteTime( out m_currentSample_clockTicks, out m_currentSample_nanoseconds ))
                    {
                        DisplayExecutionTime();
                    }
                    else
                    {
                        toolStripStatus_AbsoluteTime.Text = "";
                    }
                }
                else
                {
                    m_lastSample_clockTicks  = m_currentSample_clockTicks;
                    m_lastSample_nanoseconds = m_currentSample_nanoseconds;

                    toolStripStatus_AbsoluteTime.Text = "";
                }

                return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
            };

            m_hostingSiteImpl.NotifyOnVisualizationEvent += delegate( Hst.Forms.HostingSite host, Hst.Forms.HostingSite.VisualizationEvent e )
            {
                switch(e)
                {
                    case Hst.Forms.HostingSite.VisualizationEvent.NewStackFrame:
                        UpdateCurrentMethod();
                        break;
                }

                return Hst.Forms.HostingSite.NotificationResponse.DoNothing;
            };

            InnerAction_LoadImage();
        }

        private void UpdateCurrentMethod()
        {
            string txt = "<no current method>";

            while(true)
            {
                var th         = m_selectedThread           ; if(th         == null) break;
                var stackFrame = m_selectedThread.StackFrame; if(stackFrame == null) break;
                var cm         = stackFrame.CodeMapOfTarget ; if(cm         == null) break;
                var md         = cm.Target                  ; if(md         == null) break;

                txt = string.Format( "{0}", md.ToShortString() );
                break;
            }

            toolStripStatus_CurrentMethod.Text = txt;
        }

        //--//

        public static void SetPanelHeight( Panel panel  ,
                                           int   height )
        {
            Size size = panel.Size;

            if(size.Height != height)
            {
                size.Height = height;

                panel.Size = size;
            }
        }

        internal void ReportFormStatus( Form form    ,
                                        bool fOpened )
        {
            foreach(var item in m_viewEntriesList)
            {
                if(item.Form == form)
                {
                    item.MenuItem.Checked = fOpened;
                }
            }
        }

        private void RegisterView( Hst.Forms.BaseDebuggerForm            form ,
                                   Hst.Forms.HostingSite.PublicationMode mode )
        {
            if(ViewEntry.Find( m_viewEntriesList, form, mode ) == null)
            {
                var item = new ViewEntry( form, mode );

                m_viewEntriesList.Add( item );

                switch(mode)
                {
                    case Hst.Forms.HostingSite.PublicationMode.Window:
                        {
                            var items = this.toolStripMenuItem_Windows.DropDownItems;
                            var index = items.IndexOf( this.toolStripSeparator_Files );

                            items.Insert( index, item.MenuItem );
                        }
                        break;

                    case Hst.Forms.HostingSite.PublicationMode.View:
                        {
                            var items = this.toolStripMenuItem_View.DropDownItems;

                            items.Add( item.MenuItem );
                        }
                        break;

                    case Hst.Forms.HostingSite.PublicationMode.Tools:
                        {
                            var items       = this.toolStripMenuItem_Tools.DropDownItems;
                            var indexTop    = items.IndexOf( this.toolStripSeparator_Tools_Top    );
                            var indexBottom = items.IndexOf( this.toolStripSeparator_Tools_Bottom );

                            items.Insert( indexBottom, item.MenuItem );

                            this.toolStripSeparator_Tools_Bottom.Visible = true;
                        }
                        break;
                }
            }
        }

        private void UnregisterView( Hst.Forms.BaseDebuggerForm            form ,
                                     Hst.Forms.HostingSite.PublicationMode mode )
        {
            var item = ViewEntry.Find( m_viewEntriesList, form, mode );

            if(item != null)
            {
                m_viewEntriesList.Remove( item );

                var menu      = item.MenuItem;
                var menuOwner = (ToolStripMenuItem)menu.OwnerItem;

                menuOwner.DropDownItems.Remove( menu );

                switch(mode)
                {
                    case Hst.Forms.HostingSite.PublicationMode.Window:
                        {
                            var items = this.toolStripMenuItem_Windows.DropDownItems;
                            var index = items.IndexOf( this.toolStripSeparator_Files );

                            if(index == 0)
                            {
                                this.toolStripSeparator_Files.Visible = false;
                            }
                        }
                        break;

                    case Hst.Forms.HostingSite.PublicationMode.View:
                        {
                        }
                        break;

                    case Hst.Forms.HostingSite.PublicationMode.Tools:
                        {
                            var items       = this.toolStripMenuItem_Tools.DropDownItems;
                            var indexTop    = items.IndexOf( this.toolStripSeparator_Tools_Top    );
                            var indexBottom = items.IndexOf( this.toolStripSeparator_Tools_Bottom );

                            if(indexTop + 1 == indexBottom)
                            {
                                this.toolStripSeparator_Tools_Bottom.Visible = false;
                            }
                        }
                        break;
                }
            }
        }

        //--//

        private void SetWaitCursor( Hst.Forms.HostingSite.ExecutionState state )
        {
            switch(state)
            {
                case Hst.Forms.HostingSite.ExecutionState.Idle   :
                case Hst.Forms.HostingSite.ExecutionState.Loaded :
                case Hst.Forms.HostingSite.ExecutionState.Running:
                case Hst.Forms.HostingSite.ExecutionState.Paused :
                    this.UseWaitCursor = false;
                    break;

                default:
                    this.UseWaitCursor = false;
                    break;
            }
        }

        private void UpdateExecutionState( Hst.Forms.HostingSite.ExecutionState state )
        {
            toolStripButton_Start            .Enabled = state == Hst.Forms.HostingSite.ExecutionState.Loaded || state == Hst.Forms.HostingSite.ExecutionState.Paused                                                         ;
            toolStripButton_BreakAll         .Enabled =                                                                                                                 state == Hst.Forms.HostingSite.ExecutionState.Running;
            toolStripButton_StopDebugging    .Enabled =                                                         state == Hst.Forms.HostingSite.ExecutionState.Paused || state == Hst.Forms.HostingSite.ExecutionState.Running;
            toolStripButton_ShowNextStatement.Enabled =                                                         state == Hst.Forms.HostingSite.ExecutionState.Paused                                                         ;
            toolStripButton_Restart          .Enabled =                                                         state == Hst.Forms.HostingSite.ExecutionState.Paused                                                         ;
            toolStripButton_StepInto         .Enabled = state == Hst.Forms.HostingSite.ExecutionState.Loaded || state == Hst.Forms.HostingSite.ExecutionState.Paused                                                         ;
            toolStripButton_StepOver         .Enabled =                                                         state == Hst.Forms.HostingSite.ExecutionState.Paused                                                         ;
            toolStripButton_StepOut          .Enabled = m_activeThread != null && m_activeThread.StackTrace.Count > 1;

            toolStripStatus_ExecutionState.Text = state.ToString();

            SetWaitCursor( state );

            var oldState = m_currentState;

            m_currentState = state;

            m_hostingSiteImpl.RaiseNotification( oldState, state );
        }

        private void DisplayExecutionTime()
        {
            ulong diff_clockTicks  = m_currentSample_clockTicks  - m_lastSample_clockTicks;
            ulong diff_nanoseconds = m_currentSample_nanoseconds - m_lastSample_nanoseconds;
            ulong base_clockTicks  = m_currentSample_clockTicks  - m_baseSample_clockTicks;
            ulong base_nanoseconds = m_currentSample_nanoseconds - m_baseSample_nanoseconds;

            //
            // \u0393 = Greek Delta
            // \u00b5 = Greek micro
            //
            toolStripStatus_AbsoluteTime.Text = string.Format( "Clks={0} (\u0394={2})  \u00B5Sec={1} (\u0394={3})",
                base_clockTicks, (double)base_nanoseconds / 1000.0,
                diff_clockTicks, (double)diff_nanoseconds / 1000.0 );
        }

        private void EnteringState_Running()
        {
            m_threads.Clear();
            m_activeThread   = null;
            m_selectedThread = null;

            ResetVisualEffects();

            SwitchToFile( null, null );
        }

        private void ExitingState_Running( Debugging.DebugInfo diTarget )
        {
            if(m_imageInformation != null)
            {
                m_activeThread   = ThreadStatus.Analyze( m_threads, m_processorHost.MemoryDelta, m_processorHost.Breakpoints );
                m_selectedThread = m_activeThread;

                if(diTarget != null)
                {
                    m_activeThread.TopStackFrame.MoveForwardToDebugInfo( diTarget );
                }

                UpdateDisplay( true );
            }
        }

        private void ProcessCodeCoverage()
        {
            Emulation.Hosting.CodeCoverage svc;
            
            if(m_processorHost.GetHostingService( out svc ))
            {
                if(svc.Enable)
                {
                    m_imageInformation.CollectCodeCoverage( m_processorHost );

                    foreach(VisualTreeInfo vti in m_visualTreesList)
                    {
                        List             < ImageInformation.RangeToSourceCode > lst;
                        GrowOnlyHashTable< Debugging.DebugInfo, ulong         > ht = HashTableFactory.New< Debugging.DebugInfo, ulong >();

                        if(m_imageInformation.FileToCodeLookup.TryGetValue( vti.Input.File.ToUpper(), out lst ))
                        {
                            foreach(ImageInformation.RangeToSourceCode rng in lst)
                            {
                                ImageInformation.RangeToSourceCode.CodeCoverage profile = rng.Profile;

                                if(profile != null)
                                {
                                    foreach(var pair in rng.DebugInfos)
                                    {
                                        ulong count;

                                        ht.TryGetValue( pair.Info, out count );

                                        count += profile.Cycles;

                                        ht[pair.Info] = count;
                                    }
                                }
                            }
                        }

                        ulong max = 0;

                        foreach(Debugging.DebugInfo di in ht.Keys)
                        {
                            max = Math.Max( ht[di], max );
                        }

                        if(max != 0)
                        {
                            double scale = 1.0 / max;

                            foreach(Debugging.DebugInfo di in ht.Keys)
                            {
                                double ratio = ht[di] * scale;

                                Brush brush = new SolidBrush( Color.FromArgb( (int)(255 * ratio), 255, 0, 0 ) );

                                vti.HighlightSourceCode( codeView1, uint.MaxValue, di, null, null, brush, c_visualEffect_Depth_Profile );
                            }
                        }
                    }
                }
            }
        }

        //--//

        public bool LocateVisualTreeForDebugInfo(     Debugging.DebugInfo di            ,
                                                  out VisualTreeInfo      vti           ,
                                                  out VisualItem          bringIntoView )
        {
            vti           = null;
            bringIntoView = null;

            if(di != null)
            {
                string file = di.SrcFileName;

                vti = codeView1.CreateVisualTree( m_imageInformation, file, file );
                if(vti != null)
                {
                    for(int lineNum = di.BeginLineNumber; lineNum <= di.EndLineNumber; lineNum++)
                    {
                        bringIntoView = vti.FindLine( lineNum );
                        if(bringIntoView != null)
                        {
                            break;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public void VisualizeFile( string file )
        {
            VisualTreeInfo vti = codeView1.CreateVisualTree( m_imageInformation, file, file );
            if(vti != null)
            {
                SwitchToFile( vti, null );
            }
        }
        
        public void VisualizeDebugInfo( Debugging.DebugInfo di )
        {
            VisualTreeInfo vti;
            VisualItem     bringIntoView;

            if(LocateVisualTreeForDebugInfo( di, out vti, out bringIntoView ))
            {
                SwitchToFile( vti, bringIntoView );

                this.BringToFront();
            }
        }

        public void VisualizeStackFrame( StackFrame sf )
        {
            VisualTreeInfo vti;
            VisualItem     bringIntoView;

            m_selectedThread            = sf.Thread;
            m_selectedThread.StackFrame = sf;

            LocateVisualTreeForDebugInfo( sf.DebugInfo, out vti, out bringIntoView );

            PrepareDisplay( vti, sf, bringIntoView );

            NotifyNewStackFrame();
        }

        private void SwitchToFile( VisualTreeInfo vti           ,
                                   VisualItem     bringIntoView )
        {
            if(vti != null)
            {
                m_visualTreesList.Remove(    vti );
                m_visualTreesList.Insert( 0, vti );

                UpdateCheckedStatusForFiles( vti );

                codeView1.InstallVisualTree( vti, bringIntoView );
            }
            else
            {
                codeView1.RefreshVisualTree();
            }
        }

        private void UpdateCheckedStatusForFiles( VisualTreeInfo vti )
        {
            ToolStripItemCollection coll = toolStripMenuItem_Windows.DropDownItems;

            //
            // Remove previous files.
            //
            int index = coll.IndexOf( this.toolStripSeparator_Files );

            while(index + 1 < coll.Count)
            {
                coll.RemoveAt( index + 1 );
            }

            //
            // Have separator only if there are open files.
            //
            this.toolStripSeparator_Files.Visible = index > 0 && (m_visualTreesList.Count != 0);

            for(int pos = 0; pos < m_visualTreesList.Count; pos++)
            {
                VisualTreeInfo                  vti2    = m_visualTreesList[pos];
                ToolStripMenuItem               newItem = new ToolStripMenuItem();
                int                             fileIdx = pos + 1;
                IR.SourceCodeTracker.SourceCode srcFile = vti2.Input;

                newItem.Text     = string.Format( fileIdx < 10 ? "&{0} {1}{2}" : "{0} {1}{2}", fileIdx, vti2.DisplayName, (srcFile != null && srcFile.UsingCachedValues) ? " <cached>" : "" );
                newItem.Tag      = vti2;
                newItem.Checked  = (vti2 == vti);
                newItem.Click   += new System.EventHandler( delegate( object sender, EventArgs e )
                {
                    SwitchToFile( vti2, null );
                } );

                coll.Add( newItem );
            }
        }

        //--//

        private void UpdateDisplay( bool fProcessCodeCoverage )
        {
            ResetVisualEffects();

            if(fProcessCodeCoverage)
            {
                ProcessCodeCoverage();
            }

            VisualTreeInfo switchToVti;
            VisualItem     bringIntoView;

            CreateVisualEffects( out switchToVti, out bringIntoView );

            PrepareDisplay( switchToVti, m_selectedThread.StackFrame, bringIntoView );

            SynchronizeBreakpointsUI();

            NotifyNewStackFrame();
        }

        private void ResetVisualEffects()
        {
            foreach(VisualTreeInfo vti in codeView1.VisualTrees.Values)
            {
                vti.EnumerateVisualEffects( delegate( VisualEffect ve )
                {
                    if(ve is VisualEffect.InlineDisassembly)
                    {
                        return VisualTreeInfo.CallbackResult.Delete;
                    }

                    if(ve is VisualEffect.SourceCodeHighlight)
                    {
                        if(ve.Context == null)
                        {
                            return VisualTreeInfo.CallbackResult.Delete;
                        }
                    }

                    return VisualTreeInfo.CallbackResult.Keep;
                } );
            }
        }

        private void CreateVisualEffects( out VisualTreeInfo switchToVti   ,
                                          out VisualItem     bringIntoView )
        {
            GrowOnlySet< Debugging.DebugInfo > visited = SetFactory.NewWithReferenceEquality< Debugging.DebugInfo >();

            switchToVti   = null;
            bringIntoView = null;

            m_versionStackTrace++;

            foreach(StackFrame sf in m_selectedThread.StackTrace)
            {
                Debugging.DebugInfo di = sf.DebugInfo;

                if(di != null)
                {
                    string         file = di.SrcFileName;
                    VisualTreeInfo vti  = codeView1.CreateVisualTree( m_imageInformation, file, file );
                    if(vti != null)
                    {
                        VisualEffect.SourceCodeHighlight ve;

                        if(sf.Depth == 0)
                        {
                            ve = vti.HighlightSourceCode( codeView1, sf.ProgramCounter, di, m_icon_CurrentStatement, null, m_brush_CurrentPC, c_visualEffect_Depth_CurrentStatement );
                        }
                        else
                        {
                            if(visited.Contains( di ) == false)
                            {
                                ve = vti.HighlightSourceCode( codeView1, sf.ProgramCounter, di, m_icon_StackFrame, null, m_brush_PastPC, c_visualEffect_Depth_OnTheStackTrace );
                            }
                            else
                            {
                                ve = null;
                            }
                        }

                        if(ve != null && sf == m_selectedThread.StackFrame)
                        {
                            switchToVti   = vti;
                            bringIntoView = ve.TopSelectedLine;
                        }
                    }

                    visited.Insert( di );
                }
            }
        }

        private void PrepareDisplay( VisualTreeInfo vti           ,
                                     StackFrame     sf            ,
                                     VisualItem     bringIntoView )
        {
            if(vti == null)
            {
                vti = codeView1.CreateEmptyVisualTree( "<disassembly>" );
            }

            vti.ClearVisualEffects( typeof(VisualEffect.InlineDisassembly) );

            if(m_currentSession.DisplayDisassembly || sf == null)
            {
                InstallDisassemblyBlock( vti, sf );
            }

            SwitchToFile( vti, bringIntoView );
        }

        private void InstallDisassemblyBlock( VisualTreeInfo vti ,
                                              StackFrame     sf  )
        {
            List< ImageInformation.DisassemblyLine > disasm;
            ContainerVisualItem                      line                       = null;
            uint                                     pastOpcodesToDisassembly   = m_currentSession.PastOpcodesToDisassembly;
            uint                                     futureOpcodesToDisassembly = m_currentSession.FutureOpcodesToDisassembly;

            if(sf != null && sf.Region != null && sf.DebugInfo != null)
            {
                disasm = sf.DisassembleBlock( m_processorHost.MemoryDelta, pastOpcodesToDisassembly, futureOpcodesToDisassembly );

                line = vti.FindLine( sf.DebugInfo.EndLineNumber );
            }
            else
            {
                ContainerVisualItem topElement = vti.VisualTreeRoot;
                GraphicsContext     ctx        = codeView1.CtxForLayout;

                topElement.Clear();

                line = new ContainerVisualItem( null );
                topElement.Add( line );

                TextVisualItem item = new TextVisualItem( null, "<No Source Code Available>" );

                item.PrepareText( ctx );

                line.Add( item );

                //--//

                disasm = new List< ImageInformation.DisassemblyLine >();

                Emulation.Hosting.ProcessorStatus svc; m_processorHost.GetHostingService( out svc );

                uint address      =               svc.ProgramCounter;
                uint addressStart = address - 3 * pastOpcodesToDisassembly   * sizeof(uint);
                uint addressEnd   = address + 3 * futureOpcodesToDisassembly * sizeof(uint);

                m_imageInformation.DisassembleBlock( m_processorHost.MemoryDelta, disasm, addressStart, address, addressEnd );
            }

            if(disasm.Count > 0)
            {
                vti.InstallDisassemblyBlock( line, codeView1, disasm.ToArray(), c_visualEffect_Depth_Disassembly );

                SynchronizeBreakpointsUI();
            }
        }

        //--//

        public static void GrayOutRowInDataGridView( DataGridViewRow row )
        {
            foreach(DataGridViewCell cell in row.Cells)
            {
                cell.Style.SelectionBackColor = SystemColors.Window;
                cell.Style.SelectionForeColor = SystemColors.GrayText;
                cell.Style.ForeColor          = SystemColors.GrayText;
            }
        }

        public static void GrayOutRowsInDataGridView( DataGridViewRowCollection rows )
        {
            foreach(DataGridViewRow row in rows)
            {
                GrayOutRowInDataGridView( row );
            }
        }

        //--//

        void ProcessKeyDownEvent( KeyEventArgs e )
        {
            if(e.Handled == false)
            {
                bool fGot = false;

                switch(e.KeyCode)
                {
                    case Keys.Up:
                        switch(e.Modifiers)
                        {
                            case Keys.Control:
                                Action_MoveInTheStackTrace( -1 );
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.Down:
                        switch(e.Modifiers)
                        {
                            case Keys.Control:
                                Action_MoveInTheStackTrace( 1 );
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.Left:
                        switch(e.Modifiers)
                        {
                            case Keys.Control:
                                Action_MoveInTheThreadList( -1 );
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.Right:
                        switch(e.Modifiers)
                        {
                            case Keys.Control:
                                Action_MoveInTheThreadList( 1 );
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.F5:
                        switch(e.Modifiers)
                        {
                            case Keys.Control | Keys.Shift:
                                Action_Restart();
                                fGot = true;
                                break;

                            case Keys.Shift:
                                Action_StopDebugging();
                                fGot = true;
                                break;

                            case Keys.None:
                                Action_Start();
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.Cancel:
                        switch(e.Modifiers)
                        {
                            case Keys.Control | Keys.Alt:
                                Action_BreakAll();
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.F10:
                        switch(e.Modifiers)
                        {
                            case Keys.None:
                                Action_StepOver();
                                fGot = true;
                                break;
                        }
                        break;

                    case Keys.F11:
                        switch(e.Modifiers)
                        {
                            case Keys.None:
                                Action_StepInto();
                                fGot = true;
                                break;

                            case Keys.Control:
                                Action_ToggleDisassembly();
                                fGot = true;
                                break;

                            case Keys.Shift:
                                Action_StepOut();
                                fGot = true;
                                break;
                        }
                        break;
                }

                if(fGot)
                {
                    e.Handled          = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        void ProcessKeyUpEvent( KeyEventArgs e )
        {
        }

        //--//

        private void NotifyNewStackFrame()
        {
            ExecuteInFormThread( delegate()
            {
                m_hostingSiteImpl.RaiseNotification( Hst.Forms.HostingSite.VisualizationEvent.NewStackFrame );
            } );
        }

        private void SynchronizeBreakpointsUI()
        {
            ExecuteInFormThread( delegate()
            {
                foreach(VisualTreeInfo vti in codeView1.VisualTrees.Values)
                {
                    vti.EnumerateVisualEffects( delegate( VisualEffect ve )
                    {
                        if(ve is VisualEffect.SourceCodeHighlight)
                        {
                            if(ve.Context is Emulation.Hosting.Breakpoint)
                            {
                                return VisualTreeInfo.CallbackResult.Delete;
                            }
                        }

                        return VisualTreeInfo.CallbackResult.Keep;
                    } );
                }

                foreach(Emulation.Hosting.Breakpoint bp in m_processorHost.Breakpoints)
                {
                    if(bp.ShowInUI)
                    {
                        IR.ImageBuilders.SequentialRegion reg;
                        uint                              offset;
                        Debugging.DebugInfo               di;

                        if(m_imageInformation.LocateFirstSourceCode( bp.Address, out reg, out offset, out di ))
                        {
                            if(bp.DebugInfo != null)
                            {
                                di = bp.DebugInfo;
                            }

                            if(di != null)
                            {
                                string         file = di.SrcFileName;
                                VisualTreeInfo vti  = codeView1.GetVisualTree( file );
                                if(vti != null)
                                {
                                    VisualEffect.SourceCodeHighlight ve;

                                    if(bp.IsActive)
                                    {
                                        ve = vti.HighlightSourceCode( codeView1, uint.MaxValue, di, m_icon_Breakpoint, null, m_brush_Breakpoint, c_visualEffect_Depth_Breakpoint );
                                    }
                                    else
                                    {
                                        ve = vti.HighlightSourceCode( codeView1, uint.MaxValue, di, m_icon_BreakpointDisabled, m_pen_Breakpoint, null, c_visualEffect_Depth_Breakpoint );
                                    }

                                    ve.Context = bp;
                                    ve.Version = bp.Version;
                                }
                            }
                        }

                        foreach(VisualTreeInfo vti in codeView1.VisualTrees.Values)
                        {
                            VisualEffect.SourceCodeHighlight ve;

                            if(bp.IsActive)
                            {
                                ve = vti.HighlightSourceCode( codeView1, bp.Address, null, m_icon_Breakpoint, null, m_brush_Breakpoint, c_visualEffect_Depth_Breakpoint );
                            }
                            else
                            {
                                ve = vti.HighlightSourceCode( codeView1, bp.Address, null, m_icon_BreakpointDisabled, m_pen_Breakpoint, null, c_visualEffect_Depth_Breakpoint );
                            }

                            ve.Context = bp;
                            ve.Version = bp.Version;
                        }
                    }
                }

                codeView1.RefreshVisualTree();

                m_hostingSiteImpl.RaiseNotification( Hst.Forms.HostingSite.VisualizationEvent.BreakpointsChange );
            } );
        }

        //
        // Access Methods
        //

        public Hst.Forms.HostingSite HostingSite
        {
            get
            {
                return m_hostingSiteImpl;
            }
        }

        public Hst.AbstractHost Host
        {
            get
            {
                return m_processorHost;
            }
        }

        public MemoryDelta MemoryDelta
        {
            get
            {
                return m_processorHost.MemoryDelta;
            }
        }

        public ImageInformation ImageInformation
        {
            get
            {
                return m_imageInformation;
            }
        }

        public int VersionBreakpoints
        {
            get
            {
                return m_versionBreakpoints;
            }
        }

        //--//

        public Hst.Forms.HostingSite.ExecutionState CurrentState
        {
            get
            {
                return m_currentState;
            }
        }

        public Session CurrentSession
        {
            get
            {
                return m_currentSession;
            }

            set
            {
                m_currentSession = value;

                InnerAction_LoadImage();
            }
        }

        //--//

        public List< ThreadStatus > Threads
        {
            get
            {
                return m_threads;
            }
        }

        public ThreadStatus ActiveThread
        {
            get
            {
                return m_activeThread;
            }
        }

        public ThreadStatus SelectedThread
        {
            get
            {
                return m_selectedThread;
            }
        }

        public StackFrame SelectedStackFrame
        {
            get
            {
                if(m_selectedThread != null)
                {
                    return m_selectedThread.StackFrame;
                }

                return null;
            }
        }

        public int VersionStackTrace
        {
            get
            {
                return m_versionStackTrace;
            }
        }

        //--//

        public Configuration.Environment.Manager ConfigurationManager
        {
            get
            {
                return m_configurationManager;
            }
        }

        //--//

        public bool IsIdle
        {
            get
            {
                switch(m_currentState)
                {
                    case Hst.Forms.HostingSite.ExecutionState.Idle:
                    case Hst.Forms.HostingSite.ExecutionState.Loaded:
                    case Hst.Forms.HostingSite.ExecutionState.Paused:
                        return true;
                }

                return false;
            }
        }

        //
        // Event Methods
        //

        private void codeView1_HitSink( CodeView       owner  ,
                                        VisualItem     origin ,
                                        PointF         relPos ,
                                        MouseEventArgs e      ,
                                        bool           fDown  ,
                                        bool           fUp    )
        {
            VisualTreeInfo vti = codeView1.ActiveVisualTree;
            if(vti != null)
            {
                if(fDown && e.Clicks >= 2)
                {
                    Debugging.DebugInfo diTarget = origin.Context as Debugging.DebugInfo;
                    if(diTarget != null)
                    {
                        List< ImageInformation.RangeToSourceCode > lst;
                        Debugging.DebugInfo                        diTargetLine;
                        Debugging.DebugInfo                        diClosest          = null;
                        bool                                       fClosestIntersects = false;
                        string                                     fileName           = vti.Input.File;

                        diTargetLine = Debugging.DebugInfo.CreateMarkerForLine( diTarget.SrcFileName, diTarget.MethodName, diTarget.BeginLineNumber );

                        if(m_imageInformation.FileToCodeLookup.TryGetValue( fileName.ToUpper(), out lst ))
                        {
                            foreach(ImageInformation.RangeToSourceCode rng in lst)
                            {
                                foreach(var pair in rng.DebugInfos)
                                {
                                    Debugging.DebugInfo di = pair.Info;
                                    if(di.SrcFileName == fileName)
                                    {
                                        Debugging.DebugInfo diLine = diTargetLine.ComputeIntersection( di );
                                        if(diLine != null)
                                        {
                                            Debugging.DebugInfo diIntersection = diTarget.ComputeIntersection( di );

                                            if(diIntersection != null)
                                            {
                                                if(diClosest == null || di.IsContainedIn( diClosest ) || fClosestIntersects == false)
                                                {
                                                    diClosest          = di;
                                                    fClosestIntersects = true;
                                                }
                                            }
                                            else
                                            {
                                                if(diClosest == null)
                                                {
                                                    diClosest = di;
                                                }
                                                else if(fClosestIntersects == false)
                                                {
                                                    int diff1 = Math.Abs( di       .BeginColumn - diTarget.BeginColumn );
                                                    int diff2 = Math.Abs( diClosest.BeginColumn - diTarget.BeginColumn );

                                                    if(diff1 < diff2)
                                                    {
                                                        diClosest = di;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(diClosest != null)
                        {
                            //
                            // Remove all the existing break points having the same DebugInfo.
                            //
                            bool fAdd = true;

                            foreach(Emulation.Hosting.Breakpoint bp in m_processorHost.Breakpoints)
                            {
                                IR.ImageBuilders.SequentialRegion reg;
                                uint                              offset;
                                Debugging.DebugInfo               di;
                                bool                              fDelete = false;

                                if(bp.DebugInfo == diClosest)
                                {
                                    fDelete = true;
                                }
                                else
                                {
                                    if(m_imageInformation.LocateFirstSourceCode( bp.Address, out reg, out offset, out di ))
                                    {
                                        if(di == diClosest)
                                        {
                                            fDelete = true;
                                        }
                                    }
                                }

                                if(fDelete)
                                {
                                    Action_RemoveBreakpoint( bp );
                                    fAdd = false;
                                }
                            }

                            if(fAdd && m_imageInformation.SourceCodeToCodeLookup.TryGetValue( diClosest, out lst ))
                            {
                                foreach(ImageInformation.RangeToSourceCode rng in lst)
                                {
                                    Action_SetBreakpoint( rng.BaseAddress, diClosest, true, false, null );
                                    break;
                                }
                            }
                        }
                    }

                    var disasm = origin.Context as ImageInformation.DisassemblyLine;
                    if(disasm != null)
                    {
                        IR.ImageBuilders.SequentialRegion reg;
                        uint                              offset;
                        Debugging.DebugInfo               di;

                        if(m_imageInformation.LocateFirstSourceCode( disasm.Address, out reg, out offset, out di ))
                        {
                            //
                            // Remove all the existing break points having the same DebugInfo.
                            //
                            bool fAdd = true;

                            foreach(Emulation.Hosting.Breakpoint bp in m_processorHost.Breakpoints)
                            {
                                if(disasm.Address == bp.Address)
                                {
                                    Action_RemoveBreakpoint( bp );
                                    fAdd = false;
                                }
                            }

                            if(fAdd)
                            {
                                Action_SetBreakpoint( disasm.Address, di, true, false, null );
                            }
                        }
                    }
                }
            }
        }

        private void DebuggerMainForm_Load( object sender, EventArgs e )
        {
            m_processorWorker.Start();
        }

        private void DebuggerMainForm_KeyDown( object       sender ,
                                               KeyEventArgs e      )
        {
            ProcessKeyDownEvent( e );
        }

        private void DebuggerMainForm_FormClosing( object               sender ,
                                                   FormClosingEventArgs e      )
        {
            foreach(Hst.AbstractUIPlugIn plugIn in m_plugIns)
            {
                plugIn.Stop();
            }

            ExecuteInWorkerThread( Hst.Forms.HostingSite.ExecutionState.Invalid, delegate()
            {
                m_processorWorkerExit = true;
            } );

            InnerAction_StopExecution();

            m_processorWorker.Join();

            m_sessionManagerForm.SaveSessions();
        }

        //--//

        private void toolStripMenuItem_File_Open_Click( object    sender ,
                                                        EventArgs e      )
        {
            Action_LoadImage();
        }

        private void toolStripMenuItem_File_Session_Load_Click( object    sender ,
                                                                EventArgs e      )
        {
            Action_LoadSession();
        }

        private void toolStripMenuItem_File_Session_Edit_Click( object    sender ,
                                                                EventArgs e      )
        {
            Action_EditConfiguration();
        }

        private void toolStripMenuItem_File_Session_Save_Click( object    sender ,
                                                                EventArgs e      )
        {
            if(m_currentSession.SettingsFile != null)
            {
                Action_SaveSession( m_currentSession.SettingsFile );
            }
        }

        private void toolStripMenuItem_File_Session_SaveAs_Click( object    sender ,
                                                                  EventArgs e      )
        {
            Action_SaveSession();
        }

        private void toolStripMenuItem_File_Exit_Click( object    sender ,
                                                        EventArgs e      )
        {
            this.Close();
        }

        //--//

        private void toolStripMenuItem_Tools_SessionManager_Click( object    sender ,
                                                                   EventArgs e      )
        {
            Session session = m_sessionManagerForm.SelectSession( false );

            if(session != null && session != m_currentSession)
            {
                m_currentSession = session;

                if(InnerAction_LoadImage() == false)
                {
                    m_imageInformation = null;
                }
            }
        }

        //--//

        private void toolStripButton_Start_Click( object    sender ,
                                                  EventArgs e      )
        {
            Action_Start();
        }

        private void toolStripButton_BreakAll_Click( object    sender ,
                                                     EventArgs e      )
        {
            Action_BreakAll();
        }

        private void toolStripButton_StopDebugging_Click( object    sender ,
                                                          EventArgs e      )
        {
            Action_StopDebugging();
        }

        private void toolStripButton_Restart_Click( object    sender ,
                                                    EventArgs e      )
        {
            Action_Restart();
        }

        private void toolStripButton_ShowNextStatement_Click( object    sender ,
                                                              EventArgs e      )
        {
            if(m_activeThread != null)
            {
                m_selectedThread = m_activeThread;

                m_activeThread.StackFrame = m_activeThread.TopStackFrame;

                Action_SelectThread( this.Threads.IndexOf( m_activeThread ) );
            }
        }

        private void toolStripButton_StepInto_Click( object    sender ,
                                                     EventArgs e      )
        {
            Action_StepInto();
        }

        private void toolStripButton_StepOver_Click( object sender, EventArgs e )
        {
            Action_StepOver();
        }

        private void toolStripButton_StepOut_Click( object sender, EventArgs e )
        {
            Action_StepOut();
        }

        private void toolStripButton_ToggleDisassembly_Click( object sender, EventArgs e )
        {
            Action_ToggleDisassembly();
        }

        private void toolStripButton_ToggleWrapper_Click( object sender, EventArgs e )
        {
            Action_ToggleWrapper();
        }

        //--//

        private void statusTrip1_DoubleClick( object sender, EventArgs e )
        {
            if(this.IsIdle)
            {
                Action_ResetAbsoluteTime();

                DisplayExecutionTime();
            }
        }
    }
}
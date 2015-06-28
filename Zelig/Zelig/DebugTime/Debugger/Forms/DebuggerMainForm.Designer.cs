namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class DebuggerMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( DebuggerMainForm ) );
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_File = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_File_Open = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_File_Open_Image = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_File_Open_Session = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_File_Session = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_File_Session_Load = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_File_Session_Edit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_File_Session_Save = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_File_Session_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_File_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_View = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Tools = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Tools_SessionManager = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_Tools_Top = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator_Tools_Bottom = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Tools_Customize = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Tools_Options = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Windows = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_Files = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Help_Contents = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Help_Index = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Help_Search = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Help_About = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus_ExecutionState = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatus_AbsoluteTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatus_CurrentMethod = new System.Windows.Forms.ToolStripStatusLabel();
            this.imageOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_Open = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Start = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_BreakAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_StopDebugging = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Restart = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_ShowNextStatement = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_StepInto = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_StepOver = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_StepOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_ToggleDisassembly = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.codeView1 = new Microsoft.Zelig.Debugger.ArmProcessor.CodeView();
            this.tabControl_Data = new System.Windows.Forms.TabControl();
            this.tabPage_Locals = new System.Windows.Forms.TabPage();
            this.localsView1 = new Microsoft.Zelig.Debugger.ArmProcessor.LocalsView();
            this.tabPage_Registers = new System.Windows.Forms.TabPage();
            this.registersView1 = new Microsoft.Zelig.Debugger.ArmProcessor.RegistersView();
            this.tabPage_StackTrace = new System.Windows.Forms.TabPage();
            this.stackTraceView1 = new Microsoft.Zelig.Debugger.ArmProcessor.StackTraceView();
            this.tabPage_Threads = new System.Windows.Forms.TabPage();
            this.threadsView1 = new Microsoft.Zelig.Debugger.ArmProcessor.ThreadsView();
            this.tabPage_Memory = new System.Windows.Forms.TabPage();
            this.memoryView1 = new Microsoft.Zelig.Debugger.ArmProcessor.MemoryView();
            this.tabPage_Breakpoints = new System.Windows.Forms.TabPage();
            this.breakpointsView1 = new Microsoft.Zelig.Debugger.ArmProcessor.BreakpointsView();
            this.sessionSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.sessionOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolStripButton_ToggleWrapper = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl_Data.SuspendLayout();
            this.tabPage_Locals.SuspendLayout();
            this.tabPage_Registers.SuspendLayout();
            this.tabPage_StackTrace.SuspendLayout();
            this.tabPage_Threads.SuspendLayout();
            this.tabPage_Memory.SuspendLayout();
            this.tabPage_Breakpoints.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_File,
            this.toolStripMenuItem_View,
            this.toolStripMenuItem_Tools,
            this.toolStripMenuItem_Windows,
            this.toolStripMenuItem_Help} );
            this.menuStrip1.Location = new System.Drawing.Point( 0, 0 );
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size( 1006, 24 );
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem_File
            // 
            this.toolStripMenuItem_File.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator4,
            this.toolStripMenuItem_File_Open,
            this.toolStripSeparator6,
            this.toolStripMenuItem_File_Session,
            this.toolStripSeparator1,
            this.toolStripMenuItem_File_Exit} );
            this.toolStripMenuItem_File.Name = "toolStripMenuItem_File";
            this.toolStripMenuItem_File.Size = new System.Drawing.Size( 35, 20 );
            this.toolStripMenuItem_File.Text = "&File";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size( 118, 6 );
            // 
            // toolStripMenuItem_File_Open
            // 
            this.toolStripMenuItem_File_Open.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_File_Open_Image,
            this.toolStripMenuItem_File_Open_Session} );
            this.toolStripMenuItem_File_Open.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.Open;
            this.toolStripMenuItem_File_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripMenuItem_File_Open.Name = "toolStripMenuItem_File_Open";
            this.toolStripMenuItem_File_Open.Size = new System.Drawing.Size( 121, 22 );
            this.toolStripMenuItem_File_Open.Text = "&Open";
            // 
            // toolStripMenuItem_File_Open_Image
            // 
            this.toolStripMenuItem_File_Open_Image.Name = "toolStripMenuItem_File_Open_Image";
            this.toolStripMenuItem_File_Open_Image.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItem_File_Open_Image.Size = new System.Drawing.Size( 167, 22 );
            this.toolStripMenuItem_File_Open_Image.Text = "&Image...";
            this.toolStripMenuItem_File_Open_Image.Click += new System.EventHandler( this.toolStripMenuItem_File_Open_Click );
            // 
            // toolStripMenuItem_File_Open_Session
            // 
            this.toolStripMenuItem_File_Open_Session.Name = "toolStripMenuItem_File_Open_Session";
            this.toolStripMenuItem_File_Open_Session.Size = new System.Drawing.Size( 167, 22 );
            this.toolStripMenuItem_File_Open_Session.Text = "&Session...";
            this.toolStripMenuItem_File_Open_Session.Click += new System.EventHandler( this.toolStripMenuItem_File_Session_Load_Click );
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size( 118, 6 );
            // 
            // toolStripMenuItem_File_Session
            // 
            this.toolStripMenuItem_File_Session.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_File_Session_Load,
            this.toolStripMenuItem_File_Session_Edit,
            this.toolStripMenuItem_File_Session_Save,
            this.toolStripMenuItem_File_Session_SaveAs} );
            this.toolStripMenuItem_File_Session.Name = "toolStripMenuItem_File_Session";
            this.toolStripMenuItem_File_Session.Size = new System.Drawing.Size( 121, 22 );
            this.toolStripMenuItem_File_Session.Text = "Session";
            // 
            // toolStripMenuItem_File_Session_Load
            // 
            this.toolStripMenuItem_File_Session_Load.Name = "toolStripMenuItem_File_Session_Load";
            this.toolStripMenuItem_File_Session_Load.Size = new System.Drawing.Size( 171, 22 );
            this.toolStripMenuItem_File_Session_Load.Text = "&Load...";
            this.toolStripMenuItem_File_Session_Load.Click += new System.EventHandler( this.toolStripMenuItem_File_Session_Load_Click );
            // 
            // toolStripMenuItem_File_Session_Edit
            // 
            this.toolStripMenuItem_File_Session_Edit.Name = "toolStripMenuItem_File_Session_Edit";
            this.toolStripMenuItem_File_Session_Edit.Size = new System.Drawing.Size( 171, 22 );
            this.toolStripMenuItem_File_Session_Edit.Text = "&Edit Configuration";
            this.toolStripMenuItem_File_Session_Edit.Click += new System.EventHandler( this.toolStripMenuItem_File_Session_Edit_Click );
            // 
            // toolStripMenuItem_File_Session_Save
            // 
            this.toolStripMenuItem_File_Session_Save.Name = "toolStripMenuItem_File_Session_Save";
            this.toolStripMenuItem_File_Session_Save.Size = new System.Drawing.Size( 171, 22 );
            this.toolStripMenuItem_File_Session_Save.Text = "&Save";
            this.toolStripMenuItem_File_Session_Save.Click += new System.EventHandler( this.toolStripMenuItem_File_Session_Save_Click );
            // 
            // toolStripMenuItem_File_Session_SaveAs
            // 
            this.toolStripMenuItem_File_Session_SaveAs.Name = "toolStripMenuItem_File_Session_SaveAs";
            this.toolStripMenuItem_File_Session_SaveAs.Size = new System.Drawing.Size( 171, 22 );
            this.toolStripMenuItem_File_Session_SaveAs.Text = "Save &As...";
            this.toolStripMenuItem_File_Session_SaveAs.Click += new System.EventHandler( this.toolStripMenuItem_File_Session_SaveAs_Click );
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size( 118, 6 );
            // 
            // toolStripMenuItem_File_Exit
            // 
            this.toolStripMenuItem_File_Exit.Name = "toolStripMenuItem_File_Exit";
            this.toolStripMenuItem_File_Exit.Size = new System.Drawing.Size( 121, 22 );
            this.toolStripMenuItem_File_Exit.Text = "E&xit";
            this.toolStripMenuItem_File_Exit.Click += new System.EventHandler( this.toolStripMenuItem_File_Exit_Click );
            // 
            // toolStripMenuItem_View
            // 
            this.toolStripMenuItem_View.Name = "toolStripMenuItem_View";
            this.toolStripMenuItem_View.Size = new System.Drawing.Size( 41, 20 );
            this.toolStripMenuItem_View.Text = "&View";
            // 
            // toolStripMenuItem_Tools
            // 
            this.toolStripMenuItem_Tools.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Tools_SessionManager,
            this.toolStripSeparator_Tools_Top,
            this.toolStripSeparator_Tools_Bottom,
            this.toolStripMenuItem_Tools_Customize,
            this.toolStripMenuItem_Tools_Options} );
            this.toolStripMenuItem_Tools.Name = "toolStripMenuItem_Tools";
            this.toolStripMenuItem_Tools.Size = new System.Drawing.Size( 44, 20 );
            this.toolStripMenuItem_Tools.Text = "&Tools";
            // 
            // toolStripMenuItem_Tools_SessionManager
            // 
            this.toolStripMenuItem_Tools_SessionManager.Name = "toolStripMenuItem_Tools_SessionManager";
            this.toolStripMenuItem_Tools_SessionManager.Size = new System.Drawing.Size( 178, 22 );
            this.toolStripMenuItem_Tools_SessionManager.Text = "&Session Manager...";
            this.toolStripMenuItem_Tools_SessionManager.Click += new System.EventHandler( this.toolStripMenuItem_Tools_SessionManager_Click );
            // 
            // toolStripSeparator_Tools_Top
            // 
            this.toolStripSeparator_Tools_Top.Name = "toolStripSeparator_Tools_Top";
            this.toolStripSeparator_Tools_Top.Size = new System.Drawing.Size( 175, 6 );
            // 
            // toolStripSeparator_Tools_Bottom
            // 
            this.toolStripSeparator_Tools_Bottom.Name = "toolStripSeparator_Tools_Bottom";
            this.toolStripSeparator_Tools_Bottom.Size = new System.Drawing.Size( 175, 6 );
            this.toolStripSeparator_Tools_Bottom.Visible = false;
            // 
            // toolStripMenuItem_Tools_Customize
            // 
            this.toolStripMenuItem_Tools_Customize.Name = "toolStripMenuItem_Tools_Customize";
            this.toolStripMenuItem_Tools_Customize.Size = new System.Drawing.Size( 178, 22 );
            this.toolStripMenuItem_Tools_Customize.Text = "&Customize";
            // 
            // toolStripMenuItem_Tools_Options
            // 
            this.toolStripMenuItem_Tools_Options.Name = "toolStripMenuItem_Tools_Options";
            this.toolStripMenuItem_Tools_Options.Size = new System.Drawing.Size( 178, 22 );
            this.toolStripMenuItem_Tools_Options.Text = "&Options";
            // 
            // toolStripMenuItem_Windows
            // 
            this.toolStripMenuItem_Windows.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator_Files} );
            this.toolStripMenuItem_Windows.Name = "toolStripMenuItem_Windows";
            this.toolStripMenuItem_Windows.Size = new System.Drawing.Size( 62, 20 );
            this.toolStripMenuItem_Windows.Text = "&Windows";
            // 
            // toolStripSeparator_Files
            // 
            this.toolStripSeparator_Files.Name = "toolStripSeparator_Files";
            this.toolStripSeparator_Files.Size = new System.Drawing.Size( 57, 6 );
            this.toolStripSeparator_Files.Visible = false;
            // 
            // toolStripMenuItem_Help
            // 
            this.toolStripMenuItem_Help.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Help_Contents,
            this.toolStripMenuItem_Help_Index,
            this.toolStripMenuItem_Help_Search,
            this.toolStripSeparator5,
            this.toolStripMenuItem_Help_About} );
            this.toolStripMenuItem_Help.Name = "toolStripMenuItem_Help";
            this.toolStripMenuItem_Help.Size = new System.Drawing.Size( 40, 20 );
            this.toolStripMenuItem_Help.Text = "&Help";
            // 
            // toolStripMenuItem_Help_Contents
            // 
            this.toolStripMenuItem_Help_Contents.Name = "toolStripMenuItem_Help_Contents";
            this.toolStripMenuItem_Help_Contents.Size = new System.Drawing.Size( 129, 22 );
            this.toolStripMenuItem_Help_Contents.Text = "&Contents";
            // 
            // toolStripMenuItem_Help_Index
            // 
            this.toolStripMenuItem_Help_Index.Name = "toolStripMenuItem_Help_Index";
            this.toolStripMenuItem_Help_Index.Size = new System.Drawing.Size( 129, 22 );
            this.toolStripMenuItem_Help_Index.Text = "&Index";
            // 
            // toolStripMenuItem_Help_Search
            // 
            this.toolStripMenuItem_Help_Search.Name = "toolStripMenuItem_Help_Search";
            this.toolStripMenuItem_Help_Search.Size = new System.Drawing.Size( 129, 22 );
            this.toolStripMenuItem_Help_Search.Text = "&Search";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size( 126, 6 );
            // 
            // toolStripMenuItem_Help_About
            // 
            this.toolStripMenuItem_Help_About.Name = "toolStripMenuItem_Help_About";
            this.toolStripMenuItem_Help_About.Size = new System.Drawing.Size( 129, 22 );
            this.toolStripMenuItem_Help_About.Text = "&About...";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus_ExecutionState,
            this.toolStripStatus_AbsoluteTime,
            this.toolStripStatus_CurrentMethod} );
            this.statusStrip1.Location = new System.Drawing.Point( 0, 511 );
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size( 1006, 22 );
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.DoubleClick += new System.EventHandler( this.statusTrip1_DoubleClick );
            // 
            // toolStripStatus_ExecutionState
            // 
            this.toolStripStatus_ExecutionState.Name = "toolStripStatus_ExecutionState";
            this.toolStripStatus_ExecutionState.Size = new System.Drawing.Size( 25, 17 );
            this.toolStripStatus_ExecutionState.Text = "Idle";
            // 
            // toolStripStatus_AbsoluteTime
            // 
            this.toolStripStatus_AbsoluteTime.Name = "toolStripStatus_AbsoluteTime";
            this.toolStripStatus_AbsoluteTime.Size = new System.Drawing.Size( 0, 17 );
            // 
            // toolStripStatus_CurrentMethod
            // 
            this.toolStripStatus_CurrentMethod.Name = "toolStripStatus_CurrentMethod";
            this.toolStripStatus_CurrentMethod.Size = new System.Drawing.Size( 0, 17 );
            // 
            // imageOpenFileDialog
            // 
            this.imageOpenFileDialog.Filter = "Zelig Image|*.ZeligImage|Hex files|*.hex|ADS images|*.axfdump";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_Open,
            this.toolStripSeparator,
            this.toolStripButton_Start,
            this.toolStripButton_BreakAll,
            this.toolStripButton_StopDebugging,
            this.toolStripButton_Restart,
            this.toolStripSeparator2,
            this.toolStripButton_ShowNextStatement,
            this.toolStripButton_StepInto,
            this.toolStripButton_StepOver,
            this.toolStripButton_StepOut,
            this.toolStripSeparator3,
            this.toolStripButton_ToggleDisassembly,
            this.toolStripButton_ToggleWrapper} );
            this.toolStrip1.Location = new System.Drawing.Point( 0, 24 );
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size( 1006, 25 );
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_Open
            // 
            this.toolStripButton_Open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Open.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.Open;
            this.toolStripButton_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Open.Name = "toolStripButton_Open";
            this.toolStripButton_Open.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_Open.Text = "Open (Ctrl+O)";
            this.toolStripButton_Open.Click += new System.EventHandler( this.toolStripMenuItem_File_Open_Click );
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size( 6, 25 );
            // 
            // toolStripButton_Start
            // 
            this.toolStripButton_Start.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Start.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.Start;
            this.toolStripButton_Start.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Start.Name = "toolStripButton_Start";
            this.toolStripButton_Start.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_Start.Text = "Start / Continue (F5)";
            this.toolStripButton_Start.Click += new System.EventHandler( this.toolStripButton_Start_Click );
            // 
            // toolStripButton_BreakAll
            // 
            this.toolStripButton_BreakAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_BreakAll.Enabled = false;
            this.toolStripButton_BreakAll.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.BreakAll;
            this.toolStripButton_BreakAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_BreakAll.Name = "toolStripButton_BreakAll";
            this.toolStripButton_BreakAll.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_BreakAll.Text = "Break All (Ctrl+Alt+Break)";
            this.toolStripButton_BreakAll.Click += new System.EventHandler( this.toolStripButton_BreakAll_Click );
            // 
            // toolStripButton_StopDebugging
            // 
            this.toolStripButton_StopDebugging.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_StopDebugging.Enabled = false;
            this.toolStripButton_StopDebugging.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.StopDebugging;
            this.toolStripButton_StopDebugging.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_StopDebugging.Name = "toolStripButton_StopDebugging";
            this.toolStripButton_StopDebugging.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_StopDebugging.Text = "Stop Debugging (Shift+F5)";
            this.toolStripButton_StopDebugging.Click += new System.EventHandler( this.toolStripButton_StopDebugging_Click );
            // 
            // toolStripButton_Restart
            // 
            this.toolStripButton_Restart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Restart.Enabled = false;
            this.toolStripButton_Restart.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.Restart;
            this.toolStripButton_Restart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Restart.Name = "toolStripButton_Restart";
            this.toolStripButton_Restart.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_Restart.Text = "Restart (Ctrl+Shift+F5)";
            this.toolStripButton_Restart.Click += new System.EventHandler( this.toolStripButton_Restart_Click );
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size( 6, 25 );
            // 
            // toolStripButton_ShowNextStatement
            // 
            this.toolStripButton_ShowNextStatement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ShowNextStatement.Enabled = false;
            this.toolStripButton_ShowNextStatement.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.ShowNextStatement;
            this.toolStripButton_ShowNextStatement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ShowNextStatement.Name = "toolStripButton_ShowNextStatement";
            this.toolStripButton_ShowNextStatement.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_ShowNextStatement.Text = "Show Next Statement";
            this.toolStripButton_ShowNextStatement.Click += new System.EventHandler( this.toolStripButton_ShowNextStatement_Click );
            // 
            // toolStripButton_StepInto
            // 
            this.toolStripButton_StepInto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_StepInto.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.StepInto;
            this.toolStripButton_StepInto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_StepInto.Name = "toolStripButton_StepInto";
            this.toolStripButton_StepInto.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_StepInto.Text = "Step Into (F11)";
            this.toolStripButton_StepInto.Click += new System.EventHandler( this.toolStripButton_StepInto_Click );
            // 
            // toolStripButton_StepOver
            // 
            this.toolStripButton_StepOver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_StepOver.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.StepOver;
            this.toolStripButton_StepOver.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_StepOver.Name = "toolStripButton_StepOver";
            this.toolStripButton_StepOver.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_StepOver.Text = "Step Over (F10)";
            this.toolStripButton_StepOver.Click += new System.EventHandler( this.toolStripButton_StepOver_Click );
            // 
            // toolStripButton_StepOut
            // 
            this.toolStripButton_StepOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_StepOut.Enabled = false;
            this.toolStripButton_StepOut.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.StepOut;
            this.toolStripButton_StepOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_StepOut.Name = "toolStripButton_StepOut";
            this.toolStripButton_StepOut.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_StepOut.Text = "Step Out (Shift+F11)";
            this.toolStripButton_StepOut.Click += new System.EventHandler( this.toolStripButton_StepOut_Click );
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size( 6, 25 );
            // 
            // toolStripButton_ToggleDisassembly
            // 
            this.toolStripButton_ToggleDisassembly.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ToggleDisassembly.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.ToggleDisassembly;
            this.toolStripButton_ToggleDisassembly.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ToggleDisassembly.Name = "toolStripButton_ToggleDisassembly";
            this.toolStripButton_ToggleDisassembly.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_ToggleDisassembly.Text = "Toggle Disassembly (Ctrl+F11)";
            this.toolStripButton_ToggleDisassembly.Click += new System.EventHandler( this.toolStripButton_ToggleDisassembly_Click );
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point( 0, 52 );
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add( this.codeView1 );
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add( this.tabControl_Data );
            this.splitContainer1.Size = new System.Drawing.Size( 1006, 456 );
            this.splitContainer1.SplitterDistance = 473;
            this.splitContainer1.TabIndex = 2;
            // 
            // codeView1
            // 
            this.codeView1.AutoScroll = true;
            this.codeView1.DefaultHitSink = null;
            this.codeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeView1.FallbackHitSink = null;
            this.codeView1.Font = new System.Drawing.Font( "Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
            this.codeView1.Location = new System.Drawing.Point( 0, 0 );
            this.codeView1.Name = "codeView1";
            this.codeView1.Size = new System.Drawing.Size( 473, 456 );
            this.codeView1.TabIndex = 0;
            // 
            // tabControl_Data
            // 
            this.tabControl_Data.Controls.Add( this.tabPage_Locals );
            this.tabControl_Data.Controls.Add( this.tabPage_Registers );
            this.tabControl_Data.Controls.Add( this.tabPage_StackTrace );
            this.tabControl_Data.Controls.Add( this.tabPage_Threads );
            this.tabControl_Data.Controls.Add( this.tabPage_Memory );
            this.tabControl_Data.Controls.Add( this.tabPage_Breakpoints );
            this.tabControl_Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Data.Location = new System.Drawing.Point( 0, 0 );
            this.tabControl_Data.Name = "tabControl_Data";
            this.tabControl_Data.SelectedIndex = 0;
            this.tabControl_Data.ShowToolTips = true;
            this.tabControl_Data.Size = new System.Drawing.Size( 529, 456 );
            this.tabControl_Data.TabIndex = 0;
            // 
            // tabPage_Locals
            // 
            this.tabPage_Locals.Controls.Add( this.localsView1 );
            this.tabPage_Locals.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage_Locals.Name = "tabPage_Locals";
            this.tabPage_Locals.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage_Locals.Size = new System.Drawing.Size( 521, 430 );
            this.tabPage_Locals.TabIndex = 1;
            this.tabPage_Locals.Text = "Locals";
            this.tabPage_Locals.UseVisualStyleBackColor = true;
            // 
            // localsView1
            // 
            this.localsView1.AutoScroll = true;
            this.localsView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localsView1.Location = new System.Drawing.Point( 3, 3 );
            this.localsView1.Name = "localsView1";
            this.localsView1.Size = new System.Drawing.Size( 515, 424 );
            this.localsView1.TabIndex = 0;
            // 
            // tabPage_Registers
            // 
            this.tabPage_Registers.Controls.Add( this.registersView1 );
            this.tabPage_Registers.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage_Registers.Name = "tabPage_Registers";
            this.tabPage_Registers.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage_Registers.Size = new System.Drawing.Size( 521, 430 );
            this.tabPage_Registers.TabIndex = 2;
            this.tabPage_Registers.Text = "Registers";
            this.tabPage_Registers.UseVisualStyleBackColor = true;
            // 
            // registersView1
            // 
            this.registersView1.AutoScroll = true;
            this.registersView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.registersView1.Location = new System.Drawing.Point( 3, 3 );
            this.registersView1.Name = "registersView1";
            this.registersView1.Size = new System.Drawing.Size( 515, 424 );
            this.registersView1.TabIndex = 0;
            // 
            // tabPage_StackTrace
            // 
            this.tabPage_StackTrace.Controls.Add( this.stackTraceView1 );
            this.tabPage_StackTrace.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage_StackTrace.Name = "tabPage_StackTrace";
            this.tabPage_StackTrace.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage_StackTrace.Size = new System.Drawing.Size( 521, 430 );
            this.tabPage_StackTrace.TabIndex = 3;
            this.tabPage_StackTrace.Text = "Stack Trace";
            this.tabPage_StackTrace.ToolTipText = "Use Ctrl+Up and Ctrl+Down to switch between stack frames";
            this.tabPage_StackTrace.UseVisualStyleBackColor = true;
            // 
            // stackTraceView1
            // 
            this.stackTraceView1.AutoScroll = true;
            this.stackTraceView1.AutoSize = true;
            this.stackTraceView1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.stackTraceView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stackTraceView1.Location = new System.Drawing.Point( 3, 3 );
            this.stackTraceView1.Name = "stackTraceView1";
            this.stackTraceView1.Size = new System.Drawing.Size( 515, 424 );
            this.stackTraceView1.TabIndex = 0;
            // 
            // tabPage_Threads
            // 
            this.tabPage_Threads.Controls.Add( this.threadsView1 );
            this.tabPage_Threads.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage_Threads.Name = "tabPage_Threads";
            this.tabPage_Threads.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage_Threads.Size = new System.Drawing.Size( 521, 430 );
            this.tabPage_Threads.TabIndex = 4;
            this.tabPage_Threads.Text = "Threads";
            this.tabPage_Threads.UseVisualStyleBackColor = true;
            // 
            // threadsView1
            // 
            this.threadsView1.AutoScroll = true;
            this.threadsView1.AutoSize = true;
            this.threadsView1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.threadsView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threadsView1.Location = new System.Drawing.Point( 3, 3 );
            this.threadsView1.Name = "threadsView1";
            this.threadsView1.Size = new System.Drawing.Size( 515, 424 );
            this.threadsView1.TabIndex = 0;
            // 
            // tabPage_Memory
            // 
            this.tabPage_Memory.Controls.Add( this.memoryView1 );
            this.tabPage_Memory.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage_Memory.Name = "tabPage_Memory";
            this.tabPage_Memory.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage_Memory.Size = new System.Drawing.Size( 521, 430 );
            this.tabPage_Memory.TabIndex = 5;
            this.tabPage_Memory.Text = "Memory";
            this.tabPage_Memory.UseVisualStyleBackColor = true;
            // 
            // memoryView1
            // 
            this.memoryView1.AutoSize = true;
            this.memoryView1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.memoryView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memoryView1.Location = new System.Drawing.Point( 3, 3 );
            this.memoryView1.Name = "memoryView1";
            this.memoryView1.Size = new System.Drawing.Size( 515, 424 );
            this.memoryView1.TabIndex = 0;
            // 
            // tabPage_Breakpoints
            // 
            this.tabPage_Breakpoints.Controls.Add( this.breakpointsView1 );
            this.tabPage_Breakpoints.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage_Breakpoints.Name = "tabPage_Breakpoints";
            this.tabPage_Breakpoints.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage_Breakpoints.Size = new System.Drawing.Size( 521, 430 );
            this.tabPage_Breakpoints.TabIndex = 6;
            this.tabPage_Breakpoints.Text = "Breakpoints";
            this.tabPage_Breakpoints.UseVisualStyleBackColor = true;
            // 
            // breakpointsView1
            // 
            this.breakpointsView1.AutoScroll = true;
            this.breakpointsView1.AutoSize = true;
            this.breakpointsView1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.breakpointsView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.breakpointsView1.Location = new System.Drawing.Point( 3, 3 );
            this.breakpointsView1.Name = "breakpointsView1";
            this.breakpointsView1.Size = new System.Drawing.Size( 515, 424 );
            this.breakpointsView1.TabIndex = 0;
            // 
            // sessionSaveFileDialog
            // 
            this.sessionSaveFileDialog.Filter = "Zelig Debug Session|*.ZeligDebugSession";
            this.sessionSaveFileDialog.Title = "Save Session";
            // 
            // sessionOpenFileDialog
            // 
            this.sessionOpenFileDialog.Filter = "Zelig Debug Session|*.ZeligDebugSession";
            this.sessionOpenFileDialog.Title = "Open Session";
            // 
            // toolStripButton_ToggleWrapper
            // 
            this.toolStripButton_ToggleWrapper.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ToggleWrapper.Image = ((System.Drawing.Image)(resources.GetObject( "toolStripButton_ToggleWrapper.Image" )));
            this.toolStripButton_ToggleWrapper.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ToggleWrapper.Name = "toolStripButton_ToggleWrapper";
            this.toolStripButton_ToggleWrapper.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_ToggleWrapper.Text = "toolStripButton1";
            this.toolStripButton_ToggleWrapper.Click += new System.EventHandler( this.toolStripButton_ToggleWrapper_Click );
            // 
            // DebuggerMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 1006, 533 );
            this.Controls.Add( this.toolStrip1 );
            this.Controls.Add( this.splitContainer1 );
            this.Controls.Add( this.statusStrip1 );
            this.Controls.Add( this.menuStrip1 );
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DebuggerMainForm";
            this.Text = "Zelig Debugger";
            this.Load += new System.EventHandler( this.DebuggerMainForm_Load );
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.DebuggerMainForm_FormClosing );
            this.KeyDown += new System.Windows.Forms.KeyEventHandler( this.DebuggerMainForm_KeyDown );
            this.menuStrip1.ResumeLayout( false );
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout( false );
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout( false );
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout( false );
            this.splitContainer1.Panel2.ResumeLayout( false );
            this.splitContainer1.ResumeLayout( false );
            this.tabControl_Data.ResumeLayout( false );
            this.tabPage_Locals.ResumeLayout( false );
            this.tabPage_Registers.ResumeLayout( false );
            this.tabPage_StackTrace.ResumeLayout( false );
            this.tabPage_StackTrace.PerformLayout();
            this.tabPage_Threads.ResumeLayout( false );
            this.tabPage_Threads.PerformLayout();
            this.tabPage_Memory.ResumeLayout( false );
            this.tabPage_Memory.PerformLayout();
            this.tabPage_Breakpoints.ResumeLayout( false );
            this.tabPage_Breakpoints.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private CodeView codeView1;
        private LocalsView localsView1;
        private RegistersView registersView1;
        private StackTraceView stackTraceView1;
        private ThreadsView threadsView1;
        private BreakpointsView breakpointsView1;
        private MemoryView memoryView1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Open;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Open_Image;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Open_Session;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Session;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Session_Load;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Session_Edit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Session_Save;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Session_SaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File_Exit;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Tools;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Tools_SessionManager;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Tools_Customize;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Tools_Options;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help_Contents;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help_Index;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help_Search;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help_About;

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Windows;

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl_Data;
        private System.Windows.Forms.TabPage tabPage_Locals;
        private System.Windows.Forms.TabPage tabPage_Registers;
        private System.Windows.Forms.TabPage tabPage_StackTrace;
        private System.Windows.Forms.TabPage tabPage_Threads;
        private System.Windows.Forms.TabPage tabPage_Memory;
        private System.Windows.Forms.TabPage tabPage_Breakpoints;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Open;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton toolStripButton_Start;
        private System.Windows.Forms.ToolStripButton toolStripButton_ShowNextStatement;
        private System.Windows.Forms.ToolStripButton toolStripButton_StopDebugging;
        private System.Windows.Forms.ToolStripButton toolStripButton_Restart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_BreakAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_StepInto;
        private System.Windows.Forms.ToolStripButton toolStripButton_StepOver;
        private System.Windows.Forms.ToolStripButton toolStripButton_StepOut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_ToggleDisassembly;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus_ExecutionState;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus_CurrentMethod;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus_AbsoluteTime;
        private System.Windows.Forms.OpenFileDialog imageOpenFileDialog;
        private System.Windows.Forms.SaveFileDialog sessionSaveFileDialog;
        private System.Windows.Forms.OpenFileDialog sessionOpenFileDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator_Files;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_View;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator_Tools_Top;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator_Tools_Bottom;
        private System.Windows.Forms.ToolStripButton toolStripButton_ToggleWrapper;
    }
}
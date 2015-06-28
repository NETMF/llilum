namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class ProfilerMainForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_New = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Start = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBox_Mode = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBox_View = new System.Windows.Forms.ToolStripComboBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add( this.toolStrip1 );
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point( 0, 0 );
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size( 668, 25 );
            this.panelTop.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_New,
            this.toolStripButton_Start,
            this.toolStripButton_Stop,
            this.toolStripComboBox_Mode,
            this.toolStripSeparator1,
            this.toolStripComboBox_View} );
            this.toolStrip1.Location = new System.Drawing.Point( 0, 0 );
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size( 668, 25 );
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_New
            // 
            this.toolStripButton_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_New.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.NewSession;
            this.toolStripButton_New.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButton_New.Name = "toolStripButton_New";
            this.toolStripButton_New.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_New.Text = "New";
            this.toolStripButton_New.ToolTipText = "Create New Profiling Session";
            this.toolStripButton_New.Click += new System.EventHandler( this.toolStripButton_New_Click );
            // 
            // toolStripButton_Start
            // 
            this.toolStripButton_Start.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Start.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.StartSession;
            this.toolStripButton_Start.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButton_Start.Name = "toolStripButton_Start";
            this.toolStripButton_Start.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_Start.Text = "Start";
            this.toolStripButton_Start.ToolTipText = "Start Collecting Data";
            this.toolStripButton_Start.Click += new System.EventHandler( this.toolStripButton_Start_Click );
            // 
            // toolStripButton_Stop
            // 
            this.toolStripButton_Stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Stop.Image = global::Microsoft.Zelig.Debugger.ArmProcessor.Properties.Resources.StopSession;
            this.toolStripButton_Stop.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButton_Stop.Name = "toolStripButton_Stop";
            this.toolStripButton_Stop.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_Stop.Text = "Stop";
            this.toolStripButton_Stop.ToolTipText = "Stop Collecting Data";
            this.toolStripButton_Stop.Click += new System.EventHandler( this.toolStripButton_Stop_Click );
            // 
            // toolStripComboBox_Mode
            // 
            this.toolStripComboBox_Mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_Mode.Items.AddRange( new object[] {
            "Calls Only",
            "Calls and Allocations"} );
            this.toolStripComboBox_Mode.Name = "toolStripComboBox_Mode";
            this.toolStripComboBox_Mode.Size = new System.Drawing.Size( 121, 25 );
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size( 6, 25 );
            // 
            // toolStripComboBox_View
            // 
            this.toolStripComboBox_View.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_View.Items.AddRange( new object[] {
            "Call Tree",
            "Functions",
            "Classes",
            "Caller / Callee",
            "Memory Allocations"} );
            this.toolStripComboBox_View.Name = "toolStripComboBox_View";
            this.toolStripComboBox_View.Size = new System.Drawing.Size( 121, 25 );
            this.toolStripComboBox_View.SelectedIndexChanged += new System.EventHandler( this.toolStripComboBox_View_SelectedIndexChanged );
            // 
            // panelBottom
            // 
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point( 0, 25 );
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size( 668, 440 );
            this.panelBottom.TabIndex = 1;
            // 
            // ProfilerMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 668, 465 );
            this.Controls.Add( this.panelBottom );
            this.Controls.Add( this.panelTop );
            this.KeyPreview = true;
            this.Name = "ProfilerMainForm";
            this.Text = "Profiler Control";
            this.panelTop.ResumeLayout( false );
            this.panelTop.PerformLayout();
            this.toolStrip1.ResumeLayout( false );
            this.toolStrip1.PerformLayout();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_New;
        private System.Windows.Forms.ToolStripButton toolStripButton_Start;
        private System.Windows.Forms.ToolStripButton toolStripButton_Stop;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_Mode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_View;
    }
}
namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class BreakpointsView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( BreakpointsView ) );
            this.panel_Breakpoints_Search = new System.Windows.Forms.Panel();
            this.listBox_Breakpoint_SearchResults = new System.Windows.Forms.ListBox();
            this.label_Breakpoint_Search = new System.Windows.Forms.Label();
            this.textBox_Breakpoint_Search = new System.Windows.Forms.TextBox();
            this.dataGridView_Breakpoints = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_DeleteBreakpoint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_DeleteAllBreakpoints = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_ToggleAllBreakpoints = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewImageColumn_Breakpoints_Status = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewTextBoxColumn_Breakpoints_Method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn_Breakpoints_Condition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn_Breakpoints_HitCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel_Breakpoints_Search.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Breakpoints)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Breakpoints_Search
            // 
            this.panel_Breakpoints_Search.Controls.Add( this.listBox_Breakpoint_SearchResults );
            this.panel_Breakpoints_Search.Controls.Add( this.label_Breakpoint_Search );
            this.panel_Breakpoints_Search.Controls.Add( this.textBox_Breakpoint_Search );
            this.panel_Breakpoints_Search.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Breakpoints_Search.Location = new System.Drawing.Point( 0, 0 );
            this.panel_Breakpoints_Search.Name = "panel_Breakpoints_Search";
            this.panel_Breakpoints_Search.Size = new System.Drawing.Size( 572, 187 );
            this.panel_Breakpoints_Search.TabIndex = 1;
            // 
            // listBox_Breakpoint_SearchResults
            // 
            this.listBox_Breakpoint_SearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_Breakpoint_SearchResults.FormattingEnabled = true;
            this.listBox_Breakpoint_SearchResults.HorizontalScrollbar = true;
            this.listBox_Breakpoint_SearchResults.Location = new System.Drawing.Point( 7, 30 );
            this.listBox_Breakpoint_SearchResults.Name = "listBox_Breakpoint_SearchResults";
            this.listBox_Breakpoint_SearchResults.Size = new System.Drawing.Size( 560, 147 );
            this.listBox_Breakpoint_SearchResults.TabIndex = 2;
            this.listBox_Breakpoint_SearchResults.DoubleClick += new System.EventHandler( this.listBox_Breakpoint_SearchResults_DoubleClick );
            this.listBox_Breakpoint_SearchResults.Click += new System.EventHandler( this.listBox_Breakpoint_SearchResults_Click );
            // 
            // label_Breakpoint_Search
            // 
            this.label_Breakpoint_Search.AutoSize = true;
            this.label_Breakpoint_Search.Location = new System.Drawing.Point( 4, 7 );
            this.label_Breakpoint_Search.Name = "label_Breakpoint_Search";
            this.label_Breakpoint_Search.Size = new System.Drawing.Size( 44, 13 );
            this.label_Breakpoint_Search.TabIndex = 1;
            this.label_Breakpoint_Search.Text = "Search:";
            // 
            // textBox_Breakpoint_Search
            // 
            this.textBox_Breakpoint_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Breakpoint_Search.Location = new System.Drawing.Point( 54, 4 );
            this.textBox_Breakpoint_Search.Name = "textBox_Breakpoint_Search";
            this.textBox_Breakpoint_Search.Size = new System.Drawing.Size( 513, 20 );
            this.textBox_Breakpoint_Search.TabIndex = 0;
            this.textBox_Breakpoint_Search.TextChanged += new System.EventHandler( this.textBox_Breakpoint_Search_TextChanged );
            // 
            // dataGridView_Breakpoints
            // 
            this.dataGridView_Breakpoints.AllowUserToAddRows = false;
            this.dataGridView_Breakpoints.AllowUserToDeleteRows = false;
            this.dataGridView_Breakpoints.AllowUserToResizeRows = false;
            this.dataGridView_Breakpoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_Breakpoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Breakpoints.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewImageColumn_Breakpoints_Status,
            this.dataGridViewTextBoxColumn_Breakpoints_Method,
            this.dataGridViewTextBoxColumn_Breakpoints_Condition,
            this.dataGridViewTextBoxColumn_Breakpoints_HitCount} );
            this.dataGridView_Breakpoints.Location = new System.Drawing.Point( 0, 28 );
            this.dataGridView_Breakpoints.Name = "dataGridView_Breakpoints";
            this.dataGridView_Breakpoints.ReadOnly = true;
            this.dataGridView_Breakpoints.RowHeadersWidth = 24;
            this.dataGridView_Breakpoints.Size = new System.Drawing.Size( 572, 254 );
            this.dataGridView_Breakpoints.TabIndex = 2;
            this.dataGridView_Breakpoints.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler( this.dataGridView_Breakpoints_CellContentClick );
            this.dataGridView_Breakpoints.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler( this.dataGridView_Breakpoints_CellContentDoubleClick );
            this.dataGridView_Breakpoints.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler( this.dataGridView_Breakpoints_CellContentClick );
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_DeleteBreakpoint,
            this.toolStripSeparator1,
            this.toolStripButton_DeleteAllBreakpoints,
            this.toolStripButton_ToggleAllBreakpoints} );
            this.toolStrip1.Location = new System.Drawing.Point( 0, 0 );
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size( 572, 25 );
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_DeleteBreakpoint
            // 
            this.toolStripButton_DeleteBreakpoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_DeleteBreakpoint.Enabled = false;
            this.toolStripButton_DeleteBreakpoint.Image = ((System.Drawing.Image)(resources.GetObject( "toolStripButton_DeleteBreakpoint.Image" )));
            this.toolStripButton_DeleteBreakpoint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_DeleteBreakpoint.Name = "toolStripButton_DeleteBreakpoint";
            this.toolStripButton_DeleteBreakpoint.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_DeleteBreakpoint.ToolTipText = "Delete Breakpoint";
            this.toolStripButton_DeleteBreakpoint.Click += new System.EventHandler( this.toolStripButton_DeleteBreakpoint_Click );
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size( 6, 25 );
            // 
            // toolStripButton_DeleteAllBreakpoints
            // 
            this.toolStripButton_DeleteAllBreakpoints.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_DeleteAllBreakpoints.Enabled = false;
            this.toolStripButton_DeleteAllBreakpoints.Image = ((System.Drawing.Image)(resources.GetObject( "toolStripButton_DeleteAllBreakpoints.Image" )));
            this.toolStripButton_DeleteAllBreakpoints.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_DeleteAllBreakpoints.Name = "toolStripButton_DeleteAllBreakpoints";
            this.toolStripButton_DeleteAllBreakpoints.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_DeleteAllBreakpoints.ToolTipText = "Delete All Breakpoints";
            this.toolStripButton_DeleteAllBreakpoints.Click += new System.EventHandler( this.toolStripButton_DeleteAllBreakpoints_Click );
            // 
            // toolStripButton_ToggleAllBreakpoints
            // 
            this.toolStripButton_ToggleAllBreakpoints.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ToggleAllBreakpoints.Enabled = false;
            this.toolStripButton_ToggleAllBreakpoints.Image = ((System.Drawing.Image)(resources.GetObject( "toolStripButton_ToggleAllBreakpoints.Image" )));
            this.toolStripButton_ToggleAllBreakpoints.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ToggleAllBreakpoints.Name = "toolStripButton_ToggleAllBreakpoints";
            this.toolStripButton_ToggleAllBreakpoints.Size = new System.Drawing.Size( 23, 22 );
            this.toolStripButton_ToggleAllBreakpoints.ToolTipText = "Toggle All Breakpoints";
            this.toolStripButton_ToggleAllBreakpoints.Click += new System.EventHandler( this.toolStripButton_ToggleAllBreakpoints_Click );
            // 
            // panel1
            // 
            this.panel1.Controls.Add( this.toolStrip1 );
            this.panel1.Controls.Add( this.dataGridView_Breakpoints );
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point( 0, 187 );
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size( 572, 282 );
            this.panel1.TabIndex = 4;
            // 
            // dataGridViewImageColumn_Breakpoints_Status
            // 
            this.dataGridViewImageColumn_Breakpoints_Status.HeaderText = "";
            this.dataGridViewImageColumn_Breakpoints_Status.Name = "dataGridViewImageColumn_Breakpoints_Status";
            this.dataGridViewImageColumn_Breakpoints_Status.ReadOnly = true;
            this.dataGridViewImageColumn_Breakpoints_Status.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageColumn_Breakpoints_Status.Width = 20;
            // 
            // dataGridViewTextBoxColumn_Breakpoints_Method
            // 
            this.dataGridViewTextBoxColumn_Breakpoints_Method.HeaderText = "Method";
            this.dataGridViewTextBoxColumn_Breakpoints_Method.Name = "dataGridViewTextBoxColumn_Breakpoints_Method";
            this.dataGridViewTextBoxColumn_Breakpoints_Method.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Breakpoints_Method.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn_Breakpoints_Condition
            // 
            this.dataGridViewTextBoxColumn_Breakpoints_Condition.HeaderText = "Condition";
            this.dataGridViewTextBoxColumn_Breakpoints_Condition.Name = "dataGridViewTextBoxColumn_Breakpoints_Condition";
            this.dataGridViewTextBoxColumn_Breakpoints_Condition.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Breakpoints_Condition.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn_Breakpoints_HitCount
            // 
            this.dataGridViewTextBoxColumn_Breakpoints_HitCount.HeaderText = "Hit Count";
            this.dataGridViewTextBoxColumn_Breakpoints_HitCount.Name = "dataGridViewTextBoxColumn_Breakpoints_HitCount";
            this.dataGridViewTextBoxColumn_Breakpoints_HitCount.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Breakpoints_HitCount.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // BreakpointsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.panel1 );
            this.Controls.Add( this.panel_Breakpoints_Search );
            this.Name = "BreakpointsView";
            this.Size = new System.Drawing.Size( 572, 469 );
            this.panel_Breakpoints_Search.ResumeLayout( false );
            this.panel_Breakpoints_Search.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Breakpoints)).EndInit();
            this.toolStrip1.ResumeLayout( false );
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout( false );
            this.panel1.PerformLayout();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Panel panel_Breakpoints_Search;
        private System.Windows.Forms.ListBox listBox_Breakpoint_SearchResults;
        private System.Windows.Forms.Label label_Breakpoint_Search;
        private System.Windows.Forms.TextBox textBox_Breakpoint_Search;
        private System.Windows.Forms.DataGridView dataGridView_Breakpoints;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_DeleteBreakpoint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_DeleteAllBreakpoints;
        private System.Windows.Forms.ToolStripButton toolStripButton_ToggleAllBreakpoints;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn_Breakpoints_Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Breakpoints_Method;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Breakpoints_Condition;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Breakpoints_HitCount;
    }
}

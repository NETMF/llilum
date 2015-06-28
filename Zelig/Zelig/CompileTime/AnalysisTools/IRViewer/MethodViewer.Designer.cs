namespace Microsoft.Zelig.Tools.IRViewer
{
    partial class MethodViewer
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
            this.gViewer1 = new Microsoft.Glee.GraphViewerGdi.GViewer();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageVariables = new System.Windows.Forms.TabPage();
            this.listViewVariables = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.tabPageBasicBlock = new System.Windows.Forms.TabPage();
            this.listBoxBasicBlock = new System.Windows.Forms.ListBox();
            this.checkBoxExpandBasicBlocks = new System.Windows.Forms.CheckBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageVariables.SuspendLayout();
            this.tabPageBasicBlock.SuspendLayout();
            this.SuspendLayout();
            // 
            // gViewer1
            // 
            this.gViewer1.AutoScroll = true;
            this.gViewer1.BackwardEnabled = false;
            this.gViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gViewer1.ForwardEnabled = false;
            this.gViewer1.Graph = null;
            this.gViewer1.Location = new System.Drawing.Point( 0, 0 );
            this.gViewer1.MouseHitDistance = 0.05;
            this.gViewer1.Name = "gViewer1";
            this.gViewer1.NavigationVisible = true;
            this.gViewer1.PanButtonPressed = false;
            this.gViewer1.SaveButtonVisible = true;
            this.gViewer1.Size = new System.Drawing.Size( 301, 373 );
            this.gViewer1.TabIndex = 0;
            this.gViewer1.ZoomF = 1;
            this.gViewer1.ZoomFraction = 0.5;
            this.gViewer1.ZoomWindowThreshold = 0.05;
            this.gViewer1.SelectionChanged += new System.EventHandler( this.gViewer1_SelectionChanged );
            this.gViewer1.MouseClick += new System.Windows.Forms.MouseEventHandler( this.gViewer1_MouseClick );
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point( 148, 10 );
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size( 716, 20 );
            this.textBox1.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point( 12, 39 );
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add( this.gViewer1 );
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add( this.tabControl1 );
            this.splitContainer1.Size = new System.Drawing.Size( 856, 373 );
            this.splitContainer1.SplitterDistance = 301;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add( this.tabPageVariables );
            this.tabControl1.Controls.Add( this.tabPageBasicBlock );
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point( 0, 0 );
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size( 551, 373 );
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageVariables
            // 
            this.tabPageVariables.Controls.Add( this.listViewVariables );
            this.tabPageVariables.Location = new System.Drawing.Point( 4, 22 );
            this.tabPageVariables.Name = "tabPageVariables";
            this.tabPageVariables.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPageVariables.Size = new System.Drawing.Size( 543, 347 );
            this.tabPageVariables.TabIndex = 1;
            this.tabPageVariables.Text = "Variables";
            this.tabPageVariables.UseVisualStyleBackColor = true;
            // 
            // listViewVariables
            // 
            this.listViewVariables.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2} );
            this.listViewVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewVariables.Location = new System.Drawing.Point( 3, 3 );
            this.listViewVariables.Name = "listViewVariables";
            this.listViewVariables.Size = new System.Drawing.Size( 537, 341 );
            this.listViewVariables.TabIndex = 0;
            this.listViewVariables.UseCompatibleStateImageBehavior = false;
            this.listViewVariables.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Type";
            this.columnHeader2.Width = 266;
            // 
            // tabPageBasicBlock
            // 
            this.tabPageBasicBlock.Controls.Add( this.listBoxBasicBlock );
            this.tabPageBasicBlock.Location = new System.Drawing.Point( 4, 22 );
            this.tabPageBasicBlock.Name = "tabPageBasicBlock";
            this.tabPageBasicBlock.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPageBasicBlock.Size = new System.Drawing.Size( 543, 347 );
            this.tabPageBasicBlock.TabIndex = 0;
            this.tabPageBasicBlock.Text = "Basic Block";
            this.tabPageBasicBlock.UseVisualStyleBackColor = true;
            // 
            // listBoxBasicBlock
            // 
            this.listBoxBasicBlock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxBasicBlock.FormattingEnabled = true;
            this.listBoxBasicBlock.HorizontalScrollbar = true;
            this.listBoxBasicBlock.IntegralHeight = false;
            this.listBoxBasicBlock.Location = new System.Drawing.Point( 3, 3 );
            this.listBoxBasicBlock.Name = "listBoxBasicBlock";
            this.listBoxBasicBlock.Size = new System.Drawing.Size( 537, 341 );
            this.listBoxBasicBlock.TabIndex = 0;
            // 
            // checkBoxExpandBasicBlocks
            // 
            this.checkBoxExpandBasicBlocks.AutoSize = true;
            this.checkBoxExpandBasicBlocks.Location = new System.Drawing.Point( 12, 12 );
            this.checkBoxExpandBasicBlocks.Name = "checkBoxExpandBasicBlocks";
            this.checkBoxExpandBasicBlocks.Size = new System.Drawing.Size( 126, 17 );
            this.checkBoxExpandBasicBlocks.TabIndex = 3;
            this.checkBoxExpandBasicBlocks.Text = "Expand Basic Blocks";
            this.checkBoxExpandBasicBlocks.UseVisualStyleBackColor = true;
            this.checkBoxExpandBasicBlocks.CheckedChanged += new System.EventHandler( this.checkBoxExpandBasicBlocks_CheckedChanged );
            // 
            // MethodViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 880, 424 );
            this.Controls.Add( this.checkBoxExpandBasicBlocks );
            this.Controls.Add( this.splitContainer1 );
            this.Controls.Add( this.textBox1 );
            this.Name = "MethodViewer";
            this.Text = "Method Viewer";
            this.Load += new System.EventHandler( this.MethodViewer_Load );
            this.splitContainer1.Panel1.ResumeLayout( false );
            this.splitContainer1.Panel2.ResumeLayout( false );
            this.splitContainer1.ResumeLayout( false );
            this.tabControl1.ResumeLayout( false );
            this.tabPageVariables.ResumeLayout( false );
            this.tabPageBasicBlock.ResumeLayout( false );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Glee.GraphViewerGdi.GViewer gViewer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageBasicBlock;
        private System.Windows.Forms.TabPage tabPageVariables;
        private System.Windows.Forms.ListBox listBoxBasicBlock;
        private System.Windows.Forms.ListView listViewVariables;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.CheckBox checkBoxExpandBasicBlocks;
    }
}
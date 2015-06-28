namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class RegistersView
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip( this.components );
            this.toolStripMenuItem_HexDisplay = new System.Windows.Forms.ToolStripMenuItem();
            this.treeBasedGridView_Registers = new Microsoft.Zelig.Debugger.ArmProcessor.TreeBasedGridView();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_HexDisplay} );
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size( 183, 48 );
            // 
            // toolStripMenuItem_HexDisplay
            // 
            this.toolStripMenuItem_HexDisplay.Name = "toolStripMenuItem_HexDisplay";
            this.toolStripMenuItem_HexDisplay.Size = new System.Drawing.Size( 182, 22 );
            this.toolStripMenuItem_HexDisplay.Text = "Hexadecimal Display";
            this.toolStripMenuItem_HexDisplay.CheckedChanged += new System.EventHandler( this.toolStripMenuItem_HexDisplay_CheckedChanged );
            this.toolStripMenuItem_HexDisplay.Click += new System.EventHandler( this.toolStripMenuItem_HexDisplay_Click );
            this.toolStripMenuItem_HexDisplay.Checked = true;
            // 
            // treeBasedGridView_Registers
            // 
            this.treeBasedGridView_Registers.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.treeBasedGridView_Registers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeBasedGridView_Registers.Location = new System.Drawing.Point( 0, 0 );
            this.treeBasedGridView_Registers.Name = "treeBasedGridView_Registers";
            this.treeBasedGridView_Registers.Size = new System.Drawing.Size( 647, 469 );
            this.treeBasedGridView_Registers.TabIndex = 0;
            this.treeBasedGridView_Registers.NodeMouseClick += new Microsoft.Zelig.Debugger.ArmProcessor.TreeBasedGridView.NodeMouseEventHandler( this.treeBasedGridView_Registers_CellMouseClick );
            // 
            // RegistersView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.treeBasedGridView_Registers );
            this.Name = "RegistersView";
            this.Size = new System.Drawing.Size( 647, 469 );
            this.contextMenuStrip1.ResumeLayout( false );
            this.ResumeLayout( false );

        }

        #endregion

        private TreeBasedGridView treeBasedGridView_Registers;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_HexDisplay;

    }
}

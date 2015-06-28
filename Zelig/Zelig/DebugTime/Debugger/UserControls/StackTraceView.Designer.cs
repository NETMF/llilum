namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class StackTraceView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView_StackTrace = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn_StackTrace_Flags = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewTextBoxColumn_StackTrace_Method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn_StackTrace_Depth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_StackTrace)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView_StackTrace
            // 
            this.dataGridView_StackTrace.AllowUserToAddRows = false;
            this.dataGridView_StackTrace.AllowUserToDeleteRows = false;
            this.dataGridView_StackTrace.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_StackTrace.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn_StackTrace_Flags,
            this.dataGridViewTextBoxColumn_StackTrace_Method,
            this.dataGridViewTextBoxColumn_StackTrace_Depth} );
            this.dataGridView_StackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_StackTrace.Location = new System.Drawing.Point( 0, 0 );
            this.dataGridView_StackTrace.Name = "dataGridView_StackTrace";
            this.dataGridView_StackTrace.ReadOnly = true;
            this.dataGridView_StackTrace.RowHeadersWidth = 4;
            this.dataGridView_StackTrace.Size = new System.Drawing.Size( 543, 483 );
            this.dataGridView_StackTrace.TabIndex = 1;
            this.dataGridView_StackTrace.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler( this.dataGridView_StackTrace_CellContentClick );
            // 
            // dataGridViewTextBoxColumn_StackTrace_Flags
            // 
            this.dataGridViewTextBoxColumn_StackTrace_Flags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            this.dataGridViewTextBoxColumn_StackTrace_Flags.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn_StackTrace_Flags.HeaderText = "";
            this.dataGridViewTextBoxColumn_StackTrace_Flags.Name = "dataGridViewTextBoxColumn_StackTrace_Flags";
            this.dataGridViewTextBoxColumn_StackTrace_Flags.ReadOnly = true;
            this.dataGridViewTextBoxColumn_StackTrace_Flags.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn_StackTrace_Flags.Width = 20;
            // 
            // dataGridViewTextBoxColumn_StackTrace_Method
            // 
            this.dataGridViewTextBoxColumn_StackTrace_Method.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn_StackTrace_Method.HeaderText = "Method";
            this.dataGridViewTextBoxColumn_StackTrace_Method.Name = "dataGridViewTextBoxColumn_StackTrace_Method";
            this.dataGridViewTextBoxColumn_StackTrace_Method.ReadOnly = true;
            this.dataGridViewTextBoxColumn_StackTrace_Method.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn_StackTrace_Depth
            // 
            this.dataGridViewTextBoxColumn_StackTrace_Depth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn_StackTrace_Depth.HeaderText = "Depth";
            this.dataGridViewTextBoxColumn_StackTrace_Depth.Name = "dataGridViewTextBoxColumn_StackTrace_Depth";
            this.dataGridViewTextBoxColumn_StackTrace_Depth.ReadOnly = true;
            this.dataGridViewTextBoxColumn_StackTrace_Depth.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn_StackTrace_Depth.Width = 50;
            // 
            // StackTraceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.dataGridView_StackTrace );
            this.Name = "StackTraceView";
            this.Size = new System.Drawing.Size( 543, 483 );
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_StackTrace)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_StackTrace;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewTextBoxColumn_StackTrace_Flags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_StackTrace_Method;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_StackTrace_Depth;
    }
}

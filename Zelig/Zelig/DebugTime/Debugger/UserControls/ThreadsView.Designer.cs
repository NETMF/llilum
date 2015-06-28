namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class ThreadsView
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
            this.dataGridView_Threads = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn_Threads_Flags = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewTextBoxColumn_Threads_Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn_Threads_Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn_Threads_Method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Threads)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView_Threads
            // 
            this.dataGridView_Threads.AllowUserToAddRows = false;
            this.dataGridView_Threads.AllowUserToDeleteRows = false;
            this.dataGridView_Threads.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Threads.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn_Threads_Flags,
            this.dataGridViewTextBoxColumn_Threads_Id,
            this.dataGridViewTextBoxColumn_Threads_Time,
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches,
            this.dataGridViewTextBoxColumn_Threads_Method} );
            this.dataGridView_Threads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView_Threads.Location = new System.Drawing.Point( 0, 0 );
            this.dataGridView_Threads.MultiSelect = false;
            this.dataGridView_Threads.Name = "dataGridView_Threads";
            this.dataGridView_Threads.ReadOnly = true;
            this.dataGridView_Threads.RowHeadersWidth = 4;
            this.dataGridView_Threads.Size = new System.Drawing.Size( 543, 483 );
            this.dataGridView_Threads.TabIndex = 1;
            this.dataGridView_Threads.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler( this.dataGridView_Threads_CellContentClick );
            // 
            // dataGridViewTextBoxColumn_Threads_Flags
            // 
            this.dataGridViewTextBoxColumn_Threads_Flags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White;
            this.dataGridViewTextBoxColumn_Threads_Flags.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn_Threads_Flags.HeaderText = "";
            this.dataGridViewTextBoxColumn_Threads_Flags.Name = "dataGridViewTextBoxColumn_Threads_Flags";
            this.dataGridViewTextBoxColumn_Threads_Flags.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Threads_Flags.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn_Threads_Flags.Width = 20;
            // 
            // dataGridViewTextBoxColumn_Threads_Id
            // 
            this.dataGridViewTextBoxColumn_Threads_Id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn_Threads_Id.HeaderText = "Id";
            this.dataGridViewTextBoxColumn_Threads_Id.Name = "dataGridViewTextBoxColumn_Threads_Id";
            this.dataGridViewTextBoxColumn_Threads_Id.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Threads_Id.Width = 80;
            // 
            // dataGridViewTextBoxColumn_Threads_Time
            // 
            this.dataGridViewTextBoxColumn_Threads_Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridViewTextBoxColumn_Threads_Time.HeaderText = "CPU %";
            this.dataGridViewTextBoxColumn_Threads_Time.Name = "dataGridViewTextBoxColumn_Threads_Time";
            this.dataGridViewTextBoxColumn_Threads_Time.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Threads_Time.Width = 61;
            // 
            // dataGridViewTextBoxColumn_Threads_ContextSwitches
            // 
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches.HeaderText = "# Context Switches";
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches.Name = "dataGridViewTextBoxColumn_Threads_ContextSwitches";
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches.ReadOnly = true;
            this.dataGridViewTextBoxColumn_Threads_ContextSwitches.Width = 114;
            // 
            // dataGridViewTextBoxColumn_Threads_Method
            // 
            this.dataGridViewTextBoxColumn_Threads_Method.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn_Threads_Method.HeaderText = "Method";
            this.dataGridViewTextBoxColumn_Threads_Method.Name = "dataGridViewTextBoxColumn_Threads_Method";
            this.dataGridViewTextBoxColumn_Threads_Method.ReadOnly = true;
            // 
            // ThreadsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add( this.dataGridView_Threads );
            this.Name = "ThreadsView";
            this.Size = new System.Drawing.Size( 543, 483 );
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Threads)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_Threads;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewTextBoxColumn_Threads_Flags;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Threads_Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Threads_Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Threads_ContextSwitches;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn_Threads_Method;
    }
}

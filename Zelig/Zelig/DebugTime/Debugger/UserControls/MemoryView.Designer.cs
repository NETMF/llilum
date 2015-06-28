namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class MemoryView
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox_ShowAsHex = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_ViewMode = new System.Windows.Forms.ComboBox();
            this.button_Refresh = new System.Windows.Forms.Button();
            this.textBox_Address = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.codeView1 = new Microsoft.Zelig.Debugger.ArmProcessor.CodeView();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add( this.checkBox_ShowAsHex );
            this.panel1.Controls.Add( this.label2 );
            this.panel1.Controls.Add( this.comboBox_ViewMode );
            this.panel1.Controls.Add( this.button_Refresh );
            this.panel1.Controls.Add( this.textBox_Address );
            this.panel1.Controls.Add( this.label1 );
            this.panel1.Location = new System.Drawing.Point( 0, 0 );
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size( 648, 65 );
            this.panel1.TabIndex = 1;
            // 
            // checkBox_ShowAsHex
            // 
            this.checkBox_ShowAsHex.AutoSize = true;
            this.checkBox_ShowAsHex.Checked = true;
            this.checkBox_ShowAsHex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_ShowAsHex.Location = new System.Drawing.Point( 189, 34 );
            this.checkBox_ShowAsHex.Name = "checkBox_ShowAsHex";
            this.checkBox_ShowAsHex.Size = new System.Drawing.Size( 135, 17 );
            this.checkBox_ShowAsHex.TabIndex = 5;
            this.checkBox_ShowAsHex.Text = "Show Numbers As Hex";
            this.checkBox_ShowAsHex.UseVisualStyleBackColor = true;
            this.checkBox_ShowAsHex.CheckedChanged += new System.EventHandler( this.checkBox_ShowAsHex_CheckedChanged );
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 3, 35 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 48, 13 );
            this.label2.TabIndex = 4;
            this.label2.Text = "View As:";
            // 
            // comboBox_ViewMode
            // 
            this.comboBox_ViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_ViewMode.FormattingEnabled = true;
            this.comboBox_ViewMode.Items.AddRange( new object[] {
            "Bytes",
            "Shorts",
            "Words",
            "Longs",
            "Chars",
            "Floats",
            "Doubles"} );
            this.comboBox_ViewMode.Location = new System.Drawing.Point( 57, 32 );
            this.comboBox_ViewMode.Name = "comboBox_ViewMode";
            this.comboBox_ViewMode.Size = new System.Drawing.Size( 121, 21 );
            this.comboBox_ViewMode.TabIndex = 3;
            this.comboBox_ViewMode.SelectedIndexChanged += new System.EventHandler( this.comboBox_ViewMode_SelectedIndexChanged );
            // 
            // button_Refresh
            // 
            this.button_Refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Refresh.Location = new System.Drawing.Point( 560, 6 );
            this.button_Refresh.Name = "button_Refresh";
            this.button_Refresh.Size = new System.Drawing.Size( 75, 23 );
            this.button_Refresh.TabIndex = 2;
            this.button_Refresh.Text = "Refresh";
            this.button_Refresh.UseVisualStyleBackColor = true;
            this.button_Refresh.Click += new System.EventHandler( this.button_Refresh_Click );
            // 
            // textBox_Address
            // 
            this.textBox_Address.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Address.Location = new System.Drawing.Point( 57, 6 );
            this.textBox_Address.Name = "textBox_Address";
            this.textBox_Address.Size = new System.Drawing.Size( 497, 20 );
            this.textBox_Address.TabIndex = 1;
            this.textBox_Address.KeyDown += new System.Windows.Forms.KeyEventHandler( this.textBox_Address_KeyDown );
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 3, 9 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 48, 13 );
            this.label1.TabIndex = 0;
            this.label1.Text = "Address:";
            // 
            // codeView1
            // 
            this.codeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.codeView1.AutoScroll = true;
            this.codeView1.DefaultHitSink = null;
            this.codeView1.FallbackHitSink = null;
            this.codeView1.Font = new System.Drawing.Font( "Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
            this.codeView1.Location = new System.Drawing.Point( 0, 71 );
            this.codeView1.Name = "codeView1";
            this.codeView1.Size = new System.Drawing.Size( 648, 409 );
            this.codeView1.TabIndex = 0;
            this.codeView1.SizeChanged += new System.EventHandler( this.codeView1_SizeChanged );
            // 
            // MemoryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add( this.panel1 );
            this.Controls.Add( this.codeView1 );
            this.Name = "MemoryView";
            this.Size = new System.Drawing.Size( 651, 483 );
            this.panel1.ResumeLayout( false );
            this.panel1.PerformLayout();
            this.ResumeLayout( false );

        }

        #endregion

        private CodeView codeView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_Refresh;
        private System.Windows.Forms.TextBox textBox_Address;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_ViewMode;
        private System.Windows.Forms.CheckBox checkBox_ShowAsHex;
    }
}

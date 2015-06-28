namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class OutputForm
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
            this.components = new System.ComponentModel.Container();
            this.checkBoxFileOnly = new System.Windows.Forms.CheckBox();
            this.checkBoxNoSleep = new System.Windows.Forms.CheckBox();
            this.checkBoxCalls = new System.Windows.Forms.CheckBox();
            this.checkBoxInstructions = new System.Windows.Forms.CheckBox();
            this.checkBoxRegisters = new System.Windows.Forms.CheckBox();
            this.checkBoxMemory = new System.Windows.Forms.CheckBox();
            this.buttonLoggingClear = new System.Windows.Forms.Button();
            this.richTextBoxOutput = new System.Windows.Forms.RichTextBox();
            this.buttonOutputBrowse = new System.Windows.Forms.Button();
            this.textBoxOutputFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer( this.components );
            this.saveLoggingOutputDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.checkBoxInterrupts = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxCodeCoverage = new System.Windows.Forms.CheckBox();
            this.buttonResetCodeCoverage = new System.Windows.Forms.Button();
            this.buttonDumpCodeCoverage = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxFileOnly
            // 
            this.checkBoxFileOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxFileOnly.AutoSize = true;
            this.checkBoxFileOnly.Location = new System.Drawing.Point( 539, 9 );
            this.checkBoxFileOnly.Name = "checkBoxFileOnly";
            this.checkBoxFileOnly.Size = new System.Drawing.Size( 117, 17 );
            this.checkBoxFileOnly.TabIndex = 24;
            this.checkBoxFileOnly.Text = "Output To File Only";
            this.checkBoxFileOnly.UseVisualStyleBackColor = true;
            // 
            // checkBoxNoSleep
            // 
            this.checkBoxNoSleep.AutoSize = true;
            this.checkBoxNoSleep.Location = new System.Drawing.Point( 226, 55 );
            this.checkBoxNoSleep.Name = "checkBoxNoSleep";
            this.checkBoxNoSleep.Size = new System.Drawing.Size( 103, 17 );
            this.checkBoxNoSleep.TabIndex = 23;
            this.checkBoxNoSleep.Text = "Skip Sleep Time";
            this.checkBoxNoSleep.UseVisualStyleBackColor = true;
            this.checkBoxNoSleep.CheckedChanged += new System.EventHandler( this.checkBoxNoSleep_CheckedChanged );
            // 
            // checkBoxCalls
            // 
            this.checkBoxCalls.AutoSize = true;
            this.checkBoxCalls.Location = new System.Drawing.Point( 15, 32 );
            this.checkBoxCalls.Name = "checkBoxCalls";
            this.checkBoxCalls.Size = new System.Drawing.Size( 79, 17 );
            this.checkBoxCalls.TabIndex = 22;
            this.checkBoxCalls.Text = "Trace Calls";
            this.checkBoxCalls.UseVisualStyleBackColor = true;
            this.checkBoxCalls.CheckedChanged += new System.EventHandler( this.checkBoxCalls_CheckedChanged );
            // 
            // checkBoxInstructions
            // 
            this.checkBoxInstructions.AutoSize = true;
            this.checkBoxInstructions.Location = new System.Drawing.Point( 111, 55 );
            this.checkBoxInstructions.Name = "checkBoxInstructions";
            this.checkBoxInstructions.Size = new System.Drawing.Size( 111, 17 );
            this.checkBoxInstructions.TabIndex = 21;
            this.checkBoxInstructions.Text = "Trace Instructions";
            this.checkBoxInstructions.UseVisualStyleBackColor = true;
            this.checkBoxInstructions.CheckedChanged += new System.EventHandler( this.checkBoxInstructions_CheckedChanged );
            // 
            // checkBoxRegisters
            // 
            this.checkBoxRegisters.AutoSize = true;
            this.checkBoxRegisters.Location = new System.Drawing.Point( 111, 32 );
            this.checkBoxRegisters.Name = "checkBoxRegisters";
            this.checkBoxRegisters.Size = new System.Drawing.Size( 101, 17 );
            this.checkBoxRegisters.TabIndex = 20;
            this.checkBoxRegisters.Text = "Trace Registers";
            this.checkBoxRegisters.UseVisualStyleBackColor = true;
            this.checkBoxRegisters.CheckedChanged += new System.EventHandler( this.checkBoxRegisters_CheckedChanged );
            // 
            // checkBoxMemory
            // 
            this.checkBoxMemory.AutoSize = true;
            this.checkBoxMemory.Location = new System.Drawing.Point( 15, 55 );
            this.checkBoxMemory.Name = "checkBoxMemory";
            this.checkBoxMemory.Size = new System.Drawing.Size( 94, 17 );
            this.checkBoxMemory.TabIndex = 19;
            this.checkBoxMemory.Text = "Trace Memory";
            this.checkBoxMemory.UseVisualStyleBackColor = true;
            this.checkBoxMemory.CheckedChanged += new System.EventHandler( this.checkBoxMemory_CheckedChanged );
            // 
            // buttonLoggingClear
            // 
            this.buttonLoggingClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoggingClear.Location = new System.Drawing.Point( 581, 430 );
            this.buttonLoggingClear.Name = "buttonLoggingClear";
            this.buttonLoggingClear.Size = new System.Drawing.Size( 75, 23 );
            this.buttonLoggingClear.TabIndex = 18;
            this.buttonLoggingClear.Text = "Clear";
            this.buttonLoggingClear.UseVisualStyleBackColor = true;
            this.buttonLoggingClear.Click += new System.EventHandler( this.buttonLoggingClear_Click );
            // 
            // richTextBoxOutput
            // 
            this.richTextBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxOutput.Font = new System.Drawing.Font( "Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
            this.richTextBoxOutput.Location = new System.Drawing.Point( 12, 95 );
            this.richTextBoxOutput.Name = "richTextBoxOutput";
            this.richTextBoxOutput.Size = new System.Drawing.Size( 644, 329 );
            this.richTextBoxOutput.TabIndex = 17;
            this.richTextBoxOutput.Text = "";
            this.richTextBoxOutput.WordWrap = false;
            // 
            // buttonOutputBrowse
            // 
            this.buttonOutputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOutputBrowse.Location = new System.Drawing.Point( 469, 5 );
            this.buttonOutputBrowse.Name = "buttonOutputBrowse";
            this.buttonOutputBrowse.Size = new System.Drawing.Size( 64, 23 );
            this.buttonOutputBrowse.TabIndex = 16;
            this.buttonOutputBrowse.Text = "Browse ...";
            this.buttonOutputBrowse.UseVisualStyleBackColor = true;
            this.buttonOutputBrowse.Click += new System.EventHandler( this.buttonOutputBrowse_Click );
            // 
            // textBoxOutputFile
            // 
            this.textBoxOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputFile.Location = new System.Drawing.Point( 76, 6 );
            this.textBoxOutputFile.Name = "textBoxOutputFile";
            this.textBoxOutputFile.Size = new System.Drawing.Size( 387, 20 );
            this.textBoxOutputFile.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 12, 9 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 58, 13 );
            this.label2.TabIndex = 14;
            this.label2.Text = "Output file:";
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            // 
            // checkBoxInterrupts
            // 
            this.checkBoxInterrupts.AutoSize = true;
            this.checkBoxInterrupts.Location = new System.Drawing.Point( 226, 32 );
            this.checkBoxInterrupts.Name = "checkBoxInterrupts";
            this.checkBoxInterrupts.Size = new System.Drawing.Size( 101, 17 );
            this.checkBoxInterrupts.TabIndex = 25;
            this.checkBoxInterrupts.Text = "Trace Interrupts";
            this.checkBoxInterrupts.UseVisualStyleBackColor = true;
            this.checkBoxInterrupts.CheckedChanged += new System.EventHandler( this.checkBoxInterrupts_CheckedChanged );
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add( this.checkBoxCodeCoverage );
            this.groupBox1.Controls.Add( this.buttonResetCodeCoverage );
            this.groupBox1.Controls.Add( this.buttonDumpCodeCoverage );
            this.groupBox1.Location = new System.Drawing.Point( 349, 33 );
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size( 307, 56 );
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Code Coverage";
            // 
            // checkBoxCodeCoverage
            // 
            this.checkBoxCodeCoverage.AutoSize = true;
            this.checkBoxCodeCoverage.Location = new System.Drawing.Point( 7, 22 );
            this.checkBoxCodeCoverage.Name = "checkBoxCodeCoverage";
            this.checkBoxCodeCoverage.Size = new System.Drawing.Size( 84, 17 );
            this.checkBoxCodeCoverage.TabIndex = 2;
            this.checkBoxCodeCoverage.Text = "Collect Data";
            this.checkBoxCodeCoverage.UseVisualStyleBackColor = true;
            this.checkBoxCodeCoverage.CheckedChanged += new System.EventHandler( this.checkBoxCodeCoverage_CheckedChanged );
            // 
            // buttonResetCodeCoverage
            // 
            this.buttonResetCodeCoverage.Enabled = false;
            this.buttonResetCodeCoverage.Location = new System.Drawing.Point( 109, 18 );
            this.buttonResetCodeCoverage.Name = "buttonResetCodeCoverage";
            this.buttonResetCodeCoverage.Size = new System.Drawing.Size( 75, 23 );
            this.buttonResetCodeCoverage.TabIndex = 0;
            this.buttonResetCodeCoverage.Text = "Reset";
            this.buttonResetCodeCoverage.UseVisualStyleBackColor = true;
            this.buttonResetCodeCoverage.Click += new System.EventHandler( this.buttonResetCodeCoverage_Click );
            // 
            // buttonDumpCodeCoverage
            // 
            this.buttonDumpCodeCoverage.Enabled = false;
            this.buttonDumpCodeCoverage.Location = new System.Drawing.Point( 190, 18 );
            this.buttonDumpCodeCoverage.Name = "buttonDumpCodeCoverage";
            this.buttonDumpCodeCoverage.Size = new System.Drawing.Size( 75, 23 );
            this.buttonDumpCodeCoverage.TabIndex = 1;
            this.buttonDumpCodeCoverage.Text = "Dump";
            this.buttonDumpCodeCoverage.UseVisualStyleBackColor = true;
            this.buttonDumpCodeCoverage.Click += new System.EventHandler( this.buttonDumpCodeCoverage_Click );
            // 
            // OutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 668, 465 );
            this.Controls.Add( this.groupBox1 );
            this.Controls.Add( this.checkBoxInterrupts );
            this.Controls.Add( this.checkBoxFileOnly );
            this.Controls.Add( this.checkBoxNoSleep );
            this.Controls.Add( this.checkBoxCalls );
            this.Controls.Add( this.checkBoxInstructions );
            this.Controls.Add( this.checkBoxRegisters );
            this.Controls.Add( this.checkBoxMemory );
            this.Controls.Add( this.buttonLoggingClear );
            this.Controls.Add( this.richTextBoxOutput );
            this.Controls.Add( this.buttonOutputBrowse );
            this.Controls.Add( this.textBoxOutputFile );
            this.Controls.Add( this.label2 );
            this.KeyPreview = true;
            this.Name = "OutputForm";
            this.Text = "Debug Output";
            this.groupBox1.ResumeLayout( false );
            this.groupBox1.PerformLayout();
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxFileOnly;
        private System.Windows.Forms.CheckBox checkBoxNoSleep;
        private System.Windows.Forms.CheckBox checkBoxCalls;
        private System.Windows.Forms.CheckBox checkBoxInstructions;
        private System.Windows.Forms.CheckBox checkBoxRegisters;
        private System.Windows.Forms.CheckBox checkBoxMemory;
        private System.Windows.Forms.Button buttonLoggingClear;
        private System.Windows.Forms.RichTextBox richTextBoxOutput;
        private System.Windows.Forms.Button buttonOutputBrowse;
        private System.Windows.Forms.TextBox textBoxOutputFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.SaveFileDialog saveLoggingOutputDialog1;
        private System.Windows.Forms.CheckBox checkBoxInterrupts;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxCodeCoverage;
        private System.Windows.Forms.Button buttonResetCodeCoverage;
        private System.Windows.Forms.Button buttonDumpCodeCoverage;
    }
}
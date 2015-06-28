namespace Microsoft.Zelig.Emulation.Hosting.Forms
{
    partial class BaseDebuggerForm
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
            this.SuspendLayout();
            // 
            // BaseDebuggerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 292, 266 );
            this.DoubleBuffered = true;
            this.Name = "BaseDebuggerForm";
            this.VisibleChanged += new System.EventHandler( this.BaseDebuggerForm_VisibleChanged );
            this.KeyUp += new System.Windows.Forms.KeyEventHandler( this.BaseDebuggerForm_KeyUp );
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.BaseDebuggerForm_FormClosing );
            this.KeyDown += new System.Windows.Forms.KeyEventHandler( this.BaseDebuggerForm_KeyDown );
            this.ResumeLayout( false );

        }

        #endregion
    }
}
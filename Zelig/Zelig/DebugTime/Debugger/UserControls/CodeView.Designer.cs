namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class CodeView
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
            this.SuspendLayout();
            // 
            // CodeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 7F, 15F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font( "Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)) );
            this.Name = "CodeView";
            this.Size = new System.Drawing.Size( 458, 403 );
            this.MouseDown += new System.Windows.Forms.MouseEventHandler( this.CodeView_MouseDown );
            this.MouseMove += new System.Windows.Forms.MouseEventHandler( this.CodeView_MouseMove );
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler( this.CodeView_MouseMove );
            this.MouseUp += new System.Windows.Forms.MouseEventHandler( this.CodeView_MouseUp );
            this.ResumeLayout( false );

        }

        #endregion
    }
}

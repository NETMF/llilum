namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class DisplayForm
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
            this.button_Spare = new System.Windows.Forms.Button();
            this.button_Channel = new System.Windows.Forms.Button();
            this.button_Backlight = new System.Windows.Forms.Button();
            this.button_Down = new System.Windows.Forms.Button();
            this.button_Enter = new System.Windows.Forms.Button();
            this.button_Up = new System.Windows.Forms.Button();
            this.pictureBox_LCD = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer( this.components );
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LCD)).BeginInit();
            this.SuspendLayout();
            // 
            // button_Spare
            // 
            this.button_Spare.Location = new System.Drawing.Point( 11, 50 );
            this.button_Spare.Name = "button_Spare";
            this.button_Spare.Size = new System.Drawing.Size( 75, 23 );
            this.button_Spare.TabIndex = 21;
            this.button_Spare.Text = "Spare";
            this.button_Spare.UseVisualStyleBackColor = true;
            this.button_Spare.Click += new System.EventHandler( this.button_Spare_Click );
            // 
            // button_Channel
            // 
            this.button_Channel.Location = new System.Drawing.Point( 12, 88 );
            this.button_Channel.Name = "button_Channel";
            this.button_Channel.Size = new System.Drawing.Size( 75, 23 );
            this.button_Channel.TabIndex = 20;
            this.button_Channel.Text = "Channel";
            this.button_Channel.UseVisualStyleBackColor = true;
            this.button_Channel.Click += new System.EventHandler( this.button_Channel_Click );
            // 
            // button_Backlight
            // 
            this.button_Backlight.Location = new System.Drawing.Point( 11, 12 );
            this.button_Backlight.Name = "button_Backlight";
            this.button_Backlight.Size = new System.Drawing.Size( 75, 23 );
            this.button_Backlight.TabIndex = 19;
            this.button_Backlight.Text = "Backlight";
            this.button_Backlight.UseVisualStyleBackColor = true;
            this.button_Backlight.Click += new System.EventHandler( this.button_Backlight_Click );
            // 
            // button_Down
            // 
            this.button_Down.Location = new System.Drawing.Point( 261, 88 );
            this.button_Down.Name = "button_Down";
            this.button_Down.Size = new System.Drawing.Size( 75, 23 );
            this.button_Down.TabIndex = 18;
            this.button_Down.Text = "Down";
            this.button_Down.UseVisualStyleBackColor = true;
            this.button_Down.Click += new System.EventHandler( this.button_Down_Click );
            // 
            // button_Enter
            // 
            this.button_Enter.Location = new System.Drawing.Point( 261, 50 );
            this.button_Enter.Name = "button_Enter";
            this.button_Enter.Size = new System.Drawing.Size( 75, 23 );
            this.button_Enter.TabIndex = 17;
            this.button_Enter.Text = "Enter";
            this.button_Enter.UseVisualStyleBackColor = true;
            this.button_Enter.Click += new System.EventHandler( this.button_Enter_Click );
            // 
            // button_Up
            // 
            this.button_Up.Location = new System.Drawing.Point( 261, 12 );
            this.button_Up.Name = "button_Up";
            this.button_Up.Size = new System.Drawing.Size( 75, 23 );
            this.button_Up.TabIndex = 16;
            this.button_Up.Text = "Up";
            this.button_Up.UseVisualStyleBackColor = true;
            this.button_Up.Click += new System.EventHandler( this.button_Up_Click );
            // 
            // pictureBox_LCD
            // 
            this.pictureBox_LCD.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox_LCD.Location = new System.Drawing.Point( 119, 12 );
            this.pictureBox_LCD.Name = "pictureBox_LCD";
            this.pictureBox_LCD.Size = new System.Drawing.Size( 120, 96 );
            this.pictureBox_LCD.TabIndex = 15;
            this.pictureBox_LCD.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            // 
            // DisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 350, 124 );
            this.Controls.Add( this.button_Spare );
            this.Controls.Add( this.button_Channel );
            this.Controls.Add( this.button_Backlight );
            this.Controls.Add( this.button_Down );
            this.Controls.Add( this.button_Enter );
            this.Controls.Add( this.button_Up );
            this.Controls.Add( this.pictureBox_LCD );
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size( 356, 156 );
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size( 356, 156 );
            this.Name = "DisplayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Watch UI";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LCD)).EndInit();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.Button button_Spare;
        private System.Windows.Forms.Button button_Channel;
        private System.Windows.Forms.Button button_Backlight;
        private System.Windows.Forms.Button button_Down;
        private System.Windows.Forms.Button button_Enter;
        private System.Windows.Forms.Button button_Up;
        private System.Windows.Forms.PictureBox pictureBox_LCD;
        private System.Windows.Forms.Timer timer1;
    }
}
namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    partial class EnvironmentForm
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
            this.comboBox_Engine = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Ok = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_Processor = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_Display = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_RAM = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_FLASH = new System.Windows.Forms.ComboBox();
            this.comboBox_Product = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox_Engine
            // 
            this.comboBox_Engine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Engine.FormattingEnabled = true;
            this.comboBox_Engine.Location = new System.Drawing.Point(76, 12);
            this.comboBox_Engine.Name = "comboBox_Engine";
            this.comboBox_Engine.Size = new System.Drawing.Size(698, 21);
            this.comboBox_Engine.TabIndex = 1;
            this.comboBox_Engine.SelectedIndexChanged += new System.EventHandler(this.comboBox_Engine_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Engine:";
            // 
            // button_Ok
            // 
            this.button_Ok.Enabled = false;
            this.button_Ok.Location = new System.Drawing.Point(12, 182);
            this.button_Ok.Name = "button_Ok";
            this.button_Ok.Size = new System.Drawing.Size(75, 23);
            this.button_Ok.TabIndex = 7;
            this.button_Ok.Text = "Ok";
            this.button_Ok.UseVisualStyleBackColor = true;
            this.button_Ok.Click += new System.EventHandler(this.button_Ok_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(699, 182);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Processor:";
            // 
            // comboBox_Processor
            // 
            this.comboBox_Processor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Processor.FormattingEnabled = true;
            this.comboBox_Processor.Location = new System.Drawing.Point(76, 66);
            this.comboBox_Processor.Name = "comboBox_Processor";
            this.comboBox_Processor.Size = new System.Drawing.Size(698, 21);
            this.comboBox_Processor.TabIndex = 3;
            this.comboBox_Processor.SelectedIndexChanged += new System.EventHandler(this.comboBox_Processor_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Display:";
            // 
            // comboBox_Display
            // 
            this.comboBox_Display.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Display.FormattingEnabled = true;
            this.comboBox_Display.Location = new System.Drawing.Point(76, 93);
            this.comboBox_Display.Name = "comboBox_Display";
            this.comboBox_Display.Size = new System.Drawing.Size(698, 21);
            this.comboBox_Display.TabIndex = 4;
            this.comboBox_Display.SelectedIndexChanged += new System.EventHandler(this.comboBox_Display_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "RAM:";
            // 
            // comboBox_RAM
            // 
            this.comboBox_RAM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_RAM.FormattingEnabled = true;
            this.comboBox_RAM.Location = new System.Drawing.Point(76, 120);
            this.comboBox_RAM.Name = "comboBox_RAM";
            this.comboBox_RAM.Size = new System.Drawing.Size(698, 21);
            this.comboBox_RAM.TabIndex = 5;
            this.comboBox_RAM.SelectedIndexChanged += new System.EventHandler(this.comboBox_RAM_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "FLASH:";
            // 
            // comboBox_FLASH
            // 
            this.comboBox_FLASH.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_FLASH.FormattingEnabled = true;
            this.comboBox_FLASH.Location = new System.Drawing.Point(76, 147);
            this.comboBox_FLASH.Name = "comboBox_FLASH";
            this.comboBox_FLASH.Size = new System.Drawing.Size(698, 21);
            this.comboBox_FLASH.TabIndex = 6;
            this.comboBox_FLASH.SelectedIndexChanged += new System.EventHandler(this.comboBox_FLASH_SelectedIndexChanged);
            // 
            // comboBox_Product
            // 
            this.comboBox_Product.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Product.FormattingEnabled = true;
            this.comboBox_Product.Location = new System.Drawing.Point(76, 39);
            this.comboBox_Product.Name = "comboBox_Product";
            this.comboBox_Product.Size = new System.Drawing.Size(698, 21);
            this.comboBox_Product.TabIndex = 2;
            this.comboBox_Product.SelectedIndexChanged += new System.EventHandler(this.comboBox_Product_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Product:";
            // 
            // EnvironmentForm
            // 
            this.AcceptButton = this.button_Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(791, 218);
            this.ControlBox = false;
            this.Controls.Add(this.comboBox_Product);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBox_FLASH);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox_RAM);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox_Display);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_Processor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_Ok);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_Engine);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "EnvironmentForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select Environment";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_Engine;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Ok;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_Processor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_Display;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_RAM;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_FLASH;
        private System.Windows.Forms.ComboBox comboBox_Product;
        private System.Windows.Forms.Label label6;
    }
}
namespace Microsoft.Zelig.Tools.IRViewer
{
    partial class MainForm
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gViewer1 = new Microsoft.Glee.GraphViewerGdi.GViewer();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.checkBoxExpandBasicBlocks = new System.Windows.Forms.CheckBox();
            this.listViewVariables = new System.Windows.Forms.ListView();
            this.columnHeaderVariableName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVariableType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewBasicBlock = new System.Windows.Forms.ListView();
            this.columnHeaderOperatorIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOperatorType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOperatorValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.richTextBoxCode = new System.Windows.Forms.RichTextBox();
            this.buttonLaunchCode = new System.Windows.Forms.Button();
            this.textBoxCodeFileName = new System.Windows.Forms.TextBox();
            this.listBoxMethods = new System.Windows.Forms.ListBox();
            this.checkBoxShowLineNumbers = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "IR XML Dump|*.xml";
            this.openFileDialog1.Title = "Open IR XML Dump";
            // 
            // gViewer1
            // 
            this.gViewer1.AsyncLayout = false;
            this.gViewer1.AutoScroll = true;
            this.gViewer1.BackwardEnabled = false;
            this.gViewer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gViewer1.EditObjects = false;
            this.gViewer1.ForwardEnabled = false;
            this.gViewer1.Graph = null;
            this.gViewer1.Location = new System.Drawing.Point(0, 0);
            this.gViewer1.MouseHitDistance = 0.05D;
            this.gViewer1.Name = "gViewer1";
            this.gViewer1.NavigationVisible = true;
            this.gViewer1.PanButtonPressed = false;
            this.gViewer1.SaveButtonVisible = true;
            this.gViewer1.Size = new System.Drawing.Size(301, 562);
            this.gViewer1.TabIndex = 0;
            this.gViewer1.ZoomF = 1D;
            this.gViewer1.ZoomFraction = 0.5D;
            this.gViewer1.ZoomWindowThreshold = 0.05D;
            this.gViewer1.SelectionChanged += new System.EventHandler(this.gViewer1_SelectionChanged);
            this.gViewer1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.gViewer1_MouseClick);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Location = new System.Drawing.Point(12, 10);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(856, 20);
            this.textBoxFilter.TabIndex = 1;
            this.textBoxFilter.Click += new System.EventHandler(this.textBoxFilter_Click);
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 39);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.checkBoxShowLineNumbers);
            this.splitContainer.Panel1.Controls.Add(this.gViewer1);
            this.splitContainer.Panel1.Controls.Add(this.checkBoxExpandBasicBlocks);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.listViewVariables);
            this.splitContainer.Panel2.Controls.Add(this.listViewBasicBlock);
            this.splitContainer.Panel2.Controls.Add(this.richTextBoxCode);
            this.splitContainer.Panel2.Controls.Add(this.buttonLaunchCode);
            this.splitContainer.Panel2.Controls.Add(this.textBoxCodeFileName);
            this.splitContainer.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
            this.splitContainer.Size = new System.Drawing.Size(856, 588);
            this.splitContainer.SplitterDistance = 301;
            this.splitContainer.TabIndex = 2;
            // 
            // checkBoxExpandBasicBlocks
            // 
            this.checkBoxExpandBasicBlocks.AutoSize = true;
            this.checkBoxExpandBasicBlocks.Location = new System.Drawing.Point(3, 568);
            this.checkBoxExpandBasicBlocks.Name = "checkBoxExpandBasicBlocks";
            this.checkBoxExpandBasicBlocks.Size = new System.Drawing.Size(126, 17);
            this.checkBoxExpandBasicBlocks.TabIndex = 3;
            this.checkBoxExpandBasicBlocks.Text = "Expand Basic Blocks";
            this.checkBoxExpandBasicBlocks.UseVisualStyleBackColor = true;
            this.checkBoxExpandBasicBlocks.CheckedChanged += new System.EventHandler(this.checkBoxExpandBasicBlocks_CheckedChanged);
            // 
            // listViewVariables
            // 
            this.listViewVariables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderVariableName,
            this.columnHeaderVariableType});
            this.listViewVariables.Dock = System.Windows.Forms.DockStyle.Top;
            this.listViewVariables.FullRowSelect = true;
            this.listViewVariables.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewVariables.HideSelection = false;
            this.listViewVariables.Location = new System.Drawing.Point(0, 0);
            this.listViewVariables.Name = "listViewVariables";
            this.listViewVariables.Size = new System.Drawing.Size(551, 150);
            this.listViewVariables.TabIndex = 0;
            this.listViewVariables.UseCompatibleStateImageBehavior = false;
            this.listViewVariables.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderVariableName
            // 
            this.columnHeaderVariableName.Text = "Name";
            this.columnHeaderVariableName.Width = 200;
            // 
            // columnHeaderVariableType
            // 
            this.columnHeaderVariableType.Text = "Type";
            this.columnHeaderVariableType.Width = 340;
            // 
            // listViewBasicBlock
            // 
            this.listViewBasicBlock.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderOperatorIndex,
            this.columnHeaderOperatorType,
            this.columnHeaderOperatorValue});
            this.listViewBasicBlock.FullRowSelect = true;
            this.listViewBasicBlock.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewBasicBlock.HideSelection = false;
            this.listViewBasicBlock.Location = new System.Drawing.Point(0, 153);
            this.listViewBasicBlock.MultiSelect = false;
            this.listViewBasicBlock.Name = "listViewBasicBlock";
            this.listViewBasicBlock.ShowGroups = false;
            this.listViewBasicBlock.Size = new System.Drawing.Size(551, 200);
            this.listViewBasicBlock.TabIndex = 2;
            this.listViewBasicBlock.UseCompatibleStateImageBehavior = false;
            this.listViewBasicBlock.View = System.Windows.Forms.View.Details;
            this.listViewBasicBlock.Click += new System.EventHandler(this.listViewBasicBlock_Click);
            // 
            // columnHeaderOperatorIndex
            // 
            this.columnHeaderOperatorIndex.Text = "#";
            this.columnHeaderOperatorIndex.Width = 20;
            // 
            // columnHeaderOperatorType
            // 
            this.columnHeaderOperatorType.Text = "Operator";
            this.columnHeaderOperatorType.Width = 150;
            // 
            // columnHeaderOperatorValue
            // 
            this.columnHeaderOperatorValue.Text = "Value";
            this.columnHeaderOperatorValue.Width = 200;
            // 
            // richTextBoxCode
            // 
            this.richTextBoxCode.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxCode.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBoxCode.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxCode.Location = new System.Drawing.Point(0, 379);
            this.richTextBoxCode.Name = "richTextBoxCode";
            this.richTextBoxCode.ReadOnly = true;
            this.richTextBoxCode.Size = new System.Drawing.Size(551, 209);
            this.richTextBoxCode.TabIndex = 1;
            this.richTextBoxCode.Text = "";
            this.richTextBoxCode.WordWrap = false;
            // 
            // buttonLaunchCode
            // 
            this.buttonLaunchCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLaunchCode.Enabled = false;
            this.buttonLaunchCode.Location = new System.Drawing.Point(517, 356);
            this.buttonLaunchCode.Name = "buttonLaunchCode";
            this.buttonLaunchCode.Size = new System.Drawing.Size(34, 20);
            this.buttonLaunchCode.TabIndex = 1;
            this.buttonLaunchCode.Text = ">>";
            this.buttonLaunchCode.UseVisualStyleBackColor = true;
            this.buttonLaunchCode.Click += new System.EventHandler(this.buttonLaunchCode_Click);
            // 
            // textBoxCodeFileName
            // 
            this.textBoxCodeFileName.Location = new System.Drawing.Point(0, 356);
            this.textBoxCodeFileName.Name = "textBoxCodeFileName";
            this.textBoxCodeFileName.ReadOnly = true;
            this.textBoxCodeFileName.Size = new System.Drawing.Size(514, 20);
            this.textBoxCodeFileName.TabIndex = 0;
            // 
            // listBoxMethods
            // 
            this.listBoxMethods.FormattingEnabled = true;
            this.listBoxMethods.Location = new System.Drawing.Point(12, 36);
            this.listBoxMethods.Name = "listBoxMethods";
            this.listBoxMethods.Size = new System.Drawing.Size(858, 212);
            this.listBoxMethods.TabIndex = 0;
            this.listBoxMethods.Click += new System.EventHandler(this.listBoxMethods_Click);
            // 
            // checkBoxShowLineNumbers
            // 
            this.checkBoxShowLineNumbers.AutoSize = true;
            this.checkBoxShowLineNumbers.Checked = true;
            this.checkBoxShowLineNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowLineNumbers.Location = new System.Drawing.Point(136, 568);
            this.checkBoxShowLineNumbers.Name = "checkBoxShowLineNumbers";
            this.checkBoxShowLineNumbers.Size = new System.Drawing.Size(121, 17);
            this.checkBoxShowLineNumbers.TabIndex = 4;
            this.checkBoxShowLineNumbers.Text = "Show Line Numbers";
            this.checkBoxShowLineNumbers.UseVisualStyleBackColor = true;
            this.checkBoxShowLineNumbers.CheckedChanged += new System.EventHandler(this.checkBoxShowLineNumbers_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 639);
            this.Controls.Add(this.listBoxMethods);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.textBoxFilter);
            this.MinimumSize = new System.Drawing.Size(896, 463);
            this.Name = "MainForm";
            this.Text = "IR Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Microsoft.Glee.GraphViewerGdi.GViewer gViewer1;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewVariables;
        private System.Windows.Forms.ColumnHeader columnHeaderVariableName;
        private System.Windows.Forms.ColumnHeader columnHeaderVariableType;
        private System.Windows.Forms.CheckBox checkBoxExpandBasicBlocks;
        private System.Windows.Forms.RichTextBox richTextBoxCode;
        private System.Windows.Forms.TextBox textBoxCodeFileName;
        private System.Windows.Forms.Button buttonLaunchCode;
        private System.Windows.Forms.ListView listViewBasicBlock;
        private System.Windows.Forms.ColumnHeader columnHeaderOperatorIndex;
        private System.Windows.Forms.ColumnHeader columnHeaderOperatorValue;
        private System.Windows.Forms.ColumnHeader columnHeaderOperatorType;
        private System.Windows.Forms.ListBox listBoxMethods;
        private System.Windows.Forms.CheckBox checkBoxShowLineNumbers;
    }
}


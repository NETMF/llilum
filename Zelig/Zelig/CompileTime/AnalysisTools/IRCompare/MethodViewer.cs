//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Tools.IRCompare
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    using IR  = Microsoft.Zelig.CodeGeneration.IR;
    using TS  = Microsoft.Zelig.Runtime.TypeSystem;

////using ARM = Microsoft.Zelig.Emulation.ArmProcessor;

    public partial class MethodViewer : Form
    {
        //
        // State
        //

        Dictionary< string, TS.MethodRepresentation > m_lookupMethod;
        TS.MethodRepresentation                       m_method;
        RenderMethod                                  m_renderer;
        List< RenderMethod.OutputLine >               m_text;
        string                                        m_irName;

        bool                                          m_fExpandBasicBlocks;
                                    
        IR.BasicBlock                                 m_selectedBasicBlock;

        //
        // Constructor Methods
        //

        public MethodViewer( Dictionary< string, TS.MethodRepresentation > lookupMethod ,
                             TS.MethodRepresentation                       method       ,
                             RenderMethod                                  renderer     ,
                             string                                        irName       )
        {
            m_lookupMethod       = lookupMethod;
            m_method             = method;
            m_renderer           = renderer;
            m_irName             = irName;

            m_fExpandBasicBlocks = false;

            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            textBox1.Text = m_method.Name;

            Text = "MethodViewer - " + m_method.Name + " - IR: " + m_irName;

            //--//

            ListVariables();

            m_text = m_renderer.DumpMethod( m_method );

            string[] lines = new string[m_text.Count];

            for(int i = 0; i < m_text.Count; i++)
            {
                lines[i] = m_text[i].Text;
            }

            richTextBox1.Lines = lines;

            SelectBasicBlock( null );
        }

        //--//

        private void ListVariables()
        {
            ListView.ListViewItemCollection col = listViewVariables.Items;

            listViewVariables.SuspendLayout();

            col.Clear();

            IR.ControlFlowGraphStateForCodeTransformation cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( m_method );
            if(cfg != null)
            {
                foreach(IR.VariableExpression var in cfg.DataFlow_SpanningTree_Variables)
                {
                    string name;

                    if(var.DebugName != null)
                    {
                        name = var.DebugName.Name;
                    }
                    else
                    {
                        name = var.ToString();
                    }

                    ListViewItem lvi = new ListViewItem( name );

                    lvi.SubItems.Add( var.Type.ToString() );

                    col.Add( lvi );
                }
            }

            listViewVariables.ResumeLayout();

            tabControl1.SelectedTab = tabPageVariables;
        }

        //--//--//

        private void SelectBasicBlock( IR.BasicBlock bb )
        {
            ListBox.ObjectCollection col = listBoxBasicBlock.Items;

            listBoxBasicBlock.SuspendLayout();

            col.Clear();

            m_selectedBasicBlock = bb;
            if(bb != null)
            {
                foreach(IR.Operator op in bb.Operators)
                {
                    col.Add( op );
                }
            }

            listBoxBasicBlock.ResumeLayout();

            tabControl1.SelectedTab = tabPageBasicBlock;
        }

        //--//--//

        private void MethodViewer_Load( object    sender ,
                                        EventArgs e      )
        {
            UpdateDisplay();
        }

        private void listBoxBasicBlock_DoubleClick( object    sender ,
                                                    EventArgs e      )
        {
            if(m_selectedBasicBlock != null)
            {
                int i = listBoxBasicBlock.SelectedIndex;

                if(i >= 0 && i < m_selectedBasicBlock.Operators.Length)
                {
                    IR.Operator op = m_selectedBasicBlock.Operators[i];

                    if(op is IR.CallOperator)
                    {
                        m_method = ((IR.CallOperator)op).TargetMethod;

                        UpdateDisplay();
                    }
                }
            }
        }

        private void checkBoxExpandBasicBlocks_CheckedChanged( object    sender ,
                                                               EventArgs e      )
        {
            m_fExpandBasicBlocks = checkBoxExpandBasicBlocks.Checked;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Zelig.Tools.IRViewer
{
    public partial class MainForm : Form
    {
        //
        // State
        //
        Dictionary<string, Method> m_lookupMethod;
        Method m_method;
        ListViewItem m_selectedOpLVI;
        string m_phaseName;

        bool m_fExpandBasicBlocks;

        object m_highlightedObjectAttr;
        object m_highlightedObject;

        Glee.Drawing.NodeAttr m_selectedNodeAttr;
        Glee.Drawing.Node m_selectedNode;

        BasicBlock m_selectedBasicBlock;

        SourceCodeTracker m_sourceCodeTracker;

        //
        // Constructor Methods
        //

        public MainForm()
        {
            m_sourceCodeTracker = new SourceCodeTracker();

            m_fExpandBasicBlocks = false;

            InitializeComponent();

            UpdateListBox();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            bool loadSucceeded = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

                    doc.Load(openFileDialog1.FileName);

                    var phaseName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);

                    int idx = phaseName.LastIndexOf('.');

                    if (idx >= 0)
                    {
                        phaseName = phaseName.Substring(idx + 1, phaseName.Length - idx - 1);
                    }

                    this.Text = "IRViewer - " + phaseName;

                    foreach (System.Xml.XmlNode methods in doc.SelectNodes("Methods"))
                    {
                        Parser parser = new Parser(methods);

                        m_lookupMethod = parser.Methods;
                        m_phaseName = phaseName;

                        UpdateDisplay();
                        UpdateListBox();

                        textBoxFilter.Select();

                        loadSucceeded = true;

                        break;
                    }
                }
                catch
                {
                }
            }

            if (!loadSucceeded)
            {
                Close();
            }
        }

        private void UpdateDisplay()
        {
            Text = $"IRViewer - { m_method?.Name } - { m_phaseName }";

            //--//

            if (m_method != null)
            {
                CreateGraph();

                ListVariables();
            }

            SelectBasicBlock(null);
        }

        //--//

        private void CreateGraph()
        {
            Microsoft.Glee.Drawing.Graph g = new Microsoft.Glee.Drawing.Graph(string.Format("Method for {0}", m_method.Name));

            g.GraphAttr.NodeAttr.Padding = 3;

            foreach (BasicBlockEdge edge in m_method.BasicBlockEdges)
            {
                Microsoft.Glee.Drawing.Edge edgeG = g.AddEdge(edge.From.Id, edge.To.Id) as Microsoft.Glee.Drawing.Edge;
                Microsoft.Glee.Drawing.EdgeAttr attr = edgeG.Attr;

                attr.Label = edge.Kind;
                attr.Fontsize -= 4;
            }

            foreach (BasicBlock bb in m_method.BasicBlocks)
            {
                Microsoft.Glee.Drawing.Node node = CreateNode(g, bb, m_fExpandBasicBlocks);
            }

            gViewer1.Graph = g;
        }

        private static Microsoft.Glee.Drawing.Node CreateNode(Microsoft.Glee.Drawing.Graph graph,
                                                               BasicBlock bb,
                                                               bool fExpandBasicBlocks)
        {
            Microsoft.Glee.Drawing.Node node = graph.AddNode(bb.Id) as Microsoft.Glee.Drawing.Node;
            Microsoft.Glee.Drawing.NodeAttr attr = node.Attr;

            if (bb.Type == "EntryBasicBlock")
            {
                attr.Shape = Microsoft.Glee.Drawing.Shape.Box;
                attr.Fillcolor = Microsoft.Glee.Drawing.Color.Green;
            }
            else if (bb.Type == "ExitBasicBlock")
            {
                attr.Shape = Microsoft.Glee.Drawing.Shape.Box;
                attr.Fillcolor = Microsoft.Glee.Drawing.Color.Red;
            }
            else if (bb.Type == "ExceptionHandlerBasicBlock")
            {
                attr.Shape = Microsoft.Glee.Drawing.Shape.Box;
                attr.Fillcolor = Microsoft.Glee.Drawing.Color.Purple;
            }
            else
            {
                attr.Shape = Microsoft.Glee.Drawing.Shape.Box;
            }

            if (fExpandBasicBlocks)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}:", bb.Id);
                sb.AppendLine();

                foreach (Operator op in bb.Operators)
                {
                    sb.AppendLine(op.Value);
                }

                node.Attr.Label = sb.ToString();
            }
            else
            {
                attr.Label = bb.Id + " - " + bb.Index;
            }

            return node;
        }

        //--//

        private void ListVariables()
        {
            ListView.ListViewItemCollection col = listViewVariables.Items;

            listViewVariables.SuspendLayout();

            col.Clear();

            foreach (Variable var in m_method.Variables)
            {
                ListViewItem lvi = new ListViewItem(var.Name);

                lvi.SubItems.Add(var.Type);

                col.Add(lvi);
            }

            listViewVariables.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            listViewVariables.ResumeLayout();
        }

        //--//--//

        private void SelectBasicBlock(BasicBlock bb)
        {
            ListView.ListViewItemCollection col = listViewBasicBlock.Items;

            listViewBasicBlock.SuspendLayout();

            col.Clear();

            m_selectedOpLVI = null;
            m_selectedBasicBlock = bb;
            if (bb != null)
            {
                foreach (Operator op in bb.Operators)
                {
                    ListViewItem lvi = new ListViewItem(op.Index.ToString());
                    lvi.SubItems.Add(op.Type);
                    lvi.SubItems.Add(op.Value);
                    col.Add(lvi);
                }
            }

            if (listViewBasicBlock.Items.Count > 0)
            {
                listViewBasicBlock.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }

            listViewBasicBlock.ResumeLayout();

            if (listViewBasicBlock.Items.Count > 0)
            {
                listViewBasicBlock.Items[0].Selected = true;
                listViewBasicBlock.Select();
            }

            UpdateCodePane();
        }

        //--//--//

        private void gViewer1_SelectionChanged(object sender,
                                                EventArgs e)
        {
            if (m_highlightedObject != null)
            {
                if (m_highlightedObject is Microsoft.Glee.Drawing.Edge)
                {
                    Microsoft.Glee.Drawing.Edge edge = (Microsoft.Glee.Drawing.Edge)m_highlightedObject;

                    edge.Attr = m_highlightedObjectAttr as Microsoft.Glee.Drawing.EdgeAttr;
                }
                else if (m_highlightedObject is Microsoft.Glee.Drawing.Node)
                {
                    Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)m_highlightedObject;

                    node.Attr = m_highlightedObjectAttr as Microsoft.Glee.Drawing.NodeAttr;
                }

                m_highlightedObject = null;
                m_highlightedObjectAttr = null;
            }

            if (gViewer1.SelectedObject != null)
            {
                m_highlightedObject = gViewer1.SelectedObject;

                if (m_highlightedObject is Microsoft.Glee.Drawing.Edge)
                {
                    Microsoft.Glee.Drawing.Edge edge = (Microsoft.Glee.Drawing.Edge)m_highlightedObject;

                    m_highlightedObjectAttr = edge.Attr.Clone();

                    edge.Attr.Color = Microsoft.Glee.Drawing.Color.Magenta;
                    edge.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Magenta;
                }
                else if (m_highlightedObject is Microsoft.Glee.Drawing.Node)
                {
                    Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)m_highlightedObject;

                    m_highlightedObjectAttr = node.Attr.Clone();

                    node.Attr.Color = Microsoft.Glee.Drawing.Color.Magenta;
                    node.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Magenta;
                }
            }

            gViewer1.Invalidate();
        }

        private void gViewer1_MouseClick(object sender,
                                          MouseEventArgs e)
        {
            // Do nothing if we didn't click on a node or click on the same node 
            if (m_highlightedObject is Microsoft.Glee.Drawing.Node && m_highlightedObject != m_selectedNode)
            {
                if (m_selectedNode != null)
                {
                    m_selectedNode.Attr = m_selectedNodeAttr;
                    m_selectedNode = null;
                    m_selectedNodeAttr = null;
                }

                Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)m_highlightedObject;

                foreach (BasicBlock bb in m_method.BasicBlocks)
                {
                    if (bb.Id == node.Id)
                    {
                        SelectBasicBlock(bb);
                        m_selectedNode = node;

                        // Save the original / untempered style so when we can go back to it when the node is unselected
                        m_selectedNodeAttr = ((Microsoft.Glee.Drawing.NodeAttr)m_highlightedObjectAttr).Clone();

                        // Apply the selected style to both the node and the saved styled from highlight so when
                        // mouse moves away, the node remain selected
                        ((Microsoft.Glee.Drawing.NodeAttr)m_highlightedObjectAttr).Fillcolor = Microsoft.Glee.Drawing.Color.Goldenrod;
                        node.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.Goldenrod;

                        break;
                    }
                }

                gViewer1.Invalidate();
            }
        }

        private void listViewBasicBlock_DoubleClick(object sender,
                                                    EventArgs e)
        {
            if (m_selectedBasicBlock != null && listViewBasicBlock.SelectedIndices.Count > 0)
            {
                int i = listViewBasicBlock.SelectedIndices[0];

                if (i >= 0 && i < m_selectedBasicBlock.Operators.Count)
                {
                    Operator op = m_selectedBasicBlock.Operators[i];

                    if (op.Call != null)
                    {
                        Method method;

                        if (m_lookupMethod.TryGetValue(op.Call, out method))
                        {
                            m_method = method;

                            UpdateDisplay();
                        }
                    }
                }
            }
        }

        private void checkBoxExpandBasicBlocks_CheckedChanged(object sender,
                                                               EventArgs e)
        {
            m_fExpandBasicBlocks = checkBoxExpandBasicBlocks.Checked;

            SelectBasicBlock(null);
            CreateGraph();
        }

        private void listViewBasicBlock_Click(object sender, EventArgs e)
        {
            UpdateCodePane();
        }

        private void UpdateCodePane(bool force = false)
        {
            if (listViewBasicBlock.SelectedIndices.Count == 0)
            {
                ClearCodePane();
            }
            else
            {
                // Remove selection highlight on the list view item
                if (m_selectedOpLVI != null)
                {
                    m_selectedOpLVI.BackColor = listViewBasicBlock.BackColor;
                    m_selectedOpLVI.ForeColor = listViewBasicBlock.ForeColor;
                }

                var selectedOpLVI = listViewBasicBlock.SelectedItems[0];
                var op = m_selectedBasicBlock.Operators[selectedOpLVI.Index];
                var sourceCode = m_sourceCodeTracker.GetSourceCode(op?.Debug?.File);

                if (sourceCode != null)
                {
                    bool showLineNumbers = checkBoxShowLineNumbers.Checked;
                    int lineNumberPadding = showLineNumbers ? sourceCode.Count.ToString().Length + 2 : 0; // +2 for ": "
                    var previousOp = (m_selectedOpLVI != null) ? m_selectedBasicBlock.Operators[m_selectedOpLVI.Index] : null;
                    if (force || sourceCode.File != previousOp?.Debug?.File)
                    {
                        richTextBoxCode.Clear();
                        // Add the lines to the rich text box
                        if (showLineNumbers)
                        {
                            for (int i = 1; i <= sourceCode.Count; i++)
                            {
                                string lineNumberText = i.ToString();
                                richTextBoxCode.AppendText($"{new string(' ', lineNumberPadding - 2 - lineNumberText.Length)}{lineNumberText}: {sourceCode[i]}\r\n");
                            }
                        }
                        else
                        {
                            for (int i = 1; i <= sourceCode.Count; i++)
                            {
                                richTextBoxCode.AppendText($"{sourceCode[i]}\r\n");
                            }
                        }

                        textBoxCodeFileName.Text = op.Debug.File;
                        buttonLaunchCode.Enabled = true;
                    }
                    else if (previousOp != op)
                    {
                        // Remove previous highlighting
                        richTextBoxCode.SelectionStart = richTextBoxCode.GetFirstCharIndexFromLine(previousOp.Debug.BeginLine - 1);
                        richTextBoxCode.SelectionLength = 
                                richTextBoxCode.GetFirstCharIndexFromLine(previousOp.Debug.EndLine - 1) + 
                                richTextBoxCode.Lines[previousOp.Debug.EndLine - 1].Length - richTextBoxCode.SelectionStart;
                        richTextBoxCode.SelectionBackColor = richTextBoxCode.BackColor;
                    }

                    if (force || previousOp != op)
                    {
                        // Highlight the relevant lines
                        int beginLineCharIndex = richTextBoxCode.GetFirstCharIndexFromLine(op.Debug.BeginLine - 1);
                        int endLineCharIndex = richTextBoxCode.GetFirstCharIndexFromLine(op.Debug.EndLine - 1);
                        richTextBoxCode.SelectionStart = beginLineCharIndex;
                        richTextBoxCode.SelectionLength =
                            endLineCharIndex + richTextBoxCode.Lines[op.Debug.EndLine - 1].Length - richTextBoxCode.SelectionStart;
                        richTextBoxCode.SelectionBackColor = Color.PaleGoldenrod;

                        // Highlight the relevant columns
                        richTextBoxCode.SelectionStart = beginLineCharIndex + lineNumberPadding + op.Debug.BeginColumn - 1;
                        richTextBoxCode.SelectionLength = endLineCharIndex + lineNumberPadding + op.Debug.EndColumn - richTextBoxCode.SelectionStart;
                        richTextBoxCode.SelectionBackColor = Color.Goldenrod;
                    }

                    // Scroll to a few lines before the start
                    int scrollLineTarget = op.Debug.BeginLine - 4;
                    if (scrollLineTarget < 0)
                    {
                        scrollLineTarget = 0;
                    }
                    richTextBoxCode.SelectionStart = richTextBoxCode.GetFirstCharIndexFromLine(scrollLineTarget);
                    richTextBoxCode.SelectionLength = 0;
                    richTextBoxCode.ScrollToCaret();
                }
                else
                {
                    textBoxCodeFileName.Clear();
                    richTextBoxCode.Clear();
                    richTextBoxCode.AppendText("//\r\n// No corresponding source code\r\n//\r\n");
                    buttonLaunchCode.Enabled = false;
                }

                // Override the selected LVI with the highlight color so it remain "highlighted" when losing focus.
                selectedOpLVI.BackColor = SystemColors.Highlight;
                selectedOpLVI.ForeColor = SystemColors.HighlightText;
                m_selectedOpLVI = selectedOpLVI;
            }
        }

        private void ClearCodePane()
        {
            richTextBoxCode.Clear();
            textBoxCodeFileName.Clear();
            buttonLaunchCode.Enabled = false;
            m_selectedOpLVI = null;
        }

        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            const int margin = 3;
            listViewBasicBlock.Height = splitContainer.Panel2.Height - listViewVariables.Height - richTextBoxCode.Height - buttonLaunchCode.Height - margin * 3;
            listViewBasicBlock.Width = splitContainer.Panel2.Width;

            textBoxCodeFileName.Top = listViewBasicBlock.Bottom + margin;
            textBoxCodeFileName.Width = splitContainer.Panel2.Width - buttonLaunchCode.Width - margin;

            buttonLaunchCode.Top = listViewBasicBlock.Bottom + margin;
            buttonLaunchCode.Left = textBoxCodeFileName.Right + margin;

            gViewer1.Height = splitContainer.Panel1.Height - checkBoxExpandBasicBlocks.Height - margin;
            checkBoxExpandBasicBlocks.Top = gViewer1.Bottom + margin;
        }

        private void buttonLaunchCode_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(textBoxCodeFileName.Text);
            }
            catch
            {
                // ignored
            }
        }

        private void textBoxFilter_TextChanged(object sender,
                                        EventArgs e)
        {
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            if (m_lookupMethod != null)
            {
                string filter = textBoxFilter.Text.ToLower();

                List<string> lst = new List<string>();

                foreach (string id in m_lookupMethod.Keys)
                {
                    if (string.IsNullOrEmpty(filter) || id.ToLower().Contains(filter))
                    {
                        lst.Add(id);
                    }
                }

                lst.Sort();

                ListBox.ObjectCollection col = listBoxMethods.Items;

                listBoxMethods.SuspendLayout();

                col.Clear();

                col.AddRange(lst.ToArray());

                listBoxMethods.ResumeLayout();

                listBoxMethods.Visible = listBoxMethods.Items.Count > 0;
            }
        }

        private void listBoxMethods_Click(object sender, EventArgs e)
        {
            if (m_lookupMethod != null)
            {
                if (listBoxMethods.SelectedItem != null)
                {
                    m_method = m_lookupMethod[(string)listBoxMethods.SelectedItem];

                    UpdateDisplay();

                    textBoxFilter.Text = listBoxMethods.SelectedItem as String;
                    listBoxMethods.Visible = false;
                }
            }
        }

        private void textBoxFilter_Click(object sender, EventArgs e)
        {
            textBoxFilter.SelectAll();
            listBoxMethods.Visible = listBoxMethods.Items.Count > 0;

            if (listBoxMethods.Visible)
            {
                listBoxMethods.SelectedIndex = -1;
            }
        }

        private void checkBoxShowLineNumbers_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCodePane(/*force*/true);
        }
    }
}
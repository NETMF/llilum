using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Zelig.Tools.IRViewer
{
    public partial class MethodViewer : Form
    {
        //
        // State
        //

        Dictionary< string, Method > m_lookupMethod;
        Method                       m_method;
        string                       m_phaseName;

        bool                         m_fExpandBasicBlocks;

        object                       m_selectedObjectAttr;
        object                       m_selectedObject;

        BasicBlock                   m_selectedBasicBlock;

        //
        // Constructor Methods
        //

        public MethodViewer( Dictionary< string, Method > lookupMethod ,
                             Method                       method       ,
                             string                       phaseName )
        {
            m_lookupMethod       = lookupMethod;
            m_method             = method;
            m_phaseName          = phaseName;

            m_fExpandBasicBlocks = false;

            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            textBox1.Text = m_method.Name;

            Text = "MethodViewer - " + m_method.Name + " - Phase: " + m_phaseName;

            //--//

            CreateGraph();

            ListVariables();

            SelectBasicBlock( null );
        }

        //--//

        private void CreateGraph()
        {
            Microsoft.Glee.Drawing.Graph g = new Microsoft.Glee.Drawing.Graph( string.Format( "Method for {0}", m_method.Name ) );

            g.GraphAttr.NodeAttr.Padding = 3;

            foreach(BasicBlockEdge edge in m_method.BasicBlockEdges)
            {
                Microsoft.Glee.Drawing.Edge     edgeG = g.AddEdge( edge.From.Id, edge.To.Id ) as Microsoft.Glee.Drawing.Edge;
                Microsoft.Glee.Drawing.EdgeAttr attr  = edgeG.Attr;

                attr.Label     = edge.Kind;
                attr.Fontsize -= 4;
                //attr.Styles = new Style[] { Microsoft.Glee.Drawing.Style.Dashed };
            }

            foreach(BasicBlock bb in m_method.BasicBlocks)
            {
                Microsoft.Glee.Drawing.Node node = CreateNode( g, bb, m_fExpandBasicBlocks );
            }

            gViewer1.Graph = g;
        }

        private static Microsoft.Glee.Drawing.Edge CreateEdge( Microsoft.Glee.Drawing.Graph graph ,
                                                               string                       from  ,
                                                               string                       to    ,
                                                               string                       label )
        {
            Microsoft.Glee.Drawing.Edge     edge = graph.AddEdge( from, to ) as Microsoft.Glee.Drawing.Edge;
            Microsoft.Glee.Drawing.EdgeAttr attr = edge.Attr;

            if(label != null)
            {
                attr.Label     = label;
                attr.Fontsize -= 4;
            }

            return edge;
        }

        private static Microsoft.Glee.Drawing.Node CreateNode( Microsoft.Glee.Drawing.Graph graph              ,
                                                               BasicBlock                   bb                 ,
                                                               bool                         fExpandBasicBlocks )
        {
            Microsoft.Glee.Drawing.Node     node = graph.AddNode( bb.Id ) as Microsoft.Glee.Drawing.Node;
            Microsoft.Glee.Drawing.NodeAttr attr = node.Attr;

            if(bb.Type == "EntryBasicBlock")
            {
                //attr.Shape     = Microsoft.Glee.Drawing.Shape.DoubleCircle;
                attr.Shape     = Microsoft.Glee.Drawing.Shape.Box;
                attr.Fillcolor = Microsoft.Glee.Drawing.Color.Green;
            }
            else if(bb.Type == "ExitBasicBlock")
            {
                //attr.Shape     = Microsoft.Glee.Drawing.Shape.DoubleCircle;
                attr.Shape     = Microsoft.Glee.Drawing.Shape.Box;
                attr.Fillcolor = Microsoft.Glee.Drawing.Color.Red;
            }
            else if(bb.Type == "ExceptionHandlerBasicBlock")
            {
                //attr.Shape     = Microsoft.Glee.Drawing.Shape.DoubleCircle;
                attr.Shape     = Microsoft.Glee.Drawing.Shape.Box;
                attr.Fillcolor = Microsoft.Glee.Drawing.Color.Purple;
            }
            else
            {
                attr.Shape   = Microsoft.Glee.Drawing.Shape.Box;
                //attr.Padding = 30.0;
            }

            if(fExpandBasicBlocks)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat( "{0}:", bb.Id );
                sb.AppendLine();

                foreach(Operator op in bb.Operators)
                {
                    sb.AppendLine( op.Value );
                }

                node.Attr.Label = sb.ToString();
            }
            else
            {
                attr.Label = bb.Id + " - " + bb.Index;
            }

            return node;
        }

////    private static void CreateSourceNode( Microsoft.Glee.Drawing.NodeAttr attr )
////    {
////        attr.Shape     = Microsoft.Glee.Drawing.Shape.Box;
////        attr.XRad      = 3;
////        attr.YRad      = 3;
////        attr.Fillcolor = Microsoft.Glee.Drawing.Color.Green;
////        attr.LineWidth = 10;
////    }
////
////    private void CreateTargetNode( Microsoft.Glee.Drawing.NodeAttr attr )
////    {
////        attr.Shape     = Microsoft.Glee.Drawing.Shape.DoubleCircle;
////        attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightGray;
////
////        attr.Fontsize += 4;
////    }

        //--//

        private void ListVariables()
        {
            ListView.ListViewItemCollection col = listViewVariables.Items;

            listViewVariables.SuspendLayout();

            col.Clear();

            foreach(Variable var in m_method.Variables)
            {
                ListViewItem lvi = new ListViewItem( var.Name );

                lvi.SubItems.Add( var.Type );

                col.Add( lvi );
            }

            listViewVariables.ResumeLayout();

            tabControl1.SelectedTab = tabPageVariables;
        }

        //--//--//

        private void SelectBasicBlock( BasicBlock bb )
        {
            ListBox.ObjectCollection col = listBoxBasicBlock.Items;

            listBoxBasicBlock.SuspendLayout();

            col.Clear();

            m_selectedBasicBlock = bb;
            if(bb != null)
            {
                foreach(Operator op in bb.Operators)
                {
                    col.Add( op.Value );
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

        private void gViewer1_SelectionChanged( object    sender ,
                                                EventArgs e      )
        {
            if(m_selectedObject != null)
            {
                if(m_selectedObject is Microsoft.Glee.Drawing.Edge)
                {
                    Microsoft.Glee.Drawing.Edge edge = (Microsoft.Glee.Drawing.Edge)m_selectedObject;

                    edge.Attr = m_selectedObjectAttr as Microsoft.Glee.Drawing.EdgeAttr;
                }
                else if(m_selectedObject is Microsoft.Glee.Drawing.Node)
                {
                    Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)m_selectedObject;

                    node.Attr = m_selectedObjectAttr as Microsoft.Glee.Drawing.NodeAttr;
                }

                m_selectedObject     = null;
                m_selectedObjectAttr = null;
            }

            if(gViewer1.SelectedObject == null)
            {
////            label1.Text = "No object under the mouse";
////            this.gViewer.SetToolTip( toolTip1, "" );
            }
            else
            {
                m_selectedObject = gViewer1.SelectedObject;

                if(m_selectedObject is Microsoft.Glee.Drawing.Edge)
                {
                    Microsoft.Glee.Drawing.Edge edge = (Microsoft.Glee.Drawing.Edge)m_selectedObject;

                    m_selectedObjectAttr = edge.Attr.Clone();

                    edge.Attr.Color     = Microsoft.Glee.Drawing.Color.Magenta;
                    edge.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Magenta;

////                //here you can use e.Attr.Id or e.UserData to get back to you data
////                this.gViewer.SetToolTip( this.toolTip1, String.Format( "edge from {0} {1}", edge.Source, edge.Target ) );
                }
                else if(m_selectedObject is Microsoft.Glee.Drawing.Node)
                {
                    Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)m_selectedObject;

                    m_selectedObjectAttr = node.Attr.Clone();

                    node.Attr.Color     = Microsoft.Glee.Drawing.Color.Magenta;
                    node.Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Magenta;

////                //here you can use e.Attr.Id to get back to your data
////                this.gViewer.SetToolTip( toolTip1, String.Format( "node {0}", (selectedObject as Node).Attr.Label ) );
                }
            }

            gViewer1.Invalidate();
        }

        private void gViewer1_MouseClick( object         sender ,
                                          MouseEventArgs e      )
        {
            if(gViewer1.SelectedObject is Microsoft.Glee.Drawing.Node)
            {
                Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)gViewer1.SelectedObject;

                foreach(BasicBlock bb in m_method.BasicBlocks)
                {
                    if(bb.Id == node.Id)
                    {
                        SelectBasicBlock( bb );
                        break;
                    }
                }
            }
        }

        private void listBoxBasicBlock_DoubleClick( object    sender ,
                                                    EventArgs e      )
        {
            if(m_selectedBasicBlock != null)
            {
                int i = listBoxBasicBlock.SelectedIndex;

                if(i >= 0 && i < m_selectedBasicBlock.Operators.Count)
                {
                    Operator op = m_selectedBasicBlock.Operators[i];

                    if(op.Call != null)
                    {
                        Method method;

                        if(m_lookupMethod.TryGetValue( op.Call, out method ))
                        {
                            m_method = method;

                            UpdateDisplay();
                        }
                    }
                }
            }
        }

        private void checkBoxExpandBasicBlocks_CheckedChanged( object    sender ,
                                                               EventArgs e      )
        {
            m_fExpandBasicBlocks = checkBoxExpandBasicBlocks.Checked;

            CreateGraph();
        }
    }
}
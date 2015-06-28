//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Tools.InequalityGraphVisualization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    using Microsoft.Zelig.CodeGeneration.IR;
    using Microsoft.Zelig.CodeGeneration.IR.Transformations;

    public partial class GraphForm : Form
    {
        //
        // State
        //

        ConstraintSystemCollector.GraphState m_gs;
        GrowOnlyHashTable< object, string >  m_ids = HashTableFactory.New< object, string >();

        object                               m_selectedObjectAttr;
        object                               m_selectedObject;

        //
        // Constructor Methods
        //

        public GraphForm( ConstraintSystemCollector.GraphState gs )
        {
            m_gs = gs;

            InitializeComponent();
        }

        //
        // Helper Methods
        //

        private void CreateGraph()
        {
            Microsoft.Glee.Drawing.Graph g = new Microsoft.Glee.Drawing.Graph( "None" );

            g.GraphAttr.NodeAttr.Padding = 3;

            foreach(object obj in m_gs.Vertices.Keys)
            {
                if(m_gs.WasReached( obj ))
                {
                    Microsoft.Glee.Drawing.Node node = CreateNode( g, obj );
                }
            }

            foreach(ConstraintSystemCollector.ConstraintInstance ci in m_gs.Constraints)
            {
                if(m_gs.WasReached( ci.Destination ) &&
                   m_gs.WasReached( ci.Source      )  )
                {
                    Microsoft.Glee.Drawing.Edge edgeG = CreateEdge( g, ci );
                }
            }

            gViewer1.Graph = g;

            if(m_gs.ProofTests != null)
            {
                hScrollBar1.Minimum = 0;
                hScrollBar1.Maximum = m_gs.ProofTests.Count - 1 + hScrollBar1.LargeChange;
            }
        }

        private Microsoft.Glee.Drawing.Edge CreateEdge( Microsoft.Glee.Drawing.Graph                 graph ,
                                                        ConstraintSystemCollector.ConstraintInstance ci    )
        {
            Microsoft.Glee.Drawing.Edge     edge = graph.AddEdge( GetId( ci.Source ), GetId( ci.Destination ) ) as Microsoft.Glee.Drawing.Edge;
            Microsoft.Glee.Drawing.EdgeAttr attr = edge.Attr;

            edge.UserData = ci;

            attr.ArrowHeadAtTarget = Microsoft.Glee.Drawing.ArrowStyle.Normal;
            attr.Label             = ci.Weight.ToString();
////        attr.Fontsize -= 4;

            return edge;
        }

        private Microsoft.Glee.Drawing.Node CreateNode( Microsoft.Glee.Drawing.Graph graph ,
                                                        object                       obj   )
        {
            Microsoft.Glee.Drawing.Node     node = graph.AddNode( GetId( obj ) ) as Microsoft.Glee.Drawing.Node;
            Microsoft.Glee.Drawing.NodeAttr attr = node.Attr;

            attr.Label = GetLabel( obj );

            Operator op = m_gs.Vertices[obj];

            if(op is PhiOperator)
            {
                attr.Shape = Microsoft.Glee.Drawing.Shape.Diamond;
            }
            else if(op is PiOperator)
            {
                attr.Shape = Microsoft.Glee.Drawing.Shape.DoubleCircle;
            }

            node.UserData = obj;

            return node;
        }

        private Microsoft.Glee.Drawing.Node GetNode( object obj )
        {
            string id = GetId( obj );

            return gViewer1.Graph.FindNode( id );
        }

        private void SetColor( object                       obj   ,
                               Microsoft.Glee.Drawing.Color color )
        {
            SetColorForNode( GetNode( obj ), color );
        }

        private void SetColorForNode( Microsoft.Glee.Drawing.Node  node  ,
                                      Microsoft.Glee.Drawing.Color color )
        {
            Microsoft.Glee.Drawing.NodeAttr nodeAttr = node.Attr;

            nodeAttr.Color     = color;
            nodeAttr.Fontcolor = color;
            nodeAttr.Fillcolor = Microsoft.Glee.Drawing.Color.White;
        }

        private string GetId( object obj )
        {
            string id;
            
            if(m_ids.TryGetValue( obj, out id ) == false)
            {
                id = string.Format( "Id{0}", m_ids.Count );

                m_ids[obj] = id;
            }

            return id;
        }

        private string GetLabel( object obj )
        {
            string res;

            if(m_gs.Labels.TryGetValue( obj, out res ) == false)
            {
                res = obj.ToString();
            }

            return res;
        }

        //--//

        private void ClearText()
        {
            richTextBox1.Clear();
        }

        private void AppendText(        string   fmt  ,
                                 params object[] args )
        {
            richTextBox1.AppendText( string.Format( fmt, args ) );
            richTextBox1.AppendText( "\n" );
        }

        //
        // Event Methods
        //

        private void GraphForm_Load( object    sender ,
                                     EventArgs e      )
        {
            CreateGraph();
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
            ClearText();

            richTextBox1.Clear();

            if(gViewer1.SelectedObject is Microsoft.Glee.Drawing.Edge)
            {
                Microsoft.Glee.Drawing.Edge edge = (Microsoft.Glee.Drawing.Edge)gViewer1.SelectedObject;

                ConstraintSystemCollector.ConstraintInstance ci = (ConstraintSystemCollector.ConstraintInstance)edge.UserData;

                AppendText( "Constraint: {0} - {1} <= {2}", GetLabel( ci.Destination ), GetLabel( ci.Source ), ci.Weight );
            }

            if(gViewer1.SelectedObject is Microsoft.Glee.Drawing.Node)
            {
                Microsoft.Glee.Drawing.Node node = (Microsoft.Glee.Drawing.Node)gViewer1.SelectedObject;

                object obj = node.UserData;

                if(obj is ConstraintSystemCollector.ArrayLengthHolder)
                {
                    ConstraintSystemCollector.ArrayLengthHolder hld = (ConstraintSystemCollector.ArrayLengthHolder)obj;

                    obj = hld.Array;
                }

                AppendText( "Symbol: {0} = {1}", GetLabel( obj ), obj );

                Operator op;

                if(m_gs.Vertices.TryGetValue( obj, out op ) && op != null)
                {
                    AppendText( "Definition: {0} is Op_{1} : {2}", GetLabel( obj ), op.SpanningTreeIndex, op.ToPrettyString() );

                    Operator opStart = op;
                    Operator opStop  = op;

                    for(int i = 0; i < 3; i++)
                    {
                        Operator op2 = opStart.GetPreviousOperator();

                        if(op2 == null)
                        {
                            break;
                        }

                        opStart = op2;
                    }

                    for(int i = 0; i < 3; i++)
                    {
                        Operator op2 = opStop.GetNextOperator();

                        if(op2 == null)
                        {
                            break;
                        }

                        opStop = op2;
                    }

                    Operator[] ops   = op.BasicBlock.Operators;
                    int        start = ArrayUtility.FindInNotNullArray( ops, opStart );
                    int        stop  = ArrayUtility.FindInNotNullArray( ops, opStop  );

                    AppendText( "----" );

                    while(start <= stop)
                    {
                        Operator op2 = ops[start++];

                        AppendText( "Op_{0}: {1}", op2.SpanningTreeIndex, op2.ToPrettyString() );
                    }
                }
            }
        }

        private void hScrollBar1_ValueChanged( object    sender ,
                                               EventArgs e      )
        {
            if(m_gs.ProofTests != null)
            {
                Stack< ConstraintSystemCollector.ConstraintInstance > stack = new Stack< ConstraintSystemCollector.ConstraintInstance >();
                System.Text.StringBuilder                             sb    = new StringBuilder();
                string                                                dir   = (m_gs.Flavor == ConstraintSystemCollector.Kind.LessThanOrEqual) ? "<=" : ">=";

                for(int i = 0; i < hScrollBar1.Value; i++)
                {
                    object val = m_gs.ProofTests[i];

                    if(val is ConstraintSystemCollector.ConstraintInstance)
                    {
                        var ci = val as ConstraintSystemCollector.ConstraintInstance;

                        stack.Push( ci );

                        SetColor( ci.Source     , Microsoft.Glee.Drawing.Color.Blue  );
                        SetColor( ci.Destination, Microsoft.Glee.Drawing.Color.Brown );

                        sb.AppendFormat( "Checking: ## {0} {1} ## - ## {2} {3} ## {4} {5}\n", GetLabel( ci.Destination ), ci.Destination, GetLabel( ci.Source ), ci.Source, m_gs.Flavor == ConstraintSystemCollector.Kind.LessThanOrEqual ? "<=" : ">=", ci.Weight );
                    }
                    else if(val is ConstraintSystemCollector.Lattice)
                    {
                        var res = (ConstraintSystemCollector.Lattice)val;

                        if(stack.Count > 0)
                        {
                            var ci = stack.Pop();

                            switch(res)
                            {
                                case ConstraintSystemCollector.Lattice.True:
                                    SetColor( ci.Destination, Microsoft.Glee.Drawing.Color.Green );
                                    break;

                                case ConstraintSystemCollector.Lattice.Reduced:
                                    SetColor( ci.Destination, Microsoft.Glee.Drawing.Color.Orange );
                                    break;

                                case ConstraintSystemCollector.Lattice.False:
                                    SetColor( ci.Destination, Microsoft.Glee.Drawing.Color.Red );
                                    break;
                            }

                            sb.AppendFormat( "Result: ## {0} {1} ## - ## {2} {3} ## {4} {5} ==> {6}\n", GetLabel( ci.Destination ), ci.Destination, GetLabel( ci.Source ), ci.Source, dir, ci.Weight, res );
                        }
                    }
                    else
                    {
                        foreach(Microsoft.Glee.Drawing.Node node in gViewer1.Graph.NodeMap.Values)
                        {
                            SetColorForNode( node, Microsoft.Glee.Drawing.Color.Black );
                        }

                        sb.Length = 0;
                    }
                }

                var ciArray = stack.ToArray();
                for(int i = ciArray.Length; --i >= 0;)
                {
                    var ci   = ciArray[i];
                    var node = GetNode( ci.Destination );

                    if(i == 0)
                    {
                        node.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightGreen;

                        textBox1.Text = string.Format( "Step: {0}, trying to prove {1} {2} {3} {4}", hScrollBar1.Value, GetLabel( ci.Destination ), GetLabel( ci.Source ), dir, ci.Weight );
                    }
                    else
                    {
                        node.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightGray;
                    }
                }

                richTextBox1.Clear();
                richTextBox1.AppendText( sb.ToString() );
                richTextBox1.ScrollToCaret();

                gViewer1.Refresh();
            }
        }
    }
}

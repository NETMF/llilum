//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;


    public partial class TreeBasedGridView : UserControl
    {
        public class NodeMouseEventArgs : System.Windows.Forms.MouseEventArgs
        {
            //
            // State
            //

            public readonly GridNode SelectedNode;
            public readonly int      SelectedColumn;

            //
            // Constructor Methods
            //

            internal NodeMouseEventArgs( GridNode     selectedNode   ,
                                         int          selectedColumn ,
                                         MouseButtons button         ,
                                         int          clicks         ,
                                         int          x              ,
                                         int          y              ,
                                         int          delta          ) : base( button, clicks, x, y, delta )
            {
                this.SelectedNode   = selectedNode;
                this.SelectedColumn = selectedColumn;
            }

            //
            // Access Methods
            //

        }

        public delegate void NodeMouseEventHandler( Object sender, NodeMouseEventArgs e );

        //--//

        public class GridColumnDefinition
        {
            //
            // State
            //

            public readonly string                       Name;
            public readonly DataGridViewContentAlignment Alignment;
            public readonly bool                         UseIcon;
            public readonly bool                         IsEditable;
            public readonly bool                         HighlightOnChange;

            //
            // Constructor Methods
            //

            public GridColumnDefinition( string                       name              ,
                                         DataGridViewContentAlignment alignment         ,
                                         bool                         useIcon           ,
                                         bool                         isEditable        ,
                                         bool                         highlightOnChange )
            {
                this.Name              = name;
                this.Alignment         = alignment;
                this.UseIcon           = useIcon;
                this.IsEditable        = isEditable;
                this.HighlightOnChange = highlightOnChange;
            }
        }

        public class GridNode
        {
            public delegate void ExpandDelegate( GridNode node                                  );
            public delegate bool UpdateDelegate( GridNode node, string proposedValue, int index );

            //
            // State
            //

            private readonly TreeBasedGridView          m_owner;
            private          DataGridViewRow            m_row;

            private          GridNode                   m_parent;
            private          LinkedListNode< GridNode > m_parentNode;
            private readonly LinkedList< GridNode >     m_childNodes = new LinkedList< GridNode >();
            private          int                        m_depth = -1;
            private          bool                       m_fPopulated;

            private          Icon                       m_icon;
            private readonly string[]                   m_values;
            private          object                     m_tag;
            private          bool                       m_fIsExpanded;
            private          bool                       m_fChanged;
            private          ExpandDelegate             m_expandCallback;
            private          UpdateDelegate             m_updateCallback;

            //
            // Constructor Methods
            //

            internal GridNode( TreeBasedGridView owner )
            {
                m_owner  = owner;
                m_values = new string[owner.m_columnDefinitions.Count];
            }

            internal GridNode( TreeBasedGridView          owner  ,
                               string[]                   values ,
                               GridNode                   parent ,
                               LinkedListNode< GridNode > anchor ,
                               bool                       fAfter ) : this( owner )
            {
                for(int i = 0; i < m_values.Length; i++)
                {
                    if(i <  values.Length)
                    {
                        m_values[i] = values[i];
                    }
                    else
                    {
                        m_values[i] = "";
                    }
                }

                //--//

                m_parent = parent;
                m_depth  = parent.Depth + 1;

                var nodes = parent.m_childNodes;

                if(anchor != null)
                {
                    if(fAfter)
                    {
                        m_parentNode = nodes.AddAfter( anchor, this );
                    }
                    else
                    {
                        m_parentNode = nodes.AddBefore( anchor, this );
                    }
                }
                else
                {
                    m_parentNode = nodes.AddLast( this );
                }

                this.IsVisible = parent.IsVisible && parent.IsExpanded;
            }

            //
            // Helper Methods
            //

            public void Select()
            {
                if(m_row != null)
                {
                    m_owner.dataGridView1.ClearSelection();
                    m_row.Selected = true;
                }
            }

            public void Remove()
            {
                if(m_parentNode != null)
                {
                    m_parent.m_childNodes.Remove( m_parentNode );

                    m_parent     = null;
                    m_parentNode = null;
                    m_depth      = -1;
                }

                this.IsVisible = false;
            }

            public void Clear()
            {
                m_owner.StartTreeUpdate();

                while(m_childNodes.First != null)
                {
                    m_childNodes.First.Value.Remove();
                }

                m_owner.EndTreeUpdate();
            }

            public void Invalidate()
            {
                Clear();

                m_fPopulated = false;
            }

            public GridNode AddChild( params string[] values )
            {
                return new GridNode( m_owner, values, this, null, false );
            }

            public GridNode AddBefore( params string[] values )
            {
                return new GridNode( m_owner, values, m_parent, m_parentNode, false );
            }

            public GridNode AddAfter( params string[] values )
            {
                return new GridNode( m_owner, values, m_parent, m_parentNode, true );
            }

            public bool Enumerate( EnumerateDelegate dlg )
            {
                foreach(var node in GetPreOrderEnumerator())
                {
                    if(dlg( node ) == false)
                    {
                        return false;
                    }
                }
                
                return true;
            }

            private IEnumerable< GridNode > GetPreOrderEnumerator()
            {
                yield return this;

                var child = m_childNodes.First;
                while(child != null)
                {
                    var nextChild = child.Next;

                    foreach(var subNode in child.Value.GetPreOrderEnumerator())
                    {
                        yield return subNode;
                    }

                    child = nextChild;
                }
            }

            internal void UpdateColor()
            {
                if(m_row != null)
                {
                    var cols    = m_owner.m_columnDefinitions;
                    int numCols = cols.Count;

                    for(int i = 0; i < numCols; i++)
                    {
                        if(cols[i].HighlightOnChange)
                        {
                            m_row.Cells[i].Style.ForeColor = m_fChanged ? Color.Red : m_row.Cells[0].Style.ForeColor;
                        }
                    }

                }
            }

            private void EnsurePopulated()
            {
                if(m_fPopulated == false)
                {
                    m_fPopulated = true;

                    if(m_expandCallback != null)
                    {
                        m_expandCallback( this );
                    }
                }
            }

            //
            // Access Methods
            //

            public ExpandDelegate ExpandCallback
            {
                get
                {
                    return m_expandCallback;
                }

                set
                {
                    m_expandCallback = value;
                }
            }

            public UpdateDelegate UpdateCallback
            {
                get
                {
                    return m_updateCallback;
                }

                set
                {
                    m_updateCallback = value;
                }
            }

            public TreeBasedGridView Owner
            {
                get
                {
                    return m_owner;
                }
            }

            public GridNode Parent
            {
                get
                {
                    return m_parent;
                }
            }

            public LinkedList< GridNode > ChildNodes
            {
                get
                {
                    EnsurePopulated();

                    return m_childNodes;
                }
            }

            public LinkedList< GridNode > ChildNodesNoPopulate
            {
                get
                {
                    return m_childNodes;
                }
            }

            public GridNode[] ArrayOfChildNodesNoPopulate
            {
                get
                {
                    var lst = this.ChildNodesNoPopulate;
                    var res = new GridNode[lst.Count];
                    int pos = 0;

                    foreach(var node in lst)
                    {
                        res[pos++] = node;
                    }

                    return res;
                }
            }

            public bool UseIcon
            {
                get
                {
                    return m_owner.m_columnDefinitions[0].UseIcon;
                }
            }

            public Icon Icon
            {
                get
                {
                    return m_icon;
                }

                set
                {
                    m_icon = value;
                }
            }

            public string this[int index]
            {
                get
                {
                    return m_values[index];
                }

                set
                {
                    if(m_values[index] != value)
                    {
                        m_values[index] = value;

                        if(m_row != null)
                        {
                            m_row.Cells[index].Value = value;
                        }
                    }
                }
            }

            public object Tag
            {
                get
                {
                    return m_tag;
                }

                set
                {
                    m_tag = value;
                }
            }

            public bool HasChanged
            {
                get
                {
                    return m_fChanged;
                }

                set
                {
                    if(m_fChanged != value)
                    {
                        m_fChanged = value;

                        UpdateColor();
                    }
                }
            }

            public int Depth
            {
                get
                {
                    return m_depth;
                }
            }

            public bool IsExpanded
            {
                get
                {
                    //
                    // The root is always expanded. 
                    //
                    if(this.IsRoot)
                    {
                        return true;
                    }

                    return m_fIsExpanded;
                }

                set
                {
                    if(m_fIsExpanded != value)
                    {
                        m_owner.StartTreeUpdate();

                        m_fIsExpanded = value;

                        foreach(var child in this.ChildNodes)
                        {
                            child.IsVisible = m_fIsExpanded;
                        }

                        m_owner.EndTreeUpdate();
                    }
                }
            }

            public bool IsRoot
            {
                get
                {
                    return m_owner.m_root == this;
                }
            }

            public bool IsVisible
            {
                get
                {
                    //
                    // Special case for the root: always visible, even if it doesn't have a UI element.
                    //
                    if(this.IsRoot)
                    {
                        return true;
                    }

                    return m_row != null;
                }

                internal set
                {
                    bool fIsVisible = this.IsVisible;

                    if(fIsVisible != value)
                    {
                        m_owner.StartTreeUpdate();

                        var rows = m_owner.dataGridView1.Rows;

                        if(value)
                        {
                            DataGridViewRow row = new DataGridViewRow();

                            m_row = row;

                            row.Height = m_owner.dataGridView1.RowTemplate.Height;
                            row.Tag    = this;

                            var cols    = m_owner.m_columnDefinitions;
                            int numCols = cols.Count;
                            var cells   = new DataGridViewCell[ numCols ];

                            for(int i = 0; i < numCols; i++)
                            {
                                var cell = i == 0 ? new GridNodeCell() : new DataGridViewTextBoxCell();

                                cells[i] = cell;

                                cell.Value = this[i];
                            }

                            row.Cells.AddRange( cells );

                            if(m_fChanged)
                            {
                                UpdateColor();
                            }

                            //--//

                            var prevNode = this.PreviousVisibleNode;
                            if(prevNode == null)
                            {
                                rows.Add( m_row );
                            }
                            else if(prevNode.IsRoot)
                            {
                                rows.Insert( 0, m_row );
                            }
                            else
                            {
                                rows.Insert( prevNode.m_row.Index + 1, m_row );
                            }

                            //--//

                            if(this.IsExpanded)
                            {
                                foreach(var child in this.ChildNodes)
                                {
                                    child.IsVisible = true;
                                }
                            }
                        }
                        else
                        {
                            foreach(var child in this.ChildNodesNoPopulate)
                            {
                                child.IsVisible = false;
                            }

                            rows.Remove( m_row );

                            m_row = null;
                        }

                        m_owner.EndTreeUpdate();
                    }
                }
            }

            private GridNode PreviousVisibleNode
            {
                get
                {
                    if(m_parentNode == null)
                    {
                        return null;
                    }

                    var node = m_parentNode.Previous;
                    if(node == null)
                    {
                        return m_parent;
                    }

                    while(true)
                    {
                        var lastChild = node.Value.LastVisibleChild;
                        if(lastChild == null)
                        {
                            return node.Value;
                        }

                        node = lastChild;
                    }
                }
            }

            private LinkedListNode< GridNode > LastVisibleChild
            {
                get
                {
                    var node = m_childNodes.Last;
                    while(node != null)
                    {
                        if(node.Value.IsVisible)
                        {
                            return node;
                        }

                        node = node.Previous;
                    }

                    return null;
                }
            }

            public bool HasNodes
            {
                get
                {
                    return this.ChildNodes.First != null;
                }
            }

            public bool IsUpdatable
            {
                get
                {
                    return (m_updateCallback != null);
                }
            }

            public bool IsLastInList
            {
                get
                {
                    // used for drawing tree lines by cell
                    return m_parentNode.Next == null;
                }
            }
        }

        class GridNodeCell : DataGridViewTextBoxCell
        {
            const int c_IndentationX     = 20;
            const int c_LineOffsetX      =  9;
            const int c_IconSizeX        = 16;
            const int c_IconSizeY        = 16;
            const int c_IconOffsetX      =  2;
            const int c_IconOffsetY      =  2;
            const int c_UserIconOffsetX  = 18;
            const int c_UserIconOffsetY  =  2;
            const int c_TreeIconMarginX  =  3;
            const int c_TreeIconMarginY  =  3;
            const int c_TreeIconSizeX    =  9;
            const int c_TreeIconSizeY    =  9;

            //
            // Constructor Methods
            //

            internal GridNodeCell()
            {
            }

            //
            // Helper Methods
            //

            protected override Rectangle GetContentBounds( Graphics              graphics  ,
                                                           DataGridViewCellStyle cellStyle ,
                                                           int                   rowIndex  )
            {
                // get the node reference at this row
                GridNode node = (GridNode)this.DataGridView.Rows[this.RowIndex].Tag;
                int      width = c_IndentationX + (node.UseIcon ? c_IconSizeX : 0);

                var res = base.GetContentBounds( graphics, cellStyle, rowIndex );

                // pad text to include space for tree lines and icons, plus indent to show tree depth
                res.Inflate( (node.Depth * c_IndentationX) + width,  0 );

                return res;
            }

            protected override Size GetPreferredSize( Graphics              graphics       ,
                                                      DataGridViewCellStyle cellStyle      ,
                                                      int                   rowIndex       ,
                                                      Size                  constraintSize )

            {
                // get the node reference at this row
                GridNode node = (GridNode)this.DataGridView.Rows[this.RowIndex].Tag;
                int      width = c_IndentationX + (node.UseIcon ? c_IconSizeX : 0);

                Size res = base.GetPreferredSize( graphics, cellStyle, rowIndex, constraintSize );

                // pad text to include space for tree lines and icons, plus indent to show tree depth
                return new Size( res.Width + (node.Depth * c_IndentationX) + width, res.Height );
            }

            protected override void Paint( Graphics                        graphics            ,
                                           Rectangle                       clipBounds          ,
                                           Rectangle                       cellBounds          ,
                                           int                             rowIndex            ,
                                           DataGridViewElementStates       cellState           ,
                                           object                          value               ,
                                           object                          formattedValue      ,
                                           string                          errorText           ,
                                           DataGridViewCellStyle           cellStyle           ,
                                           DataGridViewAdvancedBorderStyle advancedBorderStyle ,
                                           DataGridViewPaintParts          paintParts          )
            {
                // get the node reference at this row
                GridNode node  = (GridNode)this.DataGridView.Rows[this.RowIndex].Tag;
                int      width = c_IndentationX + (node.UseIcon ? c_IconSizeX : 0);
                
                // pad text to include space for tree lines and icons, plus indent to show tree depth     
      
                cellStyle.Padding = new Padding( cellStyle.Padding.Left + (node.Depth * c_IndentationX) + width, cellStyle.Padding.Top, cellStyle.Padding.Right, cellStyle.Padding.Bottom );

                // allow base to paint text
                base.Paint( graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts );

                // pen for tree lines
                Pen linePen = new Pen( Brushes.DarkGray );

                int xNodeLeft   = cellBounds.Left;
                int yNodeTop    = cellBounds.Top;
                int yNodeBottom = cellBounds.Bottom;
                int yNodeMiddle = yNodeTop + (yNodeBottom - yNodeTop) / 2;

                // make sure we aren't drawing tree lines for the root level nodes in the grid            
                if(node.Depth > 0)
                {
                    // walk up the tree to figure out what lines need to be drawn for the previous levels
                    GridNode tempNode = node.Parent;

                    for(int i = 1; i < node.Depth; i++)
                    {
                        if(tempNode.IsLastInList == false)
                        {
                            int x = xNodeLeft + ((tempNode.Depth - 1) * c_IndentationX) + c_LineOffsetX;

                            // draw line for ancestor that has 
                            graphics.DrawLine( linePen, x, yNodeTop, x, yNodeBottom );
                        }

                        tempNode = tempNode.Parent;
                    }

                    int xNode = xNodeLeft + ((node.Depth - 1) * c_IndentationX) + c_LineOffsetX;

                    // draw the line protruding from parent down to middle of row
                    graphics.DrawLine( linePen, new Point( xNode, yNodeTop ), new Point( xNode, yNodeMiddle ) );

                    // draw the line from middle of row pointing out to value
                    graphics.DrawLine( linePen, new Point( xNode, yNodeMiddle ), new Point( xNode + c_IndentationX, yNodeMiddle ) );

                    // draw the line protruding down to the next node if this is not the last in list
                    if(node.IsLastInList == false)
                    {
                        graphics.DrawLine(linePen, new Point( xNode, yNodeMiddle ), new Point( xNode, yNodeBottom ) );
                    }
                }

                {
                    int xNode = xNodeLeft + (node.Depth * c_IndentationX);

                    if(node.Icon != null)
                    {
                        Rectangle variableIconBounds = new Rectangle( xNode + c_UserIconOffsetX, yNodeTop + c_UserIconOffsetY, c_IconSizeX, c_IconSizeY );

                        graphics.DrawIconUnstretched( node.Icon, variableIconBounds );
                    }

                    // draw tree expand/collapse icon if this is a parent
                    if(node.HasNodes)
                    {
                        Rectangle treeIconBounds = new Rectangle( xNode + c_IconOffsetX, yNodeTop + c_IconOffsetY, c_IconSizeX, c_IconSizeY );

                        if(node.IsExpanded)
                        {
                            // draw the line protruding down from the tree
                            graphics.DrawLine( linePen, new Point( xNode + c_LineOffsetX, yNodeMiddle ), new Point( xNode + c_LineOffsetX, yNodeBottom ) );
                            
                            // draw the icon over the line
                            graphics.DrawIconUnstretched( TreeBasedGridView.NodeExpanded, treeIconBounds );

                        }
                        else
                        {
                            graphics.DrawIconUnstretched( TreeBasedGridView.NodeCollapsed, treeIconBounds );
                        }
                    }
                }

                // dispose the pen
                linePen.Dispose();
            }

            internal static bool HitIcon( GridNode                       node ,
                                          DataGridViewCellMouseEventArgs e    )
            {
                var hit = new Rectangle( c_IconOffsetX + c_TreeIconMarginX + node.Depth * c_IndentationX, c_IconOffsetY + c_TreeIconMarginY, c_TreeIconSizeX, c_TreeIconSizeY );

                return hit.Contains( e.X, e.Y );
            }
        }

        public delegate bool EnumerateDelegate( GridNode node );

        //--//

        //
        // State
        //

        static Icon NodeCollapsed = Properties.Resources.NodeCollapsed;
        static Icon NodeExpanded  = Properties.Resources.NodeExpanded;
        
        private          GridNode                     m_root;
        private          int                          m_suspendCount;
        private          NodeMouseEventHandler        m_eventHeaderMouseClick;
        private          NodeMouseEventHandler        m_eventHeaderMouseDoubleClick;
        private          NodeMouseEventHandler        m_eventNodeMouseClick;
        private          NodeMouseEventHandler        m_eventNodeMouseDoubleClick;
        private readonly List< GridColumnDefinition > m_columnDefinitions = new List< GridColumnDefinition >();

        //
        // Constructor Methods
        //

        public TreeBasedGridView()
        {
            InitializeComponent();
        }

        //
        // Helper Methods
        //

        public void SetColumns( params GridColumnDefinition[] colDefs )
        {
            m_columnDefinitions.Clear();

            this.SuspendLayout();

            var columns = this.dataGridView1.Columns;

            columns.Clear();

            foreach(var colDef in colDefs)
            {
                var newCol = new DataGridViewTextBoxColumn();

                newCol.HeaderText                 = colDef.Name;
                newCol.Name                       = colDef.Name;
                newCol.SortMode                   = DataGridViewColumnSortMode.NotSortable;
                newCol.DefaultCellStyle           = new DataGridViewCellStyle();
                newCol.DefaultCellStyle.Alignment = colDef.Alignment;

                columns.Add( newCol );

                m_columnDefinitions.Add( colDef );
            }

            this.dataGridView1.Rows.Clear();

            this.ResumeLayout( false );

            //--//

            m_root = new GridNode( this );
        }

        public void StartTreeUpdate()
        {
            if(m_suspendCount++ == 0)
            {
                dataGridView1.SuspendLayout();
            }
        }

        public void EndTreeUpdate()
        {
            if(--m_suspendCount == 0)
            {
                dataGridView1.ResumeLayout();
            }
        }

        public void EnumerateNodesPreOrder( EnumerateDelegate dlg )
        {
            m_root.Enumerate( dlg );
        }

        //--//

        GridNode GetSelectedNode( DataGridViewCellEventArgs e )
        {
            return GetSelectedNode( e.RowIndex, e.ColumnIndex );
        }

        GridNode GetSelectedNode( DataGridViewCellMouseEventArgs e )
        {
            return GetSelectedNode( e.RowIndex, e.ColumnIndex );
        }

        GridNode GetSelectedNode( int rowIndex    ,
                                  int columnIndex )
        {
            // leave if we are a header row
            if(rowIndex == -1 || columnIndex == -1)
            {
                return null;
            }

            var row = dataGridView1.Rows[rowIndex];

            return row.Tag as GridNode;
        }

        //--//

        private void NotifyNodeMouseClick( NodeMouseEventHandler eventHeader ,
                                           GridNode              node        ,
                                           MouseEventArgs        e           ,
                                           int                   rowIndex    ,
                                           int                   columnIndex )
        {
            if(eventHeader != null)
            {
                var rec = dataGridView1.GetCellDisplayRectangle( columnIndex, rowIndex, false );

                var ev = new NodeMouseEventArgs( node, columnIndex, e.Button, e.Clicks, rec.Left, rec.Top, e.Delta );

                eventHeader( this, ev );
            }
        }

        //
        // Access Methods
        //

        public GridNode RootNode
        {
            get
            {
                return m_root;
            }
        }

        public DataGridViewRowCollection Rows
        {
            get
            {
                return dataGridView1.Rows;
            }
        }

        public event NodeMouseEventHandler NodeMouseClick
        {
            add
            {
                m_eventNodeMouseClick += value;
            }

            remove
            {
                m_eventNodeMouseClick -= value;
            }
        }

        public event NodeMouseEventHandler NodeMouseDoubleClick
        {
            add
            {
                m_eventNodeMouseDoubleClick += value;
            }

            remove
            {
                m_eventNodeMouseDoubleClick -= value;
            }
        }

        public event NodeMouseEventHandler HeaderMouseClick
        {
            add
            {
                m_eventHeaderMouseClick += value;
            }

            remove
            {
                m_eventHeaderMouseClick -= value;
            }
        }

        public event NodeMouseEventHandler HeaderMouseDoubleClick
        {
            add
            {
                m_eventHeaderMouseDoubleClick += value;
            }

            remove
            {
                m_eventHeaderMouseDoubleClick -= value;
            }
        }

        //
        // Events Methods
        //

        private void dataGridView1_ColumnHeaderMouseClick( object                         sender ,
                                                           DataGridViewCellMouseEventArgs e      )
        {
            NotifyNodeMouseClick( m_eventHeaderMouseClick, null, e, e.RowIndex, e.ColumnIndex );
        }

        private void dataGridView1_ColumnHeaderMouseDoubleClick( object                         sender ,
                                                                 DataGridViewCellMouseEventArgs e      )
        {
            NotifyNodeMouseClick( m_eventHeaderMouseDoubleClick, null, e, e.RowIndex, e.ColumnIndex );
        }

        private void dataGridView1_CellMouseClick( object                         sender ,
                                                   DataGridViewCellMouseEventArgs e      )
        {
            GridNode node = GetSelectedNode( e );

            if(node != null)
            {
                NotifyNodeMouseClick( m_eventNodeMouseClick, node, e, e.RowIndex, e.ColumnIndex );

                if(e.ColumnIndex == 0 && GridNodeCell.HitIcon( node, e ))
                {
                    if(node.HasNodes)
                    {
                        node.IsExpanded = !node.IsExpanded;
                    }
                }
            }
        }

        private void dataGridView1_CellMouseDoubleClick( object                         sender ,
                                                         DataGridViewCellMouseEventArgs e      )
        {
            GridNode node = GetSelectedNode( e );

            // check to see if we are in the value column and initiate editing
            if(node != null)
            {
                NotifyNodeMouseClick( m_eventNodeMouseDoubleClick, node, e, e.RowIndex, e.ColumnIndex );

                if(m_columnDefinitions[e.ColumnIndex].IsEditable)
                {
                    // if we don't have a callback for the update then skip editing
                    if(node.IsUpdatable)
                    {
                        dataGridView1.BeginEdit( true );
                    }
                }
                else if(e.ColumnIndex == 0)
                {
                    if(node.HasNodes)
                    {
                        node.IsExpanded = !node.IsExpanded;
                    }
                }
            }
        }

        private void dataGridView1_CellEndEdit( object                    sender ,
                                                DataGridViewCellEventArgs e      )
        {
            var row = dataGridView1.Rows[e.RowIndex];

            GridNode node  = (GridNode)row.Tag;
            int      index = e.ColumnIndex;

            var cell = row.Cells[index];
            
            if(node.UpdateCallback != null)
            {
                string proposedValue = (string)cell.Value;

                if(node.UpdateCallback( node, proposedValue, index ))
                {
                    node[index] = proposedValue;

                    cell.Value = proposedValue;

                    node.HasChanged = true;
                    return;
                }
            }
            
            // if there is not an update callback then we will not perform the edit
            cell.Value = node[index];
        }

        private void dataGridView1_KeyDown( object       sender ,
                                            KeyEventArgs e      )
        {
            if(e.Modifiers == Keys.None)
            {
                var coll = dataGridView1.SelectedRows;

                if(coll.Count == 1)
                {
                    var      row   = coll[0];
                    GridNode node  = (GridNode)row.Tag;

                    if(e.KeyCode == Keys.Left)
                    {
                        if(node.IsExpanded)
                        {
                            node.IsExpanded = false;
                        }
                        else
                        {
                            var parent = node.Parent;
                            if(parent != null)
                            {
                                parent.Select();
                            }
                        }

                        e.Handled = true;
                        return;
                    }

                    if(e.KeyCode == Keys.Right)
                    {
                        if(!node.IsExpanded)
                        {
                            node.IsExpanded = true;
                        }
                        else if(node.HasNodes)
                        {
                            var child = node.ChildNodes.First.Value;

                            child.Select();
                        }

                        e.Handled = true;
                        return;
                    }
                }
            }
        }
    }
}

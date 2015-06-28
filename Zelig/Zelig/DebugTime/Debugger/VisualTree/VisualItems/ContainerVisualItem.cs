//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Text;
    using System.Windows.Forms;

    using IR = Microsoft.Zelig.CodeGeneration.IR;


    public sealed class ContainerVisualItem : VisualItem
    {
        //
        // State
        //

        private List< VisualItem > m_children;
        private bool               m_boundsValid;

        //
        // Constructor Methods
        //

        public ContainerVisualItem( object context ) : base( context )
        {
            m_children = new List< VisualItem >();
        }

        //
        // Helper Methods
        //

        public override void CollectDepths( GrowOnlySet< int > set )
        {
            set.Insert( m_depth );

            foreach(VisualItem child in m_children)
            {
                child.CollectDepths( set );
            }
        }

        public override bool Draw( GraphicsContext ctx            ,
                                   PointF          absoluteOrigin ,
                                   int             depth          )
        {
            bool isVisible = base.Draw( ctx, absoluteOrigin, depth );

            if(isVisible)
            {
                absoluteOrigin.X += m_relativeOrigin.X;
                absoluteOrigin.Y += m_relativeOrigin.Y;

                foreach(VisualItem child in m_children)
                {
                    child.Draw( ctx, absoluteOrigin, depth );
                }
            }

            return isVisible;
        }

        public override VisualItem Contains(     PointF pt        ,
                                                 bool   fLeafOnly ,
                                             out PointF relHitPt  )
        {
            if(base.Contains( pt, fLeafOnly, out relHitPt ) != null)
            {
                pt.X -= m_relativeOrigin.X;
                pt.Y -= m_relativeOrigin.Y;

                foreach(VisualItem child in m_children)
                {
                    VisualItem hit = child.Contains( pt, fLeafOnly, out relHitPt );

                    if(hit != null)
                    {
                        return hit;
                    }
                }

                if(fLeafOnly == false)
                {
                    return this;
                }
            }

            return null;
        }

        public override VisualItem ContainsVertically(     float y       ,
                                                       out float relHitY )
        {
            if(base.ContainsVertically( y, out relHitY ) != null)
            {
                y -= m_relativeOrigin.Y;

                foreach(VisualItem child in m_children)
                {
                    VisualItem hit = child.ContainsVertically( y, out relHitY );

                    if(hit != null)
                    {
                        return hit;
                    }
                }
            }

            return null;
        }

        public override VisualItem FindMatch( object o )
        {
            foreach(VisualItem child in m_children)
            {
                VisualItem match = child.FindMatch( o );

                if(match != null)
                {
                    return match;
                }
            }

            return null;
        }

        public override bool EnumerateTree( CodeView.EnumerationCallback dlg )
        {
            if(base.EnumerateTree( dlg ) == false)
            {
                foreach(VisualItem child in m_children)
                {
                    if(child.EnumerateTree( dlg ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //--//

        public override void InvalidateBounds()
        {
            m_boundsValid = false;

            base.InvalidateBounds();
        }

        public void InsertAt( VisualItem itemNew ,
                              int        pos     )
        {
            m_children.Insert( pos, itemNew );

            itemNew.Parent = this;

            InvalidateBounds();
        }

        public void Add( VisualItem item )
        {
            m_children.Add( item );

            item.Parent = this;

            InvalidateBounds();
        }

        public void Remove( VisualItem item )
        {
            m_children.Remove( item );

            item.Parent = null;

            InvalidateBounds();
        }

        public void Clear()
        {
            m_children.Clear();

            InvalidateBounds();
        }

        public void ExpandVerticalSpaceAfter( VisualItem item    ,
                                              VisualItem newItem )
        {
            RectangleF newBounds = newItem.RelativeBounds;
            RectangleF bounds    = item   .RelativeBounds;

            bounds.Offset( item.RelativeOrigin );

            newItem.RelativeOrigin = new PointF( bounds.X, bounds.Bottom );

            foreach(VisualItem child in m_children)
            {
                PointF childPos = child.RelativeOrigin;

                if(childPos.Y > item.RelativeOrigin.Y)
                {
                    childPos.Y += newBounds.Height;

                    child.RelativeOrigin = childPos;
                }
            }

            Add( newItem );
        }

        internal void RemoveVerticalSpace( VisualItem item )
        {
            RectangleF bounds = item.RelativeBounds;

            bounds.Offset( item.RelativeOrigin );

            foreach(VisualItem child in m_children)
            {
                PointF childPos = child.RelativeOrigin;

                if(childPos.Y > item.RelativeOrigin.Y)
                {
                    childPos.Y -= bounds.Height;

                    child.RelativeOrigin = childPos;
                }
            }

            Remove( item );
        }
        
        //
        // Access Methods
        //

        public override RectangleF RelativeBounds
        {
            get
            {
                if(m_boundsValid == false)
                {
                    bool fFirst = true;

                    m_relativeBounds = new RectangleF();

                    foreach(VisualItem child in m_children)
                    {
                        RectangleF childBounds = child.RelativeBounds;

                        childBounds.Offset( child.RelativeOrigin );

                        if(fFirst)
                        {
                            fFirst = false;
                            m_relativeBounds = childBounds;
                        }
                        else
                        {
                            m_relativeBounds = RectangleF.Union( m_relativeBounds, childBounds );
                        }
                    }

                    m_boundsValid = true;
                }

                return m_relativeBounds;
            }
        }

        public List< VisualItem > Children
        {
            get
            {
                return m_children;
            }
        }
    }
}

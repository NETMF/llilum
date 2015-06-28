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


    public abstract class VisualItem
    {
        //
        // State
        //

        protected ContainerVisualItem  m_parent;
        protected object               m_context;
                               
        protected Pen                  m_borderPen;
        protected Brush                m_backgroundBrush;
        protected PointF               m_relativeOrigin;
        protected RectangleF           m_relativeBounds;
        protected int                  m_depth;
                             
        protected CodeView.HitCallback m_hitSink;

        //
        // Constructor Methods
        //

        protected VisualItem( object context )
        {
            m_context = context;
        }

        //
        // Helper Methods
        //

        public virtual void CollectDepths( GrowOnlySet< int > set )
        {
            set.Insert( m_depth );
        }

        public virtual bool Draw( GraphicsContext ctx            ,
                                  PointF          absoluteOrigin ,
                                  int             depth          )
        {
            RectangleF bounds = this.RelativeBounds;

            bounds.Offset( absoluteOrigin      );
            bounds.Offset( this.RelativeOrigin );

            if(ctx.IsVisible( bounds ))
            {
                if(m_depth == depth)
                {
                    if(m_backgroundBrush != null)
                    {
                        ctx.Gfx.FillRectangle( m_backgroundBrush, bounds );
                    }

                    if(m_borderPen != null)
                    {
                        ctx.Gfx.DrawRectangle( m_borderPen, bounds.X, bounds.Y, bounds.Width, bounds.Height );
                    }
                }

                return true;
            }

            return false;
        }

        public virtual VisualItem Contains(     PointF pt        ,
                                                bool   fLeafOnly ,
                                            out PointF relHitPt  )
        {
            relHitPt = new PointF( pt.X - m_relativeOrigin.X, pt.Y - m_relativeOrigin.Y );

            if(this.RelativeBounds.Contains( relHitPt ))
            {
                return this;
            }

            return null;
        }

        public virtual VisualItem ContainsVertically(     float y       ,
                                                      out float relHitY )
        {
            relHitY = y - m_relativeOrigin.Y;

            RectangleF bounds = this.RelativeBounds;

            if(bounds.Top <= relHitY && relHitY < bounds.Bottom)
            {
                return this;
            }

            return null;
        }

        public virtual VisualItem FindMatch( object o )
        {
            if(o.Equals( m_context ))
            {
                return this;
            }

            return null;
        }

        public virtual bool EnumerateTree( CodeView.EnumerationCallback dlg )
        {
            return dlg( this );
        }

        public virtual void InvalidateBounds()
        {
            if(m_parent != null)
            {
                m_parent.InvalidateBounds();
            }
        }

        public void Delete()
        {
            if(m_parent != null)
            {
                m_parent.Remove( this );
            }
        }

        public void ExpandVerticalSpaceAfter( VisualItem newItem )
        {
            if(m_parent != null)
            {
                m_parent.ExpandVerticalSpaceAfter( this, newItem );
            }
        }

        public void RemoveVerticalSpace()
        {
            if(m_parent != null)
            {
                m_parent.RemoveVerticalSpace( this );
            }
        }

        //
        // Access Methods
        //

        public ContainerVisualItem Parent
        {
            get
            {
                return m_parent;
            }

            internal set
            {
                m_parent = value;
            }
        }


        public object Context
        {
            get
            {
                return m_context;
            }
        }

        public Pen BorderPen
        {
            get
            {
                return m_borderPen;
            }

            set
            {
                m_borderPen = value;
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                return m_backgroundBrush;
            }

            set
            {
                m_backgroundBrush = value;
            }
        }

        public PointF AbsoluteOrigin
        {
            get
            {
                if(m_parent != null)
                {
                    PointF res = m_parent.AbsoluteOrigin;

                    res.X += m_relativeOrigin.X;
                    res.Y += m_relativeOrigin.Y;

                    return res;
                }
                else
                {
                    return m_relativeOrigin;
                }
            }

            set
            {
                m_relativeOrigin = value;

                InvalidateBounds();
            }
        }

        public PointF RelativeOrigin
        {
            get
            {
                return m_relativeOrigin;
            }

            set
            {
                m_relativeOrigin = value;

                InvalidateBounds();
            }
        }

        public virtual RectangleF RelativeBounds
        {
            get
            {
                return m_relativeBounds;
            }

            set
            {
                m_relativeBounds = value;

                InvalidateBounds();
            }
        }

        public int Depth
        {
            get
            {
                return m_depth;
            }

            set
            {
                m_depth = value;
            }
        }

        public CodeView.HitCallback HitSink
        {
            get
            {
                return m_hitSink;
            }

            set
            {
                m_hitSink = value;
            }
        }
    }
}

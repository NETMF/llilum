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


    public abstract class BaseTextVisualItem : VisualItem
    {
        //
        // State
        //

        protected Font   m_font;
        protected Brush  m_brush;
        protected string m_cachedText;

        //
        // Constructor Methods
        //

        protected BaseTextVisualItem( object context ) : base( context )
        {
        }

        //
        // Helper Methods
        //

        public override bool Draw( GraphicsContext ctx            ,
                                   PointF          absoluteOrigin ,
                                   int             depth          )
        {
            bool isVisible = base.Draw( ctx, absoluteOrigin, depth );

            if(isVisible)
            {
                if(m_depth == depth)
                {
                    absoluteOrigin.X += m_relativeOrigin.X;
                    absoluteOrigin.Y += m_relativeOrigin.Y;

                    ctx.DrawStringWithDefault( m_cachedText, m_font, m_brush, absoluteOrigin );

                    RectangleF bounds = this.RelativeBounds;

                    bounds.Offset( absoluteOrigin      );
                    bounds.Offset( this.RelativeOrigin );
                }
            }

            return isVisible;
        }

        public void PrepareText( GraphicsContext ctx )
        {
            if(m_cachedText == null)
            {
                m_cachedText = ToString( ctx );

                SizeF size = ctx.MeasureString( m_cachedText, m_font );

                this.RelativeBounds = new RectangleF( 0, 0, size.Width, size.Height );
            }
        }

        public abstract string ToString( GraphicsContext ctx );

        //--//

        public static void PlaceInALine(     GraphicsContext    ctx   ,
                                             BaseTextVisualItem item  ,
                                         ref PointF             pt    ,
                                             float              preX  ,
                                             float              postX )
        {
            item.PrepareText( ctx );

            pt.X += ctx.CharSize( item.TextFont ) * preX;

            item.RelativeOrigin = pt;

            pt.X += item.RelativeBounds.Width;
            pt.X += ctx.CharSize( item.TextFont ) * postX;
        }

        //
        // Access Methods
        //

        public Font TextFont
        {
            get
            {
                return m_font;
            }

            set
            {
                m_font = value;
            }
        }

        public Brush TextBrush
        {
            get
            {
                return m_brush;
            }

            set
            {
                m_brush = value;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return m_cachedText;
        }
    }
}

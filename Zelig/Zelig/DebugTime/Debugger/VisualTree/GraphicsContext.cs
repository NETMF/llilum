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


    public class GraphicsContext
    {
        //
        // State
        //

        public Graphics Gfx;
        public Font     DefaultFont;
        public Brush    DefaultBrush;

        //
        // Constructor Methods
        //

        public GraphicsContext( Font   defaultFont ,
                                Bitmap bm          )
        {
            this.DefaultFont  = defaultFont;
            this.DefaultBrush = SystemBrushes.WindowText;

            if(bm == null)
            {
                bm = new Bitmap( 32, 32 );
            }

            this.Gfx = Graphics.FromImage( bm );
        }

        //
        // Helper Methods
        //

        public void DrawStringWithDefault( string text           ,
                                           Font   font           ,
                                           Brush  brush          ,
                                           PointF absoluteOrigin )
        {
            if(font == null)
            {
                font = this.DefaultFont;
            }

            if(brush == null)
            {
                brush = this.DefaultBrush;
            }

            this.Gfx.DrawString( text, font, brush, absoluteOrigin, StringFormat.GenericTypographic );
        }

        public SizeF MeasureString( string text ,
                                    Font   font )
        {
            if(font == null)
            {
                font = this.DefaultFont;
            }

            return this.Gfx.MeasureString( text, font, int.MaxValue, StringFormat.GenericTypographic );
        }

        public float CharSize( Font font )
        {
            return this.MeasureString( "A", font ).Width;
        }

        public float CharHeight( Font font )
        {
            if(font == null)
            {
                font = this.DefaultFont;
            }

            return font.Height;
        }

        public bool IsVisible( RectangleF bounds )
        {
            return this.Gfx.IsVisible( bounds );
        }
    }
}

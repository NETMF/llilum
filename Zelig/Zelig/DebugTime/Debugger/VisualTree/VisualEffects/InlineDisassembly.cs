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


    public abstract partial class VisualEffect
    {
        public sealed class InlineDisassembly : VisualEffect
        {
            //
            // Constructor Methods
            //

            public InlineDisassembly( VisualTreeInfo                     owner ,
                                      ContainerVisualItem                line  ,
                                      GraphicsContext                    ctx   ,
                                      ImageInformation.DisassemblyLine[] data  ,
                                      int                                depth ) : base( owner )
            {
                GenerateView( line, ctx, data, depth );
            }

            //
            // Helper Methods
            //

            public override void Clear()
            {
                foreach(VisualItem item in this.Items)
                {
                    item.RemoveVerticalSpace();
                }

                this.Items.Clear();
            }

            //--//

            private void GenerateView( ContainerVisualItem                line  ,
                                       GraphicsContext                    ctx   ,
                                       ImageInformation.DisassemblyLine[] data  ,
                                       int                                depth )
            {
                if(line != null)
                {
                    ContainerVisualItem box = new ContainerVisualItem( null );

                    box.BorderPen       = Pens.Red;
                    box.BackgroundBrush = Brushes.Pink;
                    box.Depth           = depth;

                    PointF ptLine = new PointF( 0, 0 );
                    float  stepY  = ctx.CharHeight( null );

                    for(int i = 0; i < data.Length; i++)
                    {
                        var disasm = data[i];
                        var item   = new TextVisualItem( disasm, "   " + disasm.Text );

                        item.RelativeOrigin = ptLine;
                        item.TextBrush      = Brushes.Gray;
                        item.Depth          = depth;

                        item.PrepareText( ctx );

                        ptLine.Y += stepY;

                        box.Add( item );
                    }

                    line.ExpandVerticalSpaceAfter( box );

                    this.Items.Add( box );
                }
            }
        }
    }
}

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
        public sealed class SourceCodeHighlight : VisualEffect
        {
            //
            // State
            //

            public ContainerVisualItem TopSelectedLine;

            //
            // Constructor Methods
            //

            public SourceCodeHighlight( VisualTreeInfo      owner            ,
                                        uint                address          ,
                                        Debugging.DebugInfo di               ,
                                        Icon                icon             ,
                                        Pen                 backgroundBorder ,
                                        Brush               backgroundFill   ,
                                        int                 depth            ) : base( owner )
            {
                if(di != null)
                {
                    ContainerVisualItem topLine = null;

                    for(int lineNum = di.BeginLineNumber; lineNum <= di.EndLineNumber; lineNum++)
                    {
                        ContainerVisualItem line = owner.FindLine( lineNum );

                        if(line != null)
                        {
                            if(topLine == null)
                            {
                                topLine = line;
                            }

                            RectangleF maximumBounds = new RectangleF();
                            bool       fGot          = false;

                            line.EnumerateTree( delegate( VisualItem item )
                            {
                                BaseTextVisualItem lineItem = item as BaseTextVisualItem;
                                if(lineItem != null)
                                {
                                    Debugging.DebugInfo di2 = lineItem.Context as Debugging.DebugInfo;
                                    if(di2 != null)
                                    {
                                        Debugging.DebugInfo diIntersection = di.ComputeIntersection( di2 );

                                        if(diIntersection != null)
                                        {
                                            RectangleF bounds     = lineItem.RelativeBounds;
                                            PointF     absPosItem = lineItem.AbsoluteOrigin;
                                            PointF     absPosLine = line    .AbsoluteOrigin;
                                            PointF     relPosItem = new PointF();
                                            float      step       = bounds.Width / (di2.EndColumn - di2.BeginColumn);

                                            relPosItem.X = (absPosItem.X - absPosLine.X) + (diIntersection.BeginColumn - di2.BeginColumn) * step;
                                            relPosItem.Y = (absPosItem.Y - absPosLine.Y);

                                            bounds.Offset( relPosItem );
                                            bounds.Width = (diIntersection.EndColumn - diIntersection.BeginColumn) * step;

                                            if(fGot)
                                            {
                                                maximumBounds = RectangleF.Union( maximumBounds, bounds );
                                            }
                                            else
                                            {
                                                fGot          = true;
                                                maximumBounds = bounds;
                                            }
                                        }
                                    }
                                }

                                return false;
                            } );

                            if(fGot)
                            {
                              //maximumBounds.X      -= 1;
                                maximumBounds.Y      -= 1;
                              //maximumBounds.Width  += 2;
                                maximumBounds.Height += 2;

                                if(backgroundBorder != null ||
                                   backgroundFill   != null  )
                                {
                                    BackgroundVisualItem highlightItem = new BackgroundVisualItem( di, backgroundBorder, backgroundFill, depth );
                                    PointF               relPosItem    = new PointF();

                                    relPosItem.X = maximumBounds.X;
                                    relPosItem.Y = maximumBounds.Y;

                                    highlightItem.RelativeOrigin = relPosItem;

                                    maximumBounds.X -= relPosItem.X;
                                    maximumBounds.Y -= relPosItem.Y;

                                    highlightItem.RelativeBounds = maximumBounds;

                                    {
                                        int pos = line.Children.Count;

                                        while(--pos >= 0)
                                        {
                                            BackgroundVisualItem item = line.Children[pos] as BackgroundVisualItem;

                                            if(item != null && item.Depth > depth)
                                            {
                                                break;
                                            }
                                        }

                                        line.InsertAt( highlightItem, pos + 1 );
                                    }

                                    this.Items.Add( highlightItem );
                                }

                                //--//

                                if(icon != null)
                                {
                                    IconVisualItem iconItem   = new IconVisualItem( di, icon, depth );
                                    PointF         relPosItem = new PointF();
                                    PointF         absOrigin  = line.AbsoluteOrigin;

                                    relPosItem.X = -absOrigin.X;
                                    relPosItem.Y = maximumBounds.Y;

                                    iconItem.RelativeOrigin = relPosItem;
                                    iconItem.RelativeBounds = new RectangleF( 0, 0, icon.Width, icon.Height );

                                    {
                                        int pos = line.Children.Count;

                                        while(--pos >= 0)
                                        {
                                            IconVisualItem item = line.Children[pos] as IconVisualItem;

                                            if(item != null && item.Depth > depth)
                                            {
                                                break;
                                            }
                                        }

                                        line.InsertAt( iconItem, pos + 1 );
                                    }

                                    this.Items.Add( iconItem );
                                }
                            }
                        }
                    }

                    this.TopSelectedLine = topLine;
                }

                if(address != uint.MaxValue)
                {
                    var workList = new List< BaseTextVisualItem >();

                    owner.VisualTreeRoot.EnumerateTree( delegate( VisualItem item )
                    {
                        BaseTextVisualItem lineItem = item as BaseTextVisualItem;
                        if(lineItem != null)
                        {
                            var disasm = lineItem.Context as ImageInformation.DisassemblyLine;
                            if(disasm != null && disasm.Address == address)
                            {
                                workList.Add( lineItem );
                            }
                        }

                        return false;
                    } );

                    foreach(var lineItem in workList)
                    {
                        var        disasm        = (ImageInformation.DisassemblyLine)lineItem.Context;
                        RectangleF maximumBounds = lineItem.RelativeBounds;

                        maximumBounds.Offset( lineItem.RelativeOrigin );

                      //maximumBounds.X      -= 1;
                        maximumBounds.Y      -= 1;
                      //maximumBounds.Width  += 2;
                        maximumBounds.Height += 2;

                        if(icon != null)
                        {
                            IconVisualItem iconItem   = new IconVisualItem( disasm, icon, depth );
                            PointF         relPosItem = new PointF();

                            relPosItem.X = 0;
                            relPosItem.Y = maximumBounds.Y;

                            iconItem.RelativeOrigin = relPosItem;
                            iconItem.RelativeBounds = new RectangleF( 0, 0, icon.Width, icon.Height );

                            lineItem.Parent.Add( iconItem );

                            this.Items.Add( iconItem );
                        }
                    }
                }
            }
        }
    }
}

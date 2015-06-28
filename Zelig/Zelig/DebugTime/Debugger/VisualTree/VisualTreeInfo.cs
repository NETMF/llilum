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

    public class VisualTreeInfo
    {
        public enum CallbackResult
        {
            Keep  ,
            Delete,
        }

        public delegate CallbackResult VisualEffectEnumerationCallback( VisualEffect ve );

        //
        // State
        //

        public string                          DisplayName;
        public IR.SourceCodeTracker.SourceCode Input;
        public ContainerVisualItem             VisualTreeRoot;
        public List< VisualEffect >            VisualEffects;
        public ContainerVisualItem[]           LineLookup;
        public Point                           ScrollPosition;

        //
        // Constructor Methods
        //

        public VisualTreeInfo( string                          displayName    ,
                               IR.SourceCodeTracker.SourceCode input          ,
                               ContainerVisualItem             visualTreeRoot )
        {
            this.DisplayName    = displayName;
            this.Input          = input;
            this.VisualTreeRoot = visualTreeRoot;
            this.VisualEffects  = new List< VisualEffect >();

            if(input != null)
            {
                this.LineLookup = new ContainerVisualItem[input.Count];

                visualTreeRoot.EnumerateTree( delegate( VisualItem item )
                {
                    ContainerVisualItem lineItem = item as ContainerVisualItem;
                    if(lineItem != null)
                    {
                        Debugging.DebugInfo di = lineItem.Context as Debugging.DebugInfo;
                        if(di != null)
                        {
                            this.LineLookup[di.BeginLineNumber - 1] = lineItem;
                        }
                    }

                    return false;
                } );
            }
        }

        //
        // Helper Methods
        //

        public void Draw( GraphicsContext ctx            ,
                          PointF          absoluteOrigin )
        {
            GrowOnlySet< int > set = SetFactory.New< int >();

            this.VisualTreeRoot.CollectDepths( set );

            int[] array = set.ToArray();

            Array.Sort( array );

            for(int i = array.Length; --i >= 0; )
            {
                this.VisualTreeRoot.Draw( ctx, absoluteOrigin, array[i] );
            }
        }

        public void ScrollToItem( VisualItem bringIntoView    ,
                                  Rectangle  displayRectangle ,
                                  Rectangle  controlRectangle )
        {
            if(bringIntoView != null)
            {
                //
                // We want to have the selected item away from the window's borders,
                // so shrink the actual viewport size.
                //
                if(controlRectangle.Width > 50)
                {
                    controlRectangle.X     += 25;
                    controlRectangle.Width -= 50;
                }

                if(controlRectangle.Height > 50)
                {
                    controlRectangle.Y      += 25;
                    controlRectangle.Height -= 50;
                }

                PointF     pt       =      bringIntoView.AbsoluteOrigin;
                RectangleF rect     =      bringIntoView.RelativeBounds;
                int        x        = (int)(pt.X + rect.Left);
                int        y        = (int)(pt.Y + rect.Top );
                int        dx       =      x + displayRectangle.X;
                int        dy       =      y + displayRectangle.Y;
                Rectangle  rectItem =      new Rectangle( dx, dy, (int)rect.Width, (int)rect.Height );

                //
                // Only scroll is the item is not fully contained within the current control view.
                //
                if(controlRectangle.Contains( rectItem ) == false)
                {
                    if(dx < controlRectangle.Width)
                    {
                        x = -displayRectangle.X;
                    }

                    //
                    // Center it.
                    //
                    y -= controlRectangle.Height / 2;
                }
                else
                {
                    x = -displayRectangle.X;
                    y = -displayRectangle.Y;
                }

                this.ScrollPosition = new Point( x, y );
            }
        }

        public void ClearAllVisualEffects()
        {
            foreach(VisualEffect ve in this.VisualEffects)
            {
                ve.Clear();
            }

            this.VisualEffects.Clear();
        }

        public void ClearVisualEffects( Type filter )
        {
            EnumerateVisualEffects( delegate( VisualEffect ve )
            {
                return filter.IsInstanceOfType( ve ) ? CallbackResult.Delete : CallbackResult.Keep;
            } );
        }

        public void EnumerateVisualEffects( VisualEffectEnumerationCallback dlg )
        {
            for(int i = this.VisualEffects.Count; --i >= 0; )
            {
                VisualEffect ve = this.VisualEffects[i];

                switch(dlg( ve ))
                {
                    case CallbackResult.Delete:
                        ve.Clear();

                        this.VisualEffects.RemoveAt( i );
                        break;
                }
            }
        }

        public ContainerVisualItem FindLine( int line )
        {
            line--;

            if(line >= 0 && this.LineLookup != null && line  < this.LineLookup.Length)
            {
                return this.LineLookup[line];
            }

            return null;
        }

        public VisualEffect.SourceCodeHighlight HighlightSourceCode( CodeView            cv               ,
                                                                     uint                address          ,
                                                                     Debugging.DebugInfo di               ,
                                                                     Icon                icon             ,
                                                                     Pen                 backgroundBorder ,
                                                                     Brush               backgroundFill   ,
                                                                     int                 depth            )
        {
            return new VisualEffect.SourceCodeHighlight( this, address, di, icon, backgroundBorder, backgroundFill, depth );
        }

        public VisualEffect.InlineDisassembly InstallDisassemblyBlock( ContainerVisualItem                line    ,
                                                                       CodeView                           control ,
                                                                       ImageInformation.DisassemblyLine[] lines   ,
                                                                       int                                depth   )
        {
            return new VisualEffect.InlineDisassembly( this, line, control.CtxForLayout, lines, depth );
        }
    }
}

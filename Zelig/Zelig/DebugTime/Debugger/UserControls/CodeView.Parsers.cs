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
    using System.IO;

    public partial class CodeView : UserControl
    {
        static string[] s_preprocessingDirectives = new string[]
        {
            "#if"       ,
            "#else"     ,
            "#elif"     ,
            "#endif"    ,
            "#define"   ,
            "#undef"    ,
            "#warning"  ,
            "#error"    ,
            "#line"     ,
            "#region"   ,
            "#endregion",
            "#pragma"   ,
        };

        //
        // Helper Methods
        //

        public void RefreshVisualTree()
        {
            if(m_activeVisualTree != null)
            {
                RectangleF bounds = m_activeVisualTree.VisualTreeRoot.RelativeBounds;

                this.AutoScrollMinSize = new Size( (int)bounds.Right, (int)bounds.Bottom );
            }
            else
            {
                this.AutoScrollMinSize = new Size( 0, 0 );
            }

            m_fInvalidated = true;

            this.Invalidate();
        }

        public void InstallVisualTree( VisualTreeInfo vti           ,
                                       VisualItem     bringIntoView )
        {
            if(m_activeVisualTree != null)
            {
                Point pt = this.AutoScrollPosition;

                //
                // Maybe there's a bug in the WinForms framework, because AutoScrollPosition returns the mirrored point.
                //
                m_activeVisualTree.ScrollPosition.X = -pt.X;
                m_activeVisualTree.ScrollPosition.Y = -pt.Y;
            }

            m_activeVisualTree = vti;

            RefreshVisualTree();

            if(vti != null)
            {
                vti.ScrollToItem( bringIntoView, this.DisplayRectangle, new Rectangle( 0, 0, this.Width, this.Height ) );

                this.AutoScrollPosition = vti.ScrollPosition;
            }
        }

        //--//

        public VisualTreeInfo GetVisualTree( string file )
        {
            VisualTreeInfo vti;
            string         key = file.ToUpper();

            m_visualTrees.TryGetValue( key, out vti );

            return vti;
        }

        public VisualTreeInfo CreateEmptyVisualTree( string displayName )
        {
            VisualTreeInfo vti;
            string         key = displayName;

            if(m_visualTrees.TryGetValue( key, out vti ) == false)
            {
                ContainerVisualItem topElement = new ContainerVisualItem( null );

                vti = new VisualTreeInfo( displayName, null, topElement );

                m_visualTrees[key] = vti;
            }

            return vti;
        }

        public VisualTreeInfo CreateVisualTree( ImageInformation imageInformation ,
                                                string           displayName      ,
                                                string           file             )
        {
            VisualTreeInfo vti;
            string         key = file.ToUpper();

            if(m_visualTrees.TryGetValue( key, out vti ) == false)
            {
                try
                {
                    IR.SourceCodeTracker.SourceCode sc;

                    if(imageInformation != null && imageInformation.ImageBuilder != null)
                    {
                        sc = imageInformation.ImageBuilder.SourceCodeTracker.GetSourceCode( file );
                    }
                    else
                    {
                        sc = null;
                    }

                    if(sc == null)
                    {
                        if(!System.IO.File.Exists( file ))
                        {
                            string fileName = Path.GetFileName(file);

                            foreach(string path in m_codeSearchPaths)
                            {
                                string tmp = Path.Combine( path, fileName );

                                if(File.Exists( tmp ))
                                {
                                    file = tmp;
                                    break;
                                }
                            }
                        }

                        if(System.IO.File.Exists( file ))
                        {
                            sc = new IR.SourceCodeTracker.SourceCode( file );
                        }
                    }

                    if(sc != null)
                    {
                        ContainerVisualItem topElement = TokenizeCSharpCode( sc );

                        vti = new VisualTreeInfo( displayName, sc, topElement );

                        m_visualTrees[key] = vti;
                    }
                }
                catch
                {
                    vti = null;
                }
            }

            return vti;
        }

        private ContainerVisualItem TokenizeCSharpCode( IR.SourceCodeTracker.SourceCode sc )
        {
            ContainerVisualItem topElement = new ContainerVisualItem( sc );

            if(sc != null)
            {
                Microsoft.CSharp.Token[] tokens       = sc.Tokens;
                Microsoft.CSharp.Token   nextToken    = null;
                int                      idxToken     = 0;
                int                      lines        = sc.Count;
                ContainerVisualItem      lineItem     = null;
                Brush                    brushComment = new SolidBrush( Color.Green );
                Brush                    brushKeyword = new SolidBrush( Color.Blue  );
                Brush                    brushOther   = new SolidBrush( Color.Black );

                PointF          ptLine = new PointF( 0, 0 );
                GraphicsContext ctx    = this.CtxForLayout;
                float           stepX  = ctx.CharSize  ( null );
                float           stepY  = ctx.CharHeight( null );

                for(int line = 0; line < lines; line++)
                {
                    string text       = sc[line+1];
                    PointF ptWord     = new PointF( 0, 0 );
                    int    pos        = 0;
                    Brush  lastBrush  = null;
                    int    lastPos    = 0;
                    int    lastPosEnd = 0;

                    while(pos < text.Length)
                    {
                        int   posEnd = text.Length;
                        Brush brush  = brushOther;

                        while(true)
                        {
                            if(nextToken == null)
                            {
                                if(idxToken < tokens.Length)
                                {
                                    nextToken = tokens[idxToken++];
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if(nextToken.Position.Line < line)
                            {
                                nextToken = null;
                                continue;
                            }

                            bool fGotBrush;

                            if(pos == 0 && text[0] == '#')
                            {
                                UpdateEndPosOnMatch( text, ref posEnd, s_preprocessingDirectives );

                                brush     = brushKeyword;
                                fGotBrush = true;
                            }
                            else
                            {
                                fGotBrush = false;
                            }

                            if(nextToken.Position.Line > line)
                            {
                                break;
                            }

                            if(pos < nextToken.Position.Column)
                            {
                                posEnd = nextToken.Position.Column;
                                break;
                            }

                            if(nextToken.Type == Microsoft.CSharp.TokenType.Comment)
                            {
                                brush = brushComment;
                            }
                            else if(Microsoft.CSharp.Token.IsKeyword( nextToken.Type ))
                            {
                                brush = brushKeyword;
                            }
                            else if(fGotBrush == false)
                            {
                                brush = brushOther;
                            }

                            nextToken = null;
                        }

                        if(lastBrush != null)
                        {
                            if(lastBrush == brush)
                            {
                                lastPosEnd = posEnd;
                                pos        = posEnd;
                                continue;
                            }

                            EmitWordGroup( ctx, topElement, ref lineItem, ref ptLine, stepX, line, lastPos, lastPosEnd, lastBrush, text );

                            lastBrush = null;
                        }

                        lastPos    = pos;
                        lastPosEnd = posEnd;
                        lastBrush  = brush;
                        pos        = posEnd;
                    }

                    if(lastBrush != null)
                    {
                        EmitWordGroup( ctx, topElement, ref lineItem, ref ptLine, stepX, line, lastPos, lastPosEnd, lastBrush, text );
                    }

                    lineItem = null;

                    ptLine.Y += stepY;
                }
            }

            return topElement;
        }

        private static void UpdateEndPosOnMatch(     string   text                    ,
                                                 ref int      posEnd                  ,
                                                     string[] preprocessingDirectives )
        {
            foreach(string preprocessingDirective in preprocessingDirectives)
            {
                if(text.StartsWith( preprocessingDirective ))
                {
                    posEnd = preprocessingDirective.Length;
                    break;
                }
            }
        }

        private void EmitWordGroup(     GraphicsContext     ctx        ,
                                        ContainerVisualItem topElement ,
                                    ref ContainerVisualItem lineItem   ,
                                    ref PointF              ptLine     ,
                                        float               stepX      ,
                                        int                 line       ,
                                        int                 pos        ,
                                        int                 posEnd     ,
                                        Brush               brush      ,
                                        string              text       )
        {
            if(lineItem == null)
            {
                lineItem = new ContainerVisualItem( Debugging.DebugInfo.CreateMarkerForLine( null, null, line + 1 ) );

                lineItem.RelativeOrigin = ptLine;

                topElement.Add( lineItem );
            }

            //--//

            while(pos < posEnd)
            {
                if(text[pos] == ' ')
                {
                    pos++;
                    continue;
                }

                int posEnd2 = text.IndexOf( ' ', pos );

                if(posEnd2 < 0)
                {
                    posEnd2 = posEnd;
                }

                TextVisualItem item = new TextVisualItem( new Debugging.DebugInfo( null, line + 1, pos + 1, line + 1, posEnd2 + 1 ), text.Substring( pos, posEnd2 - pos ) );

                item.TextBrush = brush;
                PointF ptWord = new PointF( pos * stepX, 0 );

                BaseTextVisualItem.PlaceInALine( ctx, item, ref ptWord, 0, 0 );

                lineItem.Add( item );

                pos = posEnd2;
            }
        }
    }
}

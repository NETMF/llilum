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
    using System.Collections.Specialized;

    public partial class CodeView : UserControl
    {
        public delegate void HitCallback( CodeView owner, VisualItem origin, PointF relPos, MouseEventArgs e, bool fDown, bool fUp );

        public delegate bool EnumerationCallback( VisualItem item );

        //
        // State
        //

        HitCallback                                 m_fallbackHitSink;
        HitCallback                                 m_defaultHitSink;
                            
        Rectangle                                   m_lastDisplayRect;
        Bitmap                                      m_lastCachedDisplay;
        bool                                        m_fInvalidated;
        List<string>                                m_codeSearchPaths;

        GrowOnlyHashTable< string, VisualTreeInfo > m_visualTrees;
        VisualTreeInfo                              m_activeVisualTree;

        //
        // Constructor Methods
        //

        public CodeView()
        {
            InitializeComponent();

            //--//

            m_visualTrees      = HashTableFactory.New< string, VisualTreeInfo >();
            m_activeVisualTree = null;
            m_codeSearchPaths  = new List<string>();

            try
            {
                NameValueCollection nvc = System.Configuration.ConfigurationManager.AppSettings;

                foreach(string key in nvc.AllKeys)
                {
                    if(key == "SourceCodeSearchPath")
                    {
                        string paths = Environment.ExpandEnvironmentVariables(nvc[key]);
                        m_codeSearchPaths.AddRange( paths.Split( ';' ) );
                        break;
                    }
                }
            }
            catch
            {
            }
        }

        //
        // Helper Methods
        //

        protected override void OnPaint( PaintEventArgs pe )
        {
            if(m_activeVisualTree != null)
            {
                Rectangle currentDisplayRect = this.DisplayRectangle;

                if(m_lastDisplayRect != currentDisplayRect)
                {
                    m_lastDisplayRect = currentDisplayRect;
                    m_fInvalidated    = true;
                }

                Bitmap bm = m_lastCachedDisplay;

                if(bm        == null        ||
                   bm.Width  != this.Width  ||
                   bm.Height != this.Height  )
                {
                    bm = new Bitmap( this.Width, this.Height );

                    m_lastCachedDisplay = bm;
                    m_fInvalidated      = true;
                }

                if(m_fInvalidated)
                {
                    GraphicsContext ctx = new GraphicsContext( this.Font, bm );

                    ctx.Gfx.Clear( Color.White );

                    m_activeVisualTree.Draw( ctx, new PointF( m_lastDisplayRect.X, m_lastDisplayRect.Y ) );

                    m_fInvalidated = false;
                }

                pe.Graphics.DrawImage( bm, 0, 0 );
            }
        }

        protected override void OnPaintBackground( PaintEventArgs pevent )
        {
            if(m_activeVisualTree == null)
            {
                base.OnPaintBackground( pevent );
            }
        }

        //--//

        private void Notify( MouseEventArgs e     ,
                             bool           fDown ,
                             bool           fUp   )
        {
            VisualTreeInfo vti = m_activeVisualTree;

            if(vti != null)
            {
                PointF     testPt;
                PointF     hitPt;
                VisualItem hit;
                Rectangle  currentDisplayRect = this.DisplayRectangle;
                
                testPt = new PointF( e.X - currentDisplayRect.Left, e.Y - currentDisplayRect.Top );

                hit = vti.VisualTreeRoot.Contains( testPt, true, out hitPt );
                if(hit == null)
                {
////                hit = vti.VisualTreeRoot.Contains( testPt, false, out hitPt );
////                if(hit == null)
                    {
                        float y;

                        hit = vti.VisualTreeRoot.ContainsVertically( testPt.Y, out y );

                        hitPt.X = -1.0f;
                        hitPt.Y = y;
                    }
                }

                HitCallback hitSink = null;

                if(hit != null)
                {
                    for(VisualItem item = hit; item != null; item = item.Parent)
                    {
                        if(item.HitSink != null)
                        {
                            hitSink = item.HitSink;
                            break;
                        }
                    }

                    if(hitSink == null)
                    {
                        hitSink = m_defaultHitSink;
                    }
                }
                else
                {
                    hitPt = new PointF( e.X - currentDisplayRect.Left, e.Y - currentDisplayRect.Top );
                }

                if(hitSink == null)
                {
                    hitSink = m_fallbackHitSink;
                }

                if(hitSink != null)
                {
                    hitSink( this, hit, hitPt, e, fDown, fUp );
                }
            }
        }

        //--//

        //
        // Access Methods
        //

        public GraphicsContext CtxForLayout
        {
            get
            {
                return new GraphicsContext( this.Font, null );
            }
        }

        public HitCallback FallbackHitSink
        {
            get
            {
                return m_fallbackHitSink;
            }

            set
            {
                m_fallbackHitSink = value;
            }
        }

        public HitCallback DefaultHitSink
        {
            get
            {
                return m_defaultHitSink;
            }

            set
            {
                m_defaultHitSink = value;
            }
        }

        public GrowOnlyHashTable< string, VisualTreeInfo > VisualTrees
        {
            get
            {
                return m_visualTrees;
            }
        }

        public VisualTreeInfo ActiveVisualTree
        {
            get
            {
                return m_activeVisualTree;
            }
        }

        //
        // Event Methods
        //

        private void CodeView_MouseDown( object sender, MouseEventArgs e )
        {
            Notify( e, true, false );
        }

        private void CodeView_MouseMove( object sender, MouseEventArgs e )
        {
            Notify( e, false, false );
        }

        private void CodeView_MouseUp( object sender, MouseEventArgs e )
        {
            Notify( e, false, true );
        }
    }
}

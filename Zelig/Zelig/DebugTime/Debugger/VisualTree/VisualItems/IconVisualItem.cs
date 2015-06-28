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


    public class IconVisualItem : VisualItem
    {
        //
        // State
        //

        protected Icon m_icon;

        //
        // Constructor Methods
        //

        public IconVisualItem( object context ,
                               Icon   icon    ,
                               int    depth   ) : base( context )
        {
            m_icon  = icon;
            m_depth = depth;
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

                    ctx.Gfx.DrawIcon( m_icon, (int)absoluteOrigin.X, (int)absoluteOrigin.Y );
                }
            }

            return isVisible;
        }

        //
        // Access Methods
        //

    }
}

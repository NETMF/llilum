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


    public class BackgroundVisualItem : VisualItem
    {
        //
        // Constructor Methods
        //

        public BackgroundVisualItem( object context         ,
                                     Pen    borderPen       ,
                                     Brush  backgroundBrush ,
                                     int    depth           ) : base( context )
        {
            m_borderPen       = borderPen;
            m_backgroundBrush = backgroundBrush;
            m_depth           = depth;
        }

        //
        // Helper Methods
        //
        
        //
        // Access Methods
        //

    }
}

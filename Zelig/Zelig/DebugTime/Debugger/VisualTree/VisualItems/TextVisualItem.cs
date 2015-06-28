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


    public class TextVisualItem : BaseTextVisualItem
    {
        //
        // State
        //

        protected string m_text;

        //
        // Constructor Methods
        //

        public TextVisualItem( object context ,
                               string text    ) : base( context )
        {
            m_text = text;
        }

        //
        // Helper Methods
        //

        public override string ToString( GraphicsContext ctx )
        {
            return m_text;
        }
    }
}

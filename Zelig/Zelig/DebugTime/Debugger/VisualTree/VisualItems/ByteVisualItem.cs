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


    public class ByteVisualItem : BaseTextVisualItem
    {
        //
        // State
        //

        byte m_value;

        //
        // Constructor Methods
        //

        public ByteVisualItem( object context ,
                               byte   value   ) : base( context )
        {
            m_value = value;
        }

        //
        // Helper Methods
        //

        public override string ToString( GraphicsContext ctx )
        {
            return String.Format( "{0,2:X2}", m_value );
        }
    }
}

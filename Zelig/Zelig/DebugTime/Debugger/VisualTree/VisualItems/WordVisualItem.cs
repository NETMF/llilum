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

    public class WordItem : BaseTextVisualItem
    {
        //
        // State
        //

        uint m_value;

        //
        // Constructor Methods
        //

        public WordItem( object context ,
                         uint   value   ) : base( context )
        {
            m_value = value;
        }

        //
        // Helper Methods
        //

        public override string ToString( GraphicsContext ctx )
        {
            return String.Format( "{0,8:X8}", m_value );
        }
    }
}

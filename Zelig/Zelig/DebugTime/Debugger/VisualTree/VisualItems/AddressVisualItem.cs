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


    public class AddressVisualItem : BaseTextVisualItem
    {
        //
        // State
        //

        uint m_address;

        //
        // Constructor Methods
        //

        public AddressVisualItem( object context ,
                                  uint   address ) : base( context )
        {
            m_address = address;
        }

        //
        // Helper Methods
        //

        public override string ToString( GraphicsContext ctx )
        {
            return String.Format( "[{0,8:X8}]", m_address );
        }
    }
}

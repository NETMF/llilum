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


    public class CharsVisualItem : BaseTextVisualItem
    {
        //
        // State
        //

        string m_text;

        //
        // Constructor Methods
        //

        public CharsVisualItem( object                           context ,
                                Emulation.Hosting.MemoryProvider memory  ,
                                uint                             address ,
                                int                              size    ) : base( context )
        {
            char[] buf = new char[size];

            for(int i = 0; i < size; i++)
            {
                byte c;

                if(memory.GetUInt8( address++, out c ) == false)
                {
                    buf[i] = '?';
                }
                else if(c >= 20 && c < 128)
                {
                    buf[i] = (char)c;
                }
                else
                {
                    buf[i] = '.';
                }
            }

            m_text = new string( buf );
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

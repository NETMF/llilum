//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;


    public abstract class AbstractUIPlugIn : AbstractPlugIn
    {
        //
        // State
        //

        protected Forms.HostingSite m_owner;

        //
        // Constructor Methods
        //

        protected AbstractUIPlugIn( Forms.HostingSite owner )
        {
            m_owner = owner;
        }

        //
        // Helper Methods
        //

    }
}

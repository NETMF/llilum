//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class AbstractHardwarePlugIn : AbstractPlugIn
    {
        //
        // State
        //

        protected Hosting.AbstractHost m_engine;
        protected Cfg.ProductCategory  m_product;

        //
        // Constructor Methods
        //

        protected AbstractHardwarePlugIn( Hosting.AbstractHost engine  ,
                                          Cfg.ProductCategory  product )
        {
            m_engine  = engine;
            m_product = product;
        }

        //
        // Helper Methods
        //

    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class JTagCustomer
    {
        //
        // State
        //

        //
        // Helper Methods
        //

        public abstract void Deploy( Emulation.Hosting.AbstractHost                 owner    ,
                                     Cfg.ProductCategory                            product  ,
                                     List< Configuration.Environment.ImageSection > image    ,
                                     ProcessorControl.ProgressCallback              callback );

        //--//

        protected static uint[] ToUint( byte[] data8 )
        {
            return ToUint( data8, 0, data8.Length );
        }

        protected static uint[] ToUint( byte[] data8  ,
                                        int    offset ,
                                        int    size   )
        {
            uint[] data32 = new uint[(size + sizeof(uint) - 1) / sizeof(uint)];

            Buffer.BlockCopy( data8, offset, data32, 0, size );

            return data32;
        }

        //
        // Access Methods
        //
    }
}

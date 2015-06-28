//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class RamJtagLoaderCategory : JtagLoaderCategory
    {
        class RamLoader : Emulation.Hosting.JTagCustomer
        {
            //
            // State
            //

            ProductCategory                 m_product;
            Emulation.Hosting.AbstractHost  m_owner;
            Emulation.Hosting.JTagConnector m_jtag;

            //
            // Constructor Methods
            //

            public RamLoader()
            {
            }

            //
            // Helper Methods
            //

            public override void Deploy( Emulation.Hosting.AbstractHost                      owner    ,
                                         Cfg.ProductCategory                                 product  ,
                                         List< Configuration.Environment.ImageSection >      image    ,
                                         Emulation.Hosting.ProcessorControl.ProgressCallback callback )
            {
                m_owner   = owner;
                m_product = product;

                owner.GetHostingService( out m_jtag );

                //--//

////            using(var file = new System.IO.FileStream( @"s:\imagedump.bin", System.IO.FileMode.Create ))
////            {
////                foreach(var section in image)
////                {
////                    file.Seek( section.Address - 0x08000000, System.IO.SeekOrigin.Begin );
////                    file.Write( section.Payload, 0, section.Payload.Length );
////                }
////            }

                float position = 0;
                float total    = 0;

                foreach(var section in image)
                {
                    total += section.Payload.Length;
                }

                callback( "Downloading {0}/{1}", 0.0f, (float)total );

                m_jtag.StopTarget();

                foreach(var section in image)
                {
                    const int chunkSize = 4096;

                    uint   address = section.Address;
                    byte[] data    = section.Payload;
                    uint[] data32  = ToUint( data );

                    for(int chunk = 0; chunk < data32.Length; chunk += chunkSize)
                    {
                        int count = Math.Min( data32.Length - chunk, chunkSize );

                        m_jtag.WriteMemoryBlock( address, data32, chunk, count );

                        address  += (uint)(count * sizeof(uint));
                        position +=        count * sizeof(uint);

                        callback( "Downloading {0}/{1}", position, total );
                    }
                }
            }
        }

        //--//

        protected RamJtagLoaderCategory()
        {
        }

        protected override object GetServiceInner( Type t )
        {
            if(t == typeof(Emulation.Hosting.JTagCustomer))
            {
                return new RamLoader();
            }

            return base.GetServiceInner( t );
        }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Zelig.MetaData;

    class Bench
    {
        public static void Main( string[] args )
        {
            if(args != null)
            {
                foreach(string arg in args)
                {
                    switch(arg)
                    {
                        case "PELoaderTester":
                            new PELoaderTester().Run( args );
                            break;

                        case "PDBFileTester":
                            new PDBFileTester().Run( args );
                            break;

                        case "CompressionTester":
                            new CompressionTester().Run( args );
                            break;

                        case "MetaDataParserTester":
                            new MetaDataParserTester().Run( args );
                            break;

                        case "MetaDataResolverTester":
                            new MetaDataResolverTester().Run( args );
                            break;
                    }
                }
            }
        }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.UnitTest
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
                        case "TypeSystemPopulationTester":
                            TypeSystemPopulationTester.Run( args );
                            break;

                        case "GenericInstantiationClosureTester":
                            GenericInstantiationClosureTester.Run( args );
                            break;

                        case "CodeGenerationTester":
                            CodeGenerationTester.Run( args );
                            break;

                        case "SerializationSpeedTester":
                            SerializationSpeedTester.Run( args );
                            break;
                    }
                }
            }
        }
    }
}

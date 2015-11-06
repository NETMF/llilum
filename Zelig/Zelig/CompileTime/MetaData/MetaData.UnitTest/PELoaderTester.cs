//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.Test;

    class PELoaderTester : TestBase
    {
        public PELoaderTester()
        {

        }

        override public TestResult Run( string[] args )
        {
            base.Run(args);

            string currentDir = System.Environment.CurrentDirectory;

            LoadPE( Expand( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll"  ) );
            LoadPE( Expand( currentDir + @"\Test\TestPayload__CLR1_1__VanillaSingleClass.dll" ) );

            return TestResult.Pass;
        }

        static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        static void LoadPE( string file )
        {
            byte[] image = System.IO.File.ReadAllBytes( file );

            Importer.PELoader pe = new Zelig.MetaData.Importer.PELoader( file, image );

            Importer.LogWriter log = new Zelig.MetaData.Importer.LogWriter( Console.Out );

            pe.DumpHeader( log );
        }
    }
}

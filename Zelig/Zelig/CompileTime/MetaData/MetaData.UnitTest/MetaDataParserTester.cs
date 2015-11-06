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

    class MetaDataParserTester : TestBase, ISymbolResolverHelper
    {
        override public TestResult Run( string[] args )
        {
            base.Run(args);

            Importer.MetaData    md;
            MetaDataParserTester pThis;

            pThis = new MetaDataParserTester();

            string currentDir = System.Environment.CurrentDirectory;

            md = pThis.LoadAssembly( Expand( currentDir + @"\Test\TestPayload__CLR1_1__VanillaSingleClass.dll" ) );
            md = pThis.LoadAssembly( Expand( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll"  ) );

            foreach(string file in System.IO.Directory.GetFiles( Expand( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727" ), "*.dll" ))
            {
                try
                {
                    Console.WriteLine( "Parsing {0}", file );
                    md = pThis.LoadAssembly( file );
                }
                catch(Importer.PELoader.MissingCLRheaderException e)
                {
                    Console.WriteLine( "CLR header is missing, nothing to parse {0}: {1}", file, e);
                }
                catch(Importer.PELoader.IllegalPEFormatException e)
                {
                    Console.WriteLine( "Error while parsing {0}: {1}", file, e);
                }
                catch(Exception e)
                {
                    Console.WriteLine( "Error while loading {0}: {1}", file, e );
                }
            }

            return TestResult.Pass;
        }

        static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        private Microsoft.Zelig.MetaData.Importer.MetaData LoadAssembly( string file )
        {
            byte[] image = System.IO.File.ReadAllBytes( file );

            Importer.PELoader pe = new Importer.PELoader( file, image );

            Importer.MetaData md = Importer.MetaData.loadMetaData( file, this, pe, Importer.MetaDataLoaderFlags.LoadCode | Importer.MetaDataLoaderFlags.LoadDebugInfo );

            return md;
        }

        //--//

        Importer.PdbInfo.PdbFile ISymbolResolverHelper.ResolveAssemblySymbols( string file )
        {
            try
            {
                file = System.IO.Path.GetDirectoryName( file ) + @"\" + System.IO.Path.GetFileNameWithoutExtension( file ) + ".pdb";

                if(System.IO.File.Exists( file ))
                {
                    byte[] image = System.IO.File.ReadAllBytes( file );

                    Importer.PdbInfo.PdbFile pdbFile = new Importer.PdbInfo.PdbFile( new Importer.ArrayReader( image ) );

                    return pdbFile;
                }
            }
            catch
            {
            }

            return null;
        }
    }
}

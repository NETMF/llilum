//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.Test;

    class MetaDataResolverTester :  Test, IMetaDataResolverHelper, ISymbolResolverHelper
    {
        //
        // State
        //

        private string baseFile;

        override public Result Run( string[] args )
        {
            base.Run(args);

            MetaDataResolverTester pThis;
            
            string currentDir = System.Environment.CurrentDirectory;

            pThis = new MetaDataResolverTester();
            pThis.LoadAndResolve( Expand( currentDir + @"\Test\TestPayload__CLR1_1__VanillaSingleClass.dll"        ), fDump: true );
            pThis.LoadAndResolve( Expand(              @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll" ), fDump : true );

            return Result.Success;
        }

        static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        private MetaDataResolver LoadAndResolve( string file  ,
                                                 bool   fDump )
        {
            this.baseFile = file;

            Importer.MetaData md = LoadAssembly( file );

            MetaDataResolver resolver = new Zelig.MetaData.MetaDataResolver( this );

            resolver.Add( md );

            resolver.ResolveAll();

            //--//

            if(fDump)
            {
                DirectoryInfo di = Directory.CreateDirectory( md.Assembly.Name );

                string oldCD = Environment.CurrentDirectory;

                Environment.CurrentDirectory = di.FullName;

                foreach(Normalized.MetaDataAssembly asmlNormalized in resolver.NormalizedAssemblies)
                {
                    using(MetaDataDumper writer = new MetaDataDumper( asmlNormalized.Name, asmlNormalized.Version ))
                    {
                        writer.Process( asmlNormalized, true );
                    }
                }

                Environment.CurrentDirectory = oldCD;
            }

            return resolver;
        }

        private Importer.MetaData LoadAssembly( string file )
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

////            if(System.IO.Path.GetFileNameWithoutExtension( file ) == "mscorlib")
////            {
////                file = @"z:\VS_Symbols_Cache\mscorlib.pdb\06A826D298CB41A3B63D7BDACDDAF9321\mscorlib.pdb";
////            }

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

        Importer.MetaData IMetaDataResolverHelper.ResolveAssemblyReference( string          name ,
                                                                            MetaDataVersion ver  )
        {
            Importer.MetaData md;

            md = CheckAndLoad( System.IO.Path.GetDirectoryName( baseFile ), name, ver );
            if(md != null)
            {
                return md;
            }

            //md = CheckAndLoad( Expand( @"%WINDIR%\Microsoft.NET\Framework\v1.1.4322" ), name, ver );
            md = CheckAndLoad( Expand( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727" ), name, ver );
            if(md != null)
            {
                return md;
            }

            return null;
        }

        Importer.MetaData CheckAndLoad( string          dir  ,
                                        string          name ,
                                        MetaDataVersion ver  )
        {
            string file = dir + @"\" + name + ".dll";

            if(System.IO.File.Exists( file ))
            {
                try
                {
                    Importer.MetaData md = LoadAssembly( file );

                    if(md.Assembly.Name == name && md.Assembly.Version.IsCompatible( ver, false ))
                    {
                        return md;
                    }
                }
                catch
                {
                }
            }

            return null;
        }
    }
}

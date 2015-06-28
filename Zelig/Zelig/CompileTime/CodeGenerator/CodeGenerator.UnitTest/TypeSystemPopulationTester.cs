//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    using              Microsoft.Zelig.MetaData;
    using Importer   = Microsoft.Zelig.MetaData.Importer;
    using Normalized = Microsoft.Zelig.MetaData.Normalized;
    using IR         = Microsoft.Zelig.CodeGeneration.IR;
    using TS         = Microsoft.Zelig.Runtime.TypeSystem;

    using ARM        = Microsoft.Zelig.Emulation.ArmProcessor;

    class TypeSystemPopulationTester : BaseTester,
        Microsoft.Zelig.MetaData.IMetaDataResolverHelper,
        Microsoft.Zelig.MetaData.ISymbolResolverHelper
    {
        //
        // Constructor Methods
        //

        private TypeSystemPopulationTester()
        {
            m_symbolHelper           = this;
            m_metaDataResolverHelper = this;

            Initialize();

            //AddSearchDirectory( @"%WINDIR%\Microsoft.NET\Framework\v1.1.4322" );
            //AddSearchDirectory( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727" );
        }

        public static void Run( string[] args )
        {
            DateTime start = DateTime.Now;

            Run( BuildRoot + @"\Target\bin\" + BuildFlavor + @"\mscorlib_unittest.exe"              , true  );
          //Run( BuildRoot + @"\Host\bin\" + BuildFlavor + @"\Microsoft.Zelig.MetaData.UnitTest.exe", false );
          //Run( BuildRoot + @"\Host\bin\" + BuildFlavor + @"\Microsoft.Zelig.MetaData.UnitTest.exe", false );

            DateTime stop = DateTime.Now;

            Console.WriteLine( "Execution time: {0}", (stop - start) );
        }

        private static void Run( string file            ,
                                 bool   fDumpTypeSystem )
        {
            TypeSystemPopulationTester pThis;

            pThis = new TypeSystemPopulationTester();
            pThis.LoadAndResolve( file );

            pThis.ConvertToIR();

            string root       = Expand( @"%DEPOTROOT%\ZeligUnitTestResults\" );
            string file2      = Path.GetFileNameWithoutExtension( file );
            string filePrefix = root + file2;

            if(fDumpTypeSystem)
            {
                using(System.IO.TextWriter writer = new System.IO.StreamWriter( filePrefix + ".TypeSystemDump.IrTxt", false, System.Text.Encoding.ASCII ))
                {
                    foreach(TS.TypeRepresentation td in pThis.m_typeSystem.Types)
                    {
                        writer.WriteLine( "Type: {0}", td );

                        foreach(TS.TypeRepresentation itf in td.Interfaces)
                        {
                            writer.WriteLine( "  Interface: {0}", itf );
                        }

                        foreach(TS.FieldRepresentation fd in td.Fields)
                        {
                            writer.WriteLine( "  Field: {0}", fd );
                        }

                        foreach(TS.MethodRepresentation md in td.Methods)
                        {
                            writer.WriteLine( "  Method: {0}", md );
                        }

                        writer.WriteLine();
                    }
                }
            }

            //--//

            System.IO.MemoryStream msSerialized = new System.IO.MemoryStream();
            using(RedirectOutput ro = new RedirectOutput( filePrefix + ".serializer.txt" ))
            {
                IR.TypeSystemSerializer.Serialize( msSerialized, pThis.m_typeSystem );
            }
    
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream( ms, System.IO.Compression.CompressionMode.Compress );

            byte[] buf = msSerialized.ToArray();
            gzip.Write( buf, 0, buf.Length );
            gzip.Flush();

            byte[] bufCompressed = ms.ToArray();

            TypeSystemForUnitTest typeSystem2;
            using(RedirectOutput ro = new RedirectOutput( filePrefix + ".deserializer.txt" ))
            {
                typeSystem2 = (TypeSystemForUnitTest)IR.TypeSystemSerializer.Deserialize( new System.IO.MemoryStream( buf ), pThis.CreateInstanceForType, null, 0 );
            }

            System.IO.MemoryStream msSerialized2 = new System.IO.MemoryStream();
            using(RedirectOutput ro = new RedirectOutput( filePrefix + ".reserializer.txt" ))
            {
                IR.TypeSystemSerializer.Serialize( msSerialized2, typeSystem2 );
            }

            byte[] buf2 = msSerialized2.ToArray();

            if(ArrayUtility.ArrayEquals( buf, buf2 ))
            {
                Console.WriteLine( "{0}: Type System Serialization PASS"              , DateTime.Now                                   );
                Console.WriteLine( "{0}: Serialized size = {1}, compressed size = {2}", DateTime.Now, buf.Length, bufCompressed.Length );
            }
            else
            {
                Console.WriteLine( "{0}: Type System Serialization FAIL", DateTime.Now );
            }
        }

        private object CreateInstanceForType( Type t )
        {
            if(t == typeof(IR.TypeSystemForCodeTransformation))
            {
                return new TypeSystemForUnitTest( this, null );
            }

            return null;
        }

        //--//

        Importer.PdbInfo.PdbFile ISymbolResolverHelper.ResolveAssemblySymbols( string file )
        {
            return InnerResolveAssemblySymbols( file );
        }

        Importer.MetaData IMetaDataResolverHelper.ResolveAssemblyReference( string          name ,
                                                                            MetaDataVersion ver  )
        {
            return InnerResolveAssemblyReference( name, ver );
        }
    }
}

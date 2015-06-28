//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_DUMP_ALL_PHASES_AFTER_TYPE_SYSTEM_REDUCTION
//#define DEBUG_DUMP_ALL_PHASES
#define DEBUG_DUMP_XML
//#define REPORT_OPERATORS_AT_EACH_PHASE
//#define CLONE_STATE
//#define USE_IMOTE_PLATFORM

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

    class CodeGenerationTester : BaseTester,
        Microsoft.Zelig.MetaData.IMetaDataResolverHelper,
        Microsoft.Zelig.MetaData.ISymbolResolverHelper
    {
        //
        // State
        //

        private string m_filePrefix;
        private bool   m_fDumpIRToFile;
        private int    m_dumpCount;

        //
        // Constructor Methods
        //

        private CodeGenerationTester()
        {
            m_symbolHelper           = this;
            m_metaDataResolverHelper = this;

            Initialize();
        }

        public static void Run( string[] args )
        {
            string[] files =
            {
#if USE_IMOTE_PLATFORM
                BuildRoot + @"\Target\bin\" + BuildFlavor + @"\Microsoft.iMote2.dll",
                BuildRoot + @"\Target\bin\" + BuildFlavor + @"\Microsoft.DeviceModels.ModelForPXA27x.dll",
                BuildRoot + @"\Target\bin\" + BuildFlavor + @"\QuickTest.exe",
#else
                BuildRoot + @"\Target\bin\" + BuildFlavor + @"\mscorlib_unittest.exe",
                BuildRoot + @"\Target\bin\" + BuildFlavor + @"\Microsoft.VoxSoloFormFactor.dll",
#endif
            };

            CodeGenerationTester pThis = new CodeGenerationTester();

            pThis.Run( files, true, true );

            Console.WriteLine( "Execution time: {0}", pThis.GetTime() );
        }

        private void Run( string[] files         ,
                          bool     fDumpIRToFile ,
                          bool     fSerialize    )
        {
            string root       = Expand( @"%DEPOTROOT%\ZeligUnitTestResults\" );
            string file2      = Path.GetFileNameWithoutExtension( files[0] );
            string filePrefix = root + file2;

            m_filePrefix    = filePrefix;
            m_fDumpIRToFile = fDumpIRToFile;

            foreach(string file in files)
            {
                LoadAndResolve( file );
            }

            ConvertToIR();

#if CLONE_STATE
            Console.WriteLine( "{0}: Cloning State", GetTime() );
            byte[] buf;
            using(RedirectOutput ro = new RedirectOutput( filePrefix + ".serializer.txt" ))
            {
                buf = IR.TypeSystemSerializer.Serialize( m_typeSystem );
            }

            TypeSystemForUnitTest typeSystem2;
            using(RedirectOutput ro = new RedirectOutput( filePrefix + ".deserializer.txt" ))
            {
                typeSystem2 = (TypeSystemForUnitTest)IR.TypeSystemSerializer.Deserialize( new System.IO.MemoryStream( buf ), CreateInstanceForType );
            }

            TypeSystemForUnitTest typeSystem3 = m_typeSystem;
            m_typeSystem = typeSystem2;
            Console.WriteLine( "{0}: Done", GetTime() );
#endif

            Console.WriteLine( "{0}: ExecuteSteps", GetTime() );
            ExecuteSteps();
            Console.WriteLine( "{0}: Done", GetTime() );

            //--//

            if(fSerialize)
            {
                Console.WriteLine( "{0}: Type System Serialization Test...", GetTime() );

                System.IO.MemoryStream stream = new System.IO.MemoryStream();

                using(RedirectOutput ro = new RedirectOutput( filePrefix + ".serializer.txt" ))
                {
                    IR.TypeSystemSerializer.Serialize( stream, m_typeSystem );
                }

                stream.Seek( 0, SeekOrigin.Begin );

                TypeSystemForUnitTest deserializedTypeSystem;
                
                using(RedirectOutput ro = new RedirectOutput( filePrefix + ".deserializer.txt" ))
                {
                    deserializedTypeSystem = (TypeSystemForUnitTest)IR.TypeSystemSerializer.Deserialize( stream, CreateInstanceForType, null, 0 );
                }

                System.IO.MemoryStream stream2 = new System.IO.MemoryStream();
                
                using(RedirectOutput ro = new RedirectOutput( filePrefix + ".reserializer.txt" ))
                {
                    IR.TypeSystemSerializer.Serialize( stream2, deserializedTypeSystem );
                }

                if(ArrayUtility.ArrayEquals( stream.ToArray(), stream2.ToArray() ))
                {
                    Console.WriteLine( "{0}: Type System Serialization PASS", GetTime() );
                }
                else
                {
                    Console.WriteLine( "{0}: Type System Serialization FAIL", GetTime() );
                }
            }
        }

        //--//

        Importer.PdbInfo.PdbFile ISymbolResolverHelper.ResolveAssemblySymbols( string file )
        {
            return InnerResolveAssemblySymbols( file );
        }

        Importer.MetaData IMetaDataResolverHelper.ResolveAssemblyReference( string          name ,
                                                                            MetaDataVersion ver  )
        {
            //
            // Force use of our version of mscorlib.
            //
            if(name == "mscorlib" || name == "system")
            {
                return CheckAndLoad( BuildRoot + @"\Target\bin\" + BuildFlavor, name );
            }

            return InnerResolveAssemblyReference( name, ver );
        }

        //--//

        private object CreateInstanceForType( Type t )
        {
            if(t == typeof(IR.TypeSystemForCodeTransformation))
            {
                return new TypeSystemForUnitTest( this, null );
            }

            return null;
        }

        private void DumpPerformanceCounters( string filePrefix )
        {
            if(PerformanceCounters.ContextualTiming.IsEnabled())
            {
                Console.WriteLine( "{0}: Dumping Performance Counters", GetTime() );

                using(System.IO.StreamWriter output = new System.IO.StreamWriter( filePrefix + "_timing.type.txt", false, System.Text.Encoding.ASCII ))
                {
                    PerformanceCounters.ContextualTiming.DumpAllByType( output );
                }

                using(System.IO.StreamWriter output = new System.IO.StreamWriter( filePrefix + "_timing.reason.txt", false, System.Text.Encoding.ASCII ))
                {
                    PerformanceCounters.ContextualTiming.DumpAllByReason( output );
                }

                Console.WriteLine( "{0}: Done", GetTime() );
            }
        }

        //--//

        protected override void NotifyCompilationPhase( IR.CompilationSteps.PhaseDriver phase )
        {
            if(!m_fDumpIRToFile) return;

            bool fDumpIR  = false;
            bool fDumpTXT = false;
#if DEBUG_DUMP_XML
            bool fDumpXML = true;
#else
            bool fDumpXML = false;
#endif
            bool fDumpOps = false;
            bool fDumpASM = false;
            bool fDumpIMG = false;

            ++m_dumpCount;

            Console.WriteLine( "{0}: Phase: {1}", GetTime(), phase );

#if REPORT_OPERATORS_AT_EACH_PHASE
            fDumpOps = true;
#endif

////        if(phase >= IR.CompilationSteps.Phase.ConvertToSSA)
////        {
////            fDumpTXT = true;
////        }

            if(phase is IR.CompilationSteps.Phases.GenerateImage)
            {
                DumpPerformanceCounters( m_filePrefix );
                fDumpTXT = true;
            }
            else if(phase is IR.CompilationSteps.Phases.Done)
            {
                fDumpIMG = true;
                fDumpASM = true;
                fDumpTXT = true;
                fDumpIR  = true; m_typeSystem.DropCompileTimeObjects(); // Get the smallest dump.
            }

            //--//

#if DEBUG_DUMP_ALL_PHASES
            fDumpTXT = true;
#endif

#if DEBUG_DUMP_ALL_PHASES_AFTER_TYPE_SYSTEM_REDUCTION
            if(IR.CompilationSteps.PhaseDriver.CompareOrder( phase, typeof(IR.CompilationSteps.Phases.ReduceTypeSystem) ) > 0)
            {
                fDumpTXT = true;
            }
#endif

            //--//

////        if(phase is IR.CompilationSteps.Phases.LayoutTypes)
////        {
////            fDumpOps = true;
////        }
////        else if(phase is IR.CompilationSteps.Phases.GenerateImage ||
////                phase is IR.CompilationSteps.Phases.Done           )
////        {
////            fDumpOps = true;
////        }

            if(fDumpOps)
            {
                ReportOperators( phase );
            }

            if(fDumpIR || fDumpTXT || fDumpXML)
            {
                string file = string.Format( "{0}.{1:X2}.{2}.{3}.", m_filePrefix, m_dumpCount, phase.PhaseIndex, phase );

                Console.WriteLine( "{0}:   Dump {1}", GetTime(), file );

                if(fDumpIR ) SaveIrToDisk( file + "ZeligImage", m_typeSystem    );
                if(fDumpTXT) DumpIRAsText( file + "ZeligIR"                     );
                if(fDumpXML) DumpIRAsXML ( file + "xml"       , GetAllMethods() );

                if(fDumpASM)
                {
                    m_typeSystem.DisassembleImage( file + "asmdir" );
                }

                if(fDumpIMG)
                {
                    using(FileStream stream = new FileStream( file + "hex", FileMode.Create ))
                    {
                        foreach(var section in m_typeSystem.Image)
                        {
                            ARM.SRecordParser.Encode( stream, section.Payload, section.Address );
                        }
                    }
                }

                Console.WriteLine( "{0}:   Done", GetTime() );
            }
        }

        private void ReportOperators( IR.CompilationSteps.PhaseDriver phase )
        {
            GrowOnlySet< Type > types = SetFactory.NewWithReferenceEquality< Type >();

            m_typeSystem.EnumerateFlowGraphs( delegate( IR.ControlFlowGraphStateForCodeTransformation cfg )
            {
                foreach(IR.Operator op in cfg.DataFlow_SpanningTree_Operators)
                {
                    types.Insert( op.GetType() );
                }
            } );

            Console.WriteLine( "Operators at phase {0}", phase );
            foreach(Type t in types)
            {
                Console.WriteLine( "    {0}", t.Name );
            }
        }

        //--//

        private List< TS.MethodRepresentation > GetAllMethods()
        {
            List< TS.MethodRepresentation > lst = new List< TS.MethodRepresentation >();

            m_typeSystem.EnumerateMethods( delegate( TS.MethodRepresentation md )
            {
                lst.Add( md );
            } );

            return lst;
        }

        private List< TS.MethodRepresentation > GetAllReferencedMethods()
        {
            List< TS.MethodRepresentation > lst = new List< TS.MethodRepresentation >();

            foreach(TS.MethodRepresentation md in m_controller.EntitiesReferencedByMethods.Keys)
            {
                lst.Add( md );
            }

            return lst;
        }
    }
}

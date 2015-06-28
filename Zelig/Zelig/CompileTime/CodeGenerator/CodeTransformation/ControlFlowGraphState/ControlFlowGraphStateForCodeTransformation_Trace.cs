//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


//#define TRACE_EVERY_IR_CHANGE
//#define TRACE_EVERY_SINGLE_IR_CHANGE

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation : ControlFlowGraphState
    {
        //
        // Debug Methods
        //

        public void Dump()
        {
            using(var ird = new IR.TextIntermediateRepresentationDumper())
            {
                ird.WriteLine( "#################################################################################" );
                ird.WriteLine( "#################################################################################" );
                ird.WriteLine( "#################################################################################" );
                this.Dump( ird );
            }
        }

        public void DumpToFile( string file )
        {
            using(var ird = new IR.TextIntermediateRepresentationDumper( file ))
            {
                ird.WriteLine( "Method: {0}", this.m_md );
                ird.WriteLine(                          );
                this.Dump( ird );
            }
        }

        public void DumpToStream( System.IO.StreamWriter output )
        {
            using(var ird = new IR.TextIntermediateRepresentationDumper( output ))
            {
                ird.WriteLine( "Method: {0}", this.m_md );
                ird.WriteLine(                          );
                this.Dump( ird );
                ird.WriteLine();
            }
        }

#if TRACE_EVERY_IR_CHANGE
        static int    s_count;
        static int    s_dumpCount;
        static int    s_lastCount;
        static string s_lastNote = "InitialState";
        static byte[] s_lastDump;
        static string s_directory;
        static string s_phase = "InitialPhase";
        static int    s_phase_count;

        internal static void SetPhaseForTrace( CompilationSteps.PhaseDriver driver )
        {
            s_phase = string.Format( "{0:X4}_{1}", ++s_phase_count, driver.GetType().Name );
        }

        internal void TraceToFile( Delegate dlg )
        {
            TraceToFile( dlg.Method.Name );
        }

        internal void TraceToFile( string note )
        {
            const string constText = 
////        "FlowGraph(int[] Microsoft.NohauLPC3180Loader.Loader::TestArrayBoundChecks(int[],int,int))";
////        "FlowGraph(void Microsoft.Zelig.Runtime.MemorySegment::Initialize())";
////        "FlowGraph(void System.Array::Copy(System.Array,System.Array,int))";
////        "FlowGraph(void Microsoft.Zelig.Runtime.MarkAndSweepCollector::InitializeGarbageCollectionManager())";
////        "FlowGraph(int Microsoft.Zelig.Runtime.Helpers.BinaryOperations::IntDiv(int,int))";
////        "FlowGraph(int[] Microsoft.NohauLPC3180Loader.Loader::TestArrayBoundChecks(int[],int,int))";
////        "FlowGraph(uint Microsoft.NohauLPC3180Loader.Loader::Sqrt(uint))";
////        "FlowGraph(void System.Collections.Generic.ArraySortHelper`1<System.Int32>::QuickSort<object>(int[],object[],int,int,System.Collections.Generic.IComparer`1<int>))";
////        "FlowGraph(void mscorlib_UnitTest.Program::TestBitField())";
////        "FlowGraph(void mscorlib_UnitTest.Program::Main())";
////        "FlowGraph(string System.Number::ToStringFormat(string,System.Globalization.NumberFormatInfo))";
////        "FlowGraph(void Microsoft.Zelig.Runtime.ThreadManager::AddThread(System.Threading.Thread))";
////        "FlowGraph(bool Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.ProcessorARMv4.Context::Unwind())";
            "FlowGraph(void Microsoft.Zelig.Runtime.Bootstrap::EntryPoint())";

            if(this.ToString() != constText)
            {
                return;
            }
    
            s_count++;

////        if(s_count >= 0x1C1)
////        {
////        }


            var memStream = new System.IO.MemoryStream();
            var writer    = new System.IO.StreamWriter( memStream );

            using(var ird = new TextIntermediateRepresentationDumper( writer ))
            {
                ird.DumpGraph( this );
            }

            writer.Flush();

            byte[] dump = memStream.ToArray();

#if !TRACE_EVERY_SINGLE_IR_CHANGE
            if(ArrayUtility.ByteArrayEquals( dump, s_lastDump ) == false)
#endif
            {
                string filePrefix = Environment.ExpandEnvironmentVariables( @"%DEPOTROOT%\ZeligUnitTestResults\Trace" );

                if(s_directory == null)
                {
                    s_directory = string.Format( "{0}_{1:yyyy-MM-dd--HH-mm-ss}", filePrefix, DateTime.Now );
                }

#if TRACE_EVERY_SINGLE_IR_CHANGE
                string file = string.Format( @"{0}\{1}\{2:X4}__{3:X4}_{4}", s_directory, s_phase, s_dumpCount++, s_count, note );
#else
                string file = string.Format( @"{0}\{1}\{2:X4}__{3:X4}_{4}", s_directory, s_phase, s_dumpCount++, s_lastCount, s_lastNote );
#endif
        
                file = file.Replace( ' ', '_' );
                file = file.Replace( '<', '_' );
                file = file.Replace( '>', '_' );

////            if(note.StartsWith( "ConvertInto-" ))
////            {
////                TraceAsXml( file );
////    
////                TraceAsIR( file );
////            }

                System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName( file ) );

                using(System.IO.FileStream fileStream = new System.IO.FileStream( file + ".txt", System.IO.FileMode.Create, System.IO.FileAccess.Write ) )
                {
                    fileStream.Write( dump, 0, dump.Length );
                }

                s_lastDump  = dump;
            }

            s_lastCount = s_count;
            s_lastNote  = note;
        }

        private void TraceAsXml( string file )
        {
            var doc  = new System.Xml.XmlDocument();
            var node = XmlHelper.AddElement( doc, "Methods" );

            var ird = new IR.XmlIntermediateRepresentationDumper( doc, node );

            this.Dump( ird );

            doc.Save( file + ".xml" );
        }

        private void TraceAsIR( string file )
        {
            using(var stream = new System.IO.FileStream( file + ".ZeligImage", System.IO.FileMode.Create ))
            {
                IR.TypeSystemSerializer.Serialize( stream, this.TypeSystem );
            }
        }

#else

        internal static void SetPhaseForTrace( CompilationSteps.PhaseDriver driver )
        {
        }

        internal void TraceToFile( Delegate dlg )
        {
        }

        internal void TraceToFile( string note )
        {
        }

#endif
    }
}

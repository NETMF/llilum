//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LIVE
//#define REDIRECT_OUTPUT
//#define DUMP_SINGLE_METHOD
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
    using RT         = Microsoft.Zelig.Runtime;
    using TS         = Microsoft.Zelig.Runtime.TypeSystem;


#if REDIRECT_OUTPUT
    internal class RedirectOutput : IDisposable
    {
        System.IO.TextWriter m_orig;
        System.IO.TextWriter m_writer;

        internal RedirectOutput( string file )
        {
            m_orig = Console.Out;

            m_writer = new System.IO.StreamWriter( file, false, System.Text.Encoding.ASCII );
            Console.SetOut( m_writer );
        }

        public void Dispose()
        {
            Console.SetOut( m_orig );
            m_writer.Close();
        }
    }
#else
    internal class RedirectOutput : IDisposable
    {
        internal RedirectOutput( string file )
        {
        }

        public void Dispose()
        {
        }
    }
#endif

    public abstract class BaseTester :
        TS.IEnvironmentProvider
    {
        public class TypeSystemForUnitTest : IR.TypeSystemForCodeTransformation
        {
            //
            // State
            //

            BaseTester m_owner;

            //
            // Constructor Methods
            //

            public TypeSystemForUnitTest( BaseTester              owner ,
                                          TS.IEnvironmentProvider env   ) : base( env )
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            protected override void NotifyCompilationPhase( IR.CompilationSteps.PhaseDriver phase )
            {
                m_owner.NotifyCompilationPhase( phase );
            }
        }

#if LIVE
        protected static string BuildRoot = Expand( @"%DEPOTROOT%\ZeligBuild" );
#else
        protected static string BuildRoot = Expand( @"%DEPOTROOT%\ZeligBuild.Reference" );
#endif

#if DEBUG
#if DEBUG_INSTRUMENTATION
        protected static string BuildFlavor = "Instrumentation";
#else
        protected static string BuildFlavor = "Debug";
#endif
#else
        protected static string BuildFlavor = "Release";
#endif

        //
        // State
        //

        protected List<string>                        m_searchOrder;
        protected string                              m_baseFile;
                                                
        protected IR.CompilationSteps.DelegationCache m_delegationCache;
        protected MetaDataResolver                    m_resolver;
        protected TypeSystemForUnitTest               m_typeSystem;
        protected IR.CompilationSteps.Controller      m_controller;
                                                 
        protected IMetaDataResolverHelper             m_metaDataResolverHelper;
        protected ISymbolResolverHelper               m_symbolHelper;

        private   PerformanceCounters.Timing          m_timing;

        //
        // Constructor Methods
        //

        protected BaseTester()
        {
            m_timing.Start();
        }

        //--//

        protected string GetTime()
        {
            return string.Format( "{0,13:F6}", (float)PerformanceCounters.Timing.ToMicroSeconds( m_timing.Sample() ) / (1000 * 1000) );
        }

        protected void Initialize()
        {
            m_searchOrder = new List<string>();
            m_resolver    = new Zelig.MetaData.MetaDataResolver( m_metaDataResolverHelper );

            AddSearchDirectory( BuildRoot + @"\Target\bin\" + BuildFlavor );
            AddSearchDirectory( BuildRoot + @"\Host\bin\"   + BuildFlavor );

            //
            // We need this assembly, for all the extra stuff about open classes.
            //
            LoadAndResolve( BuildRoot + @"\Host\bin\" + BuildFlavor + @"\Microsoft.Zelig.Runtime.dll" );
        }

        protected void CreateTypeSystem()
        {
            m_typeSystem      = new TypeSystemForUnitTest( this, this );
            m_delegationCache = new IR.CompilationSteps.DelegationCache( m_typeSystem );

            m_typeSystem.InitializeForCompilation();

            //--//

            var pa = new Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV4( m_typeSystem, null );

            m_typeSystem.PlatformAbstraction = pa;

            uint RamSize         = 768 * 1024;
            uint StartOfRam      = 0x00000000;
            uint EndOfRam        = StartOfRam + RamSize;
#if USE_IMOTE_PLATFORM
            uint SDRAMSize = 32 * 1024 * 1024;
            uint StartOfSDRAM    = 0xA0000000;
            uint EndOfSDRAM      = StartOfSDRAM + SDRAMSize;
            uint StartOfFlash    = 0x02000000;
            uint FlashSize       = 32 * 1024 * 1024;
#else
            uint StartOfFlash    = 0x90000000;
            uint FlashSize       = 8 * 1024 * 1024;
#endif
            uint EndOfFlash      = StartOfFlash + FlashSize;
            uint FlashEntryPoint = StartOfFlash;

            UIntPtr        ptrRamStart        = new UIntPtr( StartOfRam                      );
            UIntPtr        ptrRamEnd          = new UIntPtr( EndOfRam                        );
#if USE_IMOTE_PLATFORM
            UIntPtr        ptrSDRAMStart      = new UIntPtr( StartOfSDRAM                    );
            UIntPtr        ptrSDRAMEnd        = new UIntPtr( EndOfSDRAM                      );
#endif
            UIntPtr        ptrFlashStart      = new UIntPtr( StartOfFlash                    );
            UIntPtr        ptrFlashEnd        = new UIntPtr( EndOfFlash                      );
            UIntPtr        ptrFlashEntryPoint = new UIntPtr( FlashEntryPoint                 );
            RT.MemoryUsage codeDataRO         = RT.MemoryUsage.Code | RT.MemoryUsage.DataRO | RT.MemoryUsage.Bootstrap;
            RT.MemoryUsage anything           = RT.MemoryUsage.Code | RT.MemoryUsage.DataRO | RT.MemoryUsage.DataRW | RT.MemoryUsage.VectorsTable;

            pa.AddMemoryBlock( new RT.Memory.Range( ptrRamStart       , ptrRamEnd  , null, RT.MemoryAttributes.RAM                                               , anything                              , null ) );
            pa.AddMemoryBlock( new RT.Memory.Range( ptrFlashEntryPoint, ptrFlashEnd, null, RT.MemoryAttributes.FLASH | RT.MemoryAttributes.ConfiguredAtEntryPoint, codeDataRO | RT.MemoryUsage.Relocation, null ) );
#if USE_IMOTE_PLATFORM
            pa.AddMemoryBlock( new RT.Memory.Range( ptrSDRAMStart     , ptrSDRAMEnd, null, RT.MemoryAttributes.RAM                                               , anything                              , null ) );  
#endif

            //--//

            var cc = new Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention( m_typeSystem );

            m_typeSystem.CallingConvention = cc;
        }


        protected void AddSearchDirectory( string dir )
        {
            dir = Expand( dir );

            if(m_searchOrder.Contains( dir ) == false)
            {
                m_searchOrder.Add( dir );
            }
        }

        protected void ConvertAllTypes()
        {
            foreach(Normalized.MetaDataAssembly asml in m_resolver.NormalizedAssemblies)
            {
                m_typeSystem.ImportAssembly( asml );
            }
        }

        protected void ResolveAll()
        {
            m_typeSystem.ResolveAll();
        }

        protected void ExecuteSteps()
        {
            m_controller = new IR.CompilationSteps.Controller( m_typeSystem );

            m_controller.ExecuteSteps();
        }

        protected void ConvertToIR()
        {
            Console.WriteLine( "{0}: ConvertToIR", GetTime() );
            CreateTypeSystem();

            ConvertAllTypes();

            Console.WriteLine( "{0}: Done", GetTime() );

            Console.WriteLine( "{0}: ResolveAll", GetTime() );
            ResolveAll();
            Console.WriteLine( "{0}: Done", GetTime() );
        }

        protected virtual void NotifyCompilationPhase( IR.CompilationSteps.PhaseDriver phase )
        {
        }

        //--//

        object TS.IEnvironmentProvider.GetService( Type t )
        {
            if(t == typeof(Normalized.IMetaDataBootstrap))
            {
                return m_resolver;
            }

            if(t == typeof(IR.CompilationSteps.DelegationCache))
            {
                return m_delegationCache;
            }

            return null;
        }

        //--//

        protected static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        protected void LoadAndResolve( string file )
        {
            if(m_baseFile == null)
            {
                m_baseFile = file;

                m_searchOrder.Insert( 0, System.IO.Path.GetDirectoryName( file ) );
            }

            Importer.MetaData md = LoadAssembly( file );

            m_resolver.Add( md );

            m_resolver.ResolveAll();
        }

        private Importer.MetaData LoadAssembly( string file )
        {
            byte[] image = System.IO.File.ReadAllBytes( file );

            Importer.PELoader pe = new Importer.PELoader( file, image );

            Importer.MetaData md = Importer.MetaData.loadMetaData( file, m_symbolHelper, pe, Importer.MetaDataLoaderFlags.LoadCode | Importer.MetaDataLoaderFlags.LoadDebugInfo );

            return md;
        }

        //--//

        protected Importer.PdbInfo.PdbFile InnerResolveAssemblySymbols( string file )
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

        protected Importer.MetaData InnerResolveAssemblyReference( string          name ,
                                                                   MetaDataVersion ver  )
        {
            Importer.MetaData md;

            foreach(string dir in m_searchOrder)
            {
                md = CheckAndLoad( dir, name, ver );
                if(md != null)
                {
                    return md;
                }
            }

            return null;
        }

        //--//

        protected Importer.MetaData CheckAndLoad( string dir  ,
                                                  string name )
        {
            string file = dir + @"\" + name + ".dll";

            if(System.IO.File.Exists( file ))
            {
                try
                {
                    return LoadAssembly( file );
                }
                catch
                {
                }
            }

            return null;
        }

        protected Importer.MetaData CheckAndLoad( string          dir  ,
                                                  string          name ,
                                                  MetaDataVersion ver  )
        {
            Importer.MetaData md = CheckAndLoad( dir, name );

            if(md != null)
            {
                if(md.Assembly.Name == name && md.Assembly.Version.IsCompatible( ver, false ))
                {
                    return md;
                }
            }

            return null;
        }

        //--//--//

        protected void SaveIrToDisk( string                             file       ,
                                     IR.TypeSystemForCodeTransformation typeSystem )
        {
            using(System.IO.FileStream stream = new System.IO.FileStream( file, FileMode.Create ))
            {
                IR.TypeSystemSerializer.Serialize( stream, typeSystem );
            }
        }

        protected IR.TypeSystemForCodeTransformation LoadIrFromDisk( string                                 file     ,
                                                                     IR.TypeSystemSerializer.CreateInstance callback )
        {
            using(System.IO.FileStream stream = new System.IO.FileStream( file, FileMode.Open ))
            {
                return IR.TypeSystemSerializer.Deserialize( stream, callback, null, 0 );
            }
        }

        //--//

        protected void DumpIRAsText( string file )
        {
            using(IR.TextIntermediateRepresentationDumper ird = new IR.TextIntermediateRepresentationDumper( file ))
            {
                foreach(TS.TypeRepresentation td in m_typeSystem.Types)
                {
#if !DUMP_SINGLE_METHOD
                    ird.WriteLine( "###############################################################################" );
                    ird.WriteLine();
                    ird.WriteLine( "Type: {0} [Size={1}]", td, td.ValidLayout ? td.Size : uint.MaxValue );
    
                    if(td.Extends != null)
                    {
                        ird.WriteLine( "  Extends: {0}", td.Extends );
                    }
    
                    foreach(TS.TypeRepresentation itf in td.Interfaces)
                    {
                        ird.WriteLine( "  Interface: {0}", itf );
                    }
    
                    foreach(TS.FieldRepresentation fd in td.Fields)
                    {
                        ird.WriteLine( "  Field: {0} [Offset={1}]", fd, fd.ValidLayout ? fd.Offset : -1 );
                    }
    
                    ird.WriteLine();
#endif

                    foreach(TS.MethodRepresentation md in td.Methods)
                    {
                        IR.ControlFlowGraphState cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( md );

                        if(cfg != null)
                        {
#if DUMP_SINGLE_METHOD
                            //if(cfg.Method.ToString() != "<method>")
                            {
                                continue;
                            }
#endif

                            ird.WriteLine( "  Method {0}", md );
                            ird.WriteLine(                    );

                            cfg.Dump( ird );
                        }
#if !DUMP_SINGLE_METHOD
                        else
                        {
                            ird.WriteLine( "  NoCodeMethod {0}", md );
                        }
    
                        ird.WriteLine();
#endif
                    }
                }
            }
        }


        protected void DumpIRAsText( string                                 file       ,
                                     IEnumerable< TS.MethodRepresentation > enumerable )
        {
            using(IR.TextIntermediateRepresentationDumper ird = new IR.TextIntermediateRepresentationDumper( file ))
            {
                foreach(TS.MethodRepresentation md in ird.Sort( enumerable ))
                {
                    IR.ControlFlowGraphState cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( md );

                    if(cfg != null)
                    {
                        ird.WriteLine( "Method {0}", md );
                        ird.WriteLine(                  );

                        cfg.Dump( ird );
                    }
                    else
                    {
                        ird.WriteLine( "NoCodeMethod {0}", md );
                    }

                    ird.WriteLine();
                }
            }
        }

        protected void DumpIRAsXML( string                                 file       ,
                                    IEnumerable< TS.MethodRepresentation > enumerable )
        {
            System.Xml.XmlDocument doc  = new System.Xml.XmlDocument();
            System.Xml.XmlNode     node = XmlHelper.AddElement( doc, "Methods" );


            IR.XmlIntermediateRepresentationDumper ird = new IR.XmlIntermediateRepresentationDumper( doc, node );

            foreach(TS.MethodRepresentation md in ird.Sort( enumerable ))
            {
                IR.ControlFlowGraphState cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( md );

                if(cfg != null)
                {
                    cfg.Dump( ird );
                }
            }

            doc.Save( file );
        }
    }
}

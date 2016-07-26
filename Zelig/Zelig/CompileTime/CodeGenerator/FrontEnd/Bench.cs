//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_SAVE_STATE
//#define DEBUG_RELOAD_STATE



namespace Microsoft.Zelig.FrontEnd
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Win32;

    using Importer = Microsoft.Zelig.MetaData.Importer;
    using Normalized = Microsoft.Zelig.MetaData.Normalized;
    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg = Microsoft.Zelig.Configuration.Environment;
    using ARM = Microsoft.Zelig.Emulation.ArmProcessor;
    using ELF = Microsoft.Zelig.Elf;
    using LLVM;
    using CodeGeneration.IR.CompilationSteps;

    class Bench :
        TS.IEnvironmentProvider,
        TS.IConfigurationProvider,
        MetaData.IMetaDataResolverHelper,
        MetaData.ISymbolResolverHelper,
        IR.Transformations.ConstraintSystemCollector.IVisualizer
    {
        const string c_ZeligSourceCode = "/ZeligSourceCode";

        public class RawImage
        {
            public string SectionName;
            public uint   RangeStart;
            public uint   RangeEnd;
        }

        public class TypeSystemForFrontEnd : IR.TypeSystemForCodeTransformation
        {
            readonly Bench m_owner;

            public TypeSystemForFrontEnd( Bench owner,
                                          TS.IEnvironmentProvider env )
                : base( env )
            {
                m_owner = owner;
            }

            //--//

            protected override void NotifyCompilationPhase( IR.CompilationSteps.PhaseDriver phase )
            {
                m_owner.NotifyCompilationPhase( phase );
            }

            public override IR.SourceCodeTracker GetSourceCodeTracker( )
            {
                return m_owner.m_sourceCodeTracker;
            }
        }

        internal class LlvmCodeGenOptions 
            : IR.CompilationSteps.IInlineOptions
            , ITargetSectionOptions
        {
            internal LlvmCodeGenOptions( )
            {
                EnableAutoInlining = true;
                HonorInlineAttribute = true;
                InjectPrologAndEpilog = true;
            }

            public bool EnableAutoInlining { get; internal set; }

            public bool HonorInlineAttribute { get; internal set; }

            public bool InjectPrologAndEpilog { get; internal set; }

            public bool GenerateDataSectionPerType { get; internal set; }
        }

        //
        // State
        //

        const string SymbolSuffix             = ".pdb";
        const string SourceCodeDatabaseSuffix = ".srcdb";

        // Environment variables that may refer to the LLVM tools location
        static readonly string[] LlvmBinPathEnvVarNames = { "LLILUM_LLVM", "LLVM_BIN" };

        // Supported architectures
        const string c_CortexM0 = "cortex-m0";
        const string c_CortexM3 = "cortex-m3";
        const string c_CortexM4 = "cortex-m4";
        const string c_CortexM7 = "cortex-m7";
        const string c_x86_64   = "x86-64";

        // These are the default arguments to LLC.EXE if not specified in the command line arguments
        const string DefaultLlcArgs_common     = "-O2 -code-model=small -data-sections -filetype=obj";
        const string DefaultLlcArgs_target_m0  = "-march=thumb -mcpu=cortex-m0 -mtriple=thumbv6m-none-eabi";
        const string DefaultLlcArgs_target_m3  = "-march=thumb -mcpu=cortex-m3 -mtriple=thumbv7m-none-eabi";
        //const string DefaultLlcArgs_target_m4 = "-march=thumb -mcpu=cortex-m4 -mtriple=thumbv7m-none-eabi"; https://github.com/NETMF/llilum/issues/136
        const string DefaultLlcArgs_target_m4 = "-march=thumb -mcpu=cortex-m3 -mtriple=thumbv7m-none-eabi";
        const string DefaultLlcArgs_target_m7  = "-march=thumb -mcpu=cortex-m7 -mtriple=thumbv7m-none-eabi";
        const string DefaultLlcArgs_target_x86 = "-march=x86 -mcpu=x86-64 -mtriple=x86_64-pc-windows-msvc -function-sections -dwarf-version=3";
        const string DefaultLlcArgs_target_df  = DefaultLlcArgs_target_m3;
        const string DefaultLlcArgs_reloc      = "-relocation-model=pic";
        //--//
        const string DefaultOptExeArgs_common  = "-verify-debug-info -verify-dom-info -verify-each -verify-loop-info -verify-regalloc -verify-region-info -aa-eval -indvars -gvn -globaldce -adce -dce -tailcallopt -scalarrepl -mem2reg -ipconstprop -deadargelim -sccp -dce -ipsccp -dce -constmerge -scev-aa -targetlibinfo -irce -dse -dce -argpromotion -mem2reg -adce -mem2reg -globaldce -die -dce -dse";
        const string DefaultOptArgs_target_m0  = "-march=thumb -mcpu=cortex-m0";
        const string DefaultOptArgs_target_m3  = "-march=thumb -mcpu=cortex-m3";
        //const string DefaultOptArgs_target_m4 = "-march=thumb -mcpu=cortex-m4"; https://github.com/NETMF/llilum/issues/136
        const string DefaultOptArgs_target_m4  = "-march=thumb -mcpu=cortex-m3";
        const string DefaultOptArgs_target_m7  = "-march=thumb -mcpu=cortex-m7";
        const string DefaultOptArgs_target_x86 = "-march=x86 -mcpu=x86-64";
        const string DefaultOptArgs_target_df  = DefaultOptArgs_target_m3;

        const string LlvmRegSoftwareBinPath    = @"SOFTWARE\LLVM\3.8.0";

        //--//

        //
        // Bench is a root object in the system. This makes it easier to put any object in the watch window from the global hierarchy.
        //
        private static Bench                        s_pThis;

        private string                              m_outputName;
        private string                              m_outputDir;
        private string                              m_targetFile;
        private string                              m_entryPointName;
        
        private bool                                m_fReloadState;
        private bool                                m_fDumpIL;
        private bool                                m_fDumpIRpre;
        private bool                                m_fDumpIRpost;
        private bool                                m_fDumpIR;
        private bool                                m_fDumpFlattenedCallGraph;
        private bool                                m_fDumpIRXMLpre;
        private bool                                m_fDumpIRXMLpost;
        private bool                                m_fDumpIRXML;
        private bool                                m_fDumpIRBeforeEachPhase;
        private bool                                m_fDumpCFG;
        private bool                                m_fDumpLLVMIR;
        private bool                                m_fSkipLlvmOptExe;
        private bool                                m_fGenerateObj;
        private bool                                m_fDumpLLVMIR_TextRepresentation;
        private bool                                m_fDumpASM;
        private bool                                m_fDumpASMDIR;
        private bool                                m_fDumpHEX;
        private bool                                m_fSkipReadOnly;
        private uint                                m_nativeIntSize;
        private List< RawImage >                    m_dumpRawImage;

        private int                                 m_phaseExecutionCounter;

        private string                              m_libraryLocation_HostBuild;
        private string                              m_libraryLocation_TargetBuild;
        private string                              m_compilationSetupBinaryPath;
        private string                              m_LlvmBinPath;
        private string                              m_LlvmOptArgs;
        private string                              m_LlvmLlcArgs;

        private HashSet< string >                   m_phasesForDiagnosticDumps;
        private List< string >                      m_references;
        private List< string >                      m_searchOrder;
        private List< string >                      m_importDirectories;
        private List< string >                      m_importLibraries;

        private LlvmCodeGenOptions                  m_LlvmCodeGenOptions;
        private IR.CompilationSteps.DelegationCache m_delegationCache;
        private MetaData.MetaDataResolver           m_resolver;
        private TypeSystemForFrontEnd               m_typeSystem;
        private IR.CompilationSteps.Controller      m_controller;
        private Cfg.CompilationSetupCategory        m_compilationSetup;
        private GrowOnlyHashTable< string, object > m_configurationOptions;
        private List<String>                        m_disabledPhases;

        private Cfg.ProductCategory                 m_product;
        private Cfg.MemoryMapCategory               m_memoryMap;

        private IR.SourceCodeTracker                m_sourceCodeTracker;
        
        //
        // This parameter overrides any infereces from compilation setup
        // (e.g.: -CompilationSetup Microsoft.Llilum.BoardConfigurations.LPC1768MBEDCompilationSetup)
        // There is no reason for the compilation setup not to express all needed switches
        //
        private string                              m_architecture;

        //--//

        private Cfg.Manager                         m_configurationManager;
        private PerformanceCounters.Timing          m_timing;
        private long                                m_lastTiming;

        //
        // Constructor Methods
        //

        private Bench( )
        {
            m_timing.Start( );

#if DEBUG
#if DEBUG_INSTRUMENTATION
            Environment.SetEnvironmentVariable( "Flavor", "Instrumentation" );
#else
            Environment.SetEnvironmentVariable( "Flavor", "Debug" );
#endif
#else
            Environment.SetEnvironmentVariable( "Flavor", "Release" );
#endif

            //--//

            m_outputDir                 = ".";

            m_nativeIntSize             = 32;
            m_phaseExecutionCounter     = 0;

            m_dumpRawImage              = new List<RawImage>( );
            
            m_phasesForDiagnosticDumps  = new HashSet<string>( );
            m_references                = new List<string>( );
            m_searchOrder               = new List<string>( );
            m_importDirectories         = new List<string>( );
            m_importLibraries           = new List<string>( );
            
            m_resolver                  = new Zelig.MetaData.MetaDataResolver( this );

            m_configurationOptions      = HashTableFactory.New<string, object>( );

            m_disabledPhases            = new List<String>( );

            m_sourceCodeTracker         = new IR.SourceCodeTracker( );
            m_LlvmCodeGenOptions        = new LlvmCodeGenOptions( );
        }

        //--//

        private string GetTime( )
        {
            long newTiming  = m_timing.Sample( );
            long diffTiming = newTiming - m_lastTiming;

            m_lastTiming = newTiming;

            return string.Format( "{0,10:F3} [{1,8:F3}]", ( float )PerformanceCounters.Timing.ToMicroSeconds( newTiming ) / ( 1000 * 1000 ), ( float )PerformanceCounters.Timing.ToMicroSeconds( diffTiming ) / ( 1000 * 1000 ) );
        }

        //--//

        private string AddSearchDirectory( string dir )
        {
            if( m_searchOrder.Contains( dir ) == false )
            {
                m_searchOrder.Add( dir );
            }

            return dir;
        }

        void NotifyCompilationPhase( IR.CompilationSteps.PhaseDriver phase )
        {
            m_phaseExecutionCounter++;
            
            int types   = 0;
            int fields  = 0;
            int methods = 0;
            m_typeSystem.GetTypeSystemStatistics( ref types, ref fields, ref methods );

            ConsoleColor color = Console.ForegroundColor;
            Console.Write( "{0}: Phase: {1,-50} ", GetTime( ), phase );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine( "[types: {0,5},  fields: {1,6},  methods: {2,6}]", types, fields, methods );
            Console.ForegroundColor = color;

#if DEBUG_SAVE_STATE
            if(phase == Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phase.GenerateImage)
            {
                SaveIrToDisk( "temp.ZeligImage", m_typeSystem );
            }
#endif

            if(m_fDumpIRBeforeEachPhase)
            {
                var currentPhase = phase.ToString( );

                if(m_phasesForDiagnosticDumps.Contains( "All" ) || m_phasesForDiagnosticDumps.Contains( currentPhase ))
                {
                    string path = Path.Combine( m_outputDir, "phases" );

                    if(Directory.Exists( path ) == false)
                    {
                        Directory.CreateDirectory( path );
                    }
                    
                    string filePrefix = Path.Combine( path, m_phaseExecutionCounter.ToString() + "_" + m_outputName );
                    
                    DumpIRAsText( filePrefix + "." + currentPhase + ".ZeligIR"     );
                    DumpIRAsXML ( filePrefix + "." + currentPhase + ".ZeligIR.xml" );
                }
            }
        }

        //--//

        private static string RemoveFileExtension( string file )
        {
            return System.IO.Path.GetDirectoryName( file ) + @"\" + System.IO.Path.GetFileNameWithoutExtension( file );
        }

        private static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        private Importer.MetaData LoadAssembly( string file )
        {
            byte[] image = System.IO.File.ReadAllBytes( file );

            Importer.PELoader pe = new Importer.PELoader( file, image );

            Importer.MetaData md = Importer.MetaData.loadMetaData( file, this, pe, Importer.MetaDataLoaderFlags.LoadCode | Importer.MetaDataLoaderFlags.LoadDebugInfo );

            return md;
        }

        //--//

        object TS.IEnvironmentProvider.GetService( Type t )
        {
            if( t == typeof( Normalized.IMetaDataBootstrap ) )
            {
                return m_resolver;
            }

            if( t == typeof( TS.IConfigurationProvider ) )
            {
                return this;
            }

            if( t == typeof( IR.CompilationSteps.DelegationCache ) )
            {
                return m_delegationCache;
            }

            if( t == typeof( IR.CompilationSteps.IInlineOptions ) )
            {
                return m_LlvmCodeGenOptions;
            }

            if( t == typeof( ITargetSectionOptions ) )
            {
                return m_LlvmCodeGenOptions;
            }
            return null;
        }

        //--//

        bool TS.IConfigurationProvider.GetValue( string optionName,
                                                 out object val )
        {
            return m_configurationOptions.TryGetValue( optionName, out val );
        }

        //--//

        void IR.Transformations.ConstraintSystemCollector.IVisualizer.DisplayGraph( IR.Transformations.ConstraintSystemCollector.GraphState gs )
        {
            Microsoft.Zelig.Tools.InequalityGraphVisualization.Viewer.Show( gs );
        }

        //--//

        Importer.PdbInfo.PdbFile MetaData.ISymbolResolverHelper.ResolveAssemblySymbols( string file )
        {
            try
            {
                string   root       = RemoveFileExtension( file );
                string   symbolFile = root + SymbolSuffix;
                string   sourceFile = root + SourceCodeDatabaseSuffix;
                FileInfo symbolInfo = new FileInfo( symbolFile );

                if( symbolInfo.Exists )
                {
                    byte[] image = System.IO.File.ReadAllBytes( symbolFile );

                    Importer.PdbInfo.PdbFile pdbFile = new Importer.PdbInfo.PdbFile( new Importer.ArrayReader( image ) );

                    Importer.PdbInfo.DataStream streamSrc = pdbFile.GetStream( c_ZeligSourceCode );
                    if( streamSrc != null )
                    {
                        try
                        {
                            IR.SourceCodeTracker sct = new IR.SourceCodeTracker( );

                            using( var stream = new MemoryStream( streamSrc.Payload ) )
                            {
                                using( var ctx = IR.TypeSystemSerializer.GetDeserializationContext( stream, null, null, 0 ) )
                                {
                                    ctx.TransformGeneric( ref sct );
                                }
                            }

                            m_sourceCodeTracker.Merge( sct );
                        }
                        catch
                        {
                        }
                    }

                    return pdbFile;
                }

            }
            catch
            {
            }

            return null;
        }

        Importer.MetaData MetaData.IMetaDataResolverHelper.ResolveAssemblyReference( string name,
                                                                                     MetaData.MetaDataVersion ver )
        {
            //
            // Force use of our version of mscorlib.
            //
            if( name == "mscorlib" )
            {
                return CheckAndLoad( m_libraryLocation_TargetBuild, name );
            }

            Importer.MetaData md;

            foreach( string dir in m_searchOrder )
            {
                md = CheckAndLoad( dir, name, ver );
                if( md != null )
                {
                    return md;
                }
            }

            return null;
        }

        //--//

        private Importer.MetaData CheckAndLoad( string dir,
                                                string name )
        {
            string file = Path.Combine( dir, name + ".dll" );

            if( System.IO.File.Exists( file ) )
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

        private Importer.MetaData CheckAndLoad( string dir,
                                                string name,
                                                MetaData.MetaDataVersion ver )
        {
            Importer.MetaData md = CheckAndLoad( dir, name );

            if( md != null )
            {
                if( md.Assembly.Name == name )
                {
                    if( ver == null || md.Assembly.Version.IsCompatible( ver, false ) )
                    {
                        return md;
                    }
                }
            }

            return null;
        }

        //--//

        private void EmbedSourceCodeAll( string sDirectory )
        {
            DirectoryInfo dir = new DirectoryInfo( sDirectory );

            if( dir.Exists == false )
            {
                Console.WriteLine( "Cannot find directory '{0}'", sDirectory );
            }
            else
            {
                foreach( FileInfo file in dir.GetFiles( ) )
                {
                    if( string.Compare( file.Extension, SymbolSuffix, true ) == 0 )
                    {
                        try
                        {
                            EmbedSourceCode( file.FullName );
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void EmbedSourceCode( string file )
        {
            try
            {
                var fi = new System.IO.FileInfo( file );

                if( fi.Exists == false )
                {
                    Console.WriteLine( "Cannot find file '{0}'", file );
                }
                else if( m_fSkipReadOnly && fi.IsReadOnly )
                {
                    Console.WriteLine( "Skipping read-only file '{0}'", file );
                }
                else
                {
                    IR.SourceCodeTracker sct = new IR.SourceCodeTracker( );

                    byte[] image = System.IO.File.ReadAllBytes( file );

                    Importer.PdbInfo.PdbFile pdbFile = new Importer.PdbInfo.PdbFile( new Importer.ArrayReader( image ) );

                    foreach( var pdbFunc in pdbFile.Functions )
                    {
                        foreach( var pdbLine in pdbFunc.LineBlocks )
                        {
                            IR.SourceCodeTracker.SourceCode sc = sct.GetSourceCode( pdbLine.File.Name );
                            if( sc != null )
                            {
                                sc.UsingCachedValues = true;
                            }
                        }
                    }

                    {
                        MemoryStream output = new MemoryStream( );

                        using( IR.TransformationContextForCodeTransformation ctx = IR.TypeSystemSerializer.GetSerializationContext( output ) )
                        {
                            ctx.TransformGeneric( ref sct );
                        }

                        Importer.PdbInfo.DataStream stream = pdbFile.CreateNewStream( c_ZeligSourceCode );

                        stream.Payload = output.ToArray( );
                    }

                    System.IO.File.WriteAllBytes( file, pdbFile.Emit( ) );

                    Console.WriteLine( "Embedded source code in '{0}'", file );
                }
            }
            catch( Importer.PdbInfo.PdbException )
            {
                Console.WriteLine( "Skipping '{0}', unrecognized PDB format", file );
            }
        }

        //--//--//

        private void SaveIrToDisk( string file,
                                   IR.TypeSystemForCodeTransformation typeSystem )
        {
            using( System.IO.FileStream stream = new System.IO.FileStream( file, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024 ) )
            {
                IR.TypeSystemSerializer.Serialize( stream, typeSystem );
            }
        }

        private IR.TypeSystemForCodeTransformation LoadIrFromDisk( string file,
                                                                   IR.TypeSystemSerializer.CreateInstance callback )
        {
            using( System.IO.FileStream stream = new System.IO.FileStream( file, FileMode.Open ) )
            {
                return IR.TypeSystemSerializer.Deserialize( stream, callback, null, 0 );
            }
        }

        private object CreateInstanceForType( Type t )
        {
            if( t == typeof( IR.TypeSystemForCodeTransformation ) )
            {
                return new TypeSystemForFrontEnd( this, this );
            }

            return null;
        }

        //--//

        private void DumpIRAsText( string file )
        {
            using( var ird = new IR.TextIntermediateRepresentationDumper( file ) )
            {
                var types = m_typeSystem.Types.ToArray( );

                Array.Sort( types, ( x, y ) => x.ToString( ).CompareTo( y.ToString( ) ) );

                foreach( var td in types )
                {
                    ird.WriteLine( "###############################################################################" );
                    ird.WriteLine( );
                    ird.WriteLine( "Type: {0} [Size={1}]", td, td.ValidLayout ? td.Size : uint.MaxValue );

                    if( td.Extends != null )
                    {
                        ird.WriteLine( "  Extends: {0}", td.Extends );
                    }

                    foreach( var itf in td.Interfaces )
                    {
                        ird.WriteLine( "  Interface: {0}", itf );
                    }

                    foreach( var fd in td.Fields )
                    {
                        ird.WriteLine( "  Field: {0} [Offset={1}]", fd, fd.ValidLayout ? fd.Offset : -1 );
                    }

                    ird.WriteLine( );

                    var methods = ArrayUtility.CopyNotNullArray( td.Methods );

                    Array.Sort( methods, ( x, y ) => x.ToShortString( ).CompareTo( y.ToShortString( ) ) );

                    foreach( var md in methods )
                    {
                        IR.ControlFlowGraphState cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( md );

                        if( cfg != null )
                        {
                            ird.WriteLine( "  Method {0}", md );
                            ird.WriteLine( );

                            cfg.Dump( ird );
                        }
                        else
                        {
                            ird.WriteLine( "  NoCodeMethod {0}", md );
                        }

                        ird.WriteLine( );
                    }
                }
            }
        }
        
        private void DumpCallGraph( string file, bool fCallsTo )
        {
            using( var sr = new StreamWriter( file ) )
            {
                sr.WriteLine( "Call Graph for all surviving methods" ); 
                sr.WriteLine( "=========================================================" ); 
                sr.WriteLine( $"Listing all calls {{0}} a method", fCallsTo ? "to" : "from" ); 
                sr.WriteLine( "=========================================================" ); 
                sr.WriteLine( ); 
                sr.WriteLine( ); 

                var callsDb = fCallsTo ? m_typeSystem.FlattenedCallsDataBase_CallsTo : m_typeSystem.FlattenedCallsDataBase_CallsFrom;

                foreach(var method in callsDb.Keys)
                {
                    sr.WriteLine( $"Method: {method.ToShortString()}" ); 
                    sr.WriteLine( ); 

                    var calls = callsDb[ method ];

                    if(calls != null && calls.Count > 0)
                    {
                        calls.Sort( (x, y) => { return x.Target.FullyQualifiedName.CompareTo( y.Target.FullyQualifiedName );  } );  

                        foreach(var call in calls)
                        {
                            sr.WriteLine( $" {{0}} {call.ToString( )}", fCallsTo ? "<==" : "==>" );
                        }
                    }
                    else
                    {
                        sr.WriteLine( " <== no callers ==>" );
                    }

                    sr.WriteLine( "=========================================================" );
                    sr.WriteLine( ); 
                }
            }
        }

        private void DumpIRAsXML(string file)
        {
            // Collect all methods into a list
            var allMethods = new List<TS.MethodRepresentation>();
            m_typeSystem.EnumerateMethods(method => allMethods.Add(method));

            // Initialize the XML IRDumper
            var doc = new System.Xml.XmlDocument();
            var node = XmlHelper.AddElement(doc, "Methods");
            var irDumper = new IR.XmlIntermediateRepresentationDumper(doc, node);

            // Dump each method alphabetically
            foreach (var method in irDumper.Sort(allMethods))
            {
                IR.TypeSystemForCodeTransformation.GetCodeForMethod(method)?.Dump(irDumper);
            }

            doc.Save(file);
        }

        //--//

        private void InitializeConfigurationManager( )
        {
            if( m_configurationManager == null )
            {
                m_configurationManager = new Cfg.Manager( );

                m_configurationManager.AddAllAssemblies( m_compilationSetupBinaryPath );

                m_configurationManager.ComputeAllPossibleValuesForFields( );
            }
        }

        private void SearchConfigurationOptions( Cfg.AbstractCategory category )
        {
            foreach( var valueCtx in category.SearchValuesWithAttributes( typeof( Cfg.LinkToConfigurationOptionAttribute ) ) )
            {
                var attrib = valueCtx.GetAttribute<Cfg.LinkToConfigurationOptionAttribute>( );

                m_configurationOptions[ attrib.Name ] = valueCtx.Value;
            }
        }

        private IEnumerable<T> GetConfigurationOptions<T>( ) where T : Cfg.AbstractCategory
        {
            InitializeConfigurationManager( );

            foreach( Cfg.AbstractCategory value in m_configurationManager.AllValues )
            {
                if( value is T )
                {
                    value.ApplyDefaultValues( );

                    yield return ( T )value;
                }
            }
        }

        private T GetConfigurationOption<T>( Type target ) where T : Cfg.AbstractCategory
        {
            foreach( var option in GetConfigurationOptions<T>( ) )
            {
                if( option.GetType( ) == target )
                {
                    return option;
                }
            }

            return null;
        }
        
        private static string[] RecombineArgs( string[] args )
        {
            List<string> arguments = new List<string>(args.Length);

            for(int i = 0; i < args.Length; ++i)
            {
                // An argument like "C:\my directory\with\a\space" will be split as
                // "C:\my" and "irectory\with\a\space" acrosss two entries in args. 
                // We need to re-combine those entries.
                // It is important to note that this is a fairly simplistic approach
                // to quoted string args. In particular there is no support for any
                // sort of escaped quotes for quoted strings within an argument etc...
                
                // We must find a matching pair of double quotes. 
                // if they are on the same arg, then move on.
                if( args[ i ].StartsWith( "\"" ) && !args[ i ].EndsWith("\"") )
                {
                    //start building the complete string
                    var quotedString = new StringBuilder( );
                    for( ; i < args.Length; ++i )
                    {
                        if( quotedString.Length > 0 )
                            quotedString.Append( ' ' );

                        quotedString.Append( args[ i ] );
                        if( args[ i ].EndsWith( "\"" ) )
                        {
                            arguments.Add( quotedString.ToString( ) );
                            break;
                        }
                    }
                    // if we are at the last entry in args already, then 
                    // we have an unmatched quote we may be able to recover from
                    // so ignore it and hope for the best
                    if( i >= args.Length )
                    {
                        arguments.Add( quotedString.ToString( ) );
                    }
                }
                else
                    arguments.Add( args[i] );
            }

            return arguments.ToArray( ); 
        }

        private bool Parse( string[] args, ref bool fNoSDK)
        {
            if( args == null )
                return false;

            for( int i = 0; i < args.Length; i++ )
            {
                string arg = args[ i ];

                if( arg == string.Empty )
                {
                    continue;
                }

                if( arg.StartsWith( "/" ) ||
                    arg.StartsWith( "-" ) )
                {
                    string option = arg.Substring( 1 );

                    if( IsMatch( option, "Cfg" ) )
                    {
                        string file;

                        if( !GetArgument( arg, args, ref i, out file, true ) )
                        {
                            return false;
                        }

                        using( System.IO.StreamReader stream = new System.IO.StreamReader( file ) )
                        {
                            string line;

                            while( ( line = stream.ReadLine( ) ) != null )
                            {
                                if( line.StartsWith( "#" ) )
                                {
                                    continue;
                                }

                                var arguments = line.Split( new char[ ] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );

                                string[ ] recombinedArgs = RecombineArgs( arguments );

                                if( recombinedArgs == null )
                                {
                                    Console.WriteLine( String.Format( "Arguments at line '{0}' could not be recombined", line ) );

                                    return false;
                                }

                                if( Parse( recombinedArgs, ref fNoSDK ) == false )
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else if( IsMatch( option, "NoSDK" ) )
                    {
                        fNoSDK = true;
                    }
                    else if( IsMatch( option, "DumpIL" ) )
                    {
                        m_fDumpIL = true;
                    }
                    else if( IsMatch( option, "ReloadState" ) )
                    {
                        m_fReloadState = true;
                    }
                    else if( IsMatch( option, "DumpIRpre" ) )
                    {
                        m_fDumpIRpre = true;
                    }
                    else if( IsMatch( option, "DumpIRpost" ) )
                    {
                        m_fDumpIRpost = true;
                    }
                    else if( IsMatch( option, "DumpIR" ) )
                    {
                        m_fDumpIR = true;
                    }
                    else if( IsMatch( option, "DumpFlattenedCallGraph" ) )
                    {
                        m_fDumpFlattenedCallGraph = true;
                    }
                    else if( IsMatch( option, "DumpIRXMLpre" ) )
                    {
                        m_fDumpIRXMLpre = true;
                    }
                    else if( IsMatch( option, "DumpIRXMLpost" ) )
                    {
                        m_fDumpIRXMLpost = true;
                    }
                    else if( IsMatch( option, "DumpIRBeforePhase" ) )
                    {
                        m_fDumpIRBeforeEachPhase = true;

                        string phase;
                        bool fFoundOne = false;
                        while( GetArgument( arg, args, ref i, out phase, false, false ) )
                        {
                            m_phasesForDiagnosticDumps.Add( phase );

                            fFoundOne = true;
                        }

                        if( fFoundOne == false )
                        {
                            return false;
                        }
                    }
                    else if( IsMatch( option, "DumpIRXML" ) )
                    {
                        m_fDumpIRXML = true;
                    }
                    else if( IsMatch( option, "DumpLLVMIR" ) )
                    {
                        m_fDumpLLVMIR = true;
                    }
                    else if( IsMatch( option, "SkipLlvmOpt" ) )
                    {
                        m_fSkipLlvmOptExe = true;
                    }
                    else if( IsMatch( option, "GenerateObj" ) )
                    {
                        m_fDumpLLVMIR = true;
                        m_fGenerateObj = true;
                    }
                    else if( IsMatch( option, "DumpLLVMIR_TextRepresentation" ) )
                    {
                        m_fDumpLLVMIR_TextRepresentation = true;
                    }
                    else if( IsMatch( option, "DumpCFG" ) )
                    {
                        m_fDumpCFG = true;
                    }
                    else if( IsMatch( option, "DumpASM" ) )
                    {
                        m_fDumpASM = true;
                    }
                    else if( IsMatch( option, "DumpASMDIR" ) )
                    {
                        m_fDumpASMDIR = true;
                    }
                    else if( IsMatch( option, "DumpHEX" ) )
                    {
                        m_fDumpHEX = true;
                    }
                    else if( IsMatch( option, "DumpRAW" ) )
                    {
                        string section;
                        uint rangeStart;
                        uint rangeEnd;

                        if( !GetArgument( arg, args, ref i, out section, false ) ||
                            !GetArgument( arg, args, ref i, out rangeStart, true ) ||
                            !GetArgument( arg, args, ref i, out rangeEnd, true ) )
                        {
                            return false;
                        }

                        m_dumpRawImage.Add( new RawImage
                        {
                            SectionName = section,
                            RangeStart = rangeStart,
                            RangeEnd = rangeEnd
                        } );
                    }
                    else if( IsMatch( option, "HostAssemblyDir" ) )
                    {
                        string dir;

                        if( !GetArgument( arg, args, ref i, out dir, true ) )
                        {
                            return false;
                        }

                        m_libraryLocation_HostBuild = AddSearchDirectory( dir );
                    }
                    else if( IsMatch( option, "DeviceAssemblyDir" ) )
                    {
                        string dir;

                        if( !GetArgument( arg, args, ref i, out dir, true ) )
                        {
                            return false;
                        }

                        m_libraryLocation_TargetBuild = AddSearchDirectory( dir );
                    }
                    else if( IsMatch( option, "ImportDirectory" ) )
                    {
                        string dir;

                        if( !GetArgument( arg, args, ref i, out dir, true ) )
                        {
                            return false;
                        }

                        dir = dir.ToLower( );

                        if( !m_importDirectories.Contains( dir ) )
                        {
                            m_importDirectories.Add( dir );
                        }
                    }
                    else if( IsMatch( option, "ImportLibrary" ) )
                    {
                        string file;

                        if( !GetArgument( arg, args, ref i, out file, true ) )
                        {
                            return false;
                        }

                        file = file.ToLower( );

                        if( !m_importLibraries.Contains( file ) )
                        {
                            m_importLibraries.Add( file );
                        }
                    }
                    else if( IsMatch( option, "MaxProcs" ) )
                    {
                        uint iMaxProcs;

                        if( !GetArgument( arg, args, ref i, out iMaxProcs, false ) )
                        {
                            return false;
                        }

                        IR.CompilationSteps.ParallelTransformationsHandler.MaximumNumberOfProcessorsToUse =
                            ( int )iMaxProcs;
                    }
                    else if( IsMatch( option, "OutputName" ) )
                    {
                        string name;

                        if( !GetArgument( arg, args, ref i, out name, false ) )
                        {
                            return false;
                        }

                        m_outputName = name;
                    }
                    else if( IsMatch( option, "NativeIntSize" ) )
                    {
                        string name;

                        if( !GetArgument( arg, args, ref i, out name, false ) )
                        {
                            return false;
                        }

                        if( !UInt32.TryParse( name, out m_nativeIntSize ) )
                            return false;
                    }
                    else if( IsMatch( option, "OutputDir" ) )
                    {
                        string dir;

                        if( !GetArgument( arg, args, ref i, out dir, true ) )
                        {
                            return false;
                        }

                        m_outputDir = dir;
                    }
                    else if( IsMatch( option, "Reference" ) )
                    {
                        string reference;

                        if( !GetArgument( arg, args, ref i, out reference, false ) )
                        {
                            return false;
                        }

                        m_references.Add( reference );
                    }
                    else if( IsMatch( option, "CompilationSetupPath" ) )
                    {
                        string compilationBinary;

                        if( !GetArgument( arg, args, ref i, out compilationBinary, true ) )
                        {
                            return false;
                        }

                        m_compilationSetupBinaryPath = compilationBinary;
                    }
                    else if( IsMatch( option, "LlvmBinPath" ) )
                    {
                        string toolsPath;

                        if( !GetArgument( arg, args, ref i, out toolsPath, true ) )
                        {
                            return false;
                        }

                        m_LlvmBinPath = toolsPath;
                    }
                    else if( IsMatch( option, "LlvmOptArgs" ) )
                    {
                        string optArgs;

                        if( !GetArgument( arg, args, ref i, out optArgs, false ) )
                        {
                            return false;
                        }

                        m_LlvmOptArgs = optArgs.Trim( '"' );
                    }
                    else if( IsMatch( option, "LlvmLlcArgs" ) )
                    {
                        string llcArgs;

                        if( !GetArgument( arg, args, ref i, out llcArgs, false ) )
                        {
                            return false;
                        }

                        m_LlvmLlcArgs = llcArgs.Trim( '"' );
                    }
                    else if( IsMatch( option, "CompilationSetup" ) )
                    {
                        string compilationSetup;

                        if( !GetArgument( arg, args, ref i, out compilationSetup, false ) )
                        {
                            return false;
                        }

                        m_compilationSetup = null;

                        foreach( var setup in GetConfigurationOptions<Cfg.CompilationSetupCategory>( ) )
                        {
                            if( setup.GetType( ).FullName == compilationSetup )
                            {
                                m_compilationSetup = setup;
                                break;
                            }
                        }

                        if( m_compilationSetup == null )
                        {
                            Console.WriteLine( "Cannot find definition for compilation setup '{0}'", compilationSetup );
                            return false;
                        }

                        SearchConfigurationOptions( m_compilationSetup );

                        m_product = GetConfigurationOption<Cfg.ProductCategory>( m_compilationSetup.Product );
                        if( m_product == null )
                        {
                            Console.Error.WriteLine( "Cannot compile without a product definition!" );
                            return false;
                        }

                        SearchConfigurationOptions( m_product );

                        m_memoryMap = GetConfigurationOption<Cfg.MemoryMapCategory>( m_compilationSetup.MemoryMap );
                        if( m_memoryMap == null )
                        {
                            Console.Error.WriteLine( "Cannot compile without a memory map!" );
                            return false;
                        }

                        SearchConfigurationOptions( m_memoryMap );
                    }
                    else if( IsMatch( option, "CompilationOption" ) )
                    {
                        string type;
                        string name;
                        string value;

                        if( !GetArgument( arg, args, ref i, out type, false ) ||
                            !GetArgument( arg, args, ref i, out name, false ) ||
                            !GetArgument( arg, args, ref i, out value, false ) )
                        {
                            return false;
                        }

                        Type t = Type.GetType( type );
                        if( t == null )
                        {
                            Console.Error.WriteLine( "Cannot find type '{0}'", type );
                            return false;
                        }

                        try
                        {
                            object res;

                            if(t == typeof( String ))
                            {
                                res = value;
                            }
                            else
                            {
                                res = t.InvokeMember( "Parse",
                                    System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static |
                                    System.Reflection.BindingFlags.Public, null, null, new object[] { value } );
                            }

                            m_configurationOptions[ name ] = res;
                        }
                        catch( Exception ex )
                        {
                            Console.Error.WriteLine( "Cannot parse value '{0}': {1}", value, ex );
                            return false;

                        }
                    }
                    else if( IsMatch( option, "CompilationPhaseDisabled" ) )
                    {
                        string phase;

                        if( !GetArgument( arg, args, ref i, out phase, false ) )
                        {
                            return false;
                        }
                        m_disabledPhases.Add( phase.Trim( ) );
                    }
                    else if( IsMatch( option, "Include" ) )
                    {
                        string dir;

                        if( !GetArgument( arg, args, ref i, out dir, true ) )
                        {
                            return false;
                        }

                        AddSearchDirectory( dir );
                    }
                    else if( IsMatch( option, "SkipReadOnly" ) )
                    {
                        m_fSkipReadOnly = true;
                    }
                    else if( IsMatch( option, "EmbedSourceCode" ) )
                    {
                        string sFile;

                        if( !GetArgument( arg, args, ref i, out sFile, true ) )
                        {
                            return false;
                        }

                        EmbedSourceCode( sFile );
                        return false;
                    }
                    else if( IsMatch( option, "EmbedSourceCodeAll" ) )
                    {
                        string sDir;

                        if( !GetArgument( arg, args, ref i, out sDir, true ) )
                        {
                            return false;
                        }

                        EmbedSourceCodeAll( sDir );
                        return false;
                    }
                    else if( IsMatch( option, "EntryPoint" ) )
                    {
                        string sEP;

                        if( !GetArgument( arg, args, ref i, out sEP, true ) )
                        {
                            return false;
                        }

                        m_entryPointName = sEP;
                    }
                    else if( IsMatch( option, "Debug" ) )
                    {
                        m_fSkipLlvmOptExe = true;
                        m_LlvmCodeGenOptions.EnableAutoInlining = false;
                        m_LlvmCodeGenOptions.HonorInlineAttribute = false;
                        m_LlvmCodeGenOptions.InjectPrologAndEpilog = false;
                    }
                    else if( IsMatch( option, "Architecture" ) )
                    {
                        string arch;

                        if( !GetArgument( arg, args, ref i, out arch, true ) )
                        {
                            return false;
                        }

                        ValidateArchitecture( arch );

                        m_architecture = arch;
                    }
                    else if( IsMatch( option, "DisableAutoInlining" ) )
                    {
                        m_LlvmCodeGenOptions.EnableAutoInlining = false;
                    }
                    else if( IsMatch( option, "IgnoreInlineAttributes" ) )
                    {
                        m_LlvmCodeGenOptions.HonorInlineAttribute = false;
                    }
                    else if( IsMatch( option, "DisablePrologEpilogInjection" ) )
                    {
                        m_LlvmCodeGenOptions.InjectPrologAndEpilog = false;
                    }
                    else if( IsMatch( option, "GenerateDataSectionPerType" ) )
                    {
                        m_LlvmCodeGenOptions.GenerateDataSectionPerType = true;
                    }
                    else
                    {
                        Console.WriteLine( "Unrecognized option: {0}", option );
                        return false;
                    }
                }
                else
                {
                    arg = Expand( arg );
                    if( File.Exists( arg ) == false )
                    {
                        Console.WriteLine( "Cannot find '{0}'", arg );
                        return false;
                    }

                    if( m_targetFile != null )
                    {
                        Console.WriteLine( "ERROR: Only one target file per compilation." );
                    }

                    m_targetFile = arg;

                    m_searchOrder.Insert( 0, System.IO.Path.GetDirectoryName( arg ) );
                }
            }

            return true;
        }
        
        private void ValidateArchitecture( string arch )
        {
            switch(arch)
            {
                case c_CortexM0:
                case c_CortexM3:
                case c_CortexM4:
                case c_CortexM7:
                case c_x86_64:
                    break;
                default:
                    throw new NotSupportedException( "The only architectures currently supported are x86-64 and Cortex-M, for M0[+], M3, M4 and M7 variants" );
            }
        }

        private static bool IsMatch( string arg,
                                     string cmd )
        {
            return String.Compare( arg, cmd, StringComparison.OrdinalIgnoreCase ) == 0;
        }

        private static bool GetArgument( string arg,
                                         string[] args,
                                         ref int i,
                                         out string value,
                                         bool fExpand, 
                                         bool fErrorOnParse = true )
        {
            if( i + 1 < args.Length )
            {
                i++;

                value = args[ i ];

                if( fExpand )
                {
                    value = Expand( value );
                }

                return true;
            }

            if(fErrorOnParse)
            {
                Console.WriteLine( "Option '{0}' needs an argument", arg );
            }

            value = null;
            return false;
        }

        private static bool GetArgument( string arg,
                                         string[] args,
                                         ref int i,
                                         out uint value,
                                         bool fCanBeHex )
        {
            string str;

            if( GetArgument( arg, args, ref i, out str, false ) )
            {
                if( uint.TryParse( str, out value ) )
                {
                    return true;
                }

                if( fCanBeHex )
                {
                    if( str.StartsWith( "0x" ) && uint.TryParse( str.Substring( 2 ), System.Globalization.NumberStyles.AllowHexSpecifier, null, out value ) )
                    {
                        return true;
                    }
                }

                Console.WriteLine( "Option '{0}' needs a numeric argument, got '{1}'", arg, str );
            }

            value = 0;
            return false;
        }

        //--//

        private bool ValidateOptions( )
        {
            if( !ValidateLlvmToolsPath( ) )
                return false;

            /*
            if( m_compilationSetup == null )
            {
                Console.Error.WriteLine( "Cannot compile without a compilation setup!" );
                return false;
            }*/

            return true;
        }

        // Verify the LLVM tools path
        // if the path isn't set from command line options try to figure it out
        // using the following (in order):
        //   1) The path this program is executing from (Standard end user install scenario)
        //   2) LlvmBinPathEnvVarNames environment variables for curent process
        //   3) LlvmBinPathEnvVarNames environment variable/registry for curent user
        //   4) LlvmBinPathEnvVarNames environment variable/registry for curent machine
        //   5) HKCU\<LlvmRegSoftwareBinPath>\@ToolsBin
        //   6) HKLM\<LlvmRegSoftwareBinPath>\@ToolsBin
        //   7) HKCU\<LlvmRegSoftwareBinPath>\@SrcRoot + "build\Win32\Release\bin"
        //   8) HKCU\<LlvmRegSoftwareBinPath>\@SrcRoot + "build\x64\Release\bin"
        //   9) HKLM\<LlvmRegSoftwareBinPath>\@SrcRoot + "build\Win32\Release\bin"
        //   10) HKLM\<LlvmRegSoftwareBinPath>\@SrcRoot + "build\x64\Release\bin"
        //   11) Scan all paths in the processes PATH environment variable for the
        //       first one containing opt.exe, llc.exe and llvm-dis.exe
        //
        //  If none of the above yields a path that contains the required apps then fail the validation
        //
        // NOTE: this will update m_LlvmBinPath to the resolved path if one was found.
        //       If the path to the LLVM tools isn't already in the PATH environment var
        //       it is added to the PATH for this process.
        private bool ValidateLlvmToolsPath( )
        {
            // if the tools aren't needed, based on options, no point verifying anything else
            if(m_fSkipLlvmOptExe && !m_fGenerateObj)
                return true;

            // cover the expected production scenario first
            if (string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                var thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                var localPath = Path.GetDirectoryName(thisAssembly.Location);
                if(PathHasLlvmTools(localPath))
                    m_LlvmBinPath = localPath;
            }

            // cover the most common internal development scenarios
            if (string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                m_LlvmBinPath = FindLLvmToolsPathOrSubPathFromEnvVars( EnvironmentVariableTarget.Process
                                                                     , EnvironmentVariableTarget.User
                                                                     , EnvironmentVariableTarget.Machine
                                                                     );
            }

            if(string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                m_LlvmBinPath = GetRegValueString( RegistryHive.CurrentUser, LlvmRegSoftwareBinPath, "ToolsBin" );
            }

            if(string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                m_LlvmBinPath = GetRegValueString( RegistryHive.LocalMachine, LlvmRegSoftwareBinPath, "ToolsBin" );
            }

            if(string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                var srcRootDir = GetRegValueString( RegistryHive.CurrentUser, LlvmRegSoftwareBinPath, "SrcRoot" );
                if(!string.IsNullOrWhiteSpace( srcRootDir ))
                {
                    m_LlvmBinPath = Path.Combine( srcRootDir, "build", "Win32", "Release", "bin" );
                    if(string.IsNullOrWhiteSpace( m_LlvmBinPath ) && Environment.Is64BitOperatingSystem)
                    {
                        m_LlvmBinPath = m_LlvmBinPath = Path.Combine( srcRootDir, "build", "x64", "Release", "bin" );
                    }
                }
            }

            if(string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                var srcRootDir = GetRegValueString( RegistryHive.LocalMachine, LlvmRegSoftwareBinPath, "SrcRoot" );
                if(!string.IsNullOrWhiteSpace( srcRootDir ))
                {
                    m_LlvmBinPath = Path.Combine( srcRootDir, "build", "Win32", "Release", "bin" );
                    if(string.IsNullOrWhiteSpace( m_LlvmBinPath ) && Environment.Is64BitOperatingSystem)
                    {
                        m_LlvmBinPath = m_LlvmBinPath = Path.Combine( srcRootDir, "build", "x64", "Release", "bin" );
                    }
                }
            }

            // try scanning the PATH environment var to see if the tools are already in the PATH
            var envPathToLlvmTools = FindLlvmToolsInPath( );
            if(string.IsNullOrWhiteSpace( m_LlvmBinPath ) && !string.IsNullOrWhiteSpace( envPathToLlvmTools ))
            {
                m_LlvmBinPath = envPathToLlvmTools;
            }

            if(string.IsNullOrWhiteSpace( m_LlvmBinPath ))
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine( "ERROR: LLVM Tools location not specified and could not be determined" );
                Console.ForegroundColor = color;
                return false;
            }

            if(!PathHasLlvmTools( m_LlvmBinPath ))
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine( "ERROR: LLVM Tools not found in path '{0}'", m_LlvmBinPath );
                Console.ForegroundColor = color;
                return false;
            }

            // if the llvm tools path isn't already in the PATH environment variable, add it so the tools are found when launched
            if( string.IsNullOrWhiteSpace( envPathToLlvmTools ) )
            {
                Environment.SetEnvironmentVariable( "PATH", $"{Environment.GetEnvironmentVariable( "PATH" )};{m_LlvmBinPath}" );
            }

            return true;
        }

        private static string FindLLvmToolsPathOrSubPathFromEnvVars( params EnvironmentVariableTarget[] targets )
        {
            foreach( var target in targets )
            {
                foreach( var envVar in LlvmBinPathEnvVarNames )
                {
                    var envPath = Environment.GetEnvironmentVariable( envVar, target );
                    if( string.IsNullOrEmpty( envPath ) )
                        continue;

                    if( PathHasLlvmTools( envPath ) )
                    {
                        return envPath;
                    }

                    // if the path points to the LLVM build source root then the actual
                    // binaries are in a platform sepcific sub folder, try Win32 first.
                    var envSubPath = Path.Combine( envPath, "build", "Win32", "Release", "bin" );
                    if( PathHasLlvmTools( envSubPath ) )
                    {
                        return envSubPath;
                    }

                    envSubPath = Path.Combine( envPath, "build", "x64", "Release", "bin" );
                    if( PathHasLlvmTools( envPath ) )
                    {
                        return envPath;
                    }
                }
            }
            return null;
        }

        private string FindLlvmToolsInPath( )
        {
            var envPathVar = Environment.ExpandEnvironmentVariables( "PATH" );
            var envPaths = envPathVar.Split( ';' );
            return ( from path in envPaths
                     where PathHasLlvmTools( path )
                     select path
                   ).FirstOrDefault( );
        }

        static bool PathHasLlvmTools( string path )
        {
            return File.Exists( Path.Combine( path, "opt.exe" ) )   &&
                   File.Exists( Path.Combine( path, "llc.exe" ) )   &&
                   File.Exists( Path.Combine( path, "llvm-dis.exe" ) );
        }

        private string GetRegValueString( RegistryHive hive, string subkey, string valueName, string defaultValue = null, RegistryView view = RegistryView.Default )
        {
            using( var hkRoot = RegistryKey.OpenBaseKey( hive, view ) )
            using( var key = hkRoot.OpenSubKey( subkey ) )
            {
                if( key == null )
                    return null;

                return ( string )key.GetValue( valueName );
            }
        }

        private void Compile( )
        {
            //
            // Store build artifacts on a per-product basis
            //
            m_outputDir = Path.Combine( m_outputDir,  ExtractProductName( m_compilationSetup.Product ) );

            if( m_outputName == null )
            {
                m_outputName = Path.GetFileNameWithoutExtension( m_targetFile );
            }

            string filePrefix = Path.Combine( m_outputDir, m_outputName );

            /*FileStream fs = new FileStream( filePrefix + "_cout.txt", FileMode.Create );
            StreamWriter sw = new StreamWriter( fs );
            Console.SetOut( sw );*/

            //--//

            //
            // We need this assembly, for all the extra stuff about open classes.
            //
            if( m_fReloadState == false )
            {
                MetaData.IMetaDataResolverHelper resolver = this;
                Importer.MetaData                md;

                md = resolver.ResolveAssemblyReference( "Microsoft.Zelig.Runtime", null );

                m_resolver.Add( md );

                m_resolver.ResolveAll( );

                md = resolver.ResolveAssemblyReference( "mscorlib", null );

                m_resolver.Add( md );

                m_resolver.ResolveAll( );

                //--//

                md = LoadAssembly( m_targetFile );

                m_resolver.Add( md );

                m_resolver.ResolveAll( );

                //--//

                foreach( string reference in m_references )
                {
                    md = resolver.ResolveAssemblyReference( reference, null );

                    m_resolver.Add( md );

                    m_resolver.ResolveAll( );
                }
            }

            //--//

#if DEBUG_RELOAD_STATE
            m_typeSystem = (TypeSystemForFrontEnd)LoadIrFromDisk( "temp.ZeligImage", CreateInstanceForType );
            m_typeSystem.GenerateImage( Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phase.GenerateImage ); 

            m_controller = new IR.CompilationSteps.Controller( m_typeSystem );

            if(m_fDumpIL)
            {
            }

            if(m_fDumpIRpre)
            {
            }
#else

            if( m_fReloadState == false )
            {
                m_typeSystem = new TypeSystemForFrontEnd( this, this );

                m_delegationCache = new IR.CompilationSteps.DelegationCache( m_typeSystem );

                m_typeSystem.InitializeForCompilation( );

                //--//

                var pa = ( IR.Abstractions.Platform )Activator.CreateInstance( m_compilationSetup.Platform, m_typeSystem, m_memoryMap );
                
                m_typeSystem.PlatformAbstraction = pa;

                //--//

                var cc = ( IR.Abstractions.CallingConvention )Activator.CreateInstance( m_compilationSetup.CallingConvention, m_typeSystem );
                
                m_typeSystem.CallingConvention = cc;

                //--//
                
                m_typeSystem.Product = m_product.GetType();

                //--//

                Console.WriteLine( "{0}: ConvertToIR", GetTime( ) );

                foreach( Normalized.MetaDataAssembly asml in m_resolver.NormalizedAssemblies )
                {
                    m_typeSystem.ImportAssembly( asml );
                }

                Console.WriteLine( "{0}: Done", GetTime( ) );
                
                //--//

                Console.WriteLine( "{0}: ResolveAll", GetTime( ) );
                m_typeSystem.ResolveAll( );
                Console.WriteLine( "{0}: Done", GetTime( ) );
                
                //--//
                
                Directory.CreateDirectory( Path.GetDirectoryName( filePrefix ) );

                if( m_fDumpIL )
                {
                    DirectoryInfo di = Directory.CreateDirectory( filePrefix );

                    string oldCD = Environment.CurrentDirectory;

                    Environment.CurrentDirectory = di.FullName;

                    foreach( Normalized.MetaDataAssembly asmlNormalized in m_resolver.NormalizedAssemblies )
                    {
                        using( MetaData.MetaDataDumper writer = new MetaData.MetaDataDumper( asmlNormalized.Name, asmlNormalized.Version ) )
                        {
                            writer.Process( asmlNormalized, true );
                        }
                    }

                    Environment.CurrentDirectory = oldCD;
                }

                if( m_fDumpCFG )
                {
                    DirectoryInfo di = Directory.CreateDirectory( filePrefix + "CFG" );

                    string oldCD = Environment.CurrentDirectory;

                    Environment.CurrentDirectory = di.FullName;

                    foreach( var t in m_typeSystem.Types )
                    {
                        foreach( var method in t.Methods )
                        {
                            IR.ControlFlowGraphStateForCodeTransformation cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( method );

                            if( cfg != null )
                            {
                                cfg.DumpToFile( method.Identity.ToString( ) + ".txt" );
                            }
                        }
                    }

                    Environment.CurrentDirectory = oldCD;
                }

                if( m_fDumpIRpre )
                {
                    using( System.IO.TextWriter writer = new System.IO.StreamWriter( filePrefix + ".TypeSystemDump.IrTxtpre", false, System.Text.Encoding.ASCII ) )
                    {
                        writer.WriteLine( "======================" );
                        writer.WriteLine( "==== Types table ====" );
                        writer.WriteLine( "======================" );
                        foreach( TS.TypeRepresentation td in m_typeSystem.Types )
                        {
                            writer.WriteLine( "Type    : {0}", td );
                            writer.WriteLine( "Assembly: {0}", td.Owner.Name );
                            writer.WriteLine( "Version : {0}", td.Owner.Version );
                        }

                        writer.WriteLine( "" );
                        writer.WriteLine( "" );
                        writer.WriteLine( "" );
                        writer.WriteLine( "======================" );
                        writer.WriteLine( "==== Type Details ====" );
                        writer.WriteLine( "======================" );
                        foreach( TS.TypeRepresentation td in m_typeSystem.Types )
                        {
                            writer.WriteLine( "Type: {0}", td );

                            foreach( TS.TypeRepresentation itf in td.Interfaces )
                            {
                                writer.WriteLine( "  Interface: {0}", itf );
                            }

                            foreach( TS.FieldRepresentation fd in td.Fields )
                            {
                                writer.WriteLine( "  Field: {0}", fd );
                            }

                            foreach( TS.MethodRepresentation md in td.Methods )
                            {
                                writer.WriteLine( "  Method: {0}", md );
                            }

                            writer.WriteLine( );
                        }
                    }

                    DumpIRAsText( filePrefix + ".ZeligIR_Pre" );
                }

                if (m_fDumpIRXMLpre)
                {
                    DumpIRAsXML(filePrefix + ".ZeligIR_Pre.xml");
                }

                //--//

                m_typeSystem.NativeImportDirectories = m_importDirectories;
                m_typeSystem.NativeImportLibraries = m_importLibraries;

                Console.WriteLine( "{0}: ExecuteSteps", GetTime( ) );
                m_controller = new IR.CompilationSteps.Controller( m_typeSystem, m_disabledPhases );

                m_controller.ExecuteSteps( );

                if( m_fDumpIRpost )
                {
                    using( System.IO.TextWriter writer = new System.IO.StreamWriter( filePrefix + ".TypeSystemDump.IrTxtpost", false, System.Text.Encoding.ASCII ) )
                    {
                        writer.WriteLine( "======================" );
                        writer.WriteLine( "==== Types table ====" );
                        writer.WriteLine( "======================" );
                        foreach( TS.TypeRepresentation td in m_typeSystem.Types )
                        {
                            writer.WriteLine( "Type    : {0}", td );
                            writer.WriteLine( "Assembly: {0}", td.Owner.Name );
                            writer.WriteLine( "Version : {0}", td.Owner.Version );
                        }

                        writer.WriteLine( "" );
                        writer.WriteLine( "" );
                        writer.WriteLine( "" );
                        writer.WriteLine( "======================" );
                        writer.WriteLine( "==== Type Details ====" );
                        writer.WriteLine( "======================" );
                        foreach( TS.TypeRepresentation td in m_typeSystem.Types )
                        {
                            writer.WriteLine( "Type    : {0}", td );

                            foreach( TS.TypeRepresentation itf in td.Interfaces )
                            {
                                writer.WriteLine( "  Interface: {0}", itf );
                            }

                            foreach( TS.FieldRepresentation fd in td.Fields )
                            {
                                writer.WriteLine( "  Field: {0}", fd );
                            }

                            foreach( TS.MethodRepresentation md in td.Methods )
                            {
                                writer.WriteLine( "  Method: {0}", md );
                            }

                            writer.WriteLine( );
                        }
                    }

                    DumpIRAsText( filePrefix + ".ZeligIR_Post" );
                }

                if (m_fDumpIRXMLpost)
                {
                    DumpIRAsXML(filePrefix + ".ZeligIR_Post.xml");
                }

                SaveIrToDisk( "temp.ZeligImage", m_typeSystem );
            }
            else
            {
                m_typeSystem = ( TypeSystemForFrontEnd )LoadIrFromDisk( "temp.ZeligImage", CreateInstanceForType );

                m_delegationCache = new IR.CompilationSteps.DelegationCache( m_typeSystem );

                //--//

                Console.WriteLine( "{0}: ExecuteSteps", GetTime( ) );
                m_controller = new IR.CompilationSteps.Controller( m_typeSystem );

                m_controller.ExecuteSteps( true );
            }

            Console.WriteLine( "{0}: Done", GetTime( ) );
            Console.Out.Flush( );
#endif

            if( PerformanceCounters.ContextualTiming.IsEnabled( ) )
            {
                Console.WriteLine( "{0}: Dumping Performance Counters", GetTime( ) );

                using( System.IO.StreamWriter output = new System.IO.StreamWriter( filePrefix + "_timing.type.txt", false, System.Text.Encoding.ASCII ) )
                {
                    PerformanceCounters.ContextualTiming.DumpAllByType( output );
                }

                using( System.IO.StreamWriter output = new System.IO.StreamWriter( filePrefix + "_timing.reason.txt", false, System.Text.Encoding.ASCII ) )
                {
                    PerformanceCounters.ContextualTiming.DumpAllByReason( output );
                }

                Console.WriteLine( "{0}: Done", GetTime( ) );
            }

            //
            // We don't need to serialized the prohibited set.
            //
            Console.WriteLine( "{0}: Saving Results for {1}", GetTime( ), filePrefix );
            m_typeSystem.DropCompileTimeObjects( );

            {
                Console.WriteLine( "{0}:     Image...", GetTime( ) );

                SaveIrToDisk( filePrefix + ".ZeligImage", m_typeSystem );

                Console.WriteLine( "{0}:     Image done", GetTime( ) );
            }

            if( m_fDumpIR )
            {
                Console.WriteLine( "{0}:     IR dump...", GetTime( ) );

                DumpIRAsText( filePrefix + ".ZeligIR" );

                Console.WriteLine( "{0}:     IR dump done", GetTime( ) );
            }

            if( m_fDumpFlattenedCallGraph )
            {
                Console.WriteLine( "{0}:     Flattened call graph dump...", GetTime( ) );
                
                DumpCallGraph( filePrefix + ".ZeligCallsToGraph"  , fCallsTo: true  );
                DumpCallGraph( filePrefix + ".ZeligCallsFromGraph", fCallsTo: false );

                Console.WriteLine( "{0}:     call graph dump done", GetTime( ) );
            }            

            if (m_fDumpIRXML)
            {
                Console.WriteLine("{0}:     IR dump (XML)...", GetTime());

                DumpIRAsXML(filePrefix + ".ZeligIR.xml");

                Console.WriteLine("{0}:     IR dump (XML) done", GetTime());
            }

            if( m_fDumpASMDIR )
            {
                Console.WriteLine( "{0}:     ASMDIR dump...", GetTime( ) );

                m_typeSystem.DisassembleImage( filePrefix + ".asmdir" );

                Console.WriteLine( "{0}:     ASMDIR dump done", GetTime( ) );
            }

            if( m_fDumpASM )
            {
                Console.WriteLine( "{0}:     ASM dump...", GetTime( ) );

                m_typeSystem.DisassembleImageInOneFile( filePrefix + ".asm" );

                Console.WriteLine( "{0}:     ASM dump done", GetTime( ) );
            }

            if( m_fDumpHEX )
            {
                Console.WriteLine( "{0}:     HEX file...", GetTime( ) );

                using( FileStream stream = new FileStream( filePrefix + ".hex", FileMode.Create ) )
                {
                    foreach( var section in m_typeSystem.Image )
                    {
                        ARM.SRecordParser.Encode( stream, section.Payload, section.Address );
                    }

                    TS.MethodRepresentation mdEntrypoint = m_typeSystem.TryGetHandler( Microsoft.Zelig.Runtime.HardwareException.Reset );
                    if( mdEntrypoint != null )
                    {
                        IR.ControlFlowGraphStateForCodeTransformation cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( mdEntrypoint );
                        if( cfg != null )
                        {
                            IR.ImageBuilders.SequentialRegion reg = m_typeSystem.ImageBuilder.GetAssociatedRegion( cfg );
                            if( reg != null )
                            {
                                ARM.SRecordParser.EncodeEntrypoint( stream, reg.BaseAddress.ToUInt32( ) );
                            }
                        }
                    }
                }

                Console.WriteLine( "{0}:     HEX file done", GetTime( ) );
            }

            if( m_fDumpLLVMIR_TextRepresentation )
            {
                Console.WriteLine( "Writing LLVM IR text representation" );
                m_typeSystem.Module.DumpToFile( filePrefix + ".ll", LLVM.OutputFormat.BitCodeSource );
            }

            if( m_fDumpLLVMIR )
            {
                Console.WriteLine( "Writing LLVM Bitcode representation" );
                m_typeSystem.Module.DumpToFile( filePrefix + ".bc", LLVM.OutputFormat.BitCodeBinary );
            }

            if( m_fDumpLLVMIR && !m_fSkipLlvmOptExe )
            {
                var optSwitches = BuildOptArchitectureArgs( ); 

                Console.WriteLine( "Optimizing LLVM Bitcode representation" );

                var args = string.Format( "{0} {1} -o {2}"          ,
                                        m_LlvmOptArgs ?? optSwitches,
                                        filePrefix + ".bc"          ,
                                        filePrefix + "_opt.bc" 
                                        );
                ShellExec( "opt.exe", args );
                ShellExec( "llvm-dis.exe", filePrefix + "_opt.bc" );
            }
            else if( m_fGenerateObj )
            {
                if(File.Exists(filePrefix + "_opt.bc"))
                {
                    File.Delete(filePrefix + "_opt.bc");
                }
                if(File.Exists(filePrefix + ".bc"))
                {
                    File.Copy(filePrefix + ".bc", filePrefix + "_opt.bc");
                }
            }

            if( m_fGenerateObj )
            {
                var objFile     = filePrefix + "_opt.o";
                var llcSwitches = BuildLlcArchitectureArgs(); 

                Console.WriteLine( "Compiling LLVM Bitcode" );
                var args = string.Format( "{0} -o={1} {2}"          ,
                                        m_LlvmLlcArgs ?? llcSwitches,
                                        objFile                     ,
                                        filePrefix + "_opt.bc"         
                                        );

                ShellExec( "llc.exe", args );

                if(m_architecture != c_x86_64)
                {
                    DumpElfInformation(objFile, filePrefix);
                }
            }

            foreach( var raw in m_dumpRawImage )
            {
                var mem = new MemoryStream( );

                foreach( var section in m_typeSystem.Image )
                {
                    uint sectionStart = section.Address;
                    uint sectionEnd   = sectionStart + ( uint )section.Payload.Length;

                    uint sectionStart2 = Math.Max( sectionStart, raw.RangeStart );
                    uint sectionEnd2   = Math.Min( sectionEnd, raw.RangeEnd );

                    if( sectionStart2 < sectionEnd2 )
                    {
                        uint offset = sectionStart2 - raw.RangeStart;

                        mem.SetLength( offset );
                        mem.Seek( offset, SeekOrigin.Begin );
                        mem.Write( section.Payload, 0, section.Payload.Length );
                    }
                }

                if( mem.Length > 0 )
                {
                    Console.WriteLine( "{0}:     RAW file...", GetTime( ) );

                    using( FileStream stream = new FileStream( filePrefix + "." + raw.SectionName + ".bin", FileMode.Create ) )
                    {
                        var buf = mem.ToArray( );

                        stream.Write( buf, 0, buf.Length );
                    }

                    Console.WriteLine( "{0}:     RAW file done", GetTime( ) );
                }
            }

            Console.WriteLine( "{0}: Done", GetTime( ) );
        }

        private string ExtractProductName( Type product )
        {
            var name = product.ToString();

            return name.LastIndexOf( '.' ) != -1 ? name.Substring( name.LastIndexOf( '.' ) + 1 ) : name; 
        }

        private object BuildLlcArchitectureArgs( )
        {
            return ConcatArgs( DefaultLlcArgs_common, GetLlcSwitchesForTargetArchitecture( ), DefaultLlcArgs_reloc ); 
        }

        private string BuildOptArchitectureArgs( )
        {
            return ConcatArgs( DefaultOptExeArgs_common, GetOptSwitchesForTargetArchitecture() ); 
        }

        private string GetOptSwitchesForTargetArchitecture( )
        {
            switch(m_architecture)
            {
                case c_CortexM0:
                    return DefaultOptArgs_target_m0;
                case c_CortexM3:
                    return DefaultOptArgs_target_m3;
                case c_CortexM4:
                    return DefaultOptArgs_target_m4;
                case c_CortexM7:
                    return DefaultOptArgs_target_m7;
                case c_x86_64:
                    return DefaultOptArgs_target_x86;
                default:
                    return DefaultOptArgs_target_df;
            }
        }

        private string GetLlcSwitchesForTargetArchitecture( )
        {
            switch(m_architecture)
            {
                case c_CortexM0:
                    return DefaultLlcArgs_target_m0;
                case c_CortexM3:
                    return DefaultLlcArgs_target_m3;
                case c_CortexM4:
                    return DefaultLlcArgs_target_m4;
                case c_CortexM7:
                    return DefaultLlcArgs_target_m7;
                case c_x86_64:
                    return DefaultLlcArgs_target_x86;
                default:
                    return DefaultLlcArgs_target_df;
            }
        }

        private string ConcatArgs( params string[] args )
        {
            var sb = new StringBuilder();

            for(int i = 0; i < args.Length; ++i)
            {
                sb.Append( args[i] );

                if(i < args.Length - 1)
                {
                    sb.Append( " " );
                }
            }

            return sb.ToString( ); 
        }

        private int ShellExec( string exe, string args )
        {
            var startInfo = new ProcessStartInfo( exe, args )
            { CreateNoWindow = true
            , UseShellExecute = false
            , RedirectStandardError = true
            , RedirectStandardOutput = true
            };

            // for reasons unknown launching a process via ProcessStart doesn't actually inherit the
            // STDOUT and STDERR so this has to capture the output from poth and then send it to the
            // console explicitly or the output from the process will just end up in the bit pool at
            // the bottom of the computer's chasis.
            var proc = new Process( ) { StartInfo = startInfo };

            proc.ErrorDataReceived += ( s, e ) => { if( e.Data != null ) Console.Error.WriteLine( e.Data ); };
            proc.OutputDataReceived += ( s, e ) => { if( e.Data != null ) Console.WriteLine( e.Data ); };
            proc.Start( );
            proc.BeginOutputReadLine( );
            proc.BeginErrorReadLine( );
            proc.WaitForExit( );

            return proc.ExitCode;
        }

        private void DumpElfInformation( string objFile, string filePrefix )
        {
            var objElfObjs = ELF.ElfObject.FileUtil.Parse( objFile );

            using(var sr = new System.IO.StreamWriter( filePrefix + "_opt.ElfDump" ))
            {
                foreach(var obj in objElfObjs)
                {
                    sr.WriteLine( ELF.OutputFormatter.PrintElfHeader( obj.Header ) );
                    sr.WriteLine( ELF.OutputFormatter.PrintSectionHeaders( obj.Sections ) );
                    sr.WriteLine( ELF.OutputFormatter.PrintAllSizes( ELF.OutputFormatter.ComputeAllSizes( obj.SymbolTable ) ) );

                    var llilumTypes    = new Dictionary<string, ELF.OutputFormatter.NameSizePair>();
                    var lillumMethods  = new List<ELF.OutputFormatter.NameSizeQuadruple>();
                    var otherFunctions = new List<ELF.OutputFormatter.NameSizeQuadruple>();

                    uint totalSize = ELF.OutputFormatter.ComputeAllSizesMethodByMethod( obj.SymbolTable, llilumTypes, lillumMethods, otherFunctions ); 
                    
                    sr.WriteLine( ELF.OutputFormatter.PrintTotalSizeMethodByMethod( totalSize ) );

                    sr.WriteLine( ); 
                    sr.WriteLine(  );

                    sr.WriteLine( ELF.OutputFormatter.PrintAllTypesSizes  ( llilumTypes  , otherFunctions ) );
                    sr.WriteLine( ELF.OutputFormatter.PrintAllMethodsSizes( lillumMethods, otherFunctions, bySizeOrder: false ) );
                    sr.WriteLine( ELF.OutputFormatter.PrintAllMethodsSizes( lillumMethods, otherFunctions, bySizeOrder: true ) );

                    sr.WriteLine( "################################################" );
                    sr.WriteLine( "########### CROSS CHECK INFORMATION ############" );
                    sr.WriteLine( "################################################" );

                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( "============================================================================================" );
                    sr.WriteLine( "  All types in llilum type system before LLVM code gen" );
                    sr.WriteLine( );
                    sr.WriteLine( "  Total types: " + m_typeSystem.Types.Count );
                    sr.WriteLine( );

                    var allTypes   = new List<TS.TypeRepresentation>();
                    var allMethods = new List<TS.MethodRepresentation>();

                    foreach(var tr in m_typeSystem.Types)
                    {
                        if(m_typeSystem.ShouldIncludeInCodeGenStats( tr ))
                        {
                            allTypes.Add( tr );

                            foreach(var mr in tr.Methods)
                            {
                                allMethods.Add( mr );
                            }
                        }
                    }

                    foreach(var t in allTypes)
                    {
                        sr.WriteLine( t.FullName );
                    }

                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( "============================================================================================" );
                    sr.WriteLine( "  All methods in llilum type system before LLVM code gen" );
                    sr.WriteLine( );
                    sr.WriteLine( "  Total methods: " + allMethods.Count );
                    sr.WriteLine( );

                    foreach(var mr in allMethods)
                    {
                        sr.WriteLine( mr.ToShortStringNoReturnValue( ) );
                    }

                    //--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//

                    //
                    // String table
                    //

                    List<string> stringTable = new List<string>();
                    var raw = obj.SymbolTable.StringTable;

                    int index = 0;
                    do
                    {
                        var s = raw.GetString( (uint)index ); 

                        if(string.IsNullOrEmpty( s ))
                        {
                            index++; continue;
                        }

                        stringTable.Add( s );

                        index += s.Length; // GetString will stop at the null separator

                    } while(index < raw.Header.sh_size); 

                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( "============================================================================================" );
                    sr.WriteLine( "  Strings table" );
                    sr.WriteLine( );
                    sr.WriteLine( "Total strings: " + stringTable.Count );
                    sr.WriteLine( );

                    for(int i = 0; i < stringTable.Count; i++)
                    {
                        sr.WriteLine( "{0:D6}: '{1}'", i, stringTable[i] );
                    }

                    //--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//--o--//

                    //
                    // Symbol table
                    //

                    var symbols = obj.SymbolTable.Symbols;

                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( "============================================================================================" );
                    sr.WriteLine( "  Symbols" );
                    sr.WriteLine( );
                    sr.WriteLine( "Total symbols: " + symbols.Length );
                    sr.WriteLine( );
                    
                    for(int i = 0; i < symbols.Length; i++)
                    {
                        var symbol = symbols[ i ];

                        sr.WriteLine( "{0:D6}: Name: '{1}'\r\nSize: {2}\r\nType: {3}\r\nReferencedSection: '{4}'\r\nBinding: {5}\r\nVisibility: {6}\r\n", 
                            i,
                            symbol.Name,
                            symbol.SymbolDef.st_size,
                            symbol.Type, 
                            symbol.ReferencedSection?.Name, 
                            symbol.Binding, 
                            symbol.Visibility 
                            );
                    }

                    //
                    // Done
                    // 

                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( "  Elf Object dump completed for {0}", obj.FileName );
                    sr.WriteLine( "============================================================================================" );
                    sr.WriteLine( );
                    sr.WriteLine( );
                    sr.WriteLine( );
                }
            }
        }

        internal static bool RunBench(string[] args)
        {
            bool fNoSDK = false;

            Bench bench = new Bench();
            Bench.s_pThis = bench;

            // path with space need to be re-assembled
            string[] recombinedArgs = RecombineArgs(args);

            if(recombinedArgs != null && bench.Parse( recombinedArgs, ref fNoSDK ))
            {
                if(bench.ValidateOptions( ))
                {
                    bench.Compile();
                }
            }

            return fNoSDK;
        }

        public static int Main( string[] args )
        {
            int retVal = 0;
            System.Threading.Thread th = new System.Threading.Thread( ()=>
            {
                bool fNoSDK = false;

                try
                {
                    fNoSDK = RunBench(args);
                }
                catch( Importer.SilentCompilationAbortException )
                {
                    retVal = -1;
                }
                catch( Exception ex )
                {
                    Console.Error.WriteLine( "Caught exception: {0}", ex );
                    retVal = ex.HResult > unchecked((int)0x80000000u) ? ex.HResult : -1;
                }
                finally
                {
                    if( System.Diagnostics.Debugger.IsAttached && fNoSDK == true)
                    {
                        Console.WriteLine( "Press <enter> to exit" );
                        Console.ReadLine( );
                    }
                }
            }, 1024*1024*1024 );
            th.Start( );
            th.Join( );
            return retVal;
        }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_SAVE_STATE
//#define DEBUG_RELOAD_STATE


using System.ComponentModel;
using Microsoft.Zelig.CodeGeneration.IR.Transformations;

namespace Microsoft.Zelig.FrontEnd
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    using Importer = Microsoft.Zelig.MetaData.Importer;
    using Normalized = Microsoft.Zelig.MetaData.Normalized;
    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg = Microsoft.Zelig.Configuration.Environment;
    using ARM = Microsoft.Zelig.Emulation.ArmProcessor;
    using LLVM;

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
        private bool                                m_fDumpIRXMLpre;
        private bool                                m_fDumpIRXMLpost;
        private bool                                m_fDumpIRXML;
        private bool                                m_fDumpCFG;
        private bool                                m_fDumpLLVMIR;
        private bool                                m_fDumpLLVMIR_TextRepresentation;
        private bool                                m_fDumpASM;
        private bool                                m_fDumpASMDIR;
        private bool                                m_fDumpHEX;
        private bool                                m_fSkipReadOnly;
        private uint                                m_nativeIntSize;
        private List< RawImage >                    m_dumpRawImage;

        private string                              m_libraryLocation_HostBuild;
        private string                              m_libraryLocation_TargetBuild;
        private string                              m_compilationSetupBinaryPath;

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

            m_outputDir = ".";

            m_nativeIntSize = 32;

            m_dumpRawImage = new List<RawImage>( );

            m_references = new List<string>( );
            m_searchOrder = new List<string>( );
            m_importDirectories = new List<string>( );
            m_importLibraries = new List<string>( );

            m_resolver = new Zelig.MetaData.MetaDataResolver( this );

            m_configurationOptions = HashTableFactory.New<string, object>( );

            m_disabledPhases = new List<String>( );

            m_sourceCodeTracker = new IR.SourceCodeTracker( );
            m_LlvmCodeGenOptions = new LlvmCodeGenOptions( );
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
            Console.WriteLine( "{0}: Phase: {1}", GetTime( ), phase );

#if DEBUG_SAVE_STATE
            if(phase == Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phase.GenerateImage)
            {
                SaveIrToDisk( "temp.ZeligImage", m_typeSystem );
            }
#endif

            ////        if(phase == IR.CompilationSteps.Phase.ReduceTypeSystem + 1)
            ////        {
            ////            string filePrefix = Path.Combine( m_outputDir, m_outputName );
            ////
            ////            DumpIRAsText( filePrefix + ".ZeligIR_post" );
            ////        }

            ////        switch(phase)
            ////        {
            ////            case Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phase.Optimizations:
            ////            case Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phase.AllocateRegisters:
            ////                Console.WriteLine( "Press ENTER" );
            ////                Console.ReadLine();
            ////                break;
            ////        }
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
                // We need to re-combine those entries
                
                // We must find a matching pair of double quotes. 
                // Matching quotes must appear in the same or next argument. 
                if( args[ i ].StartsWith( "\"" ) && !args[ i ].EndsWith("\"") )
                {
                    // Look in the very next argument.
                    // if we are at the last entry in args already, then 
                    // we have an unmatched quote we may be able to recover from
                    if( i == args.Length - 1 )
                    {
                        // ignore and hope for the best
                    }
                    else if( args[i + 1].EndsWith("\"") )
                    {
                        args[ i + 1 ] = args[ i ] + " " + args[ i + 1 ];
                        ++i;
                    }
                    else
                    {
                        // no matching double-quotes
                        return null;
                    }
                }
                arguments.Add( args[i] );
            }

            return arguments.ToArray( ); 
        }

        private bool Parse( string[] args, ref bool fNoSDK)
        {
            if( args != null )
            {
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
                        else if( IsMatch( option, "DumpIRXMLpre" ) )
                        {
                            m_fDumpIRXMLpre = true;
                        }
                        else if( IsMatch( option, "DumpIRXMLpost" ) )
                        {
                            m_fDumpIRXMLpost = true;
                        }
                        else if( IsMatch( option, "DumpIRXML" ) )
                        {
                            m_fDumpIRXML = true;
                        }
                        else if( IsMatch( option, "DumpLLVMIR" ) )
                        {
                            m_fDumpLLVMIR = true;
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
                                object res = t.InvokeMember( "Parse",
                                    System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static |
                                    System.Reflection.BindingFlags.Public, null, null, new object[ ] { value } );

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
                            m_LlvmCodeGenOptions.EnableAutoInlining = false;
                            m_LlvmCodeGenOptions.HonorInlineAttribute = false;
                            m_LlvmCodeGenOptions.InjectPrologAndEpilog = false;
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

            return false;
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
                                             bool fExpand )
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

            Console.WriteLine( "Option '{0}' needs an argument", arg );

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
            /*
            if( m_compilationSetup == null )
            {
                Console.Error.WriteLine( "Cannot compile without a compilation setup!" );
                return false;
            }*/

            return true;
        }

        //--//

        private void Compile( )
        {

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
                
                m_typeSystem.Product = m_product.Model;

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
                        foreach( var m in t.Methods )
                        {
                            IR.ControlFlowGraphStateForCodeTransformation cfg = IR.TypeSystemForCodeTransformation.GetCodeForMethod( m );

                            if( cfg != null )
                            {
                                cfg.DumpToFile( m.m_identity.ToString( ) + ".txt" );
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
                
                // todo: add compiler flag for these?
                //m_typeSystem.Module.DumpToFile( filePrefix + ".s", LLVM.OutputFormat.TargetAsmSource );
                //m_typeSystem.Module.DumpToFile( filePrefix + ".o", LLVM.OutputFormat.TargetObjectFile );
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

        //--//

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

        public static void Main( string[] args )
        {
            System.Threading.Thread th = new System.Threading.Thread( ()=>
            {
                bool fNoSDK = false;

                try
                {
                    fNoSDK = RunBench(args);
                }
                catch( Importer.SilentCompilationAbortException )
                {
                }
                catch( Exception ex )
                {
                    Console.Error.WriteLine( "Caught exception: {0}", ex );
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
        }
    }
}

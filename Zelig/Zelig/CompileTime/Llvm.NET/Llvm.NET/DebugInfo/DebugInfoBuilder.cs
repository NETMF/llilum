using Llvm.NET.Dwarf;
using System;
using System.Linq;

namespace Llvm.NET.DebugInfo
{
    /// <summary>DebugInfoBuilder is a factory class for creating DebugInformation for an LLVM <see cref="Module"/></summary>
    /// <remarks>
    /// Many Debug information metadata nodes are created with unresolved references to additional metadata. To ensure such
    /// metadata is resolved applications should call the <see cref="Finish"/> method to resolve and finalize the metadata.
    /// After this point only fully resolved nodes may be added to ensure that the data remains valid.
    /// </remarks>
    public sealed class DebugInfoBuilder : IDisposable
    {
        public DebugInfoBuilder( Module owningModule )
            : this( owningModule, true )
        {
        }

        // keeping this private for now as there doesn't seem to be a good reason to support
        // allowUnresolved == false
        private DebugInfoBuilder( Module owningModule, bool allowUnresolved )
        {
            BuilderHandle = LLVMNative.NewDIBuilder( owningModule.ModuleHandle, allowUnresolved );
        }

        public CompileUnit CreateCompileUnit( SourceLanguage language
                                            , string fileName
                                            , string filePath
                                            , string producer
                                            , bool optimized
                                            , string flags
                                            , uint runtimeVersion
                                            )
        {
            var handle = LLVMNative.DIBuilderCreateCompileUnit( BuilderHandle
                                                              , ( uint )language
                                                              , fileName
                                                              , filePath
                                                              , producer
                                                              , optimized ? 1 : 0
                                                              , flags
                                                              , runtimeVersion
                                                              );
            return new CompileUnit( handle );
        }

        public File CreateFile( string path )
        {
            return CreateFile( System.IO.Path.GetFileName( path ), System.IO.Path.GetDirectoryName( path ) );
        }

        public File CreateFile( string fileName, string directory )
        {
            if( string.IsNullOrWhiteSpace( fileName ) )
                throw new ArgumentException( "File name cannot be empty or null" );

            var handle = LLVMNative.DIBuilderCreateFile( BuilderHandle, fileName, directory??string.Empty );
            // REVIEW: should this deal with uniquing? if so, is it per context? Per module? ...?
            return new File( handle );
        }

        public LexicalBlock CreateLexicalBlock( Scope scope, File file, uint line, uint column )
        {
            var handle = LLVMNative.DIBuilderCreateLexicalBlock( BuilderHandle, scope.MetadataHandle, file.MetadataHandle, line, column );
            return new LexicalBlock( handle );
        }

        public LexicalBlockFile CreateLexicalBlockFile( Scope scope, File file, uint discriminator )
        {
            var handle = LLVMNative.DIBuilderCreateLexicalBlockFile( BuilderHandle, scope.MetadataHandle, file.MetadataHandle, discriminator );
            return new LexicalBlockFile( handle );
        }

        public SubProgram CreateFunction( Scope scope
                                        , string name
                                        , string mangledName
                                        , File file
                                        , uint line
                                        , CompositeType compositeType
                                        , bool isLocalToUnit
                                        , bool isDefinition
                                        , uint scopeLine
                                        , uint flags
                                        , bool isOptimized
                                        , Function function
                                        )
        {
            var handle = LLVMNative.DIBuilderCreateFunction( BuilderHandle
                                                           , scope.MetadataHandle
                                                           , name
                                                           , mangledName
                                                           , file.MetadataHandle
                                                           , line
                                                           , compositeType.MetadataHandle
                                                           , isLocalToUnit ? 1 : 0
                                                           , isDefinition ? 1 : 0
                                                           , scopeLine
                                                           , flags
                                                           , isOptimized ? 1 : 0
                                                           , function.ValueHandle
                                                           );
            return new SubProgram( handle );
        }

        public Variable CreateLocalVariable( uint dwarfTag
                                           , Scope scope
                                           , string name
                                           , File file
                                           , uint line
                                           , Type type
                                           , bool alwaysPreserve
                                           , uint flags
                                           , uint argNo
                                           )
        {
            var handle = LLVMNative.DIBuilderCreateLocalVariable( BuilderHandle
                                                                , dwarfTag, scope.MetadataHandle
                                                                , name
                                                                , file.MetadataHandle
                                                                , line
                                                                , type.MetadataHandle
                                                                , alwaysPreserve ? 1 : 0
                                                                , flags
                                                                , argNo
                                                                );
            return new Variable( handle );
        }

        public BasicType CreateBasicType( string name, ulong bitSize, ulong bitAlign, uint encoding )
        {
            var handle = LLVMNative.DIBuilderCreateBasicType( BuilderHandle, name, bitSize, bitAlign, encoding );
            return new BasicType( handle );
        }

        public DerivedType CreatePointerType( Type pointeeType, string name, ulong bitSize, ulong bitAlign)
        {
            var handle = LLVMNative.DIBuilderCreatePointerType( BuilderHandle, pointeeType.MetadataHandle, bitSize, bitAlign, name );
            return new DerivedType( handle );
        }

        public TypeArray CreateTypeArray( params Type[ ] types )
        {
            var handles = types.Select( t => t.MetadataHandle ).ToArray( );
            var count = handles.LongLength;
            if( count == 0 )
                handles = new LLVMMetadataRef[ ] { default( LLVMMetadataRef ) };

            var handle = LLVMNative.DIBuilderGetOrCreateTypeArray( BuilderHandle, out handles[ 0 ], (ulong)count );
            return new TypeArray( handle );
        }

        public SubroutineType CreateSubroutineType( File file, TypeArray types )
        {
            var handle = LLVMNative.DIBuilderCreateSubroutineType( BuilderHandle, file.MetadataHandle, types.MetadataHandle );
            return new SubroutineType( handle );
        }

        public SubroutineType CreateSubroutineType( File file, params Type[] types )
        {
            var typeArray = GetOrCreateTypeArray( types );
            return CreateSubroutineType( file, typeArray );
        }

        public CompositeType CreateStructType( Scope scope
                                             , string name
                                             , File file
                                             , uint line
                                             , ulong bitSize
                                             , ulong bitAlign
                                             , uint flags
                                             , Type derivedFrom
                                             , Array elements
                                             )
        {
            var handle = LLVMNative.DIBuilderCreateStructType( BuilderHandle
                                                             , scope.MetadataHandle
                                                             , name
                                                             , file.MetadataHandle
                                                             , line
                                                             , bitSize
                                                             , bitAlign
                                                             , flags
                                                             , derivedFrom.MetadataHandle
                                                             , elements.MetadataHandle
                                                             );
            return new CompositeType( handle );
        }

        public CompositeType CreateStructType( Scope scope
                                             , string name
                                             , File file
                                             , uint line
                                             , ulong bitSize
                                             , ulong bitAlign
                                             , uint flags
                                             , Type derivedFrom
                                             , params Descriptor[] elements
                                             )
        {
            return CreateStructType( scope, name, file, line, bitSize, bitAlign, flags, derivedFrom, GetOrCreateArray( elements ) );
        }

        public DerivedType CreateMemberType( Scope scope
                                           , string name
                                           , File file
                                           , uint line
                                           , ulong bitSize
                                           , ulong bitAlign
                                           , ulong bitOffset
                                           , uint flags
                                           , Type type
                                           )
        {
            var handle = LLVMNative.DIBuilderCreateMemberType( BuilderHandle
                                                             , scope.MetadataHandle
                                                             , name
                                                             , file.MetadataHandle
                                                             , line
                                                             , bitSize
                                                             , bitAlign
                                                             , bitOffset
                                                             , flags
                                                             , type.MetadataHandle
                                                             );
            return new DerivedType( handle );
        }

        public CompositeType CreateArrayType( ulong bitSize, ulong bitAlign, Type elementType, Array subScripts )
        {
            var handle = LLVMNative.DIBuilderCreateArrayType( BuilderHandle, bitSize, bitAlign, elementType.MetadataHandle, subScripts.MetadataHandle );
            return new CompositeType( handle );
        }

        public CompositeType CreateArrayType( ulong bitSize, ulong bitAlign, Type elementType, params Descriptor[] subScripts )
        {
            return CreateArrayType( bitSize, bitAlign, elementType, GetOrCreateArray( subScripts ) );
        }

        public DerivedType CreateTypedef(Type type, string name, File file, uint line, Descriptor context )
        {
            var handle = LLVMNative.DIBuilderCreateTypedef( BuilderHandle, type.MetadataHandle, name, file.MetadataHandle, line, context.MetadataHandle );
            return new DerivedType( handle );
        }

        public Subrange CreateSubrange( long lo, long count )
        {
            var handle = LLVMNative.DIBuilderGetOrCreateSubrange( BuilderHandle, lo, count );
            return new Subrange( handle );
        }

        public Array GetOrCreateArray( Descriptor[ ] elements )
        {
            var buf = elements.Select( d => d.MetadataHandle ).ToArray( );
            var handle = LLVMNative.DIBuilderGetOrCreateArray( BuilderHandle, out buf[ 0 ], ( ulong )buf.LongLength );
            return new Array( handle );
        }

        public TypeArray GetOrCreateTypeArray( Type[] types )
        {
            var buf = types.Select( t => t.MetadataHandle ).ToArray();
            var handle = LLVMNative.DIBuilderGetOrCreateTypeArray( BuilderHandle, out buf[ 0 ], (ulong)buf.LongLength );
            return new TypeArray( handle );
        }

        public void Finish()
        {
            if( !IsFinished )
            {
                LLVMNative.DIBuilderFinalize( BuilderHandle );
                IsFinished = true;
            }
        }

        public void Dispose( )
        {
            if( BuilderHandle.Pointer != IntPtr.Zero )
            {
                LLVMNative.DIBuilderDestroy( BuilderHandle );
                BuilderHandle = default(LLVMDIBuilderRef);
            }
        }

        private bool IsFinished;
        internal LLVMDIBuilderRef BuilderHandle { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Llvm.NET.Values;
using Llvm.NET.Instructions;

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

        public DICompileUnit CreateCompileUnit( SourceLanguage language
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
            return new DICompileUnit( handle );
        }

        public DIFile CreateFile( string path )
        {
            if( string.IsNullOrWhiteSpace( path ) )
                throw new ArgumentException( "Path cannot be null, empty or whitespace" );

            return CreateFile( System.IO.Path.GetFileName( path ), System.IO.Path.GetDirectoryName( path ) );
        }

        public DIFile CreateFile( string fileName, string directory )
        {
            if( string.IsNullOrWhiteSpace( fileName ) )
                throw new ArgumentException( "File name cannot be empty or null" );

            var handle = LLVMNative.DIBuilderCreateFile( BuilderHandle, fileName, directory??string.Empty );
            // REVIEW: should this deal with uniquing? if so, is it per context? Per module? ...?
            return new DIFile( handle );
        }

        public DILexicalBlock CreateLexicalBlock( DIScope scope, DIFile file, uint line, uint column )
        {
            var handle = LLVMNative.DIBuilderCreateLexicalBlock( BuilderHandle, scope.MetadataHandle, file.MetadataHandle, line, column );
            return new DILexicalBlock( handle );
        }

        public DILexicalBlockFile CreateLexicalBlockFile( DIScope scope, DIFile file, uint discriminator )
        {
            var handle = LLVMNative.DIBuilderCreateLexicalBlockFile( BuilderHandle, scope.MetadataHandle, file.MetadataHandle, discriminator );
            return new DILexicalBlockFile( handle );
        }

        public DISubProgram CreateFunction( DIScope scope
                                        , string name
                                        , string mangledName
                                        , DIFile file
                                        , uint line
                                        , DICompositeType compositeType
                                        , bool isLocalToUnit
                                        , bool isDefinition
                                        , uint scopeLine
                                        , uint flags
                                        , bool isOptimized
                                        , Function function
                                        )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                name = string.Empty;

            if( string.IsNullOrWhiteSpace( mangledName ) )
                mangledName = string.Empty;

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
            return new DISubProgram( handle );
        }

        public DILocalVariable CreateLocalVariable( uint dwarfTag
                                                , DIScope scope
                                                , string name
                                                , DIFile file
                                                , uint line
                                                , DIType type
                                                , bool alwaysPreserve
                                                , uint flags
                                                , uint argNo
                                                )
        {
            var handle = LLVMNative.DIBuilderCreateLocalVariable( BuilderHandle
                                                                , dwarfTag
                                                                , scope.MetadataHandle
                                                                , name
                                                                , file.MetadataHandle
                                                                , line
                                                                , type.MetadataHandle
                                                                , alwaysPreserve ? 1 : 0
                                                                , flags
                                                                , argNo
                                                                );
            return new DILocalVariable( handle );
        }

        public DIBasicType CreateBasicType( string name, ulong bitSize, ulong bitAlign, DiTypeKind encoding )
        {
            var handle = LLVMNative.DIBuilderCreateBasicType( BuilderHandle, name, bitSize, bitAlign, (uint)encoding );
            return new DIBasicType( handle );
        }

        public DIDerivedType CreatePointerType( DIType pointeeType, string name, ulong bitSize, ulong bitAlign)
        {
            var handle = LLVMNative.DIBuilderCreatePointerType( BuilderHandle, pointeeType.MetadataHandle, bitSize, bitAlign, name ?? string.Empty );
            return new DIDerivedType( handle );
        }
        public DITypeArray CreateTypeArray( params DIType[ ] types ) => CreateTypeArray( ( IEnumerable<DIType> )types );

        public DITypeArray CreateTypeArray( IEnumerable<DIType> types )
        {
            var handles = types.Select( t => t.MetadataHandle ).ToArray( );
            var count = handles.LongLength;
            if( count == 0 )
                handles = new LLVMMetadataRef[ ] { default( LLVMMetadataRef ) };

            var handle = LLVMNative.DIBuilderGetOrCreateTypeArray( BuilderHandle, out handles[ 0 ], (ulong)count );
            return new DITypeArray( handle );
        }

        public DISubroutineType CreateSubroutineType( DIFile file, uint flags, DITypeArray types )
        {
            var handle = LLVMNative.DIBuilderCreateSubroutineType( BuilderHandle, file.MetadataHandle, types.MetadataHandle, flags );
            return new DISubroutineType( handle );
        }

        public DISubroutineType CreateSubroutineType( DIFile file, uint flags )
        {
            var typeArray = GetOrCreateTypeArray( null );
            return CreateSubroutineType( file, flags, typeArray );
        }
        public DISubroutineType CreateSubroutineType( DIFile file, uint flags, DIType returnType, IEnumerable<DIType> types )
        {
            var typeArray = GetOrCreateTypeArray( ScalarEnumerable.Combine( returnType, types ) );
            return CreateSubroutineType( file, flags, typeArray );
        }

        public DICompositeType CreateStructType( DIScope scope
                                             , string name
                                             , DIFile file
                                             , uint line
                                             , ulong bitSize
                                             , ulong bitAlign
                                             , uint flags
                                             , DIType derivedFrom
                                             , DIArray elements
                                             )
        {
            var handle = LLVMNative.DIBuilderCreateStructType( BuilderHandle
                                                             , scope.MetadataHandle
                                                             , name
                                                             , file?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                             , line
                                                             , bitSize
                                                             , bitAlign
                                                             , flags
                                                             , derivedFrom?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                             , elements.MetadataHandle
                                                             );
            return new DICompositeType( handle );
        }

        public DICompositeType CreateStructType( DIScope scope
                                             , string name
                                             , DIFile file
                                             , uint line
                                             , ulong bitSize
                                             , ulong bitAlign
                                             , uint flags
                                             , DIType derivedFrom
                                             , params DINode[] elements
                                             )
        {
            return CreateStructType( scope, name, file, line, bitSize, bitAlign, flags, derivedFrom, GetOrCreateArray( elements ) );
        }

        public DICompositeType CreateStructType( DIScope scope
                                             , string name
                                             , DIFile file
                                             , uint line
                                             , ulong bitSize
                                             , ulong bitAlign
                                             , uint flags
                                             , DIType derivedFrom
                                             , IEnumerable<DINode> elements
                                             )
        {
            return CreateStructType( scope, name, file, line, bitSize, bitAlign, flags, derivedFrom, GetOrCreateArray( elements ) );
        }

        public DIDerivedType CreateMemberType( DIScope scope
                                           , string name
                                           , DIFile file
                                           , uint line
                                           , ulong bitSize
                                           , ulong bitAlign
                                           , ulong bitOffset
                                           , uint flags
                                           , DIType type
                                           )
        {
            var handle = LLVMNative.DIBuilderCreateMemberType( BuilderHandle
                                                             , scope.MetadataHandle
                                                             , name
                                                             , file?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                             , line
                                                             , bitSize
                                                             , bitAlign
                                                             , bitOffset
                                                             , flags
                                                             , type.MetadataHandle
                                                             );
            return new DIDerivedType( handle );
        }

        public DICompositeType CreateArrayType( ulong bitSize, ulong bitAlign, DIType elementType, DIArray subScripts )
        {
            var handle = LLVMNative.DIBuilderCreateArrayType( BuilderHandle, bitSize, bitAlign, elementType.MetadataHandle, subScripts.MetadataHandle );
            return new DICompositeType( handle );
        }

        public DICompositeType CreateArrayType( ulong bitSize, ulong bitAlign, DIType elementType, params DINode[] subScripts )
        {
            return CreateArrayType( bitSize, bitAlign, elementType, GetOrCreateArray( subScripts ) );
        }

        public DIDerivedType CreateTypedef(DIType type, string name, DIFile file, uint line, DINode context )
        {
            var handle = LLVMNative.DIBuilderCreateTypedef( BuilderHandle, type.MetadataHandle, name, file.MetadataHandle, line, context.MetadataHandle );
            return new DIDerivedType( handle );
        }

        public DISubrange CreateSubrange( long lo, long count )
        {
            var handle = LLVMNative.DIBuilderGetOrCreateSubrange( BuilderHandle, lo, count );
            return new DISubrange( handle );
        }

        public DIArray GetOrCreateArray( IEnumerable<DINode> elements )
        {
            var buf = elements.Select( d => d?.MetadataHandle ?? LLVMMetadataRef.Zero ).ToArray( );
            var actualLen = buf.LongLength;
            // for the out parameter trick to work - need to have a valid array with at least one element
            if( buf.LongLength == 0 )
                buf = new LLVMMetadataRef[ 1 ];

            var handle = LLVMNative.DIBuilderGetOrCreateArray( BuilderHandle, out buf[ 0 ], ( ulong )actualLen );
            return new DIArray( handle );
        }

        public DITypeArray GetOrCreateTypeArray( params DIType[ ] types ) => GetOrCreateTypeArray( ( IEnumerable<DIType> )types );
        public DITypeArray GetOrCreateTypeArray( IEnumerable<DIType> types )
        {
            var buf = types.Select( t => t?.MetadataHandle ?? LLVMMetadataRef.Zero ).ToArray();
            var handle = LLVMNative.DIBuilderGetOrCreateTypeArray( BuilderHandle, out buf[ 0 ], (ulong)buf.LongLength );
            return new DITypeArray( handle );
        }

        public DIEnumerator CreateEnumeratorValue( string name, long value )
        {
            var handle = LLVMNative.DIBuilderCreateEnumeratorValue( BuilderHandle, name, value );
            return new DIEnumerator( handle );
        }

        public DICompositeType CreateEnumerationType( DIScope scope
                                                  , string name
                                                  , DIFile file
                                                  , uint lineNumber
                                                  , ulong sizeInBits
                                                  , ulong alignInBits
                                                  , IEnumerable<DIEnumerator> elements
                                                  , DIType underlyingType
                                                  , string uniqueId = ""
                                                  )
        {
            var elementHandles = elements.Select( e => e.MetadataHandle ).ToArray( );
            var elementArray = LLVMNative.DIBuilderGetOrCreateArray( BuilderHandle, out elementHandles[ 0 ], (ulong)elementHandles.LongLength );
            var handle = LLVMNative.DIBuilderCreateEnumerationType( BuilderHandle
                                                                  , scope.MetadataHandle
                                                                  , name
                                                                  , file.MetadataHandle
                                                                  , lineNumber
                                                                  , sizeInBits
                                                                  , alignInBits
                                                                  , elementArray
                                                                  , underlyingType.MetadataHandle
                                                                  , uniqueId
                                                                  );
            return new DICompositeType( handle );
        }

        public DIGlobalVariable CreateGlobalVariable( DINode scope
                                                  , string name
                                                  , string linkageName
                                                  , DIFile file
                                                  , uint lineNo
                                                  , DIType type
                                                  , bool isLocalToUnit
                                                  , Value value
                                                  , DINode decl = null
                                                  )
        {
            var handle = LLVMNative.DIBuilderCreateGlobalVariable( BuilderHandle
                                                                 , scope.MetadataHandle
                                                                 , name
                                                                 , linkageName
                                                                 , file.MetadataHandle
                                                                 , lineNo
                                                                 , type.MetadataHandle
                                                                 , isLocalToUnit
                                                                 , value.ValueHandle
                                                                 , decl?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                                 );
            return new DIGlobalVariable( handle );
        }

        public void Finish()
        {
            if( !IsFinished )
            {
                LLVMNative.DIBuilderFinalize( BuilderHandle );
                IsFinished = true;
            }
        }

        public Value InsertDeclare( Value storage, DILocalVariable varInfo, DILocation location, Instruction insertBefore )
        {
            return InsertDeclare( storage, varInfo, CreateExpression( ), location, insertBefore );
        }

        public Value InsertDeclare( Value storage, DILocalVariable varInfo, DIExpression expr, DILocation location, Instruction insertBefore )
        {
            var handle = LLVMNative.DIBuilderInsertDeclareBefore( BuilderHandle
                                                                , storage.ValueHandle
                                                                , varInfo.MetadataHandle
                                                                , expr.MetadataHandle
                                                                , location.MetadataHandle
                                                                , insertBefore.ValueHandle
                                                                );
            return Value.FromHandle( handle );
        }

        public Value InsertDeclare( Value storage, DILocalVariable varInfo, DILocation location, BasicBlock insertAtEnd )
        {
            return InsertDeclare( storage, varInfo, CreateExpression( ), location, insertAtEnd );
        }

        public Value InsertDeclare( Value storage, DILocalVariable varInfo, DIExpression expr, DILocation location, BasicBlock insertAtEnd )
        {
            var handle = LLVMNative.DIBuilderInsertDeclareAtEnd( BuilderHandle
                                                                , storage.ValueHandle
                                                                , varInfo.MetadataHandle
                                                                , expr.MetadataHandle
                                                                , location.MetadataHandle
                                                                , insertAtEnd.BlockHandle
                                                                );
            return Value.FromHandle( handle );
        }

        public DIExpression CreateExpression( params ExpressionOp[ ] operations ) => CreateExpression( ( IEnumerable<ExpressionOp> )operations );

        public DIExpression CreateExpression( IEnumerable<ExpressionOp> operations )
        {
            var args = operations.Cast<long>().ToArray( );
            var actualCount = args.LongLength;
            if( args.Length == 0 )
                args = new long[ 1 ];

            var handle = LLVMNative.DIBuilderCreateExpression( BuilderHandle, out args[ 0 ], (ulong)actualCount );
            return new DIExpression( handle );
        }

        public DICompositeType CreateReplaceableCompositeType( Tag tag
                                                              , string name
                                                              , DINode scope
                                                              , DIFile file
                                                              , uint line
                                                              , uint lang = 0
                                                              , ulong sizeInBits = 0
                                                              , ulong alignBits = 0
                                                              , uint flags = 0
                                                              )
        {
            var handle = LLVMNative.DIBuilderCreateReplaceableCompositeType( BuilderHandle
                                                                           , ( uint )tag
                                                                           , name
                                                                           , scope.MetadataHandle
                                                                           , file?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                                           , line
                                                                           , lang
                                                                           , sizeInBits
                                                                           , alignBits
                                                                           , flags
                                                                           );
            return new DICompositeType( handle );
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

using System;
using System.Collections.Generic;
using System.Linq;
using Llvm.NET.Values;
using Llvm.NET.Instructions;
using System.IO;

namespace Llvm.NET.DebugInfo
{
    /// <summary>DebugInfoBuilder is a factory class for creating DebugInformation for an LLVM
    /// <see cref="Module"/></summary>
    /// <remarks>
    /// Many Debug information metadata nodes are created with unresolved references to additional
    /// metadata. To ensure such metadata is resolved applications should call the <see cref="Finish"/>
    /// method to resolve and finalize the metadata. After this point only fully resolved nodes may
    /// be added to ensure that the data remains valid.
    /// </remarks>
    public sealed class DebugInfoBuilder : IDisposable
    {
        /// <summary>Creates a new <see cref="DICompileUnit"/></summary>
        /// <param name="language"><see cref="SourceLanguage"/> for the compilation unit</param>
        /// <param name="srcFilePath">Full path to the source file of this compilation unit</param>
        /// <param name="producer">Name of the application processing the compilation unit</param>
        /// <param name="optimized">Flag to indicate if the code in this compilation unit is optimized</param>
        /// <param name="flags">Additional tool specific flags</param>
        /// <param name="runtimeVersion">Runtime version</param>
        /// <returns><see cref="DICompileUnit"/></returns>
        public DICompileUnit CreateCompileUnit( SourceLanguage language
                                              , string srcFilePath
                                              , string producer
                                              , bool optimized
                                              , string flags
                                              , uint runtimeVersion
                                              )
        {
            return CreateCompileUnit( language
                                    , Path.GetFileName( srcFilePath )
                                    , Path.GetDirectoryName( srcFilePath )?? Environment.CurrentDirectory
                                    , producer
                                    , optimized
                                    , flags
                                    , runtimeVersion
                                    );
        }

        /// <summary>Creates a new <see cref="DICompileUnit"/></summary>
        /// <param name="language"><see cref="SourceLanguage"/> for the compilation unit</param>
        /// <param name="fileName">Name of the source file of this compilation unit (without any path)</param>
        /// <param name="fileDirectory">Path of the directory containing the file</param>
        /// <param name="producer">Name of the application processing the compilation unit</param>
        /// <param name="optimized">Flag to indicate if the code in this compilation unit is optimized</param>
        /// <param name="flags">Additional tool specific flags</param>
        /// <param name="runtimeVersion">Runtime version</param>
        /// <returns><see cref="DICompileUnit"/></returns>
        public DICompileUnit CreateCompileUnit( SourceLanguage language
                                              , string fileName
                                              , string fileDirectory
                                              , string producer
                                              , bool optimized
                                              , string flags
                                              , uint runtimeVersion
                                              )
        {
            if( OwningModule.DICompileUnit != null )
                throw new InvalidOperationException( "LLVM only allows one DICompileUnit per module" );

            var handle = NativeMethods.DIBuilderCreateCompileUnit( BuilderHandle
                                                                 , ( uint )language
                                                                 , fileName
                                                                 , fileDirectory
                                                                 , producer
                                                                 , optimized ? 1 : 0
                                                                 , flags
                                                                 , runtimeVersion
                                                                 );
            var retVal = DINode.FromHandle< DICompileUnit >( handle );
            OwningModule.DICompileUnit = retVal;
            return retVal;
        }

        /// <summary>Creates a <see cref="DINamespace"/></summary>
        /// <param name="scope">Containing scope for the namespace or null if the namespace is a global one</param>
        /// <param name="name">Name of the namespace</param>
        /// <param name="file">Source file containing the declaration (may be null if more than one or not known)</param>
        /// <param name="line">Line number of the namespace declaration</param>
        /// <returns></returns>
        public DINamespace CreateNamespace( DIScope scope, string name, DIFile file, uint line )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentException( "name cannot be null or empty", nameof( name ) );

            var handle = NativeMethods.DIBuilderCreateNamespace( BuilderHandle
                                                               , scope?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                               , name
                                                               , file?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                               , line
                                                               );
            return DINode.FromHandle< DINamespace >( handle );
        }

        /// <summary>Creates a <see cref="DIFile"/></summary>
        /// <param name="path">Path of the file (may be <see langword="null"/> or empty)</param>
        /// <returns>
        /// <see cref="DIFile"/> or <see langword="null"/> if <paramref name="path"/>
        /// is <see langword="null"/> empty, or all whitespace
        /// </returns>
        public DIFile CreateFile( string path )
        {
            if( string.IsNullOrWhiteSpace( path ) )
                return null;

            return CreateFile( Path.GetFileName( path ), Path.GetDirectoryName( path ) );
        }

        /// <summary>Creates a <see cref="DIFile"/></summary>
        /// <param name="fileName">Name of the file (may be <see langword="null"/> or empty)</param>
        /// <param name="directory">Path of the directory containing the file (may be <see langword="null"/> or empty)</param>
        /// <returns>
        /// <see cref="DIFile"/> or <see langword="null"/> if <paramref name="fileName"/>
        /// is <see langword="null"/> empty, or all whitespace
        /// </returns>
        public DIFile CreateFile( string fileName, string directory )
        {
            if( string.IsNullOrWhiteSpace( fileName ) )
                return null;

            var handle = NativeMethods.DIBuilderCreateFile( BuilderHandle, fileName, directory??string.Empty );
            // REVIEW: should this deal with uniquing? if so, is it per context? Per module? ...?
            return DINode.FromHandle< DIFile >( handle );
        }

        public DILexicalBlock CreateLexicalBlock( DIScope scope, DIFile file, uint line, uint column )
        {
            var handle = NativeMethods.DIBuilderCreateLexicalBlock( BuilderHandle, scope.MetadataHandle, file.MetadataHandle, line, column );
            return DINode.FromHandle< DILexicalBlock >( handle );
        }

        public DILexicalBlockFile CreateLexicalBlockFile( DIScope scope, DIFile file, uint discriminator )
        {
            var handle = NativeMethods.DIBuilderCreateLexicalBlockFile( BuilderHandle, scope.MetadataHandle, file.MetadataHandle, discriminator );
            return DINode.FromHandle< DILexicalBlockFile >( handle );
        }

        public DISubProgram CreateFunction( DIScope scope
                                          , string name
                                          , string mangledName
                                          , DIFile file
                                          , uint line
                                          , DISubroutineType signatureType
                                          , bool isLocalToUnit
                                          , bool isDefinition
                                          , uint scopeLine
                                          , DebugInfoFlags flags
                                          , bool isOptimized
                                          , Function function
                                          , MDNode TParam = null
                                          , MDNode Decl = null
                                          )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                name = string.Empty;

            if( string.IsNullOrWhiteSpace( mangledName ) )
                mangledName = string.Empty;

            var handle = NativeMethods.DIBuilderCreateFunction( BuilderHandle
                                                              , scope.MetadataHandle
                                                              , name
                                                              , mangledName
                                                              , file?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                              , line
                                                              , signatureType.MetadataHandle
                                                              , isLocalToUnit ? 1 : 0
                                                              , isDefinition ? 1 : 0
                                                              , scopeLine
                                                              , (uint)flags
                                                              , isOptimized ? 1 : 0
                                                              , function.ValueHandle
                                                              , TParam?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                              , Decl?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                              );
            return DINode.FromHandle< DISubProgram >( handle );
        }

        public DISubProgram ForwardDeclareFunction( DIScope scope
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
                                                  , Function function = null
                                                  , MDNode TParam = null
                                                  , MDNode Decl = null
                                                  )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                name = string.Empty;

            if( string.IsNullOrWhiteSpace( mangledName ) )
                mangledName = string.Empty;

            var handle = NativeMethods.DIBuilderCreateTempFunctionFwdDecl( BuilderHandle
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
                                                                         , function?.ValueHandle ?? LLVMValueRef.Zero
                                                                         , TParam?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                                         , Decl?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                                         );
            return DINode.FromHandle< DISubProgram >( handle );
        }

        public DILocalVariable CreateLocalVariable( DIScope scope
                                                  , string name
                                                  , DIFile file
                                                  , uint line
                                                  , DIType type
                                                  , bool alwaysPreserve
                                                  , uint flags
                                                  , uint argNo
                                                  )
        {
            return CreateLocalVariable( Tag.AutoVariable, scope, name, file, line, type, alwaysPreserve, flags, argNo );
        }

        public DILocalVariable CreateArgument( DIScope scope
                                             , string name
                                             , DIFile file
                                             , uint line
                                             , DIType type
                                             , bool alwaysPreserve
                                             , uint flags
                                             , uint argNo
                                             )
        {
            return CreateLocalVariable( Tag.ArgVariable, scope, name, file, line, type, alwaysPreserve, flags, argNo );
        }

        public DIBasicType CreateBasicType( string name, ulong bitSize, ulong bitAlign, DiTypeKind encoding )
        {
            var handle = NativeMethods.DIBuilderCreateBasicType( BuilderHandle, name, bitSize, bitAlign, (uint)encoding );
            return DINode.FromHandle< DIBasicType >( handle );
        }

        public DIDerivedType CreatePointerType( DIType pointeeType, string name, ulong bitSize, ulong bitAlign)
        {
            var handle = NativeMethods.DIBuilderCreatePointerType( BuilderHandle
                                                                 , pointeeType?.MetadataHandle ?? LLVMMetadataRef.Zero // null == void
                                                                 , bitSize
                                                                 , bitAlign
                                                                 , name ?? string.Empty
                                                                 );
            return DINode.FromHandle< DIDerivedType >( handle );
        }

        public DIDerivedType CreateQualifiedType( DIType baseType, QualifiedTypeTag tag )
        {
            var handle = NativeMethods.DIBuilderCreateQualifiedType( BuilderHandle, ( uint )tag, baseType.MetadataHandle );
            return DINode.FromHandle< DIDerivedType >( handle );
        }

        public DITypeArray CreateTypeArray( params DIType[ ] types ) => CreateTypeArray( ( IEnumerable<DIType> )types );

        public DITypeArray CreateTypeArray( IEnumerable<DIType> types )
        {
            var handles = types.Select( t => t.MetadataHandle ).ToArray( );
            var count = handles.LongLength;
            if( count == 0 )
                handles = new [ ] { default( LLVMMetadataRef ) };

            var handle = NativeMethods.DIBuilderGetOrCreateTypeArray( BuilderHandle, out handles[ 0 ], (ulong)count );
            return new DITypeArray( handle );
        }

        public DISubroutineType CreateSubroutineType( DIFile file, uint flags, DITypeArray types )
        {
            var handle = NativeMethods.DIBuilderCreateSubroutineType( BuilderHandle, file?.MetadataHandle ?? LLVMMetadataRef.Zero , types.MetadataHandle, flags );
            return DINode.FromHandle< DISubroutineType >( handle );
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
            var handle = NativeMethods.DIBuilderCreateStructType( BuilderHandle
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
            return DINode.FromHandle<DICompositeType>( handle );
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
            var handle = NativeMethods.DIBuilderCreateMemberType( BuilderHandle
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
            return DINode.FromHandle< DIDerivedType >( handle );
        }

        public DICompositeType CreateArrayType( ulong bitSize, ulong bitAlign, DIType elementType, DIArray subScripts )
        {
            var handle = NativeMethods.DIBuilderCreateArrayType( BuilderHandle, bitSize, bitAlign, elementType.MetadataHandle, subScripts.MetadataHandle );
            return DINode.FromHandle<DICompositeType>( handle );
        }

        public DICompositeType CreateArrayType( ulong bitSize, ulong bitAlign, DIType elementType, params DINode[] subScripts )
        {
            return CreateArrayType( bitSize, bitAlign, elementType, GetOrCreateArray( subScripts ) );
        }

        public DIDerivedType CreateTypedef( DIType type, string name, DIFile file, uint line, DINode context )
        {
            var handle = NativeMethods.DIBuilderCreateTypedef( BuilderHandle
                                                             , type?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                             , name
                                                             , file?.MetadataHandle  ?? LLVMMetadataRef.Zero
                                                             , line
                                                             , context?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                             );
            return DINode.FromHandle<DIDerivedType>( handle );
        }

        public DISubrange CreateSubrange( long lo, long count )
        {
            var handle = NativeMethods.DIBuilderGetOrCreateSubrange( BuilderHandle, lo, count );
            return DINode.FromHandle<DISubrange>( handle );
        }

        public DIArray GetOrCreateArray( IEnumerable<DINode> elements )
        {
            var buf = elements.Select( d => d?.MetadataHandle ?? LLVMMetadataRef.Zero ).ToArray( );
            var actualLen = buf.LongLength;
            // for the out parameter trick to work - need to have a valid array with at least one element
            if( buf.LongLength == 0 )
                buf = new LLVMMetadataRef[ 1 ];

            var handle = NativeMethods.DIBuilderGetOrCreateArray( BuilderHandle, out buf[ 0 ], ( ulong )actualLen );
            return new DIArray( handle );
        }

        public DITypeArray GetOrCreateTypeArray( params DIType[ ] types ) => GetOrCreateTypeArray( ( IEnumerable<DIType> )types );
        public DITypeArray GetOrCreateTypeArray( IEnumerable<DIType> types )
        {
            var buf = types.Select( t => t?.MetadataHandle ?? LLVMMetadataRef.Zero ).ToArray();
            var handle = NativeMethods.DIBuilderGetOrCreateTypeArray( BuilderHandle, out buf[ 0 ], (ulong)buf.LongLength );
            return new DITypeArray( handle );
        }

        public DIEnumerator CreateEnumeratorValue( string name, long value )
        {
            var handle = NativeMethods.DIBuilderCreateEnumeratorValue( BuilderHandle, name, value );
            return DINode.FromHandle<DIEnumerator>( handle );
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
            var elementArray = NativeMethods.DIBuilderGetOrCreateArray( BuilderHandle, out elementHandles[ 0 ], (ulong)elementHandles.LongLength );
            var handle = NativeMethods.DIBuilderCreateEnumerationType( BuilderHandle
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
            return DINode.FromHandle< DICompositeType >( handle );
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
            var handle = NativeMethods.DIBuilderCreateGlobalVariable( BuilderHandle
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
            return DINode.FromHandle< DIGlobalVariable >( handle );
        }

        public void Finish()
        {
            if( !IsFinished )
            {
                NativeMethods.DIBuilderFinalize( BuilderHandle );
                IsFinished = true;
            }
        }

        public Value InsertDeclare( Value storage, DILocalVariable varInfo, DILocation location, Instruction insertBefore )
        {
            return InsertDeclare( storage, varInfo, CreateExpression( ), location, insertBefore );
        }

        public Value InsertDeclare( Value storage, DILocalVariable varInfo, DIExpression expr, DILocation location, Instruction insertBefore )
        {
            var handle = NativeMethods.DIBuilderInsertDeclareBefore( BuilderHandle
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
            var handle = NativeMethods.DIBuilderInsertDeclareAtEnd( BuilderHandle
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

            var handle = NativeMethods.DIBuilderCreateExpression( BuilderHandle, out args[ 0 ], (ulong)actualCount );
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
            var handle = NativeMethods.DIBuilderCreateReplaceableCompositeType( BuilderHandle
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
            return DINode.FromHandle< DICompositeType >( handle );
        }

        public void Dispose( )
        {
            if( BuilderHandle.Pointer != IntPtr.Zero )
            {
                NativeMethods.DIBuilderDestroy( BuilderHandle );
                BuilderHandle = default(LLVMDIBuilderRef);
            }
        }

        internal DebugInfoBuilder( Module owningModule )
            : this( owningModule, true )
        {
        }

        // keeping this private for now as there doesn't seem to be a good reason to support
        // allowUnresolved == false
        private DebugInfoBuilder( Module owningModule, bool allowUnresolved )
        {
            if( owningModule == null )
                throw new ArgumentNullException( nameof( owningModule ) );

            BuilderHandle = NativeMethods.NewDIBuilder( owningModule.ModuleHandle, allowUnresolved );
            OwningModule = owningModule;
        }

        private DILocalVariable CreateLocalVariable( Tag dwarfTag
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
            var handle = NativeMethods.DIBuilderCreateLocalVariable( BuilderHandle
                                                                   , (uint)dwarfTag
                                                                   , scope.MetadataHandle
                                                                   , name
                                                                   , file?.MetadataHandle ?? LLVMMetadataRef.Zero
                                                                   , line
                                                                   , type.MetadataHandle
                                                                   , alwaysPreserve ? 1 : 0
                                                                   , flags
                                                                   , argNo
                                                                   );
            return DINode.FromHandle< DILocalVariable >( handle );
        }

        private readonly Module OwningModule;
        private bool IsFinished;
        internal LLVMDIBuilderRef BuilderHandle { get; private set; }
    }
}

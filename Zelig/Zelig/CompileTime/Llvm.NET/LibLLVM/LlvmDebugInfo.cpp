#include <type_traits>
#include "LlvmDebugInfo.h"
#include <llvm/IR/Module.h>
#include <llvm/IR/DIBuilder.h>

using namespace llvm;

// Create wrappers for C Binding types (see CBindingWrapping.h).
DEFINE_SIMPLE_CONVERSION_FUNCTIONS( DIBuilder, LLVMDIBuilderRef )

#define LLVM_DEFINE_DINODE_CAST(name)                                                     \
LLVMDIDescriptorRef LLVMIsA##name( LLVMDIDescriptorRef Val )                              \
{                                                                                         \
    return wrap( static_cast<DIDescriptor*>( dyn_cast_or_null<name>( unwrap( Val ) ) ) ); \
}

LLVM_FOR_EACH_DIDESCRIPTOR_SUBCLASS( LLVM_DEFINE_DINODE_CAST )

extern "C" LLVMDIBuilderRef LLVMDIBuilderCreate( LLVMModuleRef M )
{
    return wrap( new DIBuilder( *unwrap( M ) ) );
}

extern "C" void LLVMDIBuilderDispose( LLVMDIBuilderRef Builder )
{
    delete unwrap( Builder );
}

extern "C" void LLVMDIBuilderFinalize( LLVMDIBuilderRef Builder )
{
    auto bldr = unwrap( Builder );
    bldr->finalize( );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateCompileUnit( LLVMDIBuilderRef Builder
                                                      , unsigned Lang
                                                      , const char* File
                                                      , const char* Dir
                                                      , const char* Producer
                                                      , bool isOptimized
                                                      , const char* Flags
                                                      , unsigned RuntimeVer
                                                      , const char* SplitName
                                                      )
{
    auto bldr = unwrap( Builder );
    auto unwrappedResult = bldr->createCompileUnit( Lang
                                                  , File
                                                  , Dir
                                                  , Producer
                                                  , isOptimized
                                                  , Flags
                                                  , RuntimeVer
                                                  , SplitName
                                                  );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateFile( LLVMDIBuilderRef Builder
                                               , const char* Filename
                                               , const char* Directory
                                               )
{
    auto bldr = unwrap( Builder );
    auto result = bldr->createFile( Filename, Directory );
    return wrap( result );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateSubroutineType( LLVMDIBuilderRef Builder
                                                         , LLVMValueRef File
                                                         , LLVMValueRef ParameterTypes
                                                         )
{
    auto bldr = unwrap( Builder );
    auto unwrappedVal = bldr->createSubroutineType( unwrapDI<DIFile>( File )
                                                  , unwrapDI<DIArray>( ParameterTypes )
                                                  );
    return wrap( unwrappedVal );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateFunction( LLVMDIBuilderRef Builder
                                                   , LLVMValueRef Scope
                                                   , const char* Name
                                                   , const char* LinkageName
                                                   , LLVMValueRef File
                                                   , unsigned LineNo
                                                   , LLVMValueRef Ty
                                                   , bool isLocalToUnit
                                                   , bool isDefinition
                                                   , unsigned ScopeLine
                                                   , unsigned Flags
                                                   , bool isOptimized
                                                   , LLVMValueRef Fn
                                                   , LLVMValueRef TParam
                                                   , LLVMValueRef Decl
                                                   )
{
    auto bldr = unwrap( Builder );
    auto unwrapped = bldr->createFunction( unwrapDI<DIScope>( Scope )
                                         , Name
                                         , LinkageName
                                         , unwrapDI<DIFile>( File )
                                         , LineNo
                                         , unwrapDI<DICompositeType>( Ty )
                                         , isLocalToUnit
                                         , isDefinition
                                         , ScopeLine
                                         , Flags
                                         , isOptimized
                                         , unwrap<Function>( Fn )
                                         , unwrapDI<MDNode*>( TParam )
                                         , unwrapDI<MDNode*>( Decl )
                                         );
    return wrap( unwrapped );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateBasicType( LLVMDIBuilderRef Builder
                                                    , const char* Name
                                                    , uint64_t SizeInBits
                                                    , uint64_t AlignInBits
                                                    , unsigned Encoding
                                                    )
{
    auto bldr = unwrap( Builder );
    auto unwrapped = bldr->createBasicType( Name, SizeInBits, AlignInBits, Encoding );
    return wrap( unwrapped );
}

extern "C" LLVMValueRef LLVMDIBuilderCreatePointerType( LLVMDIBuilderRef Builder
                                                      , LLVMValueRef PointeeTy
                                                      , uint64_t SizeInBits
                                                      , uint64_t AlignInBits
                                                      , const char* Name
                                                      )
{
    auto bldr = unwrap( Builder );
    auto unwrapped = bldr->createPointerType( unwrapDI<DIType>( PointeeTy ), SizeInBits, AlignInBits, Name );
    return wrap( unwrapped );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateForwardDecl( LLVMDIBuilderRef Builder
                                                      , unsigned Tag
                                                      , const char* Name
                                                      , LLVMValueRef Scope
                                                      , LLVMValueRef File
                                                      , unsigned Line
                                                      , unsigned RuntimeLang
                                                      , uint64_t SizeInBits
                                                      , uint64_t AlignInBits
                                                      , const char* UniqueId
                                                      )
{
    auto bldr = unwrap( Builder );
    auto unwrapped = bldr->createForwardDecl( Tag
                                            , Name
                                            , unwrapDI<DIDescriptor>( Scope )
                                            , unwrapDI<DIFile>( File )
                                            , Line
                                            , RuntimeLang
                                            , SizeInBits
                                            , AlignInBits
                                            , UniqueId
                                            );
    return wrap( unwrapped );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateStructType( LLVMDIBuilderRef Builder
                                                     , LLVMValueRef Scope
                                                     , const char* Name
                                                     , LLVMValueRef File
                                                     , unsigned LineNumber
                                                     , uint64_t SizeInBits
                                                     , uint64_t AlignInBits
                                                     , unsigned Flags
                                                     , LLVMValueRef DerivedFrom
                                                     , LLVMValueRef Elements
                                                     , unsigned RunTimeLang
                                                     , LLVMValueRef VTableHolder
                                                     , const char *UniqueId
                                                     )
{
    auto bldr = unwrap( Builder );
    auto unwrapped = bldr->createStructType( unwrapDI<DIDescriptor>( Scope )
                                           , Name
                                           , unwrapDI<DIFile>( File )
                                           , LineNumber
                                           , SizeInBits
                                           , AlignInBits
                                           , Flags
                                           , unwrapDI<DIType>( DerivedFrom )
                                           , unwrapDI<DIArray>( Elements )
                                           , RunTimeLang
                                           , unwrapDI<DIType>( VTableHolder )
                                           , UniqueId
                                           );    
    return wrap( unwrapped );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateClassType( LLVMDIBuilderRef Builder
                                                    , LLVMValueRef Scope
                                                    , const char* Name
                                                    , LLVMValueRef File
                                                    , unsigned LineNumber
                                                    , uint64_t SizeInBits
                                                    , uint64_t AlignInBits
                                                    , uint64_t OffsetInBits
                                                    , unsigned Flags
                                                    , LLVMValueRef DerivedFrom
                                                    , LLVMValueRef Elements
                                                    , LLVMValueRef VTableHolder
                                                    , LLVMValueRef TemplateParms
                                                    , const char *UniqueId
                                                    )
{
    auto bldr = unwrap( Builder );
    auto unwrapped = bldr->createClassType( unwrapDI<DIDescriptor>( Scope )
                                          , Name
                                          , unwrapDI<DIFile>( File )
                                          , LineNumber
                                          , SizeInBits
                                          , AlignInBits
                                          , OffsetInBits
                                          , Flags
                                          , unwrapDI<DIType>( DerivedFrom )
                                          , unwrapDI<DIArray>( Elements )
                                          , unwrapDI<DIType>( VTableHolder )
                                          , unwrapDI<MDNode*>( TemplateParms )
                                          , UniqueId
                                          );
    return wrap( unwrapped );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateMemberType( LLVMDIBuilderRef Builder
                                                     , LLVMValueRef Scope
                                                     , const char* Name
                                                     , LLVMValueRef File
                                                     , unsigned LineNo
                                                     , uint64_t SizeInBits
                                                     , uint64_t AlignInBits
                                                     , uint64_t OffsetInBits
                                                     , unsigned Flags
                                                     , LLVMValueRef Ty
                                                     )
{
    auto bldr = unwrap( Builder );
    auto unwrappedResult = bldr->createMemberType( unwrapDI<DIDescriptor>( Scope )
                                                 , Name
                                                 , unwrapDI<DIFile>( File )
                                                 , LineNo
                                                 , SizeInBits
                                                 , AlignInBits
                                                 , OffsetInBits
                                                 , Flags
                                                 , unwrapDI<DIType>( Ty )
                                                 );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateLexicalBlock( LLVMDIBuilderRef Builder
                                                       , LLVMValueRef Scope
                                                       , LLVMValueRef File
                                                       , unsigned Line
                                                       , unsigned Col
                                                       , unsigned Discriminator
                                                       )
{
    auto bldr = unwrap( Builder );
    auto unwrappedResult = bldr->createLexicalBlock( unwrapDI<DIDescriptor>( Scope )
                                                   , unwrapDI<DIFile>( File )
                                                   , Line
                                                   , Col
                                                   , Discriminator
                                                   );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateStaticVariable( LLVMDIBuilderRef Builder
                                                         , LLVMValueRef Context
                                                         , const char* Name
                                                         , const char* LinkageName
                                                         , LLVMValueRef File
                                                         , unsigned LineNo
                                                         , LLVMValueRef Ty
                                                         , bool isLocalToUnit
                                                         , LLVMValueRef Val
                                                         , LLVMValueRef Decl
                                                         )
{
    auto bldr = unwrap( Builder );
    auto unwrappedResult = bldr->createStaticVariable( unwrapDI<DIDescriptor>( Context )
                                                     , Name
                                                     , LinkageName
                                                     , unwrapDI<DIFile>( File )
                                                     , LineNo
                                                     , unwrapDI<DIType>( Ty )
                                                     , isLocalToUnit
                                                     , unwrap( Val )
                                                     , unwrapDI<MDNode*>( Decl )
                                                     );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateLocalVariable( LLVMDIBuilderRef Builder
                                                        , unsigned Tag
                                                        , LLVMValueRef Scope
                                                        , const char* Name
                                                        , LLVMValueRef File
                                                        , unsigned LineNo
                                                        , LLVMValueRef Ty
                                                        , bool AlwaysPreserve
                                                        , unsigned Flags
                                                        , unsigned ArgNo
                                                        )
{
    auto bldr = unwrap( Builder );
    auto unwrappedResult = bldr->createLocalVariable( Tag
                                                    , unwrapDI<DIDescriptor>( Scope )
                                                    , Name
                                                    , unwrapDI<DIFile>( File )
                                                    , LineNo
                                                    , unwrapDI<DIType>( Ty )
                                                    , AlwaysPreserve
                                                    , Flags
                                                    , ArgNo
                                                    );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateArrayType( LLVMDIBuilderRef Builder
                                                    , uint64_t Size
                                                    , uint64_t AlignInBits
                                                    , LLVMValueRef Ty
                                                    , LLVMValueRef Subscripts
                                                    )
{
    auto bldr = unwrap( Builder );
    auto unrwappedResult = bldr->createArrayType( Size
                                                , AlignInBits
                                                , unwrapDI<DIType>( Ty )
                                                , unwrapDI<DIArray>( Subscripts )
                                                );
   return wrap( unrwappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateVectorType( LLVMDIBuilderRef Builder
                                                     , uint64_t Size
                                                     , uint64_t AlignInBits
                                                     , LLVMValueRef Ty
                                                     , LLVMValueRef Subscripts
                                                     )
{
    auto bldr = unwrap( Builder );    
    auto unwrappedResult = bldr->createVectorType( Size
                                                 , AlignInBits
                                                 , unwrapDI<DIType>( Ty )
                                                 , unwrapDI<DIArray>( Subscripts )
                                                 );

    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderGetOrCreateSubrange( LLVMDIBuilderRef Builder
                                                        , int64_t Lo
                                                        , int64_t Count
                                                        )
{
    auto bldr = unwrap( Builder );    
    auto unwrappedResult = bldr->getOrCreateSubrange( Lo, Count );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderGetOrCreateArray( LLVMDIBuilderRef Builder
                                                     , LLVMValueRef* Ptr
                                                     , unsigned Count
                                                     )
{
    auto bldr = unwrap( Builder );    
    auto unwrappedResult = bldr->getOrCreateArray( ArrayRef<Value*>( reinterpret_cast< Value** >( Ptr ), Count ) );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderInsertDeclareAtEnd( LLVMDIBuilderRef Builder
                                                       , LLVMValueRef Val
                                                       , LLVMValueRef VarInfo
                                                       , LLVMBasicBlockRef InsertAtEnd
                                                       )
{
    auto bldr = unwrap( Builder );    
    auto unwrappedResult = bldr->insertDeclare( unwrap( Val )
                                              , unwrapDI<DIVariable>( VarInfo )
                                              , unwrap( InsertAtEnd )
                                              );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderInsertDeclareBefore( LLVMDIBuilderRef Builder
                                                        , LLVMValueRef Val
                                                        , LLVMValueRef VarInfo
                                                        , LLVMValueRef InsertBefore
                                                        )
{
    auto bldr = unwrap( Builder );
    auto unwrappedResult = bldr->insertDeclare( unwrap( Val )
                                              , unwrapDI<DIVariable>( VarInfo )
                                              , unwrap<Instruction>( InsertBefore )
                                              );
    return wrap( unwrappedResult );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateEnumerator( LLVMDIBuilderRef Builder
                                                     , const char* Name
                                                     , uint64_t Val
                                                     )
{
    return wrap( unwrap( Builder )->createEnumerator( Name, Val ) );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateEnumerationType( LLVMDIBuilderRef Builder
                                                          , LLVMValueRef Scope
                                                          , const char* Name
                                                          , LLVMValueRef File
                                                          , unsigned LineNumber
                                                          , uint64_t SizeInBits
                                                          , uint64_t AlignInBits
                                                          , LLVMValueRef Elements
                                                          , LLVMValueRef ClassType
                                                          )
{
    return wrap( unwrap( Builder )->createEnumerationType( unwrapDI<DIDescriptor>( Scope )
                                                         , Name
                                                         , unwrapDI<DIFile>( File )
                                                         , LineNumber
                                                         , SizeInBits
                                                         , AlignInBits
                                                         , unwrapDI<DIArray>( Elements )
                                                         , unwrapDI<DIType>( ClassType )
                                                         ) 
               );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateUnionType( LLVMDIBuilderRef Builder
                                                    , LLVMValueRef Scope
                                                    , const char* Name
                                                    , LLVMValueRef File
                                                    , unsigned LineNumber
                                                    , uint64_t SizeInBits
                                                    , uint64_t AlignInBits
                                                    , unsigned Flags
                                                    , LLVMValueRef Elements
                                                    , unsigned RunTimeLang
                                                    , const char* UniqueId
                                                    )
{
    return wrap( unwrap( Builder )->createUnionType( unwrapDI<DIDescriptor>( Scope )
                                                   , Name
                                                   , unwrapDI<DIFile>( File )
                                                   , LineNumber
                                                   , SizeInBits
                                                   , AlignInBits
                                                   , Flags
                                                   , unwrapDI<DIArray>( Elements )
                                                   , RunTimeLang
                                                   , UniqueId
                                                   )
               );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateTemplateTypeParameter( LLVMDIBuilderRef Builder
                                                                , LLVMValueRef Scope
                                                                , const char* Name
                                                                , LLVMValueRef Ty
                                                                , LLVMValueRef File
                                                                , unsigned LineNo
                                                                , unsigned ColumnNo
                                                                )
{
    return wrap( unwrap( Builder )->createTemplateTypeParameter( unwrapDI<DIDescriptor>( Scope )
                                                               , Name
                                                               , unwrapDI<DIType>( Ty )
                                                               , unwrapDI<MDNode*>( File )
                                                               , LineNo
                                                               , ColumnNo
                                                               )
               );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateOpDeref( LLVMTypeRef IntTy )
{
    return LLVMConstInt( IntTy, DIBuilder::OpDeref, true );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateOpPlus( LLVMTypeRef IntTy )
{
    return LLVMConstInt( IntTy, DIBuilder::OpPlus, true );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateComplexVariable( LLVMDIBuilderRef Builder
                                                          , unsigned Tag
                                                          , LLVMValueRef Scope
                                                          , const char *Name
                                                          , LLVMValueRef File
                                                          , unsigned LineNo
                                                          , LLVMValueRef Ty
                                                          , LLVMValueRef* AddrOps
                                                          , unsigned AddrOpsCount
                                                          , unsigned ArgNo
                                                          )
{
    llvm::ArrayRef<llvm::Value*> addr_ops( ( llvm::Value** )AddrOps, AddrOpsCount );

    return wrap( unwrap( Builder )->createComplexVariable( Tag
                                                         , unwrapDI<DIDescriptor>( Scope )
                                                         , Name
                                                         , unwrapDI<DIFile>( File )
                                                         , LineNo
                                                         , unwrapDI<DIType>( Ty )
                                                         , addr_ops
                                                         , ArgNo
                                                         )
               );
}

extern "C" LLVMValueRef LLVMDIBuilderCreateNameSpace( LLVMDIBuilderRef Builder
                                                    , LLVMValueRef Scope
                                                    , const char* Name
                                                    , LLVMValueRef File
                                                    , unsigned LineNo
                                                    )
{
    return wrap( unwrap( Builder )->createNameSpace( unwrapDI<DIDescriptor>( Scope )
                                                   , Name
                                                   , unwrapDI<DIFile>( File )
                                                   , LineNo
                                                   )
               );
}

extern "C" void LLVMDICompositeTypeSetTypeArray( LLVMValueRef* CompositeType
                                               , LLVMValueRef TypeArray
                                               )
{
    auto compositeType = unwrapDI<DICompositeType>( *CompositeType );
    compositeType.setTypeArray( unwrapDI<DIArray>( TypeArray ) );
    *CompositeType = wrap( compositeType );
}


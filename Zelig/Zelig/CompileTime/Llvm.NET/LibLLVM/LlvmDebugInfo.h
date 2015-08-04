#pragma once

#include "llvm-c/Core.h"

#ifdef __cplusplus
extern "C" {
#endif

#define DECLARE_LLVMC_REF( n ) typedef struct LLVMOpaque##n##* LLVM##n##Ref

    DECLARE_LLVMC_REF( DIBuilder );
    DECLARE_LLVMC_REF( DIBasicType );
    DECLARE_LLVMC_REF( DICompileUnit );
    DECLARE_LLVMC_REF( DICompositeType );
    DECLARE_LLVMC_REF( DIDerivedType );
    DECLARE_LLVMC_REF( DIDescriptor );
    DECLARE_LLVMC_REF( DIFile );
    DECLARE_LLVMC_REF( DIEnumerator );
    DECLARE_LLVMC_REF( DIType );
    DECLARE_LLVMC_REF( DIArray );
    DECLARE_LLVMC_REF( DIGlobalVariable );
    DECLARE_LLVMC_REF( DIImportedEntity );
    DECLARE_LLVMC_REF( DINameSpace );
    DECLARE_LLVMC_REF( DIVariable );
    DECLARE_LLVMC_REF( DISubrange );
    DECLARE_LLVMC_REF( DILexicalBlockFile );
    DECLARE_LLVMC_REF( DILexicalBlock );
    DECLARE_LLVMC_REF( DIScope );
    DECLARE_LLVMC_REF( DISubprogram );
    DECLARE_LLVMC_REF( DITemplateTypeParameter );
    DECLARE_LLVMC_REF( DITemplateValueParameter );
    DECLARE_LLVMC_REF( DIObjCProperty );

#define LLVM_FOR_EACH_DIDESCRIPTOR_SUBCLASS(macro) \
    macro(DIDescriptor)                            \
        macro(DILocation)                          \
        macro(DIObjCProperty)                      \
        macro(DITemplateValueParameter)            \
        macro(DITemplateTypeParameter)             \
        macro(DIEnumerator)                        \
        macro(DIGlobalVariable)                    \
        macro(DIImportedEntity)                    \
        macro(DISubrange)                          \
        macro(DIVariable)                          \
        macro(DIScope)                             \
            macro(DISubprogram)                    \
            macro(DICompileUnit)                   \
            macro(DIFile)                          \
            macro(DINameSpace)                     \
            macro(DILexicalBlock)                  \
            macro(DILexicalBlockFile)              \
            macro(DIType)                          \
                macro(DIBasicType)                 \
                macro(DIDerivedType)               \
                    macro(DICompositeType)         

#define LLVM_DECLARE_DINODE_CAST(name) LLVMDIDescriptorRef LLVMIsA##name( LLVMDIDescriptorRef node );

    LLVM_FOR_EACH_DIDESCRIPTOR_SUBCLASS( LLVM_DECLARE_DINODE_CAST );

    LLVMDIBuilderRef LLVMDIBuilderCreate( LLVMModuleRef M );

    void LLVMDIBuilderDispose( LLVMDIBuilderRef Builder );

    void LLVMDIBuilderFinalize( LLVMDIBuilderRef Builder );

    LLVMValueRef LLVMDIBuilderCreateCompileUnit( LLVMDIBuilderRef Builder
                                                 , unsigned Lang
                                                 , const char* File
                                                 , const char* Dir
                                                 , const char* Producer
                                                 , bool isOptimized
                                                 , const char* Flags
                                                 , unsigned RuntimeVer
                                                 , const char* SplitName
                                                 );

    LLVMValueRef LLVMDIBuilderCreateFile( LLVMDIBuilderRef Builder, const char* Filename, const char* Directory );

    LLVMValueRef LLVMDIBuilderCreateSubroutineType( LLVMDIBuilderRef Builder
                                                    , LLVMValueRef File
                                                    , LLVMValueRef ParameterTypes
                                                    );

    LLVMValueRef LLVMDIBuilderCreateFunction( LLVMDIBuilderRef Builder
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
                                              );

    LLVMValueRef LLVMDIBuilderCreateBasicType( LLVMDIBuilderRef Builder
                                               , const char* Name
                                               , uint64_t SizeInBits
                                               , uint64_t AlignInBits
                                               , unsigned Encoding
                                               );

    LLVMValueRef LLVMDIBuilderCreatePointerType( LLVMDIBuilderRef Builder
                                                 , LLVMValueRef PointeeTy
                                                 , uint64_t SizeInBits
                                                 , uint64_t AlignInBits
                                                 , const char* Name
                                                 );

    LLVMValueRef LLVMDIBuilderCreateForwardDecl( LLVMDIBuilderRef Builder
                                                 , unsigned Tag
                                                 , const char* Name
                                                 , LLVMValueRef Scope
                                                 , LLVMValueRef File
                                                 , unsigned Line
                                                 , unsigned RuntimeLang
                                                 , uint64_t SizeInBits
                                                 , uint64_t AlignInBits
                                                 , const char* UniqueId
                                                 );

    LLVMValueRef LLVMDIBuilderCreateStructType( LLVMDIBuilderRef Builder
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
                                                );

    LLVMValueRef LLVMDIBuilderCreateClassType( LLVMDIBuilderRef Builder
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
                                               );

    LLVMValueRef LLVMDIBuilderCreateMemberType( LLVMDIBuilderRef Builder
                                                , LLVMValueRef Scope
                                                , const char* Name
                                                , LLVMValueRef File
                                                , unsigned LineNo
                                                , uint64_t SizeInBits
                                                , uint64_t AlignInBits
                                                , uint64_t OffsetInBits
                                                , unsigned Flags
                                                , LLVMValueRef Ty
                                                );

    LLVMValueRef LLVMDIBuilderCreateLexicalBlock( LLVMDIBuilderRef Builder
                                                  , LLVMValueRef Scope
                                                  , LLVMValueRef File
                                                  , unsigned Line
                                                  , unsigned Col
                                                  , unsigned Discriminator
                                                  );

    LLVMValueRef LLVMDIBuilderCreateStaticVariable( LLVMDIBuilderRef Builder
                                                    , LLVMValueRef Context
                                                    , const char* Name
                                                    , const char* LinkageName
                                                    , LLVMValueRef File
                                                    , unsigned LineNo
                                                    , LLVMValueRef Ty
                                                    , bool isLocalToUnit
                                                    , LLVMValueRef Val
                                                    , LLVMValueRef Decl
                                                    );

    LLVMValueRef LLVMDIBuilderCreateLocalVariable( LLVMDIBuilderRef Builder
                                                   , unsigned Tag
                                                   , LLVMValueRef Scope
                                                   , const char* Name
                                                   , LLVMValueRef File
                                                   , unsigned LineNo
                                                   , LLVMValueRef Ty
                                                   , bool AlwaysPreserve
                                                   , unsigned Flags
                                                   , unsigned ArgNo
                                                   );

    LLVMValueRef LLVMDIBuilderCreateArrayType( LLVMDIBuilderRef Builder
                                               , uint64_t Size
                                               , uint64_t AlignInBits
                                               , LLVMValueRef Ty
                                               , LLVMValueRef Subscripts
                                               );

    LLVMValueRef LLVMDIBuilderCreateVectorType( LLVMDIBuilderRef Builder
                                                , uint64_t Size
                                                , uint64_t AlignInBits
                                                , LLVMValueRef Ty
                                                , LLVMValueRef Subscripts
                                                );

    LLVMValueRef LLVMDIBuilderGetOrCreateSubrange( LLVMDIBuilderRef Builder
                                                   , int64_t Lo
                                                   , int64_t Count
                                                   );
    LLVMValueRef LLVMDIBuilderGetOrCreateArray( LLVMDIBuilderRef Builder
                                                , LLVMValueRef* Ptr
                                                , unsigned Count
                                                );

    LLVMValueRef LLVMDIBuilderInsertDeclareAtEnd( LLVMDIBuilderRef Builder
                                                  , LLVMValueRef Val
                                                  , LLVMValueRef VarInfo
                                                  , LLVMBasicBlockRef InsertAtEnd
                                                  );

    LLVMValueRef LLVMDIBuilderInsertDeclareBefore( LLVMDIBuilderRef Builder
                                                   , LLVMValueRef Val
                                                   , LLVMValueRef VarInfo
                                                   , LLVMValueRef InsertBefore
                                                   );

    LLVMValueRef LLVMDIBuilderCreateEnumerator( LLVMDIBuilderRef Builder
                                                , const char* Name
                                                , uint64_t Val
                                                );

    LLVMValueRef LLVMDIBuilderCreateEnumerationType( LLVMDIBuilderRef Builder
                                                     , LLVMValueRef Scope
                                                     , const char* Name
                                                     , LLVMValueRef File
                                                     , unsigned LineNumber
                                                     , uint64_t SizeInBits
                                                     , uint64_t AlignInBits
                                                     , LLVMValueRef Elements
                                                     , LLVMValueRef ClassType
                                                     );

    LLVMValueRef LLVMDIBuilderCreateUnionType( LLVMDIBuilderRef Builder
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
                                               );

    LLVMValueRef LLVMDIBuilderCreateTemplateTypeParameter( LLVMDIBuilderRef Builder
                                                           , LLVMValueRef Scope
                                                           , const char* Name
                                                           , LLVMValueRef Ty
                                                           , LLVMValueRef File
                                                           , unsigned LineNo
                                                           , unsigned ColumnNo
                                                           );

    LLVMValueRef LLVMDIBuilderCreateOpDeref( LLVMTypeRef IntTy );

    LLVMValueRef LLVMDIBuilderCreateOpPlus( LLVMTypeRef IntTy );

    LLVMValueRef LLVMDIBuilderCreateComplexVariable( LLVMDIBuilderRef Builder
                                                     , unsigned Tag
                                                     , LLVMValueRef Scope
                                                     , const char *Name
                                                     , LLVMValueRef File
                                                     , unsigned LineNo
                                                     , LLVMValueRef Ty
                                                     , LLVMValueRef* AddrOps
                                                     , unsigned AddrOpsCount
                                                     , unsigned ArgNo
                                                     );

    LLVMValueRef LLVMDIBuilderCreateNameSpace( LLVMDIBuilderRef Builder
                                               , LLVMValueRef Scope
                                               , const char* Name
                                               , LLVMValueRef File
                                               , unsigned LineNo
                                               );

    void LLVMDICompositeTypeSetTypeArray( LLVMValueRef* CompositeType
                                          , LLVMValueRef TypeArray
                                          );

    void LLVMAddModuleFlag( LLVMModuleRef M, const char *name, uint32_t value );

#ifdef __cplusplus
}
#endif

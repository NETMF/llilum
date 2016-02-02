//===- DIBuilderBindings.h - Bindings for DIBuilder -------------*- C++ -*-===//
//
//                     The LLVM Compiler Infrastructure
//
// This file is distributed under the University of Illinois Open Source
// License. See LICENSE.TXT for details.
//
//===----------------------------------------------------------------------===//
//
// This file defines C bindings for the DIBuilder class.
//
//===----------------------------------------------------------------------===//

// This file was adapted from the GO language bindings provided in standard LLVM Distribution 

#ifndef LLVM_BINDINGS_LLVM_DIBUILDERBINDINGS_H
#define LLVM_BINDINGS_LLVM_DIBUILDERBINDINGS_H

#include <stdint.h>
#include "IRBindings.h"
#include "llvm-c/Core.h"

#ifdef __cplusplus
extern "C" {
#endif

    enum LLVMDwarfTag
    {
        LLVMDwarfTagArrayType = 0x01,
        LLVMDwarfTagCLassType = 0x02,
        LLVMDwarfTagEntryPoint = 0x03,
        LLVMDwarfTagEnumerationType = 0x04,
        LLVMDwarfTagFormalParameter = 0x05,
        LLVMDwarfTagImportedDeclaration = 0x08,
        LLVMDwarfTagLabel = 0x0a,
        LLVMDwarfTagLexicalBlock = 0x0b,
        LLVMDwarfTagMember = 0x0d,
        LLVMDwarfTagPointerType = 0x0f,
        LLVMDwarfTagReferenceType = 0x10,
        LLVMDwarfTagCompileUnit = 0x11,
        LLVMDwarfTagStringType = 0x12,
        LLVMDwarfTagStructureType = 0x13,
        LLVMDwarfTagSubroutineType = 0x15,
        LLVMDwarfTagTypeDef = 0x16,
        LLVMDwarfTagUnionType = 0x17,
        LLVMDwarfTagUnspecifiedParameters = 0x18,
        LLVMDwarfTagVariant = 0x19,
        LLVMDwarfTagCommonBlock = 0x1a,
        LLVMDwarfTagCommonInclusion = 0x1b,
        LLVMDwarfTagInheritance = 0x1c,
        LLVMDwarfTagInlinedSubroutine = 0x1d,
        LLVMDwarfTagModule = 0x1e,
        LLVMDwarfTagPtrToMemberType = 0x1f,
        LLVMDwarfTagSetType = 0x20,
        LLVMDwarfTagSubrangeType = 0x21,
        LLVMDwarfTagWithStatement = 0x22,
        LLVMDwarfTagAccessDeclaration = 0x23,
        LLVMDwarfTagBaseType = 0x24,
        LLVMDwarfTagCatchBlock = 0x25,
        LLVMDwarfTagConstType = 0x26,
        LLVMDwarfTagConstant = 0x27,
        LLVMDwarfTagEnumerator = 0x28,
        LLVMDwarfTagFileType = 0x29,
        LLVMDwarfTagFriend = 0x2a,
        LLVMDwarfTagNameList = 0x2b,
        LLVMDwarfTagNameListItem = 0x2c,
        LLVMDwarfTagPackedType = 0x2d,
        LLVMDwarfTagSubProgram = 0x2e,
        LLVMDwarfTagTemplateTypeParameter = 0x2f,
        LLVMDwarfTagTemplateValueParameter = 0x30,
        LLVMDwarfTagThrownType = 0x31,
        LLVMDwarfTagTryBlock = 0x32,
        LLVMDwarfTagVariantPart = 0x33,
        LLVMDwarfTagVariable = 0x34,
        LLVMDwarfTagVolatileType = 0x35,
        LLVMDwarfTagDwarfProcedure = 0x36,
        LLVMDwarfTagRestrictType = 0x37,
        LLVMDwarfTagInterfaceType = 0x38,
        LLVMDwarfTagNamespace = 0x39,
        LLVMDwarfTagImportedModule = 0x3a,
        LLVMDwarfTagUnspecifiedType = 0x3b,
        LLVMDwarfTagPartialUnit = 0x3c,
        LLVMDwarfTagImportedUnit = 0x3d,
        LLVMDwarfTagCondition = 0x3f,
        LLVMDwarfTagSharedType = 0x40,
        LLVMDwarfTagTypeUnit = 0x41,
        LLVMDwarfTagRValueReferenceType = 0x42,
        LLVMDwarfTagTemplateAlias = 0x43,

        // New in DWARF 5:
        LLVMDwarfTagCoArrayType = 0x44,
        LLVMDwarfTagGenericSubrange = 0x45,
        LLVMDwarfTagDynamicType = 0x46,

        LLVMDwarfTagMipsLoop = 0x4081,
        LLVMDwarfTagFormatLabel = 0x4101,
        LLVMDwarfTagFunctionTemplate = 0x4102,
        LLVMDwarfTagClassTemplate = 0x4103,
        LLVMDwarfTagGnuTemplateTemplateParam = 0x4106,
        LLVMDwarfTagGnuTemplateParameterPack = 0x4107,
        LLVMDwarfTagGnuFormalParameterPack = 0x4108,
        LLVMDwarfTagLoUser = 0x4080,
        LLVMDwarfTagAppleProperty = 0x4200,
        LLVMDwarfTagHiUser = 0xffff
    };

    // This should match the MetaDataKind enum in the C++ headers
    enum LLVMMetadataKind
    {
        LLVMMetadaKindMDTuple,
        LLVMMetadaKindDILocation,
        LLVMMetadaKindGenericDINode,
        LLVMMetadaKindDISubrange,
        LLVMMetadaKindDIEnumerator,
        LLVMMetadaKindDIBasicType,
        LLVMMetadaKindDIDerivedType,
        LLVMMetadaKindDICompositeType,
        LLVMMetadaKindDISubroutineType,
        LLVMMetadaKindDIFile,
        LLVMMetadaKindDICompileUnit,
        LLVMMetadaKindDISubprogram,
        LLVMMetadaKindDILexicalBlock,
        LLVMMetadaKindDILexicalBlockFile,
        LLVMMetadaKindDINamespace,
        LLVMMetadaKindDIModule,
        LLVMMetadaKindDITemplateTypeParameter,
        LLVMMetadaKindDITemplateValueParameter,
        LLVMMetadaKindDIGlobalVariable,
        LLVMMetadaKindDILocalVariable,
        LLVMMetadaKindDIExpression,
        LLVMMetadaKindDIObjCProperty,
        LLVMMetadaKindDIImportedEntity,
        LLVMMetadaKindConstantAsMetadata,
        LLVMMetadaKindLocalAsMetadata,
        LLVMMetadaKindMDString
    };

    enum LLVMMetadataFormat
    {
        LLVMMetadataFormatDefault,
        LLVMMetadataFormatAsOperand,
    };

    typedef struct LLVMOpaqueDIBuilder *LLVMDIBuilderRef;

    void LLVMSetDILocation( LLVMValueRef inst, LLVMMetadataRef location );
    void LLVMSetDebugLoc( LLVMValueRef inst, unsigned line, unsigned column, LLVMMetadataRef scope );

    LLVMMetadataRef /*DILocalScope*/ LLVMGetDILocationScope( LLVMMetadataRef /*DILocation*/ location );
    unsigned LLVMGetDILocationLine( LLVMMetadataRef /*DILocation*/ location );
    unsigned LLVMGetDILocationColumn( LLVMMetadataRef /*DILocation*/ location );
    LLVMMetadataRef /*DILocation*/ LLVMGetDILocationInlinedAt( LLVMMetadataRef /*DILocation*/ location );
    LLVMMetadataRef /*DILocalScope*/ LLVMDILocationGetInlinedAtScope( LLVMMetadataRef /*DILocation*/ location );

    LLVMDIBuilderRef LLVMNewDIBuilder( LLVMModuleRef m, LLVMBool allowUnresolved );

    void LLVMDIBuilderDestroy( LLVMDIBuilderRef d );
    void LLVMDIBuilderFinalize( LLVMDIBuilderRef d );

    LLVMMetadataRef LLVMDIBuilderCreateCompileUnit( LLVMDIBuilderRef D
                                                    , unsigned Language
                                                    , const char *File
                                                    , const char *Dir
                                                    , const char *Producer
                                                    , int Optimized
                                                    , const char *Flags
                                                    , unsigned RuntimeVersion
                                                    );

    LLVMMetadataRef LLVMDIBuilderCreateFile( LLVMDIBuilderRef D
                                             , const char *File
                                             , const char *Dir
                                             );

    LLVMMetadataRef LLVMDIBuilderCreateLexicalBlock( LLVMDIBuilderRef D
                                                     , LLVMMetadataRef Scope
                                                     , LLVMMetadataRef File
                                                     , unsigned Line
                                                     , unsigned Column
                                                     );

    LLVMMetadataRef LLVMDIBuilderCreateLexicalBlockFile( LLVMDIBuilderRef D
                                                         , LLVMMetadataRef Scope
                                                         , LLVMMetadataRef File
                                                         , unsigned Discriminator
                                                         );

    LLVMMetadataRef LLVMDIBuilderCreateFunction( LLVMDIBuilderRef D
                                                 , LLVMMetadataRef Scope
                                                 , const char *Name
                                                 , const char *LinkageName
                                                 , LLVMMetadataRef File
                                                 , unsigned Line
                                                 , LLVMMetadataRef CompositeType
                                                 , int IsLocalToUnit
                                                 , int IsDefinition
                                                 , unsigned ScopeLine
                                                 , unsigned Flags
                                                 , int IsOptimized
                                                 , LLVMMetadataRef /*DITemplateParameterArray*/ TParams /*= nullptr*/
                                                 , LLVMMetadataRef /*DISubProgram */ Decl /*= nullptr*/
                                                 );

    LLVMMetadataRef LLVMDIBuilderCreateTempFunctionFwdDecl( LLVMDIBuilderRef D
                                                            , LLVMMetadataRef /*DIScope* */Scope
                                                            , char const* Name
                                                            , char const* LinkageName
                                                            , LLVMMetadataRef /*DIFile* */ File
                                                            , unsigned LineNo
                                                            , LLVMMetadataRef /*DISubroutineType* */ Ty
                                                            , bool isLocalToUnit
                                                            , bool isDefinition
                                                            , unsigned ScopeLine
                                                            , unsigned Flags /*= 0*/
                                                            , bool isOptimized /*= false*/
                                                            , LLVMMetadataRef /*DITemplateParameterArray*/ TParams /*= nullptr*/
                                                            , LLVMMetadataRef /*DISubProgram */ Decl /*= nullptr*/
                                                            );

    LLVMMetadataRef LLVMDIBuilderCreateAutoVariable( LLVMDIBuilderRef D
                                                     , LLVMMetadataRef Scope
                                                     , const char *Name
                                                     , LLVMMetadataRef File
                                                     , unsigned Line
                                                     , LLVMMetadataRef Ty
                                                     , int AlwaysPreserve
                                                     , unsigned Flags
                                                     );

    LLVMMetadataRef LLVMDIBuilderCreateParameterVariable( LLVMDIBuilderRef D
                                                          , LLVMMetadataRef Scope
                                                          , const char *Name
                                                          , unsigned ArgNo
                                                          , LLVMMetadataRef File
                                                          , unsigned Line
                                                          , LLVMMetadataRef Ty
                                                          , int AlwaysPreserve
                                                          , unsigned Flags
                                                          );

    LLVMMetadataRef LLVMDIBuilderCreateBasicType( LLVMDIBuilderRef D
                                                  , const char *Name
                                                  , uint64_t SizeInBits
                                                  , uint64_t AlignInBits
                                                  , unsigned Encoding
                                                  );

    LLVMMetadataRef LLVMDIBuilderCreatePointerType( LLVMDIBuilderRef D
                                                    , LLVMMetadataRef PointeeType
                                                    , uint64_t SizeInBits
                                                    , uint64_t AlignInBits
                                                    , const char *Name
                                                    );

    LLVMMetadataRef LLVMDIBuilderCreateQualifiedType( LLVMDIBuilderRef D
                                                      , uint32_t Tag
                                                      , LLVMMetadataRef BaseType
                                                      );

    LLVMMetadataRef LLVMDIBuilderCreateSubroutineType( LLVMDIBuilderRef D
                                                       , LLVMMetadataRef ParameterTypes
                                                       , unsigned Flags
                                                       );

    LLVMMetadataRef LLVMDIBuilderCreateStructType( LLVMDIBuilderRef D
                                                   , LLVMMetadataRef Scope
                                                   , const char *Name
                                                   , LLVMMetadataRef File
                                                   , unsigned Line
                                                   , uint64_t SizeInBits
                                                   , uint64_t AlignInBits
                                                   , unsigned Flags
                                                   , LLVMMetadataRef DerivedFrom
                                                   , LLVMMetadataRef ElementTypes
                                                   );

    LLVMMetadataRef LLVMDIBuilderCreateReplaceableCompositeType( LLVMDIBuilderRef D
                                                                 , unsigned Tag
                                                                 , const char *Name
                                                                 , LLVMMetadataRef Scope
                                                                 , LLVMMetadataRef File
                                                                 , unsigned Line
                                                                 , unsigned RuntimeLang
                                                                 , uint64_t SizeInBits
                                                                 , uint64_t AlignInBits
                                                                 , unsigned Flags
                                                                 );

    LLVMMetadataRef LLVMDIBuilderCreateMemberType( LLVMDIBuilderRef D
                                                   , LLVMMetadataRef Scope
                                                   , const char *Name
                                                   , LLVMMetadataRef File
                                                   , unsigned Line
                                                   , uint64_t SizeInBits
                                                   , uint64_t AlignInBits
                                                   , uint64_t OffsetInBits
                                                   , unsigned Flags
                                                   , LLVMMetadataRef Ty
                                                   );

    LLVMMetadataRef LLVMDIBuilderCreateArrayType( LLVMDIBuilderRef D
                                                  , uint64_t SizeInBits
                                                  , uint64_t AlignInBits
                                                  , LLVMMetadataRef ElementType
                                                  , LLVMMetadataRef Subscripts
                                                  );

    LLVMMetadataRef LLVMDIBuilderCreateVectorType( LLVMDIBuilderRef D
                                                   , uint64_t SizeInBits
                                                   , uint64_t AlignInBits
                                                   , LLVMMetadataRef ElementType
                                                   , LLVMMetadataRef Subscripts
                                                   );

    LLVMMetadataRef LLVMDIBuilderCreateTypedef( LLVMDIBuilderRef D
                                                , LLVMMetadataRef Ty
                                                , const char *Name
                                                , LLVMMetadataRef File
                                                , unsigned Line
                                                , LLVMMetadataRef Context
                                                );

    LLVMMetadataRef LLVMDIBuilderGetOrCreateSubrange( LLVMDIBuilderRef D
                                                      , int64_t Lo
                                                      , int64_t Count
                                                      );

    LLVMMetadataRef LLVMDIBuilderGetOrCreateArray( LLVMDIBuilderRef D
                                                   , LLVMMetadataRef *Data
                                                   , size_t Length
                                                   );

    LLVMMetadataRef LLVMDIBuilderGetOrCreateTypeArray( LLVMDIBuilderRef D
                                                       , LLVMMetadataRef *Data
                                                       , size_t Length
                                                       );

    LLVMMetadataRef LLVMDIBuilderCreateExpression( LLVMDIBuilderRef Dref
                                                   , int64_t *Addr
                                                   , size_t Length
                                                   );

    LLVMValueRef LLVMDIBuilderInsertDeclareAtEnd( LLVMDIBuilderRef D
                                                  , LLVMValueRef Storage
                                                  , LLVMMetadataRef VarInfo
                                                  , LLVMMetadataRef Expr
                                                  , LLVMMetadataRef diLocation
                                                  , LLVMBasicBlockRef Block
                                                  );

    LLVMValueRef LLVMDIBuilderInsertValueAtEnd( LLVMDIBuilderRef D
                                                , LLVMValueRef Val
                                                , uint64_t Offset
                                                , LLVMMetadataRef VarInfo
                                                , LLVMMetadataRef Expr
                                                , LLVMMetadataRef diLocation
                                                , LLVMBasicBlockRef Block
                                                );

    /// createEnumerationType - Create debugging information entry for an
    /// enumeration.
    /// @param Scope          Scope in which this enumeration is defined.
    /// @param Name           Union name.
    /// @param File           File where this member is defined.
    /// @param LineNumber     Line number.
    /// @param SizeInBits     Member size.
    /// @param AlignInBits    Member alignment.
    /// @param Elements       Enumeration elements.
    /// @param UnderlyingType Underlying type of a C++11/ObjC fixed enum.
    /// @param UniqueIdentifier A unique identifier for the enum.
    LLVMMetadataRef LLVMDIBuilderCreateEnumerationType( LLVMDIBuilderRef D
                                                        , LLVMMetadataRef Scope          // DIScope
                                                        , char const* Name
                                                        , LLVMMetadataRef File           // DIFile
                                                        , unsigned LineNumber
                                                        , uint64_t SizeInBits
                                                        , uint64_t AlignInBits
                                                        , LLVMMetadataRef Elements       // DIArray
                                                        , LLVMMetadataRef UnderlyingType // DIType
                                                        , char const*
                                                        );

    /// createEnumerator - Create a single enumerator value.
    //DIEnumerator createEnumerator( StringRef Name, int64_t Val );
    LLVMMetadataRef LLVMDIBuilderCreateEnumeratorValue( LLVMDIBuilderRef D, char const* Name, int64_t Val );

    /// createGlobalVariable - Create a new descriptor for the specified
    /// variable.
    /// @param Context     Variable scope.
    /// @param Name        Name of the variable.
    /// @param LinkageName Mangled  name of the variable.
    /// @param File        File where this variable is defined.
    /// @param LineNo      Line number.
    /// @param Ty          Variable Type.
    /// @param isLocalToUnit Boolean flag indicate whether this variable is
    ///                      externally visible or not.
    /// @param Val         llvm::Value of the variable.
    /// @param Decl        Reference to the corresponding declaration.
    /*DIGlobalVariable*/
    LLVMMetadataRef LLVMDIBuilderCreateGlobalVariable( LLVMDIBuilderRef D
                                                       , LLVMMetadataRef Context
                                                       , char const* Name
                                                       , char const* LinkageName
                                                       , LLVMMetadataRef File  // DIFile
                                                       , unsigned LineNo
                                                       , LLVMMetadataRef Ty    //DITypeRef
                                                       , LLVMBool isLocalToUnit
                                                       , LLVMValueRef Val
                                                       , LLVMMetadataRef Decl // = nullptr
                                                       );

    LLVMDwarfTag LLVMDIDescriptorGetTag( LLVMMetadataRef descriptor );

    /// insertDeclare - Insert a new llvm.dbg.declare intrinsic call.
    /// @param Storage      llvm::Value of the variable
    /// @param VarInfo      Variable's debug info descriptor.
    /// @param Expr         A complex location expression.
    /// @param InsertBefore Location for the new intrinsic.
    /*Instruction */
    LLVMValueRef LLVMDIBuilderInsertDeclareBefore( LLVMDIBuilderRef Dref
                                                   , LLVMValueRef Storage      // Value
                                                   , LLVMMetadataRef VarInfo   // DIVariable
                                                   , LLVMMetadataRef Expr      // DIExpression
                                                   , LLVMMetadataRef diLocation
                                                   , LLVMValueRef InsertBefore // Instruction
                                                   );

    /// Insert a new llvm.dbg.value intrinsic call.
    /// \param Val          llvm::Value of the variable
    /// \param Offset       Offset
    /// \param VarInfo      Variable's debug info descriptor.
    /// \param Expr         A complex location expression.
    /// \param DL           Debug info location.
    /// \param InsertBefore Location for the new intrinsic.
    /*Instruction **/
    LLVMValueRef LLVMDIBuilderInsertValueBefore( LLVMDIBuilderRef Dref
                                                 , /*llvm::Value **/LLVMValueRef Val
                                                 , uint64_t Offset
                                                 , /*DILocalVariable **/ LLVMMetadataRef VarInfo
                                                 , /*DIExpression **/ LLVMMetadataRef Expr
                                                 , /*const DILocation **/ LLVMMetadataRef DL
                                                 , /*Instruction **/ LLVMValueRef InsertBefore
                                                 );

    // caller must call LLVMDisposeMessage() on the returned string
    char const* LLVMMetadataAsString( LLVMMetadataRef descriptor );

    void LLVMMDNodeReplaceAllUsesWith( LLVMMetadataRef oldDescriptor, LLVMMetadataRef newDescriptor );

    LLVMMetadataRef LLVMDILocation( LLVMContextRef context, unsigned Line, unsigned Column, LLVMMetadataRef scope, LLVMMetadataRef InlinedAt );
    LLVMBool LLVMSubProgramDescribes( LLVMMetadataRef subProgram, LLVMValueRef /*const Function **/F );
    LLVMMetadataRef LLVMDIBuilderCreateNamespace( LLVMDIBuilderRef Dref, LLVMMetadataRef scope, char const* name, LLVMMetadataRef file, unsigned line );
    LLVMContextRef LLVMGetNodeContext( LLVMMetadataRef /*MDNode*/ node );
    LLVMMetadataKind LLVMGetMetadataID( LLVMMetadataRef /*Metadata*/ md );

    uint32_t LLVMMDNodeGetNumOperands( LLVMMetadataRef /*MDNode*/ node );
    LLVMMDOperandRef LLVMMDNodeGetOperand( LLVMMetadataRef /*MDNode*/ node, uint32_t index );
    LLVMMetadataRef LLVMGetOperandNode( LLVMMDOperandRef operand );
    /*DISubProgram*/ LLVMMetadataRef LLVMDILocalScopeGetSubProgram( LLVMMetadataRef /*DILocalScope*/ localScope );

#ifdef __cplusplus
} // extern "C"
#endif

#endif

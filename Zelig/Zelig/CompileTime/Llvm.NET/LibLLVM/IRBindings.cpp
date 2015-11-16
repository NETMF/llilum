//===- IRBindings.cpp - Additional bindings for ir ------------------------===//
//
//                     The LLVM Compiler Infrastructure
//
// This file is distributed under the University of Illinois Open Source
// License. See LICENSE.TXT for details.
//
//===----------------------------------------------------------------------===//
//
// This file defines additional C bindings for the ir component.
//
//===----------------------------------------------------------------------===//

#include "IRBindings.h"

#include "llvm/IR/Attributes.h"
#include "llvm/IR/DebugLoc.h"
#include "llvm/IR/Function.h"
#include "llvm/IR/IRBuilder.h"
#include "llvm/IR/LLVMContext.h"
#include "llvm/IR/Module.h"
#include "llvm/IR/CallSite.h"

using namespace llvm;

extern "C"
{
    unsigned LLVMGetAttributeSetSize( )
    {
        return sizeof( AttributeSet );
    }

    void LLVMCopyConstructAttributeSet( uintptr_t pDst, uintptr_t pSrc )
    {
        AttributeSet const& srcAttributes = *reinterpret_cast< AttributeSet const* >( pSrc );
        AttributeSet&  dstAttributes = *reinterpret_cast< AttributeSet* >( pDst );
        dstAttributes = srcAttributes;
    }

    void LLVMAttributeSetAddAttribute( LLVMContextRef context, uintptr_t pAttributeSet, int index, LLVMAttrKind kind )
    {
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        attributes = attributes.addAttribute( *unwrap( context ), index, ( Attribute::AttrKind )kind );
    }

    void LLVMAttributeSetAddTargetDependentAttribute( LLVMContextRef context, uintptr_t pAttributeSet, int index, char const* name, char const* value )
    {
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        attributes = attributes.addAttribute( *unwrap( context ), index, name, value );
    }

    void LLVMAttributeSetRemoveTargetDependentAttribute( LLVMContextRef context, uintptr_t pAttributeSet, int index, char const* name )
    {
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        AttrBuilder bldr;
        bldr.addAttribute( name );
        attributes = attributes.removeAttributes( *unwrap( context ), index, bldr );
    }

    LLVMBool LLVMAttributeSetHasAttribute( uintptr_t pAttributeSet, int index, LLVMAttrKind kind )
    {
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        return attributes.hasAttribute( index, ( Attribute::AttrKind )kind );
    }

    void LLVMAttributeSetRemoveAttribute( LLVMContextRef context, uintptr_t pAttributeSet, int index, LLVMAttrKind kind )
    {
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        attributes = attributes.removeAttribute( *unwrap( context ), index, (Attribute::AttrKind)kind );
    }

    void LLVMAttributeSetSetAttributeValue( LLVMContextRef context, uintptr_t pAttributeSet, int index, LLVMAttrKind kind, uint64_t value )
    {
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        AttrBuilder builder( attributes, index );
        switch( kind )
        {
        case LLVMAttrKind::LLVMAttrKindAlignment:
            assert( index > AttributeSet::AttrIndex::ReturnIndex && "Expected parameter index");
            assert( value <= UINT32_MAX && "expected value <= UINT32_MAX");
            builder.addAlignmentAttr( value );
            break;

        case LLVMAttrKind::LLVMAttrKindStackAlignment:
            assert( index == AttributeSet::AttrIndex::FunctionIndex && "Stack alignment only applicable to the function itself" );
            assert( value <= UINT32_MAX && "expected value <= UINT32_MAX" );
            builder.addStackAlignmentAttr( value );
            break;

        case LLVMAttrKind::LLVMAttrKindDereferenceable:
            assert( index != AttributeSet::AttrIndex::FunctionIndex && "Expected a return or param index" );
            builder.addDereferenceableAttr( value );
            break;

        case LLVMAttrKind::LLVMAttrKindDereferenceableOrNull:
            assert( index != AttributeSet::AttrIndex::FunctionIndex && "Expected a return or param index" );
            builder.addDereferenceableOrNullAttr( value );
            break;

        default:
            assert( false && "Attribute kind doesn't have a value to set" );
            break;
        }

        LLVMContext& ctx = *unwrap( context );
        attributes = attributes.addAttributes( ctx, index, AttributeSet::get( ctx, index, builder ) );
    }

    uint64_t LLVMAttributeSetGetAttributeValue( uintptr_t pAttributeSet, int index, LLVMAttrKind kind )
    {
        AttributeSet* pAttributes = reinterpret_cast< AttributeSet* >( pAttributeSet );
        Attribute attr = pAttributes->getAttribute( index, ( Attribute::AttrKind )kind );
        return attr.getValueAsInt( );
    }

    LLVMBool LLVMAttributeSetHasAttributes( uintptr_t pAttributeSet, int index )
    {
        AttributeSet* pAttributes = reinterpret_cast< AttributeSet* >( pAttributeSet );
        return pAttributes->hasAttributes( index );
    }

    char const* LLVMAttributeSetGetAttributesAsString( uintptr_t pAttributeSet, int index )
    {
        AttributeSet* pAttributes = reinterpret_cast< AttributeSet* >( pAttributeSet );
        return LLVMCreateMessage( pAttributes->getAsString( index ).c_str( ) );
    }

    LLVMBool LLVMAttributeSetHasTargetDependentAttribute( uintptr_t pAttributeSet, int index, char const* name )
    {
        AttributeSet* pAttributes = reinterpret_cast< AttributeSet* >( pAttributeSet );
        return pAttributes->hasAttribute( index, name );
    }

    LLVMBool LLVMAttributeSetHasAny( uintptr_t pAttributeSet, int index )
    {
        AttributeSet* pAttributes = reinterpret_cast< AttributeSet* >( pAttributeSet );
        return pAttributes->hasAttributes( index );
    }

    void LLVMGetFunctionAttributeSet( LLVMValueRef /*Function*/ function, uintptr_t pAttributeSet )
    {
        Function* pFunc = unwrap<Function>( function );
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        attributes = pFunc->getAttributes( );
    }

    void LLVMSetFunctionAttributeSet( LLVMValueRef /*Function*/ function, uintptr_t pAttributeSet )
    {
        Function* pFunc = unwrap<Function>( function );
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        pFunc->setAttributes( attributes );
    }

    void LLVMAttributeSetGetParamAttributes( uintptr_t pAttributeSet, int index, uintptr_t pResult )
    {
        AttributeSet const& src = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        AttributeSet& resultSet = *reinterpret_cast< AttributeSet* >( pResult );
        resultSet = src.getParamAttributes( index );
    }

    void LLVMAttributeSetGetReturnAttributes( uintptr_t pAttributeSet, uintptr_t pResult )
    {
        AttributeSet const& src = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        AttributeSet& resultSet = *reinterpret_cast< AttributeSet* >( pResult );
        resultSet = src.getRetAttributes( );
    }

    void LLVMAttributeSetGetFunctionAttributes( int index, uintptr_t pAttributeSet, uintptr_t pResult )
    {
        AttributeSet const& src = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        AttributeSet& resultSet = *reinterpret_cast< AttributeSet* >( pResult );
        resultSet = src.getFnAttributes( );
    }

    void LLVMAttributeSetAddAttributes2( LLVMContextRef context, uintptr_t pSrcAttributeSet, int index, uintptr_t pAttributes, uintptr_t pResult )
    {
        AttributeSet const& src = *reinterpret_cast< AttributeSet* >( pSrcAttributeSet );
        AttributeSet const& attributes = *reinterpret_cast< AttributeSet* >( pAttributes );
        AttributeSet& resultSet = *reinterpret_cast< AttributeSet* >( pResult );
        resultSet = src.addAttributes( *unwrap( context ), index, attributes );
    }

    void LLVMGetCallSiteAttributeSet( LLVMValueRef /*Instruction*/ instruction, uintptr_t pAttributeSet )
    {
        CallSite call = CallSite( unwrap<Instruction>( instruction ) );

        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        attributes = call.getAttributes( );
    }

    void LLVMSetCallSiteAttributeSet( LLVMValueRef /*Instruction*/ instruction, uintptr_t pAttributeSet )
    {
        CallSite call = CallSite( unwrap<Instruction>( instruction ) );
        AttributeSet& attributes = *reinterpret_cast< AttributeSet* >( pAttributeSet );
        call.setAttributes( attributes );
    }

    LLVMMetadataRef LLVMConstantAsMetadata( LLVMValueRef C )
    {
        return wrap( ConstantAsMetadata::get( unwrap<Constant>( C ) ) );
    }

    LLVMMetadataRef LLVMMDString2( LLVMContextRef C, char const *Str, unsigned SLen )
    {
        return wrap( MDString::get( *unwrap( C ), StringRef( Str, SLen ) ) );
    }

    LLVMMetadataRef LLVMMDNode2( LLVMContextRef C
                                 , LLVMMetadataRef *MDs
                                 , unsigned Count
                                 )
    {
        auto node = MDNode::get( *unwrap( C )
                                 , ArrayRef<Metadata *>( unwrap( MDs ), Count )
                                 );
        return wrap( node );
    }

    LLVMMetadataRef LLVMTemporaryMDNode( LLVMContextRef C
                                         , LLVMMetadataRef *MDs
                                         , unsigned Count
                                         )
    {
        auto node = MDTuple::getTemporary( *unwrap( C )
                                          , ArrayRef<Metadata *>( unwrap( MDs ), Count )
                                          );
        return wrap( node.release() );
    }

    void LLVMAddNamedMetadataOperand2( LLVMModuleRef M
                                       , char const *name
                                       , LLVMMetadataRef Val
                                       )
    {
        NamedMDNode *N = unwrap( M )->getOrInsertNamedMetadata( name );
        if( !N )
            return;

        if( !Val )
            return;

        N->addOperand( unwrap<MDNode>( Val ) );
    }

    void LLVMSetMetadata2( LLVMValueRef Inst, unsigned KindID, LLVMMetadataRef MD )
    {
        MDNode *N = MD ? unwrap<MDNode>( MD ) : nullptr;
        unwrap<Instruction>( Inst )->setMetadata( KindID, N );
    }

    void LLVMMetadataReplaceAllUsesWith( LLVMMetadataRef MD, LLVMMetadataRef New )
    {
        auto *Node = unwrap<MDNode>( MD );
        Node->replaceAllUsesWith( unwrap<Metadata>( New ) );
        MDNode::deleteTemporary( Node );
    }

    void LLVMSetCurrentDebugLocation2( LLVMBuilderRef Bref
                                       , unsigned Line
                                       , unsigned Col
                                       , LLVMMetadataRef Scope
                                       , LLVMMetadataRef InlinedAt
                                       )
    {
        auto loc = DebugLoc::get( Line
                                  , Col
                                  , Scope ? unwrap<MDNode>( Scope ) : nullptr
                                  , InlinedAt ? unwrap<MDNode>( InlinedAt ) : nullptr
                                  );
        unwrap( Bref )->SetCurrentDebugLocation( loc );
    }

    LLVMBool LLVMIsTemporary( LLVMMetadataRef M )
    {
        auto pMetadata = unwrap<MDNode>( M );
        return pMetadata->isTemporary();
    }

    LLVMBool LLVMIsResolved( LLVMMetadataRef M )
    {
        auto pMetadata = unwrap<MDNode>( M );
        return pMetadata->isResolved( );
    }

    LLVMBool LLVMIsUniqued( LLVMMetadataRef M )
    {
        auto pMetadata = unwrap<MDNode>( M );
        return pMetadata->isUniqued( );
    }

    LLVMBool LLVMIsDistinct( LLVMMetadataRef M )
    {
        auto pMetadata = unwrap<MDNode>( M );
        return pMetadata->isDistinct( );
    }

    char const* LLVMGetMDStringText( LLVMMetadataRef mdstring, unsigned* len )
    {
        MDString const* S = unwrap<MDString>( mdstring );
        *len = S->getString( ).size( );
        return S->getString( ).data( );
    }

    void LLVMMDNodeResolveCycles( LLVMMetadataRef M )
    {
        MDNode* pNode = unwrap<MDNode>( M );
        if( pNode->isResolved() )
            return;

        pNode->resolveCycles( );
    }

    static AtomicOrdering mapFromLLVMOrdering( LLVMAtomicOrdering Ordering )
    {
        switch( Ordering )
        {
        case LLVMAtomicOrderingNotAtomic:
            return NotAtomic;
        case LLVMAtomicOrderingUnordered:
            return Unordered;
        case LLVMAtomicOrderingMonotonic:
            return Monotonic;
        case LLVMAtomicOrderingAcquire:
            return Acquire;
        case LLVMAtomicOrderingRelease:
            return Release;
        case LLVMAtomicOrderingAcquireRelease:
            return AcquireRelease;
        case LLVMAtomicOrderingSequentiallyConsistent:
            return SequentiallyConsistent;
        }

        llvm_unreachable( "Invalid LLVMAtomicOrdering value!" );
    }

    LLVMValueRef LLVMBuildAtomicCmpXchg( LLVMBuilderRef B
                                         , LLVMValueRef Ptr
                                         , LLVMValueRef Cmp
                                         , LLVMValueRef New
                                         , LLVMAtomicOrdering successOrdering
                                         , LLVMAtomicOrdering failureOrdering
                                         , LLVMBool singleThread
                                         )
    {
        auto builder = unwrap(B);
        auto cmpxchg = builder->CreateAtomicCmpXchg( unwrap( Ptr )
                                                     , unwrap( Cmp )
                                                     , unwrap( New )
                                                     , mapFromLLVMOrdering( successOrdering )
                                                     , mapFromLLVMOrdering( failureOrdering )
                                                     , singleThread ? SingleThread : CrossThread
                                                     );


        return wrap( cmpxchg );
    }
}

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

using namespace llvm;

extern "C"
{
    LLVMBool LLVMFunctionHasAttributes( LLVMValueRef Fn, int index )
    {
        Function *Func = unwrap<Function>( Fn );
        AttributeSet attributes = Func->getAttributes( );
        return attributes.hasAttributes( index );
    }

    char const* LLVMGetFunctionAttributesAsString( LLVMValueRef Fn, int index )
    {
        Function *Func = unwrap<Function>( Fn );
        AttributeSet attributes = Func->getAttributes( );
        return LLVMCreateMessage( attributes.getAsString( index ).c_str( ) );
    }

    void LLVMAddTargetDependentFunctionAttr2( LLVMValueRef Fn, int index, char const* name, char const* value )
    {
        Function *Func = unwrap<Function>( Fn );
        auto Idx =  static_cast<AttributeSet::AttrIndex>( index );
        AttrBuilder B;

        B.addAttribute( name, value );
        AttributeSet Set = AttributeSet::get( Func->getContext( ), Idx, B );
        Func->addAttributes( Idx, Set );
    }

    void LLVMRemoveTargetDependentFunctionAttr2( LLVMValueRef Fn, int index, char const* name )
    {
        Function *Func = unwrap<Function>( Fn );
        auto Idx = static_cast<AttributeSet::AttrIndex>( index );
        AttrBuilder B( Func->getAttributes(), index );
        B.removeAttribute( name );
        Func->setAttributes( AttributeSet::get( Func->getContext( ), Idx, B ) );
    }

    void LLVMSetFunctionAttributeValue( LLVMValueRef Fn, int index, LLVMAttrKind kind, uint64_t value )
    {
        Function *Func = unwrap<Function>( Fn );
        AttrBuilder builder( Func->getAttributes( ), index );
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
        auto newAttributeSet = AttributeSet::get( Func->getContext( ), AttributeSet::AttrIndex::FunctionIndex, builder );
        Func->setAttributes( newAttributeSet );
    }

    uint64_t LLVMGetFunctionAttributeValue( LLVMValueRef Fn, int index, LLVMAttrKind kind )
    {
        Function *Func = unwrap<Function>( Fn );
        AttributeSet const attributes = Func->getAttributes( );
        Attribute attr = attributes.getAttribute( index, (Attribute::AttrKind)kind );
        return attr.getValueAsInt();
    }

    void LLVMAddFunctionAttr2( LLVMValueRef Fn, int index, LLVMAttrKind kind )
    {
        Function *Func = unwrap<Function>( Fn );
        AttrBuilder builder( Func->getAttributes( ), index );
        builder.addAttribute( ( Attribute::AttrKind )kind );
        auto newAttributeSet = AttributeSet::get( Func->getContext( ), index, builder );
        Func->setAttributes( newAttributeSet );
    }

    LLVMBool LLVMHasFunctionAttr2( LLVMValueRef Fn, int index, LLVMAttrKind kind )
    {
        Function *Func = unwrap<Function>( Fn );
        AttributeSet const attributes = Func->getAttributes( );
        return attributes.hasAttribute( index, ( Attribute::AttrKind )kind );
    }

    LLVMBool LLVMHasTargetDependentAttribute( LLVMValueRef Fn, int index, char const* name )
    {
        Function *Func = unwrap<Function>( Fn );
        AttributeSet const attributes = Func->getAttributes( );
        return attributes.hasAttribute( index, name );
    }

    void LLVMRemoveFunctionAttr2( LLVMValueRef Fn, int index, LLVMAttrKind kind )
    {
        Function *Func = unwrap<Function>( Fn );
        AttrBuilder builder( Func->getAttributes( ), index );
        builder.removeAttribute( ( Attribute::AttrKind )kind );
        auto newAttributeSet = AttributeSet::get( Func->getContext( ), index, builder );
        Func->setAttributes( newAttributeSet );
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

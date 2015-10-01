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
    void LLVMAddFunctionAttr2( LLVMValueRef Fn, uint32_t Kind, uint64_t Value )
    {
        Function *Func = unwrap<Function>( Fn );
        const AttributeSet PAL = Func->getAttributes( );
        const Attribute Attr = Attribute::get( Func->getContext(), ( Attribute::AttrKind )Kind, Value );
        AttrBuilder B( Attr );
        const AttributeSet PALnew = PAL.addAttributes( Func->getContext( )
                                                       , AttributeSet::FunctionIndex
                                                       , AttributeSet::get( Func->getContext( )
                                                                          , AttributeSet::FunctionIndex
                                                                          , B
                                                                          )
                                                       );
        Func->setAttributes( PALnew );
    }

    void LLVMRemoveFunctionAttr2( LLVMValueRef Fn, uint32_t Kind )
    {
        Function *Func = unwrap<Function>( Fn );
        const AttributeSet PAL = Func->getAttributes( );
        const AttributeSet PALnew = PAL.removeAttribute( Func->getContext( )
                                                         , AttributeSet::FunctionIndex
                                                         , ( Attribute::AttrKind )Kind
                                                         );
        Func->setAttributes( PALnew );
    }

    LLVMMetadataRef LLVMConstantAsMetadata( LLVMValueRef C )
    {
        return wrap( ConstantAsMetadata::get( unwrap<Constant>( C ) ) );
    }

    LLVMMetadataRef LLVMMDString2( LLVMContextRef C, const char *Str, unsigned SLen )
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
                                       , const char *name
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
}
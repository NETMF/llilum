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
#include "llvm/IR/DebugInfoMetadata.h"
#include <type_traits>

using namespace llvm;

extern "C"
{
    void LLVMGetVersionInfo( LLVMVersionInfo* pVersionInfo )
    {
        *pVersionInfo = { LLVM_VERSION_MAJOR, LLVM_VERSION_MINOR, LLVM_VERSION_PATCH, LLVM_VERSION_STRING };
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

    LLVMMetadataRef LLVMFunctionGetSubprogram( LLVMValueRef function )
    {
        Function* pFunction = unwrap<Function>( function );
        return wrap( pFunction->getSubprogram( ) );
    }

    void LLVMFunctionSetSubprogram( LLVMValueRef function, LLVMMetadataRef subprogram )
    {
        Function* pFunction = unwrap<Function>( function );
        pFunction->setSubprogram( unwrap<DISubprogram>( subprogram ) );
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

    LLVMBool LLVMFunctionHasPersonalityFunction( LLVMValueRef function )
    {
        Function* pFunc = unwrap<Function>( function );
        return pFunc->hasPersonalityFn( );
    }
}

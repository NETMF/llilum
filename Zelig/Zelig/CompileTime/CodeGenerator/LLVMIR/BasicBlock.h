#pragma once
#include "LLVMHeaders.h"

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

class BasicBlock_impl;

ref class _Module;
ref class _Function;
ref class _Value;
ref class _Type;

public ref class _BasicBlock
{
private:
    //
    // State
    // 
    
    _Function      ^ owner;
    _Module	       ^ module;
    int             debugCurCol;
    int             debugCurLine;
    DISubprogram*   curDISub;

    //
    // Private methods
    // 

    //If the value was just loaded, returns to the unloaded ref, else it creates a tmp val.
    _Value^      RevertImmediate                    ( _Value^ val );
    _Value^      CastToFunctionPointer              ( _Value^ val, _Type^ funcTy );
    llvm::Value* DecorateInstructionWithDebugInfo   ( llvm::Value* inst );
    void         LoadParams                         ( _Function^ func, List< _Value^ >^ args, vector<llvm::Value* > &params );

internal:
    BasicBlock_impl* _pimpl;

public:
    // 
    // Contructors
    // 
    _BasicBlock( _Function^ owner, BasicBlock_impl* pimpl );

    //
    // Public methods
    //
    void InsertASMString( String^ ASM );
    //void InsertWarning(String^ msg);

    //
    // debug 
    //
    void    SetDebugInfo    ( int curLine, int curCol, String^ srcFile, String^ mangledName );

    //
    // Memory 
    _Value^ LoadToImmediate     ( _Value^ val );
    void    InsertStore         ( _Value^ dst, _Value^ src );
    void    InsertStoreIntoBT   ( _Value^ dst, _Value^ src );
    _Value^ LoadIndirect        ( _Value^ val, _Type^ ptrTy );
    void    InsertMemCpy        ( _Value^ dst, _Value^ src );
    void    InsertMemSet        ( _Value^ dst, unsigned char value );

    //
    // Binary ops
    //
    _Value^ InsertBinaryOp      ( int op, _Value^ a, _Value^ b, bool isSigned );
    _Value^ InsertUnaryOp       ( int op, _Value^ val, bool isSigned );

    //
    // Cmp and branch
    //
    _Value^ InsertCmp                   ( int predicate, bool isSigned, _Value^ valA, _Value^ valB );
    void    InsertUnconditionalBranch   ( _BasicBlock^ bb );
    void    InsertConditionalBranch     ( _Value^ cond, _BasicBlock^ trueBB, _BasicBlock^  falseBB );
    void    InsertSwitchAndCases        ( _Value^ cond, _BasicBlock^ defaultBB, List<int>^ casesValues, List<_BasicBlock^>^ casesBBs );

    //
    // Conversions 
    //
    _Value^ InsertZExt              ( _Value^ val, _Type^ ty, int significantBits );
    _Value^ InsertSExt              ( _Value^ val, _Type^ ty, int significantBits );
    _Value^ InsertTrunc             ( _Value^ val, _Type^ ty, int significantBits );
    _Value^ InsertPointerToInt      ( _Value^ val );
    _Value^ InsertIntToPointer      ( _Value^ val, _Type^ ptrTy );
    _Value^ GetAddressAsUIntPtr     ( _Value^ val );
    _Value^ GetBTCast               ( _Value^ val, _Type^ ty );
    _Value^ InsertSIToFPFloat       ( _Value^ val );
    _Value^ InsertSIToFPDouble      ( _Value^ val );
    _Value^ InsertFPFloatToFPDouble ( _Value^ val );
    _Value^ InsertFPToUI            ( _Value^ val, _Type^ ty );

    //
    // Types
    //
    _Value^ ExtractFirstElementFromBasicType( _Value^ val );
    _Value^ GetField                        ( _Value^ obj, _Type^ zTy, _Type^ fieldType, int offset );
    _Value^ IndexLLVMArray                  ( _Value^ obj, _Value^ idx );


    //
    // Func calls
    //
    _Value^ InsertCall          ( _Function^ func, List< _Value^ >^ args );
    _Value^ InsertIndirectCall  ( _Function^ func, _Value^ ptr, List< _Value^ >^ args );
    _Value^ GetFunctionArgument ( _Function^ func, int n );
    void    InsertRet           ( _Value^ val );
};

//--//

NS_END
NS_END
NS_END

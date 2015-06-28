#include "llvmheaders.h"
#include "BasicBlock.h"
#include "BasicBlock_impl.h"
#include "Value.h"
#include "Value_impl.h"
#include "Type.h"
#include "Type_impl.h"
#include "Module.h"
#include "Module_impl.h"
#include <map>

//
// Adds current location info to the instruction.
//
#define __D(X) DecorateInstructionWithDebugInfo(X)

//#define DUMP_INLINED_COMMENTS_AS_ASM_CALLS

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

BasicBlock_impl::BasicBlock_impl( llvm::BasicBlock* pbb )
{
    _pbb = pbb;
    builder = new IRBuilder<>( pbb );
}

llvm::BasicBlock* BasicBlock_impl::GetLLVMObject( )
{
    return _pbb;
}

// managed code
_BasicBlock::_BasicBlock( _Function ^owner, BasicBlock_impl* pimpl ) : owner( owner )
{
    _pimpl = pimpl;
    module = owner->Module( );
}

_Value^ _BasicBlock::CastToFunctionPointer( _Value^ val, _Type ^funcTy )
{
    Type_impl *timpl = module->GetOrInsertPointerType( funcTy )->_pimpl;

	val = LoadToImmediate(val);

	Value *llvmValue = val->_pimpl->GetLLVMObject();
	uint32_t idxs[] = { 0 };

	llvmValue = _pimpl->builder->CreateExtractValue(llvmValue, idxs);
	llvmValue = _pimpl->builder->CreateExtractValue(llvmValue, idxs);

    return gcnew _Value( module, new Value_impl( timpl,
		__D(_pimpl->builder->CreateBitCast(llvmValue, timpl->GetLLVMObjectForStorage(), "indirect_function_pointer_cast")), false));
}

_Value^ _BasicBlock::LoadToImmediate( _Value ^val )
{
    if( val->IsImmediate( ) ) return val;
    return gcnew _Value( module, new Value_impl( val->Type( )->_pimpl,
        __D( _pimpl->builder->CreateLoad( val->_pimpl->GetLLVMObject( ), "LoadToImmediate" ) ), true ) );
}

_Value^ _BasicBlock::RevertImmediate( _Value ^val )
{
    if( !val->IsImmediate( ) ) return val;

    if( isa<LoadInst>( val->_pimpl->GetLLVMObject( ) ) )
    {
        return gcnew _Value( module, new Value_impl( val->Type( )->_pimpl,
            cast<LoadInst>( val->_pimpl->GetLLVMObject( ) )->getPointerOperand( ), false ) );
    }

    return nullptr;
}

llvm::Value *_BasicBlock::DecorateInstructionWithDebugInfo( llvm::Value *inst )
{
    if( isa<llvm::Instruction>( inst ) )
    {
        if( curDISub != nullptr ) cast<llvm::Instruction>( inst )->setDebugLoc( DebugLoc::get( debugCurLine, debugCurCol, *curDISub ) );
    }
    return inst;
}

void _BasicBlock::InsertASMString( String ^ASM )
{
#ifdef DUMP_INLINED_COMMENTS_AS_ASM_CALLS

    string strASM = (const char*)(Marshal::StringToHGlobalAnsi(ASM)).ToPointer();
    std::vector<llvm::Type*>FuncTy_3_args;
    FunctionType* FuncTy_3 = FunctionType::get(llvm::Type::getVoidTy(owner->_pimpl->GetLLVMObject()->getContext()), FuncTy_3_args, false);
    auto asmFunc = InlineAsm::get(FuncTy_3, strASM, "", true);
    _pimpl->builder->CreateCall(asmFunc);

#endif
}

void _BasicBlock::SetDebugInfo( int curLine, int curCol, String^ srcFile, String^ mangledName )
{
    string mangledNameStr = std::string( (const char *)( Marshal::StringToHGlobalAnsi( mangledName ) ).ToPointer( ) );

    debugCurLine = curLine;
    debugCurCol = curCol;

    curDISub = module->_pimpl->GetDISubprogram( mangledNameStr );

    if( curDISub == nullptr )
    {
        curDISub = new DISubprogram( );

        module->SetCurrentDIFile( srcFile );

        string ffn = module->_pimpl->curDIFile->getFilename( );

        DIArray dita;
        DICompositeType funcType = module->_pimpl->dib->createSubroutineType( *module->_pimpl->curDIFile, dita );
        *curDISub = module->_pimpl->dib->createFunction( *module->_pimpl->curDIFile, mangledNameStr, mangledNameStr, *module->_pimpl->curDIFile, curLine, funcType, true, true, curLine, 0U, true, cast<Function>( owner->_pimpl->GetLLVMObject( ) ) );

        module->_pimpl->SetDISubprogram( mangledNameStr, curDISub );
    }
}

void _BasicBlock::InsertStore( _Value ^dst, _Value ^src )
{
    Value *llvmSrc = src->_pimpl->GetLLVMObject( );
    Value *llvmDst = dst->_pimpl->GetLLVMObject( );

    if( llvmSrc->getType( ) != cast<PointerType>( llvmDst->getType( ) )->getElementType( ) )
    {
        Console::WriteLine( "For \"Ptr must be a pointer to Val type!\" Assert." );
        Console::WriteLine( "getOperand(0)->getType()" );
        llvmSrc->getType( )->dump( ); Console::WriteLine( "" );
        Console::WriteLine( "cast<PointerType>(getOperand(1)->getType())->getElementType()" );
        cast<PointerType>( llvmDst->getType( ) )->getElementType( )->dump( ); Console::WriteLine( "" );
    }

    _pimpl->builder->CreateStore( llvmSrc, llvmDst );
}

void _BasicBlock::InsertStoreIntoBT( _Value ^dst, _Value ^src )
{
    Value *llvmSrc = src->_pimpl->GetLLVMObject( );
    Value *llvmDst = dst->_pimpl->GetLLVMObject( );

    if( !dst->IsPointer( ) )
    {
        uint32_t idxs[ ] = { 0 };
        _pimpl->builder->CreateInsertValue( llvmDst, llvmSrc, idxs );
    }
    else
    {
        llvmDst = _pimpl->builder->CreateStructGEP( llvmDst, 0 );

        if( llvmSrc->getType( ) != cast<PointerType>( llvmDst->getType( ) )->getElementType( ) )
        {
            Console::WriteLine( "For \"Ptr must be a pointer to Val type!\" Assert." );
            Console::WriteLine( "getOperand(0)->getType()" );
            llvmSrc->getType( )->dump( ); Console::WriteLine( "" );
            Console::WriteLine( "cast<PointerType>(getOperand(1)->getType())->getElementType()" );
            cast<PointerType>( llvmDst->getType( ) )->getElementType( )->dump( ); Console::WriteLine( "" );
        }

        _pimpl->builder->CreateStore( llvmSrc, llvmDst );
    }
}

_Value ^_BasicBlock::LoadIndirect( _Value ^val, _Type ^ptrTy )
{
    val = LoadToImmediate( val );

    if( val->Type( )->_pimpl->IsBoxed( ) )
    {
        return gcnew _Value( module, new Value_impl( val->Type( )->_pimpl->underlyingBoxedType,
            _pimpl->builder->CreateStructGEP( val->_pimpl->GetLLVMObject( ), 1 ),
            false ) );
    }
    return gcnew _Value( module, new Value_impl( ptrTy->_pimpl,
        _pimpl->builder->CreateBitCast( val->_pimpl->GetLLVMObject( ), ptrTy->_pimpl->GetLLVMObjectForStorage( )->getPointerTo( ) ),
        false ) );
}

void _BasicBlock::InsertMemCpy( _Value ^dst, _Value ^src )
{
    dst = RevertImmediate( dst );
    _Value ^ riSrc = RevertImmediate( src );

    //I can't do a memcopy because I can't find a
    //src address, so I do a copy by copy instead
    if( riSrc == nullptr )
    {
        Value *llvmDst = dst->_pimpl->GetLLVMObject( );
        Value *llvmSrc = src->_pimpl->GetLLVMObject( );

        //INSERT FIELD BY FIELD LOAD/STORE AND RETURN
        for( size_t i = 0; i < llvmDst->getType( )->getPointerElementType( )->getStructNumElements( ); ++i )
        {
            llvm::Value *idxs[ ] = { _pimpl->builder->getInt32( 0 ), _pimpl->builder->getInt32( i ) };
            uint32_t srcIdxs[ ] = { i };

            llvm::Value *tmpSrc = __D( _pimpl->builder->CreateExtractValue( llvmSrc, srcIdxs, "fieldByFieldSrcExtrsact" ) );
            llvm::Value *tmpDst = __D( _pimpl->builder->CreateInBoundsGEP( llvmDst, idxs, "fieldByFieldDstGep" ) );

            __D( _pimpl->builder->CreateStore( tmpSrc, tmpDst, false ) );
        }

        return;
    }

    src = riSrc;

    assert( src != nullptr && dst != nullptr );
    assert( src->IsPointer( ) );
    assert( dst->IsPointer( ) );
    assert( src->Type( )->__op_Equality( dst->Type( ) ) );

    _pimpl->builder->CreateMemCpy( dst->_pimpl->GetLLVMObject( ), src->_pimpl->GetLLVMObject( ), dst->Type( )->GetSizeInBits( ) / 8, 0, false );
}

void _BasicBlock::InsertMemSet( _Value ^dst, unsigned char value )
{
    dst = RevertImmediate( dst );
    assert( dst != nullptr );
    assert( dst->IsPointer( ) );

    _pimpl->builder->CreateMemSet( dst->_pimpl->GetLLVMObject( ), _pimpl->builder->getInt8( value ), dst->Type( )->GetSizeInBits( ) / 8, 0, false );
}

enum BinaryOperator_ALU_ENUM
{
    ADD = 0,
    SUB = 1,
    MUL = 2,
    DIV = 3,
    REM = 4,
    AND = 5,
    OR = 6,
    XOR = 7,
    SHL = 8,
    SHR = 9,
};

_Value^ _BasicBlock::InsertBinaryOp( int op, _Value^ a, _Value^ b, bool isSigned )
{
    BinaryOperator_ALU_ENUM binOp = (BinaryOperator_ALU_ENUM)op;
    static map<BinaryOperator_ALU_ENUM, string> binOpNames;
    if( binOpNames.empty( ) )
    {
        binOpNames[ ADD ] = "Add";
        binOpNames[ SUB ] = "Sub";
        binOpNames[ MUL ] = "Mul";
        binOpNames[ DIV ] = "Div";
        binOpNames[ REM ] = "Rem";
        binOpNames[ AND ] = "And";
        binOpNames[ OR ] = "Or";
        binOpNames[ XOR ] = "XOr";
        binOpNames[ SHL ] = "ShiftL";
        binOpNames[ SHR ] = "ShiftR";
    }

    assert( a->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( )
            || a->_pimpl->GetLLVMObject( )->getType( )->isFloatingPointTy( ) );
    assert( b->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( )
            || b->_pimpl->GetLLVMObject( )->getType( )->isFloatingPointTy( ) );

    llvm::Value *retVal;

    a = LoadToImmediate( a );
    b = LoadToImmediate( b );

    llvm::Value *loadedA = a->_pimpl->GetLLVMObject( );
    llvm::Value *loadedB = b->_pimpl->GetLLVMObject( );

    if( a->IsInteger( ) && b->IsInteger( ) )
    {
        switch( binOp )
        {
            case ADD:
            retVal = __D( _pimpl->builder->CreateAdd( loadedA, loadedB ) );
            break;
            case SUB:
            retVal = __D( _pimpl->builder->CreateSub( loadedA, loadedB ) );
            break;
            case MUL:
            retVal = __D( _pimpl->builder->CreateMul( loadedA, loadedB ) );
            break;
            case DIV:
            if( isSigned ) retVal = __D( _pimpl->builder->CreateSDiv( loadedA, loadedB ) );
            else retVal = __D( _pimpl->builder->CreateUDiv( loadedA, loadedB ) );
            break;
            case REM:
            if( isSigned ) retVal = __D( _pimpl->builder->CreateSRem( loadedA, loadedB ) );
            else retVal = __D( _pimpl->builder->CreateURem( loadedA, loadedB ) );
            break;
            case AND:
            retVal = __D( _pimpl->builder->CreateAnd( loadedA, loadedB ) );
            break;
            case OR:
            retVal = __D( _pimpl->builder->CreateOr( loadedA, loadedB ) );
            break;
            case XOR:
            retVal = __D( _pimpl->builder->CreateXor( loadedA, loadedB ) );
            break;
            case SHL:
            retVal = __D( _pimpl->builder->CreateShl( loadedA, loadedB ) );
            break;
            case SHR:
            if( isSigned ) retVal = __D( _pimpl->builder->CreateAShr( loadedA, loadedB ) );
            else retVal = __D( _pimpl->builder->CreateLShr( loadedA, loadedB ) );
            break;
            default:
            throw new runtime_error( ( string( "Parameters combination not supported for Binary Operator: " ) + binOpNames[ binOp ] ).c_str( ) );
        }
    }
    else if( a->IsFloatingPoint( ) && b->IsFloatingPoint( ) )
    {
        switch( binOp )
        {
            case ADD:
            retVal = __D( _pimpl->builder->CreateFAdd( loadedA, loadedB ) );
            break;
            case SUB:
            retVal = __D( _pimpl->builder->CreateFSub( loadedA, loadedB ) );
            break;
            case MUL:
            retVal = __D( _pimpl->builder->CreateFMul( loadedA, loadedB ) );
            break;
            case DIV:
            retVal = __D( _pimpl->builder->CreateFDiv( loadedA, loadedB ) );
            break;
            default:
            throw new runtime_error( ( string( "Parameters combination not supported for Binary Operator: " ) + binOpNames[ binOp ] ).c_str( ) );
        }
    }
    else throw new runtime_error( ( string( "Parameters combination not supported for Binary Operator: " ) + binOpNames[ binOp ] ).c_str( ) );

    return gcnew _Value( module, new Value_impl( a->Type( )->_pimpl, retVal, true ) );
}

enum UnaryOperator_ALU_ENUM
{
    NEG = 0,
    NOT = 1,
    FINITE = 2,
};

_Value^ _BasicBlock::InsertUnaryOp( int op, _Value^ val, bool isSigned )
{
    UnaryOperator_ALU_ENUM unOp = (UnaryOperator_ALU_ENUM)op;
    static map<UnaryOperator_ALU_ENUM, string> unOpNames;
    if( unOpNames.empty( ) )
    {
        unOpNames[ NEG ] = "Neg";
        unOpNames[ NOT ] = "Not";
        unOpNames[ FINITE ] = "Finite";
    }

    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) || val->_pimpl->GetLLVMObject( )->getType( )->isFloatingPointTy( ) );
    val = LoadToImmediate( val );

    llvm::Value *retVal = val->_pimpl->GetLLVMObject( );

    switch( unOp )
    {
        case NEG:
        if( val->IsInteger( ) )
        {
            retVal = __D( _pimpl->builder->CreateNeg( retVal ) );
        }
        else
        {
            retVal = __D( _pimpl->builder->CreateFNeg( retVal ) );
        }
        break;
        case NOT:
        retVal = __D( _pimpl->builder->CreateNot( retVal ) );
        break;
    }

    return gcnew _Value( module, new Value_impl( val->Type( )->_pimpl, retVal, true ) );
}

_Value ^_BasicBlock::InsertCmp( int predicate, bool isSigned, _Value ^valA, _Value ^valB )
{
    const int SignedBase = 10;
    const int FloatBase = SignedBase + 10;

    static std::map<int, CmpInst::Predicate> predicates;
    if( predicates.empty( ) )
    {
        predicates[ 0 ] = llvm::CmpInst::ICMP_EQ;
        predicates[ 1 ] = llvm::CmpInst::ICMP_UGE;
        predicates[ 2 ] = llvm::CmpInst::ICMP_UGT;
        predicates[ 3 ] = llvm::CmpInst::ICMP_ULE;
        predicates[ 4 ] = llvm::CmpInst::ICMP_ULT;
        predicates[ 5 ] = llvm::CmpInst::ICMP_NE;

        predicates[ SignedBase + 0 ] = llvm::CmpInst::ICMP_EQ;
        predicates[ SignedBase + 1 ] = llvm::CmpInst::ICMP_SGE;
        predicates[ SignedBase + 2 ] = llvm::CmpInst::ICMP_SGT;
        predicates[ SignedBase + 3 ] = llvm::CmpInst::ICMP_SLE;
        predicates[ SignedBase + 4 ] = llvm::CmpInst::ICMP_SLT;
        predicates[ SignedBase + 5 ] = llvm::CmpInst::ICMP_NE;

        predicates[ FloatBase + 0 ] = llvm::CmpInst::FCMP_OEQ;
        predicates[ FloatBase + 1 ] = llvm::CmpInst::FCMP_OGE;
        predicates[ FloatBase + 2 ] = llvm::CmpInst::FCMP_OGT;
        predicates[ FloatBase + 3 ] = llvm::CmpInst::FCMP_OLE;
        predicates[ FloatBase + 4 ] = llvm::CmpInst::FCMP_OLT;
        predicates[ FloatBase + 5 ] = llvm::CmpInst::FCMP_ONE;
    }

    assert( valA->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( )
            || valA->_pimpl->GetLLVMObject( )->getType( )->isFloatingPointTy( ) );
    assert( valB->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( )
            || valB->_pimpl->GetLLVMObject( )->getType( )->isFloatingPointTy( ) );

    valA = LoadToImmediate( valA );
    valB = LoadToImmediate( valB );

    llvm::Value *llvmValA = valA->_pimpl->GetLLVMObject( );
    llvm::Value *llvmValB = valB->_pimpl->GetLLVMObject( );

    Type_impl *booleanImpl = module->GetType( "LLVM.System.Boolean" )->_pimpl;

    if( valA->IsInteger( ) && valB->IsInteger( ) )
    {
        CmpInst::Predicate p = predicates[ predicate + ( isSigned ? SignedBase : 0 ) ];
        return gcnew _Value( module, new Value_impl( booleanImpl,
            __D( _pimpl->builder->CreateZExtOrBitCast( _pimpl->builder->CreateICmp( p, llvmValA, llvmValB, "icmp" ), booleanImpl->GetLLVMObject( ) ) ), true ) );
    }
    else if( valA->IsFloatingPoint( ) && valB->IsFloatingPoint( ) )
    {
        CmpInst::Predicate p = predicates[ predicate + FloatBase ];
        return gcnew _Value( module, new Value_impl( booleanImpl,
            __D( _pimpl->builder->CreateZExtOrBitCast( _pimpl->builder->CreateFCmp( p, llvmValA, llvmValB, "fcmp" ), booleanImpl->GetLLVMObject( ) ) ), true ) );
    }
    else
    {
        Console::WriteLine( "valA:" ); valA->Dump( );
        Console::WriteLine( "valB:" ); valB->Dump( );
        throw new runtime_error( "Parameters combination not supported for CMP Operator." );
    }
}

_Value ^_BasicBlock::InsertZExt( _Value ^val, _Type^ ty, int significantBits )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    val = LoadToImmediate( val );

    llvm::Value *retVal = __D( _pimpl->builder->CreateTruncOrBitCast( val->_pimpl->GetLLVMObject( ), _pimpl->builder->getIntNTy( significantBits ) ) );

    return gcnew _Value( module, new Value_impl( ty->_pimpl,
        __D( _pimpl->builder->CreateZExtOrBitCast( retVal, ty->_pimpl->GetLLVMObjectForStorage( ), "zext" ) ), true ) );
}

_Value ^_BasicBlock::InsertSExt( _Value ^val, _Type^ ty, int significantBits )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    val = LoadToImmediate( val );

    llvm::Value *retVal = __D( _pimpl->builder->CreateTruncOrBitCast( val->_pimpl->GetLLVMObject( ), _pimpl->builder->getIntNTy( significantBits ) ) );

    return gcnew _Value( module, new Value_impl( ty->_pimpl,
        __D( _pimpl->builder->CreateSExtOrBitCast( retVal, ty->_pimpl->GetLLVMObjectForStorage( ), "sext" ) ), true ) );
}

_Value ^_BasicBlock::InsertTrunc( _Value ^val, _Type^ ty, int significantBits )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    val = LoadToImmediate( val );

    llvm::Value *retVal = __D( _pimpl->builder->CreateTruncOrBitCast( val->_pimpl->GetLLVMObject( ), ty->_pimpl->GetLLVMObjectForStorage( ) ) );

    return gcnew _Value( module, new Value_impl( ty->_pimpl, retVal, true ) );
}

_Value ^_BasicBlock::InsertPointerToInt( _Value ^val )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isPointerTy( ) );
    val = LoadToImmediate( val );
    _Type ^ty = module->GetType( "LLVM.System.UInt32" );

    Value *llvmVal = _pimpl->builder->CreatePtrToInt( val->_pimpl->GetLLVMObject( ), ty->_pimpl->GetLLVMObjectForStorage( ) );

    if( !val->Type( )->IsValueType( ) && val->Type()->_pimpl->GetName()!="Microsoft.Zelig.Runtime.ObjectHeader" )
    {
        _Type ^ohTy = module->GetType( "Microsoft.Zelig.Runtime.ObjectHeader" );
        llvmVal = _pimpl->builder->CreateAdd( llvmVal, _pimpl->builder->getInt32( ohTy->GetSizeInBits( ) / 8 ), "headerOffsetAdd" );
    }

    return gcnew _Value( module, new Value_impl( ty->_pimpl, llvmVal, true ) );
}

_Value^ _BasicBlock::InsertIntToPointer( _Value ^val, _Type ^ptrTy )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    val = LoadToImmediate( val );

    Value *llvmVal = val->_pimpl->GetLLVMObject( );

	if (!ptrTy->IsValueType() && ptrTy->_pimpl->GetName() != "Microsoft.Zelig.Runtime.ObjectHeader")
    {
        _Type ^ohTy = module->GetType( "Microsoft.Zelig.Runtime.ObjectHeader" );
        llvmVal = _pimpl->builder->CreateSub( llvmVal, _pimpl->builder->getInt32( ohTy->GetSizeInBits( ) / 8 ), "headerOffsetSub" );
    }

    llvmVal = _pimpl->builder->CreateIntToPtr( llvmVal, ptrTy->_pimpl->GetLLVMObjectForStorage( ) );

    return gcnew _Value( module, new Value_impl( ptrTy->_pimpl, llvmVal, true ) );
}

_Value ^_BasicBlock::GetAddressAsUIntPtr( _Value ^val )
{
    _Type ^ptrTy = module->GetType( "LLVM.System.UIntPtr" );
    val = RevertImmediate( val );

    assert( val != nullptr );

    return gcnew _Value( module, new Value_impl( ptrTy->_pimpl, val->_pimpl->GetLLVMObject( ), true ) );
}

_Value ^_BasicBlock::GetBTCast( _Value ^val, _Type ^ty )
{
    if( val->Type( )->GetBTUnderlyingType( ) == nullptr ) return val;
    if( ty->GetBTUnderlyingType( ) == nullptr ) return val;

    if( val->Type( )->GetSizeInBitsForStorage( ) != ty->GetSizeInBitsForStorage( ) )
    {
        uint32_t idxs[ ] = { 0 };
        //Needs integer cast:
        val = LoadToImmediate( val );
        _Value ^newVal = owner->GetLocalStackValue( "ForParameterIntegerCast", ty );

        Value *llvmVal = _pimpl->builder->CreateExtractValue( val->_pimpl->GetLLVMObject( ), idxs );

        llvmVal = _pimpl->builder->CreateIntCast( llvmVal, newVal->Type( )->_pimpl->GetLLVMObject( )->getStructElementType( 0 ), false );

        _pimpl->builder->CreateStore(
            llvmVal,
            _pimpl->builder->CreateStructGEP( newVal->_pimpl->GetLLVMObject( ), 0 )
            );

        return newVal;
    }
    else
    {
        _Value ^retVal = RevertImmediate( val );
        if( retVal == nullptr ) return val;

        return gcnew _Value( module, new Value_impl( ty->_pimpl,
            _pimpl->builder->CreateBitCast( val->_pimpl->GetLLVMObject( ), ty->_pimpl->GetLLVMObject( )->getPointerTo( ) ), false ) );
    }
}
_Value ^_BasicBlock::ExtractFirstElementFromBasicType( _Value^ val )
{
    _Type ^ty = module->GetType( gcnew String( ( string( "LLVM." ) + val->Type( )->_pimpl->GetName( ) ).c_str( ) ) );

    assert( ty != nullptr );

    Value *llvmVal = val->_pimpl->GetLLVMObject( );

    if( llvmVal->getType( )->isPointerTy( ) )
    {
        llvmVal = _pimpl->builder->CreateLoad( llvmVal );
    }

    uint32_t idxs[ ] = { 0 };

    llvmVal = _pimpl->builder->CreateExtractValue( llvmVal, idxs, ".m_value" );

    return gcnew _Value( module, new Value_impl( ty->_pimpl, llvmVal, true ) );
}

_Value ^_BasicBlock::InsertSIToFPFloat( _Value^ val )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    _Type ^ty = module->GetType( "LLVM.System.Single" );
    val = LoadToImmediate( val );

    return gcnew _Value( module, new Value_impl( ty->_pimpl,
        __D( _pimpl->builder->CreateSIToFP( val->_pimpl->GetLLVMObject( ), ty->_pimpl->GetLLVMObject( ), "sitofp" ) ), true ) );
}

_Value ^_BasicBlock::InsertSIToFPDouble( _Value^ val )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    _Type ^ty = module->GetType( "LLVM.System.Double" );
    val = LoadToImmediate( val );

    return gcnew _Value( module, new Value_impl( ty->_pimpl,
        __D( _pimpl->builder->CreateSIToFP( val->_pimpl->GetLLVMObject( ), ty->_pimpl->GetLLVMObject( ), "sitofpd" ) ), true ) );
}

_Value^ _BasicBlock::InsertFPFloatToFPDouble( _Value^ val )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isFloatTy( ) );
    _Type ^ty = module->GetType( "LLVM.System.Double" );
    val = LoadToImmediate( val );

    return gcnew _Value( module, new Value_impl( ty->_pimpl,
        __D( _pimpl->builder->CreateFPExt( val->_pimpl->GetLLVMObject( ), ty->_pimpl->GetLLVMObject( ), "fpext" ) ), true ) );
}

_Value^ _BasicBlock::InsertFPToUI( _Value^ val, _Type ^ty )
{
    assert( val->_pimpl->GetLLVMObject( )->getType( )->isFloatingPointTy( ) );
    _Type^ intType = ty;
    val = LoadToImmediate( val );

    return gcnew _Value( module, new Value_impl( intType->_pimpl,
        __D( _pimpl->builder->CreateFPToUI( val->_pimpl->GetLLVMObject( ), intType->_pimpl->GetLLVMObject( ), "fptoui" ) ), true ) );
}

void _BasicBlock::InsertUnconditionalBranch( _BasicBlock ^bb )
{
    __D( _pimpl->builder->CreateBr( bb->_pimpl->GetLLVMObject( ) ) );
}

void _BasicBlock::InsertConditionalBranch( _Value^ cond, _BasicBlock^ trueBB, _BasicBlock ^ falseBB )
{
    assert( cond->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    cond = LoadToImmediate( cond );

    //Review: Testing all the bits for now. We need to check its always valid to trunc the condition
    // to the first bit if we want to change it. That being said, most of the times this should be
    // get rid on the instructions conv pass from LLVM.
    llvm::Value *vA = cond->_pimpl->GetLLVMObject( );
    llvm::Value *vB = ConstantInt::get( cond->Type( )->_pimpl->GetLLVMObject( ), 0 );

    llvm::Value *condVal = __D( _pimpl->builder->CreateICmpNE( vA, vB, "icmpe" ) );

    __D( _pimpl->builder->CreateCondBr( condVal, trueBB->_pimpl->_pbb, falseBB->_pimpl->_pbb ) );
}

void _BasicBlock::InsertSwitchAndCases( _Value^ cond, _BasicBlock^ defaultBB, List<int>^ casesValues, List<_BasicBlock^>^ casesBBs )
{
    assert( cond->_pimpl->GetLLVMObject( )->getType( )->isIntegerTy( ) );
    cond = LoadToImmediate( cond );
    llvm::SwitchInst *si = cast<llvm::SwitchInst>( __D( _pimpl->builder->CreateSwitch( cond->_pimpl->GetLLVMObject( ), defaultBB->_pimpl->_pbb, casesBBs->Count ) ) );

    for( int i = 0; i < casesBBs->Count; ++i )
    {
        si->addCase( _pimpl->builder->getInt32( casesValues[ i ] ), casesBBs[ i ]->_pimpl->_pbb );
    }
}

void _BasicBlock::LoadParams( _Function^ func, List< _Value^ >^ args, vector<llvm::Value *> &params )
{
    for( int i = 0; i < args->Count; ++i )
    {
        args[ i ] = LoadToImmediate( args[ i ] );

        Value *llvmValue = args[ i ]->_pimpl->GetLLVMObject( );
        llvm::Type *pty = func->_pimpl->GetLLVMObject( )->getType( )->getPointerElementType( )->getFunctionParamType( i );
        Type_impl *tiPty = Type_impl::GetTypeImpl( pty );

        //IntPtr/UIntPtr can be casted to anything
        if( args[ i ]->Type( )->_pimpl->GetName( ) == "System.IntPtr"
            || args[ i ]->Type( )->_pimpl->GetName( ) == "System.UIntPtr" )
        {
            args[ i ] = RevertImmediate( args[ i ] );
            assert( args[ i ] != nullptr );
            llvmValue = args[ i ]->_pimpl->GetLLVMObject( );
            llvmValue = _pimpl->builder->CreateLoad(
                _pimpl->builder->CreateBitCast( llvmValue, pty->getPointerTo( ) )
                );
        }

        //Anything can be casted to IntPtr/UIntPtr
        if( tiPty->GetName( ) == "System.IntPtr"
            || tiPty->GetName( ) == "System.UIntPtr" )
        {
            args[ i ] = RevertImmediate( args[ i ] );
            assert( args[ i ] != nullptr );
            llvmValue = args[ i ]->_pimpl->GetLLVMObject( );
            llvmValue = _pimpl->builder->CreateLoad(
                _pimpl->builder->CreateBitCast( llvmValue, pty->getPointerTo( ) )
                );
        }

        if( args[ i ]->Type( )->_pimpl->GetName( ).substr( 0, 5 ) == "LLVM." )
        {
            if( isa<Constant>( llvmValue ) )
            {
                llvmValue = new GlobalVariable( *module->_pimpl->GetLLVMObject( ),
                                                llvmValue->getType( ), true, GlobalValue::InternalLinkage, cast<Constant>( llvmValue ) );
            }
            else
            {
                args[ i ] = RevertImmediate( args[ i ] );
                assert( args[ i ] != nullptr );
                llvmValue = args[ i ]->_pimpl->GetLLVMObject( );
            }

            llvmValue = _pimpl->builder->CreateLoad(
                _pimpl->builder->CreateBitCast( llvmValue, pty->getPointerTo( ) )
                );
        }

        params.push_back( llvmValue );
    }
}

_Value^ _BasicBlock::InsertCall( _Function^ func, List< _Value^ >^ args )
{
    vector<llvm::Value *> params;
    LoadParams( func, args, params );

    llvm::Value *retVal = __D( _pimpl->builder->CreateCall( func->_pimpl->GetLLVMObject( ), params ) );

    if( cast<FunctionType>( func->Type( )->_pimpl->GetLLVMObject( ) )->getReturnType( )->isVoidTy( ) ) return nullptr;

    return gcnew _Value( module, new Value_impl(
        Type_impl::GetTypeImpl( cast<FunctionType>( func->Type( )->_pimpl->GetLLVMObject( ) )->getReturnType( ) ),
        retVal, true ) );
}

_Value^ _BasicBlock::InsertIndirectCall( _Function^ func, _Value^ ptr, List< _Value^ >^ args )
{
    vector<llvm::Value *> params;
    LoadParams( func, args, params );

    ptr = CastToFunctionPointer( ptr, func->Type( ) );

    llvm::Value *retVal = __D( _pimpl->builder->CreateCall( ptr->_pimpl->GetLLVMObject( ), params ) );

    if( cast<FunctionType>( func->Type( )->_pimpl->GetLLVMObject( ) )->getReturnType( )->isVoidTy( ) ) return nullptr;

    return gcnew _Value( module, new Value_impl(
        Type_impl::GetTypeImpl( cast<FunctionType>( func->Type( )->_pimpl->GetLLVMObject( ) )->getReturnType( ) ),
        retVal, true ) );
}

_Value^ _BasicBlock::GetFunctionArgument( _Function^ func, int n )
{
    int i = n;
    llvm::Function::arg_iterator AI = cast<Function>( func->_pimpl->GetLLVMObject( ) )->arg_begin( );
    while( i-- > 0 ) AI++;
    return gcnew _Value( module, new Value_impl( func->Type( )->_pimpl->functionArgs[ n ], AI, true ) );
}

static Type_impl *SetValuesForByteOffsetAccess( Type_impl *ty, vector<size_t>& values, int offset, string& fieldName )
{
    for( int i = 0; i < ty->fields.size( ); ++i )
    {
        bool firstFieldOfManagedObject = ( i == 0 && !ty->IsValueType( ) && ty != Type_impl::GetTypeImpl( "Microsoft.Zelig.Runtime.ObjectHeader" ) );
        int thisFieldSize = ty->fields[ i ]->type->GetSizeInBitsForStorage( ) / 8;
        if( firstFieldOfManagedObject ) thisFieldSize = ty->fields[ i ]->type->GetSizeInBits( ) / 8;
        int curOffset = offset - ( ty->fields[ i ]->offset + thisFieldSize );

        if( curOffset < 0 )
        {
            values.push_back( ty->fields[ i ]->finalIdx );
            //On non value types (nor objectheader which is special) we force inspection of the
            //super class
            fieldName = ty->fields[ i ]->name;
            if( !firstFieldOfManagedObject && ( -curOffset ) == thisFieldSize ) return ty->fields[ i ]->type;
            return SetValuesForByteOffsetAccess( ty->fields[ i ]->type, values, curOffset + thisFieldSize, fieldName );
        }
    }

    throw new runtime_error( "Invalid offset for field access!!" );
}

_Value^ _BasicBlock::GetField( _Value ^obj, _Type ^zTy, _Type ^fieldType, int offset )
{
    obj = LoadToImmediate( obj );

    //Special case for boxed types
    if( obj->Type( )->_pimpl->IsBoxed( ) )
    {
        llvm::Value *values[ ] = { _pimpl->builder->getInt32( 0 ), _pimpl->builder->getInt32( 1 ) };

        _Value ^retVal = gcnew _Value( module, new Value_impl(
            Type_impl::GetTypeImpl( obj->Type( )->_pimpl->GetLLVMObject( )->getStructElementType( 1 ) ),
            __D( _pimpl->builder->CreateGEP( obj->_pimpl->GetLLVMObject( ), values, "BoxedFieldAccessGep" ) )
            , false ) );

        obj = retVal;
    }

    Type_impl *finalTimpl = obj->Type( )->_pimpl;

    //Special case for indexing into pointer to value types
    if( obj->Type( )->_pimpl->underlyingPointerType != nullptr &&
        obj->Type( )->_pimpl->underlyingPointerType->IsValueType( ) )
    {
        finalTimpl = obj->Type( )->_pimpl->underlyingPointerType;
    }

    vector<size_t> values;
    string fieldName;
    Type_impl *timpl = SetValuesForByteOffsetAccess( finalTimpl, values, offset, fieldName );

    vector<llvm::Value *> valuesForGep;
    valuesForGep.push_back( _pimpl->builder->getInt32( 0 ) );

    for( int i = 0; i < values.size( ); ++i )
    {
        valuesForGep.push_back( _pimpl->builder->getInt32( values[ i ] ) );
    }

    _Value ^retVal = gcnew _Value( module, new Value_impl( timpl,
        __D( _pimpl->builder->CreateGEP( obj->_pimpl->GetLLVMObject( ), valuesForGep, finalTimpl->GetName( ) + "." + fieldName ) )
        , false ) );

    return retVal;
}

_Value^ _BasicBlock::IndexLLVMArray( _Value ^obj, _Value ^idx )
{
    idx = LoadToImmediate( idx );

    llvm::Value *idxs[ ] = { _pimpl->builder->getInt32( 0 ), idx->_pimpl->GetLLVMObject( ) };
    llvm::Value *retVal = __D( _pimpl->builder->CreateGEP( obj->_pimpl->GetLLVMObject( ), idxs ) );

    return gcnew _Value( module, new Value_impl(
        Type_impl::GetTypeImpl( obj->Type( )->_pimpl->GetLLVMObject( )->getArrayElementType( ) ), retVal, false ) );
}

void _BasicBlock::InsertRet( _Value^ val )
{
    if( val == nullptr )
    {
        __D( _pimpl->builder->CreateRetVoid( ) );
    }
    else
    {
        __D( _pimpl->builder->CreateRet( LoadToImmediate( val )->_pimpl->_pValue ) );
    }
}

NS_END
NS_END
NS_END
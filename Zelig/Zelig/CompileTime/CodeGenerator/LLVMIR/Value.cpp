#include "llvmheaders.h"
#include "Value.h"
#include "Value_impl.h"
#include "Type.h"
#include "Type_impl.h"
#include "Module.h"
#include "Module_impl.h"
#include "BasicBlock.h"
#include "BasicBlock_impl.h"

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//
// native code

Value_impl::Value_impl( Type_impl* typeImpl, llvm::Value* pValue, bool immediate ) : typeImpl( typeImpl ), immediate( immediate )
{
    _pValue = pValue;
}

llvm::Value* Value_impl::GetLLVMObject( )
{
    return _pValue;
}

void Value_impl::Dump( )
{
    printf( "Value: \n" );
    _pValue->dump( );
    printf( "Of Type: \n" );
    typeImpl->Dump( );
}

//*************************************************//
//*************************************************//
//*************************************************//

_Value::_Value( _Module^ module ) :module( module ), _pimpl( nullptr )
{
}

//Global
void _Value::globalCtor( _Module^ module, String^ name, _Type^ type, bool constant, Value_impl* initializer )
{
    const char* szName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );

    llvm::Value* val = new llvm::GlobalVariable( *module->_pimpl->GetLLVMObject( ),
                                                 type->_pimpl->GetLLVMObject( ), constant, initializer == nullptr ? GlobalValue::ExternalLinkage : GlobalValue::InternalLinkage,
                                                 cast<llvm::Constant>( initializer == nullptr ? nullptr : initializer->GetLLVMObject( ) ), szName );

    //				_pimpl = new Value_impl(type->_pimpl, val);
}

//Wrapper
_Value::_Value( _Module^ module, Value_impl* value ) : module( module )
{
    _pimpl = value;
}

_Type^ _Value::Type( )
{
    return module->GetOrInsertType( _pimpl->typeImpl );
}

_Module^ _Value::Module( )
{
    return module;
}

bool _Value::IsInteger( )
{
    return _pimpl->_pValue->getType( )->isIntegerTy( );
}

bool _Value::IsFloat( )
{
    return _pimpl->_pValue->getType( )->isFloatTy( );
}

bool _Value::IsDouble( )
{
    return _pimpl->_pValue->getType( )->isDoubleTy( );
}

bool _Value::IsFloatingPoint( )
{
    return _pimpl->_pValue->getType( )->isFloatingPointTy( );
}

bool _Value::IsPointer( )
{
    return _pimpl->_pValue->getType( )->isPointerTy( );
}

bool _Value::IsPointerPointer( )
{
    return _pimpl->_pValue->getType( )->isPointerTy( ) && _pimpl->_pValue->getType( )->getPointerElementType( )->isPointerTy( );
}

bool _Value::IsImmediate( )
{
    return _pimpl->immediate;
}

bool _Value::IsAnUninitializedGlobal( )
{
    GlobalVariable *gv = cast_or_null<GlobalVariable>( _pimpl->GetLLVMObject( ) );
    if( gv != nullptr )
    {
        return !gv->hasInitializer( );
    }
    assert( 0 && "Caling IsAnUninitializedGlobal() in a non GlobalVariable!" );
    return false;
}

void _Value::SetGlobalInitializer( _UntypedConstValue^ val )
{
    GlobalVariable *gv = cast_or_null<GlobalVariable>( _pimpl->GetLLVMObject( ) );
    if( gv != nullptr )
    {

        /*printf("Setting initializer of: "); gv->dump(); printf("\n");
        printf("To: "); val->val->dump();  printf("\n");*/
        return gv->setInitializer( cast<Constant>( val->val ) );
    }
}

bool _Value::IsZeroedValue( )
{
    if( !isa<Constant>( ( _pimpl->GetLLVMObject( ) ) ) ) return false;
    return cast<Constant>( _pimpl->GetLLVMObject( ) )->isZeroValue( );
}

void _Value::MergeToAndRemove( _Value^ targetVal )
{
    GlobalVariable *gv = cast_or_null<GlobalVariable>( _pimpl->GetLLVMObject( ) );
    if( gv != nullptr )
    {
        gv->replaceAllUsesWith( targetVal->_pimpl->GetLLVMObject( ) );
        gv->removeFromParent( );
    }
}

void _Value::FlagAsConstant( )
{
    GlobalVariable *gv = cast_or_null<GlobalVariable>( _pimpl->GetLLVMObject( ) );
    if( gv != nullptr )
    {
        gv->setConstant( true );
        gv->setSection( ".text" );
        gv->setUnnamedAddr( true );
    }
}

void _Value::Dump( )
{
    _pimpl->Dump( );
    printf( "%s", IsImmediate( ) ? "IS IMMEDIATE\n" : "" );
}

//*************************************************//
//*************************************************//
//*************************************************//

_Function::_Function( _Module^ module, String^ name, _Type^ funcType ) :_Value( module ), entryBlock( nullptr )
{
    const char* szName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );

    llvm::Value* val = module->_pimpl->GetLLVMObject( )->getOrInsertFunction( szName, cast<FunctionType>( funcType->_pimpl->GetLLVMObject( ) ) );

    Function *fn = cast<Function>( val );
    if( fn->empty( ) )
    {
        fn->setLinkage( GlobalValue::ExternalWeakLinkage );
    }

    _pimpl = new Value_impl( funcType->_pimpl, val, false );
}

_BasicBlock^ _Function::GetOrInsertBasicBlock( String^ blockName )
{
    Function *func = cast<Function>( _pimpl->GetLLVMObject( ) );
    string blockNameStr = (const char*)( Marshal::StringToHGlobalAnsi( blockName ) ).ToPointer( );

    for( Function::iterator i = func->begin( ), e = func->end( ); i != e; ++i )
    {
        if( i->getName( ) == blockNameStr )
        {
            return gcnew _BasicBlock( this, new BasicBlock_impl( i ) );
        }
    }

    BasicBlock_impl *bbi = new BasicBlock_impl( BasicBlock::Create( _pimpl->GetLLVMObject( )->getContext( ), blockNameStr, func ) );

    if( entryBlock == nullptr )
    {
        entryBlock = bbi;
    }

    return gcnew _BasicBlock( this, bbi );
}

_Value^ _Function::GetLocalStackValue( String^ name, _Type^ type )
{
    const char* szName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );

    Function *fn = cast<Function>( _pimpl->GetLLVMObject( ) );

    if( fn->empty( ) )
    {
        throw gcnew Exception( "Trying to add local value to empty function." );
    }

    BasicBlock &bb = fn->getEntryBlock( );

    IRBuilder<> TmpB( &bb, bb.begin( ) );

    llvm::Value *retVal = TmpB.CreateAlloca( type->_pimpl->GetLLVMObjectForStorage( ), nullptr, szName );

    return gcnew _Value( module, new Value_impl( type->_pimpl, retVal, false ) );
}

void _Function::DeleteBody( )
{
    llvm::Function *fn = cast<Function>( _pimpl->GetLLVMObject( ) );
    fn->deleteBody( );
}

void _Function::SetExternalLinkage( )
{
    cast<Function>( _pimpl->GetLLVMObject( ) )->setLinkage( GlobalValue::ExternalLinkage );
}

void _Function::SetInternalLinkage( )
{
    cast<Function>( _pimpl->GetLLVMObject( ) )->setLinkage( GlobalValue::InternalLinkage );
}

//--//

NS_END
NS_END
NS_END
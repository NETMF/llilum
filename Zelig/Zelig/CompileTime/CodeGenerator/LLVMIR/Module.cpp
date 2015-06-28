#include "llvmheaders.h"
#include "BasicBlock.h"
#include "BasicBlock_impl.h"
#include "Module.h"
#include "Module_impl.h"
#include "Value.h"
#include "Value_impl.h"
#include "Type.h"
#include "Type_impl.h"

#include <io.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <Windows.h>

#include <exception>

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

Module_impl::Module_impl( const char* assembly, unsigned nativeIntSize )
{
    InitializeNativeTarget( );
    _pC = new llvm::LLVMContext( );
    _pM = new Module( assembly, ( *_pC ) );
    _pB = new IRBuilder<>( ( *_pC ) );
    _nativeIntSize = nativeIntSize;

    _pM->addModuleFlag( Module::ModFlagBehavior::Override, "Debug Info Version", DEBUG_METADATA_VERSION );
    dib = new DIBuilder( *_pM );

    // _pM->setDataLayout( "e-S64" );

    dicu = dib->createCompileUnit( dwarf::DW_LANG_lo_user + 0xabc, "out.bc", "", "ZeligIR2LLVMIR", true, "", 0 );
	SetCurrentDIFile("nowhere");
}

llvm::BasicBlock* Module_impl::InsertBlock( llvm::Function* fp, string name )
{
    auto pBB = BasicBlock::Create( _pB->getContext( ), name.c_str( ), fp );
    return pBB;
}

bool Module_impl::DumpToFile( string filename, bool text )
{
    if( GetFileAttributesA( filename.c_str( ) ) != INVALID_FILE_ATTRIBUTES )
    {
        SetFileAttributesA( filename.c_str( ), FILE_ATTRIBUTE_NORMAL );
    }

    int fd_bc = _open( filename.c_str( ), _O_TRUNC | _O_CREAT | _O_BINARY | _O_RDWR );

    if( fd_bc == -1 )
    {
        cout << "failed to create IR file. GetLastError=0x" << hex << GetLastError( ) << endl;
        return false;
    }

    if( text )
    {
        _pM->print( raw_fd_ostream( fd_bc, true ), nullptr );
    }
    else
    {
        WriteBitcodeToFile( _pM, raw_fd_ostream( fd_bc, true ) );
    }
    return true;
}

bool Module_impl::Compile( )
{
    llvm::PassManager PM;

    PM.add( createVerifierPass( ) );
    // Memory To Register
    //PM.add(createPromoteMemoryToRegisterPass());
    //PM.add(createDeadCodeEliminationPass());
    // Make sure everything is still good.
    PM.add( createVerifierPass( ) );

    //PM.add(createPrintModulePass(outs(), "code compilation"));
    bool retVal = PM.run( *_pM );
    dib->finalize( );
    return retVal;
}

void Module_impl::SetCurrentDIFile( std::string fn )
{
    if( dIFiles.find( fn ) == dIFiles.end( ) )
    {
        dIFiles[ fn ] = new DIFile( );
        *dIFiles[ fn ] = dib->createFile( fn, "" );
    }

    curDIFile = dIFiles[ fn ];
}

DISubprogram *Module_impl::GetDISubprogram( std::string fn )
{
    return dISubprograms[ fn ];
}

void Module_impl::SetDISubprogram( std::string fn, DISubprogram *disub )
{
    if( dISubprograms[ fn ] == nullptr )
    {
        dISubprograms[ fn ] = disub;
    }
}

unsigned Module_impl::GetNativeIntSize( )
{
    return _nativeIntSize;
}

unsigned Module_impl::GetPointerSize( )
{
    return GetNativeIntSize( );
}

llvm::Module *Module_impl::GetLLVMObject( )
{
    return _pM;
}

//*************************************************//
//*************************************************//
//*************************************************//

_Module::_Module( String^ assemblyName, unsigned nativeIntSize )
{
    const char* name = (const char*)( Marshal::StringToHGlobalAnsi( assemblyName ) ).ToPointer( );
    _pimpl = new Module_impl( name, nativeIntSize );
    _globalsCounter = 0;
}

void _Module::SetCurrentDIFile( String^ fn )
{
    const char* name = (const char*)( Marshal::StringToHGlobalAnsi( fn ) ).ToPointer( );
    _pimpl->SetCurrentDIFile( name );
}

bool _Module::CheckTypeExistenceByName( String ^name )
{
    const char* strName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );
    return _pimpl->GetLLVMObject( )->getTypeByName( strName ) != nullptr;
}

_Type ^_Module::GetOrInsertType( Type_impl *timpl )
{
    return gcnew _Type( this, timpl );
}

_Type ^_Module::GetOrInsertType( String^ name, int sizeInBits )
{
    const char* strName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );
    return GetOrInsertType( Type_impl::GetOrInsertTypeImpl( this->_pimpl, strName, sizeInBits ) );
}

_Type ^_Module::GetOrInsertFunctionType( String^ name, _Type ^returnType, List<_Type ^>^ argTypes )
{
    const char* strName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );

    std::vector<llvm::Type *> params;

    if( argTypes != nullptr )
    {
        for each( _Type^ type in argTypes )
        {
            params.push_back( type->_pimpl->GetLLVMObjectForStorage( ) );
        }
    }

    llvm::Type *funcType = FunctionType::get( returnType->_pimpl->GetLLVMObjectForStorage( ), params, false );

    Type_impl *timpl = Type_impl::GetOrInsertTypeImpl( this->_pimpl, strName, 0, funcType );

    for each( _Type^ type in argTypes )
    {
        timpl->functionArgs.push_back( type->_pimpl );
    }

    return GetOrInsertType( timpl );
}

_Type ^_Module::GetOrInsertPointerType( String^ name, _Type ^underlyingType )
{
    const char* strName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );
    llvm::Type *ptrType;
    //Special case for void pointer
    assert( !underlyingType->_pimpl->GetLLVMObjectForStorage( )->isVoidTy( ) );

    ptrType = underlyingType->_pimpl->GetLLVMObjectForStorage( )->getPointerTo( );

    _Type ^retTy = GetOrInsertType( Type_impl::GetOrInsertTypeImpl( this->_pimpl, strName, _pimpl->GetPointerSize( ), ptrType ) );
    retTy->_pimpl->SetValueTypeFlag( true );
    retTy->_pimpl->SetHasHeaderFlag( false );
    retTy->_pimpl->SetBoxedFlag( false );
    retTy->_pimpl->underlyingPointerType = underlyingType->_pimpl;

    return retTy;
}

_Type ^_Module::GetOrInsertPointerType( _Type ^underlyingType )
{
    return GetOrInsertPointerType( gcnew String( ( underlyingType->_pimpl->GetName( ) + " *" ).c_str( ) ), underlyingType );
}

_Type ^_Module::GetOrInsertBoxedType( String^ name, _Type ^headerType, _Type ^underlyingType )
{
    const char* strName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );

    std::vector<llvm::Type*> members;
    members.push_back( headerType->_pimpl->GetLLVMObject( ) );
    members.push_back( underlyingType->_pimpl->GetLLVMObjectForStorage( ) );
    llvm::Type *structType = StructType::create( this->_pimpl->GetLLVMObject( )->getContext( ), members, strName, true );

    _Type ^retTy = GetOrInsertType( Type_impl::GetOrInsertTypeImpl( this->_pimpl, strName, headerType->GetSizeInBits( ) + underlyingType->GetSizeInBitsForStorage( ), structType ) );
    retTy->_pimpl->SetValueTypeFlag( false );
    retTy->_pimpl->SetHasHeaderFlag( true );
    retTy->_pimpl->SetBoxedFlag( true );
    retTy->_pimpl->underlyingBoxedType = underlyingType->_pimpl;

    return retTy;
}

_Type ^_Module::GetOrInsertZeroSizedArray( _Type ^type )
{
    llvm::Type *arrayType = ArrayType::get( type->_pimpl->GetLLVMObjectForStorage( ), 0 );

    _Type ^retTy = GetOrInsertType( Type_impl::GetOrInsertTypeImpl( this->_pimpl, std::string( "MemoryArray of " ) + type->_pimpl->GetName( ), type->GetSizeInBitsForStorage( ), arrayType ) );
    retTy->_pimpl->SetValueTypeFlag( true );
    retTy->_pimpl->SetHasHeaderFlag( false );
    retTy->_pimpl->SetBoxedFlag( false );
    return retTy;
}

_Type ^_Module::GetType( String^ name )
{
    const char* strName = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );
    Type_impl *timpl = Type_impl::GetTypeImpl( strName );
    if( timpl == nullptr ) return nullptr;
    return GetOrInsertType( timpl );
}

_Type ^_Module::GetVoidType( )
{
    return GetType( "System.Void" );
}

_Value ^_Module::GetIntConstant( _Type ^type, uint64_t v, bool isSigned )
{
    Constant *val = ConstantInt::get( _pimpl->GetLLVMObject( )->getContext( ),
                                      APInt( type->_pimpl->GetSizeInBits( ), v, isSigned ) );

    if( type->_pimpl->GetName( ) == "LLVM.System.IntPtr" || type->_pimpl->GetName( ) == "LLVM.System.UIntPtr" )
    {
        val = ConstantExpr::getIntToPtr( val, type->_pimpl->GetLLVMObject( ) );
    }

    return gcnew _Value( this, new Value_impl( type->_pimpl, val, true ) );
}

_Value ^_Module::GetFloatConstant( float c )
{
    _Type^ t = GetType( "LLVM.System.Single" );
    Value *val = llvm::ConstantFP::get( _pimpl->GetLLVMObject( )->getContext( ), APFloat( c ) );
    return gcnew _Value( this, new Value_impl( t->_pimpl, val, true ) );
}

_Value ^_Module::GetDoubleConstant( double c )
{
    _Type^ t = GetType( "LLVM.System.Double" );
    Value *val = llvm::ConstantFP::get( _pimpl->GetLLVMObject( )->getContext( ), APFloat( c ) );
    return gcnew _Value( this, new Value_impl( t->_pimpl, val, true ) );
}

_Value ^_Module::GetNullPointer( _Type ^type )
{
    Value *val = llvm::Constant::getNullValue( type->_pimpl->GetLLVMObjectForStorage( ) );
    return gcnew _Value( this, new Value_impl( type->_pimpl, val, true ) );
}

bool _Module::CheckIfAExtendsB( _Type ^a, _Type ^b )
{
    llvm::Type *tyA = a->_pimpl->GetLLVMObject( );
    llvm::Type *tyB = b->_pimpl->GetLLVMObject( );

    if( tyA == tyB ) return true;

    if( tyA->isStructTy( ) && tyB->isStructTy( ) )
    {
        StructType *stA = cast<StructType>( tyA );

        if( stA->isOpaque( ) ) return false;
        llvm::Type *stSuper = stA->getStructElementType( 0 );

        while( stSuper->isStructTy( ) )
        {
            if( stSuper == tyB ) return true;
            if( cast<StructType>( stSuper )->isOpaque( ) ) return false;
            stSuper = cast<StructType>( stSuper )->getStructElementType( 0 );
        }
    }
    return false;
}

bool _Module::CheckIfBothTypesPertainToTheSameHierarchy( _Type ^a, _Type ^b )
{
    return CheckIfAExtendsB( a, b ) || CheckIfAExtendsB( b, a );
}

_Function^ _Module::GetOrInsertFunction( String^ name, _Type^ funcType )
{
    return gcnew _Function( this, name, funcType );
}

_UntypedConstValue ^_Module::GetUCVStruct( _Type^ structType, List<_UntypedConstValue ^>^ structMembers, bool anon )
{
    vector<Constant *> fields;

    if( structMembers != nullptr )
    {
        for each( _UntypedConstValue^ ucv in structMembers )
        {
            llvm::Type *curType = cast<StructType>( structType->_pimpl->GetLLVMObject( ) )->getTypeAtIndex( fields.size( ) );
            llvm::Constant *curVal = ucv->val;

            //Zero initializer coersion shortcut:

            if( curVal->isZeroValue( ) )
            {
                curVal = Constant::getNullValue( curType );
            }
            else
            {
                if( curType->isPointerTy( ) && curVal->getType( ) != curType )
                {
                    //It seems that llvm is wrongly switching the bitcast to a GEP.
                    //Let's try to force it with a ptrtoint, inttotr
                    //curVal = ConstantExpr::getPtrToInt( curVal, llvm::IntegerType::get( _pimpl->GetLLVMObject( )->getContext( ), 32 ) );
                    //curVal = ConstantExpr::getIntToPtr( curVal, curType );
                    curVal = ConstantExpr::getBitCast( curVal, curType );
                }
            }

            fields.push_back( curVal );
        }
    }

    llvm::Constant *retVal;
    if( anon )
    {
        retVal = ConstantStruct::getAnon( fields, true );
    }
    else
    {
        retVal = ConstantStruct::get( cast<StructType>( structType->_pimpl->GetLLVMObject( ) ), fields );
    }

    return gcnew _UntypedConstValue( retVal );
}

_UntypedConstValue ^_Module::GetUCVArray( _Type^ arrayMemberType, List<_UntypedConstValue ^>^ arrayMembers )
{
    vector<Constant *> members;
    llvm::Type *curType = arrayMemberType->_pimpl->GetLLVMObjectForStorage( );

    if( arrayMembers != nullptr )
    {
        for each( _UntypedConstValue^ ucv in arrayMembers )
        {
            llvm::Constant *curVal = ucv->val;

            if( curType->isPointerTy( ) && curVal->getType( ) != curType )
            {
                //It seems that llvm is wrongly switching the bitcast to a GEP.
                //Let's try to force it with a ptrtoint, inttotr
                curVal = ConstantExpr::getPtrToInt( curVal, llvm::IntegerType::get( _pimpl->GetLLVMObject( )->getContext( ), 32 ) );
                curVal = ConstantExpr::getIntToPtr( curVal, curType );
                //curVal = ConstantExpr::getBitCast(curVal, curType);
            }

            members.push_back( curVal );
        }
    }

    return gcnew _UntypedConstValue(
        ConstantArray::get( ArrayType::get( curType, members.size( ) ), members ) );
}

_UntypedConstValue ^_Module::GetUCVInt( _Type ^type, uint64_t v, bool isSigned )
{
    return gcnew _UntypedConstValue( Constant::getIntegerValue( type->_pimpl->GetLLVMObject( ),
        APInt( type->_pimpl->GetSizeInBits( ), v, isSigned ) ) );
}

_UntypedConstValue ^_Module::GetUCVNullPointer( _Type ^type )
{
    return gcnew _UntypedConstValue( Constant::getNullValue( type->_pimpl->GetLLVMObjectForStorage( ) ) );
}

_UntypedConstValue ^_Module::GetUCVZeroInitialized( _Type ^type )
{
    return gcnew _UntypedConstValue( Constant::getNullValue( type->_pimpl->GetLLVMObjectForStorage( ) ) );
}

_UntypedConstValue ^_Module::GetUCVConstantPointerFromValue( _Value^ val )
{
    return gcnew _UntypedConstValue( cast<Constant>( val->_pimpl->GetLLVMObject( ) ) );
}

_Value ^_Module::GetGlobalFromUCV( _Type^ type, _UntypedConstValue ^ucv )
{
    char gName[ 256 ];

    sprintf( gName, "G%d", GetMonotonicUniqueId( ) );

    Value *val = new GlobalVariable( *_pimpl->GetLLVMObject( ), ucv->val->getType( ), false, GlobalValue::InternalLinkage, ucv->val, gName );

    return gcnew _Value( this, new Value_impl( type->_pimpl, val, !type->IsValueType( ) ) );
}

_Value ^_Module::GetUninitializedGlobal( _Type^ type )
{
    char gName[ 256 ];

    sprintf( gName, "G%d", GetMonotonicUniqueId( ) );

    Value *val = new GlobalVariable( *_pimpl->GetLLVMObject( ), type->_pimpl->GetLLVMObject( ), false, GlobalValue::InternalLinkage, nullptr, gName );

    return gcnew _Value( this, new Value_impl( type->_pimpl, val, !type->IsValueType( ) ) );
}

void _Module::CreateAlias( _Value^ val, String^ name )
{
    const char* str = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );
    llvm::Value *llvmVal = val->_pimpl->GetLLVMObject( );
    if( isa<llvm::GlobalValue>( llvmVal ) )
    {
        llvm::GlobalValue *alias = llvm::GlobalAlias::create( str, cast<llvm::GlobalValue>( llvmVal ) );
        alias->setLinkage( GlobalValue::ExternalLinkage );
    }
    else
    {
        printf( "Warning: Ignoring alias \"%s\" because aliasee is not global.", str );
    }
}

bool _Module::Compile( )
{
    return _pimpl->Compile( );
}

bool _Module::DumpToFile( String^ fileName, bool text )
{
    const char* str = (const char*)( Marshal::StringToHGlobalAnsi( fileName ) ).ToPointer( );
    return _pimpl->DumpToFile( str, text );
}

/**********************************/

_UntypedConstValue::_UntypedConstValue( llvm::Constant *val ) :val( val )
{
}

//--//

NS_END
NS_END
NS_END
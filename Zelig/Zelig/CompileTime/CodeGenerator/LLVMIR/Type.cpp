#include "llvmheaders.h"
#include "Type.h"
#include "Type_impl.h"
#include "Module.h"
#include "Module_impl.h"
#include "Value.h"
#include "Value_impl.h"
#include <string>

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

std::map<std::string, Type_impl* > Type_impl::typeImpls;
std::map<llvm::Type* , Type_impl* > Type_impl::typeImpls_reverseLookupForLLVMTypes;

void Type_impl::privateInit( Module_impl* owner, std::string name, int sizeInBits )
{
    this->owner = owner;
    this->sizeInBits = sizeInBits;
    this->name = name;
    this->isValueType = true;
    this->hasHeader = false;
    this->isBoxed = false;
    this->underlyingBoxedType = nullptr;
    this->underlyingPointerType = nullptr;
}

Type_impl::Type_impl( Module_impl* owner, std::string name, int sizeInBits, llvm::Type* ty )
{
    privateInit( owner, name, sizeInBits );
    llvmType = ty;
    typeImpls_reverseLookupForLLVMTypes[ llvmType ] = this;
}

Type_impl::Type_impl( Module_impl* owner, std::string name, int sizeInBits )
{
    privateInit( owner, name, sizeInBits );
    llvm::Module* module = owner->GetLLVMObject( );

    assert( module->getTypeByName( name ) == nullptr && "Trying to override typeimpl!" );

    if( name == "System.Void" )
    {
        llvmType = llvm::Type::getVoidTy( module->getContext( ) );
    }
    //Basic LLVM types used to break basic value types circular ref.
    else if( name == "LLVM.System.Boolean" || name == "LLVM.System.Char" || name == "LLVM.System.SByte"
             || name == "LLVM.System.Byte" || name == "LLVM.System.Int16" || name == "LLVM.System.UInt16"
             || name == "LLVM.System.Int32" || name == "LLVM.System.UInt32" || name == "LLVM.System.Int64"
             || name == "LLVM.System.UInt64" )
    {
        llvmType = llvm::Type::getIntNTy( module->getContext( ), sizeInBits );
    }
    else if( name == "LLVM.System.Single" )
    {
        llvmType = llvm::Type::getFloatTy( module->getContext( ) );
    }
    else if( name == "LLVM.System.Double" )
    {
        llvmType = llvm::Type::getDoubleTy( module->getContext( ) );
    }
    else if( name == "LLVM.System.IntPtr" || name == "LLVM.System.UIntPtr" )
    {
        llvmType = llvm::Type::getInt8PtrTy( module->getContext( ) );
    }
    else
    {
        llvmType = StructType::create( module->getContext( ), this->name );
    }

    typeImpls_reverseLookupForLLVMTypes[ llvmType ] = this;
}

Type_impl* Type_impl::GetOrInsertTypeImpl( Module_impl* owner, std::string name, int sizeInBits, llvm::Type* ty )
{
    if( typeImpls.find( name ) == typeImpls.end( ) )
    {
        typeImpls[ name ] = new Type_impl( owner, name, sizeInBits, ty );
    }
    return typeImpls[ name ];
}

Type_impl* Type_impl::GetOrInsertTypeImpl( Module_impl* owner, std::string name, int sizeInBits )
{
    if( typeImpls.find( name ) == typeImpls.end( ) )
    {
        typeImpls[ name ] = new Type_impl( owner, name, sizeInBits );
    }
    return typeImpls[ name ];
}

Type_impl* Type_impl::GetTypeImpl( std::string name )
{
    if( typeImpls.find( name ) == typeImpls.end( ) ) return nullptr;
    return typeImpls[ name ];
}

Type_impl* Type_impl::GetTypeImpl( llvm::Type* ty )
{
    Type_impl* retval = typeImpls_reverseLookupForLLVMTypes[ ty ];

    if( retval == nullptr )
    {
        if( isa<PointerType>( ty ) && isa<StructType>( ty->getPointerElementType( ) ) )
        {
            return  typeImpls_reverseLookupForLLVMTypes[ ty->getPointerElementType( ) ];
        }
        printf( "Dump of type that caused the assert: \n" );
        ty->dump( );
        assert( 0 );
    }
    return retval;
}

unsigned Type_impl::GetSizeInBits( )
{
    return sizeInBits;
}

unsigned Type_impl::GetSizeInBitsForStorage( )
{
    if( isValueType ) return GetSizeInBits( );
    return owner->GetPointerSize( );
}

void Type_impl::AddField( unsigned offset, Type_impl* type, bool forceInline, string name )
{
    Type_field* f = new Type_field( name, type, offset, forceInline );

    //
    // Add the field in offset order, by moving all other pointer to fields
    //
    fields.resize( fields.size( ) + 1 );
    size_t i = 0;
    for( i = fields.size( ) - 1; i > 0; --i )
    {
        if( fields[ i - 1 ]->offset <= f->offset )
            break;

        fields[ i ] = fields[ i - 1 ];
    }
    fields[ i ] = f;
}

void Type_impl::Internal_AddTypeToStruct( int& idx, vector < llvm::Type* > &llvmFields, size_t& i )
{
    fields[ idx ]->finalIdx = llvmFields.size( );
    if( fields[ idx ]->forceInline )
    {
        llvmFields.push_back( fields[ idx ]->type->GetLLVMObject( ) );
        i = ( i - 1 ) + fields[ idx ]->type->GetSizeInBits( ) / 8;
    }
    else
    {
        llvmFields.push_back( fields[ idx ]->type->GetLLVMObjectForStorage( ) );
        i = ( i - 1 ) + fields[ idx ]->type->GetSizeInBitsForStorage( ) / 8;
    }
    idx++;
}

void Type_impl::SetupFields( )
{
    if( !isa<StructType>( GetLLVMObject( ) ) || fields.size( ) == 0 ) return;

    vector < llvm::Type* > llvmFields;

    int idx = 0;

    for( size_t i = 0; i < sizeInBits / 8; ++i )
    {
        if( idx < fields.size( ) && fields[ idx ]->offset == i )
        {
            Internal_AddTypeToStruct( idx, llvmFields, i );
        }
        else
        {
            llvmFields.push_back( Type_impl::GetOrInsertTypeImpl( owner, "System.Byte", 8 )->GetLLVMObject( ) );
        }
    }

    for( ; idx < fields.size( ); ++idx )
    {
        size_t i = 0;

        Internal_AddTypeToStruct( idx, llvmFields, i );
    }

    cast<StructType>( llvmType )->setBody( llvmFields, true );
}

void Type_impl::SetHasHeaderFlag( bool val )
{
    hasHeader = val;
}

void Type_impl::SetValueTypeFlag( bool val )
{
    isValueType = val;
}

void Type_impl::SetBoxedFlag( bool val )
{
    isBoxed = val;
}

bool Type_impl::IsBoxed( )
{
    return isBoxed;
}

bool Type_impl::IsValueType( )
{
    return isValueType;
}

std::string Type_impl::GetName( )
{
    return name;
}

void Type_impl::Dump( )
{
    printf( "Name:%s\n", name.c_str( ) );
    llvmType->dump( );
    printf( "\n" );
}

llvm::Type* Type_impl::GetLLVMObjectForStorage( )
{
    if( isValueType ) return GetLLVMObject( );
    return GetLLVMObject( )->getPointerTo( );
}

llvm::Type* Type_impl::GetLLVMObject( )
{
    return llvmType;
}

//*************************************************//
//*************************************************//
//*************************************************//

_Type::_Type( _Module^ module, Type_impl* impl ) : module( module )
{
    _pimpl = impl;
}

void _Type::AddField( unsigned offset, _Type^ type, bool forceInline, String^ name )
{
    const char* nameStr = (const char*)( Marshal::StringToHGlobalAnsi( name ) ).ToPointer( );

    _pimpl->AddField( offset, type->_pimpl, forceInline, nameStr );
}

void _Type::SetupFields( )
{
    _pimpl->SetupFields( );
}

bool _Type::IsInteger( )
{
    return _pimpl->GetLLVMObject( )->isIntegerTy( );
}

bool _Type::IsFloat( )
{
    return _pimpl->GetLLVMObject( )->isFloatTy( );
}

bool _Type::IsDouble( )
{
    return _pimpl->GetLLVMObject( )->isDoubleTy( );
}

bool _Type::IsFloatingPoint( )
{
    return IsFloat( ) || IsDouble( );
}

bool _Type::IsPointer( )
{
    return _pimpl->GetLLVMObjectForStorage( )->isPointerTy( );
}

bool _Type::IsStruct( )
{
    return _pimpl->GetLLVMObjectForStorage( )->isStructTy( );
}

bool _Type::IsPointerPointer( )
{
    return _pimpl->GetLLVMObject( )->isPointerTy( ) && _pimpl->GetLLVMObject( )->getPointerElementType( )->isPointerTy( );
}

bool _Type::IsValueType( )
{
    return _pimpl->IsValueType( );
}

bool _Type::__op_Equality( _Type^ b )
{
    return _pimpl == b->_pimpl;
}

_Type^ _Type::GetBTUnderlyingType( )
{
    return module->GetType( gcnew String( ( string( "LLVM." ) + _pimpl->GetName( ) ).c_str( ) ) );
}

int _Type::GetSizeInBits( )
{
    return _pimpl->GetSizeInBits( );
}

int _Type::GetSizeInBitsForStorage( )
{
    return _pimpl->GetSizeInBitsForStorage( );
}

void _Type::SetValueTypeFlag( bool val )
{
    _pimpl->SetValueTypeFlag( val );
}

void _Type::Dump( )
{
    _pimpl->Dump( );
}

//--//

NS_END
NS_END
NS_END

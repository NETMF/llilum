#pragma once

#include "Value.h"
#include "Type.h"

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

using namespace System::Collections::Generic;

ref class _UntypedConstValue;
class Module_impl;

/*
 * Implements all major translation operations 
 *
 */
public ref class _Module
{
internal:

    //
    // State
    //

    Module_impl* _pimpl;

    _Type^ GetOrInsertType( Type_impl* timpl );

    //--//

    static __int64 _globalsCounter;

    static int GetMonotonicUniqueId( )
    {
        __int64% trackRefCounter = _globalsCounter;

        return (int)System::Threading::Interlocked::Increment( trackRefCounter );
    }

public:

    //
    // Contructors
    // 

    _Module( String^ assemblyName, unsigned nativeIntSize );

    //
    // Public Methods
    //

    // 
    // Debug
    // 
    void SetCurrentDIFile( String^ fn );

    //
    // Types
    //

    bool   CheckTypeExistenceByName ( String^ name );
    _Type^ GetOrInsertType          ( String^ name, int sizeInBits );
    _Type^ GetOrInsertFunctionType  ( String^ name, _Type^ returnType, List<_Type^ >^ argTypes );
    _Type^ GetOrInsertBoxedType     ( String^ name, _Type^ headerType, _Type^ underlyingType );
    _Type^ GetOrInsertPointerType   ( String^ name, _Type^ underlyingType );
    _Type^ GetOrInsertPointerType   ( _Type^ underlyingType );
    _Type^ GetOrInsertZeroSizedArray( _Type^ type );
    _Type^ GetType                  ( String^ name );
    _Type^ GetVoidType              ( );

    //
    // Constants
    // 

    _Value^ GetIntConstant      ( _Type^ type, uint64_t v, bool isSigned );
    _Value^ GetFloatConstant    ( float c );
    _Value^ GetDoubleConstant   ( double c );
    _Value^ GetNullPointer      ( _Type^ type );

    //
    // Hierahcy 
    // 
    bool CheckIfAExtendsB                         ( _Type^ a, _Type^ b );
    bool CheckIfBothTypesPertainToTheSameHierarchy( _Type^ a, _Type^ b );

    //
    // Functions
    //
    _Function^ GetOrInsertFunction( String^ name, _Type^ funcType );

    //
    // Constant values
    // 
    _UntypedConstValue^ GetUCVStruct                    ( _Type^ structType, List<_UntypedConstValue^ >^ structMembers, bool anon );
    _UntypedConstValue^ GetUCVArray                     ( _Type^ arrayMemberType, List<_UntypedConstValue^ >^ arrayMembers );
    _UntypedConstValue^ GetUCVInt                       ( _Type^ type, uint64_t v, bool isSigned );
    _UntypedConstValue^ GetUCVNullPointer               ( _Type^ type );
    _UntypedConstValue^ GetUCVZeroInitialized           ( _Type^ type );
    _UntypedConstValue^ GetUCVConstantPointerFromValue  ( _Value^ val );
    _Value^             GetGlobalFromUCV                ( _Type^ type, _UntypedConstValue^ ucv );
    _Value^             GetUninitializedGlobal          ( _Type^ type );

    //
    //Misc and Compile
    // 
    void CreateAlias( _Value^ val, String^ name );
    bool Compile    ( );
    bool DumpToFile ( String^ fileName, bool text );
};

//--//
//--//
//--//

public ref class _UntypedConstValue
{
internal:
    llvm::Constant* val;

    _UntypedConstValue( llvm::Constant* val );
};

//--//

NS_END
NS_END
NS_END

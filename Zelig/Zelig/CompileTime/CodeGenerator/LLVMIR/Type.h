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

class Type_impl;
ref class _Module;
ref class _Function;
ref class _Value;

/*
* 
 * Type introspection  
 */
public ref class _Type
{
private:
    _Module^ module;

internal: 

    //
    // State
    // 

    Type_impl* _pimpl;

    //
    // Contructors 
    // 
    _Type( _Module^ module, Type_impl* impl );

public:

    //
    // Public methods 
    //

    //
    // Fields 
    // 

    void AddField   ( unsigned offset, _Type^ type, bool forceInline, String^ name );
    void SetupFields( );

    //
    // Type introspection 
    // 

    bool    IsInteger          ( );
    bool    IsFloat            ( );
    bool    IsDouble           ( );
    bool    IsFloatingPoint    ( );
    bool    IsPointer          ( );
    bool    IsStruct           ( );
    bool    IsPointerPointer   ( );
    bool    IsValueType        ( );
    void    SetValueTypeFlag   ( bool val );
    int     GetSizeInBits( );
    int     GetSizeInBitsForStorage( );
    _Type^  GetBTUnderlyingType( );
    bool    __op_Equality      ( _Type^ b );

    //
    // Inspect 
    // 

    void Dump( );
};

//--//

NS_END
NS_END
NS_END

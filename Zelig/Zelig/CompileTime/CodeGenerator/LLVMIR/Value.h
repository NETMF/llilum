#pragma once

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

class Value_impl;
class BasicBlock_impl;
class Type_impl;
ref class _UntypedConstValue;
ref class _Function;
ref class _BasicBlock;
ref class _Module;
ref class _Type;

/* 
 * 
 * 
 */
public ref class _Value
{
private:
    void globalCtor( _Module^ module, String^ name, _Type^ type, bool constant, Value_impl* initializer );

internal:

    // 
    // State
    // 
    _Module^    module;
    Value_impl* _pimpl;

    //
    // Contructors
    // 
    _Value( _Module^ module );
    _Value( _Module^ module, Value_impl* value );
public:

    //
    // Extractors 
    // 
    _Type^  Type( );
    _Module^ Module( );

    //
    // Public methods 
    //

    bool IsInteger              ( );
    bool IsFloat                ( );
    bool IsDouble               ( );
    bool IsFloatingPoint        ( );
    bool IsPointer              ( );
    bool IsPointerPointer       ( );
    bool IsImmediate            ( );
    bool IsZeroedValue          ( );
    bool IsAnUninitializedGlobal( );
    void SetGlobalInitializer   ( _UntypedConstValue^ val );
    void MergeToAndRemove       ( _Value^ targetVal );
    void FlagAsConstant         ( );

    //
    // Introspect 
    // 
    void Dump( );
};

/* 
 * Holds the module and the funciton implementation 
 * 
 */
public ref class _Function : public _Value
{
private:
    BasicBlock_impl *entryBlock;

internal:
    _Module^ _module;

public:

    //
    // Constructor 
    // 

    _Function( _Module^ module, String^ name, _Type^ funcType );

    //
    // Public methods 
    // 

    _BasicBlock^ GetOrInsertBasicBlock  ( String^ blockName );
    _Value^      GetLocalStackValue     ( String^ name, _Type^ type );
    void        DeleteBody              ( );
    void        SetExternalLinkage      ( );
    void        SetInternalLinkage      ( );
};

//--//

NS_END
NS_END
NS_END

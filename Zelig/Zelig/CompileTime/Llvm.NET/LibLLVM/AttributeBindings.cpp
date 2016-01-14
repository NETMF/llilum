#include "AttributeBindings.h"
#include "llvm\IR\Attributes.h"
#include "llvm\IR\LLVMContext.h"
#include "llvm\IR\Value.h"
#include "llvm\IR\Function.h"
#include "llvm\IR\CallSite.h"
#include "llvm\Support\CBindingWrapping.h"
#include <type_traits>

using namespace llvm;

DEFINE_SIMPLE_CONVERSION_FUNCTIONS( AttrBuilder, LLVMAttributeBuilderRef )

// Validate assumptions regarding the attribute classes made in this code
// in order to project the types to other languages
static_assert( sizeof( std::uintptr_t ) == sizeof( Attribute ), "ERROR: Size mismatch on Attribute" );
static_assert( std::is_trivially_copy_constructible<Attribute>::value, "ERROR: Attribute cannot be trivially copy constructed" );
static_assert( std::is_standard_layout<Attribute>::value, "ERROR: Attribute is not a 'C' compatible standard layout" );

LLVMAttributeValue wrap( Attribute attribute )
{
    return *reinterpret_cast< LLVMAttributeValue* >( &attribute );
}

Attribute AsAttribute( LLVMAttributeValue attribute )
{
    return *reinterpret_cast< Attribute* >( &attribute );
}

static_assert( sizeof( std::uintptr_t ) == sizeof( AttributeSet ), "ERROR: Size mismatch on AttributeSet" );
static_assert( std::is_trivially_copy_constructible<AttributeSet>::value, "ERROR: AttributeSet cannot be trivially copy constructed" );
static_assert( std::is_standard_layout<AttributeSet>::value, "ERROR: AttributeSet is not a 'C' compatible standard layout" );

LLVMAttributeSet wrap( AttributeSet attribute )
{
    return *reinterpret_cast< LLVMAttributeSet* >( &attribute );
}

AttributeSet AsAttributeSet( LLVMAttributeSet attribute )
{
    return *reinterpret_cast< AttributeSet* >( &attribute );
}

static_assert( sizeof( ArrayRef<Attribute>::iterator ) == sizeof( uintptr_t ), "ERROR: Size mismatch on ArrayRef<Attribute>::iterator" );

extern "C"
{
    //--- AttributeSet wrappers 
    LLVMAttributeSet LLVMCreateEmptyAttributeSet( )
    {
        return wrap( AttributeSet( ) );
    }

    LLVMAttributeSet LLVMCreateAttributeSetFromKindArray( LLVMContextRef context, unsigned index, LLVMAttrKind* pKinds, uint64_t len )
    {
        auto attributes = ArrayRef<Attribute::AttrKind>( reinterpret_cast< Attribute::AttrKind* >( pKinds ), static_cast< size_t >( len ) );
        return wrap( AttributeSet::get( *unwrap( context ), index, attributes ) );
    }

    LLVMAttributeSet LLVMCreateAttributeSetFromAttributeSetArray( LLVMContextRef context, LLVMAttributeSet* pAttributes, uint64_t len )
    {
        auto attributeSets = ArrayRef<AttributeSet>( reinterpret_cast< AttributeSet* >( pAttributes ), static_cast< size_t >( len ) );
        return wrap( AttributeSet::get( *unwrap( context ), attributeSets ) );
    }

    LLVMAttributeSet LLVMCreateAttributeSetFromBuilder( LLVMContextRef context, unsigned index, LLVMAttributeBuilderRef bldr )
    {
        return wrap( AttributeSet::get( *unwrap( context ), index, *unwrap( bldr ) ) );
    }

    LLVMAttributeSet LLVMAttributeSetAddKind( LLVMAttributeSet attributeSet, LLVMContextRef context, unsigned index, LLVMAttrKind kind )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return wrap( attributes.addAttribute( *unwrap( context ), index, ( Attribute::AttrKind )kind ) );
    }

    LLVMAttributeSet LLVMAttributeSetAddString( LLVMAttributeSet attributeSet, LLVMContextRef context, unsigned index, char const*name )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return wrap( attributes.addAttribute( *unwrap( context ), index, name ) );
    }

    LLVMAttributeSet LLVMAttributeSetAddStringValue( LLVMAttributeSet attributeSet, LLVMContextRef context, unsigned index, char const* name, char const* value )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return wrap( attributes.addAttribute( *unwrap( context ), index, name, value ) );
    }

    LLVMAttributeSet LLVMAttributeSetAddAttributes( LLVMAttributeSet attributeSet, LLVMContextRef context, unsigned index, LLVMAttributeSet otherAttributeSet )
    {
        auto attributes = AsAttributeSet( attributeSet );
        auto otherAttributes = AsAttributeSet( otherAttributeSet );
        return wrap( attributes.addAttributes( *unwrap( context ), index, otherAttributes ) );
    }

    LLVMAttributeSet LLVMAttributeSetRemoveAttributeKind( LLVMAttributeSet attributeSet, unsigned index, LLVMAttrKind kind )
    {
        auto attributes = AsAttributeSet( attributeSet );
        if( attributes.isEmpty( ) )
            return attributeSet;

        return wrap( attributes.removeAttribute( attributes.getContext(), index, (Attribute::AttrKind )kind ) );
    }

    LLVMAttributeSet LLVMAttributeSetRemoveAttributeSet( LLVMAttributeSet attributeSet, unsigned index, LLVMAttributeSet attributesToRemove )
    {
        auto attributes = AsAttributeSet( attributeSet );
        auto attributeSetToRemove = AsAttributeSet( attributesToRemove );
        if( attributes.isEmpty( ) || attributeSetToRemove.isEmpty( ) )
            return attributeSet;

        assert( &attributes.getContext( ) == &attributeSetToRemove.getContext( ) && "Mismatched contexts on AttributeSet" );
        return wrap( attributes.removeAttributes( attributes.getContext(), index, attributeSetToRemove ) );
    }

    LLVMAttributeSet LLVMAttributeSetRemoveAttributeBuilder( LLVMAttributeSet attributeSet, LLVMContextRef context, unsigned index, LLVMAttributeBuilderRef bldr )
    {
        LLVMContext* pContext = unwrap( context );
        auto attributes = AsAttributeSet( attributeSet );
        AttrBuilder& attributeBuilder = *unwrap( bldr );

        assert( ( attributes.isEmpty() || &attributes.getContext( ) == pContext ) && "Mismatched contexts on AttributeSet" );

        return wrap( attributes.removeAttributes( *pContext, index, attributeBuilder ) );
    }

    LLVMContextRef LLVMAttributeSetGetContext( LLVMAttributeSet attributeSet )
    {
        return wrap( &AsAttributeSet( attributeSet ).getContext( ) );
    }

    LLVMAttributeSet LLVMAttributeGetAttributes( LLVMAttributeSet attributeSet, unsigned index )
    {
        auto attributes = AsAttributeSet( attributeSet );
        switch( index )
        {
        case AttributeSet::AttrIndex::FunctionIndex:
            return wrap( attributes.getFnAttributes( ) );

        case AttributeSet::AttrIndex::ReturnIndex:
            return wrap( attributes.getRetAttributes( ) );

        default:
            return wrap( attributes.getParamAttributes( index ) );
        }
    }

    LLVMBool LLVMAttributeSetHasAttributeKind( LLVMAttributeSet attributeSet, unsigned index, LLVMAttrKind kind )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return attributes.hasAttribute( index, ( Attribute::AttrKind )kind );
    }

    LLVMBool LLVMAttributeSetHasStringAttribute( LLVMAttributeSet attributeSet, unsigned index, char const* name )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return attributes.hasAttribute( index, name );
    }

    LLVMBool LLVMAttributeSetHasAttributes( LLVMAttributeSet attributeSet, unsigned index )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return attributes.hasAttributes( index );
    }

    LLVMBool LLVMAttributeSetHasAttributeSomewhere( LLVMAttributeSet attributeSet, LLVMAttrKind kind )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return attributes.hasAttrSomewhere( ( Attribute::AttrKind )kind );
    }

    LLVMAttributeValue LLVMAttributeSetGetAttributeByKind( LLVMAttributeSet attributeSet, unsigned index, LLVMAttrKind kind )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return wrap( attributes.getAttribute( index, ( Attribute::AttrKind )kind ) );
    }

    LLVMAttributeValue LLVMAttributeSetGetAttributeByName( LLVMAttributeSet attributeSet, unsigned index, char const* name )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return wrap( attributes.getAttribute( index, name ) );
    }

    char const* LLVMAttributeSetToString( LLVMAttributeSet attributeSet, unsigned index, LLVMBool inGroup )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return LLVMCreateMessage( attributes.getAsString( index, inGroup != 0 ).c_str() );
    }

    unsigned LLVMAttributeSetGetNumSlots( LLVMAttributeSet attributeSet )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return attributes.getNumSlots( );
    }

    LLVMAttributeSet LLVMAttributeSetGetSlotAttributes( LLVMAttributeSet attributeSet, unsigned slot )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return wrap( attributes.getSlotAttributes( slot ) );
    }

    unsigned LLVMAttributeSetGetSlotIndex( LLVMAttributeSet attributeSet, unsigned slot )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return attributes.getSlotIndex( slot );
    }

    LLVMAttributeSet LLVMGetFunctionAttributeSet( LLVMValueRef /*Function*/ function )
    {
        auto func = unwrap<Function>( function );
        return wrap( func->getAttributes() );
    }

    void LLVMSetFunctionAttributeSet( LLVMValueRef /*Function*/ function, LLVMAttributeSet attributeSet )
    {
        auto func = unwrap<Function>( function );
        func->setAttributes( AsAttributeSet( attributeSet ) );
    }

    LLVMAttributeSet LLVMGetCallSiteAttributeSet( LLVMValueRef /*Instruction*/ instruction )
    {
        CallSite call = CallSite( unwrap<Instruction>( instruction ) );

       return wrap( call.getAttributes( ) );
    }

    void LLVMSetCallSiteAttributeSet( LLVMValueRef /*Instruction*/ instruction, LLVMAttributeSet attributeSet )
    {
        CallSite call = CallSite( unwrap<Instruction>( instruction ) );
        call.setAttributes( AsAttributeSet( attributeSet ) );
    }

    uintptr_t LLVMAttributeSetGetIteratorStartToken( LLVMAttributeSet attributeSet, unsigned slot )
    {
        auto attributes = AsAttributeSet( attributeSet );
        return reinterpret_cast<uintptr_t>( attributes.begin( slot ) );
    }

    LLVMAttributeValue LLVMAttributeSetIteratorGetNext( LLVMAttributeSet attributeSet, unsigned slot, uintptr_t* pToken )
    {
        auto attributes = AsAttributeSet( attributeSet );
        auto it = *reinterpret_cast< Attribute**>( pToken );
        if( it >= attributes.end( slot ) || it < attributes.begin( slot ) )
            return wrap( Attribute( ) );
        
        auto retVal = wrap( *it );
        *pToken = reinterpret_cast<uintptr_t>( ++it );
        return retVal;
    }

    //--- Attribute Wrappers

    LLVMBool LLVMIsEnumAttribute( LLVMAttributeValue attribute )
    {
        return AsAttribute( attribute ).isEnumAttribute( );
    }

    LLVMBool LLVMIsIntAttribute( LLVMAttributeValue attribute )
    {
        return AsAttribute( attribute ).isIntAttribute( );
    }

    LLVMBool LLVMIsStringAttribute( LLVMAttributeValue attribute )
    {
        return AsAttribute( attribute ).isStringAttribute( );
    }

    LLVMBool LLVMHasAttributeKind( LLVMAttributeValue attribute, LLVMAttrKind kind )
    {
        return AsAttribute( attribute ).hasAttribute( ( llvm::Attribute::AttrKind )kind );
    }

    LLVMBool LLVMHasAttributeString( LLVMAttributeValue attribute, char const* name )
    {
        return AsAttribute( attribute ).hasAttribute( name );
    }

    LLVMAttrKind LLVMGetAttributeKind( LLVMAttributeValue attribute )
    {
        return ( LLVMAttrKind )AsAttribute( attribute ).getKindAsEnum( );
    }

    uint64_t LLVMGetAttributeValue( LLVMAttributeValue attribute )
    {
        return AsAttribute( attribute ).getValueAsInt( );
    }

    char const* LLVMGetAttributeName( LLVMAttributeValue attribute )
    {
        return AsAttribute( attribute ).getKindAsString( ).data();
    }

    char const* LLVMGetAttributeStringValue( LLVMAttributeValue attribute )
    {
        return AsAttribute( attribute ).getValueAsString( ).data();
    }

    char const* LLVMAttributeToString( LLVMAttributeValue attribute )
    {
        return LLVMCreateMessage( AsAttribute( attribute ).getAsString( ).c_str( ) );
    }

    LLVMAttributeValue LLVMCreateAttribute( LLVMContextRef ctx, LLVMAttrKind kind, uint64_t value )
    {
        return wrap( llvm::Attribute::get( *unwrap( ctx ), ( llvm::Attribute::AttrKind )kind, value ) );
    }

    LLVMAttributeValue LVMCreateTargetDependentAttribute( LLVMContextRef ctx, char const* name, char const* value )
    {
        return wrap( llvm::Attribute::get( *unwrap( ctx ), name, value ) );
    }

    //--- AttrBuilder Wrappers

    LLVMAttributeBuilderRef LLVMCreateAttributeBuilder( )
    {
        return wrap( new AttrBuilder( ) );
    }

    LLVMAttributeBuilderRef LLVMCreateAttributeBuilder2( LLVMAttributeValue value )
    {
        return wrap( new AttrBuilder( AsAttribute( value ) ) );
    }

    LLVMAttributeBuilderRef LLVMCreateAttributeBuilder3( LLVMAttributeSet attributeSet, unsigned index )
    {
        return wrap( new AttrBuilder( AsAttributeSet( attributeSet ), index ) );
    }

    void LLVMAttributeBuilderDispose( LLVMAttributeBuilderRef bldr )
    {
        delete unwrap( bldr );
    }

    void LLVMAttributeBuilderClear( LLVMAttributeBuilderRef bldr )
    {
        unwrap( bldr )->clear( );
    }

    void LLVMAttributeBuilderAddEnum( LLVMAttributeBuilderRef bldr, LLVMAttrKind kind )
    {
        unwrap( bldr )->addAttribute( ( Attribute::AttrKind )kind );
    }

    void LLVMAttributeBuilderAddAttribute( LLVMAttributeBuilderRef bldr, LLVMAttributeValue value )
    {
        unwrap( bldr )->addAttribute( AsAttribute( value ) );
    }

    void LLVMAttributeBuilderAddStringAttribute( LLVMAttributeBuilderRef bldr, char const* name, char const* value )
    {
        unwrap( bldr )->addAttribute( name, value );
    }

    void LLVMAttributeBuilderRemoveEnum( LLVMAttributeBuilderRef bldr, LLVMAttrKind kind )
    {
        unwrap( bldr )->removeAttribute( ( Attribute::AttrKind )kind );
    }

    void LLVMAttributeBuilderRemoveAttributes( LLVMAttributeBuilderRef bldr, LLVMAttributeSet attributeSet, unsigned index )
    {
        unwrap( bldr )->removeAttributes( AsAttributeSet( attributeSet ), index );
    }

    void LLVMAttributeBuilderRemoveAttribute( LLVMAttributeBuilderRef bldr, char const* name )
    {
        unwrap( bldr )->removeAttribute( name );
    }

    void LLVMAttributeBuilderRemoveBldr( LLVMAttributeBuilderRef bldr, LLVMAttributeBuilderRef other )
    {
        unwrap( bldr )->remove( *unwrap( other ) );
    }

    void LLVMAttributeBuilderMerge( LLVMAttributeBuilderRef bldr, LLVMAttributeBuilderRef other )
    {
        unwrap( bldr )->merge( *unwrap( other ) );
    }

    LLVMBool LLVMAttributeBuilderOverlaps( LLVMAttributeBuilderRef bldr, LLVMAttributeBuilderRef other )
    {
        return unwrap( bldr )->overlaps( *unwrap( other ) );
    }

    LLVMBool LLVMAttributeBuilderContainsEnum( LLVMAttributeBuilderRef bldr, LLVMAttrKind kind )
    {
        return unwrap( bldr )->contains( ( Attribute::AttrKind )kind );
    }

    LLVMBool LLVMAttributeBuilderContainsName( LLVMAttributeBuilderRef bldr, char const* name )
    {
        return unwrap( bldr )->contains( name );
    }

    LLVMBool LLVMAttributeBuilderHasAnyAttributes( LLVMAttributeBuilderRef bldr )
    {
        return unwrap( bldr )->hasAttributes( );
    }

    LLVMBool LLVMAttributeBuilderHasAttributes( LLVMAttributeBuilderRef bldr, LLVMAttributeSet attributeset, unsigned index )
    {
        return unwrap( bldr )->hasAttributes( AsAttributeSet( attributeset ), index );
    }

    LLVMBool LLVMAttributeBuilderHasTargetIndependentAttrs( LLVMAttributeBuilderRef bldr )
    {
        return !unwrap( bldr )->empty( );
    }

    LLVMBool LLVMAttributeBuilderHasTargetDependentAttrs( LLVMAttributeBuilderRef bldr )
    {
        return !unwrap( bldr )->td_empty( );
    }
}

#include "llvmheaders.h"
#include "Value.h"
#include <unordered_map>
#include <unordered_set>
#include <vector>

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

class Module_impl;

//--//

/* 
*   Type field representation 
 * 
 */
struct Type_field
{
    Type_field( string name, Type_impl* type, unsigned int offset, bool forceInline ) :
        name( name ),
        type( type ),
        offset( offset ),
        forceInline( forceInline ),
        finalIdx( 0 )
    {

    }

    string          name;
    Type_impl*      type;
    unsigned int    offset;
    bool            forceInline;
    size_t          finalIdx;
};

//--//

/*
 *
 *
 */
class Type_impl
{
private:

    //
    // LLVM types store
    //
    static std::map<std::string, Type_impl* > typeImpls;
    static std::map<llvm::Type* , Type_impl* > typeImpls_reverseLookupForLLVMTypes;

    //
    //  State
    //

    llvm::Type*     llvmType;
    Module_impl*    owner;
    bool            isValueType;
    bool            hasHeader;
    bool			isBoxed; //Review: Remove and check on underlyingBoxedPointer.
    std::string     name;
    int             sizeInBits;

    void Internal_AddTypeToStruct( int& idx, vector < llvm::Type* > &llvmFields, size_t& i );

    //
    // Contructors
    // 

    Type_impl( Module_impl* owner, std::string name, int sizeInBits, llvm::Type* ty );
    Type_impl( Module_impl* owner, std::string name, int sizeInBits );
    
    void privateInit( Module_impl* owner, std::string name, int sizeInBits );

public:

    //
    // Properties 
    //
    std::vector<Type_field* >   fields;
    std::vector<Type_impl* >    functionArgs;
    Type_impl*					underlyingBoxedType;
    Type_impl*					underlyingPointerType;

    //
    // Public methods 
    // 

    static Type_impl* GetOrInsertTypeImpl   ( Module_impl* owner, std::string name, int sizeInBits, llvm::Type* ty );
    static Type_impl* GetOrInsertTypeImpl   ( Module_impl* owner, std::string name, int sizeInBits );
    static Type_impl* GetTypeImpl           ( std::string name );
    static Type_impl* GetTypeImpl           ( llvm::Type* ty );

    llvm::Type* GetLLVMObjectForStorage ( );
    llvm::Type* GetLLVMObject           ( );
    unsigned    GetSizeInBits              ( );
    unsigned    GetSizeInBitsForStorage    ( );

    void AddField   ( unsigned offset, Type_impl* type, bool forceInline, string name );
    void SetupFields( );

    void SetHasHeaderFlag   ( bool val );
    void SetValueTypeFlag   ( bool val );
    void SetBoxedFlag       ( bool val );

    bool IsBoxed( );
    bool IsValueType( );

    std::string GetName ( );
    void Dump           ( );
};

//--//

NS_END
NS_END
NS_END

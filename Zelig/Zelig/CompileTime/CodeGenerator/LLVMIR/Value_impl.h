#include "llvmheaders.h"
#include "Value.h"

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

/*
 * Links a type to an LLVM type 
 *
 */
class Value_impl
{
    friend ref class _BasicBlock;
    friend ref class _Value;

private:

    //
    // State 
    // 

    llvm::Value*    _pValue;
    Type_impl*      typeImpl;
    bool            immediate;

public:
    //
    // Contructors
    // 
    Value_impl( Type_impl* typeImpl, llvm::Value* pValue, bool immediate );

    //
    // Public methods 
    // 
	
    llvm::Value* GetLLVMObject( );
    void         Dump( );
};

//--//

NS_END
NS_END
NS_END

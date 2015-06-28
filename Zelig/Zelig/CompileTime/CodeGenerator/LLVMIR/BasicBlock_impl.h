#include "llvmheaders.h"
#include "BasicBlock.h"

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

/*
 * Holds the references to the IR builder and the basic block
 *
 */
class BasicBlock_impl
{
    friend ref class _BasicBlock;

private:

    //
    // State
    //
    llvm::BasicBlock*   _pbb;
    IRBuilder<>*        builder;

public:

    //
    // Contructors
    //

    BasicBlock_impl( llvm::BasicBlock* pbb );

    //
    // Public methods
    //

    llvm::BasicBlock* GetLLVMObject( );
};

//--//

NS_END
NS_END
NS_END

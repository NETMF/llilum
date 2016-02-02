using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class UserOp1 : Instruction
    {
        internal UserOp1( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

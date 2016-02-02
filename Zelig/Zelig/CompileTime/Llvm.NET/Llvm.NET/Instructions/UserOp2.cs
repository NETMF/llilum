using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class UserOp2 : Instruction
    {
        internal UserOp2( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

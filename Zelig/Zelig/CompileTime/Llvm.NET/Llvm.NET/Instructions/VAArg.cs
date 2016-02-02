using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class VaArg : UnaryInstruction
    {
        internal VaArg( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

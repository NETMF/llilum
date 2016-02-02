using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Cast
        : UnaryInstruction
    {
        internal Cast( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Alloca
        : UnaryInstruction
    {
        internal Alloca( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

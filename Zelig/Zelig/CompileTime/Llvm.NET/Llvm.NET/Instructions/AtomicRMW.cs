using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class AtomicRMW
        : Instruction
    {
        internal AtomicRMW( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

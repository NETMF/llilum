using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class CatchSwitch
        : Instruction
    {
        internal CatchSwitch( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

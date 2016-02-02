using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class CatchReturn
        : Terminator
    {
        internal CatchReturn( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

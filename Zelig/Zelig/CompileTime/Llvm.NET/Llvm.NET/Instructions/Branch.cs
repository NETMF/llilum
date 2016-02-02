using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Branch
        : Terminator
    {
        internal Branch( LLVMValueRef valueRef)
            : base( valueRef )
        {
        }
    }
}

using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class CleanupReturn
        : Terminator
    {
        internal CleanupReturn( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

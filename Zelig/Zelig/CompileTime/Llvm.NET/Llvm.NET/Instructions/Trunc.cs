using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class Trunc
        : Cast
    {
        internal Trunc( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

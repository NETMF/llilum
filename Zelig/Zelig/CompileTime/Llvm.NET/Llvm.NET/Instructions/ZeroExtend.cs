
using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    public class ZeroExtend
        : Cast
    {
        internal ZeroExtend( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

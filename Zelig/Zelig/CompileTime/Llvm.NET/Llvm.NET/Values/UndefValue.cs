using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    public class UndefValue : Constant
    {
        internal UndefValue( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

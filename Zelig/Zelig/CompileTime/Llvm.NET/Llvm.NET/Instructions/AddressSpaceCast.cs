using Llvm.NET.Native;

namespace Llvm.NET.Instructions
{
    class AddressSpaceCast : Cast
    {
        internal AddressSpaceCast( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

namespace Llvm.NET.Instructions
{
    public class MemIntrinsic
        : Intrinsic
    {
        internal MemIntrinsic( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAMemIntrinsic ) )
        {
        }
    }
}

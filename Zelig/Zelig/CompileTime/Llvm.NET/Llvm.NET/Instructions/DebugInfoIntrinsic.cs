namespace Llvm.NET.Instructions
{
    public class DebugInfoIntrinsic
        : Intrinsic
    {
        internal DebugInfoIntrinsic( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsADbgInfoIntrinsic ) )
        { 
        }
    }
}

namespace Llvm.NET.Instructions
{
    class FPTrunc : Cast
    {
        internal FPTrunc( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

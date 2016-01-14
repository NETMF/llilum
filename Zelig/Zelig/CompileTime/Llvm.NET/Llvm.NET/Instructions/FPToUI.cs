namespace Llvm.NET.Instructions
{
    class FPToUI : Cast
    {
        internal FPToUI( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

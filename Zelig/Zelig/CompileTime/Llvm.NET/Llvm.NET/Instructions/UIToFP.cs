namespace Llvm.NET.Instructions
{
    class UIToFP : Cast
    {
        internal UIToFP( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

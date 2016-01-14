namespace Llvm.NET.Instructions
{
    class AddressSpaceCast : Cast
    {
        internal AddressSpaceCast( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

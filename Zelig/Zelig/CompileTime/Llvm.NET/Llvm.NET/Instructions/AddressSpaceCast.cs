namespace Llvm.NET.Instructions
{
    class AddressSpaceCast : Cast
    {
        internal AddressSpaceCast( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal AddressSpaceCast( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

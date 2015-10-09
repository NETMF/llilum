namespace Llvm.NET.Instructions
{
    class VaArg : UnaryInstruction
    {
        internal VaArg( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal VaArg( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

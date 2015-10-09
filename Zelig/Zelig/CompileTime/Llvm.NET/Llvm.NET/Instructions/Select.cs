namespace Llvm.NET.Instructions
{
    class Select : Instruction
    {
        internal Select( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Select( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABranchInst ) )
        {
        }
    }
}

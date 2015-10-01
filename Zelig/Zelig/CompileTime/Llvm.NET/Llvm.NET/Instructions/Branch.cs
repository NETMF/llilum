namespace Llvm.NET.Instructions
{
    public class Branch
        : Terminator
    {
        internal Branch( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Branch( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsABranchInst ) )
        {
        }
    }
}

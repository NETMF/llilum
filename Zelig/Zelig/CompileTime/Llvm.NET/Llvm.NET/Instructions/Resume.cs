namespace Llvm.NET.Instructions
{
    public class Resume
        : Terminator
    {
        internal Resume( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Resume( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAExtractElementInst ) )
        {
        }
    }
}

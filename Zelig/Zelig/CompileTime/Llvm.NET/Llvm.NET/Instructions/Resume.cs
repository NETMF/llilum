namespace Llvm.NET.Instructions
{
    public class ResumeInstruction
        : Terminator
    {
        internal ResumeInstruction( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractElementInst ) )
        {
        }
    }
}

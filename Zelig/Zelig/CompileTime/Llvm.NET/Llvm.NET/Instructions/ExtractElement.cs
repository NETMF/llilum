namespace Llvm.NET.Instructions
{
    public class ExtractElement
        : Instruction
    {
        internal ExtractElement( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractElementInst ) )
        {
        }
    }
}

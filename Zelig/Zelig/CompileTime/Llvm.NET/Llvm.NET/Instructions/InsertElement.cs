namespace Llvm.NET.Instructions
{
    public class InsertElement
        : Instruction
    {
        internal InsertElement( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractElementInst ) )
        {
        }
    }
}

namespace Llvm.NET.Instructions
{
    public class Fence
        : Instruction
    {
        internal Fence( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractElementInst ) )
        {
        }
    }
}

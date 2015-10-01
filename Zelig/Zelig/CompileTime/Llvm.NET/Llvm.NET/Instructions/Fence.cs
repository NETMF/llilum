namespace Llvm.NET.Instructions
{
    public class Fence
        : Instruction
    {
        internal Fence( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Fence( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAExtractElementInst ) )
        {
        }
    }
}

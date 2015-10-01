namespace Llvm.NET.Instructions
{
    public class ShuffleVector
        : Instruction
    {
        internal ShuffleVector( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ShuffleVector( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAExtractElementInst ) )
        {
        }
    }
}

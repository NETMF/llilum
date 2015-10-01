namespace Llvm.NET.Instructions
{
    public class AtomicRMW
        : Instruction
    {
        internal AtomicRMW( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal AtomicRMW( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAExtractElementInst ) )
        {
        }
    }
}

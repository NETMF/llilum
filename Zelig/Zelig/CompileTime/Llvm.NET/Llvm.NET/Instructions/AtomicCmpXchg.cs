namespace Llvm.NET.Instructions
{
    public class AtomicCmpXchg
        : Instruction
    {
        internal AtomicCmpXchg( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal AtomicCmpXchg( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAExtractElementInst ) )
        {
        }
    }
}

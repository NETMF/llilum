namespace Llvm.NET.Instructions
{
    public class GetElementPtr
        : Instruction
    {
        internal GetElementPtr( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal GetElementPtr( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAGetElementPtrInst ) )
        {
        }
    }
}

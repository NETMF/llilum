namespace Llvm.NET.Instructions
{
    public class Call
        : Instruction
    {
        internal Call( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Call( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsACallInst ) )
        {
        }
    }
}

namespace Llvm.NET.Instructions
{
    public class Alloca
        : UnaryInstruction
    {
        internal Alloca( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Alloca( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAAllocaInst ) )
        {
        }
    }
}

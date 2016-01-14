namespace Llvm.NET.Instructions
{
    public class Alloca
        : UnaryInstruction
    {
        internal Alloca( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAAllocaInst ) )
        {
        }
    }
}

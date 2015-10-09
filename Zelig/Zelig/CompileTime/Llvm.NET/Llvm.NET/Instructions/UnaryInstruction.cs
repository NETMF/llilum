namespace Llvm.NET.Instructions
{
    public class UnaryInstruction
        : Instruction
    {
        internal UnaryInstruction( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, NativeMethods.IsAUnaryInstruction ) )
        {
        }
    }
}

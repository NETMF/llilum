namespace Llvm.NET.Instructions
{
    public class BinaryOperator
        : Instruction
    {
        internal BinaryOperator( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal BinaryOperator( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsABinaryOperator ) )
        {
        }
    }
}

namespace Llvm.NET.Instructions
{
    public class UnaryInstruction
        : Instruction
    {
        internal UnaryInstruction( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAUnaryInstruction ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static UnaryInstruction FromHandle( LLVMValueRef valueRef )
        {
            return (UnaryInstruction)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new UnaryInstruction( h ) );
        }
    }
}

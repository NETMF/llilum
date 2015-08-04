namespace Llvm.NET.Instructions
{
    public class Terminator
        : Instruction
    {
        internal Terminator( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsATerminatorInst ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static Terminator FromHandle( LLVMValueRef valueRef )
        {
            return (Terminator)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new Terminator( h ) );
        }
    }
}

namespace Llvm.NET.Instructions
{
    public class Cast
        : UnaryInstruction
    {
        internal Cast( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsACastInst ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static Cast FromHandle( LLVMValueRef valueRef )
        {
            return (Cast)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new Cast( h ) );
        }
    }
}

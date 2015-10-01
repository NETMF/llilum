namespace Llvm.NET.Instructions
{
    public class MemMove
        : MemIntrinsic
    {
        internal MemMove( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAMemMoveInst ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static MemMove FromHandle( LLVMValueRef valueRef )
        {
            return (MemMove)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new MemMove( h ) );
        }
    }
}

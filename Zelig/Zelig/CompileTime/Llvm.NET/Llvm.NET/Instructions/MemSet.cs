namespace Llvm.NET.Instructions
{
    public class MemSet
        : MemIntrinsic
    {
        internal MemSet( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAMemSetInst ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static MemSet FromHandle( LLVMValueRef valueRef )
        {
            return (MemSet)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new MemSet( h ) );
        }
    }
}

namespace Llvm.NET.Instructions
{
    public class MemIntrinsic
        : Intrinsic
    {
        internal MemIntrinsic( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAMemIntrinsic ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static MemIntrinsic FromHandle( LLVMValueRef valueRef )
        {
            return (MemIntrinsic)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new MemIntrinsic( h ) );
        }
    }
}

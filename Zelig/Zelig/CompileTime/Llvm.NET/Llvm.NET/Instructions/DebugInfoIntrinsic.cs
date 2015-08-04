namespace Llvm.NET.Instructions
{
    public class DebugInfoIntrinsic
        : Intrinsic
    {
        internal DebugInfoIntrinsic( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsADbgInfoIntrinsic ) )
        { 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static DebugInfoIntrinsic FromHandle( LLVMValueRef valueRef )
        {
            return (DebugInfoIntrinsic)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new DebugInfoIntrinsic( h ) );
        }
    }
}

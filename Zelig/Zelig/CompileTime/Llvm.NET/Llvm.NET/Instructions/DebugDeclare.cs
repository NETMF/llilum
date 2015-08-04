namespace Llvm.NET.Instructions
{
    public class DebugDeclare
        : DebugInfoIntrinsic
    {
        internal DebugDeclare( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsADbgDeclareInst ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static DebugDeclare FromHandle( LLVMValueRef valueRef )
        {
            return (DebugDeclare)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new DebugDeclare( h ) );
        }
    }
}

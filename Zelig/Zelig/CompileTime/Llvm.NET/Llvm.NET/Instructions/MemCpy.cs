namespace Llvm.NET.Instructions
{
    public class MemCpy
        : MemIntrinsic
    {
        internal MemCpy( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAMemCpyInst ) )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static MemCpy FromHandle( LLVMValueRef valueRef )
        {
            return (MemCpy)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new MemCpy( h ) );
        }
    }
}

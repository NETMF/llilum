namespace Llvm.NET
{
    public class ConstantPointerNull : Constant
    {
        public static ConstantPointerNull From( TypeRef type )
        {
            return FromHandle( LLVMNative.ConstPointerNull( type.TypeHandle ) );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static ConstantPointerNull FromHandle( LLVMValueRef valueRef )
        {
            return ( ConstantPointerNull )Context.CurrentContext.GetValueFor( valueRef, ( h ) => new ConstantPointerNull( h ) );
        }

        internal ConstantPointerNull( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAConstantPointerNull ) )
        {
        }
    }
}

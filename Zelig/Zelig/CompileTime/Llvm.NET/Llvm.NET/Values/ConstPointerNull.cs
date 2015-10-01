using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    public class ConstantPointerNull : Constant
    {
        public static ConstantPointerNull From( TypeRef type )
        {
            return FromHandle( LLVMNative.ConstPointerNull( type.TypeHandle ) );
        }

        internal new static ConstantPointerNull FromHandle( LLVMValueRef valueRef )
        {
            return ( ConstantPointerNull )Context.CurrentContext.GetValueFor( valueRef, ( h ) => new ConstantPointerNull( h ) );
        }

        internal ConstantPointerNull( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantPointerNull( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAConstantPointerNull ) )
        {
        }
    }
}

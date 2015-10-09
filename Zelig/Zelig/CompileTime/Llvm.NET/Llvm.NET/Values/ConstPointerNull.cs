using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    public class ConstantPointerNull : Constant
    {
        public static ConstantPointerNull From( ITypeRef type )
        {
            return FromHandle<ConstantPointerNull>( NativeMethods.ConstPointerNull( type.GetTypeRef() ) );
        }

        internal ConstantPointerNull( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal ConstantPointerNull( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAConstantPointerNull ) )
        {
        }
    }
}

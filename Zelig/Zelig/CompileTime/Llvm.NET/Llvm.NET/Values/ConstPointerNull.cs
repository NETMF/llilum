using Llvm.NET.Native;
using Llvm.NET.Types;

namespace Llvm.NET.Values
{
    public class ConstantPointerNull
        : Constant
    {
        /// <summary>Creates a constant null pointer to a given type</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ConstantPointerNull From( ITypeRef type )
        {
            return FromHandle<ConstantPointerNull>( NativeMethods.ConstPointerNull( type.GetTypeRef() ) );
        }

        internal ConstantPointerNull( LLVMValueRef valueRef )
            : base( valueRef )
        {
        }
    }
}

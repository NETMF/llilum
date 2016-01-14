using System.Diagnostics;
using Llvm.NET.Types;

namespace Llvm.NET.Instructions
{
    public class MemCpy
        : MemIntrinsic
    {
        internal MemCpy( LLVMValueRef handle )
            : base( handle )
        {
        }

        internal static string GetIntrinsicNameForArgs( IPointerType dst, IPointerType src, ITypeRef len )
        {
            Debug.Assert( dst != null && dst.ElementType.IsInteger && dst.ElementType.IntegerBitWidth > 0 );
            Debug.Assert( src != null && src.ElementType.IsInteger && src.ElementType.IntegerBitWidth > 0 );
            Debug.Assert( len.IsInteger && len.IntegerBitWidth > 0 );
            return $"llvm.memcpy.p{dst.AddressSpace}i{dst.ElementType.IntegerBitWidth}.p{src.AddressSpace}i{src.ElementType.IntegerBitWidth}.i{len.IntegerBitWidth}";
        }
    }
}

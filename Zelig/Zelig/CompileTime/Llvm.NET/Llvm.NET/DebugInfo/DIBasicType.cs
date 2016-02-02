using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Debug information for a basic type</summary>
    /// <seealso cref="DebugInfoBuilder.CreateBasicType(string, ulong, ulong, DiTypeKind)"/>
    public class DIBasicType : DIType
    {
        internal DIBasicType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

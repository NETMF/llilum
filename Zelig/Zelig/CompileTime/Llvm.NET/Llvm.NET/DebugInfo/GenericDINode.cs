using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    public class GenericDINode : DINode
    {
        internal GenericDINode( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

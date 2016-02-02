using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    public class DIModule : DIScope
    {
        internal DIModule( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

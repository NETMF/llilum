using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    public class DITemplateParameter : DINode
    {
        internal DITemplateParameter( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

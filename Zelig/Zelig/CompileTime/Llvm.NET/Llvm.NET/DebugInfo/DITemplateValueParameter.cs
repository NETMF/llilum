using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    public class DITemplateValueParameter : DITemplateParameter
    {
        internal DITemplateValueParameter( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    public class DITemplateTypeParameter : DITemplateParameter
    {
        internal DITemplateTypeParameter( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

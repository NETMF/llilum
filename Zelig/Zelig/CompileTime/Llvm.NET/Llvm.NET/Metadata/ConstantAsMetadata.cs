using Llvm.NET.Native;

namespace Llvm.NET
{
    public class ConstantAsMetadata
        : ValueAsMetadata
    {
        internal ConstantAsMetadata( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}
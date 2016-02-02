using Llvm.NET.Native;

namespace Llvm.NET
{
    public class LocalAsMetadata
        : ValueAsMetadata
    {
        internal LocalAsMetadata( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}
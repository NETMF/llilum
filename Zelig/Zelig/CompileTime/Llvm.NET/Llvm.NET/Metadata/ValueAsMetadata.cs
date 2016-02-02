using Llvm.NET.Native;

namespace Llvm.NET
{
    public class ValueAsMetadata
        : LlvmMetadata
    {
        internal ValueAsMetadata( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}
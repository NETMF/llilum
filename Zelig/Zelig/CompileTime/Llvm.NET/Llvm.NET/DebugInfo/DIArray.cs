namespace Llvm.NET.DebugInfo
{
    /// <summary>Array of see <a href="Descriptor"/> nodes for use with see <a href="DebugInfoBuilder"/> methods</summary>
    public class DIArray
    {
        internal DIArray( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }
}

namespace Llvm.NET.DebugInfo
{
    /// <summary>Array of see <a href="Type"/> nodes for use with see <a href="DebugInfoBuilder"/> methods</summary>
    public class DITypeArray
    {
        internal DITypeArray( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }
}

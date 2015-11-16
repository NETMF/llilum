namespace Llvm.NET.DebugInfo
{
    /// <summary>Array of <see cref="DINode"/> debug information nodes for use with see <a href="DebugInfoBuilder"/> methods</summary>
    /// <seealso cref="DebugInfoBuilder.GetOrCreateArray(System.Collections.Generic.IEnumerable{DINode})"/>
    public class DIArray
    {
        internal DIArray( LLVMMetadataRef handle )
        {
            MetadataHandle = handle;
        }

        internal LLVMMetadataRef MetadataHandle { get; }
    }
}

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diImportedEntity"/></summary>
    public class DIImportedEntity : DINode
    {
        internal DIImportedEntity( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

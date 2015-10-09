namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dinamespace"/></summary>
    public class DINamespace : DIScope
    {
        internal DINamespace( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

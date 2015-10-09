namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#difile"/></summary>
    public class DIFile : DIScope
    {
        internal DIFile( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

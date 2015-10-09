namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dicompileunit"/></summary>
    public class DICompileUnit : DIScope
    {
        internal DICompileUnit( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

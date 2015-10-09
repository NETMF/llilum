namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dicompositetype"/></summary>
    public class DICompositeType : DIType
    {
        internal DICompositeType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

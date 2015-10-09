namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dibasictype"/></summary>
    public class DIBasicType : DIType
    {
        internal DIBasicType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubrange"/></summary>
    public class DISubrange : DINode
    {
        internal DISubrange( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

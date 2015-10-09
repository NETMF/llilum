namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dienumerator"/></summary>
    public class DIEnumerator : DINode
    {
        internal DIEnumerator( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

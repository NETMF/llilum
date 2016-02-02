using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diexpression"/></summary>
    public class DIExpression : MDNode
    {
        internal DIExpression( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

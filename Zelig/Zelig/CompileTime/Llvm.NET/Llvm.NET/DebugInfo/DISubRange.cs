using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubrange"/></summary>
    public class DISubRange : DINode
    {
        internal DISubRange( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

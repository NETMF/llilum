using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dilexicalblock"/></summary>
    public class DILexicalBlock : DILexicalBlockBase
    {
        internal DILexicalBlock( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

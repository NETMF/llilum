using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dilexicalblockfile"/></summary>
    public class DILexicalBlockFile : DILexicalBlockBase
    {
        internal DILexicalBlockFile( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

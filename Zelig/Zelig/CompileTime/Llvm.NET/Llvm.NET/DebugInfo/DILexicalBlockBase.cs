using Llvm.NET.Native;

namespace Llvm.NET.DebugInfo
{
    public class DILexicalBlockBase : DILocalScope
    {
        internal DILexicalBlockBase( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

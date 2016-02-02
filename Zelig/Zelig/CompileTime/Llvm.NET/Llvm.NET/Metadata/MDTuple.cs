using Llvm.NET.Native;

namespace Llvm.NET
{
    public class MDTuple : MDNode
    {
        internal MDTuple( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}
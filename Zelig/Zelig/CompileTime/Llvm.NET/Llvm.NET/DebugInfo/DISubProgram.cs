using Llvm.NET.Values;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubprogram"/></summary>
    public class DISubProgram : DILocalScope
    {
        internal DISubProgram( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public bool Describes( Function function ) => NativeMethods.SubProgramDescribes( MetadataHandle, function.ValueHandle );
    }
}

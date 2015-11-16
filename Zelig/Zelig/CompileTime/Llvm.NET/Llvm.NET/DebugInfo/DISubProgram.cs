using Llvm.NET.Values;

namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#disubprogram"/></summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubProgram" )]
    public class DISubProgram : DILocalScope
    {
        internal DISubProgram( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specific type required by interop call" )]
        public bool Describes( Function function ) => NativeMethods.SubProgramDescribes( MetadataHandle, function.ValueHandle );
    }
}

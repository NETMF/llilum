namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#dilocalvariable"/></summary>
    public class DILocalVariable : DIVariable
    {
        internal DILocalVariable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

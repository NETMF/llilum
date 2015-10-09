namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diglobalvariable"/></summary>
    public class DIGlobalVariable : DIVariable
    {
        internal DIGlobalVariable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

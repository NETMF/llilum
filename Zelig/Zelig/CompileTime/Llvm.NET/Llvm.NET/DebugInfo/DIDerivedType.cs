namespace Llvm.NET.DebugInfo
{
    /// <summary>see <a href="http://llvm.org/docs/LangRef.html#diderivedtype"/></summary>
    public class DIDerivedType : DIType
    {
        internal DIDerivedType( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

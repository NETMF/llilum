namespace Llvm.NET.DebugInfo
{
    /// <summary>Legal scope for lexical blocks, local variables, and debug info locations</summary>
    public class DILocalScope : DIScope
    {
        internal DILocalScope( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        // returns "this" if the scope is a subprogram, otherwise walks up the scopes to find 
        // the containing subprogram.
        public DISubProgram SubProgram => FromHandle< DISubProgram>( NativeMethods.DILocalScopeGetSubProgram(MetadataHandle) );
    }
}

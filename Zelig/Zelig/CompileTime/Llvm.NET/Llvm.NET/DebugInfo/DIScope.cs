namespace Llvm.NET.DebugInfo
{
    /// <summary>Base class for all Debug info scopes</summary>
    public class DIScope : DINode
    {
        internal DIScope( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public DIFile File => DINode.FromHandle< DIFile >( NativeMethods.DIScopeGetFile( MetadataHandle ) );
    }
}

namespace Llvm.NET.DebugInfo
{
    /// <summary>Base class for all Debug info scopes</summary>
    public class DIScope : DINode
    {
        internal DIScope( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public DIFile File
        {
            get
            {
                var handle = NativeMethods.DIScopeGetFile( MetadataHandle );
                if( handle == LLVMMetadataRef.Zero )
                    return null;
                return DINode.FromHandle<DIFile>( handle );
            }
        }
    }
}

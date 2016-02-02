using Llvm.NET.Native;

namespace Llvm.NET
{
    public class MDString
        : LlvmMetadata
    {
        internal MDString( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public override string ToString( )
        {
            uint len;
            var ptr = NativeMethods.GetMDStringText( MetadataHandle, out len );
            return NativeMethods.NormalizeLineEndings( ptr, ( int )len );
        }
    }
}
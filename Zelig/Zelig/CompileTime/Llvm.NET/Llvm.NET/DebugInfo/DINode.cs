using System;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Root of the object hierarchy for Debug information metadata nodes</summary>
    public class DINode : MDNode
    {
        /// <summary>Dwarf tag for the descriptor</summary>
        public Tag Tag
        {
            get
            {
                if( MetadataHandle.Pointer == IntPtr.Zero )
                    return (Tag)(ushort.MaxValue);

                return ( Tag )NativeMethods.DIDescriptorGetTag( MetadataHandle );
            }
        }

        internal DINode( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        /// <inheritdoc/>
        public override string ToString( )
        {
            if( MetadataHandle.Pointer == IntPtr.Zero )
                return string.Empty;

            return NativeMethods.MarshalMsg( NativeMethods.DIDescriptorAsString( MetadataHandle ) );
        }
    }
}

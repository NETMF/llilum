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

        internal static DINode FromHandle( LLVMMetadataRef handle ) => FromHandle<DINode>( handle );

        internal static T FromHandle<T>( LLVMMetadataRef handle )
            where T : DINode
        {
            var context = Context.GetContextFor( handle );
            return FromHandle<T>( context, handle );
        }
    }
}

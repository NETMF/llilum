using System;
using System.Collections.Generic;
using Llvm.NET.Native;
using Llvm.NET.Values;

namespace Llvm.NET
{
    public class MDNode : LlvmMetadata
    {
        internal MDNode( LLVMMetadataRef handle )
            : base( handle )
        {
            Operands = new MDNodeOperandList( this );
        }

        public Context Context => Context.GetContextFor( MetadataHandle );
        public bool IsDeleted => MetadataHandle == LLVMMetadataRef.Zero;
        public bool IsTemporary => NativeMethods.IsTemporary( MetadataHandle );
        public bool IsResolved => NativeMethods.IsResolved( MetadataHandle );
        public bool IsUniqued => NativeMethods.IsUniqued( MetadataHandle );
        public bool IsDistinct => NativeMethods.IsDistinct( MetadataHandle );
        public IReadOnlyList<MDOperand> Operands { get; }

        public void ResolveCycles( ) => NativeMethods.MDNodeResolveCycles( MetadataHandle );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MDNode" )]
        public override void ReplaceAllUsesWith( LlvmMetadata other )
        {
            if( other == null )
                throw new ArgumentNullException( nameof( other ) );

            if( !IsTemporary || IsResolved )
                throw new InvalidOperationException( "Cannot replace non temporary or resolved MDNode" );

            if( MetadataHandle.Pointer == IntPtr.Zero )
                throw new InvalidOperationException( "Cannot Replace all uses of a null descriptor" );

            NativeMethods.MDNodeReplaceAllUsesWith( MetadataHandle, other.MetadataHandle );
            // remove current node mapping from the context.
            // It won't be valid for use after clearing the handle
            Context.RemoveDeletedNode( this );
            MetadataHandle = LLVMMetadataRef.Zero;
        }

        internal static T FromHandle<T>( LLVMMetadataRef handle )
        where T : MDNode
        {
            if( handle.Pointer.IsNull( ) )
                return null;

            var context = Context.GetContextFor( handle );
            return FromHandle<T>( context, handle );
        }
    }
}
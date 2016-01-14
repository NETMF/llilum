using System.Collections;
using System.Collections.Generic;

namespace Llvm.NET
{
    /// <summary>Wraps an LLVM NamedMDNode</summary>
    /// <remarks>Despite its name a NamedMDNode is not itself an MDNode.</remarks>
    public class NamedMDNode
    {
        internal NamedMDNode( LLVMNamedMDNodeRef nativeNode )
        {
            NativeHandle = nativeNode;
            Operands = new OperandIterator( this );
        }

        // TODO: Enable retrieving the name from LibLLVM
        //public string Name { get; }

        public IReadOnlyList<MDNode> Operands { get; }
        public NativeModule ParentModule => NativeModule.FromHandle( NativeMethods.NamedMDNodeGetParentModule( NativeHandle ) );

        private LLVMNamedMDNodeRef NativeHandle;

        private class OperandIterator
            : IReadOnlyList<MDNode>
        {
            internal OperandIterator( NamedMDNode owner )
            {
                OwningNode = owner;
            }

            public MDNode this[ int index ]
            {
                get
                {
                    var nodeHanlde = NativeMethods.NamedMDNodeGetOperand( OwningNode.NativeHandle, (uint)index );
                    return LlvmMetadata.FromHandle<MDNode>( OwningNode.ParentModule.Context, nodeHanlde );
                }
            }

            public int Count => (int)NativeMethods.NamedMDNodeGetNumOperands( OwningNode.NativeHandle );

            public IEnumerator<MDNode> GetEnumerator( )
            {
                for( int i = 0; i < Count; ++i )
                    yield return this[ i ];
            }

            IEnumerator IEnumerable.GetEnumerator( )
            {
                return GetEnumerator( );
            }

            private NamedMDNode OwningNode;
        }
    }
}

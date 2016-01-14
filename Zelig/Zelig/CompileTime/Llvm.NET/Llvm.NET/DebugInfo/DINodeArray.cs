using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Generic wrapper to treat an MDTuple as an array of elements of specific type</summary>
    /// <typeparam name="T">Type of elements</typeparam>
    /// <remarks>
    /// This treats the operands of a tuple as the elements of the array
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public class TupleTypedArrayWrapper<T>
        : IReadOnlyList<T>
        where T : LlvmMetadata

    {
        public TupleTypedArrayWrapper( MDTuple tuple )
        {
            Tuple = tuple;
        }

        public MDTuple Tuple { get; }

        public int Count => Tuple.Operands.Count;

        public T this[ int index ]
        {
            get
            {
                if( Tuple == null )
                    throw new InvalidOperationException( "Wrapped node is null" );

                if( index > Tuple.Operands.Count )
                    throw new ArgumentOutOfRangeException( nameof( index ) );

                return Tuple.Operands[ index ].Metadata as T;
            }
        }

        public IEnumerator<T> GetEnumerator( )
        {
            return Tuple.Operands
                        .Select( n => n.Metadata as T )
                        .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator( )
        {
            return GetEnumerator();
        }
    }

    /// <summary>Array of <see cref="DINode"/> debug information nodes for use with <see cref="DebugInfoBuilder"/> methods</summary>
    /// <seealso cref="DebugInfoBuilder.GetOrCreateArray(IEnumerable{DINode})"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
    public class DINodeArray : TupleTypedArrayWrapper<DINode>
    {
        internal DINodeArray( MDTuple tuple )
            : base( tuple )
        {
        }
    }
}

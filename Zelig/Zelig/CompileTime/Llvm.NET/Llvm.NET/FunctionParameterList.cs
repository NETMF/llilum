using System;
using System.Collections.Generic;

namespace Llvm.NET
{
    /// <summary>Support class to provide readonly list semantics to the parameters of a method</summary>
    internal class FunctionParameterList
        : IReadOnlyList<Argument>
    {
        internal FunctionParameterList( Function owningFunction )
        {
            OwningFunction = owningFunction;
        }

        public Argument this[ int index ]
        {
            get
            {
                if( index >= Count || index < 0 )
                    throw new IndexOutOfRangeException( );

                return (Argument)Value.FromHandle( LLVMNative.GetParam( OwningFunction.ValueHandle, ( uint )index ) );
            }
        }

        public int Count
        {
            get
            {
                uint count = LLVMNative.CountParams( OwningFunction.ValueHandle );
                return ( int )Math.Min( count, int.MaxValue );
            }
        }

        public IEnumerator<Argument> GetEnumerator( )
        {
            for( uint i = 0; i < Count; ++i )
            {
                LLVMValueRef val = LLVMNative.GetParam( OwningFunction.ValueHandle, i );
                if( val.Pointer == IntPtr.Zero )
                    yield break;

                yield return (Argument)Value.FromHandle( val );
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) => GetEnumerator( );

        private Function OwningFunction;
    }
}

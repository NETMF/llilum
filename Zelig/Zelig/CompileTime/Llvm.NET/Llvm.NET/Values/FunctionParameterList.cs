using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
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

                return Value.FromHandle<Argument>( NativeMethods.GetParam( OwningFunction.ValueHandle, ( uint )index ) );
            }
        }

        public int Count
        {
            get
            {
                uint count = NativeMethods.CountParams( OwningFunction.ValueHandle );
                return ( int )Math.Min( count, int.MaxValue );
            }
        }

        public IEnumerator<Argument> GetEnumerator( )
        {
            for( uint i = 0; i < Count; ++i )
            {
                LLVMValueRef val = NativeMethods.GetParam( OwningFunction.ValueHandle, i );
                if( val.Pointer == IntPtr.Zero )
                    yield break;

                yield return Value.FromHandle<Argument>( val );
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) => GetEnumerator( );

        private Function OwningFunction;
    }
}

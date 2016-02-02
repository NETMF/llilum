using System;
using System.Collections.Generic;
using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    /// <summary>Support class to provide read-only list semantics to the operands of a <see cref="User"/> of a method</summary>
    internal class UserOperandList
        : IReadOnlyList<Value>
    {
        internal UserOperandList( User owner )
        {
            Owner = owner;
        }

        public Value this[ int index ]
        {
            get
            {
                if( index >= Count || index < 0 )
                    throw new ArgumentOutOfRangeException( nameof( index ) );

                return Value.FromHandle( NativeMethods.GetOperand( Owner.ValueHandle, ( uint )index ) );
            }
        }

        public int Count
        {
            get
            {
                int count = NativeMethods.GetNumOperands( Owner.ValueHandle );
                return Math.Min( count, int.MaxValue );
            }
        }

        public IEnumerator<Value> GetEnumerator( )
        {
            for( uint i = 0; i < Count; ++i )
            {
                LLVMValueRef val = NativeMethods.GetOperand( Owner.ValueHandle, i );
                if( val.Pointer == IntPtr.Zero )
                    yield break;

                yield return Value.FromHandle( val );
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) => GetEnumerator( );

        private User Owner;
    }
}

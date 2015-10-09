using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
{
    /// <summary>Support class to provide readonly list semantics to the parameters of a method</summary>
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
                    throw new IndexOutOfRangeException( );

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

                // REVIEW: This won't create a properly down castable Value instance
                // it can only create a Value instance. (Although if Value.FromHandle())
                // were to get smarter it could figure out the tproper type and create
                // the instance (TypeRef does this for its derived types already)
                yield return Value.FromHandle( val );
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) => GetEnumerator( );

        private User Owner;
    }
}

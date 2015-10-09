using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
{
    /// <summary>Contains an LLVM User value</summary>
    /// <remarks>
    /// A user is one role in the user->uses relationship
    /// conveyed by the LLVM value model. A User can contain
    /// references (e.g. uses) of other values.
    /// </remarks>
    public class User : Value
    {
        internal User( LLVMValueRef userRef )
            : base( ValidateConversion( userRef, NativeMethods.IsAUser ) )
        {
            OperandList = new UserOperandList( this );
        }

        /// <summary>Collection of operands</summary>
        public IReadOnlyList<Value> Operands => OperandList;

        /// <summary>Enumerable collection of <see cref="Use"/>s</summary>
        public IEnumerable<Use> Uses
        {
            get
            {
                LLVMUseRef current = NativeMethods.GetFirstUse( ValueHandle );
                while( current.Pointer != IntPtr.Zero )
                {
                    // TODO: intern the use instances?
                    yield return new Use( current );
                    current = NativeMethods.GetNextUse( current );
                }
            }
        }


        private UserOperandList OperandList;
    }

    /// <summary>LLVM Use, which is essentially a tuple of the <see cref="User"/> and the <see cref="Value"/> used</summary>
    /// <remarks>
    /// A Use in LLVM forms a link in a directed graph of dependencies for values.
    /// </remarks>
    public class Use
    {
        internal Use( LLVMUseRef useRef )
        {
            OpaqueHandle = useRef;
        }

        public User User => Value.FromHandle<User>( NativeMethods.GetUser( OpaqueHandle ) );
        public Value Value => Value.FromHandle( NativeMethods.GetUsedValue( OpaqueHandle ) );
        private LLVMUseRef OpaqueHandle;
    }

}

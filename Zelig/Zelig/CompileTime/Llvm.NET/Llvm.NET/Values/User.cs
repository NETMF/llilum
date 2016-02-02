using System;
using System.Collections.Generic;
using Llvm.NET.Native;

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
            : base( userRef )
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
}

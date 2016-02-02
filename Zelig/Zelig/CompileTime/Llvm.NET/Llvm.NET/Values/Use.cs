using System;
using System.Linq;
using System.Collections.Generic;
using Llvm.NET.Native;

namespace Llvm.NET.Values
{
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
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Llvm.NET.Native
{
    internal static class IntPtrExtensions
    {
        public static bool IsNull( this IntPtr self ) => self == IntPtr.Zero;
        public static bool IsNull( this UIntPtr self ) => self == UIntPtr.Zero;
    }

    /// <summary>Base class for LLVM disposable types that are instantiated outside of an LLVM <see cref="Context"/> and therefore won't be disposed by the context</summary>
    [SecurityCritical]
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    internal abstract class SafeHandleNullIsInvalid
        : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected SafeHandleNullIsInvalid( bool ownsHandle)
            : base( IntPtr.Zero, ownsHandle )
        {
        }

        public bool IsNull => handle.IsNull();

        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                return IsNull;
            }
        }
    }

    [SecurityCritical]
    internal class AttributeBuilderHandle
        : SafeHandleNullIsInvalid
    {
        internal AttributeBuilderHandle()
            : base( true )
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Required for marshaling support (used via reflection)" )]
        internal AttributeBuilderHandle( IntPtr handle )
            : base( true )
        {
            SetHandle( handle );
        }

        [SecurityCritical]
        protected override bool ReleaseHandle( )
        {
            NativeMethods.AttributeBuilderDispose( this.handle );
            return true;
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Llvm.NET.DebugInfo
{
    /// <summary>Base class for Debug info types</summary>
    public class DIType : DIScope
    {
        internal DIType( LLVMMetadataRef handle )
            : base( handle )
        {
        }

        public DebugInfoFlags Flags
        {
            get
            {
                if( MetadataHandle.Pointer == IntPtr.Zero )
                    return 0;

                return ( DebugInfoFlags )NativeMethods.DITypeGetFlags( MetadataHandle );
            }
        }

        public UInt32 Line => NativeMethods.DITypeGetLine( MetadataHandle );
        public UInt64 BitSize => NativeMethods.DITypeGetSizeInBits( MetadataHandle );
        public UInt64 BitAlignment => NativeMethods.DITypeGetAlignInBits( MetadataHandle );
        public UInt64 BitOffset => NativeMethods.DITypeGetOffsetInBits( MetadataHandle );
        public DIScope Scope => new DIScope( NativeMethods.DITypeGetScope( MetadataHandle ) );
        public bool IsPrivate => ( Flags & DebugInfoFlags.AccessibilityMask ) == DebugInfoFlags.Private;
        public bool IsProtected => ( Flags & DebugInfoFlags.AccessibilityMask ) == DebugInfoFlags.Protected;
        public bool IsPublic => ( Flags & DebugInfoFlags.AccessibilityMask ) == DebugInfoFlags.Public;
        public bool IsForwardDeclaration => Flags.HasFlag( DebugInfoFlags.FwdDecl );
        public bool IsAppleBlockExtension => Flags.HasFlag( DebugInfoFlags.AppleBlock );
        public bool IsBlockByRefStruct => Flags.HasFlag( DebugInfoFlags.BlockByrefStruct );
        public bool IsVirtual => Flags.HasFlag( DebugInfoFlags.Virtual );
        public bool IsArtificial => Flags.HasFlag( DebugInfoFlags.Artificial );
        public bool IsObjectPointer => Flags.HasFlag( DebugInfoFlags.ObjectPointer );
        public bool IsObjClassComplete => Flags.HasFlag( DebugInfoFlags.ObjcClassComplete );
        public bool IsVector => Flags.HasFlag( DebugInfoFlags.Vector );
        public bool IsStaticMember => Flags.HasFlag( DebugInfoFlags.StaticMember );
        public bool IsLvalueReference => Flags.HasFlag( DebugInfoFlags.LValueReference );
        public bool IsRvalueReference => Flags.HasFlag( DebugInfoFlags.RValueReference );

        public string Name => Marshal.PtrToStringAnsi( NativeMethods.DITypeGetName( MetadataHandle ) );
    }
}

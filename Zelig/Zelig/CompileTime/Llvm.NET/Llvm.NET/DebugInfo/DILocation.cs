using System;

namespace Llvm.NET.DebugInfo
{
    public class DILocation : MDNode
    {
        public DILocation( Context context, uint line, uint column, DILocalScope scope )
            : this( context, line, column, scope, null )
        {
        }

        public DILocation( Context context, uint line, uint column, DILocalScope scope, DILocation inlinedAt )
            : base( NativeMethods.DILocation( context.ContextHandle, line, column, scope.MetadataHandle, inlinedAt?.MetadataHandle ?? LLVMMetadataRef.Zero ) )
        {
        }

        public DILocalScope Scope => DINode.FromHandle< DILocalScope >( NativeMethods.GetDILocationScope( MetadataHandle ) );

        public uint Line => NativeMethods.GetDILocationLine( MetadataHandle );

        public uint Column => NativeMethods.GetDILocationColumn( MetadataHandle );

        public DILocation InlinedAt
        {
            get
            {
                var handle = NativeMethods.GetDILocationInlinedAt( MetadataHandle );
                if( handle.Pointer == IntPtr.Zero )
                    return null;

                return new DILocation( handle );
            }
        }

        public override string ToString( )
        {
            return $"{Scope.File}({Line},{Column})";
        }

        internal DILocation( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

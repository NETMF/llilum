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

        internal DILocation( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

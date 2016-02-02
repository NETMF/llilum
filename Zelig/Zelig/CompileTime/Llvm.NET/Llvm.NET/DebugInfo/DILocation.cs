using Llvm.NET.Native;
using Llvm.NET.Values;

namespace Llvm.NET.DebugInfo
{
    public class DILocation : MDNode
    {
        public DILocation( Context context, uint line, uint column, DILocalScope scope )
            : this( context, line, column, scope, null )
        {
        }

        public DILocation( Context context, uint line, uint column, DILocalScope scope, DILocation inlinedAt )
            : base( NativeMethods.DILocation( context.VerifyArgNotNull( nameof( context ) ).ContextHandle
                                            , line
                                            , column
                                            , scope.VerifyArgNotNull(nameof(scope)).MetadataHandle
                                            , inlinedAt?.MetadataHandle ?? LLVMMetadataRef.Zero 
                                            )
                  )
        {
        }

        public DILocalScope Scope => FromHandle< DILocalScope >( NativeMethods.GetDILocationScope( MetadataHandle ) );

        public uint Line => NativeMethods.GetDILocationLine( MetadataHandle );

        public uint Column => NativeMethods.GetDILocationColumn( MetadataHandle );

        public DILocation InlinedAt
        {
            get
            {
                var handle = NativeMethods.GetDILocationInlinedAt( MetadataHandle );
                return FromHandle<DILocation>( handle );
            }
        }

        public DILocalScope InlinedAtScope
        {
            get
            {
                var handle = NativeMethods.DILocationGetInlinedAtScope( MetadataHandle );
                return FromHandle<DILocalScope>( handle );
            }
        }

        public override string ToString( )
        {
            return $"{Scope.File}({Line},{Column})";
        }

        public bool Describes( Function function )
        {
            return Scope.SubProgram.Describes( function )
                || InlinedAtScope.SubProgram.Describes( function );

        }

        internal DILocation( LLVMMetadataRef handle )
            : base( handle )
        {
        }
    }
}

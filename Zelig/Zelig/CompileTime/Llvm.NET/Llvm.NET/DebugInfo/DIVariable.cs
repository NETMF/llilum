namespace Llvm.NET.DebugInfo
{
    public class DIVariable : DINode
    {
        internal DIVariable( LLVMMetadataRef handle )
            : base( handle )
        {
        }
        
        // TODO: UInt32 Line => NativeMethods.DIVariableGetLine( MetadataHandle );

        public DIScope Scope => Operands[ 0 ]?.Metadata as DIScope;

        public string Name => ( Operands[ 1 ]?.Metadata as MDString )?.ToString( ) ?? string.Empty;

        public DIFile File => Operands[ 2 ]?.Metadata as DIFile;

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods" )]
        public DIType Type => Operands[ 3 ]?.Metadata as DIType;
    }
}

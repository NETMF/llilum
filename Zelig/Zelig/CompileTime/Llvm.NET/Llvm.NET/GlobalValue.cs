namespace Llvm.NET
{
    /// <summary>LLVM Global value </summary>
    public class GlobalValue : Constant
    {
        /// <summary>Visibility of this global value</summary>
        public Visibility Visibility
        {
            get
            {
                return ( Visibility )LLVMNative.GetVisibility( ValueHandle );
            }
            set
            {
                LLVMNative.SetVisibility( ValueHandle, ( LLVMVisibility )value );
            }
        }

        /// <summary>Linkage specification for this symbol</summary>
        public Linkage Linkage
        {
            get
            {
                return ( Linkage )LLVMNative.GetLinkage( ValueHandle );
            }
            set
            {
                LLVMNative.SetLinkage( ValueHandle, ( LLVMLinkage )value );
            }
        }

        /// <summary>Flag to indicate if this is an Unnamed address</summary>
        public bool UnnamedAddress
        {
            get
            {
                return LLVMNative.HasUnnamedAddr( ValueHandle );
            }
            set
            {
                LLVMNative.SetUnnamedAddr( ValueHandle, value );
            }
        }

        /// <summary>Flag to indicate if this is a declaration</summary>
        public bool IsDeclaration => LLVMNative.IsDeclaration( ValueHandle );

        /// <summary>Module containing this global value</summary>
        public Module ParentModule => Type.Context.GetModuleFor( LLVMNative.GetGlobalParent( ValueHandle ) );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Language", "CSE0003:Use expression-bodied members", Justification = "Readability" )]
        internal new static GlobalValue FromHandle( LLVMValueRef valueRef )
        {
            return (GlobalValue)Context.CurrentContext.GetValueFor( valueRef, ( h )=>new GlobalValue( h ) );
        }

        internal GlobalValue( LLVMValueRef valueRef )
            : base( ValidateConversion( valueRef, LLVMNative.IsAGlobalValue ) )
        {
        }
    }
}

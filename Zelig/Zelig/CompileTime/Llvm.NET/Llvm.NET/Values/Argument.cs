namespace Llvm.NET.Values
{
    /// <summary>An LLVM Value representing an Argument in a function</summary>
    public class Argument
        : Value
    {
        /// <summary>Function this argument belongs to</summary>
        public Function ContainingFunction => new Function( LLVMNative.GetParamParent( ValueHandle ) );

        /// <summary>Sets the alignment for the argument</summary>
        /// <param name="value">Alignment value for this argument</param>
        public void SetAlignment( uint value )
        {
            LLVMNative.SetParamAlignment( ValueHandle, value );
        }

        /// <summary>Current attributes for the argument</summary>
        public Attributes Attributes => (Attributes)LLVMNative.GetAttribute( ValueHandle );
        
        /// <summary>Add attributes to the argument</summary>
        /// <param name="attrib">Attributes flags to add to the argument</param>
        public void AddAttributes( Attributes attrib ) => LLVMNative.AddAttribute( ValueHandle, ( LLVMAttribute )attrib );

        /// <summary>Remove attributes from the argument</summary>
        /// <param name="attrib">attributes to remove from the argument</param>
        public void RemoveAttributes( Attributes attrib ) => LLVMNative.RemoveAttribute( ValueHandle, ( LLVMAttribute )attrib );

        internal Argument( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Argument( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, LLVMNative.IsAArgument ) )
        {
        }
    }
}

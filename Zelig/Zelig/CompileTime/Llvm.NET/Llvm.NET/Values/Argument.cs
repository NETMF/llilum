using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
{
    /// <summary>An LLVM Value representing an Argument to a function</summary>
    public class Argument
        : Value
    {
        /// <summary>Function this argument belongs to</summary>
        public Function ContainingFunction => FromHandle<Function>( NativeMethods.GetParamParent( ValueHandle ) );
        
        /// <summary>Zero based index of the argument</summary>
        public uint Index => NativeMethods.GetArgumentIndex( ValueHandle );

        /// <summary>Sets the alignment for the argument</summary>
        /// <param name="value">Alignment value for this argument</param>
        public void SetAlignment( uint value )
        {
            NativeMethods.SetParamAlignment( ValueHandle, value );
        }

        /// <summary>Attributes for this parameter</summary>
        public IAttributeSet Attributes { get; }

        /// <summary>Add a set of attributes using fluent style coding</summary>
        /// <param name="attributes">Attributes to add</param>
        /// <returns></returns>
        public Argument AddAttributes( IEnumerable<AttributeValue> attributes )
        {
            Attributes.Add( attributes );
            return this;
        }

        /// <summary>Add a set of attributes using fluent style coding</summary>
        /// <param name="attributes">Attributes to add</param>
        /// <returns></returns>
        public Argument AddAttributes( params AttributeValue[] attributes )
        {
            Attributes.Add( attributes );
            return this;
        }

        internal Argument( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal Argument( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAArgument ) )
        {
            Attributes = new AttributeSetImpl( ContainingFunction, FunctionAttributeIndex.Parameter0 + (int)Index );
        }
    }
}

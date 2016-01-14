using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
{
    /// <summary>An LLVM Value representing an Argument to a function</summary>
    public class Argument
        : Value
        , IAttributeSetContainer
    {
        /// <summary>Function this argument belongs to</summary>
        public Function ContainingFunction => FromHandle<Function>( NativeMethods.GetParamParent( ValueHandle ) );
        
        /// <summary>Zero based index of the argument</summary>
        public uint Index => NativeMethods.GetArgumentIndex( ValueHandle );

        /// <summary>Sets the alignment for the argument</summary>
        /// <param name="value">Alignment value for this argument</param>
        public Argument SetAlignment( uint value )
        {
            ContainingFunction.AddAttribute( FunctionAttributeIndex.Parameter0 + ( int ) Index
                                           , new AttributeValue( Context, AttributeKind.Alignment, value )
                                           );
            return this;
        }

        /// <summary>Attributes for this parameter</summary>
        public AttributeSet Attributes
        {
            get { return ContainingFunction.Attributes.ParameterAttributes( ( int )Index ); }
            set
            {
                ContainingFunction.AddAttributes( FunctionAttributeIndex.Parameter0 + ( int )Index, value );
            }
        }

        internal Argument( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsAArgument ) )
        {
        }
    }
}

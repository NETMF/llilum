using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
{
    /// <summary>Interface for a set of attributes on a Function, Function return or parameter</summary>
    public interface IAttributeSet
    {
        /// <summary>Parameter alignement (only valid on parameters)</summary>
        uint? ParamAlignment { get; set; }

        /// <summary>Stack alignment requiirements for this function if not the same as the ABI</summary>
        uint? StackAlignment { get; set; }

        ulong? DereferenceableBytes { get; set; }

        ulong? DereferenceableOrNullBytes { get; set; }

        /// <summary>Adds a set of boolean attributes to the function itself</summary>
        /// <param name="attributes">Attributes to add</param>
        /// <returns>This instance for use in fluent style coding</returns>
        IAttributeSet Add( params AttributeValue[ ] attributes );

        /// <summary>Add a collection of attributes to the function itself</summary>
        /// <param name="attributes"></param>
        /// <returns>This instance for use in fluent style coding</returns>
        IAttributeSet Add( IEnumerable<AttributeValue> attributes );

        /// <summary>Adds a single boolean attribute</summary>
        /// <param name="kind">AttributeValue kind to add</param>
        /// <returns>This instance for use in fluent style coding</returns>
        IAttributeSet Add( AttributeValue kind );

        /// <summary>Removes the specified attribute from the attribute set</summary>
        /// <returns>This instance for use in fluent style coding</returns>
        IAttributeSet Remove( AttributeKind kind );

        /// <summary>Remove a target specific attribute</summary>
        /// <param name="name">Name of the attribute</param>
        /// <returns>This instance for use in fluent style coding</returns>
        IAttributeSet Remove( string name );

        /// <summary>Tests if this attribute set has a given AttributeValue kind</summary>
        /// <param name="kind">Kind of AttributeValue to test for</param>
        /// <returns>true if the AttributeValue esists or false if not</returns>
        bool Has( AttributeKind kind );

        /// <summary>Tests if this attribute set has a given string attribute</summary>
        /// <param name="name">Name of the attribute to test for</param>
        /// <returns>true if the attribute exists or false if not</returns>
        bool Has( string name );
    }

    internal sealed class AttributeSetImpl
        : IAttributeSet
    {
        /// <inheritdoc/>
        public override string ToString( )
        {
            if( !NativeMethods.FunctionHasAttributes( OwningFunction.ValueHandle, ( int )Index ) )
                return string.Empty;

            var intPtr = NativeMethods.GetFunctionAttributesAsString( OwningFunction.ValueHandle, ( int )Index );
            return NativeMethods.MarshalMsg( intPtr );
        }

        /// <inheritdoc/>
        public IAttributeSet Add( AttributeValue attribute )
        {
            OwningFunction.AddAttribute( Index, attribute );
            return this;
        }

        /// <inheritdoc/>
        public IAttributeSet Add( IEnumerable< AttributeValue > attributes )
        {
            OwningFunction.AddAttributes( Index, attributes );
            return this;
        }

        /// <inheritdoc/>
        public IAttributeSet Add( params AttributeValue[ ] attributes ) => Add( ( IEnumerable<AttributeValue> )attributes );

        /// <inheritdoc/>
        public IAttributeSet Remove( AttributeKind kind )
        {
            OwningFunction.RemoveAttribute( Index, kind );
            return this;
        }

        /// <inheritdoc/>
        public IAttributeSet Remove( string name )
        {
            OwningFunction.RemoveAttribute( Index, name );
            return this;
        }

        /// <inheritdoc/>
        public bool Has( AttributeKind kind ) => OwningFunction.HasAttribute( Index, kind );

        /// <inheritdoc/>
        public bool Has( string name ) => OwningFunction.HasAttribute( Index, name );

        /// <inheritdoc/>
        public uint? ParamAlignment
        {
            get
            {
                if( !OwningFunction.HasAttribute( Index, AttributeKind.Alignment ) )
                    return null;

                return (uint)OwningFunction.GetAttributeValue( Index, AttributeKind.Alignment );
            }

            set
            {
                if( !value.HasValue )
                {
                    if( OwningFunction.HasAttribute( Index, AttributeKind.Alignment ) )
                        OwningFunction.RemoveAttribute( Index, AttributeKind.Alignment );
                }
                else
                {
                    OwningFunction.AddAttribute( Index, new AttributeValue( AttributeKind.Alignment, value.Value ) );
                }
            }
        }

        /// <inheritdoc/>
        public uint? StackAlignment
        {
            get
            {
                if( !Has( AttributeKind.StackAlignment ) )
                    return null;

                return ( uint )OwningFunction.GetAttributeValue( Index, AttributeKind.StackAlignment );
            }

            set
            {
                if( !value.HasValue )
                {
                    if( OwningFunction.HasAttribute( Index, AttributeKind.StackAlignment ) )
                        OwningFunction.RemoveAttribute( Index, AttributeKind.StackAlignment );
                }
                else
                {
                    OwningFunction.AddAttribute( Index, new AttributeValue( AttributeKind.StackAlignment, value.Value ) );
                }
            }
        }

        /// <inheritdoc/>
        public ulong? DereferenceableBytes
        {
            get
            {
                if( !Has( AttributeKind.Dereferenceable ) )
                    return null;

                return ( uint )OwningFunction.GetAttributeValue( Index, AttributeKind.Dereferenceable );
            }

            set
            {
                if( !value.HasValue )
                {
                    if( OwningFunction.HasAttribute( Index, AttributeKind.Dereferenceable ) )
                        OwningFunction.RemoveAttribute( Index, AttributeKind.Dereferenceable );
                }
                else
                {
                    OwningFunction.AddAttribute( Index, new AttributeValue( AttributeKind.Dereferenceable, value.Value ) );
                }
            }
        }

        /// <inheritdoc/>
        public ulong? DereferenceableOrNullBytes
        {
            get
            {
                if( !Has( AttributeKind.DereferenceableOrNull ) )
                    return null;

                return ( uint )OwningFunction.GetAttributeValue( Index, AttributeKind.DereferenceableOrNull );
            }
            set
            {
                if( !value.HasValue )
                {
                    if( OwningFunction.HasAttribute( Index, AttributeKind.DereferenceableOrNull ) )
                        OwningFunction.RemoveAttribute( Index, AttributeKind.DereferenceableOrNull );
                }
                else
                {
                    OwningFunction.AddAttribute( Index, new AttributeValue( AttributeKind.DereferenceableOrNull, value.Value ) );
                }
            }
        }

        internal AttributeSetImpl( Function function, FunctionAttributeIndex index )
        {
            if( function == null )
                throw new ArgumentNullException( nameof( function ) );

            OwningFunction = function;
            // validate index parameter
            switch( index )
            {
            case FunctionAttributeIndex.Function:
            case FunctionAttributeIndex.ReturnType:
                // OK as-is
                break;
            default:
                // check the index against the function's param count
                var argIndex = ((int)index) - (int)FunctionAttributeIndex.Parameter0;
                if( argIndex < 0 || argIndex >= OwningFunction.Parameters.Count )
                    throw new ArgumentOutOfRangeException( nameof( index ) );
                break;
            }
            Index = index;
        }

        private readonly Function OwningFunction;
        private readonly FunctionAttributeIndex Index;
    }
}

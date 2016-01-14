using System;
using System.Collections.Generic;

namespace Llvm.NET.Values
{
    /// <summary>Interface for values containing an AttributeSet</summary>
    /// <remarks>
    /// This is used to allow the <see cref="AttributeSetContainer"/> extension
    /// to act as mutators for the otherwise immutable <see cref="AttributeSet"/>.
    /// Each method of the extension class will read the attribute set from the container
    /// and create a new set based on the parameters (adding or removing attributes from the set)
    /// producing a new attributeSet that is then re-assigned back to the container. 
    /// </remarks>
    public interface IAttributeSetContainer
    {
        /// <summary>Attributes for this container</summary>
        AttributeSet Attributes { get; set; }
    }

    /// <summary>Static class to provide mutators for otherwise immutable <see cref="AttributeSet"/>s</summary>
    /// <remarks>
    /// The methods of this class provide mutators for an <see cref="AttributeSet"/> contained in a 
    /// class implementing <see cref="IAttributeSetContainer"/>. (This includes <see cref="Function"/>
    /// <see cref="Instructions.CallInstruction"/>, and <see cref="Instructions.Invoke"/>. An
    /// <see cref="AttributeSet"/> is immutable, all of the methods that modify an attribute set actually
    /// produce a new attribute set. (This follows the underlying LLVM model and semantics). Thus, to 
    /// change the attributes of a <see cref="IAttributeSetContainer"/> you must get the
    /// <see cref="IAttributeSetContainer.Attributes"/> set, produce a modified version and then set the
    /// new value back to the <see cref="IAttributeSetContainer.Attributes"/> property. The methods in 
    /// this class will perform the read, modify and write back sequence as a single call for any of the
    /// available <see cref="IAttributeSetContainer"/> implementations. 
    /// </remarks>
    public static class AttributeSetContainer
    {
        public static T AddAttribute<T>( this T self, FunctionAttributeIndex index, AttributeKind[] values )
            where T : IAttributeSetContainer
        {
            if(values == null)
                throw new ArgumentNullException( nameof( values ) );

            using( var bldr = new AttributeBuilder( self.Attributes, index ) )
            {
                foreach( var kind in values )
                    bldr.Add( kind );

                self.Attributes = self.Attributes.Add( index, bldr.ToAttributeSet( index ) );
                return self;
            }
        }

        public static T AddAttribute<T>( this T self, FunctionAttributeIndex index, AttributeKind value )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Add( index, new AttributeValue( self.Attributes.Context, value ) );
            return self;
        }

        public static T AddAttribute<T>( this T self, FunctionAttributeIndex index, AttributeValue value )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Add( index, value );
            return self;
        }

        /// <summary>Compatibility extension method to handle migrating code from older attribute handling</summary>
        /// <param name="self">Function to add attributes to</param>
        /// <param name="attributes">Attributes to add</param>
        /// <returns>The function itself</returns>
        /// <remarks>
        /// Adds attributes to a given function itself (as opposed to the return or one of the function's parameters)
        /// This is equivalent to calling <see cref="AddAttributes{T}(T, FunctionAttributeIndex, AttributeValue[])"/>
        /// with <see cref="FunctionAttributeIndex.Function"/> as the first parameter
        /// </remarks>
        public static Function AddAttributes( this Function self, params AttributeValue[] attributes)
        {
            if(self == null)
                throw new ArgumentNullException( nameof( self ) );

            self.Attributes = self.Attributes.Add( FunctionAttributeIndex.Function, attributes );
            return self;
        }

        /// <summary>Compatibility extension method to handle migrating code from older attribute handling</summary>
        /// <param name="self">Function to add attributes to</param>
        /// <param name="attributes">Attributes to add</param>
        /// <returns>The function itself</returns>
        /// <remarks>
        /// Adds attributes to a given function itself (as opposed to the return or one of the function's parameters)
        /// This is equivalent to calling <see cref="AddAttributes{T}(T, FunctionAttributeIndex, IEnumerable{AttributeValue})"/>
        /// with <see cref="FunctionAttributeIndex.Function"/> as the first parameter
        /// </remarks>
        public static Function AddAttributes( this Function self, IEnumerable<AttributeValue> attributes)
        {
            if(self == null)
                throw new ArgumentNullException( nameof( self ) );

            self.Attributes = self.Attributes.Add( FunctionAttributeIndex.Function, attributes );
            return self;
        }

        /// <summary>Compatibility extension method to handle migrating code from older attribute handling</summary>
        /// <param name="self">Function to remove attributes from</param>
        /// <param name="kind">Attribute to remove</param>
        /// <returns>The function itself</returns>
        /// <remarks>
        /// Removes attributes from a given function itself (as opposed to the return or one of the function's parameters)
        /// This is equivalent to calling <see cref="RemoveAttribute{T}(T, FunctionAttributeIndex, AttributeKind)"/>
        /// with <see cref="FunctionAttributeIndex.Function"/> as the first parameter
        /// </remarks>
        public static Function RemoveAttribute( this Function self, AttributeKind kind )
        {
            if(self == null)
                throw new ArgumentNullException( nameof( self ) );

            self.Attributes = self.Attributes.Remove( FunctionAttributeIndex.Function, kind );
            return self;
        }

        /// <summary>Compatibility extension method to handle migrating code from older attribute handling</summary>
        /// <param name="self">Function to remove attributes from</param>
        /// <param name="name">Attribute to remove</param>
        /// <returns>The function itself</returns>
        /// <remarks>
        /// Removes attributes from a given function itself (as opposed to the return or one of the function's parameters)
        /// This is equivalent to calling <see cref="RemoveAttribute{T}(T, FunctionAttributeIndex, AttributeKind)"/>
        /// with <see cref="FunctionAttributeIndex.Function"/> as the first parameter
        /// </remarks>
        public static Function RemoveAttribute( this Function self, string name )
        {
            if(self == null)
                throw new ArgumentNullException( nameof( self ) );

            self.Attributes = self.Attributes.Remove( FunctionAttributeIndex.Function, name );
            return self;
        }

        public static T AddAttributes<T>( this T self, FunctionAttributeIndex index, params AttributeValue[] attributes )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Add( index, attributes );
            return self;
        }

        public static T AddAttributes<T>( this T self, FunctionAttributeIndex index, AttributeSet attributes )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Add( index, attributes[index] );
            return self;
        }

        public static T AddAttributes<T>( this T self, FunctionAttributeIndex index, IEnumerable<AttributeValue> attributes )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Add( index, attributes );
            return self;
        }

        public static T RemoveAttribute<T>( this T self, FunctionAttributeIndex index, AttributeKind kind )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Remove( index, kind );
            return self;
        }

        public static T RemoveAttribute<T>( this T self, FunctionAttributeIndex index, string name )
            where T : IAttributeSetContainer
        {
            self.Attributes = self.Attributes.Remove( index, name );
            return self;
        }
    }
}

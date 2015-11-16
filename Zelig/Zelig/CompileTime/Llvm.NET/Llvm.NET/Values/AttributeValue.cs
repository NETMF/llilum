using System;
using System.Diagnostics.CodeAnalysis;

namespace Llvm.NET.Values
{
    /// <summary>Single attribute for functions, function returns and function parameters</summary>
    public class AttributeValue
    {
        /// <summary>Creates a simple boolean attribute</summary>
        /// <param name="kind">Kind of attribute</param>
        public AttributeValue( AttributeKind kind )
        {
            if( AttributeHasValue( kind ) )
                throw new ArgumentException( $"Attribute {kind} requires a value", nameof( kind ) );

            Kind = kind;
            Name = null;
            StringValue = null;
            IntegerValue = null;
        }

        /// <summary>Creates an attribute with an integer value parameter</summary>
        /// <param name="kind">The kind of attribute</param>
        /// <param name="value">Value for the attribute</param>
        /// <remarks>
        /// <para>Not all attributes support a value and those that do don't all support
        /// a full 64bit value. The following table provdes the kinds of attributes
        /// accepting a value and the allowed size of the values.</para>
        /// <list type="table">
        /// <listheader><term><see cref="AttributeKind"/></term><term>Bit Length</term></listheader>
        /// <item><term><see cref="AttributeKind.Alignment"/></term><term>32</term></item>
        /// <item><term><see cref="AttributeKind.StackAlignment"/></term><term>32</term></item>
        /// <item><term><see cref="AttributeKind.Dereferenceable"/></term><term>64</term></item>
        /// <item><term><see cref="AttributeKind.DereferenceableOrNull"/></term><term>64</term></item>
        /// </list>
        /// </remarks>
        public AttributeValue( AttributeKind kind, UInt64 value )
        {
            AttributeSet.RangeCheckIntAttributeValue( kind, value );

            Kind = kind;
            Name = null;
            StringValue = null;
            IntegerValue = value;
        }

        /// <summary>Adds a valueless named attribute</summary>
        /// <param name="name">Attribute name</param>
        public AttributeValue( string name )
            : this( name, string.Empty )
        {
        }

        /// <summary>Adds a Target specific named attrinute with value</summary>
        /// <param name="name">Name of the attribute</param>
        /// <param name="value">Value of the attribute</param>
        public AttributeValue( string name, string value )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentException( "AttributeValue name cannot be null, Empty or all whitespace" );

            Kind = null;
            Name = name;
            StringValue = value;
            IntegerValue = null;
        }

        /// <summary>Kind of the attribute, or null for target specifc named attributes</summary>
        public AttributeKind? Kind { get; }

        /// <summary>Name of a named attribute or null for other kinds of attributes</summary>
        public string Name { get; }

        /// <summary>StringValue for named attributes with values</summary>
        public string StringValue { get; }

        /// <summary>Integer value of the attrinute or null if the attribute doens't have a value</summary>
        public UInt64? IntegerValue { get; }

        /// <summary>Flag to indicate if this attribute is a target specific string value</summary>
        public bool IsString => Name != null;

        /// <summary>Flag to indicate if this attribute has an integer attibrute</summary>
        public bool IsInt => Kind.HasValue && AttributeHasValue( Kind.Value );

        /// <summary>Flag to indicate if this attribute is a simple enumeration value</summary>
        public bool IsEnum => Kind.HasValue && !AttributeHasValue( Kind.Value );

        /// <summary>Determines if a given AttributeValue uses a parameter value</summary>
        /// <param name="kind">AttributeValue kind to test</param>
        /// <returns>true if the AttributeValue has a parameter</returns>
        /// <remarks>
        /// Most of the attributes are simple boolean flags, however some, in particular
        /// those dealing with sizes or alignment require a parameter value. This method
        /// is used to determine which category the AttributeValue falls into. Any AttributeValue 
        /// returning true requires an integer parameter and has special handling so 
        /// cannot be used in the AddAttributes() methods.
        /// </remarks>
        public static bool AttributeHasValue( AttributeKind kind )
        {
            switch( kind )
            {
            case AttributeKind.Alignment:
            case AttributeKind.Dereferenceable:
            case AttributeKind.DereferenceableOrNull:
            case AttributeKind.StackAlignment:
                return true;

            default:
                return false;
            }
        }

        /// <summary>Implicitly cast an <see cref="AttributeKind"/> to an <see cref="AttributeValue"/></summary>
        /// <param name="kind">Kind of attrinute to create</param>
        [SuppressMessage( "Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Available via constructor, this is for convenience" )]
        public static implicit operator AttributeValue( AttributeKind kind ) => new AttributeValue( kind );

        /// <summary>Implicitly cast a string to an named <see cref="AttributeValue"/></summary>
        /// <param name="kind">Attribute name</param>
        [SuppressMessage( "Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Available via constructor, this is for convenience" )]
        public static implicit operator AttributeValue( string kind ) => new AttributeValue( kind );
    }
}

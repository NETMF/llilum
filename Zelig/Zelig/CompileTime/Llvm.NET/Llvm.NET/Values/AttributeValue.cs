using System;

namespace Llvm.NET.Values
{
    /// <summary>Single attribute for Functions, Function returns and function parameters</summary>
    public struct AttributeValue
    {
        /// <summary>Creates a simple boolean attribute</summary>
        /// <param name="kind"></param>
        public AttributeValue( AttributeKind kind )
        {
            if( Function.AttributeHasValue( kind ) )
                throw new ArgumentException( $"Attribute {kind} requires a value", nameof( kind ) );

            Kind = kind;
            Name = null;
            StringValue = null;
            IntegerValue = null;
        }

        /// <summary>Creates an attribute with an integer value parameter</summary>
        /// <param name="kind"></param>
        /// <param name="value"></param>
        public AttributeValue( AttributeKind kind, UInt64 value )
        {
            Function.RangeCheckIntAttributeValue( kind, value );

            Kind = kind;
            Name = null;
            StringValue = null;
            IntegerValue = value;
        }

        public AttributeValue( string name )
            : this( name, null )
        {
        }

        public AttributeValue( string name, string value )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentException( "AttributeValue name cannot be null, Empty or all whitespace" );

            Kind = null;
            Name = name;
            StringValue = value;
            IntegerValue = null;
        }

        public AttributeKind? Kind { get; }

        public string Name { get; }

        public string StringValue { get; }

        public UInt64? IntegerValue { get; }

        public bool IsString => Name != null;

        public bool IsInt => Kind.HasValue && Function.AttributeHasValue( Kind.Value );

        public bool IsEnum => Kind.HasValue && !Function.AttributeHasValue( Kind.Value );

        public static implicit operator AttributeValue( AttributeKind kind ) => new AttributeValue( kind );
        public static implicit operator AttributeValue( string kind ) => new AttributeValue( kind );
    }
}

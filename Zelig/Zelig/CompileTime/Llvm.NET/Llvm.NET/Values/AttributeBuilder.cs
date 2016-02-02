using System;
using Llvm.NET.Native;

namespace Llvm.NET.Values
{
    /// <summary>Factory for building AttributeSets, which are otherwise immutable</summary>
    public sealed class AttributeBuilder
        : IDisposable
    {
        public AttributeBuilder()
        {
            BuilderHandle = NativeMethods.CreateAttributeBuilder( );
        }

        public AttributeBuilder( AttributeValue value )
        {
            BuilderHandle = NativeMethods.CreateAttributeBuilder2( value.NativeAttribute );
        }

        public AttributeBuilder( AttributeSet attributes, FunctionAttributeIndex index )
        {
            BuilderHandle = NativeMethods.CreateAttributeBuilder3( attributes.NativeAttributeSet, ( uint )index );
        }

        public void Dispose( )
        {
            BuilderHandle.Dispose( );
        }

        public AttributeBuilder Add( AttributeKind kind )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            if( kind.RequiresIntValue( ) )
                throw new ArgumentException( "Attribute requires a value" );

            NativeMethods.AttributeBuilderAddEnum( BuilderHandle, ( LLVMAttrKind )kind );
            return this;
        }

        public AttributeBuilder Add( AttributeValue value )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderAddAttribute( BuilderHandle, value.NativeAttribute );
            return this;
        }

        public AttributeBuilder Add( string name )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            return Add( name, string.Empty );
        }

        public AttributeBuilder Add( string name, string value )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderAddStringAttribute( BuilderHandle, name, value );
            return this;
        }

        public AttributeBuilder Remove( AttributeKind kind )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderRemoveEnum( BuilderHandle, ( LLVMAttrKind )kind );
            return this;
        }

        public AttributeBuilder Remove( AttributeSet attributes, FunctionAttributeIndex index )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderRemoveAttributes( BuilderHandle, attributes.NativeAttributeSet, (uint)index );
            return this;
        }

        public AttributeBuilder Remove( string name )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderRemoveAttribute( BuilderHandle, name );
            return this;
        }

        public AttributeBuilder Merge( AttributeBuilder other )
        {
            if(other == null)
                throw new ArgumentNullException( nameof( other ) );

            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderMerge( BuilderHandle, other.BuilderHandle );
            return this;
        }

        public AttributeBuilder Remove( AttributeBuilder other )
        {
            if(other == null)
                throw new ArgumentNullException( nameof( other ) );

            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            NativeMethods.AttributeBuilderRemoveBldr( BuilderHandle, other.BuilderHandle );
            return this;
        }

        public bool Overlaps( AttributeBuilder other )
        {
            if(other == null)
                throw new ArgumentNullException( nameof( other ) );

            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            return NativeMethods.AttributeBuilderOverlaps( BuilderHandle, other.BuilderHandle );
        }

        public bool Contains( AttributeKind kind )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            return NativeMethods.AttributeBuilderContainsEnum( BuilderHandle, ( LLVMAttrKind )kind );
        }

        public bool Contains( string name )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            return NativeMethods.AttributeBuilderContainsName( BuilderHandle, name );
        }

        public bool HasAnyAttributes => BuilderHandle.IsClosed ? false : (bool)NativeMethods.AttributeBuilderHasAnyAttributes( BuilderHandle );

        public bool HasAttributes( AttributeSet attributes, FunctionAttributeIndex index )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            return NativeMethods.AttributeBuilderHasAttributes( BuilderHandle, attributes.NativeAttributeSet, ( uint )index );
        }

        public bool IsEmpty
        {
            get
            {
                if( BuilderHandle.IsClosed )
                    return true; // don't throw an exception in a property getter method

                return !NativeMethods.AttributeBuilderHasTargetDependentAttrs( BuilderHandle )
                    && !NativeMethods.AttributeBuilderHasTargetIndependentAttrs( BuilderHandle );
            }
        }

        public AttributeSet ToAttributeSet( FunctionAttributeIndex index ) => ToAttributeSet( index, Context.CurrentContext );

        public AttributeSet ToAttributeSet( FunctionAttributeIndex index, Context context )
        {
            if( BuilderHandle.IsClosed )
                throw new ObjectDisposedException( nameof( AttributeBuilder ) );

            return new AttributeSet( context, index, this );
        }

        internal AttributeBuilderHandle BuilderHandle;
    }
}

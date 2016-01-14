using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Llvm.NET.Values
{
    /// <summary>AttributeSet for a <see cref="Function"/>, <see cref="Instructions.CallInstruction"/>, or <see cref="Instructions.Invoke"/> instruction</summary>
    /// <remarks>
    /// The underlying LLVM AttributeSet class is an immutable value type, unfortunately it includes a non-default constructor
    /// and therefore isn't a POD. However, it is trivially copy constructible and standard layout so this class simply wraps
    /// the LLVM AttributeSet class. All data allocated for an AttributeSet is owned by a <see cref="NET.Context"/>, which
    /// will handle cleaning it up on <see cref="NET.Context.Dispose()"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable", Justification = "LLVM Context owns the attribute set" )]
    public struct AttributeSet
        : IEquatable<AttributeSet>
    {
        public AttributeSet( Context context, FunctionAttributeIndex index, AttributeBuilder builder )
        {
            if( context == null )
                throw new ArgumentNullException( nameof( context ) );

            if( builder == null )
                throw new ArgumentNullException( nameof( builder ) );

            context.VerifyAsArg( nameof( context ) );
            NativeAttributeSet = NativeMethods.CreateAttributeSetFromBuilder( context.ContextHandle, ( uint )index, builder.BuilderHandle );
        }

        #region IEquatable
        public override int GetHashCode( ) => NativeAttributeSet.GetHashCode( );

        public override bool Equals( object obj )
        {
            if( ReferenceEquals( this, obj ) )
                return true;

            if( obj is AttributeSet )
                return Equals( ( LLVMMetadataRef )obj );

            if( obj is UIntPtr )
                return NativeAttributeSet.Equals( obj );

            return false;
        }

        public bool Equals( AttributeSet other ) => NativeAttributeSet == other.NativeAttributeSet;

        public static bool operator ==( AttributeSet left, AttributeSet right ) => left.Equals( right );
        public static bool operator !=( AttributeSet left, AttributeSet right ) => !left.Equals( right );
        #endregion

        /// <summary>Context used to intern this <see cref="AttributeSet"/></summary>
        /// <remarks>
        /// The Context returned will be <see cref="Context.CurrentThreadContext"/> if this <see cref="AttributeSet"/> is empty.
        /// This ensures that there is always a context available for creating new AttributeSets
        /// </remarks>
        public Context Context
        {
            get
            {
                if( NativeAttributeSet.IsNull( ) )
                    return Context.CurrentContext;

                return Context.GetContextFor( NativeMethods.AttributeSetGetContext( NativeAttributeSet ) );
            }
        }

        /// <summary>Retrieves an attributeSet filtered by the specified function index</summary>
        /// <param name="index">Index to filter on</param>
        /// <returns>A new <see cref="AttributeSet"/>with attributes from this set belonging to the specified index</returns>
        public AttributeSet this[ FunctionAttributeIndex index ]
        {
            get
            {
                // explicitly check Context so that getter method doesn't throw an exception
                if( Context == null || Context.IsDisposed || !HasAny( index ) )
                    return new AttributeSet( );

                return new AttributeSet( NativeMethods.AttributeGetAttributes( NativeAttributeSet, ( uint )index ) );
            }
        }

        /// <summary>Enumerates the <see cref="FunctionAttributeIndex"/> values that have attributes associated in this <see cref="AttributeSet"/></summary>
        public IEnumerable<FunctionAttributeIndex> Indexes
        {
            get
            {
                // explicitly check Context so that getter method doesn't throw an exception
                if( Context == null || Context.IsDisposed )
                    yield break;

                var numIndices = NativeMethods.AttributeSetGetNumSlots( NativeAttributeSet );
                for( uint i = 0; i < numIndices; ++i )
                    yield return ( FunctionAttributeIndex )NativeMethods.AttributeSetGetSlotIndex( NativeAttributeSet, i );
            }
        }

        /// <summary>Gets the attributes for the function return</summary>
        public AttributeSet ReturnAttributes => this[ FunctionAttributeIndex.ReturnType ];

        /// <summary>Gets the attributes for the function itself</summary>
        public AttributeSet FunctionAttributes => this[ FunctionAttributeIndex.Function ];

        /// <summary>Gets the attributes for a function parameter</summary>
        /// <param name="parameterIndex">Parameter index [ 0 based ]</param>
        /// <returns><see cref="AttributeSet"/>filtered for the specified parameter</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AttributeSet" )]
        public AttributeSet ParameterAttributes( int parameterIndex )
        {
            Context.VerifyOperation( );
            // prevent overflow on offset addition below
            if( parameterIndex > int.MaxValue - ( int )FunctionAttributeIndex.Parameter0 )
                throw new ArgumentOutOfRangeException( nameof( parameterIndex ) );

            var index = FunctionAttributeIndex.Parameter0 + parameterIndex;
            return new AttributeSet( NativeMethods.AttributeGetAttributes( NativeAttributeSet, ( uint )index ) );
        }

        /// <summary>Get LLVM formatted string representation of this <see cref="AttributeSet"/> for a given index</summary>
        /// <param name="index">Index to get the string for</param>
        /// <returns>Formatted string for the specified attribute index</returns>
        public string AsString( FunctionAttributeIndex index )
        {
            Context.VerifyOperation( );
            var msgPtr = NativeMethods.AttributeSetToString( NativeAttributeSet, ( uint )index, false );
            return NativeMethods.MarshalMsg( msgPtr );
        }

        public IEnumerable<IndexedAttributeValue> AllAttributes
        {
            get
            {
                var numSlots = NativeMethods.AttributeSetGetNumSlots( NativeAttributeSet );
                for( uint slot = 0; slot < numSlots; ++slot )
                {
                    var index = ( FunctionAttributeIndex )NativeMethods.AttributeSetGetSlotIndex( NativeAttributeSet, slot );
                    UIntPtr token = NativeMethods.AttributeSetGetIteratorStartToken( NativeAttributeSet, slot );
                    UIntPtr attr = UIntPtr.Zero;
                    do
                    {
                        attr = NativeMethods.AttributeSetIteratorGetNext( NativeAttributeSet, slot, ref token );
                        if( attr == UIntPtr.Zero )
                            break;

                        yield return new IndexedAttributeValue( index, new AttributeValue( attr ) );
                    } while( attr != UIntPtr.Zero );
                }
            }
        }

        /// <summary>Creates a formatted string representation of the entire <see cref="AttributeSet"/> (e.g. all indices)</summary>
        /// <returns>Formatted string representation of the <see cref="AttributeSet"/></returns>
        public override string ToString( )
        {
            Context.VerifyOperation( );
            var bldr = new StringBuilder( );
            var indexGroups = from attribute in AllAttributes
                              group attribute.Value.ToString( ) by attribute.Index;

            foreach( var group in indexGroups )
            {
                var values = string.Join( ", ", group );
                if( group.Key < FunctionAttributeIndex.Parameter0 )
                    bldr.AppendFormat( "[{0}: {1}]", group.Key, values );
                else
                    bldr.AppendFormat( "[Parameter{0}: {1}]", group.Key - FunctionAttributeIndex.Parameter0, values );
            }

            return bldr.ToString( );
        }

        /// <summary>Adds a set of attributes</summary>
        /// <param name="index">Index for the attribute</param>
        /// <param name="attributes">Attributes to add</param>
        public AttributeSet Add( FunctionAttributeIndex index, params AttributeValue[ ] attributes )
        {
            return Add( index, ( IEnumerable<AttributeValue> )attributes );
        }

        /// <summary>Produces a new <see cref="AttributeSet"/> that includes the attributes from this set along with additional attributes provided</summary>
        /// <param name="index"><see cref="FunctionAttributeIndex"/> to use for the new attributes</param>
        /// <param name="attributes">Collection of attributes to add</param>
        /// <returns>Newly created <see cref="AttributeSet"/>with the new attributes added</returns>
        public AttributeSet Add( FunctionAttributeIndex index, IEnumerable<AttributeValue> attributes )
        {
            if( attributes == null )
                throw new ArgumentNullException( nameof( attributes ) );

            Context.VerifyOperation( );
            using( var bldr = new AttributeBuilder( ) )
            {
                foreach( var attribute in attributes )
                    bldr.Add( attribute );

                return Add( index, bldr.ToAttributeSet( index, Context ) );
            }
        }

        /// <summary>Adds a single attribute</summary>
        /// <param name="index">Index for the attribute</param>
        /// <param name="attribute"><see cref="AttributeValue"/> kind to add</param>
        public AttributeSet Add( FunctionAttributeIndex index, AttributeValue attribute )
        {
            Context.VerifyOperation( );
            using( var bldr = new AttributeBuilder( ) )
            {
                bldr.Add( attribute );
                return Add( index, bldr.ToAttributeSet( index, Context ) );
            }
        }

        /// <summary>Adds Attributes from another attribute set along a given index</summary>
        /// <param name="index">Index to add attributes to and from</param>
        /// <param name="attributes"><see cref="AttributeSet"/> to add the attributes from</param>
        /// <returns>New <see cref="AttributeSet"/>Containing all attributes of this set plus any
        ///  attributes from <paramref name="attributes"/> along the specified <paramref name="index"/></returns>
        public AttributeSet Add( FunctionAttributeIndex index, AttributeSet attributes )
        {
            Context.VerifyOperation( );
            var nativeSet = NativeMethods.AttributeSetAddAttributes( NativeAttributeSet, Context.ContextHandle, ( uint )index, attributes.NativeAttributeSet );
            return new AttributeSet( nativeSet );
        }

        /// <summary>Removes the specified attribute from the attribute set</summary>
        public AttributeSet Remove( FunctionAttributeIndex index, AttributeKind kind )
        {
            Context.VerifyOperation( );
            var nativeSet = NativeMethods.AttributeSetRemoveAttributeKind( NativeAttributeSet, ( uint )index, ( LLVMAttrKind )kind );
            return new AttributeSet( nativeSet );
        }

        public AttributeSet Remove( FunctionAttributeIndex index, AttributeBuilder builder )
        {
            if( builder == null )
                throw new ArgumentNullException( nameof( builder ) );

            Context.VerifyOperation( );
            var nativeSet = NativeMethods.AttributeSetRemoveAttributeBuilder( NativeAttributeSet, Context.ContextHandle, ( uint )index, builder.BuilderHandle );
            return new AttributeSet( nativeSet );
        }

        /// <summary>Remove a target specific attribute</summary>
        /// <param name="index">Index for the attribute</param>
        /// <param name="name">Name of the attribute</param>
        public AttributeSet Remove( FunctionAttributeIndex index, string name )
        {
            Context.VerifyOperation( );
            using( var bldr = new AttributeBuilder( ) )
            {
                bldr.Add( name );
                return Remove( index, bldr );
            }
        }

        /// <summary>Get an integer value for an index</summary>
        /// <param name="index">Index to get the value from</param>
        /// <param name="kind"><see cref="AttributeKind"/> to get the value of (see remarks for supported attributes)</param>
        /// <returns>Value of the attribute</returns>
        /// <remarks>
        /// The only attributes supporting an integer value are <see cref="AttributeKind.Alignment"/>,
        /// <see cref="AttributeKind.StackAlignment"/>, <see cref="AttributeKind.Dereferenceable"/>,
        /// <see cref="AttributeKind.DereferenceableOrNull"/>.
        /// </remarks>
        public UInt64 GetAttributeValue( FunctionAttributeIndex index, AttributeKind kind )
        {
            Context.VerifyOperation( );
            kind.VerifyIntAttributeUsage( index, 0 );
            var value = new AttributeValue( NativeMethods.AttributeSetGetAttributeByKind( NativeAttributeSet, ( uint )index, ( LLVMAttrKind )kind ) );
            Debug.Assert( value.IntegerValue.HasValue );
            return value.IntegerValue.Value;
        }

        /// <summary>Tests if an <see cref="AttributeSet"/> has any attributes in the specified index</summary>
        /// <param name="index">Index for the attribute</param>
        public bool HasAny( FunctionAttributeIndex index )
        {
            Context.VerifyOperation( );
            return NativeMethods.AttributeSetHasAttributes( NativeAttributeSet, ( uint )index );
        }

        /// <summary>Tests if this attribute set has a given AttributeValue kind</summary>
        /// <param name="index">Index for the attribute</param>
        /// <param name="kind">Kind of AttributeValue to test for</param>
        /// <returns>true if the AttributeValue exists or false if not</returns>
        public bool Has( FunctionAttributeIndex index, AttributeKind kind )
        {
            Context.VerifyOperation( );
            return NativeMethods.AttributeSetHasAttributeKind( NativeAttributeSet, ( uint )index, ( LLVMAttrKind )kind );
        }

        /// <summary>Tests if this attribute set has a given string attribute</summary>
        /// <param name="index">Index for the attribute</param>
        /// <param name="name">Name of the attribute to test for</param>
        /// <returns>true if the attribute exists or false if not</returns>
        public bool Has( FunctionAttributeIndex index, string name )
        {
            Context.VerifyOperation( );
            return NativeMethods.AttributeSetHasStringAttribute( NativeAttributeSet, ( uint )index, name );
        }

        internal AttributeSet( UIntPtr nativeAttributeSet )
        {
            NativeAttributeSet = nativeAttributeSet;
        }

        // underlying native llvm::AttributeSet follows the basic Pointer to Implementation (PIMPL) pattern.
        // Thus, the total size of the structure is that of a pointer. While it isn't POD it is trivially
        // copy constructible and standard layout so it is safe to just use a pointer here and pass that to
        // native code as a simple IntPtr. The implementation for the PIMPL pattern is allocated and owned by
        // the context. Since AttributeSets are "interned" multiple AttributeSets may refer to the same internal
        // implementation instance. There is no dispose for the AttributeSet however as the context will take
        // care of cleaning them up when it is disposed. 
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "LLVM Context will clean up the allocations for this" )]
        internal readonly UIntPtr NativeAttributeSet;
    }
}

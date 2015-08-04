using System;
using System.Collections.Generic;

namespace Llvm.NET
{
    /// <summary>Interface to allow adding arbitrary named data items to an object</summary>
    /// <remarks>
    /// It is sometimes useful for code generation applications to attach some tool specific
    /// data to the LLVM objects created but that don't need representation as LLVM MetadataNodes.
    /// This interface provides such a facility.
    /// </remarks>
    public interface IExtensiblePropertyContainer
    {
        /// <summary>Try to get a value from the container</summary>
        /// <typeparam name="T">Type of value to retrieve</typeparam>
        /// <param name="id">id of the value to retrieve</param>
        /// <param name="value">value retrieved if present (or default value of type <typeparamref name="T"/> otherwise)</param>
        /// <returns>
        /// true if the item was found and it's type matches <typeparamref name="T"/> false otherwise.
        /// </returns>
        bool TryGetExtendedPropertyValue<T>( string id, out T value );

        /// <summary>Adds a value to the container</summary>
        /// <param name="id">Id of the value</param>
        /// <param name="value">value to add</param>
        /// <remarks>
        /// Adds the value with the specified id. If a value with the same id
        /// already exists and its type is the same as vlaue it is replaced. If
        /// the existing value is of a different type, then an ArgumentException
        /// is thrown.
        /// </remarks>
        void AddExtendedPropertyValue( string id, object value );
    }

    /// <summary>Provides consistent accessors for an extended property</summary>
    /// <typeparam name="T">Type of values stored in the property</typeparam>
    public class ExtensiblePropertyDescriptor<T>
    {
        /// <summary>Creates a new instance of a property descriptor</summary>
        /// <param name="name">Name of the pextended property</param>
        public ExtensiblePropertyDescriptor( string name )
        {
            Name = name;
        }

        /// <summary>Gets a value for the property from the container</summary>
        /// <param name="container">container</param>
        /// <returns>Value retrieved from the property or the default value of type <paramref name="T"/></returns>
        public T GetValueFrom( IExtensiblePropertyContainer container )
        {
            return GetValueFrom( container, default(T) );
        }

        /// <summary>Gets a value for the property from the container</summary>
        /// <param name="container">container</param>
        /// <param name="defaultValue">default value if the value is not yet present as an extended property</param>
        /// <returns>Value retrieved from the property or <paramref name="defaultValue"/> if it wasn't found</returns>
        /// <remarks>If the value didn't exist a new value with <paramref name="defaultValue"/> is added to the container</remarks>
        public T GetValueFrom( IExtensiblePropertyContainer container, T defaultValue )
        {
            return GetValueFrom( container, ( ) => defaultValue );
        }

        /// <summary>Gets a value for the property from the container</summary>
        /// <param name="container">container</param>
        /// <param name="lazyDefaultFactory">default value factory delegate to create the default value if the value is not yet present as an extended property</param>
        /// <returns>Value retrieved from the property or default value created by <paramref name="lazyDefaultFactory"/> if it wasn't found</returns>
        /// <remarks>If the value didn't exist a new value created by calling with <paramref name="lazyDefaultFactory"/> is added to the container</remarks>
        public T GetValueFrom( IExtensiblePropertyContainer container, Func<T> lazyDefaultFactory )
        {
            T retVal;
            if( container.TryGetExtendedPropertyValue( Name, out retVal ) )
                return retVal;

            retVal = lazyDefaultFactory( );
            container.AddExtendedPropertyValue( Name, retVal );
            return retVal;
        }

        /// <summary>Sets the value of an extended property in a container</summary>
        /// <param name="container">Container to set the value in</param>
        /// <param name="value">value of the property</param>
        public void SetValueIn( IExtensiblePropertyContainer container, T value )
        {
            container.AddExtendedPropertyValue( Name, value );
        }

        /// <summary>Name of the property</summary>
        public string Name { get; }
    }

    internal class ExtensiblePropertyContainer
        : IExtensiblePropertyContainer
    {
        public void AddExtendedPropertyValue( string id, object value )
        {
            lock(Items)
            {
                object currentValue;
                if( Items.TryGetValue( id, out currentValue ) )
                {
                    if( currentValue.GetType( ) != value.GetType() )
                        throw new ArgumentException( " Cannot change type of an extended property once set", nameof( value ) );
                }
                Items[ id ] = value;
            }
        }

        public bool TryGetExtendedPropertyValue<T>( string id, out T value )
        {
            value = default(T);
            object item;
            if( !Items.TryGetValue( id, out item ) )
                return false;

            if( !(item is T) )
                return false;

            value = ( T )item;
            return true;
        }

        Dictionary<string, object> Items = new Dictionary<string, object>();
    }
}

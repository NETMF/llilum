// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [Serializable]
////[TypeDependencyAttribute( "System.Collections.Generic.GenericEqualityComparer`1" )]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_EqualityComparer_of_T" )]
    [Microsoft.Zelig.Internals.TypeDependency( typeof(System.Collections.Generic.GenericEqualityComparer<>) )]
    public abstract class EqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
    {
        static EqualityComparer<T> defaultComparer;

        public static EqualityComparer<T> Default
        {
            get
            {
                EqualityComparer<T> comparer = defaultComparer;
                if(comparer == null)
                {
                    comparer = CreateComparer();
    
                    defaultComparer = comparer;
                }
    
                return comparer;
            }
        }

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern EqualityComparer<T> CreateComparer();
////    {
////        Type t = typeof( T );
////
////        // Specialize type byte for performance reasons
////        if(t == typeof( byte ))
////        {
////            return (EqualityComparer<T>)(object)(new ByteEqualityComparer());
////        }
////
////        // If T implements IEquatable<T> return a GenericEqualityComparer<T>
////        if(typeof( IEquatable<T> ).IsAssignableFrom( t ))
////        {
////            //return (EqualityComparer<T>)Activator.CreateInstance(typeof(GenericEqualityComparer<>).MakeGenericType(t));
////            return (EqualityComparer<T>)(typeof( GenericEqualityComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( t ));
////        }
////
////        // If T is a Nullable<U> where U implements IEquatable<U> return a NullableEqualityComparer<U>
////        if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof( Nullable<> ))
////        {
////            Type u = t.GetGenericArguments()[0];
////            if(typeof( IEquatable<> ).MakeGenericType( u ).IsAssignableFrom( u ))
////            {
////                //return (EqualityComparer<T>)Activator.CreateInstance(typeof(NullableEqualityComparer<>).MakeGenericType(u));
////                return (EqualityComparer<T>)(typeof( NullableEqualityComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( u ));
////            }
////        }
////
////        // Otherwise return an ObjectEqualityComparer<T>
////        return new ObjectEqualityComparer<T>();
////    }

        public abstract bool Equals( T x, T y );
        public abstract int GetHashCode( T obj );

        internal virtual int IndexOf( T[] array, T value, int startIndex, int count )
        {
            int endIndex = startIndex + count;
            for(int i = startIndex; i < endIndex; i++)
            {
                if(Equals( array[i], value )) return i;
            }
            return -1;
        }

        internal virtual int LastIndexOf( T[] array, T value, int startIndex, int count )
        {
            int endIndex = startIndex - count + 1;
            for(int i = startIndex; i >= endIndex; i--)
            {
                if(Equals( array[i], value )) return i;
            }
            return -1;
        }

        int IEqualityComparer.GetHashCode( object obj )
        {
            if(obj == null) return 0;

            if(obj is T) return GetHashCode( (T)obj );

            ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArgumentForComparison );
            return 0;
        }

        bool IEqualityComparer.Equals( object x, object y )
        {
            if(x == y) return true;

            if(x == null || y == null) return false;

            if((x is T) && (y is T)) return Equals( (T)x, (T)y );

            ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArgumentForComparison );
            return false;
        }
    }

    // The methods in this class look identical to the inherited methods, but the calls
    // to Equal bind to IEquatable<T>.Equals(T) instead of Object.Equals(Object)
    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_GenericEqualityComparer_of_T" )]
    internal class GenericEqualityComparer<T> : EqualityComparer<T> where T : IEquatable<T>
    {
        public override bool Equals( T x, T y )
        {
            if(x != null)
            {
                if(y != null) return x.Equals( y );
                return false;
            }

            if(y != null) return false;

            return true;
        }

        public override int GetHashCode( T obj )
        {
            if(obj == null) return 0;

            return obj.GetHashCode();
        }

        internal override int IndexOf( T[] array, T value, int startIndex, int count )
        {
            int endIndex = startIndex + count;
            if(value == null)
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(array[i] == null) return i;
                }
            }
            else
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(array[i] != null && array[i].Equals( value )) return i;
                }
            }
            return -1;
        }
    
        internal override int LastIndexOf( T[] array, T value, int startIndex, int count )
        {
            int endIndex = startIndex - count + 1;
            if(value == null)
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    if(array[i] == null) return i;
                }
            }
            else
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    if(array[i] != null && array[i].Equals( value )) return i;
                }
            }
            return -1;
        }
    
        // Equals method for the comparer itself.
        public override bool Equals( Object obj )
        {
            GenericEqualityComparer<T> comparer = obj as GenericEqualityComparer<T>;
    
            return comparer != null;
        }
    
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_NullableEqualityComparer_of_T" )]
    internal class NullableEqualityComparer<T> : EqualityComparer< Nullable<T> > where T : struct, IEquatable<T>
    {
        public override bool Equals( Nullable<T> x, Nullable<T> y )
        {
            if(x.HasValue)
            {
                if(y.HasValue) return x.value.Equals( y.value );
    
                return false;
            }
    
            if(y.HasValue) return false;
    
            return true;
        }
    
        public override int GetHashCode( Nullable<T> obj )
        {
            return obj.GetHashCode();
        }
    
        internal override int IndexOf( Nullable<T>[] array, Nullable<T> value, int startIndex, int count )
        {
            int endIndex = startIndex + count;
            if(!value.HasValue)
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(!array[i].HasValue) return i;
                }
            }
            else
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(array[i].HasValue && array[i].value.Equals( value.value )) return i;
                }
            }
            return -1;
        }
    
        internal override int LastIndexOf( Nullable<T>[] array, Nullable<T> value, int startIndex, int count )
        {
            int endIndex = startIndex - count + 1;
            if(!value.HasValue)
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    if(!array[i].HasValue) return i;
                }
            }
            else
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    if(array[i].HasValue && array[i].value.Equals( value.value )) return i;
                }
            }
            return -1;
        }
    
        // Equals method for the comparer itself.
        public override bool Equals( Object obj )
        {
            NullableEqualityComparer<T> comparer = obj as NullableEqualityComparer<T>;
            return comparer != null;
        }
    
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_ObjectEqualityComparer_of_T" )]
    internal class ObjectEqualityComparer<T> : EqualityComparer<T>
    {
        public override bool Equals( T x, T y )
        {
            if(x != null)
            {
                if(y != null) return x.Equals( y );

                return false;
            }

            if(y != null) return false;

            return true;
        }

        public override int GetHashCode( T obj )
        {
            if(obj == null) return 0;

            return obj.GetHashCode();
        }

        internal override int IndexOf( T[] array, T value, int startIndex, int count )
        {
            int endIndex = startIndex + count;
            if(value == null)
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(array[i] == null) return i;
                }
            }
            else
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(array[i] != null && array[i].Equals( value )) return i;
                }
            }
            return -1;
        }
    
        internal override int LastIndexOf( T[] array, T value, int startIndex, int count )
        {
            int endIndex = startIndex - count + 1;
            if(value == null)
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    if(array[i] == null) return i;
                }
            }
            else
            {
                for(int i = startIndex; i >= endIndex; i--)
                {
                    if(array[i] != null && array[i].Equals( value )) return i;
                }
            }
            return -1;
        }
    
        // Equals method for the comparer itself.
        public override bool Equals( Object obj )
        {
            ObjectEqualityComparer<T> comparer = obj as ObjectEqualityComparer<T>;
            return comparer != null;
        }
    
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }
    }

    // Performance of IndexOf on byte array is very important for some scenarios.
    // We will call the C runtime function memchr, which is optimized.
    [Serializable]
    internal class ByteEqualityComparer : EqualityComparer<byte>
    {
        public override bool Equals( byte x, byte y )
        {
            return x == y;
        }

        public override int GetHashCode( byte b )
        {
            return b.GetHashCode( );
        }
    }

////    internal unsafe override int IndexOf( byte[] array, byte value, int startIndex, int count )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(startIndex < 0)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if(count < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        if(count > array.Length - startIndex)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////        }
////
////        if(count == 0) return -1;
////
////        fixed(byte* pbytes = array)
////        {
////            return Buffer.IndexOfByte( pbytes, value, startIndex, count );
////        }
////    }
////
////    internal override int LastIndexOf( byte[] array, byte value, int startIndex, int count )
////    {
////        int endIndex = startIndex - count + 1;
////        for(int i = startIndex; i >= endIndex; i--)
////        {
////            if(array[i] == value) return i;
////        }
////        return -1;
////    }
////
////    // Equals method for the comparer itself.
////    public override bool Equals( Object obj )
////    {
////        ByteEqualityComparer comparer = obj as ByteEqualityComparer;
////        return comparer != null;
////    }
////
////    public override int GetHashCode()
////    {
////        return this.GetType().Name.GetHashCode();
////    }
    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_EqualityComparer_of_Enum" )]
    internal class EnumEqualityComparer<T> : EqualityComparer<T> where T : struct
    {
        public override bool Equals(T x, T y) {
            int x_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCast(x);
            int y_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCast(y);
            return x_final == y_final;
        }
 
        public override int GetHashCode(T obj) {
            int x_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCast(obj);
            return x_final.GetHashCode();
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_EqualityComparer_of_Enum_sbyte" )]
    internal class SByteEnumEqualityComparer<T> : EnumEqualityComparer<T> where T : struct
    {
        public override int GetHashCode( T obj )
        {
            int x_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCast(obj); 
            return ((sbyte)x_final).GetHashCode( );
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_EqualityComparer_of_Enum_short" )]
    internal class ShortEnumEqualityComparer<T> : EnumEqualityComparer<T> where T : struct
    {
        public override int GetHashCode( T obj )
        {
            int x_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCast(obj);
            return ((short)x_final).GetHashCode( );
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_EqualityComparer_of_Enum_long" )]
    internal class LongEnumEqualityComparer<T> : EqualityComparer<T> where T : struct
    {
        public override bool Equals(T x, T y) {
            long x_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCastLong(x); 
            long y_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCastLong(y);
            return x_final == y_final;
        }
 
        public override int GetHashCode(T obj) {
            long x_final = System.Runtime.CompilerServices.JitHelpers.UnsafeEnumCastLong(obj);
            return x_final.GetHashCode();
        }
 
    }
}


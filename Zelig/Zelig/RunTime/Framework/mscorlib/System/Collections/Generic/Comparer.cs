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
////[TypeDependencyAttribute( "System.Collections.Generic.GenericComparer`1" )]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_Comparer_of_T" )]
    [Microsoft.Zelig.Internals.TypeDependency( typeof(System.Collections.Generic.GenericComparer<>) )]
    public abstract class Comparer<T> : IComparer, IComparer<T>
    {
        static Comparer<T> defaultComparer;

        public static Comparer<T> Default
        {
            get
            {
                Comparer<T> comparer = defaultComparer;
                if(comparer == null)
                {
                    comparer = CreateComparer();
                    defaultComparer = comparer;
                }
                return comparer;
            }
        }

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern Comparer<T> CreateComparer();
////    {
////        Type t = typeof( T );
////
////        // If T implements IComparable<T> return a GenericComparer<T>
////        if(typeof( IComparable<T> ).IsAssignableFrom( t ))
////        {
////            //return (Comparer<T>)Activator.CreateInstance(typeof(GenericComparer<>).MakeGenericType(t));
////            return (Comparer<T>)(typeof( GenericComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( t ));
////        }
////
////        // If T is a Nullable<U> where U implements IComparable<U> return a NullableComparer<U>
////        if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof( Nullable<> ))
////        {
////            Type u = t.GetGenericArguments()[0];
////            if(typeof( IComparable<> ).MakeGenericType( u ).IsAssignableFrom( u ))
////            {
////                //return (Comparer<T>)Activator.CreateInstance(typeof(NullableComparer<>).MakeGenericType(u));
////                return (Comparer<T>)(typeof( NullableComparer<int> ).TypeHandle.CreateInstanceForAnotherGenericParameter( u ));
////            }
////        }
////
////        // Otherwise return an ObjectComparer<T>
////        return new ObjectComparer<T>();
////    }

        public abstract int Compare( T x, T y );

        int IComparer.Compare( object x, object y )
        {
            if(x == null) return y == null ? 0 : -1;
            if(y == null) return 1;

            if(x is T && y is T) return Compare( (T)x, (T)y );

            ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArgumentForComparison );
            return 0;
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_GenericComparer_of_T" )]
    internal class GenericComparer<T> : Comparer<T> where T : IComparable<T>
    {
        public override int Compare( T x, T y )
        {
            if(x != null)
            {
                if(y != null) return x.CompareTo( y );
    
                return 1;
            }
    
            if(y != null) return -1;
    
            return 0;
        }
    
        // Equals method for the comparer itself.
        public override bool Equals( Object obj )
        {
            GenericComparer<T> comparer = obj as GenericComparer<T>;
    
            return comparer != null;
        }
    
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }
    }

    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_NullableComparer_of_T" )]
    internal class NullableComparer<T> : Comparer< Nullable<T> > where T : struct, IComparable<T>
    {
        public override int Compare( Nullable<T> x, Nullable<T> y )
        {
            if(x.HasValue)
            {
                if(y.HasValue) return x.value.CompareTo( y.value );
    
                return 1;
            }
    
            if(y.HasValue) return -1;
    
            return 0;
        }
    
        // Equals method for the comparer itself.
        public override bool Equals( Object obj )
        {
            NullableComparer<T> comparer = obj as NullableComparer<T>;
    
            return comparer != null;
        }
    
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }
    }
    
    [Serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_ObjectComparer_of_T" )]
    internal class ObjectComparer<T> : Comparer<T>
    {
        public override int Compare( T x, T y )
        {
            return System.Collections.Comparer.Default.Compare( x, y );
        }
    
        // Equals method for the comparer itself.
        public override bool Equals( Object obj )
        {
            ObjectComparer<T> comparer = obj as ObjectComparer<T>;
    
            return comparer != null;
        }
    
        public override int GetHashCode()
        {
            return this.GetType().Name.GetHashCode();
        }
    }
}

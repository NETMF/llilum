// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
////using System.Security;

    // Warning, don't put System.Runtime.Serialization.On*Serializ*Attribute
    // on this class without first fixing ObjectClone::InvokeVtsCallbacks
    // Also, because we have special type system support that says a a boxed Nullable<T>
    // can be used where a boxed<T> is use, Nullable<T> can not implement any intefaces
    // at all (since T may not).   Do NOT add any interfaces to Nullable!
    // 
////[TypeDependencyAttribute( "System.Collections.Generic.NullableComparer`1" )]
////[TypeDependencyAttribute( "System.Collections.Generic.NullableEqualityComparer`1" )]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Nullable_of_T" )]
    [Microsoft.Zelig.Internals.TypeDependency( typeof(System.Collections.Generic.NullableComparer<>) )]
    [Microsoft.Zelig.Internals.TypeDependency( typeof(System.Collections.Generic.NullableEqualityComparer<>) )]
    [Serializable]
    public struct Nullable<T> where T : struct
    {
        private  bool hasValue;
        internal T    value;

        public Nullable( T value )
        {
            this.value    = value;
            this.hasValue = true;
        }

        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        public T Value
        {
            get
            {
                if(!HasValue)
                {
                    ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_NoValue );
                }

                return value;
            }
        }

        public T GetValueOrDefault()
        {
            return value;
        }

        public T GetValueOrDefault( T defaultValue )
        {
            return HasValue ? value : defaultValue;
        }

        public override bool Equals( object other )
        {
            if(!HasValue) return other == null;

            if(other == null) return false;

            return value.Equals( other );
        }

        public override int GetHashCode()
        {
            return HasValue ? value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return HasValue ? value.ToString() : "";
        }

        public static implicit operator Nullable<T>( T value )
        {
            return new Nullable<T>( value );
        }

        public static explicit operator T( Nullable<T> value )
        {
            return value.Value;
        }

        //--//

        //
        // ZELIG HACK: Once we have support for generic open classes, we can move this helper method to the Zelig code base.
        //
        public object Box()
        {
            return HasValue ? (object)value : null;
        }
    }

    public static class Nullable
    {
        public static int Compare<T>( Nullable<T> n1, Nullable<T> n2 ) where T : struct
        {
            if(n1.HasValue)
            {
                if(n2.HasValue) return Comparer<T>.Default.Compare( n1.value, n2.value );

                return 1;
            }

            if(n2.HasValue) return -1;

            return 0;
        }

        public static bool Equals<T>( Nullable<T> n1, Nullable<T> n2 ) where T : struct
        {
            if(n1.HasValue)
            {
                if(n2.HasValue) return EqualityComparer<T>.Default.Equals( n1.value, n2.value );

                return false;
            }

            if(n2.HasValue) return false;

            return true;
        }

////    // If the type provided is not a Nullable Type, return null.
////    // Otherwise, returns the underlying type of the Nullable type
////    public static Type GetUnderlyingType( Type nullableType )
////    {
////        if(nullableType == null)
////        {
////            throw new ArgumentNullException( "nullableType" );
////        }
////
////        Type result = null;
////
////        if(nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition)
////        {
////            // instantiated generic type only                
////            Type genericType = nullableType.GetGenericTypeDefinition();
////
////            if(genericType == typeof( Nullable<> ))
////            {
////                result = nullableType.GetGenericArguments()[0];
////            }
////        }
////
////        return result;
////    }
    }
}

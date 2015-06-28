//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class ReflectionHelper
    {
        public static System.Reflection.FieldInfo[] GetAllFields( Type t )
        {
            return t.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static );
        }

        public static System.Reflection.FieldInfo[] GetAllInstanceFields( Type t )
        {
            return t.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
        }

        public static System.Reflection.FieldInfo[] GetAllPublicInstanceFields( Type t )
        {
            return t.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
        }

        //--//

        public static System.Reflection.PropertyInfo[] GetAllProperties( Type t )
        {
            return t.GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static );
        }

        public static System.Reflection.PropertyInfo[] GetAllInstanceProperties( Type t )
        {
            return t.GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
        }

        public static System.Reflection.PropertyInfo[] GetAllPublicInstanceProperties( Type t )
        {
            return t.GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
        }

        //--//

        public static System.Reflection.MethodInfo[] GetAllMethods( Type t )
        {
            return t.GetMethods( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static );
        }

        public static System.Reflection.MethodInfo[] GetAllInstanceMethods( Type t )
        {
            return t.GetMethods( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );
        }

        public static System.Reflection.MethodInfo[] GetAllPublicInstanceMethods( Type t )
        {
            return t.GetMethods( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
        }

        //--//

        public static bool HasAttribute< T >( object obj      ,
                                              bool   fInherit )
        {
            return HasAttribute< T >( obj.GetType(), fInherit );
        }

        public static bool HasAttribute< T >( System.Reflection.MemberInfo mi       ,
                                              bool                         fInherit )
        {
            return mi.GetCustomAttributes( typeof( T ), fInherit ).Length > 0;
        }

        //--//

        public static T GetAttribute< T >( object obj      ,
                                           bool   fInherit )
        {
            return GetAttribute< T >( obj.GetType(), fInherit );
        }

        public static T GetAttribute< T >( System.Reflection.MemberInfo mi       ,
                                           bool                         fInherit )
        {
            T[] res = GetAttributes< T >( mi, fInherit );

            if(res.Length == 1)
            {
                return res[0];
            }

            return default(T);
        }

        //--//

        public static T[] GetAttributes< T >( object obj      ,
                                              bool   fInherit )
        {
            return GetAttributes< T >( obj.GetType(), fInherit );
        }

        public static T[] GetAttributes< T >( System.Reflection.MemberInfo mi       ,
                                              bool                         fInherit )
        {
            object[] objArray   = mi.GetCustomAttributes( typeof( T ), fInherit );
            T[]      typedArray = new T[objArray.Length];

            Array.Copy( objArray, typedArray, objArray.Length );

            return typedArray;
        }
    }
}

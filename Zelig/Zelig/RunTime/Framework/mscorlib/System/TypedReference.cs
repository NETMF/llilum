// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////////
// Void
//  This class represents the void return type
////////////////////////////////////////////////////////////////////////////////

namespace System
{
    // TypedReference is basically only ever seen on the call stack, and in param arrays.
    //  These are blob that must be dealt with by the compiler.
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
////using System.Security.Permissions;
////using System.Runtime.Versioning;
    using CultureInfo = System.Globalization.CultureInfo;
    using FieldInfo   = System.Reflection.FieldInfo;

    [Microsoft.Zelig.Internals.WellKnownType( "System_TypedReference" )]
    [CLSCompliant( false )]
    public struct TypedReference
    {
        private IntPtr Value;
        private IntPtr Type;

////    [CLSCompliant( false )]
////    [ReflectionPermission( SecurityAction.LinkDemand, MemberAccess = true )]
////    public static TypedReference MakeTypedReference( Object target, FieldInfo[] flds )
////    {
////        if(target == null)
////        {
////            throw new ArgumentNullException( "target" );
////        }
////
////        if(flds == null)
////        {
////            throw new ArgumentNullException( "flds" );
////        }
////
////        if(flds.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_ArrayZeroError" ) );
////        }
////        else
////        {
////            RuntimeFieldHandle[] fields = new RuntimeFieldHandle[flds.Length];
////
////            // For proper handling of Nullable<T> don't change GetType() to something like 'IsAssignableFrom'
////            // Currently we can't make a TypedReference to fields of Nullable<T>, which is fine.  
////            Type targetType = target.GetType();
////            for(int i = 0; i < flds.Length; i++)
////            {
////                FieldInfo field = flds[i];
////                if(!(field is RuntimeFieldInfo))
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Argument_MustBeRuntimeFieldInfo" ) );
////                }
////                else if(field.IsInitOnly || field.IsStatic)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Argument_TypedReferenceInvalidField" ) );
////                }
////
////                if(targetType != field.DeclaringType && !targetType.IsSubclassOf( field.DeclaringType ))
////                {
////                    throw new MissingMemberException( Environment.GetResourceString( "MissingMemberTypeRef" ) );
////                }
////
////                Type fieldType = field.FieldType;
////                if(fieldType.IsPrimitive)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_TypeRefPrimitve" ) );
////                }
////
////                if(i < flds.Length - 1)
////                {
////                    if(!fieldType.IsValueType)
////                    {
////                        throw new MissingMemberException( Environment.GetResourceString( "MissingMemberNestErr" ) );
////                    }
////                }
////
////                fields[i]  = field.FieldHandle;
////                targetType = fieldType;
////            }
////
////            TypedReference result = new TypedReference();
////            // reference to TypedReference is banned, so have to pass result as pointer
////            unsafe
////            {
////                InternalMakeTypedReference( &result, target, fields, targetType.TypeHandle );
////            }
////
////            return result;
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    // reference to TypedReference is banned, so have to pass result as pointer
////    private unsafe static extern void InternalMakeTypedReference( void* result, Object target, RuntimeFieldHandle[] flds, RuntimeTypeHandle lastFieldType );

        public override int GetHashCode()
        {
            if(Type == IntPtr.Zero)
            {
                return 0;
            }
            else
            {
                return __reftype(this).GetHashCode();
            }
        }

        public override bool Equals( Object o )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_NYI" ) );
#else
            throw new NotSupportedException();
#endif
        }

        public unsafe static Object ToObject( TypedReference value )
        {
            return InternalToObject( &value );
        }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal unsafe extern static Object InternalToObject( void* value );

        internal bool IsNull
        {
            get
            {
                return Value.IsNull() && Type.IsNull();
            }
        }

        public static Type GetTargetType( TypedReference value )
        {
            return __reftype(value);
        }

        public static RuntimeTypeHandle TargetTypeToken( TypedReference value )
        {
            return __reftype(value).TypeHandle;
        }

        //  This may cause the type to be changed.
        [CLSCompliant( false )]
        public unsafe static void SetTypedReference( TypedReference target, Object value )
        {
            InternalSetTypedReference( &target, value );
        }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal unsafe extern static void InternalSetTypedReference( void* target, Object value );

        // Internal method for getting a raw pointer for handles in JitHelpers.
        // The reference has to point into a local stack variable in order so it can not be moved by the GC.
        internal IntPtr GetPointerOnStack()
        {
#if _DEBUG
            BCLDebug.Assert(IsAddressInStack(Value), "Pointer not in the stack!");
#endif
            return Value;
        }

#if _DEBUG
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static bool IsAddressInStack(IntPtr ptr);
#endif
    }

}

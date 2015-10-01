// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System.Runtime.CompilerServices
{
    using System;
    using System.Reflection;

    // This Enum matchs the miImpl flags defined in corhdr.h. It is used to specify
    // certain method properties.

    [Flags]
    [Serializable]
    public enum MethodImplOptions
    {
        Unmanaged    = MethodImplAttributes.Unmanaged   ,
        ForwardRef   = MethodImplAttributes.ForwardRef  ,
        PreserveSig  = MethodImplAttributes.PreserveSig ,
        InternalCall = MethodImplAttributes.InternalCall,
        Synchronized = MethodImplAttributes.Synchronized,
        NoInlining   = MethodImplAttributes.NoInlining  ,
    }

    [Serializable]
    public enum MethodCodeType
    {
        IL      = System.Reflection.MethodImplAttributes.IL     ,
        Native  = System.Reflection.MethodImplAttributes.Native ,
        /// <internalonly/>
        OPTIL   = System.Reflection.MethodImplAttributes.OPTIL  ,
        Runtime = System.Reflection.MethodImplAttributes.Runtime,
    }

    // Custom attribute to specify additional method properties.
    [Serializable]
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false )]
    sealed public class MethodImplAttribute : Attribute
    {
        internal MethodImplOptions m_val;
        public   MethodCodeType    MethodCodeType;

        internal MethodImplAttribute( MethodImplAttributes methodImplAttributes )
        {
            MethodImplOptions all = MethodImplOptions.Unmanaged | MethodImplOptions.ForwardRef | MethodImplOptions.PreserveSig |
                                    MethodImplOptions.InternalCall | MethodImplOptions.Synchronized | MethodImplOptions.NoInlining;

            m_val = ((MethodImplOptions)methodImplAttributes) & all;
        }

        public MethodImplAttribute( MethodImplOptions methodImplOptions )
        {
            m_val = methodImplOptions;
        }

////    public MethodImplAttribute( short value )
////    {
////        m_val = (MethodImplOptions)value;
////    }

        public MethodImplAttribute()
        {
        }

        public MethodImplOptions Value
        {
            get
            {
                return m_val;
            }
        }
    }
}

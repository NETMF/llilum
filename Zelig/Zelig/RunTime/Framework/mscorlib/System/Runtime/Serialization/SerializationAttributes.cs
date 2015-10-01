// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class: OptionallySerializableAttribute
**
**
** Purpose: Various Attributes for Serialization
**
**
============================================================*/
namespace System.Runtime.Serialization
{
    using System.Reflection;

    [AttributeUsage( AttributeTargets.Field, Inherited = false )]
    public sealed class OptionalFieldAttribute : Attribute
    {
        int versionAdded = 1;

        public OptionalFieldAttribute() { }
    
        public int VersionAdded
        {
            get
            {
                return this.versionAdded;
            }

            set
            {
                if(value < 1)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Serialization_OptionalFieldVersionValue" ) );
#else
                    throw new ArgumentException();
#endif
                }

                this.versionAdded = value;
            }
        }
    }
    
    [AttributeUsage( AttributeTargets.Method, Inherited = false )]
    public sealed class OnSerializingAttribute : Attribute
    {
    }
    
    [AttributeUsage( AttributeTargets.Method, Inherited = false )]
    public sealed class OnSerializedAttribute : Attribute
    {
    }
    
    [AttributeUsage( AttributeTargets.Method, Inherited = false )]
    public sealed class OnDeserializingAttribute : Attribute
    {
    }
    
    [AttributeUsage( AttributeTargets.Method, Inherited = false )]
    public sealed class OnDeserializedAttribute : Attribute
    {
    }
    
}

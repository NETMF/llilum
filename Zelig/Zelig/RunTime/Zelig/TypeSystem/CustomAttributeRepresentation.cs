//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;


    public sealed class CustomAttributeRepresentation
    {
        public static readonly CustomAttributeRepresentation[] SharedEmptyArray = new CustomAttributeRepresentation[0];

        //
        // State
        //

        private MethodRepresentation m_constructor;
        private object[]             m_fixedArgsValues;
        private string[]             m_namedArgs;
        private object[]             m_namedArgsValues;

        //
        // Constructor Methods
        //

        public CustomAttributeRepresentation( MethodRepresentation constructor     ,
                                              object[]             fixedArgsValues ,
                                              string[]             namedArgs       ,
                                              object[]             namedArgsValues )
        {
            CHECKS.ASSERT( constructor != null, "Cannot create CustomAttributeRepresentation without supporting metadata" );

            m_constructor     = constructor;
            m_fixedArgsValues = fixedArgsValues;
            m_namedArgs       = namedArgs;
            m_namedArgsValues = namedArgsValues;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is CustomAttributeRepresentation)
            {
                CustomAttributeRepresentation other = (CustomAttributeRepresentation)obj;

                if(m_constructor == other.m_constructor)
                {
                    if(ArrayUtility.ArrayEquals( m_fixedArgsValues, other.m_fixedArgsValues ) &&
                       ArrayUtility.ArrayEquals( m_namedArgs      , other.m_namedArgs       ) &&
                       ArrayUtility.ArrayEquals( m_namedArgsValues, other.m_namedArgsValues )  )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_constructor.GetHashCode();
        }

        //--//

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_constructor     );
            context.Transform( ref m_fixedArgsValues );
            context.Transform( ref m_namedArgs       );
            context.Transform( ref m_namedArgsValues );

            context.Pop();
        }

        //--//

        public bool HasNamedArg( string name )
        {
            return GetNamedArgIndex( name ) >= 0;
        }

        public object GetNamedArg( string name )
        {
            int i = GetNamedArgIndex( name );
            if(i >= 0)
            {
                return m_namedArgsValues[i];
            }

            return null;
        }

        public bool TryToGetNamedArg< T >(     string name ,
                                           out T      val  )
        {
            return TryToGetNamedArg( name, out val, default(T) );
        }

        public bool TryToGetNamedArg< T >(     string name       ,
                                           out T      val        ,
                                               T      defaultVal )
        {
            int i = GetNamedArgIndex( name );
            if(i >= 0)
            {
                val = (T)m_namedArgsValues[i];

                return true;
            }
            else
            {
                val = defaultVal;

                return false;
            }
        }

        public T GetNamedArg< T >( string name )
        {
            object val = GetNamedArg( name );

            if(val != null)
            {
                return (T)val;
            }
            else
            {
                return default(T);
            }
        }

        public T GetNamedArg< T >( string name       ,
                                   T      defaultVal )
        {
            object val = GetNamedArg( name );

            if(val != null)
            {
                return (T)val;
            }
            else
            {
                return defaultVal;
            }
        }

        private int GetNamedArgIndex( string name )
        {
            for(int i = 0; i < m_namedArgs.Length; i++)
            {
                if(m_namedArgs[i] == name)
                {
                    return i;
                }
            }

            return -1;
        }

        //--//

        //
        // Access Methods
        //

        public MethodRepresentation Constructor
        {
            get
            {
                return m_constructor;
            }
        }

        public object[] FixedArgsValues
        {
            get
            {
                return m_fixedArgsValues;
            }
        }

        public string[] NamedArgs
        {
            get
            {
                return m_namedArgs;
            }
        }

        public object[] NamedArgsValues
        {
            get
            {
                return m_namedArgsValues;
            }
        }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataCustomAttribute : MetaDataObject,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataAssembly       m_owner;

        internal          MetaDataMethodAbstract m_constructor;

        internal          Object[]               m_fixedArgs;
        internal          NamedArg[]             m_namedArgs;

        //
        // Constructor Methods
        //

        internal MetaDataCustomAttribute( MetaDataAssembly owner ,
                                          int              token ) : base( token )
        {
            m_owner = owner;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataCustomAttribute) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataCustomAttribute other = (MetaDataCustomAttribute)obj;

                if(m_owner       == other.m_owner       &&
                   m_constructor == other.m_constructor  )
                {
                    if(ArrayUtility.ArrayEquals( m_fixedArgs, other.m_fixedArgs ) &&
                       ArrayUtility.ArrayEquals( m_namedArgs, other.m_namedArgs )  )
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

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return (MetaDataObject)m_owner.MakeUnique( this );
        }

        //
        // Access Methods
        //

        public MetaDataAssembly Owner
        {
            get
            {
                return m_owner;
            }
        }

        public MetaDataMethodAbstract Constructor
        {
            get
            {
                return m_constructor;
            }
        }

        public Object[] FixedArgs
        {
            get
            {
                return m_fixedArgs;
            }
        }

        public NamedArg[] NamedArgs
        {
            get
            {
                return m_namedArgs;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataCustomAttribute(" );

            sb.Append( m_constructor );
            sb.Append( "," );

            if(m_fixedArgs != null && m_fixedArgs.Length > 0)
            {
                sb.Append( "FixedArgs(" );
                for(int i = 0; i < m_fixedArgs.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( m_fixedArgs[i] );
                }
                sb.Append( ")," );
            }

            if(m_namedArgs != null && m_namedArgs.Length > 0)
            {
                sb.Append( "NamedArgs(" );
                for(int i = 0; i < m_namedArgs.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( m_namedArgs[i] );
                }
                sb.Append( ")," );
            }

            sb.Append( ")" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            MetaDataMethodBase md = (MetaDataMethodBase)m_constructor;

            writer.WriteLine( ".custom {0} {1}", TokenToString( m_token ), md.DumpSignature( writer, null, true ) );
        }

        //--//

        public class NamedArg
        {
            //
            // State
            //

            private bool               m_isFieldArg;
            private int                m_arrayDim;
            private String             m_name;
            private SerializationTypes m_type;
            private Object             m_value;

            //
            // Constructor Methods
            //

            internal NamedArg( bool               isFieldArg ,
                               int                arrayDim   ,
                               String             name       ,
                               SerializationTypes type       ,
                               Object             value      )
            {
                m_isFieldArg = isFieldArg;
                m_arrayDim   = arrayDim;
                m_type       = type;
                m_name       = name;
                m_value      = value;
            }

            public bool IsFieldArg
            {
                get
                {
                    return m_isFieldArg;
                }
            }

            public bool IsArray
            {
                get
                {
                    return m_arrayDim >= 0;
                }
            }

            public int ArrayDim
            {
                get
                {
                    if(m_arrayDim < 0)
                    {
                        throw new Exception( "NamedArg is not an array" );
                    }

                    return m_arrayDim;
                }
            }

            public SerializationTypes Type
            {
                get
                {
                    return m_type;
                }
            }

            public String Name
            {
                get
                {
                    return m_name;
                }
            }

            public Object Value
            {
                get
                {
                    return m_value;
                }
            }

            public override String ToString()
            {
                return "NamedArg(" + m_name + ":" + m_type + ((m_arrayDim < 0) ? "" : ("[" + m_arrayDim + "]")) + "->" + m_value + ")";
            }
        }
    }
}

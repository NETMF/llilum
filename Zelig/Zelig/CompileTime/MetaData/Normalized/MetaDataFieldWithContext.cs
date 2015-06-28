//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataFieldWithContext : MetaDataFieldAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionAbstract m_contextType;
        internal readonly MetaDataField                  m_baseField;

        //
        // Constructor Methods
        //

        internal MetaDataFieldWithContext( MetaDataTypeDefinitionAbstract contextType ,
                                           MetaDataField                  baseField   ) : base( 0 )
        {
            m_contextType = contextType;
            m_baseField   = baseField;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataFieldWithContext) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataFieldWithContext other = (MetaDataFieldWithContext)obj;

                if(m_contextType == other.m_contextType &&
                   m_baseField   == other.m_baseField    )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_baseField.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool IsOpenField
        {
            get
            {
                if(m_baseField.IsOpenField)
                {
                    if(m_contextType != null)
                    {
                        if(m_contextType.IsOpenType == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_baseField.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_baseField.UsesMethodParameters;
            }
        }

        internal override MetaDataObject MakeUnique()
        {
            return m_contextType.MakeUnique( (IMetaDataUnique)this );
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract ContextType
        {
            get
            {
                return m_contextType;
            }
        }

        public MetaDataField BaseField
        {
            get
            {
                return m_baseField;
            }
        }

        //
        // Debug Methods
        //

        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append( "{" );
                sb.Append( m_contextType.FullName );

                sb.Append( "}" );

                sb.Append( m_baseField.FullName );

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return "MetaDataFieldWithContext(" + this.FullName + ")";
        }

        public override String ToString( IMetaDataDumper context )
        {
            string res;

            context = context.PushContext( m_contextType );

            res = m_baseField.ToString( context );

            return res;
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}

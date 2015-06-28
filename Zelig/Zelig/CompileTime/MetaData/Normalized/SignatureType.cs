//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class SignatureType : MetaDataSignature,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MetaDataTypeDefinitionAbstract m_type;

        //
        // Constructor Methods
        //

        internal SignatureType( int token ) : base( token )
        {
        }

        private SignatureType( MetaDataTypeDefinitionAbstract type ) : base( 0 )
        {
            m_type = (MetaDataTypeDefinitionAbstract)type.MakeUnique();
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is SignatureType) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                SignatureType other = (SignatureType)obj;

                if(m_type == other.m_type)
                {
                    if(base.InnerEquals( other ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_type.GetHashCode();
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return m_type.MakeUnique( this );
        }

        public static SignatureType Create( MetaDataTypeDefinitionAbstract type )
        {
            return new SignatureType( type );
        }

        public static SignatureType CreateUnique( MetaDataTypeDefinitionAbstract type )
        {
            return (SignatureType)type.MakeUnique( new SignatureType( type ) );
        }

        //--//

        public bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                           MetaDataMethodAbstract         methodContext ,
                           SignatureType                  sig           )
        {
            return m_type.Match( typeContext, methodContext, sig.m_type );
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract Type
        {
            get
            {
                return m_type;
            }
        }

        public bool IsOpenType
        {
            get
            {
                return m_type.IsOpenType;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_type.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_type.UsesMethodParameters;
            }
        }

        //
        // Debug Methods
        //

        public string FullNameWithAbbreviation
        {
            get
            {
                return m_type.FullNameWithAbbreviation;
            }
        }

        public string ToStringWithAbbreviations( IMetaDataDumper context )
        {
            return m_type.ToStringWithAbbreviations( context );
        }

        public String FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append( m_type.FullName );

                return sb.ToString();
            }
        }

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "SignatureType(" );

            sb.Append( this.FullName );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}

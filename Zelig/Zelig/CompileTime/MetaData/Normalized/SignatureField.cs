//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class SignatureField : MetaDataSignature,
        IMetaDataUnique
    {
        //
        // State
        //

        internal SignatureType m_type;

        //
        // Constructor Methods
        //

        internal SignatureField( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is SignatureField) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                SignatureField other = (SignatureField)obj;

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
            return m_type.Type.MakeUnique( this );
        }

        //
        // Helper Methods
        //

        public bool Match( SignatureField sig )
        {
            return m_type.Match( null, null, sig.m_type );
        }

        //
        // Access Methods
        //

        public SignatureType TypeSignature
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
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "SignatureField(" );

            sb.Append( this.FullName );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}

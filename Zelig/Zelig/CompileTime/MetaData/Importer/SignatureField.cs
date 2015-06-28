//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public class SignatureField : Signature,
        IMetaDataNormalizeSignature
    {
        //
        // State
        //

        private readonly SignatureType m_type;

        //
        // Constructor Methods
        //

        internal SignatureField( SignatureType type )
        {
            m_type = type;
        }

        internal static SignatureField Parse( Parser      parser ,
                                              ArrayReader reader )
        {
            Signature sig = Signature.ParseUnknown( parser, reader );
            if(sig is SignatureField)
            {
                return (SignatureField)sig;
            }

            throw IllegalMetaDataFormatException.Create( "Not a field signature: {0}", sig );
        }

        //
        // IMetaDataNormalizeSignature methods
        //

        Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.SignatureField res = new Normalized.SignatureField( 0 );

            context.GetNormalizedSignature( m_type, out res.m_type, MetaDataNormalizationMode.Default );

            return (Normalized.MetaDataSignature)res.MakeUnique();
        }

        //
        // Access Methods
        //

        public SignatureType FieldType
        {
            get
            {
                return m_type;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "SignatureField(" + m_type + ")";
        }
    }
}

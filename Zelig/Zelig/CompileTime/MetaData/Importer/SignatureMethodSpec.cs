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
    public class SignatureMethodSpec : Signature,
        IMetaDataNormalizeSignature
    {
        //
        // State
        //

        private readonly SignatureType[] m_genericParameters;

        //
        // Constructor Methods
        //

        internal SignatureMethodSpec( SignatureType[] genericParameters )
        {
            m_genericParameters = genericParameters;
        }

        internal static SignatureMethodSpec Parse( Parser      parser ,
                                                   ArrayReader reader )
        {
            return SignatureMethodSpec.Parse( parser, reader, ParseFlags.MustBeWholeSignature );
        }

        internal static SignatureMethodSpec Parse( Parser               parser ,
                                                   ArrayReader          reader ,
                                                   Signature.ParseFlags flags  )
        {
            Signature sig = Signature.ParseUnknown( parser, reader, flags );
            if(sig is SignatureMethodSpec)
            {
                return (SignatureMethodSpec)sig;
            }

            throw IllegalMetaDataFormatException.Create( "Not a method spec signature: {0}", sig );
        }

        //
        // IMetaDataNormalizeSignature methods
        //

        Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.SignatureMethod res = new Normalized.SignatureMethod( 0 );

            res.m_callingConvention = 0;
            res.m_returnType        = null;

            context.GetNormalizedSignatureArray( m_genericParameters, out res.m_parameters, MetaDataNormalizationMode.Default );

            return res;
        }

        //
        // Access Methods
        //

        public SignatureType[] GenericParameters
        {
            get
            {
                return m_genericParameters;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder( "SignatureMethodSpec(" );

            sb.Append( "generic parameters(" );

            for(int i = 0; i < m_genericParameters.Length; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                sb.Append( m_genericParameters[i] );
            }

            sb.Append( "))" );

            return sb.ToString();
        }
    }
}

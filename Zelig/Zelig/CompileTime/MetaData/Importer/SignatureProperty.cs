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
    public class SignatureProperty : Signature,
        IMetaDataNormalizeSignature
    {
        //
        // State
        //

        private readonly SignatureType   m_returnType;
        private readonly SignatureType[] m_parameters;

        //
        // Constructor Methods
        //

        internal SignatureProperty( SignatureType   returnType ,
                                    SignatureType[] parameters )
        {
            m_returnType = returnType;
            m_parameters = parameters;
        }

        internal static SignatureProperty Parse( Parser      parser ,
                                                 ArrayReader reader )
        {
            Signature sig = Signature.ParseUnknown( parser, reader );
            if(sig is SignatureProperty)
            {
                return (SignatureProperty)sig;
            }

            throw IllegalMetaDataFormatException.Create( "Not a property signature: {0}", sig );
        }

        //
        // IMetaDataNormalizeSignature methods
        //

        Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.SignatureMethod res = new Normalized.SignatureMethod( 0 );

            res.m_callingConvention = Normalized.SignatureMethod.CallingConventions.Default;

            context.GetNormalizedSignature     ( m_returnType, out res.m_returnType, MetaDataNormalizationMode.Default );
            context.GetNormalizedSignatureArray( m_parameters, out res.m_parameters, MetaDataNormalizationMode.Default );

            return (Normalized.SignatureMethod)res.MakeUnique();
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder( "SignatureProperty(" );

            sb.Append( m_returnType );

            if(m_parameters.Length > 0)
            {
                sb.Append( ",(" );
                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(i != 0)
                    {
                        sb.Append( "," );
                    }

                    sb.Append( m_parameters[i] );
                }
                sb.Append( ")" );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

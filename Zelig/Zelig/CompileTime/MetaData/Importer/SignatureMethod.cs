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
    public class SignatureMethod : Signature,
        IMetaDataNormalizeSignature
    {
        //
        // State
        //

        private readonly CallingConventions m_callingConvention;
        private readonly uint               m_genericParameterCount;
        private readonly int                m_sentinelLocation;
        private readonly SignatureType      m_returnType;
        private readonly SignatureType[]    m_parameters;

        //
        // Constructor Methods
        //

        internal SignatureMethod( CallingConventions callingConvention     ,
                                  uint               genericParameterCount ,
                                  int                sentinelLocation      ,
                                  SignatureType      returnType            ,
                                  SignatureType[]    parameters            )
        {
            m_callingConvention     = callingConvention;
            m_genericParameterCount = genericParameterCount;
            m_returnType            = returnType;
            m_parameters            = parameters;
            m_sentinelLocation      = sentinelLocation;
        }

        internal static SignatureMethod Parse( Parser      parser ,
                                               ArrayReader reader )
        {
            return SignatureMethod.Parse( parser, reader, ParseFlags.MustBeWholeSignature );
        }

        internal static SignatureMethod Parse( Parser               parser ,
                                               ArrayReader          reader ,
                                               Signature.ParseFlags flags  )
        {
            Signature sig = Signature.ParseUnknown( parser, reader, flags );
            if(sig is SignatureMethod)
            {
                return (SignatureMethod)sig;
            }

            throw IllegalMetaDataFormatException.Create( "Not a method signature: {0}", sig );
        }

        //
        // IMetaDataNormalizeSignature methods
        //

        Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.SignatureMethod res = new Normalized.SignatureMethod( 0 );

            res.m_callingConvention = (Normalized.SignatureMethod.CallingConventions)m_callingConvention;

            context.GetNormalizedSignature     ( m_returnType, out res.m_returnType, MetaDataNormalizationMode.Default );
            context.GetNormalizedSignatureArray( m_parameters, out res.m_parameters, MetaDataNormalizationMode.Default );

            return (Normalized.MetaDataSignature)res.MakeUnique();
        }

        //
        // Access Methods
        //

        public CallingConventions CallingConvention
        {
            get
            {
                return m_callingConvention;
            }
        }

        public uint GenericParameterCount
        {
            get
            {
                return m_genericParameterCount;
            }
        }

        public SignatureType ReturnType
        {
            get
            {
                return m_returnType;
            }
        }

        public SignatureType[] Parameters
        {
            get
            {
                return m_parameters;
            }
        }

        public int SentinelLocation
        {
            get
            {
                return m_sentinelLocation;
            }
        }

        // other handy methods

        public bool HasThis
        {
            get
            {
                return ((((int)m_callingConvention) & ((int)Signature.CallingConventions.HasThis)) != 0);
            }
        }

        public bool ExplicitThis
        {
            get
            {
                return ((((int)m_callingConvention) & ((int)Signature.CallingConventions.ExplicitThis)) != 0);
            }
        }

        public bool NoThis
        {
            get
            {
                return !(HasThis || ExplicitThis);
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder( "SignatureMethod(" );

            sb.Append( m_callingConvention );
            sb.Append( "," );

            if((m_callingConvention & CallingConventions.Generic) != 0)
            {
                sb.Append( "genParamCount=" );
                sb.Append( m_genericParameterCount );
                sb.Append( "," );
            }

            sb.Append( m_returnType );
            sb.Append( "," );
            sb.Append( "parameters(" );

            for(int i = 0; i < m_parameters.Length; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                if(m_sentinelLocation == i)
                {
                    sb.Append( "<Sentinel>," );
                }

                sb.Append( m_parameters[i] );
            }

            sb.Append( "))" );

            return sb.ToString();
        }
    }
}

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
    public class SignatureTypeSpec : Signature,
        IMetaDataNormalizeSignature
    {
        //
        // State
        //

        private readonly SignatureType m_type;

        //
        // Constructor Methods
        //

        internal SignatureTypeSpec( SignatureType type )
        {
            m_type = type;
        }

        internal static SignatureTypeSpec Parse( Parser      parser ,
                                                 ArrayReader reader )
        {
            //
            // Section 23.2.14 ECMA spec, Partition II
            //
            ElementTypes elementType = (ElementTypes)reader.PeekUInt8();
            switch(elementType)
            {
                case ElementTypes.PTR:
                case ElementTypes.FNPTR:
                case ElementTypes.ARRAY:
                case ElementTypes.SZARRAY:
                case ElementTypes.GENERICINST:
                case ElementTypes.VAR:
                case ElementTypes.MVAR:
                    break;

                default:
                    throw IllegalMetaDataFormatException.Create( "Not a typespec signature: {0}", reader );
            }

            SignatureType type = Signature.ParseType( parser, reader, Signature.ParseFlags.MustBeWholeSignature );

            return new SignatureTypeSpec( type );
        }

        //
        // IMetaDataNormalizeSignature methods
        //

        Normalized.MetaDataSignature IMetaDataNormalizeSignature.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.SignatureType res;

            context.GetNormalizedSignature( m_type, out res, MetaDataNormalizationMode.Default );

            return res;
        }

        //
        // Access Methods
        //

        public SignatureType TypeSpecType
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
            return "SignatureTypeSpec(" + m_type + ")";
        }
    }
}

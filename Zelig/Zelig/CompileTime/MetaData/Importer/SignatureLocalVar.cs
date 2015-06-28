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
    public class SignatureLocalVar : Signature
    {
        //
        // State
        //

        private readonly SignatureType[] m_locals;

        //
        // Constructor Methods
        //

        internal SignatureLocalVar( SignatureType[] locals )
        {
            m_locals = locals;
        }

        internal static SignatureLocalVar Parse( Parser      parser ,
                                                 ArrayReader reader )
        {
            Signature sig = Signature.ParseUnknown( parser, reader );
            if(sig is SignatureLocalVar)
            {
                return (SignatureLocalVar)sig;
            }

            throw IllegalMetaDataFormatException.Create( "Not a local variables signature: {0}", sig );
        }

        //
        // Access Methods
        //

        public SignatureType[] Locals
        {
            get
            {
                return m_locals;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder( "SignatureLocalVar(" );

            for(int i = 0; i < m_locals.Length; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                sb.Append( m_locals[i] );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

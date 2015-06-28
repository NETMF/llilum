//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class SignatureMethod : MetaDataSignature,
        IMetaDataUnique
    {
        public enum CallingConventions : byte
        {
            Default            = 0x00,
            Unmanaged_cdecl    = 0x01,
            Unmanaged_sdtcall  = 0x02,
            Unmanaged_thiscall = 0x03,
            Unmanaged_fastcall = 0x04,
            VarArg             = 0x05,
            Field              = 0x06,
            LocalVar           = 0x07,
            Property           = 0x08,
            Unmanaged          = 0x09,
            GenericInst        = 0x0A,
            Mask               = 0x0F,
            Generic            = 0x10,
            HasThis            = 0x20,
            ExplicitThis       = 0x40
        }

        //
        // State
        //

        internal CallingConventions m_callingConvention;
        internal SignatureType      m_returnType;
        internal SignatureType[]    m_parameters;

        //
        // Constructor Methods
        //

        internal SignatureMethod( int token ) : base( token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is SignatureMethod) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                SignatureMethod other = (SignatureMethod)obj;

                if(m_callingConvention == other.m_callingConvention &&
                   m_returnType        == other.m_returnType         )
                {
                    if(ArrayUtility.ArrayEquals( m_parameters, other.m_parameters ))
                    {
                        if(base.InnerEquals( other ))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            int res = (int)m_callingConvention               ^
                           m_returnType       .GetHashCode() ;

            if(m_parameters != null)
            {
                for(int i = 0; i < m_parameters.Length; i++)
                {
                    res = res << 5 ^ m_parameters[i].GetHashCode() ^ res >> (32-5);
                }
            }

            return res;
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return m_returnType.Type.MakeUnique( this );
        }

        //
        // Helper Methods
        //

        public bool Match( SignatureMethod sig )
        {
            if(sig.CallingConvention == (SignatureMethod.CallingConventions)m_callingConvention)
            {
                if(m_parameters.Length == sig.m_parameters.Length)
                {
                    if(m_returnType.Match( null, null, sig.m_returnType ))
                    {
                        for(int i = 0; i < m_parameters.Length; i++)
                        {
                            if(m_parameters[i].Match( null, null, sig.m_parameters[i] ) == false)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
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

        public override bool UsesTypeParameters
        {
            get
            {
                if(m_returnType.UsesTypeParameters) return true;

                foreach(SignatureType td in m_parameters)
                {
                    if(td.UsesTypeParameters) return true;
                }

                return false;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                if(m_returnType.UsesMethodParameters) return true;

                foreach(SignatureType td in m_parameters)
                {
                    if(td.UsesMethodParameters) return true;
                }

                return false;
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

                sb.Append( m_returnType.FullName );
                sb.Append( " <= " );

                if((m_callingConvention & CallingConventions.HasThis) != 0)
                {
                    sb.Append( "this," );
                }

                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(i != 0)
                    {
                        sb.Append( "," );
                    }

                    sb.Append( m_parameters[i].FullName );
                }

                return sb.ToString();
            }
        }

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "SignatureMethod(" );

            sb.Append( m_callingConvention );
            sb.Append( "," );

            sb.Append( m_returnType );
            sb.Append( "," );
            sb.Append( "parameters(" );

            for(int i = 0; i < m_parameters.Length; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                sb.Append( m_parameters[i] );
            }

            sb.Append( "))" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }

        internal static string EnumToString( CallingConventions val )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if((val & CallingConventions.ExplicitThis) != 0                                    ) sb.Append( "explicit "           );
            if((val & CallingConventions.HasThis     ) != 0                                    ) sb.Append( "instance "           );
            if((val & CallingConventions.Mask        ) == CallingConventions.Unmanaged_cdecl   ) sb.Append( "unmanaged cdecl "    );
            if((val & CallingConventions.Mask        ) == CallingConventions.Unmanaged_sdtcall ) sb.Append( "unmanaged stdcall "  );
            if((val & CallingConventions.Mask        ) == CallingConventions.Unmanaged_thiscall) sb.Append( "unmanaged thiscall " );
            if((val & CallingConventions.Mask        ) == CallingConventions.Unmanaged_fastcall) sb.Append( "unmanaged fastcall " );
            if((val & CallingConventions.Mask        ) == CallingConventions.VarArg            ) sb.Append( "vararg "             );

            return sb.ToString();
        }
    }
}

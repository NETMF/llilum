//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataField : MetaDataFieldAbstract
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionAbstract m_owner;
        internal          FieldAttributes                m_flags;
        internal          string                         m_name;
        internal          SignatureField                 m_signature;

        internal          MetaDataFieldRVA               m_fieldRVA;
        internal          MetaDataConstant               m_constant;
        internal          MetaDataFieldLayout            m_layout;

        //
        // Constructor Methods
        //

        internal MetaDataField( MetaDataTypeDefinitionAbstract owner ,
                                int                            token ) : base( token )
        {
            m_owner = owner;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataField) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataField other = (MetaDataField)obj;

                if(m_owner     == other.m_owner     &&
                   m_flags     == other.m_flags     &&
                   m_name      == other.m_name      &&
                   m_signature == other.m_signature  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return      m_owner.GetHashCode() ^
                   (int)m_flags               ^
                        m_name .GetHashCode();
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return m_owner.MakeUnique( this );
        }

        internal bool Match( string         name ,
                             SignatureField sig  )
        {
            if(m_name == name)
            {
                return m_signature.Match( sig );
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract Owner
        {
            get
            {
                return m_owner;
            }
        }

        public FieldAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public SignatureField FieldSignature
        {
            get
            {
                return m_signature;
            }
        }

        public override bool IsOpenField
        {
            get
            {
                return m_signature.IsOpenType;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_signature.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_signature.UsesMethodParameters;
            }
        }

////    public Object DefaultValueToType( ElementTypes type )
////    {
////        if((this.flags & Attributes.HasDefault) != 0 && this.constant != null)
////        {
////            return this.constant.ValueToType( type );
////        }
////        else
////        {
////            return this.DefaultValue;
////        }
////    }

        public Object DefaultValue
        {
            get
            {
                if((m_flags & FieldAttributes.HasDefault) != 0 && m_constant != null)
                {
                    return m_constant.Value;
                }
                else if((m_flags & FieldAttributes.HasFieldRVA) != 0 && m_fieldRVA != null)
                {
                    return m_fieldRVA.DataBytes;
                }
                else
                {
                    return null;
                }
            }
        }

        public MetaDataFieldLayout Layout
        {
            get
            {
                return m_layout;
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

                sb.Append( m_flags     );
                sb.Append( ","         );
                sb.Append( m_name      );
                sb.Append( ","         );
                sb.Append( m_signature );

                return sb.ToString();
            }
        }


        public override String ToString()
        {
            return "MetaDataField(" + this.FullName + ")";
        }

        public override String ToString( IMetaDataDumper context )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( m_signature.TypeSignature.ToStringWithAbbreviations( context.PushContext( m_owner ) ) );

            sb.Append( " "                                          );
            sb.Append( m_owner.ToStringWithAbbreviations( context ) );
            sb.Append( "::"                                         );
            sb.Append( m_name                                       );
            sb.Append( " "                                          );
            sb.Append( TokenToString( m_token )                     );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( ".field " );

            sb.Append( TokenToString( m_token )                                      );
            sb.Append( " "                                                           );
            sb.Append( EnumToString( m_flags )                                       );
            sb.Append( m_signature.TypeSignature.ToStringWithAbbreviations( writer ) );
            sb.Append( " "                                                           );
            sb.Append( m_name                                                        );

            if(m_constant != null)
            {
                sb.Append( " = "            );
                sb.Append( m_constant.Value );
            }

            writer.WriteLine( sb.ToString() );
        }

        private static string EnumToString( FieldAttributes val )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.Public      ) sb.Append( "public "        );
            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.Private     ) sb.Append( "private "       );
            if((val & FieldAttributes.Static         ) != 0                           ) sb.Append( "static "        );
            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.Family      ) sb.Append( "family "        );
            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly    ) sb.Append( "assembly "      );
            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem ) sb.Append( "famandassem "   );
            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem  ) sb.Append( "famorassem "    );
            if((val & FieldAttributes.FieldAccessMask) == FieldAttributes.PrivateScope) sb.Append( "privatescope "  );
            if((val & FieldAttributes.InitOnly       ) != 0                           ) sb.Append( "initonly "      );
            if((val & FieldAttributes.Literal        ) != 0                           ) sb.Append( "literal "       );
            if((val & FieldAttributes.NotSerialized  ) != 0                           ) sb.Append( "notserialized " );
            if((val & FieldAttributes.SpecialName    ) != 0                           ) sb.Append( "specialname "   );
            if((val & FieldAttributes.RTSpecialName  ) != 0                           ) sb.Append( "rtspecialname " );

            return sb.ToString();
        }
    }
}

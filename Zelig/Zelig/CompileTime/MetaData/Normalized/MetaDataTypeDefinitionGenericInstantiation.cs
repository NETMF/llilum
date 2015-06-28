//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinitionGenericInstantiation : MetaDataTypeDefinitionAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MetaDataTypeDefinitionGeneric m_baseType;
        internal SignatureType[]               m_parameters;

        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionGenericInstantiation( MetaDataAssembly owner ,
                                                             int              token ) : base( owner, token )
        {
            m_elementType = ElementTypes.GENERICINST;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionGenericInstantiation) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionGenericInstantiation other = (MetaDataTypeDefinitionGenericInstantiation)obj;

                if(m_baseType == other.m_baseType)
                {
                    if(ArrayUtility.ArrayEquals( m_parameters, other.m_parameters))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_baseType.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool IsOpenType
        {
            get
            {
                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(m_parameters[i].IsOpenType)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          )
        {
            if(type is MetaDataTypeDefinitionGenericInstantiation)
            {
                MetaDataTypeDefinitionGenericInstantiation other = (MetaDataTypeDefinitionGenericInstantiation)type;

                if(m_baseType          == other.m_baseType          &&
                   m_parameters.Length == other.m_parameters.Length  )
                {
                    for(int i = 0; i < m_parameters.Length; i++)
                    {
                        if(m_parameters[i].Match( typeContext, methodContext, other.m_parameters[i] ) == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public MetaDataTypeDefinitionGenericInstantiation Specialize( SignatureType[] genericParameters )
        {
            MetaDataTypeDefinitionGenericInstantiation tdNew = new MetaDataTypeDefinitionGenericInstantiation( m_owner, 0 );

            tdNew.m_baseType   = m_baseType;
            tdNew.m_parameters = genericParameters;

            return tdNew;
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionGeneric GenericType
        {
            get
            {
                return m_baseType;
            }
        }

        public SignatureType[] InstantiationParams
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

        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append( m_baseType.FullName );

                sb.Append( "<" );
                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( m_parameters[i].FullName );
                }
                sb.Append( ">" );

                return sb.ToString();
            }
        }

        public override string FullNameWithAbbreviation
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append( m_baseType.FullNameWithAbbreviation );

                sb.Append( "<" );
                for(int i = 0; i < m_parameters.Length; i++)
                {
                    if(i != 0) sb.Append( "," );

                    sb.Append( m_parameters[i].FullNameWithAbbreviation );
                }
                sb.Append( ">" );

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinitionGenericInstantiation" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            context = context.PushContext( this );

            sb.Append( m_baseType.ToString( context ) );

            return sb.ToString();
        }

         public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}

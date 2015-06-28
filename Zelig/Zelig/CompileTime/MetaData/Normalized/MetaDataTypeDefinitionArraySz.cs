//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinitionArraySz : MetaDataTypeDefinitionArray,
        IMetaDataUnique
    {
        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionArraySz( MetaDataAssembly owner ,
                                                int              token ) : base( owner, token )
        {
            m_elementType = ElementTypes.SZARRAY;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionArraySz) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionArraySz other = (MetaDataTypeDefinitionArraySz)obj;

                return InnerEquals( other );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return InnerGetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          )
        {
            if(type is MetaDataTypeDefinitionArraySz)
            {
                MetaDataTypeDefinitionArraySz array = (MetaDataTypeDefinitionArraySz)type;

                return m_objectType.Match( typeContext, methodContext, array.m_objectType );
            }

            return false;
        }

        //
        // Debug Methods
        //

        public override string FullName
        {
            get
            {
                return AppendQualifiers( m_objectType.FullName );
            }
        }

        public override string FullNameWithAbbreviation
        {
            get
            {
                return AppendQualifiers( m_objectType.FullNameWithAbbreviation );
            }
        }

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinitionArraySz" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( m_objectType.ToStringWithAbbreviations( context ) );

            sb.Append( "[]" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }

        //--//

        private string AppendQualifiers( string prefix )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( prefix );

            sb.Append( "[]" );

            return sb.ToString();
        }
    }
}

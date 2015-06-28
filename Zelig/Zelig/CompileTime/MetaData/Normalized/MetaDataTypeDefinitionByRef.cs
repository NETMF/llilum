//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinitionByRef : MetaDataTypeDefinitionAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MetaDataTypeDefinitionAbstract m_type;

        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionByRef( MetaDataAssembly owner       ,
                                              int              token       ,
                                              ElementTypes     elementType ) : base( owner, token )
        {
            m_elementType = elementType;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionByRef) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionByRef other = (MetaDataTypeDefinitionByRef)obj;

                if(m_elementType == other.m_elementType &&
                   m_type        == other.m_type         )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_type.GetHashCode();
        }

        //
        // Helper Methods
        //

        public override bool IsOpenType
        {
            get
            {
                return m_type.IsOpenType;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_type.UsesTypeParameters;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_type.UsesMethodParameters;
            }
        }

        public override bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          )
        {
            if(type is MetaDataTypeDefinitionByRef)
            {
                MetaDataTypeDefinitionByRef byref = (MetaDataTypeDefinitionByRef)type;

                return m_type.Match( typeContext, methodContext, byref.m_type );
            }

            return false;
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionAbstract BaseType
        {
            get
            {
                return m_type;
            }
        }

        //
        // Debug Methods
        //

        public override string FullName
        {
            get
            {
                return InnerToString( m_type.FullName );
            }
        }

        public override string FullNameWithAbbreviation
        {
            get
            {
                return InnerToString( m_type.FullNameWithAbbreviation );
            }
        }

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinitionByRef" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            return InnerToString( m_type.ToStringWithAbbreviations( context ) );
        }

        private String InnerToString( string type )
        {
            switch(m_elementType)
            {
                case ElementTypes.BYREF:
                    return "byref " + type;

                case ElementTypes.PTR:
                    return type + " *";

                case ElementTypes.PINNED:
                    return "pinned " + type;

                default:
                    return m_elementType.ToString() + " " + type;
            }
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }
    }
}

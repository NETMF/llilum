//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinitionGeneric : MetaDataTypeDefinitionBase,
        IMetaDataUnique
    {
        //
        // State
        //

        internal MetaDataGenericTypeParam[] m_genericParams;

        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionGeneric( MetaDataAssembly owner ,
                                                int              token ) : base( owner, token )
        {
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionGeneric) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionGeneric other = (MetaDataTypeDefinitionGeneric)obj;

                if(InnerEquals( other ))
                {
                    return ArrayUtility.ArrayEquals( m_genericParams, other.m_genericParams );
                }
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

        public override bool IsOpenType
        {
            get
            {
                return true;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return true;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return false;
            }
        }

        public override bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          )
        {
            throw new NotNormalized( "Match" );
        }

        //
        // Access Methods
        //

        public MetaDataGenericTypeParam[] GenericParams
        {
            get
            {
                return m_genericParams;
            }
        }

        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                if(this.IsNestedType)
                {
                    sb.Append( this.EnclosingClass.FullName );
                    sb.Append( "."                          );
                }

                if(this.Namespace != null && this.Namespace.Length != 0)
                {
                    sb.Append( this.Namespace );
                    sb.Append( "."            );
                }

                sb.Append( this.Name );

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinitionGeneric" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if(this.IsNestedType)
            {
                sb.Append( this.EnclosingClass.ToString( context ) );
                sb.Append( "."                                     );
            }

            if(this.Namespace != null && this.Namespace.Length != 0)
            {
                sb.Append( this.Namespace );
                sb.Append( "."            );
            }

            sb.Append( this.Name                );
            sb.Append( TokenToString( m_token ) );

            sb.Append( "<" );
            for(int i = 0; i < m_genericParams.Length; i++)
            {
                if(i != 0) sb.Append( "," );

                sb.Append( m_genericParams[i].ToString( context ) );
            }
            sb.Append( ">" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            InnerDump( writer, m_genericParams );
        }
    }
}

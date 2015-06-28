//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinition : MetaDataTypeDefinitionBase,
        IMetaDataUnique
    {
        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinition( MetaDataAssembly owner ,
                                         int              token ) : base( owner, token )
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinition) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinition other = (MetaDataTypeDefinition)obj;

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

        public override bool IsOpenType
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
            // LT72: previous code used operator equality, which apparently differs in impplementation from 2.0 to 4.5
            //return this == type;
            return Equals( type );
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinition" );
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

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            InnerDump( writer, null );
        }
    }
}

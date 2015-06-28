//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;
    using System.Collections.Generic;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataTypeDefinitionPartiallyDelayedTypeParameter : MetaDataTypeDefinitionAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataTypeDefinitionGeneric m_contextType;
        internal readonly int                           m_parameterNumber;

        //
        // Constructor Methods
        //

        public MetaDataTypeDefinitionPartiallyDelayedTypeParameter( MetaDataTypeDefinitionGeneric contextType     ,
                                                                    int                           parameterNumber ) : base( contextType.Owner, 0 )
        {
            m_contextType     = contextType;
            m_parameterNumber = parameterNumber;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionPartiallyDelayedTypeParameter) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionPartiallyDelayedTypeParameter other = (MetaDataTypeDefinitionPartiallyDelayedTypeParameter)obj;

                if(m_contextType     == other.m_contextType     &&
                   m_parameterNumber == other.m_parameterNumber  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return      m_contextType    .GetHashCode() ^
                   (int)m_parameterNumber               ;
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

        public override bool Match( MetaDataTypeDefinitionAbstract typeContext   ,
                                    MetaDataMethodAbstract         methodContext ,
                                    MetaDataTypeDefinitionAbstract type          )
        {
            throw new NotNormalized( "Match" );
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinitionGeneric ContextType
        {
            get
            {
                return m_contextType;
            }
        }

        public int ParameterNumber
        {
            get
            {
                return m_parameterNumber;
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

        //
        // Debug Methods
        //

        public override string FullName
        {
            get
            {
                return AppendQualifiers();
            }
        }

        public override string FullNameWithAbbreviation
        {
            get
            {
                return AppendQualifiers();
            }
        }

        public override string ToString()
        {
            return QualifiedToString( "MetaDataTypeDefinitionPartiallyDelayedTypeParameter" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            return MetaDataTypeDefinitionAbstract.ParameterToString( context, m_parameterNumber, false, null );
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }

        //--//

        private string AppendQualifiers()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( m_contextType.FullName                              );
            sb.Append( "."                                                 );
            sb.Append( m_contextType.GenericParams[m_parameterNumber].Name );

            return sb.ToString();
        }
    }
}

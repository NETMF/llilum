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
    public sealed class MetaDataTypeDefinitionPartiallyDelayedMethodParameter : MetaDataTypeDefinitionAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal readonly MetaDataMethodGeneric m_contextMethod;
        internal readonly int                   m_parameterNumber;

        //
        // Constructor Methods
        //

        public MetaDataTypeDefinitionPartiallyDelayedMethodParameter( MetaDataMethodGeneric contextMethod   ,
                                                                      int                   parameterNumber ) : base( contextMethod.Owner.Owner, 0 )
        {
            m_contextMethod   = contextMethod;
            m_parameterNumber = parameterNumber;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionPartiallyDelayedMethodParameter) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionPartiallyDelayedMethodParameter other = (MetaDataTypeDefinitionPartiallyDelayedMethodParameter)obj;

                if(m_contextMethod   == other.m_contextMethod   &&
                   m_parameterNumber == other.m_parameterNumber  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return      m_contextMethod  .GetHashCode() ^
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

        public MetaDataMethodGeneric ContextMethod
        {
            get
            {
                return m_contextMethod;
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
                return false;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return true;
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
            return QualifiedToString( "MetaDataTypeDefinitionPartiallyDelayedMethodParameter" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            return MetaDataTypeDefinitionAbstract.ParameterToString( context, m_parameterNumber, true, null );
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }

        //--//

        private string AppendQualifiers()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( m_contextMethod.FullName                              );
            sb.Append( "."                                                   );
            sb.Append( m_contextMethod.GenericParams[m_parameterNumber].Name );

            return sb.ToString();
        }
    }
}

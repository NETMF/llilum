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
    public sealed class MetaDataTypeDefinitionDelayed : MetaDataTypeDefinitionAbstract,
        IMetaDataUnique
    {
        //
        // State
        //

        internal int  m_parameterNumber;
        internal bool m_isMethodParam;

        //
        // Constructor Methods
        //

        internal MetaDataTypeDefinitionDelayed( MetaDataAssembly owner ,
                                                int              token ) : base( owner, token )
        {
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataTypeDefinitionDelayed) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionDelayed other = (MetaDataTypeDefinitionDelayed)obj;

                if(m_owner           == other.m_owner           &&
                   m_parameterNumber == other.m_parameterNumber &&
                   m_isMethodParam   == other.m_isMethodParam    )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int) m_parameterNumber          ^
                        (m_isMethodParam   ? 0 : 1) ;
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
            if(type is MetaDataTypeDefinitionDelayed) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataTypeDefinitionDelayed other = (MetaDataTypeDefinitionDelayed)type;

                if(m_parameterNumber == other.m_parameterNumber &&
                   m_isMethodParam   == other.m_isMethodParam    )
                {
                    return true;
                }
            }

            return false;
        }

        //
        // Access Methods
        //

        public int ParameterNumber
        {
            get
            {
                return m_parameterNumber;
            }
        }

        public bool IsMethodParameter
        {
            get
            {
                return m_isMethodParam;
            }
        }

        public override bool UsesTypeParameters
        {
            get
            {
                return m_isMethodParam == false;
            }
        }

        public override bool UsesMethodParameters
        {
            get
            {
                return m_isMethodParam == true;
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
            return QualifiedToString( "MetaDataTypeDefinitionDelayed" );
        }

        public override String ToString( IMetaDataDumper context )
        {
            return MetaDataTypeDefinitionAbstract.ParameterToString( context, m_parameterNumber, m_isMethodParam, null );
        }

        public override void Dump( IMetaDataDumper writer )
        {
            throw new NotNormalized( "Dump" );
        }

        //--//

        private string AppendQualifiers()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append( "{" );
            sb.Append( m_isMethodParam ? "MVAR" : "VAR" );
            sb.Append( ","                              );
            sb.Append( m_parameterNumber                );
            sb.Append( "}" );

            return sb.ToString();
        }
    }
}

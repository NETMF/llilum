//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public sealed class MetaDataGenericParam : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private short                       m_number;
        private GenericParameterAttributes  m_flags;
        private IMetaDataTypeOrMethodDef    m_owner;
        private String                      m_name;

        private List<IMetaDataTypeDefOrRef> m_genericParamConstraints;

        //
        // Constructor Methods
        //

        private MetaDataGenericParam( int index ) : base( TokenType.GenericParam, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataGenericParam( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader ownerReader = ts.m_columns[2].m_reader;
            int                ownerIndex;

            m_number   =                               reader.ReadInt16();
            m_flags    = (GenericParameterAttributes)  reader.ReadInt16();
            ownerIndex =        ownerReader          ( reader );
            m_name     = parser.readIndexAsString    ( reader );

            m_owner = parser.getTypeOrMethodDef( ownerIndex );

            if(m_owner is MetaDataTypeDefinition)
            {
                ((MetaDataTypeDefinition)m_owner).AddGenericParam( this );
            }
            else if(m_owner is MetaDataMethod)
            {
                ((MetaDataMethod)m_owner).AddGenericParam( this );
            }
            else
            {
                throw IllegalMetaDataFormatException.Create( "Unknown owner of GenericParam: {0}", m_owner );
            }
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfTypeDefinitions:
                    if(m_owner is MetaDataTypeDefinition)
                    {
                        Normalized.MetaDataGenericTypeParam param = new Normalized.MetaDataGenericTypeParam( context.GetTypeFromContext(), m_token );

                        param.m_number = m_number;
                        param.m_flags  = m_flags;
                        param.m_name   = m_name;

                        return param;
                    }
                    break;

                case MetaDataNormalizationPhase.CreationOfMethodDefinitions:
                    if(m_owner is MetaDataMethod)
                    {
                        Normalized.MetaDataGenericMethodParam param = new Normalized.MetaDataGenericMethodParam( (Normalized.MetaDataMethodGeneric)context.GetMethodFromContext(), m_token );

                        param.m_number = m_number;
                        param.m_flags  = m_flags;
                        param.m_name   = m_name;

                        return param;
                    }
                    break;
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataGenericParam param = (Normalized.MetaDataGenericParam)obj;

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CompletionOfTypeNormalization:
                    {
                        context.GetNormalizedObjectList( m_genericParamConstraints, out param.m_genericParamConstraints, MetaDataNormalizationMode.Default );
                    }
                    return;

                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out param.m_customAttributes, MetaDataNormalizationMode.Allocate );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //--//

        internal void AddGenericParamConstraint( IMetaDataTypeDefOrRef constraint )
        {
            if(m_genericParamConstraints == null)
            {
                m_genericParamConstraints = new List<IMetaDataTypeDefOrRef>( 2 );
            }

            m_genericParamConstraints.Add( constraint );
        }

        //
        // Access Methods
        //

        public short Number
        {
            get
            {
                return m_number;
            }
        }

        public GenericParameterAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public IMetaDataTypeOrMethodDef Owner
        {
            get
            {
                return m_owner;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public List<IMetaDataTypeDefOrRef> GenericParamConstraints
        {
            get
            {
                return m_genericParamConstraints;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataGenericParam(" + m_name + ")";
        }

        public override String ToStringLong()
        {
            return "MetaDataGenericParam(" + m_number + "," + m_flags.ToString( "x" ) + "," + m_owner + "," + m_name + ")";
        }
    }
}

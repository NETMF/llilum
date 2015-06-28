//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public sealed class MetaDataProperty : MetaDataObject,
        IMetaDataHasConstant,
        IMetaDataHasSemantic,
        IMetaDataNormalize
    {
        //
        // State
        //

        private PropertyAttributes     m_flags;
        private String                 m_name;
        private SignatureProperty      m_type;
        private MetaDataTypeDefinition m_owner;
        private MetaDataConstant       m_constant;

        //
        // Constructor Methods
        //

        private MetaDataProperty( int index ) : base( TokenType.Property, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataProperty( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            int typeIndex;

            m_flags   = (PropertyAttributes)       reader.ReadInt16();
            m_name    = parser.readIndexAsString ( reader );
            typeIndex = parser.readIndexAsForBlob( reader );

            m_type = SignatureProperty.Parse( parser, parser.getSignature( typeIndex ) );

            m_owner = parser.GetTypeFromPropertyIndex( MetaData.UnpackTokenAsIndex( m_token ) );

            m_owner.AddProperty( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                    {
                        Normalized.MetaDataProperty propertyNew = new Normalized.MetaDataProperty( context.GetTypeFromContext(), m_token );

                        propertyNew.m_flags = m_flags;
                        propertyNew.m_name  = m_name;

                        context.GetNormalizedSignature( m_type    , out propertyNew.m_type    , MetaDataNormalizationMode.Default );
                        context.GetNormalizedObject   ( m_constant, out propertyNew.m_constant, MetaDataNormalizationMode.Default );

                        return propertyNew.MakeUnique();
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataProperty prop = (Normalized.MetaDataProperty)obj;

            context = context.Push( obj );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out prop.m_customAttributes, MetaDataNormalizationMode.Allocate );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //--//

        internal void SetConstant( MetaDataConstant constant )
        {
            m_constant = constant;
        }

        //
        // Access Methods
        //

        public PropertyAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public SignatureProperty PropertySignature
        {
            get
            {
                return m_type;
            }
        }

        public Object DefaultValue
        {
            get
            {
                if((m_flags & PropertyAttributes.HasDefault) != 0)
                {
                    return m_constant.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataProperty(" + m_flags.ToString( "x" ) + "," + m_name + "," + m_type + ")";
        }
    }
}

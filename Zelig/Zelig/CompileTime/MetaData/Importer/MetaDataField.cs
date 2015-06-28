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
    public sealed class MetaDataField : MetaDataObject,
        IMetaDataHasConstant,
        IMetaDataHasFieldMarshal,
        IMetaDataMemberForwarded,
        IMetaDataNormalize
    {
        //
        // State
        //

        private FieldAttributes        m_flags;
        private string                 m_name;
        private SignatureField         m_signature;
        private MetaDataTypeDefinition m_parent;
        private MetaDataFieldRVA       m_fieldRVA;
        private MetaDataConstant       m_constant;
        private MetaDataFieldLayout    m_layout;

        //
        // Constructor Methods
        //

        private MetaDataField( int index ) : base( TokenType.Field, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataField( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            int signatureIndex;

            m_flags        = (FieldAttributes)          reader.ReadInt16();
            m_name         = parser.readIndexAsString ( reader );
            signatureIndex = parser.readIndexAsForBlob( reader );

            m_signature = SignatureField.Parse( parser, parser.getSignature( signatureIndex ) );

            m_parent = parser.GetTypeFromFieldIndex( MetaData.UnpackTokenAsIndex( m_token ) );

            m_parent.AddField( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfFieldDefinitions:
                    {
                        Normalized.MetaDataField fieldNew = new Normalized.MetaDataField( context.GetTypeFromContext(), m_token );

                        fieldNew.m_flags = m_flags;
                        fieldNew.m_name  = m_name;

                        context.GetNormalizedSignature( m_signature, out fieldNew.m_signature, MetaDataNormalizationMode.Default );
                        context.GetNormalizedObject   ( m_fieldRVA , out fieldNew.m_fieldRVA , MetaDataNormalizationMode.Default );
                        context.GetNormalizedObject   ( m_constant , out fieldNew.m_constant , MetaDataNormalizationMode.Default );
                        context.GetNormalizedObject   ( m_layout   , out fieldNew.m_layout   , MetaDataNormalizationMode.Default );

                        return fieldNew;
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataField field = (Normalized.MetaDataField)obj;

            context = context.Push( obj );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out field.m_customAttributes, MetaDataNormalizationMode.Allocate );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //--//

        internal void SetRVA( MetaDataFieldRVA fieldRVA )
        {
            m_fieldRVA = fieldRVA;
        }

        internal void SetConstant( MetaDataConstant constant )
        {
            m_constant = constant;
        }

        internal void SetLayout( MetaDataFieldLayout fieldLayout )
        {
            m_layout = fieldLayout;
        }

        //
        // Access Methods
        //

        public FieldAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public override string FullName
        {
            get
            {
                if(m_parent != null)
                {
                    return m_parent.FullName + "." + this.Name;
                }
                else
                {
                    return this.Name;
                }
            }
        }

        public override string FullNameWithContext
        {
            get
            {
                return "field " + this.FullName;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public SignatureField Signature
        {
            get
            {
                return m_signature;
            }
        }

        public MetaDataTypeDefinition Parent
        {
            get
            {
                return m_parent;
            }
        }

        public Object DefaultValueToType( ElementTypes type )
        {
            if((m_flags & FieldAttributes.HasDefault) != 0 && m_constant != null)
            {
                return m_constant.ValueToType( type );
            }
            else
            {
                return this.DefaultValue;
            }
        }

        public Object DefaultValue
        {
            get
            {
                if((m_flags & FieldAttributes.HasDefault) != 0 && m_constant != null)
                {
                    return m_constant.Value;
                }
                else if((m_flags & FieldAttributes.HasFieldRVA) != 0 && m_fieldRVA != null)
                {
                    return m_fieldRVA.DataBytes;
                }
                else
                {
                    return null;
                }
            }
        }

        public int RVA
        {
            get
            {
                if((m_flags & FieldAttributes.HasFieldRVA) != 0 && m_fieldRVA != null)
                {
                    return m_fieldRVA.RVA;
                }
                else
                {
                    return 0;
                }
            }
        }

        public MetaDataFieldLayout Layout
        {
            get
            {
                return m_layout;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataField(" );

            sb.Append( m_flags );
            sb.Append( "," );

            sb.Append( m_parent.FullName );
            sb.Append( "." );
            sb.Append( m_name );
            sb.Append( "," );
            sb.Append( m_signature );

            if(this.DefaultValue != null)
            {
                sb.Append( "[" );
                sb.Append( this.DefaultValue );
                sb.Append( "]" );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

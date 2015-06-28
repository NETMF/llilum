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
    public sealed class MetaDataParam : MetaDataObject,
        IMetaDataHasConstant,
        IMetaDataHasFieldMarshal,
        IMetaDataNormalize
    {
        //
        // State
        //

        private ParamAttributes  m_flags;
        private short            m_sequence;
        private String           m_name;
        private MetaDataMethod   m_parent;
        private MetaDataConstant m_constant;

        //
        // Constructor Methods
        //

        private MetaDataParam( int index ) : base( TokenType.Param, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataParam( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            m_flags    = (ParamAttributes)         reader.ReadInt16();
            m_sequence =                           reader.ReadInt16();
            m_name     = parser.readIndexAsString( reader );

            m_parent = parser.GetMethodFromParamIndex( MetaData.UnpackTokenAsIndex( m_token ) );

            m_parent.AddParam( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.MetaDataParam paramNew = new Normalized.MetaDataParam( m_token );

            paramNew.m_flags    = m_flags;
            paramNew.m_sequence = m_sequence;
            paramNew.m_name     = m_name;

            context.GetNormalizedObject( m_constant, out paramNew.m_constant, MetaDataNormalizationMode.Default );

            return paramNew;
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataParam param = (Normalized.MetaDataParam)obj;

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out param.m_customAttributes, MetaDataNormalizationMode.Allocate );
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

        public ParamAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public short Sequence
        {
            get
            {
                return m_sequence;
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
                return "parameter " + this.FullName;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }


        public MetaDataMethod Parent
        {
            get
            {
                return m_parent;
            }
        }

        public Object DefaultValue
        {
            get
            {
                if((m_flags & ParamAttributes.HasDefault) != 0)
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
            return "MetaDataParam(" + m_name + ")";
        }

        public override String ToStringLong()
        {
            return "MetaDataParam(" + m_flags.ToString( "x" ) + "," + m_sequence + "," + m_name + "<" + m_parent + ">)";
        }
    }
}

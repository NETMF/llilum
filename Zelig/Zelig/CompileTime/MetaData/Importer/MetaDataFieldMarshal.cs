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
    public sealed class MetaDataFieldMarshal : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private IMetaDataHasFieldMarshal m_parent;
        private MarshalSpec              m_nativeType;

        //
        // Constructor Methods
        //

        private MetaDataFieldMarshal( int index ) : base( TokenType.FieldMarshal, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataFieldMarshal( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader parentReader = ts.m_columns[0].m_reader;

            int parentIndex     =        parentReader      ( reader );
            int nativeTypeIndex = parser.readIndexAsForBlob( reader );

            m_parent     = parser.getHasFieldMarshal( parentIndex );
            m_nativeType = parser.getNativeType     ( nativeTypeIndex );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
////        resolver.ResolveExternalReference( ref this.parent );
            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public IMetaDataHasFieldMarshal Parent
        {
            get
            {
                return m_parent;
            }
        }

        public MarshalSpec NativeType
        {
            get
            {
                return m_nativeType;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataFieldMarshal(" + ((MetaDataObject)m_parent).ToStringLong() + "," + m_nativeType + ")";
        }
    }
}

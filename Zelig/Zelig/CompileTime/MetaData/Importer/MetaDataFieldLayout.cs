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
    public sealed class MetaDataFieldLayout : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private int           m_offset;
        private MetaDataField m_field;

        //
        // Constructor Methods
        //

        private MetaDataFieldLayout( int index ) : base( TokenType.FieldLayout, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataFieldLayout( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader fieldReader = ts.m_columns[1].m_reader;
            int                fieldIndex;

            m_offset   =              reader.ReadInt32();
            fieldIndex = fieldReader( reader );

            m_field = parser.getField( fieldIndex );

            m_field.SetLayout( this );
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
                        Normalized.MetaDataFieldLayout res = new Normalized.MetaDataFieldLayout( m_token );

                        res.m_offset = m_offset;

                        return res;
                    }
            }

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

        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        public MetaDataField Field
        {
            get
            {
                return m_field;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataFieldLayout(" + m_offset + "," + m_field + ")";
        }
    }
}

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
    public sealed class MetaDataModule : MetaDataObject,
        IMetaDataResolutionScope,
        IMetaDataNormalize
    {
        //
        // State
        //

        private short  m_generation;
        private String m_name;
        private Guid   m_mvid;

        //
        // Constructor Methods
        //

        private MetaDataModule( int index ) : base( TokenType.Module, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataModule( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader encodingReader = ts.m_columns[3].m_reader;
            int                encodingIndex;
            int                encodingBaseIdIndex;

            m_generation        =                            reader.ReadInt16();
            m_name              = parser.readIndexAsString ( reader );
            m_mvid              = parser.readIndexAsGuid   ( reader );
            encodingIndex       =        encodingReader    ( reader );
            encodingBaseIdIndex =        encodingReader( reader );

            if(encodingIndex != 0 || encodingBaseIdIndex != 0)
            {
                throw IllegalMetaDataFormatException.Create( "Illegal Module table" );
            }
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
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

        public short Generation
        {
            get
            {
                return m_generation;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public Guid MVID
        {
            get
            {
                return m_mvid;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataModule(" + m_name + "," + m_mvid + ")";
        }
    }
}

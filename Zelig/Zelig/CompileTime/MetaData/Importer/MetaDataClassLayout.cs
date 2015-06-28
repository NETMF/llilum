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
    public sealed class MetaDataClassLayout : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private short                  m_packingSize;
        private int                    m_classSize;
        private MetaDataTypeDefinition m_parent;

        //
        // Constructor Methods
        //

        private MetaDataClassLayout( int index ) : base( TokenType.ClassLayout, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataClassLayout( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader parentReader = ts.m_columns[2].m_reader;
            int                parentIndex;

            m_packingSize =               reader.ReadInt16();
            m_classSize   =               reader.ReadInt32();
            parentIndex   = parentReader( reader );

            m_parent = parser.getTypeDef( parentIndex );

            m_parent.SetClassLayout( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CompletionOfTypeNormalization:
                    {
                        Normalized.MetaDataClassLayout cl = new Normalized.MetaDataClassLayout( m_token );

                        cl.m_packingSize = m_packingSize;
                        cl.m_classSize   = m_classSize;

                        return cl;
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

        public short PackingSize
        {
            get
            {
                return m_packingSize;
            }
        }

        public int ClassSize
        {
            get
            {
                return m_classSize;
            }
        }

        public MetaDataTypeDefinition Parent
        {
            get
            {
                return m_parent;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataClassLayout(" + m_packingSize + "," + m_classSize + "," + m_parent + ")";
        }
    }
}

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
    public sealed class MetaDataImplMap : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private ImplementationMapAttributes m_flags;
        private IMetaDataMemberForwarded    m_memberForwarded;
        private String                      m_importName;
        private MetaDataModuleRef           m_importScope;

        //
        // Constructor Methods
        //

        private MetaDataImplMap( int index ) : base( TokenType.ImplMap, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataImplMap( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader memberForwardedReader = ts.m_columns[1].m_reader;
            Parser.IndexReader importScopeReader     = ts.m_columns[3].m_reader;
            int                memberForwardedIndex;
            int                importScopeIndex;

            m_flags              = (ImplementationMapAttributes)  reader.ReadInt16();
            memberForwardedIndex =        memberForwardedReader ( reader );
            m_importName         = parser.readIndexAsString     ( reader );
            importScopeIndex     =        importScopeReader     ( reader );

            m_memberForwarded = parser.getMemberForwarded( memberForwardedIndex );
            m_importScope     = parser.getModuleRef      ( importScopeIndex     );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
////        resolver.ResolveExternalReference( ref this.memberForwarded );
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

        public ImplementationMapAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public IMetaDataMemberForwarded MemberForwarded
        {
            get
            {
                return m_memberForwarded;
            }
        }

        public String ImportName
        {
            get
            {
                return m_importName;
            }
        }

        public MetaDataModuleRef ImportScope
        {
            get
            {
                return m_importScope;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataImplMap(" + m_flags.ToString( "x" ) + "," + m_memberForwarded + "," + m_importName + "," + m_importScope + ")";
        }
    }
}

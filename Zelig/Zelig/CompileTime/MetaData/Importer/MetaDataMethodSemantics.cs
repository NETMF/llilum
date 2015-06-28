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
    public sealed class MetaDataMethodSemantics : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private MethodSemanticAttributes m_semantic;
        private MetaDataMethod           m_method;
        private IMetaDataHasSemantic     m_association;

        //
        // Constructor Methods
        //

        private MetaDataMethodSemantics( int index ) : base( TokenType.MethodSemantics, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataMethodSemantics( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader methodReader      = ts.m_columns[1].m_reader;
            Parser.IndexReader associationReader = ts.m_columns[2].m_reader;
            int                methodIndex;
            int                associationIndex;

            m_semantic       = (MethodSemanticAttributes)  reader.ReadInt16();
            methodIndex      = methodReader              ( reader );
            associationIndex = associationReader         ( reader );

            m_method      = parser.getMethod     ( methodIndex      );
            m_association = parser.getHasSemantic( associationIndex );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
////        resolver.ResolveExternalReference( ref this.association );
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

        public MethodSemanticAttributes Semantic
        {
            get
            {
                return m_semantic;
            }
        }

        public MetaDataMethod Method
        {
            get
            {
                return m_method;
            }
        }

        public IMetaDataHasSemantic Association
        {
            get
            {
                return m_association;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataMethodSemantics(" + m_semantic + "," + m_method + "," + m_association + ")";
        }
    }
}

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
    public sealed class MetaDataStandAloneSig : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private Signature m_signature;

        //
        // Constructor Methods
        //

        private MetaDataStandAloneSig( int index ) : base( TokenType.StandAloneSig, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataStandAloneSig( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader signatureReader = ts.m_columns[0].m_reader;

            int signatureIndex = signatureReader( reader );

            m_signature = Signature.ParseUnknown( parser, parser.getSignature( signatureIndex ) );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
////        this.signature.resolveExternalReferences( resolver );
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

        public Signature Signature
        {
            get
            {
                return m_signature;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataStandAloneSig(" + m_signature + ")";
        }
    }
}

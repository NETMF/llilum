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
    public sealed class MetaDataTypeSpec : MetaDataObject,
        IMetaDataTypeDefOrRef,
        IMetaDataMemberRefParent,
        IMetaDataNormalize
    {
        //
        // State
        //

        private SignatureTypeSpec m_signature;

        //
        // Constructor Methods
        //

        private MetaDataTypeSpec( int index ) : base( TokenType.TypeSpec, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataTypeSpec( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            int signatureIndex = parser.readIndexAsForBlob( reader );

            m_signature = SignatureTypeSpec.Parse( parser, parser.getSignature( signatureIndex ) );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfTypeHierarchy        :
                case MetaDataNormalizationPhase.CompletionOfTypeNormalization  :
                case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                    {
                        Normalized.SignatureType res;

                        context.GetNormalizedSignature( m_signature, out res, MetaDataNormalizationMode.Default );

                        return res.Type;
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

        public SignatureTypeSpec Signature
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
            return "MetaDataTypeSpec(" + m_signature + ")";
        }
    }
}

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
    public sealed class MetaDataMethodSpec : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private IMetaDataMethodDefOrRef m_method;
        private SignatureMethodSpec     m_instantiationValue;

        //
        // Constructor Methods
        //

        private MetaDataMethodSpec( int index ) : base( TokenType.MethodSpec, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataMethodSpec( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader methodReader = ts.m_columns[0].m_reader;

            int methodIndex             =        methodReader      ( reader );
            int instantiationValueIndex = parser.readIndexAsForBlob( reader );

            m_method = parser.getMethodDefOrRef( methodIndex );

            m_instantiationValue = SignatureMethodSpec.Parse( parser, parser.getSignature( instantiationValueIndex ) );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.MetaDataMethodAbstract baseMethod;
            Normalized.SignatureMethod        instantiationValue;

            context.GetNormalizedObject   ( m_method            , out baseMethod        , MetaDataNormalizationMode.Default );
            context.GetNormalizedSignature( m_instantiationValue, out instantiationValue, MetaDataNormalizationMode.Default );

            Normalized.MetaDataMethodGenericInstantiation mdNew = new Normalized.MetaDataMethodGenericInstantiation( context.GetAssemblyFromContext(), 0 );

            mdNew.m_baseMethod = baseMethod;
            mdNew.m_parameters = instantiationValue.m_parameters;

            return mdNew.MakeUnique();
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public IMetaDataMethodDefOrRef Method
        {
            get
            {
                return m_method;
            }
        }

        public SignatureMethodSpec InstantiationValue
        {
            get
            {
                return m_instantiationValue;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataMethodSpec()";
        }

        public override String ToStringLong()
        {
            return "MetaDataMethodSpec(" + m_method + ")";
        }
    }
}

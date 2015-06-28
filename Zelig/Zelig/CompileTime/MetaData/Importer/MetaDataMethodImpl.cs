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
    public sealed class MetaDataMethodImpl : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private MetaDataTypeDefinition  m_classObject;
        private IMetaDataMethodDefOrRef m_body;
        private IMetaDataMethodDefOrRef m_declaration;

        //
        // Constructor Methods
        //

        private MetaDataMethodImpl( int index ) : base( TokenType.MethodImpl, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataMethodImpl( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader classReader       = ts.m_columns[0].m_reader;
            Parser.IndexReader bodyReader        = ts.m_columns[1].m_reader;
            Parser.IndexReader declarationReader = ts.m_columns[2].m_reader;

            int classIndex       = classReader      ( reader );
            int bodyIndex        = bodyReader       ( reader );
            int declarationIndex = declarationReader( reader );

            m_classObject = parser.getTypeDef       ( classIndex       );
            m_body        = parser.getMethodDefOrRef( bodyIndex        );
            m_declaration = parser.getMethodDefOrRef( declarationIndex );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfMethodImplDefinitions:
                    {
                        Normalized.MetaDataMethodImpl miNew = new Normalized.MetaDataMethodImpl( m_token );

                        context.GetNormalizedObject( m_classObject, out miNew.m_classObject, MetaDataNormalizationMode.LookupExisting );
                        context.GetNormalizedObject( m_body       , out miNew.m_body       , MetaDataNormalizationMode.LookupExisting );
                        context.GetNormalizedObject( m_declaration, out miNew.m_declaration, MetaDataNormalizationMode.Default        );

                        return miNew;
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataMethodImpl mi = (Normalized.MetaDataMethodImpl)obj;

            context = context.Push( obj );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CompletionOfMethodImplNormalization:
                    {
                        Normalized.MetaDataTypeDefinitionBase td = (Normalized.MetaDataTypeDefinitionBase)mi.Body.Owner;

                        td.m_methodImpls = ArrayUtility.AppendToArray( td.m_methodImpls, mi );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public MetaDataTypeDefinition Class
        {
            get
            {
                return m_classObject;
            }
        }

        public IMetaDataMethodDefOrRef Body
        {
            get
            {
                return m_body;
            }
        }

        public IMetaDataMethodDefOrRef Declaration
        {
            get
            {
                return m_declaration;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataMethodImpl(" + m_classObject + "," + m_body + "," + m_declaration + ")";
        }
    }
}

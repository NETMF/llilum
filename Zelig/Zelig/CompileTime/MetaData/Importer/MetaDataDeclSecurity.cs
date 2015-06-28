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
    public sealed class MetaDataDeclSecurity : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private short                    m_action;
        private IMetaDataHasDeclSecurity m_parent;
        private ArrayReader              m_permissionSet;

        //
        // Constructor Methods
        //

        private MetaDataDeclSecurity( int index ) : base( TokenType.DeclSecurity, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataDeclSecurity( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader parentReader = ts.m_columns[1].m_reader;
            int                parentIndex;
            int                permissionSetIndex;

            m_action           =                            reader.ReadInt16();
            parentIndex        =        parentReader      ( reader );
            permissionSetIndex = parser.readIndexAsForBlob( reader );

            m_parent        =                  parser.getHasDeclSecurity( parentIndex        );
            m_permissionSet = new ArrayReader( parser.getBlobBytes      ( permissionSetIndex ) );
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

        public short Action
        {
            get
            {
                return m_action;
            }
        }

        public IMetaDataHasDeclSecurity Parent
        {
            get
            {
                return m_parent;
            }
        }

        public ArrayReader PermissionSet
        {
            get
            {
                return m_permissionSet;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataDeclSecurity(" + m_action + "," + m_parent + "," + m_permissionSet + ")";
        }
    }
}

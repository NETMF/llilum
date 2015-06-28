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
    public sealed class MetaDataAssembly : MetaDataObject,
        IMetaDataHasDeclSecurity,
        IMetaDataNormalize
    {
        //
        // State
        //

        private MetaData        m_owner;
        private HashAlgorithmID m_hashAlgorithmId;
        private MetaDataVersion m_version;
        private String          m_name;
        private String          m_locale;

        //
        // Constructor Methods
        //

        private MetaDataAssembly( int index ) : base( TokenType.Assembly, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataAssembly( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            m_owner           = parser.MetaData;
            m_hashAlgorithmId = (HashAlgorithmID)         reader.ReadInt32();
            m_version         = ParseVersion( parser,     reader );
            m_name            = parser.readIndexAsString( reader );
            m_locale          = parser.readIndexAsString( reader );
        }

        internal static MetaDataVersion ParseVersion( Parser      parser ,
                                                      ArrayReader reader )
        {
            MetaDataVersion ver = new MetaDataVersion();

            ver.m_majorVersion   =                         reader.ReadInt16();
            ver.m_minorVersion   =                         reader.ReadInt16();
            ver.m_buildNumber    =                         reader.ReadInt16();
            ver.m_revisionNumber =                         reader.ReadInt16();
            ver.m_flags          = (AssemblyFlags)         reader.ReadInt32();
            ver.m_publicKey      = parser.readIndexAsBlob( reader );

            return ver;
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.MetaDataAssembly res = new Normalized.MetaDataAssembly( context.UniqueDictionary, m_token );

            res.m_hashAlgorithmId = m_hashAlgorithmId;
            res.m_version         = m_version;
            res.m_name            = m_name;
            res.m_locale          = m_locale;

            return res;
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataAssembly asml = (Normalized.MetaDataAssembly)obj;

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out asml.m_customAttributes, MetaDataNormalizationMode.Allocate );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public MetaData Owner
        {
            get
            {
                return m_owner;
            }
        }

        public HashAlgorithmID HashAlgorithmId
        {
            get
            {
                return m_hashAlgorithmId;
            }
        }

        public MetaDataVersion Version
        {
            get
            {
                return m_version;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public String Locale
        {
            get
            {
                return m_locale;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataAssembly(" );

            sb.Append( m_hashAlgorithmId );
            sb.Append( "," );
            sb.Append( m_version );
            sb.Append( "," );
            sb.Append( m_name );
            sb.Append( "," );
            sb.Append( m_locale );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

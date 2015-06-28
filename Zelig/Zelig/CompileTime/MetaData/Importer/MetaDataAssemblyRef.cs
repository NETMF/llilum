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
    public sealed class MetaDataAssemblyRef : MetaDataObject,
        IMetaDataImplementation,
        IMetaDataResolutionScope,
        IMetaDataNormalize
    {
        //
        // State
        //

        private MetaDataVersion m_version;
        private String          m_name;
        private String          m_locale;
        private byte[]          m_hashValue;

        //
        // Constructor Methods
        //

        private MetaDataAssemblyRef( int index ) : base( TokenType.AssemblyRef, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataAssemblyRef( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            m_version   = MetaDataAssembly.ParseVersion( parser, reader );
            m_name      = parser.readIndexAsString     (         reader );
            m_locale    = parser.readIndexAsString     (         reader );
            m_hashValue = parser.readIndexAsBlob       (         reader );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            MetaDataResolver.AssemblyPair pair = context.FindAssembly( m_name, m_version );

            return pair.Normalized;
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

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

        public byte[] HashValue
        {
            get
            {
                return m_hashValue;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataAssemblyRef(" );

            sb.Append( m_version );
            sb.Append( "," );
            sb.Append( m_name );
            sb.Append( "," );
            sb.Append( m_locale );
            sb.Append( ",[" );
            ArrayReader.AppendAsString( sb, m_hashValue );
            sb.Append( "])" );

            return sb.ToString();
        }
    }
}

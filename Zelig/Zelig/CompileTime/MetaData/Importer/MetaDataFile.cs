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
    public sealed class MetaDataFile : MetaDataObject,
        IMetaDataImplementation,
        IMetaDataNormalize
    {
        //
        // State
        //

        private FileAttributes m_flags;
        private String         m_name;
        private byte[]         m_hashValue;

        //
        // Constructor Methods
        //

        private MetaDataFile( int index ) : base( TokenType.File, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataFile( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            m_flags     = (FileAttributes)          reader.ReadInt32();
            m_name      = parser.readIndexAsString( reader );
            m_hashValue = parser.readIndexAsBlob  ( reader );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            // LT72: added normalization for metadatafile; not necessary with previous implementation on fraemwork 3.5
            //throw context.InvalidPhase( this );

            Normalized.MetaDataFile file = new Normalized.MetaDataFile( this.Token );
            file.m_flags = this.m_flags;
            file.m_name = this.m_name;
            file.m_hashValue = this.m_hashValue;

            return file;
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public FileAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
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
            return "MetaDataFile(" + m_name + ")";
        }

        public override String ToStringLong()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataFile(" );

            sb.Append( m_flags.ToString( "x" ) );
            sb.Append( "," );
            sb.Append( m_name );
            sb.Append( ",[" );

            for(int i = 0; i < m_hashValue.Length; i++)
            {
                if(i != 0)
                {
                    sb.Append( "," );
                }

                sb.Append( "0x" );
                sb.Append( m_hashValue[i].ToString( "x2" ) );
            }

            sb.Append( "])" );

            return sb.ToString();
        }
    }
}

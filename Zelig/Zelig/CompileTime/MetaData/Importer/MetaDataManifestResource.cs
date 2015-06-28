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
    public sealed class MetaDataManifestResource : MetaDataObject,
        IMetaDataNormalize
    {
        //
        // State
        //

        private int                          m_offset;
        private ManifestResourceAttributes   m_flags;
        private String                       m_name;
        private byte[]                       m_data;
        private IMetaDataImplementation      m_implementation;
        private Dictionary< string, object > m_values;

        //
        // Constructor Methods
        //

        private MetaDataManifestResource( int index ) : base( TokenType.ManifestResource, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataManifestResource( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader implementationReader = ts.m_columns[3].m_reader;
            int                implementationIndex;

            m_offset            =                               reader.ReadInt32();
            m_flags             = (ManifestResourceAttributes)  reader.ReadInt32();
            m_name              = parser.readIndexAsString    ( reader );
            implementationIndex =        implementationReader ( reader );

            m_implementation = parser.getImplementation( implementationIndex );
            m_data           = (implementationIndex == 0) ? parser.getResourceBytes( m_offset ) : null;

            try
            {
                if(m_data != null)
                {
                    System.IO.MemoryStream stream = new System.IO.MemoryStream( m_data );

                    if(stream.CanSeek && stream.Length > 4)
                    {
                        System.IO.BinaryReader br = new System.IO.BinaryReader( stream );

                        if(br.ReadInt32() == System.Resources.ResourceManager.MagicNumber)
                        {
                            stream.Seek( 0, System.IO.SeekOrigin.Begin );

                            System.Resources.ResourceSet rs = new System.Resources.ResourceSet( stream );

                            Dictionary< string, object > values = new Dictionary< string, object >();

                            foreach(System.Collections.DictionaryEntry entry in rs)
                            {
                                string name = (string)entry.Key;

                                values[name] = entry.Value;
                            }

                            m_values = values;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ConversionOfResources:
                    {
                        Normalized.MetaDataAssembly         owner  = (Normalized.MetaDataAssembly)context.Value;
                        Normalized.MetaDataManifestResource resNew = new Normalized.MetaDataManifestResource( owner, m_token );

                        resNew.m_offset = m_offset;
                        resNew.m_flags  = m_flags;
                        resNew.m_name   = m_name;
                        resNew.m_data   = m_data;
                        resNew.m_values = m_values;

                        context.GetNormalizedObject( m_implementation, out resNew.m_implementation, MetaDataNormalizationMode.Default );

                        owner.Resources.Add( resNew );

                        return resNew;
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

        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        public ManifestResourceAttributes Flags
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

        // Returns one of MetaData{File,AssemblyRef,ExportedType}
        public IMetaDataImplementation Implementation
        {
            get
            {
                return m_implementation;
            }
        }

        public byte[] Data
        {
            get
            {
                return m_data;
            }
        }

        public Dictionary< string, object > Values
        {
            get
            {
                return m_values;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataManifestResource(" + m_name + ")";
        }

        public override String ToStringLong()
        {
            if(m_implementation == null)
            {
                return "MetaDataManifestResource(" + m_offset + "," + m_flags.ToString( "x" ) + "," + m_name + "," + m_data + ")";
            }
            else
            {
                return "MetaDataManifestResource(" + m_offset + "," + m_flags.ToString( "x" ) + "," + m_name + "," + m_implementation + ")";
            }
        }
    }
}

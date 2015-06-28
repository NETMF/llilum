//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;
    using System.Collections.Generic;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataAssembly : MetaDataObject,
        IMetaDataHasDeclSecurity,
        IMetaDataUnique
    {
        //
        // State
        //

        internal HashAlgorithmID                                    m_hashAlgorithmId;
        internal MetaDataVersion                                    m_version;
        internal String                                             m_name;
        internal String                                             m_locale;

        //--//

        internal List< MetaDataTypeDefinitionAbstract             > m_typesList;
        internal List< MetaDataTypeDefinitionGenericInstantiation > m_instantiatedTypesList;
        internal List< MetaDataTypeDefinitionAbstract             > m_otherTypesList;
        internal List< MetaDataTypeDefinitionArray                > m_pendingArraysList;

        internal List< MetaDataManifestResource >                   m_resourcesList;

        internal GrowOnlySet< IMetaDataUnique >                     m_lookupUniques;

        internal MetaDataMethodBase                                 m_entryPoint;

        //
        // Constructor Methods
        //

        internal MetaDataAssembly( GrowOnlySet< IMetaDataUnique > lookupUniques ,
                                   int                            token         ) : base( token )
        {
            m_typesList             = new List< MetaDataTypeDefinitionAbstract             >();
            m_instantiatedTypesList = new List< MetaDataTypeDefinitionGenericInstantiation >();
            m_otherTypesList        = new List< MetaDataTypeDefinitionAbstract             >();
            m_pendingArraysList     = new List< MetaDataTypeDefinitionArray                >();

            m_resourcesList         = new List< MetaDataManifestResource >();

            m_lookupUniques         = lookupUniques;
        }

        //--//

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataAssembly) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataAssembly other = (MetaDataAssembly)obj;

                if(m_hashAlgorithmId == other.m_hashAlgorithmId &&
                   m_name            == other.m_name            &&
                   m_locale          == other.m_locale           )
                {
                    if(m_version.Equals( other.m_version ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_hashAlgorithmId               ^
                        m_name           .GetHashCode() ;
        }

        //
        // Helper Methods
        //

        internal override MetaDataObject MakeUnique()
        {
            return (MetaDataObject)MakeUnique( this );
        }

        internal IMetaDataUnique MakeUnique( IMetaDataUnique obj )
        {
            IMetaDataUnique res;

            if(m_lookupUniques.Contains( obj, out res ))
            {
////            if(res != obj)
////            {
////                DebugPrint( "MakeUnique:\r\n    IN :{0}\r\n    OUT:{1}", obj, res );
////
////                obj = res;
////            }

                return res;
            }
            else
            {
                m_lookupUniques.Insert( obj );

                if(obj is MetaDataTypeDefinitionAbstract)
                {
                    MetaDataTypeDefinitionAbstract td   = (MetaDataTypeDefinitionAbstract)obj;
                    MetaDataAssembly               asml = td.Owner;

                    if(td is MetaDataTypeDefinitionBase)
                    {
                        //throw new NotNormalized( "Unexpected type: " + td );
                    }
                    else if(td is MetaDataTypeDefinitionGenericInstantiation)
                    {
                        asml.InstantiatedTypes.Add( (MetaDataTypeDefinitionGenericInstantiation)td );
                    }
                    else if(td is MetaDataTypeDefinitionArray)
                    {
                        asml.PendingArrays.Add( (MetaDataTypeDefinitionArray)td );
                    }
                    else
                    {
                        asml.OtherTypes.Add( td );
                    }
                }

                return obj;
            }
        }

        //
        // Access Methods
        //

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

        public MetaDataMethodBase EntryPoint
        {
            get
            {
                return m_entryPoint;
            }
        }

        public List< MetaDataTypeDefinitionAbstract > Types
        {
            get
            {
                return m_typesList;
            }
        }

        public List< MetaDataTypeDefinitionGenericInstantiation > InstantiatedTypes
        {
            get
            {
                return m_instantiatedTypesList;
            }
        }

        public List< MetaDataTypeDefinitionAbstract > OtherTypes
        {
            get
            {
                return m_otherTypesList;
            }
        }

        public List< MetaDataManifestResource > Resources
        {
            get
            {
                return m_resourcesList;
            }
        }

        public List< MetaDataTypeDefinitionArray > PendingArrays
        {
            get
            {
                return m_pendingArraysList;
            }
        }

////    public MetaDataFile[] files
////    {
////        get
////        {
////            return this.fileArray;
////        }
////    }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataAssembly(" );

            sb.Append( m_hashAlgorithmId );
            sb.Append( ","               );
            sb.Append( m_version         );
            sb.Append( ","               );
            sb.Append( m_name            );
            sb.Append( ","               );
            sb.Append( m_locale          );

            sb.Append( ")" );

            return sb.ToString();
        }

        public override void Dump( IMetaDataDumper writer )
        {
            writer.WriteLine( ".assembly {0} {1} {2}", TokenToString( m_token ), m_name, m_version );

            writer.IndentPush( "{" );

            if(m_customAttributes != null)
            {
                foreach(MetaDataCustomAttribute ca in m_customAttributes)
                {
                    writer.Process( ca, false );
                }
                writer.WriteLine();
            }

            writer.IndentPop( "}" );

            writer.WriteLine();

            foreach(MetaDataTypeDefinitionAbstract td in m_typesList)
            {
                writer.Process( td, true );
            }
        }
    }
}

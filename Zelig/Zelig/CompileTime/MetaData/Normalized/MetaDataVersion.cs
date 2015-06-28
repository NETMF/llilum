//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class MetaDataVersion : Normalized.IMetaDataUnique
    {
        //
        // State
        //

        internal short         m_majorVersion;
        internal short         m_minorVersion;
        internal short         m_buildNumber;
        internal short         m_revisionNumber;
        internal AssemblyFlags m_flags;
        internal byte[]        m_publicKey;

        //
        // Constructor Methods
        //

        internal MetaDataVersion()
        {
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is MetaDataVersion) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                MetaDataVersion other = (MetaDataVersion)obj;

                return IsCompatible( other, true );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_majorVersion   << 15 ^
                   (int)m_minorVersion   << 10 ^
                   (int)m_buildNumber    <<  5 ^
                   (int)m_revisionNumber       ;
        }

        //
        // Helper Methods
        //

        public bool IsCompatible( MetaDataVersion ver   ,
                                  bool            exact )
        {
            // BUGBUG: This ignores the strong name of an assembly.

            if(m_majorVersion   < ver.m_majorVersion            ) return false;
            if(m_majorVersion   > ver.m_majorVersion   && !exact) return true;

            if(m_minorVersion   < ver.m_minorVersion            ) return false;
            if(m_minorVersion   > ver.m_minorVersion   && !exact) return true;

            if(m_buildNumber    < ver.m_buildNumber             ) return false;
            if(m_buildNumber    > ver.m_buildNumber    && !exact) return true;

            if(m_revisionNumber < ver.m_revisionNumber          ) return false;
            if(m_revisionNumber > ver.m_revisionNumber && !exact) return true;

            return true;
        }

        //
        // Access Methods
        //

        public short MajorVersion
        {
            get
            {
                return m_majorVersion;
            }
        }

        public short MinorVersion
        {
            get
            {
                return m_minorVersion;
            }
        }

        public short BuildNumber
        {
            get
            {
                return m_buildNumber;
            }
        }

        public short RevisionNumber
        {
            get
            {
                return m_revisionNumber;
            }
        }

        public AssemblyFlags Flags
        {
            get
            {
                return m_flags;
            }
        }

        public byte[] PublicKey
        {
            get
            {
                return m_publicKey;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataVersion(" );

            sb.Append( m_majorVersion                              );
            sb.Append( ","                                         );
            sb.Append( m_minorVersion                              );
            sb.Append( ","                                         );
            sb.Append( m_buildNumber                               );
            sb.Append( ","                                         );
            sb.Append( m_revisionNumber                            );
            sb.Append( ","                                         );
            sb.Append( m_flags                                     );
            sb.Append( ",["                                        );
            // BUGBUG: ArrayReader.AppendAsString( sb, m_publicKey );
            sb.Append( "])" );

            return sb.ToString();
        }
    }
}

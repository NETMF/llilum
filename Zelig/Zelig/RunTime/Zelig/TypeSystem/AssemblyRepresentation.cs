//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class AssemblyRepresentation
    {
        public struct VersionData
        {
            //
            // This is just a copy of Microsoft.Zelig.MetaData.AssemblyFlags, needed to break the dependency of TypeSystem from MetaData.
            //
            public enum AssemblyFlags
            {
                PublicKey                  = 0x0001, // The assembly ref holds the full (unhashed) public key.
                CompatibilityMask          = 0x0070,
                SideBySideCompatible       = 0x0000, // The assembly is side by side compatible.
                NonSideBySideAppDomain     = 0x0010, // The assembly cannot execute with other versions if they are executing in the same application domain.
                NonSideBySideProcess       = 0x0020, // The assembly cannot execute with other versions if they are executing in the same process.
                NonSideBySideMachine       = 0x0030, // The assembly cannot execute with other versions if they are executing on the same machine.
                EnableJITcompileTracking   = 0x8000, // From "DebuggableAttribute".
                DisableJITcompileOptimizer = 0x4000  // From "DebuggableAttribute".
            }

            //
            // State
            //

            public short         MajorVersion;
            public short         MinorVersion;
            public short         BuildNumber;
            public short         RevisionNumber;
            public AssemblyFlags Flags;
            public byte[]        PublicKey;

            //
            // Helper Methods
            //

            public bool IsCompatible( ref VersionData ver   ,
                                          bool        exact )
            {
                // BUGBUG: This ignores the strong name of an assembly.

                if(this.MajorVersion   < ver.MajorVersion            ) return false;
                if(this.MajorVersion   > ver.MajorVersion   && !exact) return true;

                if(this.MinorVersion   < ver.MinorVersion            ) return false;
                if(this.MinorVersion   > ver.MinorVersion   && !exact) return true;

                if(this.BuildNumber    < ver.BuildNumber             ) return false;
                if(this.BuildNumber    > ver.BuildNumber    && !exact) return true;

                if(this.RevisionNumber < ver.RevisionNumber          ) return false;
                if(this.RevisionNumber > ver.RevisionNumber && !exact) return true;

                return true;
            }

            public void ApplyTransformation( TransformationContext context )
            {
                context.Transform( ref this.MajorVersion   );
                context.Transform( ref this.MinorVersion   );
                context.Transform( ref this.BuildNumber    );
                context.Transform( ref this.RevisionNumber );
                context.Transform( ref this.Flags          );
                context.Transform( ref this.PublicKey      );
            }

            //
            // Access Methods
            //

            //
            // Debug Methods
            //

            public override String ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataVersion(" );

                sb.Append( MajorVersion                              );
                sb.Append( ","                                       );
                sb.Append( MinorVersion                              );
                sb.Append( ","                                       );
                sb.Append( BuildNumber                               );
                sb.Append( ","                                       );
                sb.Append( RevisionNumber                            );
                sb.Append( ","                                       );
                sb.Append( Flags                                     );
                sb.Append( ",["                                      );
                // BUGBUG: ArrayReader.AppendAsString( sb, PublicKey );
                sb.Append( "])" );

                return sb.ToString();
            }
        }

        //
        // State
        //

        private string      m_name;
        private VersionData m_version;

        //
        // Constructor Methods
        //

        public AssemblyRepresentation(     string      name    ,
                                       ref VersionData version )
        {
            CHECKS.ASSERT( name != null, "Cannot create an AssemblyRepresentation without a name" );

            m_name    = name;
            m_version = version;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is AssemblyRepresentation)
            {
                AssemblyRepresentation other = (AssemblyRepresentation)obj;

                if(m_name == other.m_name)
                {
                    if(m_version.IsCompatible( ref other.m_version, true ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return      m_name.GetHashCode()     ^
                   (int)m_version.MajorVersion   ^
                   (int)m_version.MinorVersion   ^
                   (int)m_version.BuildNumber    ^
                   (int)m_version.RevisionNumber;
        }

        //--//

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContext context )
        {
            context.Push( this );

            context.Transform( ref m_name    );
            context.Transform( ref m_version );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public VersionData Version
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
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System
{
    using System.Globalization;

    // A Version object contains four hierarchical numeric components: major, minor,
    // revision and build.  Revision and build may be unspecified, which is represented
    // internally as a -1.  By definition, an unspecified component matches anything
    // (both unspecified and specified), and an unspecified component is "less than" any
    // specified component.

    public sealed class Version // : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
    {
        // AssemblyName depends on the order staying the same
        private int m_major;
        private int m_minor;
        private int m_build;    // = -1;
        private int m_revision; // = -1;

        //--//

        public static bool operator==( Version v1, Version v2 )
        {
            if(object.ReferenceEquals( v1, null ))
            {
                return object.ReferenceEquals( v2, null );
            }
            return v1.Equals( v2 );
        }

        public static bool operator !=( Version v1, Version v2 )
        {
            return !( v1 == v2 );
        }

        public static bool operator >( Version v1, Version v2 )
        {
            return v2 < v1;
        }

        public static bool operator >=( Version v1, Version v2 )
        {
            return v2 <= v1;
        }

        public static bool operator <( Version v1, Version v2 )
        {
            if(v1 == null)
            {
                throw new ArgumentNullException( "v1" );
            }
            return v1.CompareTo( v2 ) < 0;
        }

        public static bool operator <=( Version v1, Version v2 )
        {
            if(v1 == null)
            {
                throw new ArgumentNullException( "v1" );
            }
            return v1.CompareTo( v2 ) <= 0;
        }

        //--//

        public Version(int major, int minor, int build, int revision)
        {
            if(major < 0 || minor < 0 || revision < 0 || build < 0)
                throw new ArgumentOutOfRangeException( );

            m_major    = major;
            m_minor    = minor;
            m_revision = revision;
            m_build    = build;
        }

        public Version(int major, int minor)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException();

            if (minor < 0)
                throw new ArgumentOutOfRangeException();

            m_major = major;
            m_minor = minor;

            // Other 2 initialize to -1 as it done on desktop and CE
            m_build = -1;
            m_revision = -1;
        }

        // Properties for setting and getting version numbers
        public int Major
        {
            get { return m_major; }
        }

        public int Minor
        {
            get { return m_minor; }
        }

        public int Revision
        {
            get { return m_revision; }
        }

        public int Build
        {
            get { return m_build; }
        }

        public override bool Equals(Object obj)
        {
            if(((Object)obj == null    ) ||
                ( !(    obj is Version ) ))
            {
                return false;
            }

            Version v = (Version)obj;
            // check that major, minor, build & revision numbers match
            if( ( this.m_major != v.m_major )     ||
                ( this.m_minor != v.m_minor )     ||
                ( this.m_build != v.m_build )     ||
                ( this.m_revision != v.m_revision ))
            {
                return false;
            }

            return true;
        }

        public int CompareTo( Version value )
        {
            if(value == null)
            {
                return 1;
            }
            if(this.m_major != value.m_major)
            {
                if(this.m_major > value.m_major)
                {
                    return 1;
                }
                return -1;
            }
            else if(this.m_minor != value.m_minor)
            {
                if(this.m_minor > value.m_minor)
                {
                    return 1;
                }
                return -1;
            }
            else if(this.m_build != value.m_build)
            {
                if(this.m_build > value.m_build)
                {
                    return 1;
                }
                return -1;
            }
            else
            {
                if(this.m_revision == value.m_revision)
                {
                    return 0;
                }
                if(this.m_revision > value.m_revision)
                {
                    return 1;
                }
                return -1;
            }
        }

        public override int GetHashCode( )
        {
	        return 0 | (this.m_major & 15) << 28 | (this.m_minor & 255) << 20 | (this.m_build & 255) << 12 | (this.m_revision & 4095);
        }

        public override String ToString()
        {
            string retStr = m_major + "." + m_minor;

            // Adds m_build and then m_revision if they are positive. They could be -1 in this case not added.
            if (m_build >= 0)
            {
                retStr += "." + m_build;
                if (m_revision >= 0)
                {
                    retStr += "." + m_revision;
                }
            }

            return retStr;
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    public class InstructionSetVersion
    {
        //
        // Platforms
        //
        public const string Platform_ARM = "ARM";

        //
        // Platform Version
        //
        public const string PlatformVersion_4 = "v4";
        public const string PlatformVersion_5 = "v5";
        public const string PlatformVersion_6 = "v6";
        public const string PlatformVersion_7M = "v7-M";
        public const string PlatformVersion_7R = "v7-R";
        public const string PlatformVersion_7A = "v7-A";

        //
        // Platform Version
        //
        public const string PlatformVFP_NoVFP   = "";
        public const string PlatformVFP_VFP     = "vfp";
        public const string PlatformVFP_SoftVFP = "softvfp";

        //
        // State
        //

        private readonly string m_platformName;
        private readonly string m_isaVersion;
        private readonly string m_vfpVersion;

        //
        // Constructor Methods
        //

        public InstructionSetVersion(string platformName, string isaVersion, string vfpVersion)
        {
            m_platformName = platformName;
            m_isaVersion   = isaVersion;
            m_vfpVersion   = vfpVersion;
        }

        public string PlatformName
        {
            get
            {
                return m_platformName;
            }
        }

        public string ISAVersion
        {
            get
            {
                return m_isaVersion;
            }
        }

        public string VFPVersion
        {
            get
            {
                return m_vfpVersion;
            }
        }

        //
        // IEquals
        //

        public override bool Equals(object obj)
        {
            InstructionSetVersion match = obj as InstructionSetVersion;
            if(match != null)
            {
                return ((m_platformName == match.m_platformName) && (m_isaVersion == match.m_isaVersion) && (m_vfpVersion == match.m_vfpVersion));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

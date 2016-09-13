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
        public const string CodeGenerator_Zelig = "Zelig";
        public const string CodeGenerator_LLVM  = "LLVM";

        //
        // Platform descriptors fields: fields in the same group are mutually exclusive
        // 

        //
        // Platforms
        //
        public const uint Platform_Family__ARM      = 0x01000000;
        public const uint Platform_Family__Cortex   = 0x02000000;
        public const uint Platform_Family__Mask     = 0xFF000000;

        //
        // Platform Version
        //
        public const uint Platform_Version__ARMv4       = 0x00000001;
        public const uint Platform_Version__ARMv5       = 0x00000002;
        public const uint Platform_Version__ARMv6M      = 0x00000004;
        public const uint Platform_Version__ARMv7M      = 0x00000008;
        public const uint Platform_Version__ARMv7R      = 0x00000010;
        public const uint Platform_Version__ARMv7A      = 0x00000020;
        public const uint Platform_Version__x86         = 0x00000100;
        public const uint Platform_Version__ARMv7_all   = 0x00000038;
        public const uint Platform_Version__ARM_legacy  = 0x00000003;
        public const uint Platform_Version__Mask        = 0x0000FFFF;

        //
        // Platform VFP Version
        //
        public const uint Platform_VFP__NoVFP       = 0x00010000;
        public const uint Platform_VFP__SoftVFP     = 0x00020000;
        public const uint Platform_VFP__HardVFP     = 0x00040000;
        public const uint Platform_VFP__Mask        = 0x00FF0000;
        
        //--//

        //
        // State
        //

        private uint m_platformDescriptor;

        //
        // Constructor Methods
        //

        public static InstructionSetVersion ARM
        {
            get
            {
                return new InstructionSetVersion( Platform_Family__ARM );
            }
        }

        public static InstructionSetVersion Cortex
        {
            get
            {
                return new InstructionSetVersion( Platform_Family__Cortex );
            }
        }

        public static InstructionSetVersion Build( uint descriptor )
        {
            return new InstructionSetVersion( descriptor );
        }

        public InstructionSetVersion With( uint descriptor )
        {
            m_platformDescriptor |= descriptor;

            ValidateDescriptor( descriptor );

            return this;
        }

        private InstructionSetVersion( uint descriptor )
        {
            m_platformDescriptor = descriptor;
        }

        public uint PlatformFamily
        {
            get
            {
                return m_platformDescriptor & Platform_Family__Mask;
            }
        }

        public uint PlatformVersion
        {
            get
            {
                return m_platformDescriptor & Platform_Version__Mask;
            }
        }

        public uint PlatformVFPSupport
        {
            get
            {
                return m_platformDescriptor & Platform_VFP__Mask;
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
                return m_platformDescriptor == match.m_platformDescriptor;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //--//
        
        private void ValidateDescriptor( uint descriptor )
        {
            //
            // it is OK to set the same descriptor twice, but it is not OK to change fields
            //
            uint family  = descriptor & Platform_Family__Mask;
            uint version = descriptor & Platform_Version__Mask;
            uint vfp     = descriptor & Platform_VFP__Mask;
            
            uint familyCurrent  = m_platformDescriptor & Platform_Family__Mask;
            uint versionCurrent = m_platformDescriptor & Platform_Version__Mask;
            uint vfpCurrent     = m_platformDescriptor & Platform_VFP__Mask;

            if( family  != 0 && family  != familyCurrent    ||
                version != 0 && version != versionCurrent   ||
                vfp     != 0 && vfp     != vfpCurrent        )
            {
                throw new ArgumentException( String.Format( "Descriptor {0} is incompatible with current values {1}", descriptor, m_platformDescriptor ) );
            }
        }
    }
}

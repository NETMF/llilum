using System;

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    public class CurrentInstructionSetEncoding 
    {
        //
        // Singleton
        // 
        private static EncodingDefinition s_CurrentEncoding;
        private static EncodingDefinition_VFP s_CurrentEncodingVFP;
        private static object s_sync = new object();

        protected CurrentInstructionSetEncoding()
        {
        }

        //--//

        public static void RegisterCurrentEncoding( InstructionSetVersion isv )
        {
            lock(s_sync)
            {
                if(s_CurrentEncoding != null)
                {
                    throw new InvalidOperationException("Cannot change encoding");
                }

                EncodingDefinition_ARM enc = null;
                switch(isv.PlatformVersion)
                {
                    case InstructionSetVersion.Platform_Version__ARMv4:
                    case InstructionSetVersion.Platform_Version__ARMv5:
                        enc = new EncodingDefinition_ARM();
                        break;
                    case InstructionSetVersion.Platform_Version__ARMv6M:
                    case InstructionSetVersion.Platform_Version__ARMv7M:
                    case InstructionSetVersion.Platform_Version__ARMv7R:
                    case InstructionSetVersion.Platform_Version__ARMv7A:
                    default:
                        throw new ArgumentException("Cannot register unsupported instruction set");
                }
                
                EncodingDefinition_VFP encVFP = null;
                switch(isv.PlatformVFPSupport)
                {
                    case InstructionSetVersion.Platform_VFP__HardVFP:
                        encVFP = new EncodingDefinition_VFP_ARM();
                        break;
                    case InstructionSetVersion.Platform_VFP__SoftVFP:
                    case InstructionSetVersion.Platform_VFP__NoVFP:
                        break;
                    default:
                        throw new ArgumentException("Cannot register unsupported instruction set");
                }
                
                s_CurrentEncoding = enc;
                s_CurrentEncodingVFP = encVFP;
            }
        }

        public static EncodingDefinition GetEncoding()
        {
            if(s_CurrentEncoding == null)
            {
                s_CurrentEncoding = new EncodingDefinition_ARM();
                //throw new InvalidOperationException("Instruction set encoding is unknown");
            }
            return s_CurrentEncoding;
        }

        public static EncodingDefinition_VFP GetVFPEncoding()
        {
            return s_CurrentEncodingVFP;
        }
    }
}

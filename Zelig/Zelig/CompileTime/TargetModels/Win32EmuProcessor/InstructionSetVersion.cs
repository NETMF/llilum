namespace Microsoft.Zelig.TargetModel.Win32
{
    public class InstructionSetVersion
    {
        //
        // Platforms
        //
        public const string CodeGenerator_LLVM  = "LLVM";

        //
        // Platform descriptors fields: fields in the same group are mutually exclusive
        // 

        //
        // Platforms
        //
        public const uint Platform_Family__Win32    = 0x10000000;
        public const uint Platform_Family__Mask     = 0xFF000000;

        //
        // Platform Version
        //
        public const uint Platform_Version__x86     = 0x00000100;
        public const uint Platform_Version__Mask    = 0x0000FFFF;

        //
        // Platform VFP Version
        //
        public const uint Platform_VFP__NoVFP       = 0x00010000;
        public const uint Platform_VFP__Mask        = 0x00FF0000;
    }
}

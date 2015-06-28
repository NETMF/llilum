//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define VERIFY_INSTRUCTIONSET_VFP


namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_VFP_ARM;

    public class InstructionSet_VFP : InstructionSet
    {
        private static EncodingDefinition_VFP_ARM s_EncodingVFP = (EncodingDefinition_VFP_ARM)CurrentInstructionSetEncoding.GetVFPEncoding();

        //
        // State
        //

        private Opcode_VFP_DataTransfer            m_DataTransfer;
        private Opcode_VFP_BlockDataTransfer       m_BlockDataTransfer;
        private Opcode_VFP_ConditionCodeTransfer   m_ConditionCodeTransfer;
        private Opcode_VFP_SystemRegisterTransfer  m_SystemRegisterTransfer;
        private Opcode_VFP_64bitRegisterTransfer   m_64bitRegisterTransfer;
        private Opcode_VFP_32bitRegisterTransfer   m_32bitRegisterTransfer;
        private Opcode_VFP_32bitLoRegisterTransfer m_32bitLoRegisterTransfer;
        private Opcode_VFP_32bitHiRegisterTransfer m_32bitHiRegisterTransfer;
        private Opcode_VFP_CompareToZero           m_CompareToZero;
        private Opcode_VFP_ConvertFloatToFloat     m_ConvertFloatToFloat;
        private Opcode_VFP_UnaryDataOperation      m_UnaryDataOperation;
        private Opcode_VFP_BinaryDataOperation     m_BinaryDataOperation;

        //
        // Constructor Methods
        //

        public InstructionSet_VFP( InstructionSetVersion version ) : base(version)
        {
            m_DataTransfer            = new Opcode_VFP_DataTransfer           ();
            m_BlockDataTransfer       = new Opcode_VFP_BlockDataTransfer      ();
            m_ConditionCodeTransfer   = new Opcode_VFP_ConditionCodeTransfer  ();
            m_SystemRegisterTransfer  = new Opcode_VFP_SystemRegisterTransfer ();
            m_64bitRegisterTransfer   = new Opcode_VFP_64bitRegisterTransfer  ();
            m_32bitRegisterTransfer   = new Opcode_VFP_32bitRegisterTransfer  ();
            m_32bitLoRegisterTransfer = new Opcode_VFP_32bitLoRegisterTransfer();
            m_32bitHiRegisterTransfer = new Opcode_VFP_32bitHiRegisterTransfer();
            m_CompareToZero           = new Opcode_VFP_CompareToZero          ();
            m_ConvertFloatToFloat     = new Opcode_VFP_ConvertFloatToFloat    ();
            m_UnaryDataOperation      = new Opcode_VFP_UnaryDataOperation     ();
            m_BinaryDataOperation     = new Opcode_VFP_BinaryDataOperation    ();
        }

        //--//

        //
        // Helper Methods
        //

        public Opcode_VFP_DataTransfer            PrepareForVFP_DataTransfer            { get { return m_DataTransfer           ; } }
        public Opcode_VFP_BlockDataTransfer       PrepareForVFP_BlockDataTransfer       { get { return m_BlockDataTransfer      ; } }
        public Opcode_VFP_ConditionCodeTransfer   PrepareForVFP_ConditionCodeTransfer   { get { return m_ConditionCodeTransfer  ; } }
        public Opcode_VFP_SystemRegisterTransfer  PrepareForVFP_SystemRegisterTransfer  { get { return m_SystemRegisterTransfer ; } }
        public Opcode_VFP_64bitRegisterTransfer   PrepareForVFP_64bitRegisterTransfer   { get { return m_64bitRegisterTransfer  ; } }
        public Opcode_VFP_32bitRegisterTransfer   PrepareForVFP_32bitRegisterTransfer   { get { return m_32bitRegisterTransfer  ; } }
        public Opcode_VFP_32bitLoRegisterTransfer PrepareForVFP_32bitLoRegisterTransfer { get { return m_32bitLoRegisterTransfer; } }
        public Opcode_VFP_32bitHiRegisterTransfer PrepareForVFP_32bitHiRegisterTransfer { get { return m_32bitHiRegisterTransfer; } }
        public Opcode_VFP_CompareToZero           PrepareForVFP_CompareToZero           { get { return m_CompareToZero          ; } }
        public Opcode_VFP_ConvertFloatToFloat     PrepareForVFP_ConvertFloatToFloat     { get { return m_ConvertFloatToFloat    ; } }
        public Opcode_VFP_UnaryDataOperation      PrepareForVFP_UnaryDataOperation      { get { return m_UnaryDataOperation     ; } }
        public Opcode_VFP_BinaryDataOperation     PrepareForVFP_BinaryDataOperation     { get { return m_BinaryDataOperation    ; } }

        //--//

        public override Opcode Decode( uint op )
        {
            Opcode opcode = null;

            if     ((op & EncDef.opmask_ConditionCodeTransfer  ) == EncDef.op_ConditionCodeTransfer  ) opcode = m_ConditionCodeTransfer  ;
            else if((op & EncDef.opmask_SystemRegisterTransfer ) == EncDef.op_SystemRegisterTransfer ) opcode = m_SystemRegisterTransfer ;
            else if((op & EncDef.opmask_32bitLoRegisterTransfer) == EncDef.op_32bitLoRegisterTransfer) opcode = m_32bitLoRegisterTransfer;
            else if((op & EncDef.opmask_32bitHiRegisterTransfer) == EncDef.op_32bitHiRegisterTransfer) opcode = m_32bitHiRegisterTransfer;
            else if((op & EncDef.opmask_32bitRegisterTransfer  ) == EncDef.op_32bitRegisterTransfer  ) opcode = m_32bitRegisterTransfer  ;
            else if((op & EncDef.opmask_64bitRegisterTransfer  ) == EncDef.op_64bitRegisterTransfer  ) opcode = m_64bitRegisterTransfer  ;
            else if((op & EncDef.opmask_DataTransfer           ) == EncDef.op_DataTransfer           ) opcode = m_DataTransfer           ;
            else if((op & EncDef.opmask_CompareToZero          ) == EncDef.op_CompareToZero          ) opcode = m_CompareToZero          ;
            else if((op & EncDef.opmask_ConvertFloatToFloat    ) == EncDef.op_ConvertFloatToFloat    ) opcode = m_ConvertFloatToFloat    ;
            else if((op & EncDef.opmask_UnaryDataOperation     ) == EncDef.op_UnaryDataOperation     ) opcode = m_UnaryDataOperation     ;
            else if((op & EncDef.opmask_BinaryDataOperation    ) == EncDef.op_BinaryDataOperation    ) opcode = m_BinaryDataOperation    ;
            else if((op & EncDef.opmask_BlockDataTransfer      ) == EncDef.op_BlockDataTransfer      ) opcode = m_BlockDataTransfer      ;

            if(opcode != null)
            {
                opcode.Decode( op );

                return opcode;
            }

            return base.Decode( op );
        }

        //
        // Debug Methods
        //

#if VERIFY_INSTRUCTIONSET_VFP
        class Data
        {
            public string Name;
            public uint   Opcode;
        }

        static Data[] c_Data = new Data[] { 

            new Data { Name = "FCPYS"  , Opcode = 0xeeb00a40 },
            new Data { Name = "FMRS"   , Opcode = 0xee100a10 },
            new Data { Name = "FMSR"   , Opcode = 0xee000a10 },
            new Data { Name = "FMSTAT" , Opcode = 0xeef1fa10 },
            new Data { Name = "FSITOS" , Opcode = 0xeeb80ac0 },
            new Data { Name = "FUITOS" , Opcode = 0xeeb80a40 },
            new Data { Name = "FTOSIS" , Opcode = 0xeebd0a40 },
            new Data { Name = "FTOSIZS", Opcode = 0xeebd0ac0 },
            new Data { Name = "FTOUIS" , Opcode = 0xeebc0a40 },
            new Data { Name = "FTOUIZS", Opcode = 0xeebc0ac0 },
            new Data { Name = "FMRX"   , Opcode = 0xeef00a10 },
            new Data { Name = "FMXR"   , Opcode = 0xeee00a10 },

            new Data { Name = "FLDS"   , Opcode = 0xed100a00 },
            new Data { Name = "FSTS"   , Opcode = 0xed000a00 },
            new Data { Name = "FLDMIAS", Opcode = 0xec900a00 },
            new Data { Name = "FLDMDBS", Opcode = 0xed300a00 },
////        new Data { Name = "FLDMIAX", Opcode = 0xec900b00 },
////        new Data { Name = "FLDMDBX", Opcode = 0xed300b00 },
            new Data { Name = "FSTMIAS", Opcode = 0xec800a00 },
            new Data { Name = "FSTMDBS", Opcode = 0xed200a00 },
////        new Data { Name = "FSTMIAX", Opcode = 0xec800b00 },
////        new Data { Name = "FSTMDBX", Opcode = 0xed200b00 },

            new Data { Name = "FABSS"  , Opcode = 0xeeb00ac0 },
            new Data { Name = "FNEGS"  , Opcode = 0xeeb10a40 },
            new Data { Name = "FSQRTS" , Opcode = 0xeeb10ac0 },

            new Data { Name = "FADDS"  , Opcode = 0xee300a00 },
            new Data { Name = "FSUBS"  , Opcode = 0xee300a40 },
            new Data { Name = "FMULS"  , Opcode = 0xee200a00 },
            new Data { Name = "FDIVS"  , Opcode = 0xee800a00 },
            new Data { Name = "FMACS"  , Opcode = 0xee000a00 },
            new Data { Name = "FMSCS"  , Opcode = 0xee100a00 },
            new Data { Name = "FNMULS" , Opcode = 0xee200a40 },
            new Data { Name = "FNMACS" , Opcode = 0xee000a40 },
            new Data { Name = "FNMSCS" , Opcode = 0xee100a40 },

            new Data { Name = "FCMPS"  , Opcode = 0xeeb40a40 },
            new Data { Name = "FCMPZS" , Opcode = 0xeeb50a40 },
            new Data { Name = "FCMPES" , Opcode = 0xeeb40ac0 },
            new Data { Name = "FCMPEZS", Opcode = 0xeeb50ac0 },

            new Data { Name = "FCPYD"  , Opcode = 0xeeb00b40 },
            new Data { Name = "FCVTDS" , Opcode = 0xeeb70ac0 },
            new Data { Name = "FCVTSD" , Opcode = 0xeeb70bc0 },
            new Data { Name = "FMDHR"  , Opcode = 0xee200b10 },
            new Data { Name = "FMDLR"  , Opcode = 0xee000b10 },
            new Data { Name = "FMRDH"  , Opcode = 0xee300b10 },
            new Data { Name = "FMRDL"  , Opcode = 0xee100b10 },
            new Data { Name = "FSITOD" , Opcode = 0xeeb80bc0 },
            new Data { Name = "FUITOD" , Opcode = 0xeeb80b40 },
            new Data { Name = "FTOSID" , Opcode = 0xeebd0b40 },
            new Data { Name = "FTOSIZD", Opcode = 0xeebd0bc0 },
            new Data { Name = "FTOUID" , Opcode = 0xeebc0b40 },
            new Data { Name = "FTOUIZD", Opcode = 0xeebc0bc0 },

            new Data { Name = "FLDD"   , Opcode = 0xed100b00 },
            new Data { Name = "FSTD"   , Opcode = 0xed000b00 },
            new Data { Name = "FLDMIAD", Opcode = 0xec900b00 },
            new Data { Name = "FLDMDBD", Opcode = 0xed300b00 },
            new Data { Name = "FSTMIAD", Opcode = 0xec800b00 },
            new Data { Name = "FSTMDBD", Opcode = 0xed200b00 },

            new Data { Name = "FABSD"  , Opcode = 0xeeb00bc0 },
            new Data { Name = "FNEGD"  , Opcode = 0xeeb10b40 },
            new Data { Name = "FSQRTD" , Opcode = 0xeeb10bc0 },

            new Data { Name = "FADDD"  , Opcode = 0xee300b00 },
            new Data { Name = "FSUBD"  , Opcode = 0xee300b40 },
            new Data { Name = "FMULD"  , Opcode = 0xee200b00 },
            new Data { Name = "FDIVD"  , Opcode = 0xee800b00 },
            new Data { Name = "FMACD"  , Opcode = 0xee000b00 },
            new Data { Name = "FMSCD"  , Opcode = 0xee100b00 },
            new Data { Name = "FNMULD" , Opcode = 0xee200b40 },
            new Data { Name = "FNMACD" , Opcode = 0xee000b40 },
            new Data { Name = "FNMSCD" , Opcode = 0xee100b40 },

            new Data { Name = "FCMPD"  , Opcode = 0xeeb40b40 },
            new Data { Name = "FCMPZD" , Opcode = 0xeeb50b40 },
            new Data { Name = "FCMPED" , Opcode = 0xeeb40bc0 },
            new Data { Name = "FCMPEZD", Opcode = 0xeeb50bc0 },

            new Data { Name = "FMSRR"  , Opcode = 0xec400a10 },
            new Data { Name = "FMRRS"  , Opcode = 0xec500a10 },
            new Data { Name = "FMDRR"  , Opcode = 0xec400b10 },
            new Data { Name = "FMRRD"  , Opcode = 0xec500b10 },
        };

        public void TestCompliance()
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            InstructionSet.Opcode     op;

            foreach(Data d in c_Data)
            {
                op = this.Decode( d.Opcode );

                if(op == null)
                {
                    throw IncorrectEncodingException.Create( "Cannot decode {0,9} [0x{1:X8}]", d.Name, d.Opcode );
                }
                else
                {
                    uint target       = 0;
                    bool targetIsCode = false;

                    op.Print( this, str, 0, ref target, ref targetIsCode );

                    if(d.Name != str.ToString().Split( ' ' )[0])
                    {
                        throw IncorrectEncodingException.Create( "Incorrect decoding {0,9} [0x{1:X8}]: got {2}", d.Name, d.Opcode, str );
                    }

                    str.Length = 0;
                }
            }
        }
#endif

        //--//

        public abstract class Opcode_VFP_Flavor : Opcode
        {
            //
            // State
            //

            public bool IsDouble;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    bool IsDouble       )
            {
                base.Prepare( ConditionCodes );

                this.IsDouble = IsDouble;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.IsDouble = s_EncodingVFP.get_IsDouble( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_IsDouble( this.IsDouble );

                return op;
            }

            //--//

            public static string DumpRegister( uint reg      ,
                                               bool isDouble )
            {
                if(isDouble)
                {
                    switch(reg)
                    {
                        case 0 : return "d0";
                        case 2 : return "d1";
                        case 4 : return "d2";
                        case 6 : return "d3";
                        case 8 : return "d4";
                        case 10: return "d5";
                        case 12: return "d6";
                        case 14: return "d7";
                        case 16: return "d8";
                        case 18: return "d9";
                        case 20: return "d10";
                        case 22: return "d11";
                        case 24: return "d12";
                        case 26: return "d13";
                        case 28: return "d14";
                        case 30: return "d15";
                    }
                }
                else
                {
                    switch(reg)
                    {
                        case 0 : return "s0";
                        case 1 : return "s1";
                        case 2 : return "s2";
                        case 3 : return "s3";
                        case 4 : return "s4";
                        case 5 : return "s5";
                        case 6 : return "s6";
                        case 7 : return "s7";
                        case 8 : return "s8";
                        case 9 : return "s9";
                        case 10: return "s10";
                        case 11: return "s11";
                        case 12: return "s12";
                        case 13: return "s13";
                        case 14: return "s14";
                        case 15: return "s15";
                        case 16: return "s16";
                        case 17: return "s17";
                        case 18: return "s18";
                        case 19: return "s19";
                        case 20: return "s20";
                        case 21: return "s21";
                        case 22: return "s22";
                        case 23: return "s23";
                        case 24: return "s24";
                        case 25: return "s25";
                        case 26: return "s26";
                        case 27: return "s27";
                        case 28: return "s28";
                        case 29: return "s29";
                        case 30: return "s30";
                        case 31: return "s31";
                    }
                }

                return "??";
            }
        }

        public abstract class Opcode_VFP_BaseDataTransfer : Opcode_VFP_Flavor
        {
            //
            // State
            //

            public uint Rn;
            public uint Fd;
            public bool IsLoad;
            public bool Up;
            public uint Offset;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    bool IsDouble       ,
                                    uint Rn             ,
                                    uint Fd             ,
                                    bool IsLoad         ,
                                    bool Up             ,
                                    uint Offset         )
            {
                base.Prepare( ConditionCodes, IsDouble );

                this.Rn     = Rn;
                this.Fd     = Fd;
                this.IsLoad = IsLoad;
                this.Up     = Up;
                this.Offset = Offset;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rn     = s_EncodingVFP.get_Rn                 ( op );
                this.Fd     = s_EncodingVFP.get_Fd                 ( op );
                this.IsLoad = s_EncodingVFP.get_DataTransfer_IsLoad( op );
                this.Up     = s_EncodingVFP.get_DataTransfer_IsUp  ( op );
                this.Offset = s_EncodingVFP.get_DataTransfer_Offset( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Rn                 ( this.Rn     );
                op |= s_EncodingVFP.set_Fd                 ( this.Fd     );
                op |= s_EncodingVFP.set_DataTransfer_IsLoad( this.IsLoad );
                op |= s_EncodingVFP.set_DataTransfer_IsUp  ( this.Up     );
                op |= s_EncodingVFP.set_DataTransfer_Offset( this.Offset );

                return op;
            }
        }

        public sealed class Opcode_VFP_DataTransfer : Opcode_VFP_BaseDataTransfer
        {
            //
            // Constructor Methods
            //

            internal Opcode_VFP_DataTransfer()
            {
            }

            //--//

            public new void Prepare( uint ConditionCodes ,
                                     bool IsDouble       ,
                                     uint Rn             ,
                                     uint Fd             ,
                                     bool IsLoad         ,
                                     bool Up             ,
                                     uint Offset         )
            {
                base.Prepare( ConditionCodes, IsDouble, Rn, Fd, IsLoad, Up, Offset );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                return op | EncDef.op_DataTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}{2}", this.IsLoad ? "FLD" : "FST", this.IsDouble ? "D" : "S", DumpCondition() );

                str.AppendFormat( "{0},[{1}", DumpRegister( this.Fd, this.IsDouble ), DumpRegister( this.Rn ) );

                if(this.Offset != 0)
                {
                    str.AppendFormat( ",#{0}0x{1:X}", this.Up ? "" : "-", this.Offset * 4 );
                }

                str.Append( "]" );
            }
        }

        public sealed class Opcode_VFP_BlockDataTransfer : Opcode_VFP_BaseDataTransfer
        {
            //
            // State
            //

            public bool PreIndex;
            public bool WriteBack;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_BlockDataTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsDouble       ,
                                 uint Rn             ,
                                 uint Fd             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Count          )
            {
                base.Prepare( ConditionCodes, IsDouble, Rn, Fd, IsLoad, Up, Count );

                this.PreIndex  = PreIndex;
                this.WriteBack = WriteBack;
            }

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.PreIndex  = s_EncodingVFP.get_DataTransfer_IsPreIndexing  ( op );
                this.WriteBack = s_EncodingVFP.get_DataTransfer_ShouldWriteBack( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_DataTransfer_IsPreIndexing  ( this.PreIndex  );
                op |= s_EncodingVFP.set_DataTransfer_ShouldWriteBack( this.WriteBack );

                return op | EncDef.op_BlockDataTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                char c1;
                char c2;

                if(this.Rn == 13)
                {
                    if(this.IsLoad)
                    {
                        c1 = this.PreIndex ? 'E' : 'F';
                        c2 = this.Up       ? 'D' : 'A';
                    }
                    else
                    {
                        c1 = this.PreIndex ? 'F' : 'E';
                        c2 = this.Up       ? 'A' : 'D';
                    }
                }
                else
                {
                    c1 = this.Up       ? 'I' : 'D';
                    c2 = this.PreIndex ? 'B' : 'A';
                }

                PrintMnemonic( str, "{0}{1}{2}{3}{4}", this.IsLoad ? "FLDM" : "FSTM", c1, c2, this.IsDouble ? "D" : "S", DumpCondition() );

                str.AppendFormat( "{0}{1},", DumpRegister( this.Rn ), this.WriteBack ? "!" : "" );

                str.Append( "{" );

                for(uint pos = 0; pos < this.Offset; pos += this.IsDouble ? 2u : 1u)
                {
                    if(pos != 0)
                    {
                        str.Append( "," );
                    }

                    str.AppendFormat( "{0}", DumpRegister( this.Fd + pos, this.IsDouble ) );
                }

                str.Append( "}" );
            }
        }

        public sealed class Opcode_VFP_ConditionCodeTransfer : Opcode
        {
            //
            // Constructor Methods
            //

            internal Opcode_VFP_ConditionCodeTransfer()
            {
            }

            //--//

            public new void Prepare( uint ConditionCodes )
            {
                base.Prepare( ConditionCodes );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                return op | EncDef.op_ConditionCodeTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}", "FMSTAT", DumpCondition() );
            }
        }

        public abstract class Opcode_VFP_BaseRegisterTransfer : Opcode_VFP_Flavor
        {
            //
            // State
            //

            public uint Rd;
            public bool IsFromCoProc;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    bool IsDouble       ,
                                    uint Rd             ,
                                    bool IsFromCoProc   )
            {
                base.Prepare( ConditionCodes, IsDouble );

                this.Rd           = Rd;
                this.IsFromCoProc = IsFromCoProc;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rd           = s_EncodingVFP.get_Rd                           ( op );
                this.IsFromCoProc = s_EncodingVFP.get_RegisterTransfer_IsFromCoproc( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Rd                           ( this.Rd           );
                op |= s_EncodingVFP.set_RegisterTransfer_IsFromCoproc( this.IsFromCoProc );

                return op;
            }
        }

        public sealed class Opcode_VFP_SystemRegisterTransfer : Opcode_VFP_BaseRegisterTransfer
        {
            //
            // State
            //

            public uint SysReg;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_SystemRegisterTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rd             ,
                                 uint SysReg         ,
                                 bool IsFromCoProc   )
            {
                base.Prepare( ConditionCodes, false, Rd, IsFromCoProc );

                this.SysReg = SysReg;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.SysReg = s_EncodingVFP.get_SysReg( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_SysReg( this.SysReg );

                return op | EncDef.op_SystemRegisterTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}", this.IsFromCoProc ? "FMRX" : "FMXR", DumpCondition() );

                str.AppendFormat( this.IsFromCoProc ? "{0},{1}" : "{1},{0}", DumpRegister( this.Rd ), DumpSystemRegister( this.SysReg ) );
            }

            private static string DumpSystemRegister( uint reg )
            {
                switch(reg)
                {
                    case EncDef.c_systemRegister_FPSID: return "FPSID";
                    case EncDef.c_systemRegister_FPSCR: return "FPSCR";
                    case EncDef.c_systemRegister_FPEXC: return "FPEXC";
                }

                return "??";
            }
        }

        public sealed class Opcode_VFP_64bitRegisterTransfer : Opcode_VFP_BaseRegisterTransfer
        {
            //
            // State
            //

            public uint Rn;
            public uint Fm;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_64bitRegisterTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsDouble       ,
                                 uint Rd             ,
                                 uint Rn             ,
                                 uint Fm             ,
                                 bool IsFromCoProc   )
            {
                base.Prepare( ConditionCodes, IsDouble, Rd, IsFromCoProc );

                this.Rn = Rn;
                this.Fm = Fm;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rn = s_EncodingVFP.get_Rn( op );
                this.Fm = s_EncodingVFP.get_Fm( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Rn( this.Rn );
                op |= s_EncodingVFP.set_Fm( this.Fm );

                return op | EncDef.op_64bitRegisterTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                if(this.IsDouble)
                {
                    PrintMnemonic( str, "{0}{1}", this.IsFromCoProc ? "FMRRD" : "FMDRR", DumpCondition() );

                    str.AppendFormat( this.IsFromCoProc ? "{0},{1},{2}" : "{2},{0},{1}", DumpRegister( this.Rd ), DumpRegister( this.Rn ), DumpRegister( this.Fm, true ) );
                }
                else
                {
                    PrintMnemonic( str, "{0}{1}", this.IsFromCoProc ? "FMRRS" : "FMSRR", DumpCondition() );

                    str.AppendFormat( this.IsFromCoProc ? "{0},{1},{2},{3}" : "{2},{3},{0},{1}", DumpRegister( this.Rd ), DumpRegister( this.Rn ), DumpRegister( this.Fm, false ), DumpRegister( (this.Fm + 1) % 32, false ) );
                }
            }
        }

        public sealed class Opcode_VFP_32bitRegisterTransfer : Opcode_VFP_BaseRegisterTransfer
        {
            //
            // State
            //

            public uint Fn;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_32bitRegisterTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rd             ,
                                 uint Fn             ,
                                 bool IsFromCoProc   )
            {
                base.Prepare( ConditionCodes, false, Rd, IsFromCoProc );

                this.Fn = Fn;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fn = s_EncodingVFP.get_Fn( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fn( this.Fn );

                return op | EncDef.op_32bitRegisterTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}", this.IsFromCoProc ? "FMRS" : "FMSR", DumpCondition() );

                str.AppendFormat( this.IsFromCoProc ? "{0},{1}" : "{1},{0}", DumpRegister( this.Rd ), DumpRegister( this.Fn, false ) );
            }
        }

        public sealed class Opcode_VFP_32bitLoRegisterTransfer : Opcode_VFP_BaseRegisterTransfer
        {
            //
            // State
            //

            public uint Fn;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_32bitLoRegisterTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rd             ,
                                 uint Fn             ,
                                 bool IsFromCoProc   )
            {
                base.Prepare( ConditionCodes, true, Rd, IsFromCoProc );

                this.Fn = Fn;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fn = s_EncodingVFP.get_Fn( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fn( this.Fn );

                return op | EncDef.op_32bitLoRegisterTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}", this.IsFromCoProc ? "FMRDL" : "FMDLR", DumpCondition() );

                str.AppendFormat( this.IsFromCoProc ? "{0},{1}" : "{1},{0}", DumpRegister( this.Rd ), DumpRegister( this.Fn, true ) );
            }
        }

        public sealed class Opcode_VFP_32bitHiRegisterTransfer : Opcode_VFP_BaseRegisterTransfer
        {
            //
            // State
            //

            public uint Fn;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_32bitHiRegisterTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rd             ,
                                 uint Fn             ,
                                 bool IsFromCoProc   )
            {
                base.Prepare( ConditionCodes, true, Rd, IsFromCoProc );

                this.Fn = Fn;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fn = s_EncodingVFP.get_Fn( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fn( this.Fn );

                return op | EncDef.op_32bitHiRegisterTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}", this.IsFromCoProc ? "FMRDH" : "FMDHR", DumpCondition() );

                str.AppendFormat( this.IsFromCoProc ? "{0},{1}" : "{1},{0}", DumpRegister( this.Rd ), DumpRegister( this.Fn, true ) );
            }
        }

        public sealed class Opcode_VFP_CompareToZero : Opcode_VFP_Flavor
        {
            //
            // State
            //

            public uint Fd;
            public bool CheckNaN;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_CompareToZero()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsDouble       ,
                                 uint Fd             ,
                                 bool CheckNaN       )
            {
                base.Prepare( ConditionCodes, IsDouble );

                this.Fd       = Fd;
                this.CheckNaN = CheckNaN;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fd       = s_EncodingVFP.get_Fd      ( op );
                this.CheckNaN = s_EncodingVFP.get_CheckNaN( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fd      ( this.Fd       );
                op |= s_EncodingVFP.set_CheckNaN( this.CheckNaN );

                return op | EncDef.op_CompareToZero;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "FCMP{0}Z{1}{2}", this.CheckNaN ? "E" : "", this.IsDouble ? "D" : "S", DumpCondition() );

                str.AppendFormat( "{0}", DumpRegister( this.Fd, this.IsDouble ) );
            }
        }

        public sealed class Opcode_VFP_ConvertFloatToFloat : Opcode_VFP_Flavor
        {
            //
            // State
            //

            public uint Fd;
            public uint Fm;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_ConvertFloatToFloat()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsDouble       ,
                                 uint Fd             ,
                                 uint Fm             )
            {
                base.Prepare( ConditionCodes, IsDouble );

                this.Fd = Fd;
                this.Fm = Fm;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fd = s_EncodingVFP.get_Fd( op );
                this.Fm = s_EncodingVFP.get_Fm( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fd( this.Fd );
                op |= s_EncodingVFP.set_Fm( this.Fm );

                return op | EncDef.op_ConvertFloatToFloat;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "FCVT{0}{1}", this.IsDouble ? "SD" : "DS", DumpCondition() );

                str.AppendFormat( "{0},{1}", DumpRegister( this.Fd, !this.IsDouble ), DumpRegister( this.Fm, this.IsDouble ) );
            }
        }

        public sealed class Opcode_VFP_UnaryDataOperation : Opcode_VFP_Flavor
        {
            //
            // State
            //

            public uint Fd;
            public uint Fm;
            public uint Alu;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_UnaryDataOperation()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsDouble       ,
                                 uint Fd             ,
                                 uint Fm             ,
                                 uint Alu            )
            {
                base.Prepare( ConditionCodes, IsDouble );

                this.Fd  = Fd;
                this.Fm  = Fm;
                this.Alu = Alu;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fd  = s_EncodingVFP.get_Fd            ( op );
                this.Fm  = s_EncodingVFP.get_Fm            ( op );
                this.Alu = s_EncodingVFP.get_UnaryOperation( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fd            ( this.Fd  );
                op |= s_EncodingVFP.set_Fm            ( this.Fm  );
                op |= s_EncodingVFP.set_UnaryOperation( this.Alu );

                return op | EncDef.op_UnaryDataOperation;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}{2}", DumpOperation( this.Alu ), this.IsDouble ? "D" : "S", DumpCondition() );

                switch(this.Alu)
                {
                    case EncDef.c_unaryOperation_TOUI :
                    case EncDef.c_unaryOperation_TOUIZ:
                    case EncDef.c_unaryOperation_TOSI :
                    case EncDef.c_unaryOperation_TOSIZ:
                        str.AppendFormat( "{0},", DumpRegister( this.Fd, false ) );
                        break;

                    default:
                        str.AppendFormat( "{0},", DumpRegister( this.Fd, this.IsDouble ) );
                        break;

                }

                switch(this.Alu)
                {
                    case EncDef.c_unaryOperation_UITO :
                    case EncDef.c_unaryOperation_SITO :
                        str.AppendFormat( "{0}", DumpRegister( this.Fm, false ) );
                        break;

                    default:
                        str.AppendFormat( "{0}", DumpRegister( this.Fm, this.IsDouble ) );
                        break;

                }
            }

            private static string DumpOperation( uint alu )
            {
                switch(alu)
                {
                    case EncDef.c_unaryOperation_CPY  : return "FCPY";
                    case EncDef.c_unaryOperation_ABS  : return "FABS";
                    case EncDef.c_unaryOperation_NEG  : return "FNEG";
                    case EncDef.c_unaryOperation_SQRT : return "FSQRT";
                    case EncDef.c_unaryOperation_CMP  : return "FCMP";
                    case EncDef.c_unaryOperation_CMPE : return "FCMPE";
                    case EncDef.c_unaryOperation_UITO : return "FUITO";
                    case EncDef.c_unaryOperation_SITO : return "FSITO";
                    case EncDef.c_unaryOperation_TOUI : return "FTOUI";
                    case EncDef.c_unaryOperation_TOUIZ: return "FTOUIZ";
                    case EncDef.c_unaryOperation_TOSI : return "FTOSI";
                    case EncDef.c_unaryOperation_TOSIZ: return "FTOSIZ";
                }

                return "??";
            }
        }

        public sealed class Opcode_VFP_BinaryDataOperation : Opcode_VFP_Flavor
        {
            //
            // State
            //

            public uint Fd;
            public uint Fn;
            public uint Fm;
            public uint Alu;

            //
            // Constructor Methods
            //

            internal Opcode_VFP_BinaryDataOperation()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsDouble       ,
                                 uint Fd             ,
                                 uint Fn             ,
                                 uint Fm             ,
                                 uint Alu            )
            {
                base.Prepare( ConditionCodes, IsDouble );

                this.Fd  = Fd;
                this.Fn  = Fn;
                this.Fm  = Fm;
                this.Alu = Alu;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Fd  = s_EncodingVFP.get_Fd             ( op );
                this.Fn  = s_EncodingVFP.get_Fn             ( op );
                this.Fm  = s_EncodingVFP.get_Fm             ( op );
                this.Alu = s_EncodingVFP.get_BinaryOperation( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_EncodingVFP.set_Fd             ( this.Fd  );
                op |= s_EncodingVFP.set_Fn             ( this.Fn  );
                op |= s_EncodingVFP.set_Fm             ( this.Fm  );
                op |= s_EncodingVFP.set_BinaryOperation( this.Alu );

                return op | EncDef.op_BinaryDataOperation;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}{2}", DumpOperation( this.Alu ), this.IsDouble ? "D" : "S", DumpCondition() );

                str.AppendFormat( "{0},{1},{2}", DumpRegister( this.Fd, this.IsDouble ), DumpRegister( this.Fn, this.IsDouble ), DumpRegister( this.Fm, this.IsDouble ) );
            }

            private static string DumpOperation( uint alu )
            {
                switch(alu)
                {
                    case EncDef.c_binaryOperation_MAC : return "FMAC";
                    case EncDef.c_binaryOperation_NMAC: return "FNMAC";
                    case EncDef.c_binaryOperation_MSC : return "FMSC";
                    case EncDef.c_binaryOperation_NMSC: return "FNMSC";
                    case EncDef.c_binaryOperation_MUL : return "FMUL";
                    case EncDef.c_binaryOperation_NMUL: return "FNMUL";
                    case EncDef.c_binaryOperation_ADD : return "FADD";
                    case EncDef.c_binaryOperation_SUB : return "FSUB";
                    case EncDef.c_binaryOperation_DIV : return "FDIV";
                }

                return "??";
            }
        }
    }
}

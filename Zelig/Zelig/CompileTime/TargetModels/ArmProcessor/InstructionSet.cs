//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{

    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    
    public class InstructionSet
    {
        private static EncodingDefinition_ARM  s_Encoding    = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();

        public override bool Equals(object obj)
        {
            InstructionSet match = obj as InstructionSet;
            if(match != null)
            {
                return m_version == match.Version;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //
        // State
        //
        private readonly InstructionSetVersion m_version;
        //--//
        private readonly Opcode_MRS m_MRS;
        private readonly Opcode_MSR_1 m_MSR_1;
        private readonly Opcode_MSR_2 m_MSR_2;
        private readonly Opcode_DataProcessing_1 m_DataProcessing_1;
        private readonly Opcode_DataProcessing_2 m_DataProcessing_2;
        private readonly Opcode_DataProcessing_3 m_DataProcessing_3;
        private readonly Opcode_Multiply m_Multiply;
        private readonly Opcode_MultiplyLong m_MultiplyLong;
        private readonly Opcode_SingleDataSwap m_SingleDataSwap;
        private readonly Opcode_BranchAndExchange m_BranchAndExchange;
        private readonly Opcode_HalfwordDataTransfer_1 m_HalfwordDataTransfer_1;
        private readonly Opcode_HalfwordDataTransfer_2 m_HalfwordDataTransfer_2;
        private readonly Opcode_SingleDataTransfer_1 m_SingleDataTransfer_1;
        private readonly Opcode_SingleDataTransfer_2 m_SingleDataTransfer_2;
        private readonly Opcode_SingleDataTransfer_3 m_SingleDataTransfer_3;
        private readonly Opcode_BlockDataTransfer m_BlockDataTransfer;
        private readonly Opcode_Branch m_Branch;
        private readonly Opcode_CoprocDataTransfer m_CoprocDataTransfer;
        private readonly Opcode_CoprocDataOperation m_CoprocDataOperation;
        private readonly Opcode_CoprocRegisterTransfer m_CoprocRegisterTransfer;
        private readonly Opcode_SoftwareInterrupt m_SoftwareInterrupt;
        private readonly Opcode_Breakpoint m_Breakpoint;

        //
        // Constructor Methods
        //

        public InstructionSet(InstructionSetVersion version)
        {
            m_version                = version;
            //--//
            m_MRS                    = new Opcode_MRS                   ();
            m_MSR_1                  = new Opcode_MSR_1                 ();
            m_MSR_2                  = new Opcode_MSR_2                 ();
            m_DataProcessing_1       = new Opcode_DataProcessing_1      ();
            m_DataProcessing_2       = new Opcode_DataProcessing_2      ();
            m_DataProcessing_3       = new Opcode_DataProcessing_3      ();
            m_Multiply               = new Opcode_Multiply              ();
            m_MultiplyLong           = new Opcode_MultiplyLong          ();
            m_SingleDataSwap         = new Opcode_SingleDataSwap        ();
            m_BranchAndExchange      = new Opcode_BranchAndExchange     ();
            m_HalfwordDataTransfer_1 = new Opcode_HalfwordDataTransfer_1();
            m_HalfwordDataTransfer_2 = new Opcode_HalfwordDataTransfer_2();
            m_SingleDataTransfer_1   = new Opcode_SingleDataTransfer_1  ();
            m_SingleDataTransfer_2   = new Opcode_SingleDataTransfer_2  ();
            m_SingleDataTransfer_3   = new Opcode_SingleDataTransfer_3  ();
            m_BlockDataTransfer      = new Opcode_BlockDataTransfer     ();
            m_Branch                 = new Opcode_Branch                ();
            m_CoprocDataTransfer     = new Opcode_CoprocDataTransfer    ();
            m_CoprocDataOperation    = new Opcode_CoprocDataOperation   ();
            m_CoprocRegisterTransfer = new Opcode_CoprocRegisterTransfer();
            m_SoftwareInterrupt      = new Opcode_SoftwareInterrupt     ();
            m_Breakpoint             = new Opcode_Breakpoint            ();
        }

        //--//
        //--// Version
        //--//
        public InstructionSetVersion Version
        {
            get
            {
                return m_version;
            }
        }

        //--//

        public Opcode_MRS                    PrepareForMRS                    { get { return m_MRS                   ; } }
        public Opcode_MSR_1                  PrepareForMSR_1                  { get { return m_MSR_1                 ; } }
        public Opcode_MSR_2                  PrepareForMSR_2                  { get { return m_MSR_2                 ; } }
        public Opcode_DataProcessing_1       PrepareForDataProcessing_1       { get { return m_DataProcessing_1      ; } }
        public Opcode_DataProcessing_2       PrepareForDataProcessing_2       { get { return m_DataProcessing_2      ; } }
        public Opcode_DataProcessing_3       PrepareForDataProcessing_3       { get { return m_DataProcessing_3      ; } }
        public Opcode_Multiply               PrepareForMultiply               { get { return m_Multiply              ; } }
        public Opcode_MultiplyLong           PrepareForMultiplyLong           { get { return m_MultiplyLong          ; } }
        public Opcode_SingleDataSwap         PrepareForSingleDataSwap         { get { return m_SingleDataSwap        ; } }
        public Opcode_BranchAndExchange      PrepareForBranchAndExchange      { get { return m_BranchAndExchange     ; } }
        public Opcode_HalfwordDataTransfer_1 PrepareForHalfwordDataTransfer_1 { get { return m_HalfwordDataTransfer_1; } }
        public Opcode_HalfwordDataTransfer_2 PrepareForHalfwordDataTransfer_2 { get { return m_HalfwordDataTransfer_2; } }
        public Opcode_SingleDataTransfer_1   PrepareForSingleDataTransfer_1   { get { return m_SingleDataTransfer_1  ; } }
        public Opcode_SingleDataTransfer_2   PrepareForSingleDataTransfer_2   { get { return m_SingleDataTransfer_2  ; } }
        public Opcode_SingleDataTransfer_3   PrepareForSingleDataTransfer_3   { get { return m_SingleDataTransfer_3  ; } }
        public Opcode_BlockDataTransfer      PrepareForBlockDataTransfer      { get { return m_BlockDataTransfer     ; } }
        public Opcode_Branch                 PrepareForBranch                 { get { return m_Branch                ; } }
        public Opcode_CoprocDataTransfer     PrepareForCoprocDataTransfer     { get { return m_CoprocDataTransfer    ; } }
        public Opcode_CoprocDataOperation    PrepareForCoprocDataOperation    { get { return m_CoprocDataOperation   ; } }
        public Opcode_CoprocRegisterTransfer PrepareForCoprocRegisterTransfer { get { return m_CoprocRegisterTransfer; } }
        public Opcode_SoftwareInterrupt      PrepareForSoftwareInterrupt      { get { return m_SoftwareInterrupt     ; } }
        public Opcode_Breakpoint             PrepareForBreakpoint             { get { return m_Breakpoint            ; } }

        //--//

        public virtual Opcode Decode( uint op )
        {
            Opcode opcode = null;

            if     ((op & EncDef.opmask_Breakpoint            ) == EncDef.op_Breakpoint            ) opcode = m_Breakpoint            ;
            else if((op & EncDef.opmask_MRS                   ) == EncDef.op_MRS                   ) opcode = m_MRS                   ;
            else if((op & EncDef.opmask_MSR_1                 ) == EncDef.op_MSR_1                 ) opcode = m_MSR_1                 ;
            else if((op & EncDef.opmask_MSR_2                 ) == EncDef.op_MSR_2                 ) opcode = m_MSR_2                 ;
            else if((op & EncDef.opmask_DataProcessing_1      ) == EncDef.op_DataProcessing_1      ) opcode = m_DataProcessing_1      ;
            else if((op & EncDef.opmask_DataProcessing_2      ) == EncDef.op_DataProcessing_2      ) opcode = m_DataProcessing_2      ;
            else if((op & EncDef.opmask_DataProcessing_3      ) == EncDef.op_DataProcessing_3      ) opcode = m_DataProcessing_3      ;
            else if((op & EncDef.opmask_Multiply              ) == EncDef.op_Multiply              ) opcode = m_Multiply              ;
            else if((op & EncDef.opmask_MultiplyLong          ) == EncDef.op_MultiplyLong          ) opcode = m_MultiplyLong          ;
            else if((op & EncDef.opmask_SingleDataSwap        ) == EncDef.op_SingleDataSwap        ) opcode = m_SingleDataSwap        ;
            else if((op & EncDef.opmask_BranchAndExchange     ) == EncDef.op_BranchAndExchange     ) opcode = m_BranchAndExchange     ;
            else if((op & EncDef.opmask_HalfwordDataTransfer_1) == EncDef.op_HalfwordDataTransfer_1) opcode = m_HalfwordDataTransfer_1;
            else if((op & EncDef.opmask_HalfwordDataTransfer_2) == EncDef.op_HalfwordDataTransfer_2) opcode = m_HalfwordDataTransfer_2;
            else if((op & EncDef.opmask_SingleDataTransfer_1  ) == EncDef.op_SingleDataTransfer_1  ) opcode = m_SingleDataTransfer_1  ;
            else if((op & EncDef.opmask_SingleDataTransfer_2  ) == EncDef.op_SingleDataTransfer_2  ) opcode = m_SingleDataTransfer_2  ;
            else if((op & EncDef.opmask_SingleDataTransfer_3  ) == EncDef.op_SingleDataTransfer_3  ) opcode = m_SingleDataTransfer_3  ;
            else if((op & EncDef.opmask_Undefined             ) == EncDef.op_Undefined             ) opcode = null                    ;
            else if((op & EncDef.opmask_BlockDataTransfer     ) == EncDef.op_BlockDataTransfer     ) opcode = m_BlockDataTransfer     ;
            else if((op & EncDef.opmask_Branch                ) == EncDef.op_Branch                ) opcode = m_Branch                ;
            else if((op & EncDef.opmask_CoprocDataTransfer    ) == EncDef.op_CoprocDataTransfer    ) opcode = m_CoprocDataTransfer    ;
            else if((op & EncDef.opmask_CoprocDataOperation   ) == EncDef.op_CoprocDataOperation   ) opcode = m_CoprocDataOperation   ;
            else if((op & EncDef.opmask_CoprocRegisterTransfer) == EncDef.op_CoprocRegisterTransfer) opcode = m_CoprocRegisterTransfer;
            else if((op & EncDef.opmask_SoftwareInterrupt     ) == EncDef.op_SoftwareInterrupt     ) opcode = m_SoftwareInterrupt     ;

            if(opcode != null)
            {
                opcode.Decode( op );
            }

            return opcode;
        }

        public string DecodeAndPrint(     uint address      ,
                                          uint op           ,
                                      out uint target       ,
                                      out bool targetIsCode )
        {
            target       = 0;
            targetIsCode = false;

            Opcode dec = Decode( op );

            if(dec == null)
            {
                return "";
            }
            else
            {
                System.Text.StringBuilder str = new System.Text.StringBuilder();

                dec.Print( this, str, address, ref target, ref targetIsCode );

                return str.ToString();
            }
        }

        //--//

        public static string DumpMode( uint mode )
        {
            switch(mode & EncDef.c_psr_mode)
            {
                case EncDef.c_psr_mode_USER : return "USER";
                case EncDef.c_psr_mode_FIQ  : return "FIQ";
                case EncDef.c_psr_mode_IRQ  : return "IRQ";
                case EncDef.c_psr_mode_SVC  : return "SVC";
                case EncDef.c_psr_mode_ABORT: return "ABORT";
                case EncDef.c_psr_mode_UNDEF: return "UNDEF";
                case EncDef.c_psr_mode_SYS  : return "SYS";
            }

            return "??";
        }

        //--//

        public abstract class Opcode
        {
            //
            // State
            //

            public uint ConditionCodes;

            //--//

            protected void Prepare( uint ConditionCodes )
            {
                this.ConditionCodes = ConditionCodes;
            }

            //--//

            public virtual void Decode( uint op )
            {
                this.ConditionCodes = s_Encoding.get_ConditionCodes( op );
            }

            public virtual uint Encode()
            {
                return s_Encoding.set_ConditionCodes( this.ConditionCodes );
            }

            public abstract void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  );

            //--//

            protected void PrintMnemonic(        System.Text.StringBuilder str    ,
                                                 string                    format ,
                                          params object[]                  args   )
            {
                int start = str.Length;

                str.AppendFormat( format, args );

                int len = str.Length - start;

                if(len < 9)
                {
                    str.Append( new string( ' ', 9 - len ) );
                }
            }

            //--//

            protected string DumpCondition()
            {
                switch(this.ConditionCodes)
                {
                    case EncDef.c_cond_EQ: return "EQ";
                    case EncDef.c_cond_NE: return "NE";
                    case EncDef.c_cond_CS: return "CS";
                    case EncDef.c_cond_CC: return "CC";
                    case EncDef.c_cond_MI: return "MI";
                    case EncDef.c_cond_PL: return "PL";
                    case EncDef.c_cond_VS: return "VS";
                    case EncDef.c_cond_VC: return "VC";
                    case EncDef.c_cond_HI: return "HI";
                    case EncDef.c_cond_LS: return "LS";
                    case EncDef.c_cond_GE: return "GE";
                    case EncDef.c_cond_LT: return "LT";
                    case EncDef.c_cond_GT: return "GT";
                    case EncDef.c_cond_LE: return "LE";
                    case EncDef.c_cond_AL: return "";
                }

                return "??";
            }

            //--//

            public static string DumpRegister( uint reg )
            {
                switch(reg)
                {
                    case EncDef.c_register_r0 : return "r0";
                    case EncDef.c_register_r1 : return "r1";
                    case EncDef.c_register_r2 : return "r2";
                    case EncDef.c_register_r3 : return "r3";
                    case EncDef.c_register_r4 : return "r4";
                    case EncDef.c_register_r5 : return "r5";
                    case EncDef.c_register_r6 : return "r6";
                    case EncDef.c_register_r7 : return "r7";
                    case EncDef.c_register_r8 : return "r8";
                    case EncDef.c_register_r9 : return "r9";
                    case EncDef.c_register_r10: return "r10";
                    case EncDef.c_register_r11: return "r11";
                    case EncDef.c_register_r12: return "r12";
                    case EncDef.c_register_sp : return "sp";
                    case EncDef.c_register_lr : return "lr";
                    case EncDef.c_register_pc : return "pc";
                }

                return "??";
            }

            //--//

            static protected string DumpShiftType( uint stype )
            {
                switch(stype)
                {
                    case EncDef.c_shift_LSL: return "LSL";
                    case EncDef.c_shift_LSR: return "LSR";
                    case EncDef.c_shift_ASR: return "ASR";
                    case EncDef.c_shift_ROR: return "ROR";
                    case EncDef.c_shift_RRX: return "RRX";
                }

                return "???";
            }

            static protected string DumpHalfWordKind( uint kind )
            {
                switch(kind)
                {
                    case EncDef.c_halfwordkind_SWP: return "SWP";
                    case EncDef.c_halfwordkind_U2 : return "H";
                    case EncDef.c_halfwordkind_I1 : return "SB";
                    case EncDef.c_halfwordkind_I2 : return "SH";
                }

                return "??";
            }
        }

        public sealed class Opcode_MRS : Opcode
        {
            //
            // State
            //

            public bool UseSPSR;
            public uint Rd;

            //
            // Constructor Methods
            //

            internal Opcode_MRS()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool UseSPSR        ,
                                 uint Rd             )
            {
                base.Prepare( ConditionCodes );

                this.UseSPSR = UseSPSR;
                this.Rd      = Rd;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.UseSPSR = s_Encoding.get_StatusRegister_IsSPSR( op );
                this.Rd      = s_Encoding.get_Register2            ( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_StatusRegister_IsSPSR( this.UseSPSR );
                op |= s_Encoding.set_Register2            ( this.Rd      );

                return op | EncDef.op_MRS;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "MRS{0}", DumpCondition() );

                str.AppendFormat( "{0},{1}", DumpRegister( this.Rd ), this.UseSPSR ? "SPSR" : "CPSR" );
            }
        }

        public abstract class Opcode_MSR : Opcode
        {
            //
            // State
            //

            public bool UseSPSR;
            public uint Fields;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    bool UseSPSR        ,
                                    uint Fields         )
            {
                base.Prepare( ConditionCodes );

                this.UseSPSR = UseSPSR;
                this.Fields  = Fields;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.UseSPSR = s_Encoding.get_StatusRegister_IsSPSR( op );
                this.Fields  = s_Encoding.get_StatusRegister_Fields( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_StatusRegister_IsSPSR( this.UseSPSR );
                op |= s_Encoding.set_StatusRegister_Fields( this.Fields  );

                return op;
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "MSR{0}", DumpCondition() );

                str.AppendFormat( "{0}_{1}{2}{3}{4}",  this.UseSPSR                             ? "SPSR" : "CPSR",
                                                      (this.Fields & EncDef.c_psr_field_c) != 0 ? "c"    : ""    ,
                                                      (this.Fields & EncDef.c_psr_field_x) != 0 ? "x"    : ""    ,
                                                      (this.Fields & EncDef.c_psr_field_s) != 0 ? "s"    : ""    ,
                                                      (this.Fields & EncDef.c_psr_field_f) != 0 ? "f"    : ""    );
            }
        }

        public sealed class Opcode_MSR_1 : Opcode_MSR
        {
            //
            // State
            //

            public uint Rm;

            //
            // Constructor Methods
            //

            internal Opcode_MSR_1()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool UseSPSR        ,
                                 uint Fields         ,
                                 uint Rm             )
            {
                base.Prepare( ConditionCodes ,
                              UseSPSR        ,
                              Fields         );

                this.Rm = Rm;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rm = s_Encoding.get_Register4( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register4( this.Rm  );

                return op | EncDef.op_MSR_1;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                str.AppendFormat( ",{0}", DumpRegister( this.Rm ) );
            }
        }

        public sealed class Opcode_MSR_2 : Opcode_MSR
        {
            //
            // State
            //

            public uint ImmediateSeed;
            public uint ImmediateRotation;

            //
            // Constructor Methods
            //

            internal Opcode_MSR_2()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes    ,
                                 bool UseSPSR           ,
                                 uint Fields            ,
                                 uint ImmediateSeed     ,
                                 uint ImmediateRotation )
            {
                base.Prepare( ConditionCodes ,
                              UseSPSR        ,
                              Fields         );

                this.ImmediateSeed     = ImmediateSeed;
                this.ImmediateRotation = ImmediateRotation;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.ImmediateSeed     = s_Encoding.get_DataProcessing_ImmediateSeed    ( op );
                this.ImmediateRotation = s_Encoding.get_DataProcessing_ImmediateRotation( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_DataProcessing_ImmediateSeed    ( this.ImmediateSeed     );
                op |= s_Encoding.set_DataProcessing_ImmediateRotation( this.ImmediateRotation );

                return op | EncDef.op_MSR_2;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                str.AppendFormat( ",#0x{0:X}", this.Immediate );
            }

            public uint Immediate
            {
                get
                {
                    return s_Encoding.get_DataProcessing_ImmediateValue( this.ImmediateSeed, this.ImmediateRotation );
                }
            }
        }

        //--//

        public abstract class Opcode_DataProcessing : Opcode
        {
            //
            // State
            //

            public uint Rn;
            public uint Rd;
            public uint Alu;
            public bool SetCC;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    uint Rd             ,
                                    uint Alu            ,
                                    bool SetCC          )
            {
                base.Prepare( ConditionCodes );

                this.Rn    = Rn;
                this.Rd    = Rd;
                this.Alu   = Alu;
                this.SetCC = SetCC;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rn    = s_Encoding.get_Register1               ( op );
                this.Rd    = s_Encoding.get_Register2               ( op );
                this.Alu   = s_Encoding.get_DataProcessing_Operation( op );
                this.SetCC = s_Encoding.get_ShouldSetConditions     ( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register1               ( this.Rn    );
                op |= s_Encoding.set_Register2               ( this.Rd    );
                op |= s_Encoding.set_DataProcessing_Operation( this.Alu   );
                op |= s_Encoding.set_ShouldSetConditions     ( this.SetCC );

                return op;
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                bool fMov = false;
                bool fTst = false;

                switch(this.Alu)
                {
                    case EncDef.c_operation_MOV:
                    case EncDef.c_operation_MVN:
                        fMov = true;
                        break;

                    case EncDef.c_operation_TEQ:
                    case EncDef.c_operation_TST:
                    case EncDef.c_operation_CMP:
                    case EncDef.c_operation_CMN:
                        fTst = true;
                        break;
                }

                PrintMnemonic( str, "{0}{1}{2}", DumpOperation( this.Alu ), DumpCondition(), !fTst && this.SetCC ? 'S' : ' ' );

                if(fMov)
                {
                    str.AppendFormat( "{0}", DumpRegister( this.Rd ) );
                }
                else if(fTst)
                {
                    str.AppendFormat( "{0}", DumpRegister( this.Rn ) );
                }
                else
                {
                    str.AppendFormat( "{0},{1}", DumpRegister( this.Rd ), DumpRegister( this.Rn ) );
                }
            }

            private static string DumpOperation( uint alu )
            {
                switch(alu)
                {
                    case EncDef.c_operation_AND: return "AND";
                    case EncDef.c_operation_EOR: return "EOR";
                    case EncDef.c_operation_SUB: return "SUB";
                    case EncDef.c_operation_RSB: return "RSB";
                    case EncDef.c_operation_ADD: return "ADD";
                    case EncDef.c_operation_ADC: return "ADC";
                    case EncDef.c_operation_SBC: return "SBC";
                    case EncDef.c_operation_RSC: return "RSC";
                    case EncDef.c_operation_TST: return "TST";
                    case EncDef.c_operation_TEQ: return "TEQ";
                    case EncDef.c_operation_CMP: return "CMP";
                    case EncDef.c_operation_CMN: return "CMN";
                    case EncDef.c_operation_ORR: return "ORR";
                    case EncDef.c_operation_MOV: return "MOV";
                    case EncDef.c_operation_BIC: return "BIC";
                    case EncDef.c_operation_MVN: return "MVN";
                }

                return "??";
            }
        }

        public sealed class Opcode_DataProcessing_1 : Opcode_DataProcessing
        {
            //
            // State
            //

            public uint ImmediateSeed;
            public uint ImmediateRotation;

            //
            // Constructor Methods
            //

            internal Opcode_DataProcessing_1()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes    ,
                                 uint Rn                ,
                                 uint Rd                ,
                                 uint Alu               ,
                                 bool SetCC             ,
                                 uint ImmediateSeed     ,
                                 uint ImmediateRotation )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              Rd             ,
                              Alu            ,
                              SetCC          );

                this.ImmediateSeed     = ImmediateSeed;
                this.ImmediateRotation = ImmediateRotation;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.ImmediateSeed     = s_Encoding.get_DataProcessing_ImmediateSeed    ( op );
                this.ImmediateRotation = s_Encoding.get_DataProcessing_ImmediateRotation( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_DataProcessing_ImmediateSeed    ( this.ImmediateSeed     );
                op |= s_Encoding.set_DataProcessing_ImmediateRotation( this.ImmediateRotation );

                return op | EncDef.op_DataProcessing_1;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                uint offset = this.Immediate;

                PrintPre( str );

                str.AppendFormat( ",#0x{0:X}", offset );

                if(Rn == 15)
                {
                    switch(Alu)
                    {
                        case EncDef.c_operation_ADD:
                            target = (uint)(opcodeAddress + 8 + offset);
                            str.AppendFormat( " ; 0x{0:X8}", target );
                            break;

                        case EncDef.c_operation_SUB:
                            target = (uint)(opcodeAddress + 8 - offset);
                            str.AppendFormat( " ; 0x{0:X8}", target );
                            break;
                    }
                }
            }

            public uint Immediate
            {
                get
                {
                    return s_Encoding.get_DataProcessing_ImmediateValue( this.ImmediateSeed, this.ImmediateRotation );
                }
            }
        }

        public abstract class Opcode_DataProcessing_23 : Opcode_DataProcessing
        {
            //
            // State
            //

            public uint Rm;
            public uint ShiftType;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    uint Rd             ,
                                    uint Alu            ,
                                    bool SetCC          ,
                                    uint Rm             ,
                                    uint ShiftType      )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              Rd             ,
                              Alu            ,
                              SetCC          );

                this.Rm        = Rm;
                this.ShiftType = ShiftType;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rm        = s_Encoding.get_Register4 ( op );
                this.ShiftType = s_Encoding.get_Shift_Type( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register4 ( this.Rm        );
                op |= s_Encoding.set_Shift_Type( this.ShiftType );

                return op;
            }

            protected new void PrintPre( System.Text.StringBuilder str )
            {
                base.PrintPre( str );

                str.AppendFormat( ",{0}", DumpRegister( this.Rm ) );
            }
        }

        public sealed class Opcode_DataProcessing_2 : Opcode_DataProcessing_23
        {
            //
            // State
            //

            public uint ShiftValue;

            //
            // Constructor Methods
            //

            internal Opcode_DataProcessing_2()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 uint Rd             ,
                                 uint Alu            ,
                                 bool SetCC          ,
                                 uint Rm             ,
                                 uint ShiftType      ,
                                 uint ShiftValue     )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              Rd             ,
                              Alu            ,
                              SetCC          ,
                              Rm             ,
                              ShiftType      );

                this.ShiftValue = ShiftValue;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.ShiftValue = s_Encoding.get_Shift_Immediate( op );

                if(this.ShiftValue == 0)
                {
                    switch(ShiftType)
                    {
                        case EncDef.c_shift_LSR:
                        case EncDef.c_shift_ASR:
                            this.ShiftValue = 32;
                            break;

                        case EncDef.c_shift_ROR:
                            this.ShiftValue = 1;
                            this.ShiftType  = EncDef.c_shift_RRX;
                            break;
                    }
                }
            }

            public override uint Encode()
            {
                //
                // Save values.
                //
                uint shiftValue = this.ShiftValue;
                uint shiftType  = this.ShiftType;

                switch(this.ShiftType)
                {
                    case EncDef.c_shift_LSR:
                    case EncDef.c_shift_ASR:
                        if(this.ShiftValue == 32)
                        {
                            this.ShiftValue = 0;
                        }
                        break;

                    case EncDef.c_shift_RRX:
                        this.ShiftType  = EncDef.c_shift_ROR;
                        this.ShiftValue = 0;
                        break;
                }

                uint op = base.Encode();

                op |= s_Encoding.set_Shift_Immediate( this.ShiftValue );

                //
                // Restore values.
                //
                this.ShiftValue = shiftValue;
                this.ShiftType  = shiftType;


                return op | EncDef.op_DataProcessing_2;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                if(this.ShiftValue != 0)
                {
                    str.AppendFormat( ",{0} #{1}", DumpShiftType( this.ShiftType ), this.ShiftValue );
                }
            }
        }

        public sealed class Opcode_DataProcessing_3 : Opcode_DataProcessing_23
        {
            //
            // State
            //

            public uint Rs;

            //
            // Constructor Methods
            //

            internal Opcode_DataProcessing_3()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 uint Rd             ,
                                 uint Alu            ,
                                 bool SetCC          ,
                                 uint Rm             ,
                                 uint ShiftType      ,
                                 uint Rs             )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              Rd             ,
                              Alu            ,
                              SetCC          ,
                              Rm             ,
                              ShiftType      );

                this.Rs = Rs;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rs = s_Encoding.get_Register3( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register3( this.Rs );

                return op | EncDef.op_DataProcessing_3;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                str.AppendFormat( ",{0} {1}", DumpShiftType( this.ShiftType ), DumpRegister( this.Rs ) );
            }
        }

        //--//

        public sealed class Opcode_Multiply : Opcode
        {
            //
            // State
            //

            public uint Rd;
            public uint Rn;
            public uint Rs;
            public uint Rm;
            public bool SetCC;
            public bool IsAccumulate;

            //
            // Constructor Methods
            //

            internal Opcode_Multiply()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rd             ,
                                 uint Rn             ,
                                 uint Rs             ,
                                 uint Rm             ,
                                 bool SetCC          ,
                                 bool IsAccumulate   )
            {
                base.Prepare( ConditionCodes );

                this.Rd           = Rd;
                this.Rn           = Rn;
                this.Rs           = Rs;
                this.Rm           = Rm;
                this.SetCC        = SetCC;
                this.IsAccumulate = IsAccumulate;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rd           = s_Encoding.get_Register1            ( op );
                this.Rn           = s_Encoding.get_Register2            ( op );
                this.Rs           = s_Encoding.get_Register3            ( op );
                this.Rm           = s_Encoding.get_Register4            ( op );
                this.SetCC        = s_Encoding.get_ShouldSetConditions  ( op );
                this.IsAccumulate = s_Encoding.get_Multiply_IsAccumulate( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register1            ( this.Rd           );
                op |= s_Encoding.set_Register2            ( this.Rn           );
                op |= s_Encoding.set_Register3            ( this.Rs           );
                op |= s_Encoding.set_Register4            ( this.Rm           );
                op |= s_Encoding.set_ShouldSetConditions  ( this.SetCC        );
                op |= s_Encoding.set_Multiply_IsAccumulate( this.IsAccumulate );

                return op | EncDef.op_Multiply;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}{2}", this.IsAccumulate ? "MLA" : "MUL", DumpCondition(), this.SetCC ? 'S' : ' ' );

                str.AppendFormat( "{0},{1},{2}", DumpRegister( this.Rd ), DumpRegister( this.Rm ), DumpRegister( this.Rs ) );

                if(this.IsAccumulate)
                {
                    str.AppendFormat( ",{0}", DumpRegister( this.Rn ) );
                }
            }
        }

        public sealed class Opcode_MultiplyLong : Opcode
        {
            //
            // State
            //

            public uint RdHi;
            public uint RdLo;
            public uint Rs;
            public uint Rm;
            public bool SetCC;
            public bool IsAccumulate;
            public bool IsSigned;

            //
            // Constructor Methods
            //

            internal Opcode_MultiplyLong()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint RdHi           ,
                                 uint RdLo           ,
                                 uint Rs             ,
                                 uint Rm             ,
                                 bool SetCC          ,
                                 bool IsAccumulate   ,
                                 bool IsSigned       )
            {
                base.Prepare( ConditionCodes );

                this.RdHi         = RdHi;
                this.RdLo         = RdLo;
                this.Rs           = Rs;
                this.Rm           = Rm;
                this.SetCC        = SetCC;
                this.IsAccumulate = IsAccumulate;
                this.IsSigned     = IsSigned;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.RdHi         = s_Encoding.get_Register1            ( op );
                this.RdLo         = s_Encoding.get_Register2            ( op );
                this.Rs           = s_Encoding.get_Register3            ( op );
                this.Rm           = s_Encoding.get_Register4            ( op );
                this.SetCC        = s_Encoding.get_ShouldSetConditions  ( op );
                this.IsAccumulate = s_Encoding.get_Multiply_IsAccumulate( op );
                this.IsSigned     = s_Encoding.get_Multiply_IsSigned    ( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register1            ( this.RdHi         );
                op |= s_Encoding.set_Register2            ( this.RdLo         );
                op |= s_Encoding.set_Register3            ( this.Rs           );
                op |= s_Encoding.set_Register4            ( this.Rm           );
                op |= s_Encoding.set_ShouldSetConditions  ( this.SetCC        );
                op |= s_Encoding.set_Multiply_IsAccumulate( this.IsAccumulate );
                op |= s_Encoding.set_Multiply_IsSigned    ( this.IsSigned     );

                return op | EncDef.op_MultiplyLong;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}{2}{3}", this.IsSigned ? 'S' : 'U', this.IsAccumulate ? "MLAL" : "MULL", DumpCondition(), this.SetCC ? 'S' : ' ' );

                str.AppendFormat( "{0},{1},{2},{3}", DumpRegister( this.RdLo ), DumpRegister( this.RdHi ), DumpRegister( this.Rm ), DumpRegister( this.Rs ) );
            }
        }

        //--//

        public sealed class Opcode_SingleDataSwap : Opcode
        {
            //
            // State
            //

            public uint Rn;
            public uint Rd;
            public uint Rm;
            public bool IsByte;

            //
            // Constructor Methods
            //

            internal Opcode_SingleDataSwap()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 uint Rd             ,
                                 uint Rm             ,
                                 bool IsByte         )
            {
                base.Prepare( ConditionCodes );

                this.Rn     = Rn;
                this.Rd     = Rd;
                this.Rm     = Rm;
                this.IsByte = IsByte;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rn     = s_Encoding.get_Register1                  ( op );
                this.Rd     = s_Encoding.get_Register2                  ( op );
                this.Rm     = s_Encoding.get_Register4                  ( op );
                this.IsByte = s_Encoding.get_DataTransfer_IsByteTransfer( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register1                  ( this.Rn     );
                op |= s_Encoding.set_Register2                  ( this.Rd     );
                op |= s_Encoding.set_Register4                  ( this.Rm     );
                op |= s_Encoding.set_DataTransfer_IsByteTransfer( this.IsByte );

                return op | EncDef.op_SingleDataSwap;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "{0}{1}{2}", "SWP", DumpCondition(), this.IsByte ? "B" : "" );

                str.AppendFormat( "{0},{1},[{2}]", DumpRegister( this.Rd ), DumpRegister( this.Rm ), DumpRegister( this.Rn ) );
            }
        }

        //--//

        public sealed class Opcode_Branch : Opcode
        {
            //
            // State
            //

            public int  Offset;
            public bool IsLink;

            //
            // Constructor Methods
            //

            internal Opcode_Branch()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 int  Offset         ,
                                 bool IsLink         )
            {
                base.Prepare( ConditionCodes );

                this.Offset = Offset;
                this.IsLink = IsLink;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Offset = s_Encoding.get_Branch_Offset( op );
                this.IsLink = s_Encoding.get_Branch_IsLink( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Branch_Offset( this.Offset );
                op |= s_Encoding.set_Branch_IsLink( this.IsLink );

                return op | EncDef.op_Branch;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "B{0}{1}", this.IsLink ? "L" : "", DumpCondition() );

                target       = (uint)(opcodeAddress + 8 + this.Offset);
                targetIsCode = true;

                str.AppendFormat( "0x{0:X8}", target );
            }
        }

        public sealed class Opcode_BranchAndExchange : Opcode
        {
            //
            // State
            //

            public uint Rn;

            //
            // Constructor Methods
            //

            internal Opcode_BranchAndExchange()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             )
            {
                base.Prepare( ConditionCodes );

                this.Rn = Rn;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rn = s_Encoding.get_Register4( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register4( this.Rn );

                return op | EncDef.op_BranchAndExchange;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "BX{0}", DumpCondition() );

                str.AppendFormat( "{0}", DumpRegister( this.Rn ) );
            }
        }

        //--//

        public abstract class Opcode_DataTransfer : Opcode
        {
            //
            // State
            //

            public uint Rn;
            public bool IsLoad;
            public bool PreIndex;
            public bool Up;
            public bool WriteBack;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    bool IsLoad         ,
                                    bool PreIndex       ,
                                    bool Up             ,
                                    bool WriteBack      )
            {
                base.Prepare( ConditionCodes );

                this.Rn        = Rn;
                this.IsLoad    = IsLoad;
                this.PreIndex  = PreIndex;
                this.Up        = Up;
                this.WriteBack = WriteBack;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rn        = s_Encoding.get_Register1                   ( op );
                this.IsLoad    = s_Encoding.get_DataTransfer_IsLoad         ( op );
                this.PreIndex  = s_Encoding.get_DataTransfer_IsPreIndexing  ( op );
                this.Up        = s_Encoding.get_DataTransfer_IsUp           ( op );
                this.WriteBack = s_Encoding.get_DataTransfer_ShouldWriteBack( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register1                   ( this.Rn        );
                op |= s_Encoding.set_DataTransfer_IsLoad         ( this.IsLoad    );
                op |= s_Encoding.set_DataTransfer_IsPreIndexing  ( this.PreIndex  );
                op |= s_Encoding.set_DataTransfer_IsUp           ( this.Up        );
                op |= s_Encoding.set_DataTransfer_ShouldWriteBack( this.WriteBack );

                return op;
            }
        }

        public abstract class Opcode_WordDataTransfer : Opcode_DataTransfer
        {
            //
            // State
            //

            public uint Rd;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    bool IsLoad         ,
                                    bool PreIndex       ,
                                    bool Up             ,
                                    bool WriteBack      ,
                                    uint Rd             )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      );

                this.Rd = Rd;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rd = s_Encoding.get_Register2( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register2( this.Rd );

                return op;
            }
        }

        public abstract class Opcode_HalfwordDataTransfer : Opcode_WordDataTransfer
        {
            //
            // State
            //

            public uint Kind;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    bool IsLoad         ,
                                    bool PreIndex       ,
                                    bool Up             ,
                                    bool WriteBack      ,
                                    uint Rd             ,
                                    uint Kind           )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             );

                this.Kind = Kind;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Kind = s_Encoding.get_HalfWordDataTransfer_Kind( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_HalfWordDataTransfer_Kind( this.Kind );

                return op;
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                PrintMnemonic( str, "{0}{1}{2}", this.IsLoad ? "LDR" : "STR", DumpCondition(), DumpHalfWordKind( this.Kind ) );

                str.AppendFormat( "{0},[{1}", DumpRegister( this.Rd ), DumpRegister( this.Rn ) );

                if(PreIndex == false) str.Append( "]" );
            }

            protected void PrintPost( System.Text.StringBuilder str )
            {
                if(this.PreIndex) str.Append( "]" );

                if(this.WriteBack)
                {
                    str.Append( "!" );
                }
            }
        }

        public sealed class Opcode_HalfwordDataTransfer_1 : Opcode_HalfwordDataTransfer
        {
            //
            // State
            //

            public uint Rm;

            //
            // Constructor Methods
            //

            internal Opcode_HalfwordDataTransfer_1()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Rd             ,
                                 uint Kind           ,
                                 uint Rm             )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             ,
                              Kind           );

                this.Rm = Rm;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rm = s_Encoding.get_Register4( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register4( this.Rm );

                return op | EncDef.op_HalfwordDataTransfer_1;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                str.AppendFormat( ",{0}{1}", this.Up ? "" : "-", DumpRegister( this.Rm ) );

                PrintPost( str );
            }
        }

        public sealed class Opcode_HalfwordDataTransfer_2 : Opcode_HalfwordDataTransfer
        {
            //
            // State
            //

            public uint Offset;

            //
            // Constructor Methods
            //

            internal Opcode_HalfwordDataTransfer_2()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Rd             ,
                                 uint Kind           ,
                                 uint Offset         )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             ,
                              Kind           );

                this.Offset = Offset;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Offset = s_Encoding.get_HalfWordDataTransfer_Offset( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_HalfWordDataTransfer_Offset( this.Offset );

                return op | EncDef.op_HalfwordDataTransfer_2;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                str.AppendFormat( ",#{0}{1}", this.Up ? "" : "-", this.Offset );

                PrintPost( str );

                if(this.Rn == 15)
                {
                    int address = (int)(opcodeAddress + 8);

                    if(this.PreIndex)
                    {
                        address += (this.Up ? +(int)this.Offset : -(int)this.Offset);
                    }

                    target = (uint)address;
                    str.AppendFormat( " ; 0x{0:X8}", target );
                }
            }
        }

        public abstract class Opcode_SingleDataTransfer : Opcode_WordDataTransfer
        {
            //
            // State
            //

            public bool IsByte;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    bool IsLoad         ,
                                    bool PreIndex       ,
                                    bool Up             ,
                                    bool WriteBack      ,
                                    uint Rd             ,
                                    bool IsByte         )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             );

                this.IsByte = IsByte;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.IsByte = s_Encoding.get_DataTransfer_IsByteTransfer( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_DataTransfer_IsByteTransfer( this.IsByte );

                return op;
            }

            protected void PrintPre( System.Text.StringBuilder str )
            {
                if(this.IsPLD)
                {
                    PrintMnemonic( str, "PLD" );

                    str.AppendFormat( "{0}", DumpRegister( this.Rn ) );
                }
                else
                {
                    PrintMnemonic( str, "{0}{1}{2}", this.IsLoad ? "LDR" : "STR", DumpCondition(), this.IsByte ? "B" : "" );

                    str.AppendFormat( "{0},[{1}", DumpRegister( this.Rd ), DumpRegister( this.Rn ) );

                    if(this.PreIndex == false) str.Append( "]" );
                }
            }

            protected void PrintPost( System.Text.StringBuilder str )
            {
                if(this.IsPLD)
                {
                }
                else
                {
                    if(this.PreIndex) str.Append( "]" );

                    if(this.WriteBack)
                    {
                        str.Append( "!" );
                    }
                }
            }

            //--//

            public bool IsPLD
            {
                get
                {
                    if( this.ConditionCodes == EncDef.c_cond_UNUSED &&
                        this.Rd             == 0xF                  &&
                        this.IsLoad                                 &&
                        this.PreIndex                               &&
                       !this.WriteBack                               )
                    {

                        return true;
                    }

                    return false;
                }
            }
        }

        public sealed class Opcode_SingleDataTransfer_1 : Opcode_SingleDataTransfer
        {
            //
            // State
            //

            public uint Offset;

            //
            // Constructor Methods
            //

            internal Opcode_SingleDataTransfer_1()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Rd             ,
                                 bool IsByte         ,
                                 uint Offset         )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             ,
                              IsByte         );

                this.Offset = Offset;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Offset = s_Encoding.get_DataTransfer_Offset( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_DataTransfer_Offset( this.Offset );

                return op | EncDef.op_SingleDataTransfer_1;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                if(this.Offset != 0)
                {
                    str.AppendFormat( ",#{0}0x{1:X}", this.Up ? "" : "-", this.Offset );
                }

                PrintPost( str );

                if(this.Rn == 15)
                {
                    int address = (int)(opcodeAddress + 8);

                    if(this.PreIndex)
                    {
                        address += (this.Up ? +(int)this.Offset : -(int)this.Offset);
                    }

                    target = (uint)address;
                    str.AppendFormat( " ; 0x{0:X8}", target );
                }
            }
        }

        public abstract class Opcode_SingleDataTransfer_23 : Opcode_SingleDataTransfer
        {
            //
            // State
            //

            public uint Rm;
            public uint ShiftType;

            //--//

            protected void Prepare( uint ConditionCodes ,
                                    uint Rn             ,
                                    bool IsLoad         ,
                                    bool PreIndex       ,
                                    bool Up             ,
                                    bool WriteBack      ,
                                    uint Rd             ,
                                    bool IsByte         ,
                                    uint Rm             ,
                                    uint ShiftType      )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             ,
                              IsByte         );

                this.Rm        = Rm;
                this.ShiftType = ShiftType;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rm        = s_Encoding.get_Register4 ( op );
                this.ShiftType = s_Encoding.get_Shift_Type( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register4 ( this.Rm        );
                op |= s_Encoding.set_Shift_Type( this.ShiftType );

                return op;
            }

            protected new void PrintPre( System.Text.StringBuilder str )
            {
                base.PrintPre( str );

                str.AppendFormat( ",{0}{1}", this.Up ? "" : "-", DumpRegister( this.Rm ) );
            }

            protected new void PrintPost( System.Text.StringBuilder str )
            {
                base.PrintPost( str );
            }
        }

        public sealed class Opcode_SingleDataTransfer_2 : Opcode_SingleDataTransfer_23
        {
            //
            // State
            //

            public uint ShiftValue;

            //
            // Constructor Methods
            //

            internal Opcode_SingleDataTransfer_2()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Rd             ,
                                 bool IsByte         ,
                                 uint Rm             ,
                                 uint ShiftType      ,
                                 uint ShiftValue     )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             ,
                              IsByte         ,
                              Rm             ,
                              ShiftType      );

                this.ShiftValue = ShiftValue;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.ShiftValue = s_Encoding.get_Shift_Immediate( op );

                if(this.ShiftValue == 0)
                {
                    switch(ShiftType)
                    {
                        case EncDef.c_shift_LSR:
                        case EncDef.c_shift_ASR:
                            this.ShiftValue = 32;
                            break;

                        case EncDef.c_shift_ROR:
                            this.ShiftValue = 1;
                            this.ShiftType  = EncDef.c_shift_RRX;
                            break;
                    }
                }
            }

            public override uint Encode()
            {
                //
                // Save values.
                //
                uint shiftValue = this.ShiftValue;
                uint shiftType  = this.ShiftType;

                switch(this.ShiftType)
                {
                    case EncDef.c_shift_LSR:
                    case EncDef.c_shift_ASR:
                        if(this.ShiftValue == 32)
                        {
                            this.ShiftValue = 0;
                        }
                        break;

                    case EncDef.c_shift_RRX:
                        this.ShiftType  = EncDef.c_shift_ROR;
                        this.ShiftValue = 0;
                        break;
                }

                uint op = base.Encode();

                op |= s_Encoding.set_Shift_Immediate( this.ShiftValue );

                //
                // Restore values.
                //
                this.ShiftValue = shiftValue;
                this.ShiftType  = shiftType;


                return op | EncDef.op_SingleDataTransfer_2;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                if(this.ShiftValue != 0)
                {
                    str.AppendFormat( ",{0} #{1}", DumpShiftType( this.ShiftType ), this.ShiftValue );
                }

                PrintPost( str );
            }
        }

        public sealed class Opcode_SingleDataTransfer_3 : Opcode_SingleDataTransfer_23
        {
            //
            // State
            //

            public uint Rs;

            //
            // Constructor Methods
            //

            internal Opcode_SingleDataTransfer_3()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Rd             ,
                                 bool IsByte         ,
                                 uint Rm             ,
                                 uint ShiftType      ,
                                 uint Rs             )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      ,
                              Rd             ,
                              IsByte         ,
                              Rm             ,
                              ShiftType      );

                this.Rs = Rs;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Rs = s_Encoding.get_Register3( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Register3( this.Rs );

                return op | EncDef.op_SingleDataTransfer_3;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintPre( str );

                str.AppendFormat( ",{0} {1}", DumpShiftType( this.ShiftType ), DumpRegister( this.Rs ) );

                PrintPost( str );
            }
        }

        public sealed class Opcode_BlockDataTransfer : Opcode_DataTransfer
        {
            //
            // State
            //

            public uint Lst;
            public bool LoadPSR;

            //
            // Constructor Methods
            //

            internal Opcode_BlockDataTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Rn             ,
                                 bool IsLoad         ,
                                 bool PreIndex       ,
                                 bool Up             ,
                                 bool WriteBack      ,
                                 uint Lst            ,
                                 bool LoadPSR        )
            {
                base.Prepare( ConditionCodes ,
                              Rn             ,
                              IsLoad         ,
                              PreIndex       ,
                              Up             ,
                              WriteBack      );

                this.Lst     = Lst;
                this.LoadPSR = LoadPSR;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Lst     = s_Encoding.get_BlockDataTransfer_RegisterList( op );
                this.LoadPSR = s_Encoding.get_BlockDataTransfer_LoadPSR     ( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_BlockDataTransfer_RegisterList( this.Lst     );
                op |= s_Encoding.set_BlockDataTransfer_LoadPSR     ( this.LoadPSR );

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

                PrintMnemonic( str, "{0}{1}{2}{3}", this.IsLoad ? "LDM" : "STM", c1, c2, DumpCondition() );

                str.AppendFormat( "{0}{1},", DumpRegister( this.Rn ), this.WriteBack ? "!" : "" );

                uint Rd  = 0;
                uint lst = this.Lst;

                str.Append( "{" );

                while(lst != 0)
                {
                    while((lst & 1) == 0)
                    {
                        lst >>= 1;
                        Rd++;
                    }

                    uint Rfirst = Rd;

                    while((lst & 1) != 0)
                    {
                        lst >>= 1;
                        Rd++;
                    }

                    str.AppendFormat( "{0}", DumpRegister( Rfirst ) );

                    if(Rd - Rfirst > 2)
                    {
                        str.AppendFormat( "-{0}", DumpRegister( Rd-1 ) );
                    }
                    else if(Rd - Rfirst > 1)
                    {
                        str.AppendFormat( ",{0}", DumpRegister( Rd-1 ) );
                    }

                    if(lst != 0)
                    {
                        str.Append( "," );
                    }
                }

                str.Append( "}" );

                if(this.LoadPSR)
                {
                    str.Append( "^" );
                }
            }
        }

        public abstract class Opcode_Coproc : Opcode
        {
            //
            // State
            //

            public uint CpNum;

            //
            // Constructor Methods
            //

            protected Opcode_Coproc()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint CpNum          )
            {
                base.Prepare( ConditionCodes );

                this.CpNum = CpNum;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.CpNum = s_Encoding.get_Coproc_CpNum( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_Coproc_CpNum( this.CpNum     );

                return op;
            }
        }

        public sealed class Opcode_CoprocDataTransfer : Opcode_Coproc
        {
            //
            // State
            //

            public bool IsLoad;
            public bool WriteBack;
            public bool Wide;
            public bool Up;
            public bool PreIndex;
            public uint Rn;
            public uint CRd;
            public uint Offset;

            //
            // Constructor Methods
            //

            internal Opcode_CoprocDataTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsLoad         ,
                                 bool WriteBack      ,
                                 bool Wide           ,
                                 bool Up             ,
                                 bool PreIndex       ,
                                 uint CpNum          ,
                                 uint Rn             ,
                                 uint CRd            ,
                                 uint Offset         )
            {
                base.Prepare( ConditionCodes, CpNum );

                this.IsLoad    = IsLoad;
                this.WriteBack = WriteBack;
                this.Wide      = Wide;
                this.Up        = Up;
                this.PreIndex  = PreIndex;
                this.Rn        = Rn;
                this.CRd       = CRd;
                this.Offset    = Offset;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.IsLoad    = s_Encoding.get_CoprocDataTransfer_IsLoad         ( op );
                this.WriteBack = s_Encoding.get_CoprocDataTransfer_ShouldWriteBack( op );
                this.Wide      = s_Encoding.get_CoprocDataTransfer_IsWide         ( op );
                this.Up        = s_Encoding.get_CoprocDataTransfer_IsUp           ( op );
                this.PreIndex  = s_Encoding.get_CoprocDataTransfer_IsPreIndexing  ( op );
                this.Rn        = s_Encoding.get_CoprocDataTransfer_Rn             ( op );
                this.CRd       = s_Encoding.get_CoprocDataTransfer_CRd            ( op );
                this.Offset    = s_Encoding.get_CoprocDataTransfer_Offset         ( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_CoprocDataTransfer_IsLoad         ( this.IsLoad    );
                op |= s_Encoding.set_CoprocDataTransfer_ShouldWriteBack( this.WriteBack );
                op |= s_Encoding.set_CoprocDataTransfer_IsWide         ( this.Wide      );
                op |= s_Encoding.set_CoprocDataTransfer_IsUp           ( this.Up        );
                op |= s_Encoding.set_CoprocDataTransfer_IsPreIndexing  ( this.PreIndex  );
                op |= s_Encoding.set_CoprocDataTransfer_Rn             ( this.Rn        );
                op |= s_Encoding.set_CoprocDataTransfer_CRd            ( this.CRd       );
                op |= s_Encoding.set_CoprocDataTransfer_Offset         ( this.Offset    );

                return op | EncDef.op_CoprocDataTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                if(this.IsLoad)
                {
                    PrintMnemonic( str, "LDC{0}", DumpCondition() );
                }
                else
                {
                    PrintMnemonic( str, "STC{0}", DumpCondition() );
                }

                str.AppendFormat( "CP{0}, P={1}, U={2}, N={3}, W={4}, C{5}, {6}, 0x{7}", this.CpNum, this.PreIndex ? 1 : 0, this.Up ? 1 : 0, this.Wide ? 1 : 0, this.WriteBack ? 1 : 0, this.CRd, DumpRegister( this.Rn ), this.Offset );
            }
        }

        public sealed class Opcode_CoprocDataOperation : Opcode_Coproc
        {
            //
            // State
            //

            public uint Op1;
            public uint Op2;
            public uint CRn;
            public uint CRm;
            public uint CRd;

            //
            // Constructor Methods
            //

            internal Opcode_CoprocDataOperation()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Op1            ,
                                 uint Op2            ,
                                 uint CpNum          ,
                                 uint CRn            ,
                                 uint CRm            ,
                                 uint CRd            )
            {
                base.Prepare( ConditionCodes, CpNum );

                this.Op1 = Op1;
                this.Op2 = Op2;
                this.CRn = CRn;
                this.CRm = CRm;
                this.CRd = CRd;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Op1 = s_Encoding.get_CoprocDataOperation_Op1( op );
                this.Op2 = s_Encoding.get_CoprocDataOperation_Op2( op );
                this.CRn = s_Encoding.get_CoprocDataOperation_CRn( op );
                this.CRm = s_Encoding.get_CoprocDataOperation_CRm( op );
                this.CRd = s_Encoding.get_CoprocDataOperation_CRd( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_CoprocDataOperation_Op1( this.Op1 );
                op |= s_Encoding.set_CoprocDataOperation_Op2( this.Op2 );
                op |= s_Encoding.set_CoprocDataOperation_CRn( this.CRn );
                op |= s_Encoding.set_CoprocDataOperation_CRm( this.CRm );
                op |= s_Encoding.set_CoprocDataOperation_CRd( this.CRd );

                return op | EncDef.op_CoprocDataOperation;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                str.AppendFormat( "CDP CP{0},0x{1},C{2},C{3},C{4},0x{5}", this.CpNum, this.Op1, this.CRd, this.CRn, this.CRm, this.Op2 );
            }
        }

        public sealed class Opcode_CoprocRegisterTransfer : Opcode_Coproc
        {
            //
            // State
            //

            public bool IsMRC;
            public uint Op1;
            public uint Op2;
            public uint CRn;
            public uint CRm;
            public uint Rd;

            //
            // Constructor Methods
            //

            internal Opcode_CoprocRegisterTransfer()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 bool IsMRC          ,
                                 uint Op1            ,
                                 uint Op2            ,
                                 uint CpNum          ,
                                 uint CRn            ,
                                 uint CRm            ,
                                 uint Rd             )
            {
                base.Prepare( ConditionCodes, CpNum );

                this.IsMRC = IsMRC;
                this.Op1   = Op1;
                this.Op2   = Op2;
                this.CRn   = CRn;
                this.CRm   = CRm;
                this.Rd    = Rd;
            }

            //--//

            public override void Decode( uint op )
            {
                base.Decode( op );

                this.IsMRC = s_Encoding.get_CoprocRegisterTransfer_IsMRC( op );
                this.Op1   = s_Encoding.get_CoprocRegisterTransfer_Op1  ( op );
                this.Op2   = s_Encoding.get_CoprocRegisterTransfer_Op2  ( op );
                this.CRn   = s_Encoding.get_CoprocRegisterTransfer_CRn  ( op );
                this.CRm   = s_Encoding.get_CoprocRegisterTransfer_CRm  ( op );
                this.Rd    = s_Encoding.get_CoprocRegisterTransfer_Rd   ( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_CoprocRegisterTransfer_IsMRC( this.IsMRC );
                op |= s_Encoding.set_CoprocRegisterTransfer_Op1  ( this.Op1   );
                op |= s_Encoding.set_CoprocRegisterTransfer_Op2  ( this.Op2   );
                op |= s_Encoding.set_CoprocRegisterTransfer_CRn  ( this.CRn   );
                op |= s_Encoding.set_CoprocRegisterTransfer_CRm  ( this.CRm   );
                op |= s_Encoding.set_CoprocRegisterTransfer_Rd   ( this.Rd    );

                return op | EncDef.op_CoprocRegisterTransfer;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                if(this.IsMRC)
                {
                    PrintMnemonic( str, "MRC{0}", DumpCondition() );
                }
                else
                {
                    PrintMnemonic( str, "MCR{0}", DumpCondition() );
                }

                str.AppendFormat( "CP{0},0x{1},{2},C{3},C{4},0x{5}", this.CpNum, this.Op1, DumpRegister( this.Rd ), this.CRn, this.CRm, this.Op2 );
            }
        }

        public sealed class Opcode_SoftwareInterrupt : Opcode
        {
            //
            // State
            //

            public uint Value;

            //
            // Constructor Methods
            //

            internal Opcode_SoftwareInterrupt()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Value          )
            {
                base.Prepare( ConditionCodes );

                this.Value = Value;
            }

            //--//


            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Value = s_Encoding.get_SoftwareInterrupt_Immediate( op );
            }

            public override uint Encode()
            {
                uint op = base.Encode();

                op |= s_Encoding.set_SoftwareInterrupt_Immediate( this.Value );

                return op | EncDef.op_SoftwareInterrupt;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "SWI{0}", DumpCondition() );

                str.AppendFormat( "#0x{0:X6}", this.Value );
            }
        }

        public sealed class Opcode_Breakpoint : Opcode
        {
            //
            // State
            //

            public uint Value;

            //
            // Constructor Methods
            //

            internal Opcode_Breakpoint()
            {
            }

            //--//

            public void Prepare( uint ConditionCodes ,
                                 uint Value          )
            {
                base.Prepare( ConditionCodes );

                this.Value = Value;
            }

            //--//


            public override void Decode( uint op )
            {
                base.Decode( op );

                this.Value = s_Encoding.get_Breakpoint_Immediate( op );
            }

            public override uint Encode()
            {
                uint op = s_Encoding.set_Breakpoint_Immediate( this.Value );

                return op | EncDef.op_Breakpoint;
            }

            public override void Print(     InstructionSet            owner         ,
                                            System.Text.StringBuilder str           ,
                                            uint                      opcodeAddress ,
                                        ref uint                      target        ,
                                        ref bool                      targetIsCode  )
            {
                PrintMnemonic( str, "BKPT" );

                str.AppendFormat( "#0x{0:X6}", this.Value );
            }
        }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    public class EncodingDefinition_ARM : EncodingDefinition
    {
        //
        // +---------+---+---+---+---------------+---+---------+---------+---------+---------------+---------+
        // | 3 3 2 2 | 2 | 2 | 2 | 2   2   2   2 | 2 | 1 1 1 1 | 1 1 1 1 | 1 1 9 8 | 7   6   5   4 | 3 2 1 0 |
        // | 1 0 9 8 | 7 | 6 | 5 | 4   3   2   1 | 0 | 9 8 7 6 | 5 4 3 2 | 1 0     |               |         |
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---------------+---------+
        // | Cond    | 0   0   0   1   0 | Ps| 0   0 | 1 1 1 1 | Rd      | 0 0 0 0   0   0   0   0 | 0 0 0 0 | MRS
        // +---------+-------------------+---+-------+---------+---------+-------------------------+---------+
        // | Cond    | 0   0   0   1   0 | Pd| 1   0 | S S S S | 1 1 1 1 | 0 0 0 0   0   0   0   0 | Rm      | MSR
        // +---------+-------------------+---+-------+---------+---------+---------+---------------+---------+
        // | Cond    | 0   0   1   1   0 | Pd| 1   0 | S S S S | 1 1 1 1 | Rotate  | Immediate               | MSR
        // +---------+---+---+---+-------+---+---+---+---------+---------+---------+-------------------------+
        // | Cond    | 0 | 0 | 1 |    Opcode     | S | Rn      | Rd      | Rotate  | Immediate               | Data Processing / PSR Transfer
        // +---------+---+---+---+---------------+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 0 | 0 |    Opcode     | S | Rn      | Rd      | Shift Imm   | SType | 0 | Rm      | Data Processing / PSR Transfer
        // +---------+---+---+---+---------------+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 0 | 0 |    Opcode     | S | Rn      | Rd      | Shift Rp| 0 | SType | 1 | Rm      | Data Processing / PSR Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 0 | 0 | 0 | A | S | Rd      | Rn      | Rs      | 1 | 0 | 0 | 1 | Rm      | Multiply
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 0 | 1 | U | A | S | RdHi    | RdLo    | Rn      | 1 | 0 | 0 | 1 | Rm      | Multiply Long
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 1 | 0 | B | 0 | 0 | Rn      | Rd      | 0 0 0 0 | 1 | 0 | 0 | 1 | Rm      | Single Data Swap
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | 1 | 0 | 0 | 1 | 0 | 1 1 1 1 | 1 1 1 1 | 1 1 1 1 | 0 | 0 | 0 | 1 | Rn      | Branch and Exchange
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | P | U | 0 | W | L | Rn      | Rd      | 0 0 0 0 | 1 | S | H | 1 | Rm      | Halfword Data Transfer: register offset
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 0 | 0 | P | U | 1 | W | L | Rn      | Rd      | Offset  | 1 | S | H | 1 | Offset  | Halfword Data Transfer: immediate offset
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 1 | 0 | P | U | B | W | L | Rn      | Rd      |          Immediate Offset         | Single Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+---+---+---+---------+
        // | Cond    | 0 | 1 | 1 | P | U | B | W | L | Rn      | Rd      | Shift Imm   | SType | 0 | Rm      | Single Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 1 | 1 | P | U | B | W | L | Rn      | Rd      | Shift Rp| 0 | SType | 1 | Rm      | Single Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 0 | 1 | 1 | XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX| 1 | XXXXX | 1 | XXXXXX  | Undefined
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+---+-------+---+---------+
        // | Cond    | 1 | 0 | 0 | P | U | S | W | L | Rn      |             Register List                   | Block Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------------------------------------------+
        // | Cond    | 1 | 0 | 1 | L |                               Offset                                  | Branch
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+-------------------------+
        // | Cond    | 1 | 1 | 0 | P | U | N | W | L | Rn      | CRd     | CP#     |       Offset            | Coprocessor Data Transfer
        // +---------+---+---+---+---+---+---+---+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 1 | 1 | 0 | CP Opc        | CRn     | CRd     | CP#     |     CP    | 0 | CRm     | Coprocessor Data Operation
        // +---------+---+---+---+---+-----------+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 1 | 1 | 0 | CP Opc    | L | CRn     | Rd      | CP#     |     CP    | 1 | CRm     | Coprocessor Register Transfer
        // +---------+---+---+---+---+-----------+---+---------+---------+---------+-----------+---+---------+
        // | Cond    | 1 | 1 | 1 | 1 |                       Ignored by processor                            | Software Interrupt
        // +---------+---+---+---+---+-----------------------------------------------------------------------+
        //
        public const uint op_MRS                    = 0x010F0000; public const uint opmask_MRS                    = 0x0FBF0FFF;
        public const uint op_MSR_1                  = 0x0120F000; public const uint opmask_MSR_1                  = 0x0FB0FFF0;
        public const uint op_MSR_2                  = 0x0320F000; public const uint opmask_MSR_2                  = 0x0FB0F000;
        public const uint op_DataProcessing_1       = 0x02000000; public const uint opmask_DataProcessing_1       = 0x0E000000;
        public const uint op_DataProcessing_2       = 0x00000000; public const uint opmask_DataProcessing_2       = 0x0E000010;
        public const uint op_DataProcessing_3       = 0x00000010; public const uint opmask_DataProcessing_3       = 0x0E000090;
        public const uint op_Multiply               = 0x00000090; public const uint opmask_Multiply               = 0x0FC000F0;
        public const uint op_MultiplyLong           = 0x00800090; public const uint opmask_MultiplyLong           = 0x0F8000F0;
        public const uint op_SingleDataSwap         = 0x01000090; public const uint opmask_SingleDataSwap         = 0x0FB00FF0;
        public const uint op_BranchAndExchange      = 0x012FFF10; public const uint opmask_BranchAndExchange      = 0x0FFFFFF0;
        public const uint op_HalfwordDataTransfer_1 = 0x00000090; public const uint opmask_HalfwordDataTransfer_1 = 0x0E400F90;
        public const uint op_HalfwordDataTransfer_2 = 0x00400090; public const uint opmask_HalfwordDataTransfer_2 = 0x0E400090;
        public const uint op_SingleDataTransfer_1   = 0x04000000; public const uint opmask_SingleDataTransfer_1   = 0x0E000000;
        public const uint op_SingleDataTransfer_2   = 0x06000000; public const uint opmask_SingleDataTransfer_2   = 0x0E000010;
        public const uint op_SingleDataTransfer_3   = 0x06000010; public const uint opmask_SingleDataTransfer_3   = 0x0E000090;
        public const uint op_Undefined              = 0x06000090; public const uint opmask_Undefined              = 0x0E000090;
        public const uint op_BlockDataTransfer      = 0x08000000; public const uint opmask_BlockDataTransfer      = 0x0E000000;
        public const uint op_Branch                 = 0x0A000000; public const uint opmask_Branch                 = 0x0E000000;
        public const uint op_CoprocDataTransfer     = 0x0C000000; public const uint opmask_CoprocDataTransfer     = 0x0E000000;
        public const uint op_CoprocDataOperation    = 0x0E000000; public const uint opmask_CoprocDataOperation    = 0x0F000010;
        public const uint op_CoprocRegisterTransfer = 0x0E000010; public const uint opmask_CoprocRegisterTransfer = 0x0F000010;
        public const uint op_SoftwareInterrupt      = 0x0F000000; public const uint opmask_SoftwareInterrupt      = 0x0F000000;
        public const uint op_Breakpoint             = 0xE1200070; public const uint opmask_Breakpoint             = 0xFFF000F0;

        //--//

        public const int c_psr_bit_T       = 5;
        public const int c_psr_bit_F       = 6;
        public const int c_psr_bit_I       = 7;
        public const int c_psr_bit_V       = 28;
        public const int c_psr_bit_C       = 29;
        public const int c_psr_bit_Z       = 30;
        public const int c_psr_bit_N       = 31;


        public const uint c_psr_T          = 1U << c_psr_bit_T;
        public const uint c_psr_F          = 1U << c_psr_bit_F;
        public const uint c_psr_I          = 1U << c_psr_bit_I;
        public const uint c_psr_V          = 1U << c_psr_bit_V;
        public const uint c_psr_C          = 1U << c_psr_bit_C;
        public const uint c_psr_Z          = 1U << c_psr_bit_Z;
        public const uint c_psr_N          = 1U << c_psr_bit_N;


        public const int  c_psr_cc_shift   =                      c_psr_bit_V;
        public const uint c_psr_cc_num     = 1U << (c_psr_bit_N - c_psr_bit_V + 1);
        public const uint c_psr_cc_mask    = c_psr_cc_num - 1;


        public const uint c_psr_mode       = 0x0000001F;
        public const uint c_psr_mode_USER  = 0x00000010;
        public const uint c_psr_mode_FIQ   = 0x00000011;
        public const uint c_psr_mode_IRQ   = 0x00000012;
        public const uint c_psr_mode_SVC   = 0x00000013;
        public const uint c_psr_mode_ABORT = 0x00000017;
        public const uint c_psr_mode_UNDEF = 0x0000001B;
        public const uint c_psr_mode_SYS   = 0x0000001F;

        //--//

        /////////////////////////////////////////
        public const uint c_cond_EQ     = 0x0; //  Z set                                equal
        public const uint c_cond_NE     = 0x1; //  Z clear                          not equal
        public const uint c_cond_CS     = 0x2; //  C set                   unsigned     higher or same
        public const uint c_cond_CC     = 0x3; //  C clear                 unsigned     lower
        public const uint c_cond_MI     = 0x4; //  N set                                negative
        public const uint c_cond_PL     = 0x5; //  N clear                              positive or zero
        public const uint c_cond_VS     = 0x6; //  V set                                overflow
        public const uint c_cond_VC     = 0x7; //  V clear                           no overflow
        public const uint c_cond_HI     = 0x8; //  C set and Z clear       unsigned     higher
        public const uint c_cond_LS     = 0x9; //  C clear or Z set        unsigned     lower or same
        public const uint c_cond_GE     = 0xA; //  N equals V                           greater or equal
        public const uint c_cond_LT     = 0xB; //  N not equal to V                     less than
        public const uint c_cond_GT     = 0xC; //  Z clear AND (N equals V)             greater than
        public const uint c_cond_LE     = 0xD; //  Z set OR (N not equal to V)          less than or equal
        public const uint c_cond_AL     = 0xE; //  (ignored) always
        public const uint c_cond_UNUSED = 0xF; //
        /////////////////////////////////////////
        public const uint c_cond_NUM    = 0x10;

        ///////////////////////////////////////////
        public const uint c_operation_AND = 0x0; // operand1 AND operand2
        public const uint c_operation_EOR = 0x1; // operand1 EOR operand2
        public const uint c_operation_SUB = 0x2; // operand1 - operand2
        public const uint c_operation_RSB = 0x3; // operand2 - operand1
        public const uint c_operation_ADD = 0x4; // operand1 + operand2
        public const uint c_operation_ADC = 0x5; // operand1 + operand2 + carry
        public const uint c_operation_SBC = 0x6; // operand1 - operand2 + carry - 1
        public const uint c_operation_RSC = 0x7; // operand2 - operand1 + carry - 1
        public const uint c_operation_TST = 0x8; // as AND, but result is not written
        public const uint c_operation_TEQ = 0x9; // as EOR, but result is not written
        public const uint c_operation_CMP = 0xA; // as SUB, but result is not written
        public const uint c_operation_CMN = 0xB; // as ADD, but result is not written
        public const uint c_operation_ORR = 0xC; // operand1 OR operand2
        public const uint c_operation_MOV = 0xD; // operand2(operand1 is ignored)
        public const uint c_operation_BIC = 0xE; // operand1 AND NOT operand2(Bit clear)
        public const uint c_operation_MVN = 0xF; // NOT operand2(operand1 is ignored)
        ///////////////////////////////////////////

        //--//

        ///////////////////////////////////////
        public const uint c_shift_LSL = 0x0; // logical shift left
        public const uint c_shift_LSR = 0x1; // logical shift right
        public const uint c_shift_ASR = 0x2; // arithmetic shift right
        public const uint c_shift_ROR = 0x3; // rotate right
        public const uint c_shift_RRX = 0x4; // rotate right with extend
        ///////////////////////////////////////

        //--//

        //////////////////////////////////////////////
        public const uint c_halfwordkind_SWP = 0x0; //
        public const uint c_halfwordkind_U2  = 0x1; //
        public const uint c_halfwordkind_I1  = 0x2; //
        public const uint c_halfwordkind_I2  = 0x3; //
        //////////////////////////////////////////////

        //--//

        /////////////////////////////////////////
        public const uint c_psr_field_c   = 0x1; // the control field   PSR[ 7: 0]
        public const uint c_psr_field_x   = 0x2; // the extension field PSR[15: 8]
        public const uint c_psr_field_s   = 0x4; // the status field    PSR[23:16]
        public const uint c_psr_field_f   = 0x8; // the flags field     PSR[31:24]
        public const uint c_psr_field_ALL = c_psr_field_c |
                                            c_psr_field_x |
                                            c_psr_field_s |
                                            c_psr_field_f ;
        /////////////////////////////////////////

        //--//

        //////////////////////////////////////////
        public const uint c_register_cpsr = 16; //
        public const uint c_register_spsr = 17; //
        //////////////////////////////////////////

        //--//

        override public uint get_ConditionCodes     ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 28, 4 ); }
        override public uint set_ConditionCodes     ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 28, 4 ); }

        override public bool get_ShouldSetConditions( uint op  ) { return OPCODE_DECODE_CHECKFLAG   ( op , 20    ); }
        override public uint set_ShouldSetConditions( bool val ) { return OPCODE_DECODE_SETFLAG     ( val, 20    ); }

        //--//

        override public uint get_Register1( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_Register1( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }

        override public uint get_Register2( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 12, 4 ); }
        override public uint set_Register2( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 12, 4 ); }

        override public uint get_Register3( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  8, 4 ); }
        override public uint set_Register3( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  8, 4 ); }

        override public uint get_Register4( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  0, 4 ); }
        override public uint set_Register4( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  0, 4 ); }

        //--//

        override public bool get_Multiply_IsAccumulate( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 21 ); }
        override public uint set_Multiply_IsAccumulate( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 21 ); }

        override public bool get_Multiply_IsSigned    ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 22 ); }
        override public uint set_Multiply_IsSigned    ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 22 ); }

        //--//

        override public bool get_StatusRegister_IsSPSR( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 22 ); }
        override public uint set_StatusRegister_IsSPSR( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 22 ); }

        override public uint get_StatusRegister_Fields( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_StatusRegister_Fields( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }

        //--//

        override public uint get_Shift_Type     ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 5, 2 ); }
        override public uint set_Shift_Type     ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 5, 2 ); }

        override public uint get_Shift_Immediate( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 7, 5 ); }
        override public uint set_Shift_Immediate( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 7, 5 ); }

        override public uint get_Shift_Register ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 8, 4 ); }
        override public uint set_Shift_Register ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 8, 4 ); }

        //--//

        override public uint get_DataProcessing_Operation( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 21, 4 ); }
        override public uint set_DataProcessing_Operation( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 21, 4 ); }

        override public uint get_DataProcessing_ImmediateSeed    ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 0, 8 ); }
        override public uint set_DataProcessing_ImmediateSeed    ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 0, 8 ); }

        override public uint get_DataProcessing_ImmediateRotation( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 8, 4 ); }
        override public uint set_DataProcessing_ImmediateRotation( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 8, 4 ); }

        override public uint get_DataProcessing_ImmediateValue( uint op )
        {
            return get_DataProcessing_ImmediateValue( get_DataProcessing_ImmediateSeed( op ), get_DataProcessing_ImmediateRotation( op ) );
        }

        override public uint get_DataProcessing_ImmediateValue( uint imm, uint rot )
        {
            rot *= 2;

            return (imm >> (int)rot) | (imm << (int)(32 - rot));
        }

        override public bool check_DataProcessing_ImmediateValue( uint val, out uint immRes, out uint rotRes )
        {
            uint imm = val;
            uint rot = 0;

            while((imm & ~0xFF) != 0)
            {
                if(rot == 16)
                {
                    immRes = 0;
                    rotRes = 0;
                    return false;
                }

                imm = (imm << 2) | (imm >> 30);
                rot++;
            }

            immRes = imm;
            rotRes = rot;
            return true;
        }

        //--//

        override public bool get_DataTransfer_IsLoad         ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 20 ); }
        override public uint set_DataTransfer_IsLoad         ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 20 ); }

        override public bool get_DataTransfer_ShouldWriteBack( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 21 ); }
        override public uint set_DataTransfer_ShouldWriteBack( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 21 ); }

        override public bool get_DataTransfer_IsByteTransfer ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 22 ); }
        override public uint set_DataTransfer_IsByteTransfer ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 22 ); }

        override public bool get_DataTransfer_IsUp           ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 23 ); }
        override public uint set_DataTransfer_IsUp           ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 23 ); }

        override public bool get_DataTransfer_IsPreIndexing  ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 24 ); }
        override public uint set_DataTransfer_IsPreIndexing  ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 24 ); }

        override public uint get_DataTransfer_Offset         ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  0, 12 ); }
        override public uint set_DataTransfer_Offset         ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  0, 12 ); }

        //--//

        override public uint get_HalfWordDataTransfer_Kind  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,       5, 2 )                                                ; }
        override public uint set_HalfWordDataTransfer_Kind  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,       5, 2 )                                                ; }
        override public uint get_HalfWordDataTransfer_Offset( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 4, 8, 8, 4 ) | OPCODE_DECODE_EXTRACTFIELD( op , 0, 8, 0, 4 ); }
        override public uint set_HalfWordDataTransfer_Offset( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 4, 8, 8, 4 ) | OPCODE_DECODE_INSERTFIELD ( val, 0, 8, 0, 4 ); }

        //--//

        override public bool get_BlockDataTransfer_LoadPSR     ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 22 ); }
        override public uint set_BlockDataTransfer_LoadPSR     ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 22 ); }

        override public uint get_BlockDataTransfer_RegisterList( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 0 , 16 ); }
        override public uint set_BlockDataTransfer_RegisterList( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 0 , 16 ); }

        //--//

        override public bool get_Branch_IsLink( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 24 ); }
        override public uint set_Branch_IsLink( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 24 ); }

        override public int  get_Branch_Offset( uint op  ) { return ((int)(op << 8)) >> 6; }
        override public uint set_Branch_Offset( int  val ) { return OPCODE_DECODE_INSERTFIELD( val >> 2, 0, 24); }

        //--//

        override public uint get_Coproc_CpNum( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  8, 4 ); }
        override public uint set_Coproc_CpNum( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  8, 4 ); }
                                                        
        //--//

        override public bool get_CoprocRegisterTransfer_IsMRC( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 20 ); }
        override public uint set_CoprocRegisterTransfer_IsMRC( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 20 ); }

        override public uint get_CoprocRegisterTransfer_Op1  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 21, 3 ); }
        override public uint set_CoprocRegisterTransfer_Op1  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 21, 3 ); }

        override public uint get_CoprocRegisterTransfer_Op2  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  5, 3 ); }
        override public uint set_CoprocRegisterTransfer_Op2  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  5, 3 ); }

        override public uint get_CoprocRegisterTransfer_CRn  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_CoprocRegisterTransfer_CRn  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }

        override public uint get_CoprocRegisterTransfer_CRm  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  0, 4 ); }
        override public uint set_CoprocRegisterTransfer_CRm  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  0, 4 ); }

        override public uint get_CoprocRegisterTransfer_Rd   ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 12, 4 ); }
        override public uint set_CoprocRegisterTransfer_Rd   ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 12, 4 ); }

        //--//

        override public bool get_CoprocDataTransfer_IsLoad         ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 20 ); }
        override public uint set_CoprocDataTransfer_IsLoad         ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 20 ); }

        override public bool get_CoprocDataTransfer_ShouldWriteBack( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 21 ); }
        override public uint set_CoprocDataTransfer_ShouldWriteBack( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 21 ); }
                                                             
        override public bool get_CoprocDataTransfer_IsWide         ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 22 ); }
        override public uint set_CoprocDataTransfer_IsWide         ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 22 ); }

        override public bool get_CoprocDataTransfer_IsUp           ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 23 ); }
        override public uint set_CoprocDataTransfer_IsUp           ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 23 ); }

        override public bool get_CoprocDataTransfer_IsPreIndexing  ( uint op  ) { return OPCODE_DECODE_CHECKFLAG( op , 24 ); }
        override public uint set_CoprocDataTransfer_IsPreIndexing  ( bool val ) { return OPCODE_DECODE_SETFLAG  ( val, 24 ); }
                                                        
        override public uint get_CoprocDataTransfer_Rn             ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_CoprocDataTransfer_Rn             ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }
                                                        
        override public uint get_CoprocDataTransfer_CRd            ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 12, 4 ); }
        override public uint set_CoprocDataTransfer_CRd            ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 12, 4 ); }
                                                        
        override public uint get_CoprocDataTransfer_Offset         ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  0, 8 ); }
        override public uint set_CoprocDataTransfer_Offset         ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  0, 8 ); }

        //--//

        override public uint get_CoprocDataOperation_Op1  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 20, 4 ); }
        override public uint set_CoprocDataOperation_Op1  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 20, 4 ); }

        override public uint get_CoprocDataOperation_Op2  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  5, 3 ); }
        override public uint set_CoprocDataOperation_Op2  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  5, 3 ); }

        override public uint get_CoprocDataOperation_CRn  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_CoprocDataOperation_CRn  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }

        override public uint get_CoprocDataOperation_CRm  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op ,  0, 4 ); }
        override public uint set_CoprocDataOperation_CRm  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val,  0, 4 ); }

        override public uint get_CoprocDataOperation_CRd  ( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 12, 4 ); }
        override public uint set_CoprocDataOperation_CRd  ( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 12, 4 ); }

        //--//

        override public uint get_SoftwareInterrupt_Immediate( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 0, 24 ); }
        override public uint set_SoftwareInterrupt_Immediate( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 0, 24 ); }

        //--//

        override public uint get_Breakpoint_Immediate( uint op  ) { return OPCODE_DECODE_EXTRACTFIELD( op , 4, 16, 8, 12 ) | OPCODE_DECODE_EXTRACTFIELD( op , 0, 16, 0, 4 ); }
        override public uint set_Breakpoint_Immediate( uint val ) { return OPCODE_DECODE_INSERTFIELD ( val, 4, 16, 8, 12 ) | OPCODE_DECODE_INSERTFIELD ( val, 0, 16, 0, 4 ); }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    abstract public class EncodingDefinition
    {
        public const int c_PC_offset       = 8;

        //--//

        /////////////////////////////////////////

        //--//

        /////////////////////////////////////////
        public const uint c_register_r0  =  0; //
        public const uint c_register_r1  =  1; //
        public const uint c_register_r2  =  2; //
        public const uint c_register_r3  =  3; //
        public const uint c_register_r4  =  4; //
        public const uint c_register_r5  =  5; //
        public const uint c_register_r6  =  6; //
        public const uint c_register_r7  =  7; //
        public const uint c_register_r8  =  8; //
        public const uint c_register_r9  =  9; //
        public const uint c_register_r10 = 10; //
        public const uint c_register_r11 = 11; //
        public const uint c_register_r12 = 12; //
        public const uint c_register_r13 = 13; //
        public const uint c_register_r14 = 14; //
        public const uint c_register_r15 = 15; //
        public const uint c_register_sp  = 13; //
        public const uint c_register_lr  = 14; //
        public const uint c_register_pc  = 15; //
        /////////////////////////////////////////

        //////////////////////////////////////////

        public const uint c_register_lst_r0  = 1U << (int)c_register_r0 ;
        public const uint c_register_lst_r1  = 1U << (int)c_register_r1 ;
        public const uint c_register_lst_r2  = 1U << (int)c_register_r2 ;
        public const uint c_register_lst_r3  = 1U << (int)c_register_r3 ;
        public const uint c_register_lst_r4  = 1U << (int)c_register_r4 ;
        public const uint c_register_lst_r5  = 1U << (int)c_register_r5 ;
        public const uint c_register_lst_r6  = 1U << (int)c_register_r6 ;
        public const uint c_register_lst_r7  = 1U << (int)c_register_r7 ;
        public const uint c_register_lst_r8  = 1U << (int)c_register_r8 ;
        public const uint c_register_lst_r9  = 1U << (int)c_register_r9 ;
        public const uint c_register_lst_r10 = 1U << (int)c_register_r10;
        public const uint c_register_lst_r11 = 1U << (int)c_register_r11;
        public const uint c_register_lst_r12 = 1U << (int)c_register_r12;
        public const uint c_register_lst_r13 = 1U << (int)c_register_r13;
        public const uint c_register_lst_r14 = 1U << (int)c_register_r14;
        public const uint c_register_lst_r15 = 1U << (int)c_register_r15;
        public const uint c_register_lst_sp  = 1U << (int)c_register_sp ;
        public const uint c_register_lst_lr  = 1U << (int)c_register_lr ;
        public const uint c_register_lst_pc  = 1U << (int)c_register_pc ;

        //--//

        public enum Format
        {
            MRS                    ,
            MSR_1                  ,
            MSR_2                  ,
            DataProcessing_1       ,
            DataProcessing_2       ,
            DataProcessing_3       ,
            Multiply               ,
            MultiplyLong           ,
            SingleDataSwap         ,
            BranchAndExchange      ,
            HalfwordDataTransfer_1 ,
            HalfwordDataTransfer_2 ,
            SingleDataTransfer_1   ,
            SingleDataTransfer_2   ,
            SingleDataTransfer_3   ,
            Undefined              ,
            BlockDataTransfer      ,
            Branch                 ,
            CoprocDataTransfer     ,
            CoprocDataOperation    ,
            CoprocRegisterTransfer ,
            SoftwareInterrupt      ,

            FIRST_FORMAT   = MRS,
            LAST_FORMAT    = SoftwareInterrupt,
            NUM_OF_FORMATS = (SoftwareInterrupt - MRS + 1)
        };

        //--//

        [System.Diagnostics.Conditional( "DEBUG" )]
        static void OPCODE_VERIFY_INSERT_FIELD( int val    ,
                                                int bitLen )
        {
            int valHigh = val >> bitLen;

            if(valHigh !=  0 &&
               valHigh != -1  )
            {
                throw new System.ArgumentException( string.Format( "Found value outside bounds of field [{0}:{1}]: {2}", -(1 << (bitLen-1)), (1 << (bitLen-1)), val ) );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        static void OPCODE_VERIFY_INSERT_FIELD( uint val    ,
                                                int  bitLen )
        {
            uint valHigh = val >> bitLen;

            if(valHigh != 0)
            {
                throw new System.ArgumentException( string.Format( "Found value outside bounds of field [{0}:{1}]: {2}", 0, (1 << bitLen) - 1, val ) );
            }
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        static void OPCODE_VERIFY_INSERT_FIELD( uint val    ,
                                                int  valPos ,
                                                int  valLen ,
                                                int  bitLen )
        {
            uint valHigh = val >> valLen;

            if(valHigh != 0)
            {
                throw new System.ArgumentException( string.Format( "Found value outside bounds of field [{0}:{1}]: {2}", 0, (1 << bitLen) - 1, val ) );
            }
        }

        //--//

        static public uint OPCODE_DECODE_MASK( int len )
        {
            return (1U << len) - 1U;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( int val    ,
                                                      int bitPos ,
                                                      int bitLen )
        {
            OPCODE_VERIFY_INSERT_FIELD( val, bitLen );

            return ((uint)val & OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( uint val    ,
                                                      int  bitPos ,
                                                      int  bitLen )
        {
            OPCODE_VERIFY_INSERT_FIELD( val, bitLen );

            return (val & OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_INSERTFIELD( uint val    ,
                                                      int  valPos ,
                                                      int  valLen ,
                                                      int  bitPos ,
                                                      int  bitLen )
        {
            OPCODE_VERIFY_INSERT_FIELD( val, valPos, valLen, bitLen );

            return ((val >> valPos) & EncodingDefinition_ARM.OPCODE_DECODE_MASK( bitLen )) << bitPos;
        }

        static public uint OPCODE_DECODE_EXTRACTFIELD( uint op     ,
                                                       int  bitPos ,
                                                       int  bitLen )
        {
            return (op >> bitPos) & OPCODE_DECODE_MASK( bitLen );
        }

        static public uint OPCODE_DECODE_EXTRACTFIELD( uint op     ,
                                                       int  valPos ,
                                                       int  valLen ,
                                                       int  bitPos ,
                                                       int  bitLen )
        {
            return ((op >> bitPos) & EncodingDefinition_ARM.OPCODE_DECODE_MASK( bitLen )) << valPos;
        }

        //--//

        static public uint OPCODE_DECODE_SETFLAG( bool val    ,
                                                  int  bitPos )
        {
            return val ? (1U << bitPos) : 0U;
        }

        static public bool OPCODE_DECODE_CHECKFLAG( uint op     ,
                                                    int  bitPos )
        {
            return (op & (1U << bitPos)) != 0;
        }

        //--//

        abstract public uint get_ConditionCodes     ( uint op  );
        abstract public uint set_ConditionCodes     ( uint val );

        abstract public bool get_ShouldSetConditions( uint op  );
        abstract public uint set_ShouldSetConditions( bool val );

        //--//

        abstract public uint get_Register1( uint op  );
        abstract public uint set_Register1( uint val );

        abstract public uint get_Register2( uint op  );
        abstract public uint set_Register2( uint val );

        abstract public uint get_Register3( uint op  );
        abstract public uint set_Register3( uint val );

        abstract public uint get_Register4( uint op  );
        abstract public uint set_Register4( uint val );

        //--//

        abstract public bool get_Multiply_IsAccumulate( uint op  );
        abstract public uint set_Multiply_IsAccumulate( bool val );

        abstract public bool get_Multiply_IsSigned    ( uint op  );
        abstract public uint set_Multiply_IsSigned    ( bool val );

        //--//

        abstract public bool get_StatusRegister_IsSPSR( uint op  );
        abstract public uint set_StatusRegister_IsSPSR( bool val );

        abstract public uint get_StatusRegister_Fields( uint op  );
        abstract public uint set_StatusRegister_Fields( uint val );

        //--//

        abstract public uint get_Shift_Type     ( uint op  );
        abstract public uint set_Shift_Type     ( uint val );

        abstract public uint get_Shift_Immediate( uint op  );
        abstract public uint set_Shift_Immediate( uint val );

        abstract public uint get_Shift_Register ( uint op  );
        abstract public uint set_Shift_Register ( uint val );

        //--//

        abstract public uint get_DataProcessing_Operation( uint op  );
        abstract public uint set_DataProcessing_Operation( uint val );

        abstract public uint get_DataProcessing_ImmediateSeed    ( uint op  );
        abstract public uint set_DataProcessing_ImmediateSeed    ( uint val );

        abstract public uint get_DataProcessing_ImmediateRotation( uint op  );
        abstract public uint set_DataProcessing_ImmediateRotation( uint val );

        abstract public uint get_DataProcessing_ImmediateValue( uint op );

        abstract public uint get_DataProcessing_ImmediateValue( uint imm, uint rot );

        abstract public bool check_DataProcessing_ImmediateValue( uint val, out uint immRes, out uint rotRes );

        //--//

        abstract public bool get_DataTransfer_IsLoad         ( uint op  );
        abstract public uint set_DataTransfer_IsLoad         ( bool val );

        abstract public bool get_DataTransfer_ShouldWriteBack( uint op  );
        abstract public uint set_DataTransfer_ShouldWriteBack( bool val );

        abstract public bool get_DataTransfer_IsByteTransfer ( uint op  );
        abstract public uint set_DataTransfer_IsByteTransfer ( bool val );

        abstract public bool get_DataTransfer_IsUp           ( uint op  );
        abstract public uint set_DataTransfer_IsUp           ( bool val );

        abstract public bool get_DataTransfer_IsPreIndexing  ( uint op  );
        abstract public uint set_DataTransfer_IsPreIndexing  ( bool val );

        abstract public uint get_DataTransfer_Offset         ( uint op  );
        abstract public uint set_DataTransfer_Offset         ( uint val );

        //--//

        abstract public uint get_HalfWordDataTransfer_Kind  ( uint op  );
        abstract public uint set_HalfWordDataTransfer_Kind  ( uint val );
        abstract public uint get_HalfWordDataTransfer_Offset( uint op  );
        abstract public uint set_HalfWordDataTransfer_Offset( uint val );

        //--//

        abstract public bool get_BlockDataTransfer_LoadPSR     ( uint op  );
        abstract public uint set_BlockDataTransfer_LoadPSR     ( bool val );

        abstract public uint get_BlockDataTransfer_RegisterList( uint op  );
        abstract public uint set_BlockDataTransfer_RegisterList( uint val );

        //--//

        abstract public bool get_Branch_IsLink( uint op  );
        abstract public uint set_Branch_IsLink( bool val );

        abstract public int  get_Branch_Offset( uint op  );
        abstract public uint set_Branch_Offset( int  val );

        //--//

        abstract public uint get_Coproc_CpNum( uint op  );
        abstract public uint set_Coproc_CpNum( uint val );
                                                        
        //--//

        abstract public bool get_CoprocRegisterTransfer_IsMRC( uint op  );
        abstract public uint set_CoprocRegisterTransfer_IsMRC( bool val );

        abstract public uint get_CoprocRegisterTransfer_Op1  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_Op1  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_Op2  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_Op2  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_CRn  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_CRn  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_CRm  ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_CRm  ( uint val );

        abstract public uint get_CoprocRegisterTransfer_Rd   ( uint op  );
        abstract public uint set_CoprocRegisterTransfer_Rd   ( uint val );

        //--//

        abstract public bool get_CoprocDataTransfer_IsLoad         ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsLoad         ( bool val );

        abstract public bool get_CoprocDataTransfer_ShouldWriteBack( uint op  );
        abstract public uint set_CoprocDataTransfer_ShouldWriteBack( bool val );
                                                             
        abstract public bool get_CoprocDataTransfer_IsWide         ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsWide         ( bool val );

        abstract public bool get_CoprocDataTransfer_IsUp           ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsUp           ( bool val );

        abstract public bool get_CoprocDataTransfer_IsPreIndexing  ( uint op  );
        abstract public uint set_CoprocDataTransfer_IsPreIndexing  ( bool val );
                                                        
        abstract public uint get_CoprocDataTransfer_Rn             ( uint op  );
        abstract public uint set_CoprocDataTransfer_Rn             ( uint val );
                                                        
        abstract public uint get_CoprocDataTransfer_CRd            ( uint op  );
        abstract public uint set_CoprocDataTransfer_CRd            ( uint val );
                                                        
        abstract public uint get_CoprocDataTransfer_Offset         ( uint op  );
        abstract public uint set_CoprocDataTransfer_Offset         ( uint val );

        //--//

        abstract public uint get_CoprocDataOperation_Op1  ( uint op  );
        abstract public uint set_CoprocDataOperation_Op1  ( uint val );

        abstract public uint get_CoprocDataOperation_Op2  ( uint op  );
        abstract public uint set_CoprocDataOperation_Op2  ( uint val );

        abstract public uint get_CoprocDataOperation_CRn  ( uint op  );
        abstract public uint set_CoprocDataOperation_CRn  ( uint val );

        abstract public uint get_CoprocDataOperation_CRm  ( uint op  );
        abstract public uint set_CoprocDataOperation_CRm  ( uint val );

        abstract public uint get_CoprocDataOperation_CRd  ( uint op  );
        abstract public uint set_CoprocDataOperation_CRd  ( uint val );

        //--//

        abstract public uint get_SoftwareInterrupt_Immediate( uint op  );
        abstract public uint set_SoftwareInterrupt_Immediate( uint val );

        //--//

        abstract public uint get_Breakpoint_Immediate( uint op  );
        abstract public uint set_Breakpoint_Immediate( uint val );
    }
}

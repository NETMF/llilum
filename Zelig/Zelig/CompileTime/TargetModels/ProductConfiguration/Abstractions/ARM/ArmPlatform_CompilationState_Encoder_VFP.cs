//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmCompilationState
    {
        //
        // Helper Methods
        //

        public void EmitOpcode__FCPY_IfDifferent( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                                  ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            if(Fd.Encoding != Fm.Encoding)
            {
                EmitOpcode__FCPY( Fd, Fm );
            }
        }

        public void EmitOpcode__FCPY( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__UnaryOp( Fd, Fm, EncodingDefinition_VFP.c_unaryOperation_CPY );
        }

        public void EmitOpcode__FNEG( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__UnaryOp( Fd, Fm, EncodingDefinition_VFP.c_unaryOperation_NEG );
        }

        public void EmitOpcode__FCMP( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__UnaryOp( Fd, Fm, EncodingDefinition_VFP.c_unaryOperation_CMP );
        }

        public void EmitOpcode__FCMPZ( ZeligIR.Abstractions.RegisterDescriptor Fd )
        {
            InstructionSet_VFP.Opcode_VFP_CompareToZero enc = this.Encoder_VFP.PrepareForVFP_CompareToZero;

            if(Fd.IsDoublePrecision)
            {
                enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                             true                             ,  // bool IsDouble       ,
                             GetDoublePrecisionEncoding( Fd ) ,  // uint Fd             ,
                             false                            ); // bool CheckNaN       )
            }
            else
            {
                enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                             false                            ,  // bool IsDouble       ,
                             GetSinglePrecisionEncoding( Fd ) ,  // uint Fd             ,
                             false                            ); // bool CheckNaN       )
            }

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FMSTAT()
        {
            InstructionSet_VFP.Opcode_VFP_ConditionCodeTransfer enc = this.Encoder_VFP.PrepareForVFP_ConditionCodeTransfer;

            enc.Prepare( m_pendingCondition ); // uint ConditionCodes ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FMRX( ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                      ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            InstructionSet_VFP.Opcode_VFP_SystemRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_SystemRegisterTransfer;

            enc.Prepare( m_pendingCondition,         // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ) ,  // uint Rd             ,
                         GetSystemEncoding( reg ) ,  // uint SysReg         ,
                         true                     ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FMXR( ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                      ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            InstructionSet_VFP.Opcode_VFP_SystemRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_SystemRegisterTransfer;

            enc.Prepare( m_pendingCondition,         // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ) ,  // uint Rd             ,
                         GetSystemEncoding( reg ) ,  // uint SysReg         ,
                         false                    ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }


        private void EmitOpcode__UnaryOp( ZeligIR.Abstractions.RegisterDescriptor Fd  ,
                                          ZeligIR.Abstractions.RegisterDescriptor Fm  ,
                                          uint                                    Alu )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            if(Fd.IsDoublePrecision)
            {
                enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                             true                            ,  // bool IsDouble       ,
                             GetDoublePrecisionEncoding( Fd ),  // uint Fd             ,
                             GetDoublePrecisionEncoding( Fm ),  // uint Fm             ,
                             Alu                             ); // uint Alu            ,
            }
            else
            {
                enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                             false                           ,  // bool IsDouble       ,
                             GetSinglePrecisionEncoding( Fd ),  // uint Fd             ,
                             GetSinglePrecisionEncoding( Fm ),  // uint Fm             ,
                             Alu                             ); // uint Alu            ,
            }

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FSITOS( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         false                                        ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_SITO ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FSITOD( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         true                                         ,  // bool IsDouble       ,
                         GetDoublePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_SITO ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FUITOS( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         false                                        ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_UITO ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FUITOD( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         true                                         ,  // bool IsDouble       ,
                         GetDoublePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_UITO ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FTOSIS( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         false                                        ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_TOSI ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FTOSID( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         true                                         ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetDoublePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_TOSI ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FTOUIS( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         false                                        ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_TOUI ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FTOUID( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            InstructionSet_VFP.Opcode_VFP_UnaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_UnaryDataOperation;

            enc.Prepare( m_pendingCondition                           ,  // uint ConditionCodes ,
                         true                                         ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Fd )             ,  // uint Fd             ,
                         GetDoublePrecisionEncoding( Fm )             ,  // uint Fm             ,
                         EncodingDefinition_VFP.c_unaryOperation_TOUI ); // uint Alu            ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FCVTSD( ZeligIR.Abstractions.RegisterDescriptor Sd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Dm )
        {
            InstructionSet_VFP.Opcode_VFP_ConvertFloatToFloat enc = this.Encoder_VFP.PrepareForVFP_ConvertFloatToFloat;

            enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                         true                             ,  // bool IsDouble       ,
                         GetSinglePrecisionEncoding( Sd ) ,  // uint Fd             ,
                         GetDoublePrecisionEncoding( Dm ) ); // uint Fm             )

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FCVTDS( ZeligIR.Abstractions.RegisterDescriptor Dd ,
                                        ZeligIR.Abstractions.RegisterDescriptor Sm )
        {
            InstructionSet_VFP.Opcode_VFP_ConvertFloatToFloat enc = this.Encoder_VFP.PrepareForVFP_ConvertFloatToFloat;

            enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                         false                            ,  // bool IsDouble       ,
                         GetDoublePrecisionEncoding( Dd ) ,  // uint Fd             ,
                         GetSinglePrecisionEncoding( Sm ) ); // uint Fm             )

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FADD( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__BinaryOp( Fd, Fn, Fm, EncodingDefinition_VFP.c_binaryOperation_ADD );
        }

        public void EmitOpcode__FSUB( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__BinaryOp( Fd, Fn, Fm, EncodingDefinition_VFP.c_binaryOperation_SUB );
        }

        public void EmitOpcode__FMAC( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__BinaryOp( Fd, Fn, Fm, EncodingDefinition_VFP.c_binaryOperation_MAC );
        }

        public void EmitOpcode__FMUL( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__BinaryOp( Fd, Fn, Fm, EncodingDefinition_VFP.c_binaryOperation_MUL );
        }

        public void EmitOpcode__FDIV( ZeligIR.Abstractions.RegisterDescriptor Fd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fm )
        {
            EmitOpcode__BinaryOp( Fd, Fn, Fm, EncodingDefinition_VFP.c_binaryOperation_DIV );
        }

        private void EmitOpcode__BinaryOp( ZeligIR.Abstractions.RegisterDescriptor Fd  ,
                                           ZeligIR.Abstractions.RegisterDescriptor Fn  ,
                                           ZeligIR.Abstractions.RegisterDescriptor Fm  ,
                                           uint                                    Alu )
        {
            InstructionSet_VFP.Opcode_VFP_BinaryDataOperation enc = this.Encoder_VFP.PrepareForVFP_BinaryDataOperation;

            if(Fd.IsDoublePrecision)
            {
                enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                             true                             ,  // bool IsDouble       ,
                             GetDoublePrecisionEncoding( Fd ) ,  // uint Fd             ,
                             GetDoublePrecisionEncoding( Fn ) ,  // uint Fn             ,
                             GetDoublePrecisionEncoding( Fm ) ,  // uint Fm             ,
                             Alu                              ); // uint Alu            ,
            }
            else
            {
                enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                             false                            ,  // bool IsDouble       ,
                             GetSinglePrecisionEncoding( Fd ) ,  // uint Fd             ,
                             GetSinglePrecisionEncoding( Fn ) ,  // uint Fn             ,
                             GetSinglePrecisionEncoding( Fm ) ,  // uint Fm             ,
                             Alu                              ); // uint Alu            ,
            }

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FMRS( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn )
        {
            InstructionSet_VFP.Opcode_VFP_32bitRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_32bitRegisterTransfer;

            enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                         GetIntegerEncoding        ( Rd ) ,  // uint Rd             ,
                         GetSinglePrecisionEncoding( Fn ) ,  // uint Fn             ,
                         true                             ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FMSR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fn )
        {
            InstructionSet_VFP.Opcode_VFP_32bitRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_32bitRegisterTransfer;

            enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                         GetIntegerEncoding        ( Rd ),  // uint Rd             ,
                         GetSinglePrecisionEncoding( Fn ),  // uint Fn             ,
                         false                           ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FMRDL( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                       ZeligIR.Abstractions.RegisterDescriptor Fn )
        {
            InstructionSet_VFP.Opcode_VFP_32bitLoRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_32bitLoRegisterTransfer;

            enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                         GetIntegerEncoding        ( Rd ) ,  // uint Rd             ,
                         GetDoublePrecisionEncoding( Fn ) ,  // uint Fn             ,
                         true                             ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FMDLR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                       ZeligIR.Abstractions.RegisterDescriptor Fn )
        {
            InstructionSet_VFP.Opcode_VFP_32bitLoRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_32bitLoRegisterTransfer;

            enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                         GetIntegerEncoding        ( Rd ),  // uint Rd             ,
                         GetDoublePrecisionEncoding( Fn ),  // uint Fn             ,
                         false                           ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FMRDH( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                       ZeligIR.Abstractions.RegisterDescriptor Fn )
        {
            InstructionSet_VFP.Opcode_VFP_32bitHiRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_32bitHiRegisterTransfer;

            enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                         GetIntegerEncoding        ( Rd ) ,  // uint Rd             ,
                         GetDoublePrecisionEncoding( Fn ) ,  // uint Fn             ,
                         true                             ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FMDHR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                       ZeligIR.Abstractions.RegisterDescriptor Fn )
        {
            InstructionSet_VFP.Opcode_VFP_32bitHiRegisterTransfer enc = this.Encoder_VFP.PrepareForVFP_32bitHiRegisterTransfer;

            enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                         GetIntegerEncoding        ( Rd ),  // uint Rd             ,
                         GetDoublePrecisionEncoding( Fn ),  // uint Fn             ,
                         false                           ); // bool IsFromCoProc   )

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FLD( ZeligIR.Abstractions.RegisterDescriptor Fd  ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                     int                                     offset )
        {
            EmitOpcode__VFP_DataTransfer( Fd, Rd, offset, true );
        }

        public void EmitOpcode__FST( ZeligIR.Abstractions.RegisterDescriptor Fd     ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rd     ,
                                     int                                     offset )
        {
            EmitOpcode__VFP_DataTransfer( Fd, Rd, offset, false );
        }

        private void EmitOpcode__VFP_DataTransfer( ZeligIR.Abstractions.RegisterDescriptor Fd     ,
                                                   ZeligIR.Abstractions.RegisterDescriptor Rn     ,
                                                   int                                     offset ,
                                                   bool                                    fLoad  )
        {
            CHECKS.ASSERT( offset % sizeof(uint) == 0, "offset must be word-aligned, got {0}", offset );

            InstructionSet_VFP.Opcode_VFP_DataTransfer enc = this.Encoder_VFP.PrepareForVFP_DataTransfer;
            bool                                       fUp;
            uint                                       uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = ((uint)-offset) / sizeof(uint);
            }
            else
            {
                fUp     = true;
                uOffset = ((uint)offset) / sizeof(uint);
            }


            if(Fd.IsDoublePrecision)
            {
                enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                             true                             ,  // bool IsDouble       ,
                             GetIntegerEncoding        ( Rn ) ,  // uint Rn             ,
                             GetDoublePrecisionEncoding( Fd ) ,  // uint Fd             ,
                             fLoad                            ,  // bool IsLoad         ,
                             fUp                              ,  // bool Up             ,
                             uOffset                          ); // uint Offset         )
            }
            else
            {
                enc.Prepare( m_pendingCondition               ,  // uint ConditionCodes ,
                             false                            ,  // bool IsDouble       ,
                             GetIntegerEncoding        ( Rn ) ,  // uint Rn             ,
                             GetSinglePrecisionEncoding( Fd ) ,  // uint Fd             ,
                             fLoad                            ,  // bool IsLoad         ,
                             fUp                              ,  // bool Up             ,
                             uOffset                          ); // uint Offset         )
            }

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__FLDM( ZeligIR.Abstractions.RegisterDescriptor Rn         ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fd         ,
                                      uint                                    count      ,
                                      bool                                    fPreIndex  ,
                                      bool                                    fUp        ,
                                      bool                                    fWriteBack )
        {
            InstructionSet_VFP.Opcode_VFP_BlockDataTransfer enc = this.Encoder_VFP.PrepareForVFP_BlockDataTransfer;

            if(Fd.IsDoublePrecision)
            {
                enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                             true                            ,  // bool IsDouble       ,
                             GetIntegerEncoding        ( Rn ),  // uint Rn             ,
                             GetDoublePrecisionEncoding( Fd ),  // uint Fd             ,
                             true                            ,  // bool IsLoad         ,
                             fPreIndex                       ,  // bool PreIndex       ,
                             fUp                             ,  // bool Up             ,
                             fWriteBack                      ,  // bool WriteBack      ,
                             count                           ); // uint Count          )
            }
            else
            {
                enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                             false                           ,  // bool IsDouble       ,
                             GetIntegerEncoding        ( Rn ),  // uint Rn             ,
                             GetSinglePrecisionEncoding( Fd ),  // uint Fd             ,
                             true                            ,  // bool IsLoad         ,
                             fPreIndex                       ,  // bool PreIndex       ,
                             fUp                             ,  // bool Up             ,
                             fWriteBack                      ,  // bool WriteBack      ,
                             count                           ); // uint Count          )
            }
            
            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FLDMFD( ZeligIR.Abstractions.RegisterDescriptor Rn    ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fd    ,
                                        uint                                    count )
        {
            EmitOpcode__FLDM( Rn, Fd, count, false, true, true );
        }

        //--//

        public void EmitOpcode__FSTM( ZeligIR.Abstractions.RegisterDescriptor Rn         ,
                                      ZeligIR.Abstractions.RegisterDescriptor Fd         ,
                                      uint                                    count      ,
                                      bool                                    fPreIndex  ,
                                      bool                                    fUp        ,
                                      bool                                    fWriteBack )
        {
            InstructionSet_VFP.Opcode_VFP_BlockDataTransfer enc = this.Encoder_VFP.PrepareForVFP_BlockDataTransfer;

            if(Fd.IsDoublePrecision)
            {
                enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                             true                            ,  // bool IsDouble       ,
                             GetIntegerEncoding        ( Rn ),  // uint Rn             ,
                             GetDoublePrecisionEncoding( Fd ),  // uint Fd             ,
                             false                           ,  // bool IsLoad         ,
                             fPreIndex                       ,  // bool PreIndex       ,
                             fUp                             ,  // bool Up             ,
                             fWriteBack                      ,  // bool WriteBack      ,
                             count                           ); // uint Count          )
            }
            else
            {
                enc.Prepare( m_pendingCondition              ,  // uint ConditionCodes ,
                             false                           ,  // bool IsDouble       ,
                             GetIntegerEncoding        ( Rn ),  // uint Rn             ,
                             GetSinglePrecisionEncoding( Fd ),  // uint Fd             ,
                             false                           ,  // bool IsLoad         ,
                             fPreIndex                       ,  // bool PreIndex       ,
                             fUp                             ,  // bool Up             ,
                             fWriteBack                      ,  // bool WriteBack      ,
                             count                           ); // uint Count          )
            }
            
            EnqueueOpcode( enc );
        }

        public void EmitOpcode__FSTMFD( ZeligIR.Abstractions.RegisterDescriptor Rn    ,
                                        ZeligIR.Abstractions.RegisterDescriptor Fd    ,
                                        uint                                    count )
        {
            EmitOpcode__FSTM( Rn, Fd, count, true, false, true );
        }
    }
}
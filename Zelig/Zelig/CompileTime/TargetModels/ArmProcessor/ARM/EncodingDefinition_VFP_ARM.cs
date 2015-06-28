//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    public class EncodingDefinition_VFP_ARM : EncodingDefinition_VFP
    {

        //--//

        override public uint get_Rn( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_Rn( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }

        override public uint get_SysReg( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 16, 4 ); }
        override public uint set_SysReg( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 16, 4 ); }

        override public uint get_Fn( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 1, 5, 16, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 0, 5,  7, 1 ); }
        override public uint set_Fn( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 1, 5, 16, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 0, 5,  7, 1 ); }

        override public uint get_Rd( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 12, 4 ); }
        override public uint set_Rd( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 12, 4 ); }

        override public uint get_Fd( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 1, 5, 12, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 0, 5, 22, 1 ); }
        override public uint set_Fd( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 1, 5, 12, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 0, 5, 22, 1 ); }

        override public uint get_Fm( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 1, 5,  0, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 0, 5,  5, 1 ); }
        override public uint set_Fm( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 1, 5,  0, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 0, 5,  5, 1 ); }

        override public bool get_IsDouble( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 8 ); }
        override public uint set_IsDouble( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 8 ); }

        override public bool get_CheckNaN( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 7 ); }
        override public uint set_CheckNaN( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 7 ); }

        //--//

        override public uint get_BinaryOperation( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 3, 4, 23, 1 ) | EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 1, 4, 20, 2 ) | EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 0, 4, 6, 1 ); }
        override public uint set_BinaryOperation( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 3, 4, 23, 1 ) | EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 1, 4, 20, 2 ) | EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 0, 4, 6, 1 ); }

        //--//

        override public uint get_UnaryOperation( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 1, 5, 16, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op , 0, 5, 7, 1 ); }
        override public uint set_UnaryOperation( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 1, 5, 16, 4 ) | EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val, 0, 5, 7, 1 ); }

        //-//

        override public bool get_RegisterTransfer_IsFromCoproc( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 20 ); }
        override public uint set_RegisterTransfer_IsFromCoproc( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 20 ); }

        //--//

        override public bool get_DataTransfer_IsLoad         ( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 20 ); }
        override public uint set_DataTransfer_IsLoad         ( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 20 ); }

        override public bool get_DataTransfer_ShouldWriteBack( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 21 ); }
        override public uint set_DataTransfer_ShouldWriteBack( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 21 ); }

        override public bool get_DataTransfer_IsUp           ( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 23 ); }
        override public uint set_DataTransfer_IsUp           ( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 23 ); }

        override public bool get_DataTransfer_IsPreIndexing  ( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_CHECKFLAG( op , 24 ); }
        override public uint set_DataTransfer_IsPreIndexing  ( bool val ) { return EncodingDefinition_ARM.OPCODE_DECODE_SETFLAG  ( val, 24 ); }

        override public uint get_DataTransfer_Offset         ( uint op  ) { return EncodingDefinition_ARM.OPCODE_DECODE_EXTRACTFIELD( op ,  0, 8 ); }
        override public uint set_DataTransfer_Offset         ( uint val ) { return EncodingDefinition_ARM.OPCODE_DECODE_INSERTFIELD ( val,  0, 8 ); }
    }
}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.TargetModel.ArmProcessor
{
    public abstract class EncodingDefinition_VFP
    {
        //
        // +---------+-------+---+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | 3 3 2 2 | 2 2 2 | 2 | 2  | 2 | 2 2 | 1 1 1 1 | 1 1 1 1 | 1 1 9 8 | 7  | 6  | 5 | 4 | 3 2 1 0 |
        // | 1 0 9 8 | 7 6 5 | 4 | 3  | 2 | 1 0 | 9 8 7 6 | 5 4 3 2 | 1 0     |    |    |   |   |         |
        // +---------+-----------+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | Cond    | 1 1 1   0 | Op | D | Op  |   Fn    |   Fd    | 1 0 1 0 | N  | Op | M | 0 |   Fm    | Addressing Mode 1 - Single-precision vectors (binary)
        // +---------+-----------+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | Cond    | 1 1 1   0 | Op | 0 | Op  |   Dn    |   Dd    | 1 0 1 1 | 0  | Op | 0 | 0 |   Dm    | Addressing Mode 2 - Double-precision vectors (binary)
        // +---------+-----------+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | Cond    | 1 1 1   0   1  | D | 1 1 |   Op    |   Fd    | 1 0 1 0 | Op | 1  | M | 0 |   Fm    | Addressing Mode 3 - Single-precision vectors (unary)
        // +---------+----------------+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | Cond    | 1 1 1   0   1  | 0 | 1 1 |   Op    |   Dd    | 1 0 1 1 | Op | 1  | 0 | 0 |   Dm    | Addressing Mode 4 - Double-precision vectors (unary)
        // +---------+-------+---+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | Cond    | 1 1 0 | P | U  | D | W L |   Rn    |   Fd    | 1 0 1 0 |      Offset               | Addressing Mode 5 - VFP load/store multiple
        // +---------+-------+---+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        // | Cond    | 1 1 0 | P | U  | 0 | W L |   Rn    |   Dd    | 1 0 1 1 |      Offset               | Addressing Mode 5 - VFP load/store multiple
        // +---------+-------+---+----+---+-----+---------+---------+---------+----+----+---+---+---------+
        //
        //  31  28  27 26 25 24 23  22  21 20  19 18 17 16 15  12  11 10 9 8   7   6   5   4  3  0
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  0  1  U | 0 | 0  L |     Rn     |  Dd  | 1  0  1 1 |     offset         | FSTD/FLDD   Coprocessor Data Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  0  P  U | 0 | W  L |     Rn     |  Dd  | 1  0  1 1 |     offset         | FSTMD/FLDMD Coprocessor Data Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  0  0  0 | 1 | 0  Dr|     Rn     |  Rd  | 1  0  1 1 | 0 | 0 | 0 | 1 | Dm | FMDRR/FMRRD Coprocessor Data Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 0  Dr|     Dn     |  Rd  | 1  0  1 1 | 0 | 0 | 0 | 1 |  0 | FMDLR/FMRDL Coprocessor Register Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 1  Dr|     Dn     |  Rd  | 1  0  1 1 | 0 | 0 | 0 | 1 |  0 | FMDHR/FMRDH Coprocessor Register Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 0  0 |     Dn     |  Dd  | 1  0  1 1 | 0 | 0 | 0 | 0 | Dm | FMACD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 0  0 |     Dn     |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FNMACD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 0  1 |     Dn     |  Dd  | 1  0  1 1 | 0 | 0 | 0 | 0 | Dm | FMSCD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 0  1 |     Dn     |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FNMSCD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 1  0 |     Dn     |  Dd  | 1  0  1 1 | 0 | 0 | 0 | 0 | Dm | FMULD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 1  0 |     Dn     |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FNMULD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 1  1 |     Dn     |  Dd  | 1  0  1 1 | 0 | 0 | 0 | 0 | Dm | FADDD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 1  1 |     Dn     |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FSUBD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 0  0 |     Dn     |  Dd  | 1  0  1 1 | 0 | 0 | 0 | 0 | Dm | FDIVD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  0  0  0 |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FCPYD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  0  0  0 |  Dd  | 1  0  1 1 | 1 | 1 | 0 | 0 | Dm | FABSD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  0  0  1 |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FNEGD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  0  0  1 |  Dd  | 1  0  1 1 | 1 | 1 | 0 | 0 | Dm | FSQRTD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  1  0  0 |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 | Dm | FCMPD       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  1  0  0 |  Dd  | 1  0  1 1 | 1 | 1 | 0 | 0 | Dm | FCMPED      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  1  0  1 |  Dd  | 1  0  1 1 | 0 | 1 | 0 | 0 |  0 | FCMPZD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  1  0  1 |  Dd  | 1  0  1 1 | 1 | 1 | 0 | 0 |  0 | FCMPEZD     Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  1  1  1 |  Fd  | 1  0  1 1 | 1 | 1 | 0 | 0 | Dm | FCVTSD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 1  0  0  0 |  Dd  | 1  0  1 1 | 0 | 1 | M | 0 | Fm | FUITOD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 1  0  0  0 |  Dd  | 1  0  1 1 | 1 | 1 | M | 0 | Fm | FSITOD      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 1  1  0  0 |  Fd  | 1  0  1 1 | Z | 1 | 0 | 0 | Dm | FTOUID      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 1  1  0  1 |  Fd  | 1  0  1 1 | Z | 1 | 0 | 0 | Dm | FTOSID      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        //
        // ################################################################################
        // ################################################################################
        //
        //  31  28  27 26 25 24 23  22  21 20  19 18 17 16 15  12  11 10 9 8   7   6   5   4  3  0
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  0  1  U | D | 0  L |     Rn     |  Fd  | 1  0  1 0 |     offset         | FSTS/FLDS   Coprocessor Data Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  0  P  U | D | W  L |     Rn     |  Fd  | 1  0  1 0 |     offset         | FSTMS/FLDMS Coprocessor Data Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  0  0  0 | 1 | 0  Dr|     Rn     |  Rd  | 1  0  1 0 | 0 | 0 | M | 1 | Fm | FMSRR/FMRRS Coprocessor Data Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | 0 | 0  Dr|     Fn     |  Rd  | 1  0  1 0 | N | 0 | 0 | 1 |  0 | FMSR/FMRS   Coprocessor Register Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 1 | 1  Dr|     reg    |  Rd  | 1  0  1 0 | 0 | 0 | 0 | 1 |  0 | FMXR/FMRX   Coprocessor Register Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 1 | 1  1 | 0  0  0  1 | 1111 | 1  0  1 0 | 0 | 0 | 0 | 1 |  0 | FMSTAT      Coprocessor Register Transfer
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 0  0 |     Fn     |  Fd  | 1  0  1 0 | N | 0 | M | 0 | Fm | FMACS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 0  0 |     Fn     |  Fd  | 1  0  1 0 | N | 1 | M | 0 | Fm | FNMACS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 0  1 |     Fn     |  Fd  | 1  0  1 0 | N | 0 | M | 0 | Fm | FMSCS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 0  1 |     Fn     |  Fd  | 1  0  1 0 | N | 1 | M | 0 | Fm | FNMSCS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 1  0 |     Fn     |  Fd  | 1  0  1 0 | N | 0 | M | 0 | Fm | FMULS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 1  0 |     Fn     |  Fd  | 1  0  1 0 | N | 1 | M | 0 | Fm | FNMULS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 1  1 |     Fn     |  Fd  | 1  0  1 0 | N | 0 | M | 0 | Fm | FADDS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  0 | D | 1  1 |     Fn     |  Fd  | 1  0  1 0 | N | 1 | M | 0 | Fm | FSUBS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 0  0 |     Fn     |  Fd  | 1  0  1 0 | N | 0 | M | 0 | Fm | FDIVS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  0  0  0 |  Fd  | 1  0  1 0 | 0 | 1 | M | 0 | Fm | FCPYS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  0  0  0 |  Fd  | 1  0  1 0 | 1 | 1 | M | 0 | Fm | FABSS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  0  0  1 |  Fd  | 1  0  1 0 | 0 | 1 | M | 0 | Fm | FNEGS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  0  0  1 |  Fd  | 1  0  1 0 | 1 | 1 | M | 0 | Fm | FSQRTS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  1  0  0 |  Fd  | 1  0  1 0 | 0 | 1 | M | 0 | Fm | FCMPS       Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  1  0  0 |  Fd  | 1  0  1 0 | 1 | 1 | M | 0 | Fm | FCMPES      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  1  0  1 |  Fd  | 1  0  1 0 | 0 | 1 | 0 | 0 |  0 | FCMPZS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 0  1  0  1 |  Fd  | 1  0  1 0 | 1 | 1 | 0 | 0 |  0 | FCMPEZS     Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | 0 | 1  1 | 0  1  1  1 |  Dd  | 1  0  1 0 | 1 | 1 | M | 0 | Fm | FCVTDS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 1  0  0  0 |  Dd  | 1  0  1 0 | 0 | 1 | M | 0 | Fm | FUITOS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 1  0  0  0 |  Fd  | 1  0  1 0 | 1 | 1 | M | 0 | Fm | FSITOS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 1  1  0  0 |  Fd  | 1  0  1 0 | Z | 1 | M | 0 | Fm | FTOUIS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        // | cond | 1  1  1  0  1 | D | 1  1 | 1  1  0  1 |  Fd  | 1  0  1 0 | Z | 1 | M | 0 | Fm | FTOSIS      Coprocessor Data Operation
        // +------+---------------+---+------+------------+------+-----------+---+---+---+---+----+
        //
        //
        public const uint op_ConditionCodeTransfer   = 0x0EF1FA10; public const uint opmask_ConditionCodeTransfer   = 0x0FFFFFFF;
        public const uint op_SystemRegisterTransfer  = 0x0EE00A10; public const uint opmask_SystemRegisterTransfer  = 0x0FE00FFF;
        public const uint op_32bitLoRegisterTransfer = 0x0E000B10; public const uint opmask_32bitLoRegisterTransfer = 0x0FE00FFF;
        public const uint op_32bitHiRegisterTransfer = 0x0E200B10; public const uint opmask_32bitHiRegisterTransfer = 0x0FE00FFF;
        public const uint op_32bitRegisterTransfer   = 0x0E000A10; public const uint opmask_32bitRegisterTransfer   = 0x0FE00F7F;
        public const uint op_64bitRegisterTransfer   = 0x0C400A10; public const uint opmask_64bitRegisterTransfer   = 0x0FE00ED0;
        public const uint op_DataTransfer            = 0x0D000A00; public const uint opmask_DataTransfer            = 0x0F200E00;
        public const uint op_CompareToZero           = 0x0EB50A40; public const uint opmask_CompareToZero           = 0x0FBF0E7F;
        public const uint op_ConvertFloatToFloat     = 0x0EB70AC0; public const uint opmask_ConvertFloatToFloat     = 0x0FBF0ED0;
        public const uint op_UnaryDataOperation      = 0x0EB00A40; public const uint opmask_UnaryDataOperation      = 0x0FB00E50;
        public const uint op_BinaryDataOperation     = 0x0E000A00; public const uint opmask_BinaryDataOperation     = 0x0F000E10;
        public const uint op_BlockDataTransfer       = 0x0C000A00; public const uint opmask_BlockDataTransfer       = 0x0E000E00;

        //--//

        //////////////////////////////////////////////////////////////////////////
        public const uint c_register_s0    =  0                     + 32      ; //
        public const uint c_register_s1    =  1                     + 32      ; //
        public const uint c_register_s2    =  2                     + 32      ; //
        public const uint c_register_s3    =  3                     + 32      ; //
        public const uint c_register_s4    =  4                     + 32      ; //
        public const uint c_register_s5    =  5                     + 32      ; //
        public const uint c_register_s6    =  6                     + 32      ; //
        public const uint c_register_s7    =  7                     + 32      ; //
        public const uint c_register_s8    =  8                     + 32      ; //
        public const uint c_register_s9    =  9                     + 32      ; //
        public const uint c_register_s10   = 10                     + 32      ; //
        public const uint c_register_s11   = 11                     + 32      ; //
        public const uint c_register_s12   = 12                     + 32      ; //
        public const uint c_register_s13   = 13                     + 32      ; //
        public const uint c_register_s14   = 14                     + 32      ; //
        public const uint c_register_s15   = 15                     + 32      ; //
        public const uint c_register_s16   = 16                     + 32      ; //
        public const uint c_register_s17   = 17                     + 32      ; //
        public const uint c_register_s18   = 18                     + 32      ; //
        public const uint c_register_s19   = 19                     + 32      ; //
        public const uint c_register_s20   = 20                     + 32      ; //
        public const uint c_register_s21   = 21                     + 32      ; //
        public const uint c_register_s22   = 22                     + 32      ; //
        public const uint c_register_s23   = 23                     + 32      ; //
        public const uint c_register_s24   = 24                     + 32      ; //
        public const uint c_register_s25   = 25                     + 32      ; //
        public const uint c_register_s26   = 26                     + 32      ; //
        public const uint c_register_s27   = 27                     + 32      ; //
        public const uint c_register_s28   = 28                     + 32      ; //
        public const uint c_register_s29   = 29                     + 32      ; //
        public const uint c_register_s30   = 30                     + 32      ; //
        public const uint c_register_s31   = 31                     + 32      ; //
                                                                                //
        public const uint c_register_d0    =  0                     + 32+32   ; //
        public const uint c_register_d1    =  1                     + 32+32   ; //
        public const uint c_register_d2    =  2                     + 32+32   ; //
        public const uint c_register_d3    =  3                     + 32+32   ; //
        public const uint c_register_d4    =  4                     + 32+32   ; //
        public const uint c_register_d5    =  5                     + 32+32   ; //
        public const uint c_register_d6    =  6                     + 32+32   ; //
        public const uint c_register_d7    =  7                     + 32+32   ; //
        public const uint c_register_d8    =  8                     + 32+32   ; //
        public const uint c_register_d9    =  9                     + 32+32   ; //
        public const uint c_register_d10   = 10                     + 32+32   ; //
        public const uint c_register_d11   = 11                     + 32+32   ; //
        public const uint c_register_d12   = 12                     + 32+32   ; //
        public const uint c_register_d13   = 13                     + 32+32   ; //
        public const uint c_register_d14   = 14                     + 32+32   ; //
        public const uint c_register_d15   = 15                     + 32+32   ; //
                                                                                //
        public const uint c_register_FPSID = c_systemRegister_FPSID + 32+32+16; //
        public const uint c_register_FPSCR = c_systemRegister_FPSCR + 32+32+16; //
        public const uint c_register_FPEXC = c_systemRegister_FPEXC + 32+32+16; //
        //////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////
        public const uint c_systemRegister_FPSID = 0;
        public const uint c_systemRegister_FPSCR = 1;
        public const uint c_systemRegister_FPEXC = 8;
        ///////////////////////////////////////////

        ///////////////////////////////////////////
        public const uint c_binaryOperation_MAC  = 0x0; // Floating-point Multiply and Accumulate
        public const uint c_binaryOperation_NMAC = 0x1; // Floating-point Negated Multiply and Accumulate
        public const uint c_binaryOperation_MSC  = 0x2; // Floating-point Multiply and Subtract
        public const uint c_binaryOperation_NMSC = 0x3; // Floating-point Negated Multiply and Subtract
        public const uint c_binaryOperation_MUL  = 0x4; // Floating-point Multiply
        public const uint c_binaryOperation_NMUL = 0x5; // Floating-point Negated Multiply
        public const uint c_binaryOperation_ADD  = 0x6; // Floating-point Addition
        public const uint c_binaryOperation_SUB  = 0x7; // Floating-point Subtract
        public const uint c_binaryOperation_DIV  = 0x8; // Floating-point Divide
        ///////////////////////////////////////////

        //--//

        ///////////////////////////////////////////
        public const uint c_unaryOperation_CPY   = 0x00; // Floating-point Copy
        public const uint c_unaryOperation_ABS   = 0x01; // Floating-point Absolute Value
        public const uint c_unaryOperation_NEG   = 0x02; // Floating-point Negate
        public const uint c_unaryOperation_SQRT  = 0x03; // Floating-point Square Root
        public const uint c_unaryOperation_CMP   = 0x08; // Floating-point Compare
        public const uint c_unaryOperation_CMPE  = 0x09; // Floating-point Compare (NaN Exceptions)
        public const uint c_unaryOperation_UITO  = 0x10; // Convert Unsigned Integer to Floating-point
        public const uint c_unaryOperation_SITO  = 0x11; // Convert Signed Integer to Floating-point
        public const uint c_unaryOperation_TOUI  = 0x18; // Convert from Floating-point to Unsigned Integer
        public const uint c_unaryOperation_TOUIZ = 0x19; // Convert from Floating-point to Unsigned Integer, Round Towards Zero
        public const uint c_unaryOperation_TOSI  = 0x1A; // Convert from Floating-point to Signed Integer
        public const uint c_unaryOperation_TOSIZ = 0x1B; // Convert from Floating-point to Signed Integer, Round Towards Zero
        ///////////////////////////////////////////

        //--//

        ///////////////////////////////////////////
        public const int  c_fpexc_bit_EN = 30;
        public const int  c_fpexc_bit_EX = 31;

        public const uint c_fpexc_EN     = 1U << c_fpexc_bit_EN;
        public const uint c_fpexc_EX     = 1U << c_fpexc_bit_EX;
        ///////////////////////////////////////////

        ///////////////////////////////////////////
        public const int  c_fpscr_bit_IOC      =   0; // Invalid Operation (Cumulative exception bit)
        public const int  c_fpscr_bit_IOE      =   8; // Invalid Operation (Trap enable bit)
        public const int  c_fpscr_bit_DZC      =   1; // Division by Zero  (Cumulative exception bit)
        public const int  c_fpscr_bit_DZE      =   9; // Division by Zero  (Trap enable bit)
        public const int  c_fpscr_bit_OFC      =   2; // Overflow          (Cumulative exception bit)
        public const int  c_fpscr_bit_OFE      =  10; // Overflow          (Trap enable bit)
        public const int  c_fpscr_bit_UFC      =   3; // Underflow         (Cumulative exception bit)
        public const int  c_fpscr_bit_UFE      =  11; // Underflow         (Trap enable bit)
        public const int  c_fpscr_bit_IXC      =   4; // Inexact           (Cumulative exception bit)
        public const int  c_fpscr_bit_IXE      =  12; // Inexact           (Trap enable bit)
        public const int  c_fpscr_bit_IDC      =   7; // Input Denormal    (Cumulative exception bit)
        public const int  c_fpscr_bit_IDE      =  15; // Input Denormal    (Trap enable bit)
        public const int  c_fpscr_shift_LEN    =  16; // Vector length for VFP instructions
        public const int  c_fpscr_mask_LEN     = 0x7; // 
        public const int  c_fpscr_shift_STRIDE =  20; // Vector stride for VFP instructions
        public const int  c_fpscr_mask_STRIDE  = 0x3; // 
        public const int  c_fpscr_shift_RMODE  =  22; // Rounding mode control
        public const int  c_fpscr_mask_RMODE   = 0x3; // 
        public const int  c_fpscr_bit_FZ       =  24; // Flush-to-zero mode control
        public const int  c_fpscr_bit_DN       =  25; // Default NaN mode control
        public const int  c_fpscr_bit_V        =  28; // Is 1 if the comparison produced an unordered result.
        public const int  c_fpscr_bit_C        =  29; // Is 1 if the comparison produced an equal, greater than or unordered result
        public const int  c_fpscr_bit_Z        =  30; // Is 1 if the comparison produced an equal result
        public const int  c_fpscr_bit_N        =  31; // Is 1 if the comparison produced a less than result

        public const uint c_fpscr_IOC          = 1U << c_fpscr_bit_IOC;
        public const uint c_fpscr_IOE          = 1U << c_fpscr_bit_IOE;
        public const uint c_fpscr_DZC          = 1U << c_fpscr_bit_DZC;
        public const uint c_fpscr_DZE          = 1U << c_fpscr_bit_DZE;
        public const uint c_fpscr_OFC          = 1U << c_fpscr_bit_OFC;
        public const uint c_fpscr_OFE          = 1U << c_fpscr_bit_OFE;
        public const uint c_fpscr_UFC          = 1U << c_fpscr_bit_UFC;
        public const uint c_fpscr_UFE          = 1U << c_fpscr_bit_UFE;
        public const uint c_fpscr_IXC          = 1U << c_fpscr_bit_IXC;
        public const uint c_fpscr_IXE          = 1U << c_fpscr_bit_IXE;
        public const uint c_fpscr_IDC          = 1U << c_fpscr_bit_IDC;
        public const uint c_fpscr_IDE          = 1U << c_fpscr_bit_IDE;
        public const uint c_fpscr_FZ           = 1U << c_fpscr_bit_FZ;
        public const uint c_fpscr_DN           = 1U << c_fpscr_bit_DN;
        public const uint c_fpscr_V            = 1U << c_fpscr_bit_V;
        public const uint c_fpscr_C            = 1U << c_fpscr_bit_C;
        public const uint c_fpscr_Z            = 1U << c_fpscr_bit_Z;
        public const uint c_fpscr_N            = 1U << c_fpscr_bit_N;
        ///////////////////////////////////////////

        //--//

        abstract public uint get_Rn( uint op  );
        abstract public uint set_Rn( uint val );

        abstract public uint get_SysReg( uint op  );
        abstract public uint set_SysReg( uint val );

        abstract public uint get_Fn( uint op  );
        abstract public uint set_Fn( uint val );

        abstract public uint get_Rd( uint op  );
        abstract public uint set_Rd( uint val );

        abstract public uint get_Fd( uint op  );
        abstract public uint set_Fd( uint val );

        abstract public uint get_Fm( uint op  );
        abstract public uint set_Fm( uint val );

        abstract public bool get_IsDouble( uint op  );
        abstract public uint set_IsDouble( bool val );

        abstract public bool get_CheckNaN( uint op  );
        abstract public uint set_CheckNaN( bool val );

        //--//

        abstract public uint get_BinaryOperation( uint op  );
        abstract public uint set_BinaryOperation( uint val );

        //--//

        abstract public uint get_UnaryOperation( uint op  );
        abstract public uint set_UnaryOperation( uint val );

        //-//

        abstract public bool get_RegisterTransfer_IsFromCoproc( uint op  );
        abstract public uint set_RegisterTransfer_IsFromCoproc( bool val );

        //--//

        abstract public bool get_DataTransfer_IsLoad         ( uint op  );
        abstract public uint set_DataTransfer_IsLoad         ( bool val );

        abstract public bool get_DataTransfer_ShouldWriteBack( uint op  );
        abstract public uint set_DataTransfer_ShouldWriteBack( bool val );

        abstract public bool get_DataTransfer_IsUp           ( uint op  );
        abstract public uint set_DataTransfer_IsUp           ( bool val );

        abstract public bool get_DataTransfer_IsPreIndexing  ( uint op  );
        abstract public uint set_DataTransfer_IsPreIndexing  ( bool val );

        abstract public uint get_DataTransfer_Offset         ( uint op  );
        abstract public uint set_DataTransfer_Offset         ( uint val );
    }
}

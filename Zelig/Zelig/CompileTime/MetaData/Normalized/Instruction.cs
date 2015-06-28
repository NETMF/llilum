//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System;

    //
    // Don't remove "sealed" attribute unless you read comments on Equals method.
    //
    public sealed class Instruction : IMetaDataUnique
    {
        public enum Opcode : short
        {
            NOP             ,
            BREAK           ,
            LDARG_0         ,
            LDARG_1         ,
            LDARG_2         ,
            LDARG_3         ,
            LDLOC_0         ,
            LDLOC_1         ,
            LDLOC_2         ,
            LDLOC_3         ,
            STLOC_0         ,
            STLOC_1         ,
            STLOC_2         ,
            STLOC_3         ,
            LDARG_S         ,
            LDARGA_S        ,
            STARG_S         ,
            LDLOC_S         ,
            LDLOCA_S        ,
            STLOC_S         ,
            LDNULL          ,
            LDC_I4_M1       ,
            LDC_I4_0        ,
            LDC_I4_1        ,
            LDC_I4_2        ,
            LDC_I4_3        ,
            LDC_I4_4        ,
            LDC_I4_5        ,
            LDC_I4_6        ,
            LDC_I4_7        ,
            LDC_I4_8        ,
            LDC_I4_S        ,
            LDC_I4          ,
            LDC_I8          ,
            LDC_R4          ,
            LDC_R8          ,
            UNUSED49        ,
            DUP             ,
            POP             ,
            JMP             ,
            CALL            ,
            CALLI           ,
            RET             ,
            BR_S            ,
            BRFALSE_S       ,
            BRTRUE_S        ,
            BEQ_S           ,
            BGE_S           ,
            BGT_S           ,
            BLE_S           ,
            BLT_S           ,
            BNE_UN_S        ,
            BGE_UN_S        ,
            BGT_UN_S        ,
            BLE_UN_S        ,
            BLT_UN_S        ,
            BR              ,
            BRFALSE         ,
            BRTRUE          ,
            BEQ             ,
            BGE             ,
            BGT             ,
            BLE             ,
            BLT             ,
            BNE_UN          ,
            BGE_UN          ,
            BGT_UN          ,
            BLE_UN          ,
            BLT_UN          ,
            SWITCH          ,
            LDIND_I1        ,
            LDIND_U1        ,
            LDIND_I2        ,
            LDIND_U2        ,
            LDIND_I4        ,
            LDIND_U4        ,
            LDIND_I8        ,
            LDIND_I         ,
            LDIND_R4        ,
            LDIND_R8        ,
            LDIND_REF       ,
            STIND_REF       ,
            STIND_I1        ,
            STIND_I2        ,
            STIND_I4        ,
            STIND_I8        ,
            STIND_R4        ,
            STIND_R8        ,
            ADD             ,
            SUB             ,
            MUL             ,
            DIV             ,
            DIV_UN          ,
            REM             ,
            REM_UN          ,
            AND             ,
            OR              ,
            XOR             ,
            SHL             ,
            SHR             ,
            SHR_UN          ,
            NEG             ,
            NOT             ,
            CONV_I1         ,
            CONV_I2         ,
            CONV_I4         ,
            CONV_I8         ,
            CONV_R4         ,
            CONV_R8         ,
            CONV_U4         ,
            CONV_U8         ,
            CALLVIRT        ,
            CPOBJ           ,
            LDOBJ           ,
            LDSTR           ,
            NEWOBJ          ,
            CASTCLASS       ,
            ISINST          ,
            CONV_R_UN       ,
            UNUSED58        ,
            UNUSED1         ,
            UNBOX           ,
            THROW           ,
            LDFLD           ,
            LDFLDA          ,
            STFLD           ,
            LDSFLD          ,
            LDSFLDA         ,
            STSFLD          ,
            STOBJ           ,
            CONV_OVF_I1_UN  ,
            CONV_OVF_I2_UN  ,
            CONV_OVF_I4_UN  ,
            CONV_OVF_I8_UN  ,
            CONV_OVF_U1_UN  ,
            CONV_OVF_U2_UN  ,
            CONV_OVF_U4_UN  ,
            CONV_OVF_U8_UN  ,
            CONV_OVF_I_UN   ,
            CONV_OVF_U_UN   ,
            BOX             ,
            NEWARR          ,
            LDLEN           ,
            LDELEMA         ,
            LDELEM_I1       ,
            LDELEM_U1       ,
            LDELEM_I2       ,
            LDELEM_U2       ,
            LDELEM_I4       ,
            LDELEM_U4       ,
            LDELEM_I8       ,
            LDELEM_I        ,
            LDELEM_R4       ,
            LDELEM_R8       ,
            LDELEM_REF      ,
            STELEM_I        ,
            STELEM_I1       ,
            STELEM_I2       ,
            STELEM_I4       ,
            STELEM_I8       ,
            STELEM_R4       ,
            STELEM_R8       ,
            STELEM_REF      ,
            LDELEM          ,
            STELEM          ,
            UNBOX_ANY       ,
            UNUSED5         ,
            UNUSED6         ,
            UNUSED7         ,
            UNUSED8         ,
            UNUSED9         ,
            UNUSED10        ,
            UNUSED11        ,
            UNUSED12        ,
            UNUSED13        ,
            UNUSED14        ,
            UNUSED15        ,
            UNUSED16        ,
            UNUSED17        ,
            CONV_OVF_I1     ,
            CONV_OVF_U1     ,
            CONV_OVF_I2     ,
            CONV_OVF_U2     ,
            CONV_OVF_I4     ,
            CONV_OVF_U4     ,
            CONV_OVF_I8     ,
            CONV_OVF_U8     ,
            UNUSED50        ,
            UNUSED18        ,
            UNUSED19        ,
            UNUSED20        ,
            UNUSED21        ,
            UNUSED22        ,
            UNUSED23        ,
            REFANYVAL       ,
            CKFINITE        ,
            UNUSED24        ,
            UNUSED25        ,
            MKREFANY        ,
            UNUSED59        ,
            UNUSED60        ,
            UNUSED61        ,
            UNUSED62        ,
            UNUSED63        ,
            UNUSED64        ,
            UNUSED65        ,
            UNUSED66        ,
            UNUSED67        ,
            LDTOKEN         ,
            CONV_U2         ,
            CONV_U1         ,
            CONV_I          ,
            CONV_OVF_I      ,
            CONV_OVF_U      ,
            ADD_OVF         ,
            ADD_OVF_UN      ,
            MUL_OVF         ,
            MUL_OVF_UN      ,
            SUB_OVF         ,
            SUB_OVF_UN      ,
            ENDFINALLY      ,
            LEAVE           ,
            LEAVE_S         ,
            STIND_I         ,
            CONV_U          ,
            UNUSED26        ,
            UNUSED27        ,
            UNUSED28        ,
            UNUSED29        ,
            UNUSED30        ,
            UNUSED31        ,
            UNUSED32        ,
            UNUSED33        ,
            UNUSED34        ,
            UNUSED35        ,
            UNUSED36        ,
            UNUSED37        ,
            UNUSED38        ,
            UNUSED39        ,
            UNUSED40        ,
            UNUSED41        ,
            UNUSED42        ,
            UNUSED43        ,
            UNUSED44        ,
            UNUSED45        ,
            UNUSED46        ,
            UNUSED47        ,
            UNUSED48        ,
            PREFIX7         ,
            PREFIX6         ,
            PREFIX5         ,
            PREFIX4         ,
            PREFIX3         ,
            PREFIX2         ,
            PREFIX1         ,
            PREFIXREF       ,
            ARGLIST         ,
            CEQ             ,
            CGT             ,
            CGT_UN          ,
            CLT             ,
            CLT_UN          ,
            LDFTN           ,
            LDVIRTFTN       ,
            UNUSED56        ,
            LDARG           ,
            LDARGA          ,
            STARG           ,
            LDLOC           ,
            LDLOCA          ,
            STLOC           ,
            LOCALLOC        ,
            UNUSED57        ,
            ENDFILTER       ,
            UNALIGNED       ,
            VOLATILE        ,
            TAILCALL        ,
            INITOBJ         ,
            CONSTRAINED     ,
            CPBLK           ,
            INITBLK         ,
            NO              ,
            RETHROW         ,
            UNUSED51        ,
            SIZEOF          ,
            REFANYTYPE      ,
            READONLY        ,
            UNUSED53        ,
            UNUSED54        ,
            UNUSED55        ,
            UNUSED70        ,
            ILLEGAL         ,
            MACRO_END       ,
            COUNT
        }

        public enum OpcodeOperand
        {
            None    = 0x00000000,
            Var     = 0x00000001,
            Int     = 0x00000002,
            Float   = 0x00000003,
            Branch  = 0x00000004,
            Method  = 0x00000005,
            Field   = 0x00000006,
            Type    = 0x00000007,
            String  = 0x00000008,
            Sig     = 0x00000009,
            Token   = 0x0000000A,
            Switch  = 0x0000000B,
            Illegal = 0x0000000F,
        }

        public enum OpcodeSize
        {
            None        = 0x00000000,
            Byte        = 0x00000010,
            Short       = 0x00000020,
            Word        = 0x00000030,
            DWord       = 0x00000040,
            Variable    = 0x00000050,
            Implicit_M1 = 0x00000060,
            Implicit_0  = 0x00000070,
            Implicit_1  = 0x00000080,
            Implicit_2  = 0x00000090,
            Implicit_3  = 0x000000A0,
            Implicit_4  = 0x000000B0,
            Implicit_5  = 0x000000C0,
            Implicit_6  = 0x000000D0,
            Implicit_7  = 0x000000E0,
            Implicit_8  = 0x000000F0,
        }

        public enum OpcodeFlowControl
        {
            None                 = 0x00000000,
            ConditionalControl   = 0x00000100,
            UnconditionalControl = 0x00000200,
            ExceptionHandling    = 0x00000300,
        }

        public enum OpcodeAction
        {
            Invalid            = 0x00000000,
            Load               = 0x00001000, // OpcodeActionTarget.*
            LoadAddress        = 0x00002000, // OpcodeActionTarget.*
            LoadIndirect       = 0x00003000, // OpcodeActionType.*
            LoadElement        = 0x00004000, // OpcodeActionType.*
            LoadElementAddress = 0x00005000, // OpcodeActionType.*
            Store              = 0x00006000, // OpcodeActionTarget.*
            StoreIndirect      = 0x00007000, // OpcodeActionType.*
            StoreElement       = 0x00008000, // OpcodeActionType.*
            Stack              = 0x00009000, // OpcodeActionStack.*
            Jump               = 0x0000A000,
            Call               = 0x0000B000, // OpcodeActionCall.*
            Return             = 0x0000C000,
            Branch             = 0x0000D000, // OpcodeActionCond.*              Unsigned?
            ALU                = 0x0000E000, // OpcodeActionALU.*               Unsigned? Overflow?
            Convert            = 0x0000F000, // OpcodeActionType.*              Unsigned? Overflow?
            Object             = 0x00010000, // OpcodeActionObject.*
            TypedRef           = 0x00011000, // OpcodeActionTypedRef.*
            EH                 = 0x00012000, // OpcodeActionExceptionHandling.*
            Set                = 0x00013000, // OpcodeActionCond.*              Unsigned?
            Modifier           = 0x00014000, // OpcodeActionModifier.*
            Debug              = 0x00015000, // OpcodeActionDebug.*
        }

        public enum OpcodeActionDebug
        {
            Breakpoint = 0x00000000,
        }

        public enum OpcodeActionTarget
        {
            Local       = 0x00000000,
            Argument    = 0x00100000,
            Null        = 0x00200000,
            Constant    = 0x00300000,
            Field       = 0x00400000,
            StaticField = 0x00500000,
            Token       = 0x00600000,
            String      = 0x00700000,
        }

        public enum OpcodeActionStack
        {
            Dup      = 0x00000000,
            Pop      = 0x00100000,
            LocAlloc = 0x00200000,
            ArgList  = 0x00300000,
        }

        public enum OpcodeActionCall
        {
            Direct   = 0x00000000,
            Indirect = 0x00100000,
            Virtual  = 0x00200000,
        }

        public enum OpcodeActionObject
        {
            New       = 0x00000000,
            NewArr    = 0x00100000,
            LdLen     = 0x00200000,
            Cast      = 0x00300000,
            IsInst    = 0x00400000,
            Box       = 0x00500000,
            Unbox     = 0x00600000,
            UnboxAny  = 0x00700000,
            LdFtn     = 0x00800000,
            LdVirtFtn = 0x00900000,
            InitObj   = 0x00A00000,
            LdObj     = 0x00B00000,
            CpObj     = 0x00C00000,
            StObj     = 0x00D00000,
            InitBlk   = 0x00E00000,
            CpBlk     = 0x00F00000,
            SizeOf    = 0x01000000,
        }

        public enum OpcodeActionTypedReference
        {
            RefAnyVal  = 0x00000000,
            MkRefAny   = 0x00100000,
            RefAnyType = 0x00200000,
        }

        public enum OpcodeActionExceptionHandling
        {
            Throw      = 0x00000000,
            EndFinally = 0x00100000,
            Leave      = 0x00200000,
            EndFilter  = 0x00300000,
            ReThrow    = 0x00400000,
        }

        public enum OpcodeActionCondition
        {
            ALWAYS = 0x00000000,
            FALSE  = 0x00100000,
            TRUE   = 0x00200000,
            EQ     = 0x00300000,
            GE     = 0x00400000,
            GT     = 0x00500000,
            LE     = 0x00600000,
            LT     = 0x00700000,
            NE     = 0x00800000,
            Val    = 0x00900000,
        }

        public enum OpcodeActionALU
        {
            ADD    = 0x00000000,
            SUB    = 0x00100000,
            MUL    = 0x00200000,
            DIV    = 0x00300000,
            REM    = 0x00400000,
            AND    = 0x00500000,
            OR     = 0x00600000,
            XOR    = 0x00700000,
            SHL    = 0x00800000,
            SHR    = 0x00900000,
            NEG    = 0x00A00000,
            NOT    = 0x00B00000,
            NOP    = 0x00C00000,
            FINITE = 0x00D00000,
        }

        public enum OpcodeActionModifier
        {
            Unaligned   = 0x00000000,
            Volatile    = 0x00100000,
            TailCall    = 0x00200000,
            Constrained = 0x00300000,
            NoCheck     = 0x00400000,
            Readonly    = 0x00500000,
        }

        public enum OpcodeActionType
        {
            I1        = 0x00000000,
            U1        = 0x00100000,
            I2        = 0x00200000,
            U2        = 0x00300000,
            I4        = 0x00400000,
            U4        = 0x00500000,
            I8        = 0x00600000,
            U8        = 0x00700000,
            I         = 0x00800000,
            U         = 0x00900000,
            R4        = 0x00A00000,
            R8        = 0x00B00000,
            R         = 0x00C00000,
            String    = 0x00D00000,
            Reference = 0x00E00000,
            Token     = 0x00F00000,
        }


        [Flags]
        internal enum Format
        {
            None                       = OpcodeOperand.None   ,
            Var                        = OpcodeOperand.Var    ,
            Int                        = OpcodeOperand.Int    ,
            Float                      = OpcodeOperand.Float  ,
            Branch                     = OpcodeOperand.Branch ,
            Method                     = OpcodeOperand.Method ,
            Field                      = OpcodeOperand.Field  ,
            Type                       = OpcodeOperand.Type   ,
            String                     = OpcodeOperand.String ,
            Sig                        = OpcodeOperand.Sig    ,
            Token                      = OpcodeOperand.Token  ,
            Switch                     = OpcodeOperand.Switch ,
            Illegal                    = OpcodeOperand.Illegal,
            Operand_MASK               = 0x0000000F,

            SizeNone                   = OpcodeSize.None       ,
            SizeByte                   = OpcodeSize.Byte       ,
            SizeShort                  = OpcodeSize.Short      ,
            SizeWord                   = OpcodeSize.Word       ,
            SizeDWord                  = OpcodeSize.DWord      ,
            SizeVariable               = OpcodeSize.Variable   ,
            SizeImplicit_M1            = OpcodeSize.Implicit_M1,
            SizeImplicit_0             = OpcodeSize.Implicit_0 ,
            SizeImplicit_1             = OpcodeSize.Implicit_1 ,
            SizeImplicit_2             = OpcodeSize.Implicit_2 ,
            SizeImplicit_3             = OpcodeSize.Implicit_3 ,
            SizeImplicit_4             = OpcodeSize.Implicit_4 ,
            SizeImplicit_5             = OpcodeSize.Implicit_5 ,
            SizeImplicit_6             = OpcodeSize.Implicit_6 ,
            SizeImplicit_7             = OpcodeSize.Implicit_7 ,
            SizeImplicit_8             = OpcodeSize.Implicit_8 ,
            Size_MASK                  = 0x000000F0,

            ConditionalControl         = OpcodeFlowControl.ConditionalControl  ,
            UnconditionalControl       = OpcodeFlowControl.UnconditionalControl,
            ExceptionHandling          = OpcodeFlowControl.ExceptionHandling   ,
            FlowControl_MASK           = 0x00000300,

            //--//

            Unused_0x00000400          = 0x00000400,
            Unused_0x00000800          = 0x00000800,

            Action_Invalid             = OpcodeAction.Invalid            ,
            Action_Load                = OpcodeAction.Load               ,
            Action_LoadAddress         = OpcodeAction.LoadAddress        ,
            Action_LoadIndirect        = OpcodeAction.LoadIndirect       ,
            Action_LoadElement         = OpcodeAction.LoadElement        ,
            Action_LoadElementAddress  = OpcodeAction.LoadElementAddress ,
            Action_Store               = OpcodeAction.Store              ,
            Action_StoreIndirect       = OpcodeAction.StoreIndirect      ,
            Action_StoreElement        = OpcodeAction.StoreElement       ,
            Action_Stack               = OpcodeAction.Stack              ,
            Action_Jump                = OpcodeAction.Jump               ,
            Action_Call                = OpcodeAction.Call               ,
            Action_Return              = OpcodeAction.Return             ,
            Action_Branch              = OpcodeAction.Branch             ,
            Action_ALU                 = OpcodeAction.ALU                ,
            Action_Convert             = OpcodeAction.Convert            ,
            Action_Object              = OpcodeAction.Object             ,
            Action_TypedRef            = OpcodeAction.TypedRef           ,
            Action_EH                  = OpcodeAction.EH                 ,
            Action_Set                 = OpcodeAction.Set                ,
            Action_Modifier            = OpcodeAction.Modifier           ,
            Action_Debug               = OpcodeAction.Debug              ,
            Action_MASK                = 0x0001F000,

            Unused_0x00020000          = 0x00020000,
            Unused_0x00040000          = 0x00040000,
            Unused_0x00080000          = 0x00080000,

            //--//

            Debug_Breakpoint           = OpcodeActionDebug.Breakpoint,
            Debug_MASK                 = 0x01F00000,

            Target_Local               = OpcodeActionTarget.Local      ,
            Target_Argument            = OpcodeActionTarget.Argument   ,
            Target_Null                = OpcodeActionTarget.Null       ,
            Target_Constant            = OpcodeActionTarget.Constant   ,
            Target_Field               = OpcodeActionTarget.Field      ,
            Target_StaticField         = OpcodeActionTarget.StaticField,
            Target_Token               = OpcodeActionTarget.Token      ,
            Target_String              = OpcodeActionTarget.String     ,
            Target_MASK                = 0x01F00000,

            Stack_Dup                  = OpcodeActionStack.Dup     ,
            Stack_Pop                  = OpcodeActionStack.Pop     ,
            Stack_LocAlloc             = OpcodeActionStack.LocAlloc,
            Stack_ArgList              = OpcodeActionStack.ArgList ,
            Stack_MASK                 = 0x01F00000,

            Call_Direct                = OpcodeActionCall.Direct  ,
            Call_Indirect              = OpcodeActionCall.Indirect,
            Call_Virtual               = OpcodeActionCall.Virtual ,
            Call_MASK                  = 0x01F00000,

            Obj_New                    = OpcodeActionObject.New      ,
            Obj_NewArr                 = OpcodeActionObject.NewArr   ,
            Obj_LdLen                  = OpcodeActionObject.LdLen    ,
            Obj_Cast                   = OpcodeActionObject.Cast     ,
            Obj_IsInst                 = OpcodeActionObject.IsInst   ,
            Obj_Box                    = OpcodeActionObject.Box      ,
            Obj_Unbox                  = OpcodeActionObject.Unbox    ,
            Obj_UnboxAny               = OpcodeActionObject.UnboxAny ,
            Obj_LdFtn                  = OpcodeActionObject.LdFtn    ,
            Obj_LdVirtFtn              = OpcodeActionObject.LdVirtFtn,
            Obj_InitObj                = OpcodeActionObject.InitObj  ,
            Obj_LdObj                  = OpcodeActionObject.LdObj    ,
            Obj_CpObj                  = OpcodeActionObject.CpObj    ,
            Obj_StObj                  = OpcodeActionObject.StObj    ,
            Obj_InitBlk                = OpcodeActionObject.InitBlk  ,
            Obj_CpBlk                  = OpcodeActionObject.CpBlk    ,
            Obj_SizeOf                 = OpcodeActionObject.SizeOf   ,
            Obj_MASK                   = 0x01F00000,

            TypedRef_RefAnyVal         = OpcodeActionTypedReference.RefAnyVal ,
            TypedRef_MkRefAny          = OpcodeActionTypedReference.MkRefAny  ,
            TypedRef_RefAnyType        = OpcodeActionTypedReference.RefAnyType,
            TypedRef_MASK              = 0x01F00000,

            EH_Throw                   = OpcodeActionExceptionHandling.Throw     ,
            EH_EndFinally              = OpcodeActionExceptionHandling.EndFinally,
            EH_Leave                   = OpcodeActionExceptionHandling.Leave     ,
            EH_EndFilter               = OpcodeActionExceptionHandling.EndFilter ,
            EH_ReThrow                 = OpcodeActionExceptionHandling.ReThrow   ,
            EH_MASK                    = 0x01F00000,

            Cond_ALWAYS                = OpcodeActionCondition.ALWAYS,
            Cond_FALSE                 = OpcodeActionCondition.FALSE ,
            Cond_TRUE                  = OpcodeActionCondition.TRUE  ,
            Cond_EQ                    = OpcodeActionCondition.EQ    ,
            Cond_GE                    = OpcodeActionCondition.GE    ,
            Cond_GT                    = OpcodeActionCondition.GT    ,
            Cond_LE                    = OpcodeActionCondition.LE    ,
            Cond_LT                    = OpcodeActionCondition.LT    ,
            Cond_NE                    = OpcodeActionCondition.NE    ,
            Cond_Val                   = OpcodeActionCondition.Val   ,
            Cond_MASK                  = 0x01F00000,

            Op_ADD                     = OpcodeActionALU.ADD   ,
            Op_SUB                     = OpcodeActionALU.SUB   ,
            Op_MUL                     = OpcodeActionALU.MUL   ,
            Op_DIV                     = OpcodeActionALU.DIV   ,
            Op_REM                     = OpcodeActionALU.REM   ,
            Op_AND                     = OpcodeActionALU.AND   ,
            Op_OR                      = OpcodeActionALU.OR    ,
            Op_XOR                     = OpcodeActionALU.XOR   ,
            Op_SHL                     = OpcodeActionALU.SHL   ,
            Op_SHR                     = OpcodeActionALU.SHR   ,
            Op_NEG                     = OpcodeActionALU.NEG   ,
            Op_NOT                     = OpcodeActionALU.NOT   ,
            Op_NOP                     = OpcodeActionALU.NOP   ,
            Op_FINITE                  = OpcodeActionALU.FINITE,
            Op_MASK                    = 0x01F00000,

            Mod_Unaligned              = OpcodeActionModifier.Unaligned  ,
            Mod_Volatile               = OpcodeActionModifier.Volatile   ,
            Mod_TailCall               = OpcodeActionModifier.TailCall   ,
            Mod_Constrained            = OpcodeActionModifier.Constrained,
            Mod_NoCheck                = OpcodeActionModifier.NoCheck    ,
            Mod_Readonly               = OpcodeActionModifier.Readonly   ,
            Mod_MASK                   = 0x01F00000,

            Type_I1                    = OpcodeActionType.I1       ,
            Type_U1                    = OpcodeActionType.U1       ,
            Type_I2                    = OpcodeActionType.I2       ,
            Type_U2                    = OpcodeActionType.U2       ,
            Type_I4                    = OpcodeActionType.I4       ,
            Type_U4                    = OpcodeActionType.U4       ,
            Type_I8                    = OpcodeActionType.I8       ,
            Type_U8                    = OpcodeActionType.U8       ,
            Type_I                     = OpcodeActionType.I        ,
            Type_U                     = OpcodeActionType.U        ,
            Type_R4                    = OpcodeActionType.R4       ,
            Type_R8                    = OpcodeActionType.R8       ,
            Type_R                     = OpcodeActionType.R        ,
            Type_String                = OpcodeActionType.String   ,
            Type_Reference             = OpcodeActionType.Reference,
            Type_Token                 = OpcodeActionType.Token    ,
            Type_MASK                  = 0x01F00000,

            Unsigned                   = 0x02000000,
            Overflow                   = 0x04000000,

            Unused_0x08000000          = 0x08000000,
            Unused_0x10000000          = 0x10000000,
            Unused_0x20000000          = 0x20000000,
            Unused_0x40000000          = 0x40000000,
          //Unused_0x80000000          = 0x80000000,

            //--//

            //
            // Bytecode decoding.
            //

            NOP             = Action_ALU                | Op_NOP                                  ,
            BREAK           = Action_Debug              | Debug_Breakpoint                        ,
            LDARGA          = Action_LoadAddress        | Target_Argument                         ,
            LDARG           = Action_Load               | Target_Argument                         ,
            STARG           = Action_Store              | Target_Argument                         ,
            LDLOCA          = Action_LoadAddress        | Target_Local                            ,
            LDLOC           = Action_Load               | Target_Local                            ,
            STLOC           = Action_Store              | Target_Local                            ,
            LDNULL          = Action_Load               | Target_Null                             ,
            LDC             = Action_Load               | Target_Constant                         ,
            DUP             = Action_Stack              | Stack_Dup                               ,
            POP             = Action_Stack              | Stack_Pop                               ,
            JMP             = Action_Jump                                                         ,
            CALL            = Action_Call               | Call_Direct                             ,
            CALLI           = Action_Call               | Call_Indirect                           ,
            RET             = Action_Return                                                       ,
            BR              = Action_Branch             | Cond_ALWAYS                             ,
            BRFALSE         = Action_Branch             | Cond_FALSE                              ,
            BRTRUE          = Action_Branch             | Cond_TRUE                               ,
            BEQ             = Action_Branch             | Cond_EQ                                 ,
            BGE             = Action_Branch             | Cond_GE                                 ,
            BGT             = Action_Branch             | Cond_GT                                 ,
            BLE             = Action_Branch             | Cond_LE                                 ,
            BLT             = Action_Branch             | Cond_LT                                 ,
            BNE_UN          = Action_Branch             | Cond_NE            | Unsigned           ,
            BGE_UN          = Action_Branch             | Cond_GE            | Unsigned           ,
            BGT_UN          = Action_Branch             | Cond_GT            | Unsigned           ,
            BLE_UN          = Action_Branch             | Cond_LE            | Unsigned           ,
            BLT_UN          = Action_Branch             | Cond_LT            | Unsigned           ,
            SWITCH          = Action_Branch             | Cond_Val                                ,
            LDIND_I1        = Action_LoadIndirect       | Type_I1                                 ,
            LDIND_U1        = Action_LoadIndirect       | Type_U1                                 ,
            LDIND_I2        = Action_LoadIndirect       | Type_I2                                 ,
            LDIND_U2        = Action_LoadIndirect       | Type_U2                                 ,
            LDIND_I4        = Action_LoadIndirect       | Type_I4                                 ,
            LDIND_U4        = Action_LoadIndirect       | Type_U4                                 ,
            LDIND_I8        = Action_LoadIndirect       | Type_I8                                 ,
            LDIND_I         = Action_LoadIndirect       | Type_I                                  ,
            LDIND_R4        = Action_LoadIndirect       | Type_R4                                 ,
            LDIND_R8        = Action_LoadIndirect       | Type_R8                                 ,
            LDIND_REF       = Action_LoadIndirect       | Type_Reference                          ,
            STIND_REF       = Action_StoreIndirect      | Type_Reference                          ,
            STIND_I1        = Action_StoreIndirect      | Type_I1                                 ,
            STIND_I2        = Action_StoreIndirect      | Type_I2                                 ,
            STIND_I4        = Action_StoreIndirect      | Type_I4                                 ,
            STIND_I8        = Action_StoreIndirect      | Type_I8                                 ,
            STIND_R4        = Action_StoreIndirect      | Type_R4                                 ,
            STIND_R8        = Action_StoreIndirect      | Type_R8                                 ,
            ADD             = Action_ALU                | Op_ADD                                  ,
            SUB             = Action_ALU                | Op_SUB                                  ,
            MUL             = Action_ALU                | Op_MUL                                  ,
            DIV             = Action_ALU                | Op_DIV                                  ,
            DIV_UN          = Action_ALU                | Op_DIV             | Unsigned           ,
            REM             = Action_ALU                | Op_REM                                  ,
            REM_UN          = Action_ALU                | Op_REM             | Unsigned           ,
            AND             = Action_ALU                | Op_AND                                  ,
            OR              = Action_ALU                | Op_OR                                   ,
            XOR             = Action_ALU                | Op_XOR                                  ,
            SHL             = Action_ALU                | Op_SHL                                  ,
            SHR             = Action_ALU                | Op_SHR                                  ,
            SHR_UN          = Action_ALU                | Op_SHR             | Unsigned           ,
            NEG             = Action_ALU                | Op_NEG                                  ,
            NOT             = Action_ALU                | Op_NOT                                  ,
            CONV_I1         = Action_Convert            | Type_I1                                 ,
            CONV_I2         = Action_Convert            | Type_I2                                 ,
            CONV_I4         = Action_Convert            | Type_I4                                 ,
            CONV_I8         = Action_Convert            | Type_I8                                 ,
            CONV_R4         = Action_Convert            | Type_R4                                 ,
            CONV_R8         = Action_Convert            | Type_R8                                 ,
            CONV_U4         = Action_Convert            | Type_U4                                 ,
            CONV_U8         = Action_Convert            | Type_U8                                 ,
            CALLVIRT        = Action_Call               | Call_Virtual                            ,
            CPOBJ           = Action_Object             | Obj_CpObj                               ,
            LDOBJ           = Action_Object             | Obj_LdObj                               ,
            LDSTR           = Action_Load               | Target_String                           ,
            NEWOBJ          = Action_Object             | Obj_New                                 ,
            CASTCLASS       = Action_Object             | Obj_Cast                                ,
            ISINST          = Action_Object             | Obj_IsInst                              ,
            CONV_R_UN       = Action_Convert            | Type_R             | Unsigned           ,
            UNBOX           = Action_Object             | Obj_Unbox                               ,
            THROW           = Action_EH                 | EH_Throw                                ,
            LDFLD           = Action_Load               | Target_Field                            ,
            LDFLDA          = Action_LoadAddress        | Target_Field                            ,
            STFLD           = Action_Store              | Target_Field                            ,
            LDSFLD          = Action_Load               | Target_StaticField                      ,
            LDSFLDA         = Action_LoadAddress        | Target_StaticField                      ,
            STSFLD          = Action_Store              | Target_StaticField                      ,
            STOBJ           = Action_Object             | Obj_StObj                               ,
            CONV_OVF_I1_UN  = Action_Convert            | Type_I1            | Unsigned | Overflow,
            CONV_OVF_I2_UN  = Action_Convert            | Type_I2            | Unsigned | Overflow,
            CONV_OVF_I4_UN  = Action_Convert            | Type_I4            | Unsigned | Overflow,
            CONV_OVF_I8_UN  = Action_Convert            | Type_I8            | Unsigned | Overflow,
            CONV_OVF_U1_UN  = Action_Convert            | Type_R4            | Unsigned | Overflow,
            CONV_OVF_U2_UN  = Action_Convert            | Type_R8            | Unsigned | Overflow,
            CONV_OVF_U4_UN  = Action_Convert            | Type_U4            | Unsigned | Overflow,
            CONV_OVF_U8_UN  = Action_Convert            | Type_U8            | Unsigned | Overflow,
            CONV_OVF_I_UN   = Action_Convert            | Type_I             | Unsigned | Overflow,
            CONV_OVF_U_UN   = Action_Convert            | Type_U             | Unsigned | Overflow,
            BOX             = Action_Object             | Obj_Box                                 ,
            NEWARR          = Action_Object             | Obj_NewArr                              ,
            LDLEN           = Action_Object             | Obj_LdLen                               ,
            LDELEMA         = Action_LoadElementAddress | Type_Token                              ,
            LDELEM_I1       = Action_LoadElement        | Type_I1                                 ,
            LDELEM_U1       = Action_LoadElement        | Type_U1                                 ,
            LDELEM_I2       = Action_LoadElement        | Type_I2                                 ,
            LDELEM_U2       = Action_LoadElement        | Type_U2                                 ,
            LDELEM_I4       = Action_LoadElement        | Type_I4                                 ,
            LDELEM_U4       = Action_LoadElement        | Type_U4                                 ,
            LDELEM_I8       = Action_LoadElement        | Type_I8                                 ,
            LDELEM_I        = Action_LoadElement        | Type_I                                  ,
            LDELEM_R4       = Action_LoadElement        | Type_R4                                 ,
            LDELEM_R8       = Action_LoadElement        | Type_R8                                 ,
            LDELEM_REF      = Action_LoadElement        | Type_Reference                          ,
            STELEM_I        = Action_StoreElement       | Type_I                                  ,
            STELEM_I1       = Action_StoreElement       | Type_I1                                 ,
            STELEM_I2       = Action_StoreElement       | Type_I2                                 ,
            STELEM_I4       = Action_StoreElement       | Type_I4                                 ,
            STELEM_I8       = Action_StoreElement       | Type_I8                                 ,
            STELEM_R4       = Action_StoreElement       | Type_R4                                 ,
            STELEM_R8       = Action_StoreElement       | Type_R8                                 ,
            STELEM_REF      = Action_StoreElement       | Type_Reference                          ,
            LDELEM          = Action_LoadElement        | Type_Token                              ,
            STELEM          = Action_StoreElement       | Type_Token                              ,
            UNBOX_ANY       = Action_Object             | Obj_UnboxAny                            ,
            CONV_OVF_I1     = Action_Convert            | Type_I1                       | Overflow,
            CONV_OVF_U1     = Action_Convert            | Type_U1                       | Overflow,
            CONV_OVF_I2     = Action_Convert            | Type_I2                       | Overflow,
            CONV_OVF_U2     = Action_Convert            | Type_U2                       | Overflow,
            CONV_OVF_I4     = Action_Convert            | Type_I4                       | Overflow,
            CONV_OVF_U4     = Action_Convert            | Type_U4                       | Overflow,
            CONV_OVF_I8     = Action_Convert            | Type_I8                       | Overflow,
            CONV_OVF_U8     = Action_Convert            | Type_U8                       | Overflow,
            REFANYVAL       = Action_TypedRef           | TypedRef_RefAnyVal                      ,
            CKFINITE        = Action_ALU                | Op_FINITE                               ,
            MKREFANY        = Action_TypedRef           | TypedRef_MkRefAny                       ,
            LDTOKEN         = Action_Load               | Target_Token                            ,
            CONV_U2         = Action_Convert            | Type_U2                                 ,
            CONV_U1         = Action_Convert            | Type_U1                                 ,
            CONV_I          = Action_Convert            | Type_I                                  ,
            CONV_OVF_I      = Action_Convert            | Type_I                        | Overflow,
            CONV_OVF_U      = Action_Convert            | Type_U                        | Overflow,
            ADD_OVF         = Action_ALU                | Op_ADD                        | Overflow,
            ADD_OVF_UN      = Action_ALU                | Op_ADD             | Unsigned | Overflow,
            MUL_OVF         = Action_ALU                | Op_MUL                        | Overflow,
            MUL_OVF_UN      = Action_ALU                | Op_MUL             | Unsigned | Overflow,
            SUB_OVF         = Action_ALU                | Op_SUB                        | Overflow,
            SUB_OVF_UN      = Action_ALU                | Op_SUB             | Unsigned | Overflow,
            ENDFINALLY      = Action_EH                 | EH_EndFinally                           ,
            LEAVE           = Action_EH                 | EH_Leave                                ,
            LEAVE_S         = Action_EH                 | EH_Leave                                ,
            STIND_I         = Action_StoreIndirect      | Type_I                                  ,
            CONV_U          = Action_Convert            | Type_U                                  ,
            ARGLIST         = Action_Stack              | Stack_ArgList                           ,
            CEQ             = Action_Set                | Cond_EQ                                 ,
            CGT             = Action_Set                | Cond_GT                                 ,
            CGT_UN          = Action_Set                | Cond_GT            | Unsigned           ,
            CLT             = Action_Set                | Cond_LT                                 ,
            CLT_UN          = Action_Set                | Cond_LT            | Unsigned           ,
            LDFTN           = Action_Object             | Obj_LdFtn                               ,
            LDVIRTFTN       = Action_Object             | Obj_LdVirtFtn                           ,
            LOCALLOC        = Action_Stack              | Stack_LocAlloc                          ,
            ENDFILTER       = Action_EH                 | EH_EndFilter                            ,
            UNALIGNED       = Action_Modifier           | Mod_Unaligned                           ,
            VOLATILE        = Action_Modifier           | Mod_Volatile                            ,
            TAILCALL        = Action_Modifier           | Mod_TailCall                            ,
            INITOBJ         = Action_Object             | Obj_InitObj                             ,
            CONSTRAINED     = Action_Modifier           | Mod_Constrained                         ,
            CPBLK           = Action_Object             | Obj_CpBlk                               ,
            INITBLK         = Action_Object             | Obj_InitBlk                             ,
            NO              = Action_Modifier           | Mod_NoCheck                             ,
            RETHROW         = Action_EH                 | EH_ReThrow                              ,
            SIZEOF          = Action_Object             | Obj_SizeOf                              ,
            REFANYTYPE      = Action_TypedRef           | TypedRef_RefAnyType                     ,
            READONLY        = Action_Modifier           | Mod_Readonly                            ,
        }

        public class OpcodeInfo
        {
            //
            // State
            //

            private readonly String m_name;
            private readonly Opcode m_opcode;
            private readonly Format m_format;

            //
            // Constructor Methods
            //

            internal OpcodeInfo( String name   ,
                                 Opcode opcode ,
                                 Format format )
            {
                m_name   = name;
                m_opcode = opcode;
                m_format = format;
            }

            //
            // Access Methods
            //

            public String Name
            {
                get
                {
                    return m_name;
                }
            }

            public Opcode Opcode
            {
                get
                {
                    return m_opcode;
                }
            }

            public OpcodeOperand OperandFormat
            {
                get
                {
                    return (OpcodeOperand)(m_format & Format.Operand_MASK);
                }
            }

            public OpcodeFlowControl Control
            {
                get
                {
                    return (OpcodeFlowControl)(m_format & Format.FlowControl_MASK);
                }
            }

            public int OperandSize
            {
                get
                {
                    switch((OpcodeSize)(m_format & Format.Size_MASK))
                    {
                        case OpcodeSize.Byte    : return  1;
                        case OpcodeSize.Short   : return  2;
                        case OpcodeSize.Word    : return  4;
                        case OpcodeSize.DWord   : return  8;
                        case OpcodeSize.Variable: return -1;
                        default                 : return  0;
                    }
                }
            }

            public int ImplicitOperandValue
            {
                get
                {
                    switch((OpcodeSize)(m_format & Format.Size_MASK))
                    {
                        case OpcodeSize.Implicit_M1: return -1;
                        case OpcodeSize.Implicit_0 : return  0;
                        case OpcodeSize.Implicit_1 : return  1;
                        case OpcodeSize.Implicit_2 : return  2;
                        case OpcodeSize.Implicit_3 : return  3;
                        case OpcodeSize.Implicit_4 : return  4;
                        case OpcodeSize.Implicit_5 : return  5;
                        case OpcodeSize.Implicit_6 : return  6;
                        case OpcodeSize.Implicit_7 : return  7;
                        case OpcodeSize.Implicit_8 : return  8;
                        default                    : return  0;
                    }
                }
            }

            //--//

            public OpcodeAction Action
            {
                get
                {
                    return (OpcodeAction)(m_format & Format.Action_MASK);
                }
            }

            public OpcodeActionDebug ActionDebug
            {
                get
                {
                    return (OpcodeActionDebug)(m_format & Format.Debug_MASK);
                }
            }

            public OpcodeActionTarget ActionTarget
            {
                get
                {
                    return (OpcodeActionTarget)(m_format & Format.Target_MASK);
                }
            }

            public OpcodeActionStack ActionStack
            {
                get
                {
                    return (OpcodeActionStack)(m_format & Format.Stack_MASK);
                }
            }

            public OpcodeActionCall ActionCall
            {
                get
                {
                    return (OpcodeActionCall)(m_format & Format.Call_MASK);
                }
            }

            public OpcodeActionObject ActionObject
            {
                get
                {
                    return (OpcodeActionObject)(m_format & Format.Obj_MASK);
                }
            }

            public OpcodeActionTypedReference ActionTypedReference
            {
                get
                {
                    return (OpcodeActionTypedReference)(m_format & Format.TypedRef_MASK);
                }
            }

            public OpcodeActionExceptionHandling ActionExceptionHandling
            {
                get
                {
                    return (OpcodeActionExceptionHandling)(m_format & Format.EH_MASK);
                }
            }

            public OpcodeActionCondition ActionCondition
            {
                get
                {
                    return (OpcodeActionCondition)(m_format & Format.Cond_MASK);
                }
            }

            public OpcodeActionALU ActionALU
            {
                get
                {
                    return (OpcodeActionALU)(m_format & Format.Op_MASK);
                }
            }

            public OpcodeActionModifier ActionModifier
            {
                get
                {
                    return (OpcodeActionModifier)(m_format & Format.Mod_MASK);
                }
            }

            public OpcodeActionType ActionType
            {
                get
                {
                    return (OpcodeActionType)(m_format & Format.Type_MASK);
                }
            }

            public bool IsSigned
            {
                get
                {
                    return (m_format & Format.Unsigned) == 0;
                }
            }

            public bool IsUnsigned
            {
                get
                {
                    return (m_format & Format.Unsigned) != 0;
                }
            }

            public bool RequiresOverflowCheck
            {
                get
                {
                    return (m_format & Format.Overflow) != 0;
                }
            }

            //
            // Debug Methods
            //

            public override String ToString()
            {
                return m_name;
            }
        }

        internal static readonly OpcodeInfo[] OpcodeInfoTable = new OpcodeInfo[(int)Opcode.COUNT]
        {
            new OpcodeInfo( "nop"             , Opcode.NOP             , Format.NOP              | Format.None                                                        ),
            new OpcodeInfo( "break"           , Opcode.BREAK           , Format.BREAK            | Format.None                                                        ),
            new OpcodeInfo( "ldarg.0"         , Opcode.LDARG_0         , Format.LDARG            | Format.Var     | Format.SizeImplicit_0                             ),
            new OpcodeInfo( "ldarg.1"         , Opcode.LDARG_1         , Format.LDARG            | Format.Var     | Format.SizeImplicit_1                             ),
            new OpcodeInfo( "ldarg.2"         , Opcode.LDARG_2         , Format.LDARG            | Format.Var     | Format.SizeImplicit_2                             ),
            new OpcodeInfo( "ldarg.3"         , Opcode.LDARG_3         , Format.LDARG            | Format.Var     | Format.SizeImplicit_3                             ),
            new OpcodeInfo( "ldloc.0"         , Opcode.LDLOC_0         , Format.LDLOC            | Format.Var     | Format.SizeImplicit_0                             ),
            new OpcodeInfo( "ldloc.1"         , Opcode.LDLOC_1         , Format.LDLOC            | Format.Var     | Format.SizeImplicit_1                             ),
            new OpcodeInfo( "ldloc.2"         , Opcode.LDLOC_2         , Format.LDLOC            | Format.Var     | Format.SizeImplicit_2                             ),
            new OpcodeInfo( "ldloc.3"         , Opcode.LDLOC_3         , Format.LDLOC            | Format.Var     | Format.SizeImplicit_3                             ),
            new OpcodeInfo( "stloc.0"         , Opcode.STLOC_0         , Format.STLOC            | Format.Var     | Format.SizeImplicit_0                             ),
            new OpcodeInfo( "stloc.1"         , Opcode.STLOC_1         , Format.STLOC            | Format.Var     | Format.SizeImplicit_1                             ),
            new OpcodeInfo( "stloc.2"         , Opcode.STLOC_2         , Format.STLOC            | Format.Var     | Format.SizeImplicit_2                             ),
            new OpcodeInfo( "stloc.3"         , Opcode.STLOC_3         , Format.STLOC            | Format.Var     | Format.SizeImplicit_3                             ),
            new OpcodeInfo( "ldarg.s"         , Opcode.LDARG_S         , Format.LDARG            | Format.Var     | Format.SizeByte                                   ),
            new OpcodeInfo( "ldarga.s"        , Opcode.LDARGA_S        , Format.LDARGA           | Format.Var     | Format.SizeByte                                   ),
            new OpcodeInfo( "starg.s"         , Opcode.STARG_S         , Format.STARG            | Format.Var     | Format.SizeByte                                   ),
            new OpcodeInfo( "ldloc.s"         , Opcode.LDLOC_S         , Format.LDLOC            | Format.Var     | Format.SizeByte                                   ),
            new OpcodeInfo( "ldloca.s"        , Opcode.LDLOCA_S        , Format.LDLOCA           | Format.Var     | Format.SizeByte                                   ),
            new OpcodeInfo( "stloc.s"         , Opcode.STLOC_S         , Format.STLOC            | Format.Var     | Format.SizeByte                                   ),
            new OpcodeInfo( "ldnull"          , Opcode.LDNULL          , Format.LDNULL           | Format.None                                                        ),
            new OpcodeInfo( "ldc.i4.m1"       , Opcode.LDC_I4_M1       , Format.LDC              | Format.Int     | Format.SizeImplicit_M1                            ),
            new OpcodeInfo( "ldc.i4.0"        , Opcode.LDC_I4_0        , Format.LDC              | Format.Int     | Format.SizeImplicit_0                             ),
            new OpcodeInfo( "ldc.i4.1"        , Opcode.LDC_I4_1        , Format.LDC              | Format.Int     | Format.SizeImplicit_1                             ),
            new OpcodeInfo( "ldc.i4.2"        , Opcode.LDC_I4_2        , Format.LDC              | Format.Int     | Format.SizeImplicit_2                             ),
            new OpcodeInfo( "ldc.i4.3"        , Opcode.LDC_I4_3        , Format.LDC              | Format.Int     | Format.SizeImplicit_3                             ),
            new OpcodeInfo( "ldc.i4.4"        , Opcode.LDC_I4_4        , Format.LDC              | Format.Int     | Format.SizeImplicit_4                             ),
            new OpcodeInfo( "ldc.i4.5"        , Opcode.LDC_I4_5        , Format.LDC              | Format.Int     | Format.SizeImplicit_5                             ),
            new OpcodeInfo( "ldc.i4.6"        , Opcode.LDC_I4_6        , Format.LDC              | Format.Int     | Format.SizeImplicit_6                             ),
            new OpcodeInfo( "ldc.i4.7"        , Opcode.LDC_I4_7        , Format.LDC              | Format.Int     | Format.SizeImplicit_7                             ),
            new OpcodeInfo( "ldc.i4.8"        , Opcode.LDC_I4_8        , Format.LDC              | Format.Int     | Format.SizeImplicit_8                             ),
            new OpcodeInfo( "ldc.i4.s"        , Opcode.LDC_I4_S        , Format.LDC              | Format.Int     | Format.SizeByte                                   ),
            new OpcodeInfo( "ldc.i4"          , Opcode.LDC_I4          , Format.LDC              | Format.Int     | Format.SizeWord                                   ),
            new OpcodeInfo( "ldc.i8"          , Opcode.LDC_I8          , Format.LDC              | Format.Int     | Format.SizeDWord                                  ),
            new OpcodeInfo( "ldc.r4"          , Opcode.LDC_R4          , Format.LDC              | Format.Float   | Format.SizeWord                                   ),
            new OpcodeInfo( "ldc.r8"          , Opcode.LDC_R8          , Format.LDC              | Format.Float   | Format.SizeDWord                                  ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED49        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "dup"             , Opcode.DUP             , Format.DUP              | Format.None                                                        ),
            new OpcodeInfo( "pop"             , Opcode.POP             , Format.POP              | Format.None                                                        ),
            new OpcodeInfo( "jmp"             , Opcode.JMP             , Format.JMP              | Format.Method  | Format.SizeWord     | Format.UnconditionalControl ),
            new OpcodeInfo( "call"            , Opcode.CALL            , Format.CALL             | Format.Method  | Format.SizeWord                                   ),
            new OpcodeInfo( "calli"           , Opcode.CALLI           , Format.CALLI            | Format.Sig     | Format.SizeWord                                   ),
            new OpcodeInfo( "ret"             , Opcode.RET             , Format.RET              | Format.None                          | Format.UnconditionalControl ),
            new OpcodeInfo( "br.s"            , Opcode.BR_S            , Format.BR               | Format.Branch  | Format.SizeByte     | Format.UnconditionalControl ),
            new OpcodeInfo( "brfalse.s"       , Opcode.BRFALSE_S       , Format.BRFALSE          | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "brtrue.s"        , Opcode.BRTRUE_S        , Format.BRTRUE           | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "beq.s"           , Opcode.BEQ_S           , Format.BEQ              | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "bge.s"           , Opcode.BGE_S           , Format.BGE              | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "bgt.s"           , Opcode.BGT_S           , Format.BGT              | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "ble.s"           , Opcode.BLE_S           , Format.BLE              | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "blt.s"           , Opcode.BLT_S           , Format.BLT              | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "bne.un.s"        , Opcode.BNE_UN_S        , Format.BNE_UN           | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "bge.un.s"        , Opcode.BGE_UN_S        , Format.BGE_UN           | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "bgt.un.s"        , Opcode.BGT_UN_S        , Format.BGT_UN           | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "ble.un.s"        , Opcode.BLE_UN_S        , Format.BLE_UN           | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "blt.un.s"        , Opcode.BLT_UN_S        , Format.BLT_UN           | Format.Branch  | Format.SizeByte     | Format.ConditionalControl   ),
            new OpcodeInfo( "br"              , Opcode.BR              , Format.BR               | Format.Branch  | Format.SizeWord     | Format.UnconditionalControl ),
            new OpcodeInfo( "brfalse"         , Opcode.BRFALSE         , Format.BRFALSE          | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "brtrue"          , Opcode.BRTRUE          , Format.BRTRUE           | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "beq"             , Opcode.BEQ             , Format.BEQ              | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "bge"             , Opcode.BGE             , Format.BGE              | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "bgt"             , Opcode.BGT             , Format.BGT              | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "ble"             , Opcode.BLE             , Format.BLE              | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "blt"             , Opcode.BLT             , Format.BLT              | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "bne.un"          , Opcode.BNE_UN          , Format.BNE_UN           | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "bge.un"          , Opcode.BGE_UN          , Format.BGE_UN           | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "bgt.un"          , Opcode.BGT_UN          , Format.BGT_UN           | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "ble.un"          , Opcode.BLE_UN          , Format.BLE_UN           | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "blt.un"          , Opcode.BLT_UN          , Format.BLT_UN           | Format.Branch  | Format.SizeWord     | Format.ConditionalControl   ),
            new OpcodeInfo( "switch"          , Opcode.SWITCH          , Format.SWITCH           | Format.Switch  | Format.SizeVariable | Format.ConditionalControl   ),
            new OpcodeInfo( "ldind.i1"        , Opcode.LDIND_I1        , Format.LDIND_I1         | Format.None                                                        ),
            new OpcodeInfo( "ldind.u1"        , Opcode.LDIND_U1        , Format.LDIND_U1         | Format.None                                                        ),
            new OpcodeInfo( "ldind.i2"        , Opcode.LDIND_I2        , Format.LDIND_I2         | Format.None                                                        ),
            new OpcodeInfo( "ldind.u2"        , Opcode.LDIND_U2        , Format.LDIND_U2         | Format.None                                                        ),
            new OpcodeInfo( "ldind.i4"        , Opcode.LDIND_I4        , Format.LDIND_I4         | Format.None                                                        ),
            new OpcodeInfo( "ldind.u4"        , Opcode.LDIND_U4        , Format.LDIND_U4         | Format.None                                                        ),
            new OpcodeInfo( "ldind.i8"        , Opcode.LDIND_I8        , Format.LDIND_I8         | Format.None                                                        ),
            new OpcodeInfo( "ldind.i"         , Opcode.LDIND_I         , Format.LDIND_I          | Format.None                                                        ),
            new OpcodeInfo( "ldind.r4"        , Opcode.LDIND_R4        , Format.LDIND_R4         | Format.None                                                        ),
            new OpcodeInfo( "ldind.r8"        , Opcode.LDIND_R8        , Format.LDIND_R8         | Format.None                                                        ),
            new OpcodeInfo( "ldind.ref"       , Opcode.LDIND_REF       , Format.LDIND_REF        | Format.None                                                        ),
            new OpcodeInfo( "stind.ref"       , Opcode.STIND_REF       , Format.STIND_REF        | Format.None                                                        ),
            new OpcodeInfo( "stind.i1"        , Opcode.STIND_I1        , Format.STIND_I1         | Format.None                                                        ),
            new OpcodeInfo( "stind.i2"        , Opcode.STIND_I2        , Format.STIND_I2         | Format.None                                                        ),
            new OpcodeInfo( "stind.i4"        , Opcode.STIND_I4        , Format.STIND_I4         | Format.None                                                        ),
            new OpcodeInfo( "stind.i8"        , Opcode.STIND_I8        , Format.STIND_I8         | Format.None                                                        ),
            new OpcodeInfo( "stind.r4"        , Opcode.STIND_R4        , Format.STIND_R4         | Format.None                                                        ),
            new OpcodeInfo( "stind.r8"        , Opcode.STIND_R8        , Format.STIND_R8         | Format.None                                                        ),
            new OpcodeInfo( "add"             , Opcode.ADD             , Format.ADD              | Format.None                                                        ),
            new OpcodeInfo( "sub"             , Opcode.SUB             , Format.SUB              | Format.None                                                        ),
            new OpcodeInfo( "mul"             , Opcode.MUL             , Format.MUL              | Format.None                                                        ),
            new OpcodeInfo( "div"             , Opcode.DIV             , Format.DIV              | Format.None                                                        ),
            new OpcodeInfo( "div.un"          , Opcode.DIV_UN          , Format.DIV_UN           | Format.None                                                        ),
            new OpcodeInfo( "rem"             , Opcode.REM             , Format.REM              | Format.None                                                        ),
            new OpcodeInfo( "rem.un"          , Opcode.REM_UN          , Format.REM_UN           | Format.None                                                        ),
            new OpcodeInfo( "and"             , Opcode.AND             , Format.AND              | Format.None                                                        ),
            new OpcodeInfo( "or"              , Opcode.OR              , Format.OR               | Format.None                                                        ),
            new OpcodeInfo( "xor"             , Opcode.XOR             , Format.XOR              | Format.None                                                        ),
            new OpcodeInfo( "shl"             , Opcode.SHL             , Format.SHL              | Format.None                                                        ),
            new OpcodeInfo( "shr"             , Opcode.SHR             , Format.SHR              | Format.None                                                        ),
            new OpcodeInfo( "shr.un"          , Opcode.SHR_UN          , Format.SHR_UN           | Format.None                                                        ),
            new OpcodeInfo( "neg"             , Opcode.NEG             , Format.NEG              | Format.None                                                        ),
            new OpcodeInfo( "not"             , Opcode.NOT             , Format.NOT              | Format.None                                                        ),
            new OpcodeInfo( "conv.i1"         , Opcode.CONV_I1         , Format.CONV_I1          | Format.None                                                        ),
            new OpcodeInfo( "conv.i2"         , Opcode.CONV_I2         , Format.CONV_I2          | Format.None                                                        ),
            new OpcodeInfo( "conv.i4"         , Opcode.CONV_I4         , Format.CONV_I4          | Format.None                                                        ),
            new OpcodeInfo( "conv.i8"         , Opcode.CONV_I8         , Format.CONV_I8          | Format.None                                                        ),
            new OpcodeInfo( "conv.r4"         , Opcode.CONV_R4         , Format.CONV_R4          | Format.None                                                        ),
            new OpcodeInfo( "conv.r8"         , Opcode.CONV_R8         , Format.CONV_R8          | Format.None                                                        ),
            new OpcodeInfo( "conv.u4"         , Opcode.CONV_U4         , Format.CONV_U4          | Format.None                                                        ),
            new OpcodeInfo( "conv.u8"         , Opcode.CONV_U8         , Format.CONV_U8          | Format.None                                                        ),
            new OpcodeInfo( "callvirt"        , Opcode.CALLVIRT        , Format.CALLVIRT         | Format.Method  | Format.SizeWord                                   ),
            new OpcodeInfo( "cpobj"           , Opcode.CPOBJ           , Format.CPOBJ            | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "ldobj"           , Opcode.LDOBJ           , Format.LDOBJ            | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "ldstr"           , Opcode.LDSTR           , Format.LDSTR            | Format.String  | Format.SizeWord                                   ),
            new OpcodeInfo( "newobj"          , Opcode.NEWOBJ          , Format.NEWOBJ           | Format.Method  | Format.SizeWord                                   ),
            new OpcodeInfo( "castclass"       , Opcode.CASTCLASS       , Format.CASTCLASS        | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "isinst"          , Opcode.ISINST          , Format.ISINST           | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "conv.r.un"       , Opcode.CONV_R_UN       , Format.CONV_R_UN        | Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED58        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED1         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unbox"           , Opcode.UNBOX           , Format.UNBOX            | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "throw"           , Opcode.THROW           , Format.THROW            | Format.None                          | Format.ExceptionHandling    ),
            new OpcodeInfo( "ldfld"           , Opcode.LDFLD           , Format.LDFLD            | Format.Field   | Format.SizeWord                                   ),
            new OpcodeInfo( "ldflda"          , Opcode.LDFLDA          , Format.LDFLDA           | Format.Field   | Format.SizeWord                                   ),
            new OpcodeInfo( "stfld"           , Opcode.STFLD           , Format.STFLD            | Format.Field   | Format.SizeWord                                   ),
            new OpcodeInfo( "ldsfld"          , Opcode.LDSFLD          , Format.LDSFLD           | Format.Field   | Format.SizeWord                                   ),
            new OpcodeInfo( "ldsflda"         , Opcode.LDSFLDA         , Format.LDSFLDA          | Format.Field   | Format.SizeWord                                   ),
            new OpcodeInfo( "stsfld"          , Opcode.STSFLD          , Format.STSFLD           | Format.Field   | Format.SizeWord                                   ),
            new OpcodeInfo( "stobj"           , Opcode.STOBJ           , Format.STOBJ            | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "conv.ovf.i1.un"  , Opcode.CONV_OVF_I1_UN  , Format.CONV_OVF_I1_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i2.un"  , Opcode.CONV_OVF_I2_UN  , Format.CONV_OVF_I2_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i4.un"  , Opcode.CONV_OVF_I4_UN  , Format.CONV_OVF_I4_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i8.un"  , Opcode.CONV_OVF_I8_UN  , Format.CONV_OVF_I8_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u1.un"  , Opcode.CONV_OVF_U1_UN  , Format.CONV_OVF_U1_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u2.un"  , Opcode.CONV_OVF_U2_UN  , Format.CONV_OVF_U2_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u4.un"  , Opcode.CONV_OVF_U4_UN  , Format.CONV_OVF_U4_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u8.un"  , Opcode.CONV_OVF_U8_UN  , Format.CONV_OVF_U8_UN   | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i.un"   , Opcode.CONV_OVF_I_UN   , Format.CONV_OVF_I_UN    | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u.un"   , Opcode.CONV_OVF_U_UN   , Format.CONV_OVF_U_UN    | Format.None                                                        ),
            new OpcodeInfo( "box"             , Opcode.BOX             , Format.BOX              | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "newarr"          , Opcode.NEWARR          , Format.NEWARR           | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "ldlen"           , Opcode.LDLEN           , Format.LDLEN            | Format.None                                                        ),
            new OpcodeInfo( "ldelema"         , Opcode.LDELEMA         , Format.LDELEMA          | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "ldelem.i1"       , Opcode.LDELEM_I1       , Format.LDELEM_I1        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.u1"       , Opcode.LDELEM_U1       , Format.LDELEM_U1        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.i2"       , Opcode.LDELEM_I2       , Format.LDELEM_I2        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.u2"       , Opcode.LDELEM_U2       , Format.LDELEM_U2        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.i4"       , Opcode.LDELEM_I4       , Format.LDELEM_I4        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.u4"       , Opcode.LDELEM_U4       , Format.LDELEM_U4        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.i8"       , Opcode.LDELEM_I8       , Format.LDELEM_I8        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.i"        , Opcode.LDELEM_I        , Format.LDELEM_I         | Format.None                                                        ),
            new OpcodeInfo( "ldelem.r4"       , Opcode.LDELEM_R4       , Format.LDELEM_R4        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.r8"       , Opcode.LDELEM_R8       , Format.LDELEM_R8        | Format.None                                                        ),
            new OpcodeInfo( "ldelem.ref"      , Opcode.LDELEM_REF      , Format.LDELEM_REF       | Format.None                                                        ),
            new OpcodeInfo( "stelem.i"        , Opcode.STELEM_I        , Format.STELEM_I         | Format.None                                                        ),
            new OpcodeInfo( "stelem.i1"       , Opcode.STELEM_I1       , Format.STELEM_I1        | Format.None                                                        ),
            new OpcodeInfo( "stelem.i2"       , Opcode.STELEM_I2       , Format.STELEM_I2        | Format.None                                                        ),
            new OpcodeInfo( "stelem.i4"       , Opcode.STELEM_I4       , Format.STELEM_I4        | Format.None                                                        ),
            new OpcodeInfo( "stelem.i8"       , Opcode.STELEM_I8       , Format.STELEM_I8        | Format.None                                                        ),
            new OpcodeInfo( "stelem.r4"       , Opcode.STELEM_R4       , Format.STELEM_R4        | Format.None                                                        ),
            new OpcodeInfo( "stelem.r8"       , Opcode.STELEM_R8       , Format.STELEM_R8        | Format.None                                                        ),
            new OpcodeInfo( "stelem.ref"      , Opcode.STELEM_REF      , Format.STELEM_REF       | Format.None                                                        ),
            new OpcodeInfo( "ldelem"          , Opcode.LDELEM          , Format.LDELEM           | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "stelem"          , Opcode.STELEM          , Format.STELEM           | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "unbox.any"       , Opcode.UNBOX_ANY       , Format.UNBOX_ANY        | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED5         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED6         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED7         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED8         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED9         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED10        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED11        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED12        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED13        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED14        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED15        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED16        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED17        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "conv.ovf.i1"     , Opcode.CONV_OVF_I1     , Format.CONV_OVF_I1      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u1"     , Opcode.CONV_OVF_U1     , Format.CONV_OVF_U1      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i2"     , Opcode.CONV_OVF_I2     , Format.CONV_OVF_I2      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u2"     , Opcode.CONV_OVF_U2     , Format.CONV_OVF_U2      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i4"     , Opcode.CONV_OVF_I4     , Format.CONV_OVF_I4      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u4"     , Opcode.CONV_OVF_U4     , Format.CONV_OVF_U4      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i8"     , Opcode.CONV_OVF_I8     , Format.CONV_OVF_I8      | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u8"     , Opcode.CONV_OVF_U8     , Format.CONV_OVF_U8      | Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED50        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED18        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED19        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED20        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED21        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED22        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED23        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "refanyval"       , Opcode.REFANYVAL       , Format.REFANYVAL        | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "ckfinite"        , Opcode.CKFINITE        , Format.CKFINITE         | Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED24        ,                           Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED25        ,                           Format.None                                                        ),
            new OpcodeInfo( "mkrefany"        , Opcode.MKREFANY        , Format.MKREFANY         | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED59        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED60        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED61        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED62        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED63        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED64        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED65        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED66        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED67        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "ldtoken"         , Opcode.LDTOKEN         , Format.LDTOKEN          | Format.Token   | Format.SizeWord                                   ),
            new OpcodeInfo( "conv.u2"         , Opcode.CONV_U2         , Format.CONV_U2          | Format.None                                                        ),
            new OpcodeInfo( "conv.u1"         , Opcode.CONV_U1         , Format.CONV_U1          | Format.None                                                        ),
            new OpcodeInfo( "conv.i"          , Opcode.CONV_I          , Format.CONV_I           | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.i"      , Opcode.CONV_OVF_I      , Format.CONV_OVF_I       | Format.None                                                        ),
            new OpcodeInfo( "conv.ovf.u"      , Opcode.CONV_OVF_U      , Format.CONV_OVF_U       | Format.None                                                        ),
            new OpcodeInfo( "add.ovf"         , Opcode.ADD_OVF         , Format.ADD_OVF          | Format.None                                                        ),
            new OpcodeInfo( "add.ovf.un"      , Opcode.ADD_OVF_UN      , Format.ADD_OVF_UN       | Format.None                                                        ),
            new OpcodeInfo( "mul.ovf"         , Opcode.MUL_OVF         , Format.MUL_OVF          | Format.None                                                        ),
            new OpcodeInfo( "mul.ovf.un"      , Opcode.MUL_OVF_UN      , Format.MUL_OVF_UN       | Format.None                                                        ),
            new OpcodeInfo( "sub.ovf"         , Opcode.SUB_OVF         , Format.SUB_OVF          | Format.None                                                        ),
            new OpcodeInfo( "sub.ovf.un"      , Opcode.SUB_OVF_UN      , Format.SUB_OVF_UN       | Format.None                                                        ),
            new OpcodeInfo( "endfinally"      , Opcode.ENDFINALLY      , Format.ENDFINALLY       | Format.None                          | Format.ExceptionHandling    ),
            new OpcodeInfo( "leave"           , Opcode.LEAVE           , Format.LEAVE            | Format.Branch  | Format.SizeWord     | Format.ExceptionHandling    ),
            new OpcodeInfo( "leave.s"         , Opcode.LEAVE_S         , Format.LEAVE_S          | Format.Branch  | Format.SizeByte     | Format.ExceptionHandling    ),
            new OpcodeInfo( "stind.i"         , Opcode.STIND_I         , Format.STIND_I          | Format.None                                                        ),
            new OpcodeInfo( "conv.u"          , Opcode.CONV_U          , Format.CONV_U           | Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED26        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED27        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED28        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED29        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED30        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED31        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED32        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED33        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED34        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED35        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED36        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED37        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED38        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED39        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED40        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED41        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED42        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED43        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED44        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED45        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED46        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED47        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED48        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix7"         , Opcode.PREFIX7         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix6"         , Opcode.PREFIX6         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix5"         , Opcode.PREFIX5         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix4"         , Opcode.PREFIX4         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix3"         , Opcode.PREFIX3         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix2"         , Opcode.PREFIX2         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "prefix1"         , Opcode.PREFIX1         ,                           Format.None                                                        ),
            new OpcodeInfo( "prefixref"       , Opcode.PREFIXREF       ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "arglist"         , Opcode.ARGLIST         , Format.ARGLIST          | Format.None                                                        ),
            new OpcodeInfo( "ceq"             , Opcode.CEQ             , Format.CEQ              | Format.None                                                        ),
            new OpcodeInfo( "cgt"             , Opcode.CGT             , Format.CGT              | Format.None                                                        ),
            new OpcodeInfo( "cgt.un"          , Opcode.CGT_UN          , Format.CGT_UN           | Format.None                                                        ),
            new OpcodeInfo( "clt"             , Opcode.CLT             , Format.CLT              | Format.None                                                        ),
            new OpcodeInfo( "clt.un"          , Opcode.CLT_UN          , Format.CLT_UN           | Format.None                                                        ),
            new OpcodeInfo( "ldftn"           , Opcode.LDFTN           , Format.LDFTN            | Format.Method  | Format.SizeWord                                   ),
            new OpcodeInfo( "ldvirtftn"       , Opcode.LDVIRTFTN       , Format.LDVIRTFTN        | Format.Method  | Format.SizeWord                                   ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED56        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "ldarg"           , Opcode.LDARG           , Format.LDARG            | Format.Var     | Format.SizeShort                                  ),
            new OpcodeInfo( "ldarga"          , Opcode.LDARGA          , Format.LDARGA           | Format.Var     | Format.SizeShort                                  ),
            new OpcodeInfo( "starg"           , Opcode.STARG           , Format.STARG            | Format.Var     | Format.SizeShort                                  ),
            new OpcodeInfo( "ldloc"           , Opcode.LDLOC           , Format.LDLOC            | Format.Var     | Format.SizeShort                                  ),
            new OpcodeInfo( "ldloca"          , Opcode.LDLOCA          , Format.LDLOCA           | Format.Var     | Format.SizeShort                                  ),
            new OpcodeInfo( "stloc"           , Opcode.STLOC           , Format.STLOC            | Format.Var     | Format.SizeShort                                  ),
            new OpcodeInfo( "localloc"        , Opcode.LOCALLOC        , Format.LOCALLOC         | Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED57        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "endfilter"       , Opcode.ENDFILTER       , Format.ENDFILTER        | Format.None                          | Format.ExceptionHandling    ),
            new OpcodeInfo( "unaligned."      , Opcode.UNALIGNED       , Format.UNALIGNED        | Format.Int     | Format.SizeByte                                   ),
            new OpcodeInfo( "volatile."       , Opcode.VOLATILE        , Format.VOLATILE         | Format.None                                                        ),
            new OpcodeInfo( "tail."           , Opcode.TAILCALL        , Format.TAILCALL         | Format.None                                                        ),
            new OpcodeInfo( "initobj"         , Opcode.INITOBJ         , Format.INITOBJ          | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "constrained."    , Opcode.CONSTRAINED     , Format.CONSTRAINED      | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "cpblk"           , Opcode.CPBLK           , Format.CPBLK            | Format.None                                                        ),
            new OpcodeInfo( "initblk"         , Opcode.INITBLK         , Format.INITBLK          | Format.None                                                        ),
            new OpcodeInfo( "no."             , Opcode.NO              , Format.NO               | Format.Int     | Format.SizeByte                                   ),
            new OpcodeInfo( "rethrow"         , Opcode.RETHROW         , Format.RETHROW          | Format.None                          | Format.ExceptionHandling    ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED51        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "sizeof"          , Opcode.SIZEOF          , Format.SIZEOF           | Format.Type    | Format.SizeWord                                   ),
            new OpcodeInfo( "refanytype"      , Opcode.REFANYTYPE      , Format.REFANYTYPE       | Format.None                                                        ),
            new OpcodeInfo( "readonly."       , Opcode.READONLY        , Format.READONLY         | Format.None                                                        ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED53        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED54        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED55        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "unused"          , Opcode.UNUSED70        ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "illegal"         , Opcode.ILLEGAL         ,                           Format.Illegal                                                     ),
            new OpcodeInfo( "endmac"          , Opcode.MACRO_END       ,                           Format.Illegal                                                     ),
        };

////    static Instruction()
////    {
////        for(Opcode opcode = 0; opcode < Opcode.COUNT; opcode++)
////        {
////            if(OpcodeInfoTable[(int)opcode].Opcode != opcode)
////            {
////                throw new MetaDataMethod.IllegalInstructionStreamException( "opcode invariant failure" );
////            }
////        }
////    }

        //--//

        //
        // State
        //

        private readonly OpcodeInfo          m_opcodeInfo;
        private readonly object              m_operand;
        private readonly Debugging.DebugInfo m_debugInfo;

        //
        // Constructor Methods
        //

        internal Instruction( OpcodeInfo          opcodeInfo ,
                              Object              operand    ,
                              Debugging.DebugInfo debugInfo  )
        {
            m_opcodeInfo = opcodeInfo;
            m_operand    = operand;
            m_debugInfo  = debugInfo;
        }

        //
        // MetaDataEquality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is Instruction) // Since the class is sealed (no subclasses allowed), there's no need to compare using .GetType()
            {
                Instruction other = (Instruction)obj;

                if(m_opcodeInfo == other.m_opcodeInfo &&
                   m_operand    == other.m_operand    &&
                   m_debugInfo  == other.m_debugInfo   )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return      m_opcodeInfo.Name.GetHashCode() ^
                   (int)m_opcodeInfo.Opcode;
        }

        //
        // Access Methods
        //

        public OpcodeInfo Operator
        {
            get
            {
                return m_opcodeInfo;
            }
        }

        public object Argument
        {
            get
            {
                return m_operand;
            }
        }

        public Debugging.DebugInfo DebugInfo
        {
            get
            {
                return m_debugInfo;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            if(m_debugInfo != null)
            {
                return (m_opcodeInfo.Name + m_debugInfo.ToString());
            }
            else
            {
                return (m_opcodeInfo.Name);
            }
        }
    }
}

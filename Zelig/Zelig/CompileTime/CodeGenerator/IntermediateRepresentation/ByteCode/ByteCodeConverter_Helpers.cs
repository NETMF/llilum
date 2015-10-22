//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed partial class ByteCodeConverter
    {
        //
        // State
        //

        private int          m_currentInstructionOffset;

        private Expression[] m_activeStackModel;

        //
        // Constructor Methods
        //

        private TypeRepresentation ProcessInstruction_GetTypeFromActionType( Instruction        instr ,
                                                                             TypeRepresentation tdRef )
        {
            WellKnownTypes wkt = m_typeSystem.WellKnownTypes;

            switch(instr.Operator.ActionType)
            {
                case Instruction.OpcodeActionType.I1    : return wkt.System_SByte;
                case Instruction.OpcodeActionType.U1    : return wkt.System_Byte;
                case Instruction.OpcodeActionType.I2    : return wkt.System_Int16;
                case Instruction.OpcodeActionType.U2    : return wkt.System_UInt16;
                case Instruction.OpcodeActionType.I4    : return wkt.System_Int32;
                case Instruction.OpcodeActionType.U4    : return wkt.System_UInt32;
                case Instruction.OpcodeActionType.I8    : return wkt.System_Int64;
                case Instruction.OpcodeActionType.U8    : return wkt.System_UInt64;
                case Instruction.OpcodeActionType.I     : return wkt.System_IntPtr;
                case Instruction.OpcodeActionType.U     : return wkt.System_UIntPtr;
                case Instruction.OpcodeActionType.R4    : return wkt.System_Single;
                case Instruction.OpcodeActionType.R8    : return wkt.System_Double;
                case Instruction.OpcodeActionType.R     : return wkt.System_Double;
                case Instruction.OpcodeActionType.String: return wkt.System_String;

                case Instruction.OpcodeActionType.Reference:
                    {
                        return tdRef;
                    }

                case Instruction.OpcodeActionType.Token:
                    {
                        return this.CurrentArgumentAsType;
                    }

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionType {0} for {1}", instr.Operator.ActionType, instr );
                    }
            }
        }

        private Expression ProcessInstruction_GetTypeOfActionTarget(     Instruction         instr    ,
                                                                     out TypeRepresentation  tdTarget ,
                                                                     out FieldRepresentation fdTarget )
        {
            Expression ex;

            switch(instr.Operator.ActionTarget)
            {
                case Instruction.OpcodeActionTarget.Local:
                    ex = m_locals[this.CurrentArgumentAsInt32];
                    break;

                case Instruction.OpcodeActionTarget.Argument:
                    ex = m_arguments[m_variable_arguments_offset + this.CurrentArgumentAsInt32];
                    break;

                case Instruction.OpcodeActionTarget.Null:
                    ex = CreateNewNullPointer( m_typeSystem.WellKnownTypes.System_Object );
                    break;

                case Instruction.OpcodeActionTarget.Constant:
                    ex = CreateNewConstant( m_typeSystem.GetTypeOfValue( this.CurrentArgument ), this.CurrentArgument );
                    break;

                case Instruction.OpcodeActionTarget.Field:
                case Instruction.OpcodeActionTarget.StaticField:
                    {
                        fdTarget = this.CurrentArgumentAsField;
                        tdTarget = fdTarget.FieldType;
                        return null;
                    }

                case Instruction.OpcodeActionTarget.String:
                    ex = CreateNewConstant( m_typeSystem.WellKnownTypes.System_String, this.CurrentArgument );
                    break;

                case Instruction.OpcodeActionTarget.Token:
                    {
                        object o = this.CurrentArgument;

                        if(o is TypeRepresentation   ||
                           o is FieldRepresentation  ||
                           o is MethodRepresentation  )
                        {
                            ex = m_typeSystem.CreateRuntimeHandle( o );
                            break;
                        }

                        throw TypeConsistencyErrorException.Create( "Unknown token {0}", o );
                    }

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionTarget {0} for {1}", instr.Operator.ActionTarget, instr );
                    }
            }

            fdTarget = null;
            tdTarget = ex.Type;

            return ex;
        }

        //--//

        private TypeRepresentation ProcessInstruction_ExtractAddressType( Instruction        instr    ,
                                                                          Expression         exAddr   ,
                                                                          TypeRepresentation tdFormal )
        {
            if(tdFormal == null)
            {
                TypeRepresentation td = exAddr.Type;
                TypeRepresentation tdActual;

                if(td is PointerTypeRepresentation)
                {
                    tdActual = td.UnderlyingType;
                }
                else if(td is BoxedValueTypeRepresentation)
                {
                    tdActual = td.UnderlyingType;
                }
                else if(td.StackEquivalentType == StackEquivalentType.NativeInt)
                {
                    tdActual = m_typeSystem.WellKnownTypes.System_IntPtr;
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Expecting pointer type, got {0}", td );
                }

                tdFormal = ProcessInstruction_GetTypeFromActionType( instr, tdActual );

                //
                // TODO: Need to distinguish between safe and unsafe code. For safe code, the check should be performed.
                //
////            if(m_typeSystem.CanBeAssignedFrom( tdFormal, tdActual ) == false)
////            {
////                throw TypeConsistencyErrorException.Create( "Expecting pointer type {0}, got {1}", tdFormal, tdActual );
////            }
            }

            return tdFormal;
        }

        private Operator ProcessInstruction_LoadIndirect( Instruction        instr    ,
                                                          Expression         exAddr   ,
                                                          TypeRepresentation tdFormal )
        {
            TypeRepresentation td    = ProcessInstruction_ExtractAddressType( instr, exAddr, tdFormal );
            VariableExpression exRes = CreateNewTemporary( td );

            ModifyStackModel( 1, exRes );

            return LoadIndirectOperator.New( instr.DebugInfo, td, exRes, exAddr, null, 0, false, true );
        }

        private Operator ProcessInstruction_StoreIndirect( Instruction        instr    ,
                                                           Expression         exAddr   ,
                                                           Expression         exValue  ,
                                                           TypeRepresentation tdFormal )
        {
            TypeRepresentation td = ProcessInstruction_ExtractAddressType( instr, exAddr, tdFormal );

            if(exValue is ConstantExpression)
            {
                exValue = CoerceConstantToType( (ConstantExpression)exValue, td );
            }

            if(!CanBeAssignedFromEvaluationStack( td, exValue ))
            {
                throw TypeConsistencyErrorException.Create( "Expecting value of type {0}, got {1}", td, exValue.Type );
            }

            PopStackModel( 2 );

            return StoreIndirectOperator.New( instr.DebugInfo, td, exAddr, exValue, null, 0, true );
        }

        //--//--//--//--//--//--//--//--//--//

        internal int CurrentInstructionOffset
        {
            get
            {
                return m_currentInstructionOffset;
            }

            set
            {
                m_currentInstructionOffset = value;
            }
        }

        internal object CurrentArgument
        {
            get
            {
                return m_byteCodeArguments[m_currentInstructionOffset];
            }
        }

        internal int CurrentArgumentAsInt32
        {
            get
            {
                return (int)CurrentArgument;
            }
        }

        internal TypeRepresentation CurrentArgumentAsType
        {
            get
            {
                return (TypeRepresentation)CurrentArgument;
            }
        }

        internal FieldRepresentation CurrentArgumentAsField
        {
            get
            {
                return (FieldRepresentation)CurrentArgument;
            }
        }

        internal MethodRepresentation CurrentArgumentAsMethod
        {
            get
            {
                return (MethodRepresentation)CurrentArgument;
            }
        }

        internal BasicBlock GetBasicBlockFromInstruction( int offset )
        {
            if(offset < 0 || offset >= m_instructionToByteCodeBlock.Length)
            {
                return null;
            }

            return m_instructionToByteCodeBlock[offset].BasicBlock;
        }


        //--//

        internal void ModifyStackModel( int        slotsToPop ,
                                        Expression slotToPush )
        {
            Expression[] stackModel = m_activeStackModel;
            int          newLen     = stackModel.Length - slotsToPop;

            if(slotToPush != null) newLen += 1;

            Expression[] res = new Expression[newLen];

            for(int i = 0; i < newLen; i++)
            {
                if(slotToPush != null)
                {
                    res[i] = slotToPush; slotToPush = null;
                }
                else
                {
                    res[i] = stackModel[slotsToPop++];
                }
            }

            m_activeStackModel = res;
        }

        internal void PushStackModel( Expression slot )
        {
            ModifyStackModel( 0, slot );
        }

        internal void PopStackModel( int slots )
        {
            ModifyStackModel( slots, null );
        }

        internal void EmptyStackModel()
        {
            m_activeStackModel = Expression.SharedEmptyArray;
        }

        internal Expression GetArgumentFromStack( int pos, int tot )
        {
            pos = tot - 1 - pos;

            CHECKS.ASSERT( pos >= 0                       , "Evaluation stack underflow in {0}", m_md );
            CHECKS.ASSERT( pos < m_activeStackModel.Length, "Evaluation stack overflow in {0}" , m_md );

            return m_activeStackModel[pos];
        }

        internal Expression GetArgumentFromStack( int pos, int tot, TypeRepresentation formalType )
        {
            Expression argument = GetArgumentFromStack( pos, tot );

            if(argument is ConstantExpression)
            {
                argument = CoerceConstantToType( (ConstantExpression)argument, formalType );
            }

            return argument;
        }

        //--//--//--//--//--//--//--//--//--//

        private ConstantExpression CoerceConstantToType( ConstantExpression expr, TypeRepresentation type )
        {
            if (expr.Type != type)
            {
                // Null pointers can be cast to any non-value type.
                if((expr.Value == null) && !(type is ValueTypeRepresentation))
                {
                    return CreateNewNullPointer( type );
                }

                // Scalar literals are validated before MSIL, so we can cast without validating.
                if((type is ScalarTypeRepresentation) && (expr.Value != null))
                {
                    return CreateNewConstant( type, expr.Value );
                }
            }

            return expr;
        }

        private bool CanBeAssignedFromEvaluationStack( TypeRepresentation dst ,
                                                       Expression         src )
        {
            if(src.CanBeNull == CanBeNull.Yes)
            {
                if(dst is ReferenceTypeRepresentation)
                {
                    return true;
                }

                if(dst is PointerTypeRepresentation)
                {
                    return true;
                }
            }

            TypeRepresentation expandedDst = ExpandForEvaluationStack( dst );
            return expandedDst.CanBeAssignedFrom( src.Type, null );
        }

        private TypeRepresentation ExpandForEvaluationStack( TypeRepresentation td )
        {
            if(td is EnumerationTypeRepresentation)
            {
                td = td.UnderlyingType;
            }

            if(td is ScalarTypeRepresentation)
            {
                switch(td.BuiltInType)
                {
                    case TypeRepresentation.BuiltInTypes.BOOLEAN:
                    case TypeRepresentation.BuiltInTypes.I1:
                    case TypeRepresentation.BuiltInTypes.U1:
                    case TypeRepresentation.BuiltInTypes.I2:
                    case TypeRepresentation.BuiltInTypes.U2:
                    case TypeRepresentation.BuiltInTypes.CHAR:
                    case TypeRepresentation.BuiltInTypes.U4:
                        return m_typeSystem.WellKnownTypes.System_Int32;
                }
            }

            return td;
        }

        private ConstantExpression CreateNewNullPointer( TypeRepresentation td )
        {
            return m_typeSystem.CreateNullPointer( td );
        }

        private ConstantExpression CreateNewConstant( TypeRepresentation td    ,
                                                      object             value )
        {
            return m_typeSystem.CreateConstant( td, value );
        }

        private TemporaryVariableExpression CreateNewTemporary( TypeRepresentation td )
        {
            td = ExpandForEvaluationStack( td );
            return m_cfg.AllocateTemporary( td, null );
        }

        private void AddOperator( Operator op )
        {
            m_currentBasicBlock.AddOperator( op );
        }

        private void AddControl( ControlOperator op )
        {
            m_currentBasicBlock.FlowControl = op;
        }
    }
}

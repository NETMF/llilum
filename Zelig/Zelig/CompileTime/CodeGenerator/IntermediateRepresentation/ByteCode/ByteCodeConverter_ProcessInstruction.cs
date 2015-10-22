//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Diagnostics;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;
    using Microsoft.Zelig.Runtime.TypeSystem;

    public sealed partial class ByteCodeConverter
    {
        //
        // State
        //

        private int                   m_variable_arguments_offset; // Static methods always have a fake first argument.
        private VariableExpression[]  m_arguments;
        private VariableExpression[]  m_locals;
        private BitVector             m_addressTaken_arguments;
        private BitVector             m_addressTaken_locals;
        private ControlFlowGraphState m_cfg;
        private BasicBlock            m_exitBasicBlock;
        private BasicBlock            m_currentBasicBlock;
                                     
        private int                   m_mod_unaligned;
        private int                   m_mod_nocheck;
        private TypeRepresentation    m_mod_constrained;
        private bool                  m_mod_fVolatile;
        private bool                  m_mod_fTailCall;
        private bool                  m_mod_fReadonly;

        //--//

        private void PopulateBasicBlocks()
        {
            InitializeBasicBlocks();

            CreateVariables();

            m_exitBasicBlock = m_cfg.NormalizedExitBasicBlock;

            var entryPoint = m_cfg.CreateFirstNormalBasicBlock();

            using(m_cfg.LockFlowInformation())
            {
                CreateBasicBlocks( entryPoint );

                ProcessByteCodeBlocks();
            }
        }

        //--//

        private void CreateVariables()
        {
            m_variable_arguments_offset = (m_md is InstanceMethodRepresentation) ? 0 : 1;
            m_addressTaken_arguments    = new BitVector( m_md.ThisPlusArguments.Length );
            m_addressTaken_locals       = new BitVector( m_localsTypes         .Length );
        }

        //--//

        private void InitializeBasicBlocks()
        {
#if GENERICS_DEBUG
            if(m_md.ToString() == "NonVirtualMethodRepresentation(void generic Microsoft.Zelig.Runtime.KernelNode`1<T>::InsertBefore(delayed Microsoft.Zelig.Runtime.KernelNode`1<T>))")
            {
            }
#endif

            m_cfg               = m_typeSystem.CreateControlFlowGraphState( m_md, m_localsTypes, m_mdDebugInfo != null ? m_mdDebugInfo.LocalVarNames : null, out m_arguments, out m_locals );
            m_currentBasicBlock = null;
        }

        private void CreateBasicBlocks( BasicBlock entryPoint )
        {
            bool fGotFirstBasicBlock = false;

            //
            // First allocate all the basic blocks.
            //
            foreach(ByteCodeBlock current in m_byteCodeBlocks)
            {
                BasicBlock bb;

                if((current.Kind & ByteCodeBlock.Flags.ExceptionHandler) != 0)
                {
                    MetaDataTypeDefinitionAbstract tdCatch = null;

                    foreach(EHClause eh in current.HandlerFor)
                    {
                        if(tdCatch == null)
                        {
                            tdCatch = eh.TypeObject;
                        }
                        else if(tdCatch != eh.TypeObject)
                        {
                            throw TypeConsistencyErrorException.Create( "Multiple incompatible exception handlers share the same byte code for {0}", m_md );
                        }
                    }

                    if(tdCatch != null)
                    {
                        TypeRepresentation td = m_typeSystem.ConvertToIRWithoutContext( tdCatch );

                        current.EhVariable = m_cfg.AllocateExceptionObjectVariable( td );
                    }

                    bb = new ExceptionHandlerBasicBlock( m_cfg );
                }
                else
                {
                    if(!fGotFirstBasicBlock)
                    {
                        fGotFirstBasicBlock = true;

                        //
                        // The entry point already has a control operator towards the exit.
                        // We need to remove it to correctly import the byte code.
                        //
                        entryPoint.FlowControl.Delete();

                        bb = entryPoint;
                    }
                    else
                    {
                        bb = new NormalBasicBlock( m_cfg );
                    }
                }

                current.BasicBlock = bb;
            }

            //
            // Then link them to the exception handlers.
            //
            foreach(ByteCodeBlock current in m_byteCodeBlocks)
            {
                BasicBlock bb = current.BasicBlock;

                foreach(ByteCodeBlock protectedBy in current.ProtectedBy)
                {
                    bb.SetProtectedBy( (ExceptionHandlerBasicBlock)protectedBy.BasicBlock );
                }

                if(bb is ExceptionHandlerBasicBlock)
                {
                    ExceptionHandlerBasicBlock ehBB = (ExceptionHandlerBasicBlock)bb;

                    foreach(EHClause eh in current.HandlerFor)
                    {
                        ExceptionClause ehNew = new ExceptionClause( (ExceptionClause.ExceptionFlag)eh.Flags, m_typeSystem.ConvertToIRWithoutContext( eh.TypeObject ) );

                        ehBB.SetAsHandlerFor( ehNew );
                    }
                }
            }

            //
            // Create proper flow control for exit basic block.
            //
            m_cfg.AddReturnOperator();
        }

        //--//

        private void ProcessByteCodeBlocks()
        {
            //
            // First of all, find all the variable whose address has been taken.
            //
            for(int pos = 0; pos < m_instructions.Length; pos++)
            {
                Instruction instr = m_instructions[pos];

                if(instr.Operator.Action == Instruction.OpcodeAction.LoadAddress)
                {
                    switch(instr.Operator.ActionTarget)
                    {
                        case Instruction.OpcodeActionTarget.Argument:
                            m_addressTaken_arguments.Set( (int)m_byteCodeArguments[pos] + m_variable_arguments_offset );
                            break;

                        case Instruction.OpcodeActionTarget.Local:
                            m_addressTaken_locals.Set( (int)m_byteCodeArguments[pos] );
                            break;
                    }
                }
            }

            for(int pos = 0; pos < m_instructions.Length; pos++)
            {
                ByteCodeBlock current = m_instructionToByteCodeBlock[pos];

                if(current.OffsetOfFirstInstruction == pos)
                {
                    m_currentBasicBlock = current.BasicBlock;

                    Expression[] mergedStackModel = null;

                    if(current.Predecessors.Length > 0)
                    {
                        bool fFoundBackEdge = false;

                        //
                        // Merge incoming flows.
                        //
                        foreach(ByteCodeBlock prev in current.Predecessors)
                        {
                            if(prev.OffsetOfFirstInstruction >= current.OffsetOfFirstInstruction)
                            {
                                fFoundBackEdge = true;
                            }
                            else
                            {
                                Expression[] prevStackModel = prev.ExitStackModel; CHECKS.ASSERT( prevStackModel != null, "" );

                                if(mergedStackModel == null)
                                {
                                    mergedStackModel = ArrayUtility.CopyNotNullArray( prevStackModel );
                                }
                                else
                                {
                                    int depth = mergedStackModel.Length;

                                    if(prevStackModel.Length != depth)
                                    {
                                        throw TypeConsistencyErrorException.Create( "Inconsistent evaluation stack for {0}", m_md );
                                    }

                                    //--//

                                    if(depth > 0)
                                    {
                                        //
                                        // We found at least two flows joining in one point with non-empty evaluation stack.
                                        // So we need to reconcile the stack.
                                        //
                                        // To do that, we find the common type for each slots and create a new temporary variable.
                                        // Then each flow is merged, inserting an extra assignment to the new variable if needed.
                                        //
                                        for(int i = 0; i < depth; i++)
                                        {
                                            Expression exActive = mergedStackModel[i];
                                            Expression exPrev   = prevStackModel  [i];

                                            if(exActive != exPrev)
                                            {
                                                TypeRepresentation tdMerged;

                                                if(CanPromote( exActive, exPrev ))
                                                {
                                                    tdMerged = exPrev.Type;
                                                }
                                                else if(CanPromote( exPrev, exActive ))
                                                {
                                                    tdMerged = exActive.Type;
                                                }
                                                else
                                                {
                                                    tdMerged = TypeRepresentation.FindCommonType( exActive.Type, exPrev.Type );
                                                    if(tdMerged == null)
                                                    {
                                                        throw TypeConsistencyErrorException.Create( "Incompatible evaluation stack for {0}: {1} != {2}", m_md, exActive, exPrev );
                                                    }
                                                }

                                                if(!(exActive is TemporaryVariableExpression) || exActive.Type != tdMerged)
                                                {
                                                    mergedStackModel[i] = CreateNewTemporary( tdMerged );
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(fFoundBackEdge)
                        {
                            if(mergedStackModel != null && mergedStackModel.Length > 0)
                            {
                                throw TypeConsistencyErrorException.Create( "Evaluation stack not empty on backward branch for {0}", m_md );
                            }
                        }
                        else
                        {
                            //
                            // After merging the incoming flows, we need to back-patch the predecessor, adding assignments to the new temporaries.
                            //
                            foreach(ByteCodeBlock prev in current.Predecessors)
                            {
                                Expression[] exitStackModel = prev.ExitStackModel;

                                for(int i = 0; i < exitStackModel.Length; i++)
                                {
                                    if(mergedStackModel[i] != exitStackModel[i])
                                    {
                                        Operator opMerge = SingleAssignmentOperator.New( null, (VariableExpression)mergedStackModel[i], exitStackModel[i] );

                                        prev.BasicBlock.AddOperator( opMerge );
                                    }
                                }

                                prev.ExitStackModel = mergedStackModel;
                            }
                        }
                    }

                    if(mergedStackModel == null)
                    {
                        mergedStackModel = Expression.SharedEmptyArray;
                    }

                    //--//

                    if(m_currentBasicBlock is ExceptionHandlerBasicBlock)
                    {
                        ExceptionHandlerBasicBlock ehBB = (ExceptionHandlerBasicBlock)m_currentBasicBlock;

                        //
                        // Exception handlers start with an header block followed by a normal one,
                        // which contains the actual code.
                        //
                        NormalBasicBlock bb = NormalBasicBlock.CreateWithSameProtection( ehBB );

                        ehBB.AddOperator( UnconditionalControlOperator.New( null, bb ) );

                        m_currentBasicBlock = bb;

                        //--//

                        VariableExpression var = current.EhVariable;
                        if(var != null)
                        {
                            FetchExceptionOperator op = FetchExceptionOperator.New( m_instructions[current.OffsetOfFirstInstruction].DebugInfo, var.Type, var );

                            bb.AddOperator( op );

                            mergedStackModel = new Expression[] { var };
                        }
                    }

                    m_activeStackModel = mergedStackModel;

                    ConvertInstructionToOperators( current );
                }
            }
        }

        private static bool CanPromote( Expression value     ,
                                        Expression reference )
        {
            if(value is ConstantExpression)
            {
                ConstantExpression valueConst = (ConstantExpression)value;

                if(valueConst.Value == null)
                {
                    return true;
                }
            }

            return false;
        }

        //--//--//

        private void ConvertInstructionToOperators( ByteCodeBlock current )
        {
            int pos =       current.OffsetOfFirstInstruction;
            int end = pos + current.NumberOfInstructions;

            ResetModifiers();

            for(;pos < end; pos++)
            {
                Instruction instr = m_instructions[pos];

                this.CurrentInstructionOffset = pos;

                ProcessInstruction( instr );
            }

            current.ExitStackModel = ArrayUtility.CopyNotNullArray( m_activeStackModel );

            if(m_currentBasicBlock.FlowControl == null)
            {
                if(current.Successors.Length != 1)
                {
                    throw TypeConsistencyErrorException.Create( "No flow control operator on basic block with multiple exits, for {0}", m_md );
                }

                m_currentBasicBlock.AddOperator( UnconditionalControlOperator.New( null, current.Successors[0].BasicBlock ) );
            }

            if(m_activeStackModel.Length > 0)
            {
                //
                // Make sure there's no backward branch, since the evaluation stack is not empty.
                //
                foreach(ByteCodeBlock next in current.Successors)
                {
                    if(next.OffsetOfFirstInstruction <= current.OffsetOfFirstInstruction)
                    {
                        throw TypeConsistencyErrorException.Create( "Evaluation stack not empty on backward branch for {0}", m_md );
                    }
                }
            }
        }

        //--//

        private void ResetModifiers()
        {
            m_mod_unaligned   = 0;
            m_mod_nocheck     = 0;
            m_mod_constrained = null;
            m_mod_fVolatile   = false;
            m_mod_fTailCall   = false;
            m_mod_fReadonly   = false;
        }

        //--//

        private void ProcessInstruction( Instruction instr )
        {
            switch(instr.Operator.Action)
            {
                case Instruction.OpcodeAction.Load:
                    ProcessInstruction_Load( instr );
                    break;

                case Instruction.OpcodeAction.LoadAddress:
                    ProcessInstruction_LoadAddress( instr );
                    break;

                case Instruction.OpcodeAction.LoadIndirect:
                    ProcessInstruction_LoadIndirect( instr );
                    break;

                case Instruction.OpcodeAction.LoadElement:
                    ProcessInstruction_LoadElement( instr );
                    break;

                case Instruction.OpcodeAction.LoadElementAddress:
                    ProcessInstruction_LoadElementAddress( instr );
                    break;

                case Instruction.OpcodeAction.Store:
                    ProcessInstruction_Store( instr );
                    break;

                case Instruction.OpcodeAction.StoreIndirect:
                    ProcessInstruction_StoreIndirect( instr );
                    break;

                case Instruction.OpcodeAction.StoreElement:
                    ProcessInstruction_StoreElement( instr );
                    break;

                case Instruction.OpcodeAction.Stack:
                    ProcessInstruction_Stack( instr );
                    break;

                case Instruction.OpcodeAction.Jump:
                    ProcessInstruction_Jump( instr );
                    break;

                case Instruction.OpcodeAction.Call:
                    ProcessInstruction_Call( instr );
                    break;

                case Instruction.OpcodeAction.Return:
                    ProcessInstruction_Return( instr );
                    break;

                case Instruction.OpcodeAction.Branch:
                    ProcessInstruction_Branch( instr );
                    break;

                case Instruction.OpcodeAction.ALU:
                    ProcessInstruction_ALU( instr );
                    break;

                case Instruction.OpcodeAction.Convert:
                    ProcessInstruction_Convert( instr );
                    break;

                case Instruction.OpcodeAction.Object:
                    ProcessInstruction_Object( instr );
                    break;

                case Instruction.OpcodeAction.TypedRef:
                    ProcessInstruction_TypedRef( instr );
                    break;

                case Instruction.OpcodeAction.EH:
                    ProcessInstruction_ExceptionHandling( instr );
                    break;

                case Instruction.OpcodeAction.Set:
                    ProcessInstruction_Set( instr );
                    break;

                case Instruction.OpcodeAction.Modifier:
                    ProcessInstruction_Modifier( instr );
                    return; // Return directly, because the code after the switch resets the modifiers.

                case Instruction.OpcodeAction.Debug:
                    ProcessInstruction_Debug( instr );
                    break;
            }

            ResetModifiers();
        }

        //--//

        //
        // LDARG           = Action_Load               | Target_Argument                         ,
        // LDLOC           = Action_Load               | Target_Local                            ,
        // LDNULL          = Action_Load               | Target_Null                             ,
        // LDC             = Action_Load               | Target_Constant                         ,
        // LDSTR           = Action_Load               | Target_String                           ,
        // LDFLD           = Action_Load               | Target_Field                            ,
        // LDSFLD          = Action_Load               | Target_StaticField                      ,
        // LDTOKEN         = Action_Load               | Target_Token                            ,
        // LDFTN           = Action_Load               | Target_Method                           ,
        // LDVIRTFTN       = Action_Load               | Target_Method                           ,
        //
        private void ProcessInstruction_Load( Instruction instr )
        {
            if(m_mod_fVolatile)
            {
                // TODO: Implement the modifier.
            }

            TypeRepresentation  tdVal;
            FieldRepresentation fdVal;
            Expression          exVal = ProcessInstruction_GetTypeOfActionTarget( instr, out tdVal, out fdVal );

            //
            // Don't create a temporary variable if the source is a constant value.
            //
            if(exVal is ConstantExpression)
            {
                PushStackModel( exVal );
                return;
            }

            switch(instr.Operator.ActionTarget)
            {
                case Instruction.OpcodeActionTarget.Field:
                    {
                        VariableExpression exRes = CreateNewTemporary( tdVal );
                        Expression         exObj = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation tdObj = exObj.Type;

                        if(tdObj.GetInstantiationFlavor( m_typeSystem ) == TypeRepresentation.InstantiationFlavor.Delayed)
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot access the fields of delayed type {0}", tdObj );
                        }

                        ModifyStackModel( 1, exRes );

                        //
                        // This happens when we reference a value type on the evaluation stack.
                        // Normally it's the result of a previous operation that is not stored in a local variable.
                        //
                        if(tdObj is ValueTypeRepresentation)
                        {
                            VariableExpression exResPtr = CreateNewTemporary( m_typeSystem.CreateManagedPointerToType( tdObj ) );

                            AddOperator( AddressAssignmentOperator.New( instr.DebugInfo, exResPtr, exObj ) );

                            exObj = exResPtr;
                        }

                        AddOperator( LoadInstanceFieldOperator.New( instr.DebugInfo, fdVal, exRes, (VariableExpression)exObj, true ) );
                    }
                    break;

                case Instruction.OpcodeActionTarget.StaticField:
                    {
                        VariableExpression exRes = CreateNewTemporary( tdVal );

                        PushStackModel( exRes );

                        AddOperator( LoadStaticFieldOperator.New( instr.DebugInfo, fdVal, exRes ) );
                    }
                    break;

                default:
                    // 1.8.1.5 Delegate constructors
                    //     The verification algorithm shall require that one of the following code sequences is used for constructing
                    //     delegates; no other code sequence in verifiable code shall contain a newobj instruction for a delegate type.
                    //     There shall be only one instance constructor method for a delegate (overloading is not allowed)
                    //     The verification algorithm shall fail if a branch target is within these instruction sequences (other than at the
                    //     start of the sequence).
                    //     [Note: See Partition II for the signature of delegates and a validity requirement regarding the signature of the
                    //     method used in the constructor and the signature of Invoke and other methods on the delegate class. end note]
                    //
                    // 1.8.1.5.1 Delegating via virtual dispatch
                    //     The following CIL instruction sequence shall be used or the verification algorithm shall fail. The sequence
                    //     begins with an object on the stack.
                    //
                    //         dup
                    //         ldvirtftn mthd ; Method shall be on the class of the object,
                    //         ; or one of its parent classes, or an interface
                    //         ; implemented by the object
                    //         newobj delegateclass::.ctor(object, native int)
                    //
                    //     [Rationale: The dup is required to ensure that it is precisely the same object stored in the delegate as was used
                    //     to compute the virtual method. If another object of a subtype were used the object and the method wouldn’t
                    //     match and could lead to memory violations. end rationale]
                    //
                    // 1.8.1.5.2 Delegating via instance dispatch
                    //     The following CIL instruction sequence shall be used or the verification algorithm shall fail. The sequence
                    //     begins with either null or an object on the stack.
                    //
                    //         ldftn mthd ; Method shall either be a static method or
                    //         ; a method on the class of the object on the stack or
                    //         ; one of the object’s parent classes
                    //         newobj delegateclass::.ctor(object, native int)
                    {
                        Operator opNew = null;

                        if(tdVal.IsNumeric)
                        {
                            TypeRepresentation tdValInner = tdVal.UnderlyingType;
                            if(tdValInner != tdVal)
                            {
                                VariableExpression exRes = CreateNewTemporary( tdVal );
                                PushStackModel( exRes );

                                if(tdValInner.IsSigned)
                                {
                                    opNew = SignExtendOperator.New( instr.DebugInfo, tdValInner.Size, false, exRes, exVal );
                                }
                                else
                                {
                                    opNew = ZeroExtendOperator.New( instr.DebugInfo, tdValInner.Size, false, exRes, exVal );
                                }

                                AddOperator( opNew );
                            }
                        }

                        if(opNew == null)
                        {
#if FORCE_LOCAL_EXPANSION
                            // Expand the loaded argument into a new temporary.
                            VariableExpression exRes = CreateNewTemporary( tdVal );
                            PushStackModel( exRes );
                            AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, exVal ) );
#else // FORCE_LOCAL_EXPANSION
                            PushStackModel( exVal );
#endif // FORCE_LOCAL_EXPANSION
                        }
                    }
                    break;
            }
        }

        //
        // LDARGA          = Action_LoadAddress        | Target_Argument                         ,
        // LDLOCA          = Action_LoadAddress        | Target_Local                            ,
        // LDFLDA          = Action_LoadAddress        | Target_Field                            ,
        // LDSFLDA         = Action_LoadAddress        | Target_StaticField                      ,
        //
        private void ProcessInstruction_LoadAddress( Instruction instr )
        {
            TypeRepresentation  tdVal;
            FieldRepresentation fdVal;
            Expression          exVal = ProcessInstruction_GetTypeOfActionTarget( instr, out tdVal, out fdVal );
            VariableExpression  exRes = CreateNewTemporary( m_typeSystem.CreateManagedPointerToType( tdVal ) );

            switch(instr.Operator.ActionTarget)
            {
                case Instruction.OpcodeActionTarget.Field:
                    {
                        Expression         exObj = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation tdObj = exObj.Type;

                        if(tdObj.GetInstantiationFlavor( m_typeSystem ) == TypeRepresentation.InstantiationFlavor.Delayed)
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot access the field of delayed type {0}", tdObj );
                        }

                        ModifyStackModel( 1, exRes );

                        AddOperator( LoadInstanceFieldAddressOperator.New( instr.DebugInfo, fdVal, exRes, (VariableExpression)exObj, true ) );
                    }
                    break;

                case Instruction.OpcodeActionTarget.StaticField:
                    {
                        PushStackModel( exRes );

                        AddOperator( LoadStaticFieldAddressOperator.New( instr.DebugInfo, fdVal, exRes ) );
                    }
                    break;

                default:
                    {
                        PushStackModel( exRes );

                        AddOperator( AddressAssignmentOperator.New( instr.DebugInfo, exRes, exVal ) );
                    }
                    break;
            }
        }

        //
        // LDIND_I1        = Action_LoadIndirect       | Type_I1                                 ,
        // LDIND_U1        = Action_LoadIndirect       | Type_U1                                 ,
        // LDIND_I2        = Action_LoadIndirect       | Type_I2                                 ,
        // LDIND_U2        = Action_LoadIndirect       | Type_U2                                 ,
        // LDIND_I4        = Action_LoadIndirect       | Type_I4                                 ,
        // LDIND_U4        = Action_LoadIndirect       | Type_U4                                 ,
        // LDIND_I8        = Action_LoadIndirect       | Type_I8                                 ,
        // LDIND_I         = Action_LoadIndirect       | Type_I                                  ,
        // LDIND_R4        = Action_LoadIndirect       | Type_R4                                 ,
        // LDIND_R8        = Action_LoadIndirect       | Type_R8                                 ,
        // LDIND_REF       = Action_LoadIndirect       | Type_Reference                          ,
        //
        private void ProcessInstruction_LoadIndirect( Instruction instr )
        {
            Expression exAddr = GetArgumentFromStack( 0, 1 );

            AddOperator( ProcessInstruction_LoadIndirect( instr, exAddr, null ) );
        }

        //
        // LDELEM_I1       = Action_LoadElement        | Type_I1                                 ,
        // LDELEM_U1       = Action_LoadElement        | Type_U1                                 ,
        // LDELEM_I2       = Action_LoadElement        | Type_I2                                 ,
        // LDELEM_U2       = Action_LoadElement        | Type_U2                                 ,
        // LDELEM_I4       = Action_LoadElement        | Type_I4                                 ,
        // LDELEM_U4       = Action_LoadElement        | Type_U4                                 ,
        // LDELEM_I8       = Action_LoadElement        | Type_I8                                 ,
        // LDELEM_I        = Action_LoadElement        | Type_I                                  ,
        // LDELEM_R4       = Action_LoadElement        | Type_R4                                 ,
        // LDELEM_R8       = Action_LoadElement        | Type_R8                                 ,
        // LDELEM_REF      = Action_LoadElement        | Type_Reference                          ,
        // LDELEM          = Action_LoadElement        | Type_Token                              ,
        //
        private void ProcessInstruction_LoadElement( Instruction instr )
        {
            Expression         exAddr  = GetArgumentFromStack( 0, 2 );
            Expression         exIndex = GetArgumentFromStack( 1, 2 );
            TypeRepresentation td      = exAddr.Type;

            if(td is SzArrayReferenceTypeRepresentation)
            {
                SzArrayReferenceTypeRepresentation tdArray   = (SzArrayReferenceTypeRepresentation)td;
                TypeRepresentation                 tdElement = tdArray.ContainedType;
                VariableExpression                 exRes     = CreateNewTemporary( tdElement );
                TypeRepresentation                 tdFormal  = ProcessInstruction_GetTypeFromActionType( instr, exRes.Type );

                if(tdFormal.CanBeAssignedFrom( tdElement, null ) == false)
                {
                    throw TypeConsistencyErrorException.Create( "Expecting array type {0}, got {1}", tdFormal, exRes.Type );
                }

                ModifyStackModel( 2, exRes );

                AddOperator( LoadElementOperator.New( instr.DebugInfo, exRes, exAddr, exIndex, null, true ) );
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Expecting zero-based array type, got {0}", td );
            }
        }

        //
        // LDELEMA         = Action_LoadElementAddress | Type_Token                              ,
        //
        private void ProcessInstruction_LoadElementAddress( Instruction instr )
        {
            Expression         exAddr  = GetArgumentFromStack( 0, 2 );
            Expression         exIndex = GetArgumentFromStack( 1, 2 );
            TypeRepresentation td      = exAddr.Type;

            if(m_mod_fReadonly)
            {
                // TODO: Implement the modifier.
            }

            if(td is SzArrayReferenceTypeRepresentation)
            {
                SzArrayReferenceTypeRepresentation tdArray   = (SzArrayReferenceTypeRepresentation)td;
                TypeRepresentation                 tdElement = ProcessInstruction_GetTypeFromActionType( instr, null );
                VariableExpression                 exRes     = CreateNewTemporary( m_typeSystem.CreateManagedPointerToType( tdElement ) );

                if(tdArray.ContainedType != tdElement)
                {
                    throw TypeConsistencyErrorException.Create( "Expecting array type {0}, got {1}", tdElement, tdArray.ContainedType );
                }

                ModifyStackModel( 2, exRes );

                AddOperator( LoadElementAddressOperator.New( instr.DebugInfo, exRes, exAddr, exIndex, null, true ) );
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Expecting zero-based array type, got {0}", td );
            }
        }

        //
        // STARG           = Action_Store              | Target_Argument                         ,
        // STLOC           = Action_Store              | Target_Local                            ,
        // STFLD           = Action_Store              | Target_Field                            ,
        // STSFLD          = Action_Store              | Target_StaticField                      ,
        //
        private void ProcessInstruction_Store( Instruction instr )
        {
            TypeRepresentation  tdTarget;
            FieldRepresentation fdTarget;
            Expression          exTarget = ProcessInstruction_GetTypeOfActionTarget( instr, out tdTarget, out fdTarget );


            switch(instr.Operator.ActionTarget)
            {
                case Instruction.OpcodeActionTarget.Field:
                    {
                        Expression         exObj = GetArgumentFromStack( 0, 2 );
                        Expression         exVal = GetArgumentFromStack( 1, 2 );
                        TypeRepresentation tdObj = exObj.Type;

                        if(tdObj.GetInstantiationFlavor( m_typeSystem ) == TypeRepresentation.InstantiationFlavor.Delayed)
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot access the fields of delayed type {0}", tdObj );
                        }

                        PopStackModel( 2 );

                        AddOperator( StoreInstanceFieldOperator.New( instr.DebugInfo, fdTarget, exObj, exVal, true ) );
                    }
                    break;

                case Instruction.OpcodeActionTarget.StaticField:
                    {
                        Expression exVal = GetArgumentFromStack( 0, 1 );

                        PopStackModel( 1 );

                        AddOperator( StoreStaticFieldOperator.New( instr.DebugInfo, fdTarget, exVal ) );
                    }
                    break;

                default:
                    {
                        Expression exVal = GetArgumentFromStack( 0, 1 );

                        PopStackModel( 1 );

                        AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, (VariableExpression)exTarget, exVal ) );
                    }
                    break;
            }
        }

        //
        // STIND_REF       = Action_StoreIndirect      | Type_Reference                          ,
        // STIND_I1        = Action_StoreIndirect      | Type_I1                                 ,
        // STIND_I2        = Action_StoreIndirect      | Type_I2                                 ,
        // STIND_I4        = Action_StoreIndirect      | Type_I4                                 ,
        // STIND_I8        = Action_StoreIndirect      | Type_I8                                 ,
        // STIND_R4        = Action_StoreIndirect      | Type_R4                                 ,
        // STIND_R8        = Action_StoreIndirect      | Type_R8                                 ,
        // STIND_I         = Action_StoreIndirect      | Type_I                                  ,
        //
        private void ProcessInstruction_StoreIndirect( Instruction instr )
        {
            Expression exAddr  = GetArgumentFromStack( 0, 2 );
            Expression exValue = GetArgumentFromStack( 1, 2 );

            AddOperator( ProcessInstruction_StoreIndirect( instr, exAddr, exValue, null ) );
        }

        //
        // STELEM_I        = Action_StoreElement       | Type_I                                  ,
        // STELEM_I1       = Action_StoreElement       | Type_I1                                 ,
        // STELEM_I2       = Action_StoreElement       | Type_I2                                 ,
        // STELEM_I4       = Action_StoreElement       | Type_I4                                 ,
        // STELEM_I8       = Action_StoreElement       | Type_I8                                 ,
        // STELEM_R4       = Action_StoreElement       | Type_R4                                 ,
        // STELEM_R8       = Action_StoreElement       | Type_R8                                 ,
        // STELEM_REF      = Action_StoreElement       | Type_Reference                          ,
        // STELEM          = Action_StoreElement       | Type_Token                              ,
        //
        private void ProcessInstruction_StoreElement( Instruction instr )
        {
            Expression         exAddr  = GetArgumentFromStack( 0, 3 );
            Expression         exIndex = GetArgumentFromStack( 1, 3 );
            Expression         exValue = GetArgumentFromStack( 2, 3 );
            TypeRepresentation td      = exAddr.Type;

            if(td is SzArrayReferenceTypeRepresentation)
            {
                SzArrayReferenceTypeRepresentation tdArray  = (SzArrayReferenceTypeRepresentation)td;
                TypeRepresentation                 tdFormal = ProcessInstruction_GetTypeFromActionType( instr, tdArray.ContainedType );

                if(exValue is ConstantExpression)
                {
                    exValue = CoerceConstantToType( (ConstantExpression)exValue, tdFormal );
                }

                if(!CanBeAssignedFromEvaluationStack( tdFormal, exValue ))
                {
                    throw TypeConsistencyErrorException.Create( "Expecting array type {0}, got {1}", tdFormal, exValue.Type );
                }

                PopStackModel( 3 );

                AddOperator( StoreElementOperator.New( instr.DebugInfo, exAddr, exIndex, exValue, null, true ) );
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Expecting zero-based array type, got {0}", td );
            }
        }

        //
        // DUP             = Action_Stack              | Stack_Dup                               ,
        // POP             = Action_Stack              | Stack_Pop                               ,
        // LOCALLOC        = Action_Stack              | Stack_LocAlloc                          ,
        //
        private void ProcessInstruction_Stack( Instruction instr )
        {
            switch(instr.Operator.ActionStack)
            {
                case Instruction.OpcodeActionStack.Dup:
                    {
                        Expression         ex    = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes = CreateNewTemporary( ex.Type );

                        PushStackModel( exRes );

                        AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, ex ) );
                    }
                    break;

                case Instruction.OpcodeActionStack.Pop:
                    {
                        PopStackModel( 1 );

                        AddOperator( NopOperator.New( instr.DebugInfo ) );
                    }
                    break;

                case Instruction.OpcodeActionStack.LocAlloc:
                    {
                        Expression         exSize = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes  = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_IntPtr );

                        ModifyStackModel( 1, exRes );

                        AddOperator( StackAllocationOperator.New( instr.DebugInfo, exRes, exSize ) );
                    }
                    break;

                case Instruction.OpcodeActionStack.ArgList:
                    {
                        VariableExpression exRes = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_RuntimeArgumentHandle );

                        PushStackModel( exRes );

                        AddOperator( ArgListOperator.New( instr.DebugInfo, exRes ) );
                    }
                    break;

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionStack {0} for {1}", instr.Operator.ActionStack, instr );
                    }
            }
        }

        //
        // JMP             = Action_Jump                                                         ,
        //
        private void ProcessInstruction_Jump( Instruction instr )
        {
            throw new NotImplementedException();
        }

        //
        // CALL            = Action_Call               | Call_Direct                             ,
        // CALLI           = Action_Call               | Call_Indirect                           ,
        // CALLVIRT        = Action_Call               | Call_Virtual                            ,
        //
        private void ProcessInstruction_Call( Instruction instr )
        {
            if(m_mod_fTailCall)
            {
                // TODO: Implement the modifier.
            }

            if(m_mod_constrained != null)
            {
                // TODO: Implement the modifier.
            }

            switch(instr.Operator.ActionCall)
            {
                case Instruction.OpcodeActionCall.Direct:
                case Instruction.OpcodeActionCall.Virtual:
                    {
                        MethodRepresentation md        = this.CurrentArgumentAsMethod;
                        TypeRepresentation[] args      = md.ThisPlusArguments;
                        int                  argsNum   = args.Length;
                        bool                 fInstance = md is InstanceMethodRepresentation;
                        int                  skip      = fInstance ? 0 : 1; // Static methods has an extra slot in the Arguments array.
                        Expression[]         exActual  = new Expression[argsNum];
                        VariableExpression   exRes;

                        if(fInstance)
                        {
                            TypeRepresentation tdThis = args[0];

                            exActual[0] = GetArgumentFromStack( 0, argsNum );

                            if(m_mod_constrained != null)
                            {
                                ManagedPointerTypeRepresentation tdActual2 = exActual[0].Type as ManagedPointerTypeRepresentation;

                                if(tdActual2 == null || tdActual2.ContainedType != m_mod_constrained)
                                {
                                    throw TypeConsistencyErrorException.Create( "Incorrect 'this' argument on constrained call to {0} from {1}: got {2}, expecting {3}", md, m_md, exActual[0].Type, md.OwnerType );
                                }

                                TypeRepresentation tdActual3 = tdActual2.ContainedType;

                                if(m_mod_constrained is ReferenceTypeRepresentation)
                                {
                                    VariableExpression ex = CreateNewTemporary( m_mod_constrained );

                                    AddOperator( LoadIndirectOperator.New( instr.DebugInfo, m_mod_constrained, ex, exActual[0], null, 0, false, true ) );

                                    exActual[0] = ex;
                                }
                                else
                                {
                                    MethodRepresentation md2 = tdActual3.FindMatch( md, null );

                                    if(md2 != null)
                                    {
                                        md = md2;

                                        args   = md.ThisPlusArguments;
                                        tdThis = args[0];
                                    }
                                    else
                                    {
                                        TypeRepresentation tdBoxed = m_typeSystem.CreateBoxedValueType( (ValueTypeRepresentation)m_mod_constrained );
                                        VariableExpression ex      = CreateNewTemporary( m_mod_constrained );

                                        AddOperator( LoadIndirectOperator.New( instr.DebugInfo, m_mod_constrained, ex, exActual[0], null, 0, false, true ) );

                                        exRes = CreateNewTemporary( tdBoxed );

                                        AddOperator( BoxOperator.New( instr.DebugInfo, tdBoxed, exRes, ex ) );

                                        exActual[0] = exRes;
                                    }
                                }
                            }

                            if(!CanBeAssignedFromEvaluationStack( tdThis, exActual[0] ))
                            {
                                throw TypeConsistencyErrorException.Create( "Incorrect 'this' argument on call to {0} from {1}: got {2}, expecting {3}", md, m_md, exActual[0].Type, md.OwnerType );
                            }
                        }
                        else
                        {
                            exActual[0] = CreateNewNullPointer( args[0] );
                        }

                        for(int i = 1; i < argsNum; i++)
                        {
                            exActual[i] = GetArgumentFromStack( i - 1, argsNum - 1, args[i] );

                            if(!CanBeAssignedFromEvaluationStack( args[i], exActual[i] ))
                            {
                                throw TypeConsistencyErrorException.Create( "Incorrect argument {0} on call to {1} from {2}: got {3}, expecting {4}", i - 1, md, m_md, exActual[i].Type, args[i] );
                            }
                        }

                        CallOperator.CallKind callType;

                        if(instr.Operator.ActionCall == Instruction.OpcodeActionCall.Direct)
                        {
                            callType = CallOperator.CallKind.Direct;
                        }
                        else
                        {
                            callType = CallOperator.CallKind.Virtual;
                        }

                        if(md.ReturnType == m_typeSystem.WellKnownTypes.System_Void)
                        {
                            exRes = null;

                            PopStackModel( argsNum - skip );
                        }
                        else
                        {
                            exRes = CreateNewTemporary( md.ReturnType );

                            ModifyStackModel( argsNum - skip, exRes );
                        }

                        var lhs = VariableExpression.ToArray( exRes );

                        if(fInstance)
                        {
                            AddOperator( InstanceCallOperator.New( instr.DebugInfo, callType, md, lhs, exActual, true ) );
                        }
                        else
                        {
                            AddOperator( StaticCallOperator.New( instr.DebugInfo, callType, md, lhs, exActual ) );
                        }
                    }
                    break;

                case Instruction.OpcodeActionCall.Indirect:
                    {
                        throw new NotImplementedException();
                    }

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionCall {0} for {1}", instr.Operator.ActionCall, instr );
                    }
            }
        }

        //
        // RET             = Action_Return                                                       ,
        //
        private void ProcessInstruction_Return( Instruction instr )
        {
            if(m_cfg.ReturnValue == null)
            {
                if(m_activeStackModel.Length != 0)
                {
                    throw TypeConsistencyErrorException.Create( "Incorrect evaluation stack on method exit for {0}", m_md );
                }
            }
            else
            {
                if(m_activeStackModel.Length != 1)
                {
                    throw TypeConsistencyErrorException.Create( "Incorrect evaluation stack on method exit for {0}", m_md );
                }

                Expression exRes = GetArgumentFromStack( 0, 1 );

                PopStackModel( 1 );

                if(!CanBeAssignedFromEvaluationStack( m_cfg.ReturnValue.Type, exRes ))
                {
                    throw TypeConsistencyErrorException.Create( "Incorrect return type on evaluation stack on method exit for {0}", m_md );
                }

                AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, m_cfg.ReturnValue, exRes ) );
            }

            AddControl( UnconditionalControlOperator.New( instr.DebugInfo, m_exitBasicBlock ) );
        }

        //
        // BR              = Action_Branch             | Cond_ALWAYS                             ,
        // BRFALSE         = Action_Branch             | Cond_FALSE                              ,
        // BRTRUE          = Action_Branch             | Cond_TRUE                               ,
        // BEQ             = Action_Branch             | Cond_EQ                                 ,
        // BGE             = Action_Branch             | Cond_GE                                 ,
        // BGT             = Action_Branch             | Cond_GT                                 ,
        // BLE             = Action_Branch             | Cond_LE                                 ,
        // BLT             = Action_Branch             | Cond_LT                                 ,
        // BNE_UN          = Action_Branch             | Cond_NE            | Unsigned           ,
        // BGE_UN          = Action_Branch             | Cond_GE            | Unsigned           ,
        // BGT_UN          = Action_Branch             | Cond_GT            | Unsigned           ,
        // BLE_UN          = Action_Branch             | Cond_LE            | Unsigned           ,
        // BLT_UN          = Action_Branch             | Cond_LT            | Unsigned           ,
        // SWITCH          = Action_Branch             | Cond_Val                                ,
        //
        private void ProcessInstruction_Branch( Instruction instr )
        {
            Instruction.OpcodeActionCondition cond = instr.Operator.ActionCondition;
            ControlOperator                   op;
            BasicBlock                        targetBranchNotTaken = GetBasicBlockFromInstruction( this.CurrentInstructionOffset + 1 );

            switch(cond)
            {
                case Instruction.OpcodeActionCondition.ALWAYS:
                    {
                        BasicBlock targetBranchTaken = GetBasicBlockFromInstruction( this.CurrentArgumentAsInt32 );

                        op = UnconditionalControlOperator.New( instr.DebugInfo, targetBranchTaken );
                    }
                    break;

                case Instruction.OpcodeActionCondition.TRUE:
                case Instruction.OpcodeActionCondition.FALSE:
                    {
                        Expression exVal             = GetArgumentFromStack( 0, 1 );
                        BasicBlock targetBranchTaken = GetBasicBlockFromInstruction( this.CurrentArgumentAsInt32 );

                        PopStackModel( 1 );

                        if(cond == Instruction.OpcodeActionCondition.FALSE)
                        {
                            BasicBlock tmp;

                            tmp                  = targetBranchNotTaken;
                            targetBranchNotTaken = targetBranchTaken;
                            targetBranchTaken    = tmp;
                        }

                        op = BinaryConditionalControlOperator.New( instr.DebugInfo, exVal, targetBranchNotTaken, targetBranchTaken );
                    }
                    break;

                case Instruction.OpcodeActionCondition.Val:
                    {
                        Expression   exVal   = GetArgumentFromStack( 0, 1 );
                        int[]        offsets = (int[])this.CurrentArgument;
                        BasicBlock[] targets = new BasicBlock[offsets.Length];

                        for(int i = 0; i < offsets.Length; i++)
                        {
                            targets[i] = GetBasicBlockFromInstruction( offsets[i] );
                        }


                        PopStackModel( 1 );

                        op = MultiWayConditionalControlOperator.New( instr.DebugInfo, exVal, targetBranchNotTaken, targets );
                    }
                    break;

                default:
                    {
                        Expression         exLeft            = GetArgumentFromStack( 0, 2 );
                        Expression         exRight           = GetArgumentFromStack( 1, 2 );
                        VariableExpression exRes             = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_Int32 );
                        BasicBlock         targetBranchTaken = GetBasicBlockFromInstruction( this.CurrentArgumentAsInt32 );

                        PopStackModel( 2 );

                        //
                        // Section 1.5 of ECMA spec, Partition III
                        //
                        StackEquivalentType seLeft  = exLeft .StackEquivalentType;
                        StackEquivalentType seRight = exRight.StackEquivalentType;
                        if(seLeft != seRight)
                        {
                            bool fIsPointerLeft  = (seLeft  == StackEquivalentType.Pointer || seLeft  == StackEquivalentType.NativeInt);
                            bool fIsPointerRight = (seRight == StackEquivalentType.Pointer || seRight == StackEquivalentType.NativeInt);

                            //
                            // The ECMA spec says that you can only compare Pointer and NativeInt for equality, but the framework does otherwise...
                            //
                            if(fIsPointerLeft != fIsPointerRight)
                            {
                                throw TypeConsistencyErrorException.Create( "Condition branch operation '{0}' applied to incompatible types: {1} != {2}", instr, exLeft.Type, exRight.Type );
                            }
                        }

                        CompareAndSetOperator.ActionCondition cond2;

                        switch(cond)
                        {
                            case Instruction.OpcodeActionCondition.EQ: cond2 = CompareAndSetOperator.ActionCondition.EQ; break;
                            case Instruction.OpcodeActionCondition.GE: cond2 = CompareAndSetOperator.ActionCondition.GE; break;
                            case Instruction.OpcodeActionCondition.GT: cond2 = CompareAndSetOperator.ActionCondition.GT; break;
                            case Instruction.OpcodeActionCondition.LE: cond2 = CompareAndSetOperator.ActionCondition.LE; break;
                            case Instruction.OpcodeActionCondition.LT: cond2 = CompareAndSetOperator.ActionCondition.LT; break;
                            case Instruction.OpcodeActionCondition.NE: cond2 = CompareAndSetOperator.ActionCondition.NE; break;
                            default: throw TypeConsistencyErrorException.Create( "Invalid condition {0} for branch operation '{1}'", cond, instr );
                        }

                        AddOperator( CompareAndSetOperator.New( instr.DebugInfo, cond2, instr.Operator.IsSigned, exRes, exLeft, exRight ) );

                        op = BinaryConditionalControlOperator.New( instr.DebugInfo, exRes, targetBranchNotTaken, targetBranchTaken );
                    }
                    break;
            }

            AddControl( op );
        }

        //
        // NOP             = Action_ALU                | Op_NOP                                  ,
        // ADD             = Action_ALU                | Op_ADD                                  ,
        // SUB             = Action_ALU                | Op_SUB                                  ,
        // MUL             = Action_ALU                | Op_MUL                                  ,
        // DIV             = Action_ALU                | Op_DIV                                  ,
        // DIV_UN          = Action_ALU                | Op_DIV             | Unsigned           ,
        // REM             = Action_ALU                | Op_REM                                  ,
        // REM_UN          = Action_ALU                | Op_REM             | Unsigned           ,
        // AND             = Action_ALU                | Op_AND                                  ,
        // OR              = Action_ALU                | Op_OR                                   ,
        // XOR             = Action_ALU                | Op_XOR                                  ,
        // SHL             = Action_ALU                | Op_SHL                                  ,
        // SHR             = Action_ALU                | Op_SHR                                  ,
        // SHR_UN          = Action_ALU                | Op_SHR             | Unsigned           ,
        // NEG             = Action_ALU                | Op_NEG                                  ,
        // NOT             = Action_ALU                | Op_NOT                                  ,
        // CKFINITE        = Action_ALU                | Op_FINITE                               ,
        // ADD_OVF         = Action_ALU                | Op_ADD                        | Overflow,
        // ADD_OVF_UN      = Action_ALU                | Op_ADD             | Unsigned | Overflow,
        // MUL_OVF         = Action_ALU                | Op_MUL                        | Overflow,
        // MUL_OVF_UN      = Action_ALU                | Op_MUL             | Unsigned | Overflow,
        // SUB_OVF         = Action_ALU                | Op_SUB                        | Overflow,
        // SUB_OVF_UN      = Action_ALU                | Op_SUB             | Unsigned | Overflow,
        //
        private void ProcessInstruction_ALU( Instruction instr )
        {
            Instruction.OpcodeActionALU alu = instr.Operator.ActionALU;

            switch(alu)
            {
                case Instruction.OpcodeActionALU.ADD:
                case Instruction.OpcodeActionALU.SUB:
                case Instruction.OpcodeActionALU.MUL:
                case Instruction.OpcodeActionALU.DIV:
                case Instruction.OpcodeActionALU.REM:
                case Instruction.OpcodeActionALU.AND:
                case Instruction.OpcodeActionALU.OR:
                case Instruction.OpcodeActionALU.XOR:
                case Instruction.OpcodeActionALU.SHL:
                case Instruction.OpcodeActionALU.SHR:
                    {
                        Expression          exLeft  = GetArgumentFromStack( 0, 2 );
                        Expression          exRight = GetArgumentFromStack( 1, 2 );
                        TypeRepresentation  tdRes   = null;
                        StackEquivalentType seLeft  = exLeft .StackEquivalentType;
                        StackEquivalentType seRight = exRight.StackEquivalentType;
                        bool                fAddSub = alu == Instruction.OpcodeActionALU.ADD ||
                                                      alu == Instruction.OpcodeActionALU.SUB;
                        bool                fShift  = alu == Instruction.OpcodeActionALU.SHL ||
                                                      alu == Instruction.OpcodeActionALU.SHR;

                        //
                        // Section 1.5 of ECMA spec, Partition III
                        //
                        switch(seLeft)
                        {
                            case StackEquivalentType.Int32:
                                switch(seRight)
                                {
                                    case StackEquivalentType.Int32:
                                        tdRes = exLeft.Type;
                                        break;

                                    case StackEquivalentType.NativeInt:
                                        tdRes = exRight.Type;
                                        break;

                                    case StackEquivalentType.Pointer:
                                        if(alu == Instruction.OpcodeActionALU.ADD)
                                        {
                                            tdRes = exRight.Type;
                                        }
                                        break;
                                }
                                break;

                            case StackEquivalentType.Int64:
                                switch(seRight)
                                {
                                    case StackEquivalentType.Int32:
                                        if(fShift)
                                        {
                                            tdRes = exLeft.Type;
                                        }
                                        break;

                                    case StackEquivalentType.Int64:
                                        tdRes = exLeft.Type;
                                        break;
                                }
                                break;

                            case StackEquivalentType.NativeInt:
                                switch(seRight)
                                {
                                    case StackEquivalentType.Int32:
                                        tdRes = exLeft.Type;
                                        break;

                                    case StackEquivalentType.NativeInt:
                                        tdRes = exLeft.Type;
                                        break;

                                    case StackEquivalentType.Pointer:
                                        if(fAddSub)
                                        {
                                            //
                                            // The SUB case seems to be prohibited by the ECMA spec, but it occurs in unsafe code.
                                            //
                                            tdRes = exRight.Type;
                                        }
                                        break;
                                }
                                break;

                            case StackEquivalentType.Float:
                                switch(seRight)
                                {
                                    case StackEquivalentType.Float:
                                        tdRes = exLeft.Type;
                                        break;
                                }
                                break;

                            case StackEquivalentType.Pointer:
                                switch(seRight)
                                {
                                    case StackEquivalentType.Int32:
                                        if(alu == Instruction.OpcodeActionALU.DIV)
                                        {
                                            //
                                            // This case seems to be prohibited by the ECMA spec, but it occurs in unsafe code, see System.String.revmemcpyimpl.
                                            //
                                            tdRes = m_typeSystem.WellKnownTypes.System_IntPtr;
                                        }
                                        else if(fAddSub)
                                        {
                                            tdRes = exLeft.Type;
                                        }
                                        break;

                                    case StackEquivalentType.NativeInt:
                                        if(fAddSub)
                                        {
                                            tdRes = exLeft.Type;
                                        }
                                        break;

                                    case StackEquivalentType.Pointer:
                                        if(alu == Instruction.OpcodeActionALU.SUB)
                                        {
                                            tdRes = m_typeSystem.WellKnownTypes.System_IntPtr;
                                        }
                                        break;
                                }
                                break;
                        }

                        BinaryOperator.ALU alu2;

                        switch(alu)
                        {
                            case Instruction.OpcodeActionALU.ADD: alu2 = BinaryOperator.ALU.ADD; break;
                            case Instruction.OpcodeActionALU.SUB: alu2 = BinaryOperator.ALU.SUB; break;
                            case Instruction.OpcodeActionALU.MUL: alu2 = BinaryOperator.ALU.MUL; break;
                            case Instruction.OpcodeActionALU.DIV: alu2 = BinaryOperator.ALU.DIV; break;
                            case Instruction.OpcodeActionALU.REM: alu2 = BinaryOperator.ALU.REM; break;
                            case Instruction.OpcodeActionALU.AND: alu2 = BinaryOperator.ALU.AND; break;
                            case Instruction.OpcodeActionALU.OR : alu2 = BinaryOperator.ALU.OR ; break;
                            case Instruction.OpcodeActionALU.XOR: alu2 = BinaryOperator.ALU.XOR; break;
                            case Instruction.OpcodeActionALU.SHL: alu2 = BinaryOperator.ALU.SHL; break;
                            case Instruction.OpcodeActionALU.SHR: alu2 = BinaryOperator.ALU.SHR; break;
                            default: throw IncorrectEncodingException.Create( "Invalid OpcodeActionALU {0} for {1}", alu, instr );
                        }

                        if(tdRes == null)
                        {
                            throw TypeConsistencyErrorException.Create( "ALU operation '{0}' applied to incompatible types: {1} != {2}", instr, exLeft.Type, exRight.Type );
                        }

                        VariableExpression exRes = CreateNewTemporary( tdRes );

                        ModifyStackModel( 2, exRes );

                        bool fSigned;

                        switch(alu)
                        {
                            case Instruction.OpcodeActionALU.MUL:
                                //
                                // There's no unsigned bytecode for mul, but we need to determine the exact flavor for proper expansion.
                                //
                                fSigned = (exLeft.Type.IsSigned || exRight.Type.IsSigned);
                                break;

                            default:
                                fSigned = instr.Operator.IsSigned;
                                break;
                        }

                        AddOperator( BinaryOperator.New( instr.DebugInfo, alu2, fSigned, instr.Operator.RequiresOverflowCheck, exRes, exLeft, exRight ) );
                    }
                    break;

                case Instruction.OpcodeActionALU.NEG:
                case Instruction.OpcodeActionALU.NOT:
                case Instruction.OpcodeActionALU.FINITE:
                    {
                        Expression         ex    = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes = CreateNewTemporary( ex.Type );

                        ModifyStackModel( 1, exRes );

                        UnaryOperator.ALU alu2;

                        switch(alu)
                        {
                            case Instruction.OpcodeActionALU.NEG   : alu2 = UnaryOperator.ALU.NEG   ; break;
                            case Instruction.OpcodeActionALU.NOT   : alu2 = UnaryOperator.ALU.NOT   ; break;
                            case Instruction.OpcodeActionALU.FINITE: alu2 = UnaryOperator.ALU.FINITE; break;
                            default: throw IncorrectEncodingException.Create( "Invalid OpcodeActionALU {0} for {1}", alu, instr );
                        }

                        AddOperator( UnaryOperator.New( instr.DebugInfo, alu2, instr.Operator.IsSigned, instr.Operator.RequiresOverflowCheck, exRes, ex ) );
                    }
                    break;

                case Instruction.OpcodeActionALU.NOP:
                    {
                        AddOperator( NopOperator.New( instr.DebugInfo ) );
                    }
                    break;

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionALU {0} for {1}", alu, instr );
                    }
            }
        }

        //
        // CONV_I1         = Action_Convert            | Type_I1                                 ,
        // CONV_I2         = Action_Convert            | Type_I2                                 ,
        // CONV_I4         = Action_Convert            | Type_I4                                 ,
        // CONV_I8         = Action_Convert            | Type_I8                                 ,
        // CONV_R4         = Action_Convert            | Type_R4                                 ,
        // CONV_R8         = Action_Convert            | Type_R8                                 ,
        // CONV_U4         = Action_Convert            | Type_U4                                 ,
        // CONV_U8         = Action_Convert            | Type_U8                                 ,
        // CONV_R_UN       = Action_Convert            | Type_R             | Unsigned           ,
        // CONV_OVF_I1_UN  = Action_Convert            | Type_I1            | Unsigned | Overflow,
        // CONV_OVF_I2_UN  = Action_Convert            | Type_I2            | Unsigned | Overflow,
        // CONV_OVF_I4_UN  = Action_Convert            | Type_I4            | Unsigned | Overflow,
        // CONV_OVF_I8_UN  = Action_Convert            | Type_I8            | Unsigned | Overflow,
        // CONV_OVF_U1_UN  = Action_Convert            | Type_R4            | Unsigned | Overflow,
        // CONV_OVF_U2_UN  = Action_Convert            | Type_R8            | Unsigned | Overflow,
        // CONV_OVF_U4_UN  = Action_Convert            | Type_U4            | Unsigned | Overflow,
        // CONV_OVF_U8_UN  = Action_Convert            | Type_U8            | Unsigned | Overflow,
        // CONV_OVF_I_UN   = Action_Convert            | Type_I             | Unsigned | Overflow,
        // CONV_OVF_U_UN   = Action_Convert            | Type_U             | Unsigned | Overflow,
        // CONV_OVF_I1     = Action_Convert            | Type_I1                       | Overflow,
        // CONV_OVF_U1     = Action_Convert            | Type_U1                       | Overflow,
        // CONV_OVF_I2     = Action_Convert            | Type_I2                       | Overflow,
        // CONV_OVF_U2     = Action_Convert            | Type_U2                       | Overflow,
        // CONV_OVF_I4     = Action_Convert            | Type_I4                       | Overflow,
        // CONV_OVF_U4     = Action_Convert            | Type_U4                       | Overflow,
        // CONV_OVF_I8     = Action_Convert            | Type_I8                       | Overflow,
        // CONV_OVF_U8     = Action_Convert            | Type_U8                       | Overflow,
        // CONV_U2         = Action_Convert            | Type_U2                                 ,
        // CONV_U1         = Action_Convert            | Type_U1                                 ,
        // CONV_I          = Action_Convert            | Type_I                                  ,
        // CONV_OVF_I      = Action_Convert            | Type_I                        | Overflow,
        // CONV_OVF_U      = Action_Convert            | Type_U                        | Overflow,
        // CONV_U          = Action_Convert            | Type_U                                  ,
        //
        private void ProcessInstruction_Convert( Instruction instr )
        {
            Expression               exValue = GetArgumentFromStack( 0, 1 );
            ScalarTypeRepresentation td      = (ScalarTypeRepresentation)ProcessInstruction_GetTypeFromActionType( instr, this.CurrentArgumentAsType );

            if(exValue is ConstantExpression)
            {
                ConstantExpression exConst = (ConstantExpression)exValue;

                Expression exRes = CreateNewConstant( td, exConst.Value );

                ModifyStackModel( 1, exRes );

                AddOperator( NopOperator.New( instr.DebugInfo ) ); // Value is a constant, nothing to do.
            }
            else
            {
                VariableExpression exRes = CreateNewTemporary( td );

                ModifyStackModel( 1, exRes );

                ConvertKind                     kindSrc      = Convert_AnalyzeArgument( exValue );
                ConvertKind                     kindDst      = Convert_AnalyzeArgument( exRes   );
                bool                            fIsDstSigned = td.IsSigned;
                bool                            fIsSrcSigned = instr.Operator.IsSigned;
                bool                            fOverflow    = instr.Operator.RequiresOverflowCheck;
                ConvertOp                       opKind       = ConvertOp.Unknown;
                uint                            opSize       = td.Size;
                TypeRepresentation.BuiltInTypes kindInput    = TypeRepresentation.BuiltInTypes.END;
                TypeRepresentation.BuiltInTypes kindOutput   = TypeRepresentation.BuiltInTypes.END;

                switch(kindSrc)
                {
                    case ConvertKind.Int:
                        switch(kindDst)
                        {
                            case ConvertKind.Int:
                                if(fIsSrcSigned == fIsDstSigned && opSize == sizeof(int))
                                {
                                    opKind = ConvertOp.Identity;
                                }
                                else
                                {
                                    opKind = fIsDstSigned ? ConvertOp.SignExtend : ConvertOp.ZeroExtend;
                                }
                                break;

                            case ConvertKind.Long:
                                opSize = exValue.Type.Size;
                                opKind = fIsDstSigned ? ConvertOp.SignExtend : ConvertOp.ZeroExtend;
                                break;

                            case ConvertKind.Single:
                                opKind     = ConvertOp.Convert;
                                kindInput  = fIsSrcSigned ? TypeRepresentation.BuiltInTypes.I4 : TypeRepresentation.BuiltInTypes.U4;
                                kindOutput =                TypeRepresentation.BuiltInTypes.R4;
                                break;

                            case ConvertKind.Double:
                                opKind     = ConvertOp.Convert;
                                kindInput  = fIsSrcSigned ? TypeRepresentation.BuiltInTypes.I4 : TypeRepresentation.BuiltInTypes.U4;
                                kindOutput =                TypeRepresentation.BuiltInTypes.R8;
                                break;
                        }
                        break;

                    case ConvertKind.Long:
                        switch(kindDst)
                        {
                            case ConvertKind.Int:
                                opKind = ConvertOp.Truncate;
                                break;

                            case ConvertKind.Long:
                                if(fIsSrcSigned == fIsDstSigned)
                                {
                                    opKind = ConvertOp.Identity;
                                }
                                else
                                {
                                    opKind = fIsDstSigned ? ConvertOp.SignExtend : ConvertOp.ZeroExtend;
                                }
                                break;

                            case ConvertKind.Single:
                                opKind     = ConvertOp.Convert;
                                kindInput  = fIsSrcSigned ? TypeRepresentation.BuiltInTypes.I8 : TypeRepresentation.BuiltInTypes.U8;
                                kindOutput =                TypeRepresentation.BuiltInTypes.R4;
                                break;

                            case ConvertKind.Double:
                                opKind     = ConvertOp.Convert;
                                kindInput  = fIsSrcSigned ? TypeRepresentation.BuiltInTypes.I8 : TypeRepresentation.BuiltInTypes.U8;
                                kindOutput =                TypeRepresentation.BuiltInTypes.R8;
                                break;
                        }
                        break;

                    case ConvertKind.Single:
                        switch(kindDst)
                        {
                            case ConvertKind.Int:
                                opKind     = ConvertOp.Convert;
                                kindInput  =                TypeRepresentation.BuiltInTypes.R4;
                                kindOutput = fIsDstSigned ? TypeRepresentation.BuiltInTypes.I4 : TypeRepresentation.BuiltInTypes.U4;
                                break;

                            case ConvertKind.Long:
                                opKind     = ConvertOp.Convert;
                                kindInput  =                TypeRepresentation.BuiltInTypes.R4;
                                kindOutput = fIsDstSigned ? TypeRepresentation.BuiltInTypes.I8 : TypeRepresentation.BuiltInTypes.U8;
                                break;

                            case ConvertKind.Single:
                                opKind = ConvertOp.Identity;
                                break;

                            case ConvertKind.Double:
                                opKind     = ConvertOp.Convert;
                                kindInput  = TypeRepresentation.BuiltInTypes.R4;
                                kindOutput = TypeRepresentation.BuiltInTypes.R8;
                                break;
                        }
                        break;

                    case ConvertKind.Double:
                        switch(kindDst)
                        {
                            case ConvertKind.Int:
                                opKind     = ConvertOp.Convert;
                                kindInput  =                TypeRepresentation.BuiltInTypes.R8;
                                kindOutput = fIsDstSigned ? TypeRepresentation.BuiltInTypes.I4 : TypeRepresentation.BuiltInTypes.U4;
                                break;

                            case ConvertKind.Long:
                                opKind     = ConvertOp.Convert;
                                kindInput  =                TypeRepresentation.BuiltInTypes.R8;
                                kindOutput = fIsDstSigned ? TypeRepresentation.BuiltInTypes.I8 : TypeRepresentation.BuiltInTypes.U8;
                                break;

                            case ConvertKind.Single:
                                opKind     = ConvertOp.Convert;
                                kindInput  = TypeRepresentation.BuiltInTypes.R8;
                                kindOutput = TypeRepresentation.BuiltInTypes.R4;
                                break;

                            case ConvertKind.Double:
                                opKind = ConvertOp.Identity;
                                break;
                        }
                        break;

                    case ConvertKind.Pointer:
                    case ConvertKind.Object:
                        switch(kindDst)
                        {
                            case ConvertKind.Int:
                                opKind = ConvertOp.Identity;
                                break;

                            case ConvertKind.Long:
                                opSize = exValue.Type.Size;
                                opKind = fIsDstSigned ? ConvertOp.SignExtend : ConvertOp.ZeroExtend;
                                break;
                        }
                        break;
                }

                switch(opKind)
                {
                    case ConvertOp.Unknown:
                        throw new NotImplementedException();

                    case ConvertOp.Identity:
                        AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, exValue ) );
                        break;

                    case ConvertOp.ZeroExtend:
                        AddOperator( ZeroExtendOperator.New( instr.DebugInfo, opSize, fOverflow, exRes, exValue ) );
                        break;

                    case ConvertOp.SignExtend:
                        AddOperator( SignExtendOperator.New( instr.DebugInfo, opSize, fOverflow, exRes, exValue ) );
                        break;

                    case ConvertOp.Truncate:
                        AddOperator( TruncateOperator.New( instr.DebugInfo, opSize, fOverflow, exRes, exValue ) );
                        break;

                    case ConvertOp.Convert:
                        AddOperator( ConvertOperator.New( instr.DebugInfo, kindInput, kindOutput, fOverflow, exRes, exValue ) );
                        break;
                }
            }
        }

        enum ConvertKind
        {
            Int    ,
            Long   ,
            Single ,
            Double ,
            Pointer,
            Object ,
        }

        enum ConvertOp
        {
            Unknown   ,
            Identity  ,
            ZeroExtend,
            SignExtend,
            Truncate  ,
            Convert   ,
        }

        private static ConvertKind Convert_AnalyzeArgument( Expression ex )
        {
            TypeRepresentation td = ex.Type;

            if(td is PointerTypeRepresentation)
            {
                return ConvertKind.Pointer;
            }
            else if(td is ReferenceTypeRepresentation)
            {
                return ConvertKind.Object;
            }
            else if(td is ScalarTypeRepresentation)
            {
                if(td.IsInteger)
                {
                    return td.Size == 4 ? ConvertKind.Int : ConvertKind.Long;
                }
                else if(td.IsFloatingPoint)
                {
                    return td.Size == 4 ? ConvertKind.Single : ConvertKind.Double;
                }
            }

            throw TypeConsistencyErrorException.Create( "Type '{0}' cannot be used in a convert context", td );
        }

        //--//

        //
        // CPOBJ           = Action_Object             | Obj_CpObj                               ,
        // LDOBJ           = Action_Object             | Obj_LdObj                               ,
        // NEWOBJ          = Action_Object             | Obj_New                                 ,
        // CASTCLASS       = Action_Object             | Obj_Cast                                ,
        // ISINST          = Action_Object             | Obj_IsInst                              ,
        // UNBOX           = Action_Object             | Obj_Unbox                               ,
        // STOBJ           = Action_Object             | Obj_StObj                               ,
        // BOX             = Action_Object             | Obj_Box                                 ,
        // NEWARR          = Action_Object             | Obj_NewArr                              ,
        // LDLEN           = Action_Object             | Obj_LdLen                               ,
        // UNBOX_ANY       = Action_Object             | Obj_UnboxAny                            ,
        // LDFTN           = Action_Object             | Obj_LdFtn                               ,
        // LDVIRTFTN       = Action_Object             | Obj_LdVirtFtn                           ,
        // INITOBJ         = Action_Object             | Obj_InitObj                             ,
        // CPBLK           = Action_Object             | Obj_CpBlk                               ,
        // INITBLK         = Action_Object             | Obj_InitBlk                             ,
        // SIZEOF          = Action_Object             | Obj_SizeOf                              ,
        //
        private void ProcessInstruction_Object( Instruction instr )
        {
            switch(instr.Operator.ActionObject)
            {
                case Instruction.OpcodeActionObject.New:
                    {
                        MethodRepresentation        md       = this.CurrentArgumentAsMethod;
                        TypeRepresentation[]        args     = md.ThisPlusArguments;
                        int                         argsNum  = args.Length;
                        Expression[]                exActual = new Expression[argsNum];
                        TypeRepresentation          thisTd   = args[0];
                        LocalVariableExpression     local = null;

                        //
                        // If we are allocate a value type on the stack, we need to:
                        //
                        //   1) Create a local variable.
                        //   2) Initialize it to zero.
                        //   3) Take its address and store it in a temporary.
                        //   4) Call the constructor using the address as the this pointer.
                        //   5) Put the local variable on the evaluation stack.
                        //
                        if(thisTd is ManagedPointerTypeRepresentation)
                        {
                            Debug.Assert(thisTd.UnderlyingType is ValueTypeRepresentation, "Only value types can be passed as managed pointers for 'this' argument.");
                            local = m_cfg.AllocateLocal( thisTd.UnderlyingType, null );
                        }

                        TemporaryVariableExpression tmp = CreateNewTemporary( thisTd );
                        exActual[0] = tmp;

                        for(int i = 1; i < argsNum; i++)
                        {
                            exActual[i] = GetArgumentFromStack( i - 1, argsNum - 1, args[i] );

                            if(!CanBeAssignedFromEvaluationStack( args[i], exActual[i] ))
                            {
                                throw TypeConsistencyErrorException.Create( "Incorrect argument {0} on call to {1} from {2}: got {3}, expecting {4}", i - 1, md, m_md, exActual[i].Type, args[i] );
                            }
                        }

                        ModifyStackModel( argsNum - 1, local ?? exActual[0] );

                        if(local != null)
                        {
                            AddOperator( m_cfg.GenerateVariableInitialization( instr.DebugInfo,      local ) );
                            AddOperator( AddressAssignmentOperator.New       ( instr.DebugInfo, tmp, local ) );
                        }
                        else
                        {
                            Expression[] exArgs = ArrayUtility.RemoveAtPositionFromNotNullArray( exActual, 0 );

                            AddOperator( ObjectAllocationOperator.New( instr.DebugInfo, md, (VariableExpression)exActual[0], exArgs ) );
                        }

                        AddOperator( InstanceCallOperator.New( instr.DebugInfo, CallOperator.CallKind.Direct, md, exActual, true ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.NewArr:
                    {
                        TypeRepresentation td       = this.CurrentArgumentAsType;
                        Expression         exLength = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes    = CreateNewTemporary( m_typeSystem.CreateArrayOfType( td ) );

                        ModifyStackModel( 1, exRes );

                        AddOperator( ArrayAllocationOperator.New( instr.DebugInfo, exRes.Type, exRes, exLength ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.LdLen:
                    {
                        Expression         exArray = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation tdArray = exArray.Type;

                        if(!(tdArray is ArrayReferenceTypeRepresentation))
                        {
                            throw TypeConsistencyErrorException.Create( "Expecting an array, found {0}", tdArray );
                        }

                        VariableExpression ex = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_UIntPtr );

                        ModifyStackModel( 1, ex );

                        AddOperator( ArrayLengthOperator.New( instr.DebugInfo, ex, exArray ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.Cast:
                    {
                        Expression         exValue = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation td      = m_typeSystem.CreateBoxedValueTypeIfNecessary( this.CurrentArgumentAsType );
                        VariableExpression exRes   = CreateNewTemporary( td );

                        //
                        // Exact match? No need to do a cast.
                        //
                        if(exValue.Type == td)
                        {
                            AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, exValue ) );
                        }
                        else
                        {
                            AddOperator( CastOperator.New( instr.DebugInfo, td, exRes, exValue ) );
                        }

                        ModifyStackModel( 1, exRes );
                    }
                    break;

                case Instruction.OpcodeActionObject.IsInst:
                    {
                        Expression         exVal = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation td    = m_typeSystem.CreateBoxedValueTypeIfNecessary( this.CurrentArgumentAsType );

                        if(CanBeAssignedFromEvaluationStack( td, exVal ))
                        {
                            //
                            // We can statically prove that the expression is an instance of the required type,
                            // so there's nothing to do.
                            //
                            AddOperator( NopOperator.New( instr.DebugInfo ) );
                        }
                        else
                        {
                            VariableExpression exRes = CreateNewTemporary( td );

                            ModifyStackModel( 1, exRes );

                            AddOperator( IsInstanceOperator.New( instr.DebugInfo, td, exRes, exVal ) );
                        }
                    }
                    break;

                case Instruction.OpcodeActionObject.Box:
                    {
                        Expression         exVal = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation td    = this.CurrentArgumentAsType;

                        switch(td.GetInstantiationFlavor( m_typeSystem ))
                        {
                            case TypeRepresentation.InstantiationFlavor.Class:
                                {
                                    VariableExpression exRes = CreateNewTemporary( td );

                                    ModifyStackModel( 1, exRes );

                                    AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, exVal ) );
                                }
                                break;

                            case TypeRepresentation.InstantiationFlavor.Delayed:
                            case TypeRepresentation.InstantiationFlavor.ValueType:
                                if(m_typeSystem.IsNullable( td ))
                                {
                                    TypeRepresentation   tdBoxedNullable = m_typeSystem.CreateBoxedValueType(                                    td   );
                                    TypeRepresentation   tdBoxed         = m_typeSystem.CreateBoxedValueType( m_typeSystem.GetNullableParameter( td ) );
                                    VariableExpression   exResPtr        = CreateNewTemporary( tdBoxedNullable );
                                    VariableExpression   exRes           = CreateNewTemporary( tdBoxed         );
                                    MethodRepresentation mdBox           = td.FindMatch( "Box" );

                                    AddOperator( AddressAssignmentOperator.New( instr.DebugInfo, exResPtr, exVal ) );

                                    AddOperator( InstanceCallOperator.New( instr.DebugInfo, CallOperator.CallKind.Direct, mdBox, VariableExpression.ToArray( exRes ), new Expression[] { exResPtr }, false ) );
                                }
                                else
                                {
                                    TypeRepresentation tdBoxed = m_typeSystem.CreateBoxedValueTypeIfNecessary( td );
                                    VariableExpression exRes   = CreateNewTemporary( tdBoxed );

                                    ModifyStackModel( 1, exRes );

                                    AddOperator( BoxOperator.New( instr.DebugInfo, tdBoxed, exRes, exVal ) );
                                }
                                break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Uncorrect type for boxing: {0}", td );
                        }
                    }
                    break;

                case Instruction.OpcodeActionObject.Unbox:
                    {
                        Expression         exBoxed = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation td      = this.CurrentArgumentAsType;
                        TypeRepresentation tdBoxed = m_typeSystem.CreateBoxedValueTypeIfNecessary( td );
                        TypeRepresentation tdPtr   = m_typeSystem.CreateManagedPointerToType     ( td );
                        VariableExpression exRes   = CreateNewTemporary( tdPtr );

                        //
                        // Exact match? No need to do a cast.
                        //
                        if(exBoxed.Type == tdBoxed)
                        {
                            AddOperator( UnboxOperator.New( instr.DebugInfo, exRes, exBoxed ) );
                        }
                        else
                        {
                            VariableExpression exCasted = CreateNewTemporary( tdBoxed );

                            AddOperator( CastOperator.New( instr.DebugInfo, tdBoxed, exCasted, exBoxed ) );

                            AddOperator( UnboxOperator.New( instr.DebugInfo, exRes, exBoxed ) );
                        }

                        ModifyStackModel( 1, exRes );
                    }
                    break;

                case Instruction.OpcodeActionObject.UnboxAny:
                    {
                        Expression         exBoxed = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation td      = this.CurrentArgumentAsType;
                        VariableExpression exRes   = CreateNewTemporary( td );

                        switch(td.GetInstantiationFlavor( m_typeSystem ))
                        {
                            case TypeRepresentation.InstantiationFlavor.Class:
                                {
                                    //
                                    // Exact match? No need to do a cast.
                                    //
                                    if(exBoxed.Type == td)
                                    {
                                        AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, exBoxed ) );
                                    }
                                    else
                                    {
                                        AddOperator( CastOperator.New( instr.DebugInfo, td, exRes, exBoxed ) );
                                    }
                                }
                                break;

                            case TypeRepresentation.InstantiationFlavor.Delayed:
                            case TypeRepresentation.InstantiationFlavor.ValueType:
                                {
                                    TypeRepresentation tdBoxed = m_typeSystem.CreateBoxedValueTypeIfNecessary( td );
                                    TypeRepresentation tdPtr   = m_typeSystem.CreateManagedPointerToType     ( td );
                                    VariableExpression exPtr   = CreateNewTemporary( tdPtr );

                                    //
                                    // Exact match? No need to do a cast.
                                    //
                                    if(exBoxed.Type == tdBoxed)
                                    {
                                        AddOperator( UnboxOperator.New( instr.DebugInfo, exPtr, exBoxed ) );
                                    }
                                    else
                                    {
                                        VariableExpression exCasted = CreateNewTemporary( tdBoxed );

                                        AddOperator( CastOperator.New( instr.DebugInfo, tdBoxed, exCasted, exBoxed ) );

                                        AddOperator( UnboxOperator.New( instr.DebugInfo, exPtr, exCasted ) );
                                    }

                                    AddOperator( LoadIndirectOperator.New( instr.DebugInfo, td, exRes, exPtr, null, 0, false, true ) );
                                }
                                break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Uncorrect type for boxing: {0}", td );
                        }

                        ModifyStackModel( 1, exRes );
                    }
                    break;

                case Instruction.OpcodeActionObject.LdFtn:
                    {
                        VariableExpression exRes = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_IntPtr );

                        PushStackModel( exRes );

                        AddOperator( MethodRepresentationOperator.New( instr.DebugInfo, this.CurrentArgumentAsMethod, exRes ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.LdVirtFtn:
                    {
                        Expression         exObj = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_IntPtr );

                        ModifyStackModel( 1, exRes );

                        AddOperator( MethodRepresentationOperator.New( instr.DebugInfo, this.CurrentArgumentAsMethod, exRes, exObj ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.InitObj:
                    {
                        Expression         exAddr = GetArgumentFromStack( 0, 1 );
                        TypeRepresentation td     = ProcessInstruction_ExtractAddressType( instr, exAddr, this.CurrentArgumentAsType );

                        PopStackModel( 1 );

                        AddOperator( m_cfg.GenerateVariableInitialization( instr.DebugInfo, exAddr, td, true ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.LdObj:
                    {
                        Expression exAddr = GetArgumentFromStack( 0, 1 );

                        AddOperator( ProcessInstruction_LoadIndirect( instr, exAddr, this.CurrentArgumentAsType ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.CpObj:
                    {
                        TypeRepresentation td        = this.CurrentArgumentAsType;
                        Expression         exAddrDst = GetArgumentFromStack( 0, 2 );
                        Expression         exAddrSrc = GetArgumentFromStack( 1, 2 );
                        Expression         exValue;

                        AddOperator( ProcessInstruction_LoadIndirect( instr, exAddrSrc, td ) );

                        exValue = GetArgumentFromStack( 1, 2 );

                        AddOperator( ProcessInstruction_StoreIndirect( instr, exAddrDst, exValue, td ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.StObj:
                    {
                        Expression exAddr  = GetArgumentFromStack( 0, 2 );
                        Expression exValue = GetArgumentFromStack( 1, 2 );

                        AddOperator( ProcessInstruction_StoreIndirect( instr, exAddr, exValue, this.CurrentArgumentAsType ) );
                    }
                    break;

                case Instruction.OpcodeActionObject.InitBlk:
                    {
                        throw new NotImplementedException();
                    }

                case Instruction.OpcodeActionObject.CpBlk:
                    {
                        throw new NotImplementedException();
                    }

                case Instruction.OpcodeActionObject.SizeOf:
                    {
                        VariableExpression exRes = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_UInt32 );

                        PushStackModel( exRes );

                        //
                        // Section 4.25 of ECMA spec, Partition III
                        // 
                        // [Rationale: The definition of a value type can change between the time the CIL is generated and the time that it
                        // is loaded for execution. Thus, the size of the type is not always known when the CIL is generated. The sizeof
                        // instruction allows CIL code to determine the size at runtime without the need to call into the Framework class
                        // library. The computation can occur entirely at runtime or at CIL-to-native-code compilation time. sizeof
                        // returns the total size that would be occupied by each element in an array of this type – including any padding
                        // the implementation chooses to add. Specifically, array elements lie sizeof bytes apart. end rationale]
                        // 
                        TypeRepresentation td = this.CurrentArgumentAsType;
                        if(!(td is ValueTypeRepresentation))
                        {
                            td = m_typeSystem.WellKnownTypes.System_UIntPtr;
                        }

                        AddOperator( SingleAssignmentOperator.New( instr.DebugInfo, exRes, m_typeSystem.CreateConstantForTypeSize( td ) ) );
                    }
                    break;

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionObject {0} for {1}", instr.Operator.ActionObject, instr );
                    }
            }
        }

        //
        // REFANYVAL       = Action_TypedRef           | TypedRef_RefAnyVal                      ,
        // MKREFANY        = Action_TypedRef           | TypedRef_MkRefAny                       ,
        // REFANYTYPE      = Action_TypedRef           | TypedRef_RefAnyType                     ,
        //
        private void ProcessInstruction_TypedRef( Instruction instr )
        {
            switch(instr.Operator.ActionTypedReference)
            {
                case Instruction.OpcodeActionTypedReference.RefAnyVal:
                    {
                        TypeRepresentation td         = m_typeSystem.CreateManagedPointerToType( this.CurrentArgumentAsType );
                        Expression         exTypedref = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes      = CreateNewTemporary( td );

                        ModifyStackModel( 1, exRes );

                        AddOperator( RefAnyValOperator.New( instr.DebugInfo, td, exRes, exTypedref ) );
                    }
                    break;

                case Instruction.OpcodeActionTypedReference.MkRefAny:
                    {
                        TypeRepresentation td    = m_typeSystem.CreateManagedPointerToType( this.CurrentArgumentAsType );
                        Expression         exPtr = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_TypedReference );

                        ModifyStackModel( 1, exRes );

                        AddOperator( MkRefAnyOperator.New( instr.DebugInfo, td, exRes, exPtr ) );
                    }
                    break;

                case Instruction.OpcodeActionTypedReference.RefAnyType:
                    {
                        Expression         exTypedref = GetArgumentFromStack( 0, 1 );
                        VariableExpression exRes      = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_RuntimeTypeHandle );

                        ModifyStackModel( 1, exRes );

                        AddOperator( RefAnyTypeOperator.New( instr.DebugInfo, exRes, exTypedref ) );
                    }
                    break;

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionTypedReference {0} for {1}", instr.Operator.ActionTypedReference, instr );
                    }
            }
        }

        //
        // THROW           = Action_EH                 | EH_Throw                                ,
        // ENDFINALLY      = Action_EH                 | EH_EndFinally                           ,
        // LEAVE           = Action_EH                 | EH_Leave                                ,
        // LEAVE_S         = Action_EH                 | EH_Leave                                ,
        // ENDFILTER       = Action_EH                 | EH_EndFilter                            ,
        // RETHROW         = Action_EH                 | EH_ReThrow                              ,
        //
        private void ProcessInstruction_ExceptionHandling( Instruction instr )
        {
            ControlOperator op;

            switch(instr.Operator.ActionExceptionHandling)
            {
                case Instruction.OpcodeActionExceptionHandling.Throw:
                    {
                        Expression ex = GetArgumentFromStack( 0, 1 );

                        EmptyStackModel();

                        op = ThrowControlOperator.New( instr.DebugInfo, ex );
                    }
                    break;

                case Instruction.OpcodeActionExceptionHandling.EndFinally:
                    {
                        EmptyStackModel();

                        op = EndFinallyControlOperator.New( instr.DebugInfo );
                    }
                    break;

                case Instruction.OpcodeActionExceptionHandling.Leave:
                    {
                        BasicBlock targetBranchTaken = GetBasicBlockFromInstruction( this.CurrentArgumentAsInt32 );

                        EmptyStackModel();

                        op = LeaveControlOperator.New( instr.DebugInfo, targetBranchTaken );
                    }
                    break;

                case Instruction.OpcodeActionExceptionHandling.EndFilter:
                    {
                        throw new NotImplementedException();
                    }

                case Instruction.OpcodeActionExceptionHandling.ReThrow:
                    {
                        EmptyStackModel();

                        op = RethrowControlOperator.New( instr.DebugInfo );
                    }
                    break;

                default:
                    {
                        throw IncorrectEncodingException.Create( "Invalid OpcodeActionExceptionHandling {0} for {1}", instr.Operator.ActionExceptionHandling, instr );
                    }
            }

            AddControl( op );
        }

        //
        // CEQ             = Action_Set                | Cond_EQ                                 ,
        // CGT             = Action_Set                | Cond_GT                                 ,
        // CGT_UN          = Action_Set                | Cond_GT            | Unsigned           ,
        // CLT             = Action_Set                | Cond_LT                                 ,
        // CLT_UN          = Action_Set                | Cond_LT            | Unsigned           ,
        //
        private void ProcessInstruction_Set( Instruction instr )
        {
            Expression         exLeft  = GetArgumentFromStack( 0, 2 );
            Expression         exRight = GetArgumentFromStack( 1, 2 );
            VariableExpression exRes   = CreateNewTemporary( m_typeSystem.WellKnownTypes.System_Int32 );

            ModifyStackModel( 2, exRes );

            //
            // Section 1.5 of ECMA spec, Partition III
            // BUGBUG: several cases we do not handle yet...
            //
            StackEquivalentType seLeft  = exLeft .StackEquivalentType;
            StackEquivalentType seRight = exRight.StackEquivalentType;

            if(seLeft != seRight)
            {
                // ints and manager pointers are 1-o-1 compatible
                if(seLeft == StackEquivalentType.NativeInt && seRight == StackEquivalentType.Int32     ||
                   seLeft == StackEquivalentType.Int32     && seRight == StackEquivalentType.NativeInt)
                {
                }
                else if(seLeft == StackEquivalentType.Pointer && (exRight.CanBeNull == CanBeNull.Yes || seRight == StackEquivalentType.NativeInt))
                {
                }
                else if(seRight == StackEquivalentType.Pointer && (exLeft.CanBeNull == CanBeNull.Yes || seLeft == StackEquivalentType.NativeInt))
                {
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "CompareAndSet operation '{0}' applied to incompatible types: {1} != {2}", instr, exLeft.Type, exRight.Type );
                }
            }

            CompareAndSetOperator.ActionCondition cond2;

            switch(instr.Operator.ActionCondition)
            {
                case Instruction.OpcodeActionCondition.EQ: cond2 = CompareAndSetOperator.ActionCondition.EQ; break;
                case Instruction.OpcodeActionCondition.GT: cond2 = CompareAndSetOperator.ActionCondition.GT; break;
                case Instruction.OpcodeActionCondition.LT: cond2 = CompareAndSetOperator.ActionCondition.LT; break;
                default: throw TypeConsistencyErrorException.Create( "Invalid condition {0} for set operation '{1}'", instr.Operator.ActionCondition, instr );
            }

            //
            // "<var> Greater Than Zero" for an unsigned variable is equivalent to "<var> Not Equal to Zero".
            //
            if(exRight.IsEqualToZero())
            {
                if(instr.Operator.IsSigned == false && cond2 == CompareAndSetOperator.ActionCondition.GT)
                {
                    cond2 = CompareAndSetOperator.ActionCondition.NE;
                }
            }

            AddOperator( CompareAndSetOperator.New( instr.DebugInfo, cond2, instr.Operator.IsSigned, exRes, exLeft, exRight ) );
        }

        //
        // UNALIGNED       = Action_Modifier           | Mod_Unaligned                           ,
        // VOLATILE        = Action_Modifier           | Mod_Volatile                            ,
        // TAILCALL        = Action_Modifier           | Mod_TailCall                            ,
        // CONSTRAINED     = Action_Modifier           | Mod_Constrained                         ,
        // NO              = Action_Modifier           | Mod_NoCheck                             ,
        // READONLY        = Action_Modifier           | Mod_Readonly                            ,
        //
        private void ProcessInstruction_Modifier( Instruction instr )
        {
            switch(instr.Operator.ActionModifier)
            {
                case Instruction.OpcodeActionModifier.Unaligned:
                    m_mod_unaligned = this.CurrentArgumentAsInt32;
                    break;

                case Instruction.OpcodeActionModifier.Volatile:
                    m_mod_fVolatile = true;
                    break;

                case Instruction.OpcodeActionModifier.TailCall:
                    m_mod_fTailCall = true;
                    break;

                case Instruction.OpcodeActionModifier.Constrained:
                    m_mod_constrained = this.CurrentArgumentAsType;
                    break;

                case Instruction.OpcodeActionModifier.NoCheck:
                    m_mod_nocheck = this.CurrentArgumentAsInt32;
                    break;

                case Instruction.OpcodeActionModifier.Readonly:
                    m_mod_fReadonly = true;
                    break;
            }
        }

        //
        // BREAK           = Action_Debug              | Debug_Breakpoint                        ,
        //
        private void ProcessInstruction_Debug( Instruction instr )
        {
            throw new NotImplementedException();
        }
    }
}

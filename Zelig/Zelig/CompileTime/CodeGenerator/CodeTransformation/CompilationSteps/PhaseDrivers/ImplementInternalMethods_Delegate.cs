//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed partial class ImplementInternalMethods
    {
        //
        // Helper Methods
        //

        private void ImplementDelegateMethods( MethodRepresentation md )
        {
            if(TypeSystemForCodeTransformation.GetCodeForMethod( md ) == null)
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_typeSystem.CreateControlFlowGraphState( md );
                WellKnownTypes                             wkt =                                             m_typeSystem.WellKnownTypes;
                WellKnownMethods                           wkm =                                             m_typeSystem.WellKnownMethods;
                WellKnownFields                            wkf =                                             m_typeSystem.WellKnownFields;

                if(md is ConstructorMethodRepresentation)
                {
                    var bb = cfg.CreateFirstNormalBasicBlock();

                    md.BuildTimeFlags &= ~MethodRepresentation.BuildTimeAttributes.Inline;
                    md.BuildTimeFlags |=  MethodRepresentation.BuildTimeAttributes.NoInline;

                    //
                    // Create proper flow control for exit basic block.
                    //
                    cfg.AddReturnOperator();

                    //
                    // Call the base method.
                    //
                    MethodRepresentation mdParent = wkm.MulticastDelegateImpl_MulticastDelegateImpl;

                    Expression[] rhs = new Expression[cfg.Arguments.Length];
                    for(int i = 0; i < cfg.Arguments.Length; i++)
                    {
                        rhs[i] = cfg.Arguments[i];
                    }

                    // Wrap the IntPtr param in a CodePointer struct and call the parent constructor.
                    rhs[2] = cfg.AllocateLocal( wkt.Microsoft_Zelig_Runtime_TypeSystem_CodePointer, null );
                    bb.AddOperator( StoreInstanceFieldOperator.New( null, wkf.CodePointer_Target, rhs[2], cfg.Arguments[2], false ) );
                    bb.AddOperator( InstanceCallOperator.New( null, CallOperator.CallKind.Overridden, mdParent, rhs, true ) );
                }
                else if(md.Name == "Invoke")
                {
                    var                bb = cfg.CreateFirstNormalBasicBlock();
                    BasicBlock         bbExit;
                    Expression[]       rhs;
                    VariableExpression exReturnValue;

                    //
                    // Create proper flow control for exit basic block.
                    //
                    ReturnControlOperator opRet;

                    if(cfg.ReturnValue != null)
                    {
                        opRet = ReturnControlOperator.New( cfg.ReturnValue );

                        //--//

                        exReturnValue = cfg.AllocateLocal( cfg.ReturnValue.Type, null );

                        Operator op = cfg.GenerateVariableInitialization( null, exReturnValue );

                        cfg.GetInjectionPoint( BasicBlock.Qualifier.PrologueEnd ).AddOperator( op );

                        //--//

                        bbExit = cfg.CreateLastNormalBasicBlock();

                        bbExit.AddOperator( SingleAssignmentOperator.New( null, cfg.ReturnValue, exReturnValue ) );
                    }
                    else
                    {
                        opRet = ReturnControlOperator.New();

                        //--//

                        exReturnValue = null;

                        //--//

                        bbExit = cfg.NormalizedExitBasicBlock;
                    }

                    cfg.ExitBasicBlock.AddOperator( opRet );


                    //
                    // This is the code we need to implement:
                    //
                    //  DelegateImpl[] invocationList = m_invocationList;
                    //
                    //  if(invocationList == null)
                    //  {
                    //      res = Invoke( ... );
                    //  }
                    //  else
                    //  {
                    //      int len = invocationList.Length;
                    //
                    //      for(int i = 0; i < len; i++)
                    //      {
                    //          res = invocationList[i].Invoke( ... );
                    //      }
                    //  }
                    //
                    //

                    FieldRepresentation fdInvocationList = wkf.MulticastDelegateImpl_m_invocationList;
                    FieldRepresentation fdTarget         = wkf.DelegateImpl_m_target;
                    FieldRepresentation fdCodePtr        = wkf.DelegateImpl_m_codePtr;

                    VariableExpression  exInvocationList = cfg.AllocateTemporary( fdInvocationList.FieldType              , null );
                    VariableExpression  exInvocation     = cfg.AllocateTemporary( fdInvocationList.FieldType.ContainedType, null );
                    VariableExpression  exTarget         = cfg.AllocateTemporary( fdTarget        .FieldType              , null );
                    VariableExpression  exCodePtr        = cfg.AllocateTemporary( fdCodePtr       .FieldType              , null );
                    VariableExpression  exLen            = cfg.AllocateTemporary( wkt.System_Int32                        , null );
                    VariableExpression  exPos            = cfg.AllocateTemporary( wkt.System_Int32                        , null );
                    VariableExpression  exThis           = cfg.Arguments[0];

                    //--//

                    //
                    //  DelegateImpl[] invocationList = m_invocationList;
                    //
                    //  if(invocationList == null)
                    //  {
                    //      <NullBranch>
                    //  }
                    //  else
                    //  {
                    //      <NotNullBranch>
                    //  }
                    //
                    NormalBasicBlock bbNull    = new NormalBasicBlock( cfg );
                    NormalBasicBlock bbNotNull = new NormalBasicBlock( cfg );

                    bb.AddOperator( LoadInstanceFieldOperator.New( null, fdInvocationList, exInvocationList, exThis, true ) );

                    bb.FlowControl = BinaryConditionalControlOperator.New( null, exInvocationList, bbNull, bbNotNull );

                    //--//

                    //
                    //  <NullBranch>
                    //
                    //  {
                    //      object  target  = m_target;
                    //      CodePtr codePtr = m_codePtr;
                    // 
                    //      if (target != null)
                    //          <returnValue> = IndirectCall<codePtr>(target, <arguments>);
                    //      else
                    //          <returnValue> = IndirectCall<codePtr>(<arguments>);
                    //  }
                    //

                    NormalBasicBlock bbInstance = new NormalBasicBlock(cfg);
                    NormalBasicBlock bbStatic = new NormalBasicBlock(cfg);

                    bbNull.AddOperator(LoadInstanceFieldOperator.New(null, fdTarget, exTarget, exThis, true));
                    bbNull.AddOperator(LoadInstanceFieldOperator.New(null, fdCodePtr, exCodePtr, exThis, true));
                    bbNull.FlowControl = BinaryConditionalControlOperator.New(null, exTarget, bbStatic, bbInstance);

                    rhs = new Expression[cfg.Arguments.Length + 1];
                    rhs[0] = exCodePtr;
                    rhs[1] = exTarget;
                    for(int i = 1; i < cfg.Arguments.Length; i++)
                    {
                        rhs[i+1] = cfg.Arguments[i];
                    }

                    bbInstance.AddOperator(IndirectCallOperator.New(null, md, VariableExpression.ToArray(exReturnValue), rhs, true, false));
                    bbInstance.AddOperator(UnconditionalControlOperator.New(null, bbExit));

                    bbStatic.AddOperator(IndirectCallOperator.New(null, md, VariableExpression.ToArray(exReturnValue), rhs, false, false));
                    bbStatic.AddOperator(UnconditionalControlOperator.New(null, bbExit));

                    //
                    //  <NotNullBranch>
                    //
                    //  {
                    //      int len = invocationList.Length;
                    //
                    //      for(int pos = 0; pos < len; pos++)
                    //      {
                    //          DelegateImpl invocation = invocationList[i];
                    //
                    //          res = invocation.Invoke( ... );
                    //      }
                    //  }

                    NormalBasicBlock bbNotNullCheck = new NormalBasicBlock(cfg);
                    NormalBasicBlock bbNotNullInner = new NormalBasicBlock(cfg);
                    NormalBasicBlock bbNotNullInstance = new NormalBasicBlock(cfg);
                    NormalBasicBlock bbNotNullStatic = new NormalBasicBlock(cfg);
                    NormalBasicBlock bbNotNullPost = new NormalBasicBlock(cfg);

                    bbNotNull.AddOperator(ArrayLengthOperator.New(null, exLen, exInvocationList));
                    bbNotNull.AddOperator(SingleAssignmentOperator.New(null, exPos, m_typeSystem.CreateConstant(0)));
                    bbNotNull.AddOperator(UnconditionalControlOperator.New(null, bbNotNullCheck));

                    bbNotNullCheck.AddOperator(CompareConditionalControlOperator.New(null, CompareAndSetOperator.ActionCondition.LT, true, exPos, exLen, bbExit, bbNotNullInner));

                    bbNotNullInner.AddOperator(LoadElementOperator.New(null, exInvocation, exInvocationList, exPos, null, true));
                    bbNotNullInner.AddOperator(LoadInstanceFieldOperator.New(null, fdTarget, exTarget, exInvocation, true));
                    bbNotNullInner.AddOperator(LoadInstanceFieldOperator.New(null, fdCodePtr, exCodePtr, exInvocation, true));

                    rhs = new Expression[cfg.Arguments.Length + 1];
                    rhs[0] = exCodePtr;
                    rhs[1] = exTarget;
                    for (int i = 1; i < cfg.Arguments.Length; i++)
                    {
                        rhs[i+1] = cfg.Arguments[i];
                    }

                    bbNotNullInner.FlowControl = BinaryConditionalControlOperator.New(null, exTarget, bbNotNullStatic, bbNotNullInstance);

                    bbNotNullInstance.AddOperator(IndirectCallOperator.New(null, md, VariableExpression.ToArray(exReturnValue), rhs, true, false));
                    bbNotNullInstance.AddOperator(UnconditionalControlOperator.New(null, bbNotNullPost));

                    bbNotNullStatic.AddOperator(IndirectCallOperator.New(null, md, VariableExpression.ToArray(exReturnValue), rhs, false, false));
                    bbNotNullStatic.AddOperator(UnconditionalControlOperator.New(null, bbNotNullPost));

                    bbNotNullPost.AddOperator(BinaryOperator.New(null, BinaryOperator.ALU.ADD, true, false, exPos, exPos, m_typeSystem.CreateConstant(1)));
                    bbNotNullPost.AddOperator(UnconditionalControlOperator.New(null, bbNotNullCheck));
                }
                else
                {
                    var bb = cfg.CreateFirstNormalBasicBlock();

                    MethodRepresentation mdThrow = wkm.ThreadImpl_ThrowNotImplementedException;
                    Expression[]         rhs     = m_typeSystem.AddTypePointerToArgumentsOfStaticMethod( mdThrow );
                    bb.AddOperator( StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdThrow, rhs ) );

                    cfg.ExitBasicBlock.AddOperator( DeadControlOperator.New( null ) );
                }
            }
        }
    }
}

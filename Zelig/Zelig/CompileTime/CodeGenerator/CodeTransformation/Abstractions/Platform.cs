//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public abstract class Platform : ITransformationContextTarget, Operator.IOperatorLevelHelper
    {
        //
        // State
        //

        protected TypeSystemForCodeTransformation m_typeSystem;

        //
        // Constructor Methods
        //

        protected Platform() // Default constructor required by TypeSystemSerializer.
        {
        }

        protected Platform( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem = typeSystem;
        }

        //
        // Helper Methods
        //

        public virtual void ApplyTransformation( TransformationContext context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            context2.Transform( ref m_typeSystem );

            context2.Pop();
        }

        //--//

        public abstract void RegisterForNotifications( TypeSystemForCodeTransformation  ts    ,
                                                       CompilationSteps.DelegationCache cache );

        public abstract TypeRepresentation GetRuntimeType( TypeSystemForCodeTransformation ts      ,
                                                           Abstractions.RegisterDescriptor regDesc );

        public virtual void ExpandCallsClosure( CompilationSteps.ComputeCallsClosure computeCallsClosure )
        {
            foreach(var regDesc in this.GetRegisters())
            {
                computeCallsClosure.Expand( GetRuntimeType( computeCallsClosure.TypeSystem, regDesc ) );
            }
        }

        //--//

        public abstract bool CanUseMultipleConditionCodes
        {
            get;
        }

        public abstract int EstimatedCostOfLoadOperation
        {
            get;
        }

        public abstract int EstimatedCostOfStoreOperation
        {
            get;
        }

        public abstract uint MemoryAlignment
        {
            get;
        }

        public abstract void GetListOfMemoryBlocks( List< Runtime.Memory.Range > list );

        public abstract PlacementRequirements GetMemoryRequirements( object obj );

        public abstract IR.ImageBuilders.CompilationState CreateCompilationState( IR.ImageBuilders.Core                      core ,
                                                                                  ControlFlowGraphStateForCodeTransformation cfg  );

        //--//
        
        public abstract string PlatformName      { get; }
        public abstract string PlatformVersion   { get; }
        public abstract string PlatformVFP       { get; }
        public abstract bool   PlatformBigEndian { get; }

        public abstract InstructionSet GetInstructionSetProvider();

        public abstract RegisterDescriptor[] GetRegisters();

        public abstract RegisterDescriptor GetRegisterForEncoding( uint regNum );

        public abstract RegisterDescriptor GetScratchRegister();

        public abstract void ComputeNumberOfFragmentsForExpression(     TypeRepresentation                                        sourceTd        ,
                                                                        ControlFlowGraphStateForCodeTransformation.KindOfFragment kind            ,
                                                                    out uint                                                      sourceFragments ,
                                                                    out bool                                                      fDirect         );

        public abstract bool CanFitInRegister( TypeRepresentation td );

        //--//

        public abstract TypeRepresentation GetMethodWrapperType();

        public abstract bool HasRegisterContextArgument( MethodRepresentation md );

        public abstract void ComputeSetOfRegistersToSave(     Abstractions.CallingConvention             cc                ,
                                                              ControlFlowGraphStateForCodeTransformation cfg               ,
                                                              BitVector                                  modifiedRegisters ,
                                                          out BitVector                                  registersToSave   ,
                                                          out Runtime.HardwareException                  he                );

        //--//

        bool Operator.IOperatorLevelHelper.FitsInPhysicalRegister( TypeRepresentation td )
        {
            return CanFitTypeInPhysicalRegister( td );
        }

        protected abstract bool CanFitTypeInPhysicalRegister( TypeRepresentation td );

        //--//

        public abstract bool CanPropagateCopy( SingleAssignmentOperator opSrc               ,
                                               Operator                 opDst               ,
                                               int                      exIndexInDst        ,
                                               VariableExpression[]     variables           ,
                                               BitVector[]              variableUses        ,
                                               BitVector[]              variableDefinitions ,
                                               Operator[]               operators           );

        //--//

        public static void SetConstraintOnResultsBasedOnType( CompilationSteps.PhaseExecution.NotificationContext nc      ,
                                                              VariableExpression[]                                results )
        {
            for(int i = 0; i < results.Length; i++)
            {
                SetConstraintOnLhsBasedOnType( nc, results, i );
            }
        }

        public static void SetConstraintOnLhsBasedOnType( CompilationSteps.PhaseExecution.NotificationContext nc       ,
                                                          VariableExpression[]                                lhs      ,
                                                          int                                                 varIndex )
        {
            SetConstraintBasedOnType( nc, lhs[varIndex], varIndex, true );
        }

        public static void SetConstraintOnArgumentsBasedOnType( CompilationSteps.PhaseExecution.NotificationContext nc        ,
                                                                Expression[]                                        arguments )
        {
            for(int i = 0; i < arguments.Length; i++)
            {
                SetConstraintOnRhsBasedOnType( nc, arguments, i );
            }
        }

        public static void SetConstraintOnRhsBasedOnType( CompilationSteps.PhaseExecution.NotificationContext nc       ,
                                                          Expression[]                                        rhs      ,
                                                          int                                                 varIndex )
        {
            SetConstraintBasedOnType( nc, rhs[varIndex], varIndex, false );
        }

        public static void SetConstraintBasedOnType( CompilationSteps.PhaseExecution.NotificationContext nc        ,
                                                     Expression                                          ex        ,
                                                     int                                                 varIndex  ,
                                                     bool                                                fIsResult )
        {
            if(ex != null)
            {
                TypeRepresentation td = ex.Type;

                if(td.IsFloatingPoint)
                {
                    if(td.SizeOfHoldingVariableInWords == 1)
                    {
                        SetConstraint( nc, varIndex, fIsResult, RegisterClass.SinglePrecision );
                    }
                    else
                    {
                        SetConstraint( nc, varIndex, fIsResult, RegisterClass.DoublePrecision );
                    }
                }
                else if(td is PointerTypeRepresentation)
                {
                    SetConstraint( nc, varIndex, fIsResult, RegisterClass.Address );
                }
                else
                {
                    SetConstraint( nc, varIndex, fIsResult, RegisterClass.Integer );
                }
            }
        }

        public static void SetConstraintOnLHS( CompilationSteps.PhaseExecution.NotificationContext nc         ,
                                               int                                                 varIndex   ,
                                               RegisterClass                                       constraint )
        {
            SetConstraint( nc, varIndex, true, constraint );
        } 

        public static void SetConstraintOnRHS( CompilationSteps.PhaseExecution.NotificationContext nc         ,
                                               int                                                 varIndex   ,
                                               RegisterClass                                       constraint )
        {
            SetConstraint( nc, varIndex, false, constraint );
        } 

        public static void SetConstraint( CompilationSteps.PhaseExecution.NotificationContext nc         ,
                                          int                                                 varIndex   ,
                                          bool                                                fIsResult  ,
                                          RegisterClass                                       constraint )
        {
            nc.CurrentOperator.AddAnnotation( RegisterAllocationConstraintAnnotation.Create( nc.TypeSystem, varIndex, fIsResult, constraint ) );
        }

        //--//

        public static bool MoveToPseudoRegisterIfConstant( ControlFlowGraphStateForCodeTransformation cfg ,
                                                           Operator                                   op  ,
                                                           Expression                                 ex  )
        {
            if(ex is ConstantExpression)
            {
                MoveToPseudoRegister( cfg, op, ex );

                return true;
            }

            return false;
        }

        public static VariableExpression MoveToPseudoRegister( ControlFlowGraphStateForCodeTransformation cfg ,
                                                               Operator                                   op  ,
                                                               Expression                                 ex  )
        {
            VariableExpression exReg = cfg.AllocatePseudoRegister( ex.Type );

            op.AddOperatorBefore( SingleAssignmentOperator.New( op.DebugInfo, exReg, ex ) );

            op.SubstituteUsage( ex, exReg );

            return exReg;
        }

        public static PseudoRegisterExpression AllocatePseudoRegisterIfNeeded( ControlFlowGraphStateForCodeTransformation cfg ,
                                                                               Expression                                 ex  )
        {
            var var = ex as VariableExpression;
            if(var != null)
            {            
                var exStack = var.AliasedVariable as StackLocationExpression;
                if(exStack != null)
                {
                    return cfg.AllocatePseudoRegister( exStack.Type, exStack.DebugName );
                }
            }

            return null;
        }

        public static PseudoRegisterExpression AllocatePseudoRegisterIfNeeded( ControlFlowGraphStateForCodeTransformation cfg         ,
                                                                               Expression                                 ex          ,
                                                                               bool                                       fConstantOk )
        {
            var res = AllocatePseudoRegisterIfNeeded( cfg, ex );
            if(res == null)
            {
                if(fConstantOk == false && ex is ConstantExpression)
                {
                    res = cfg.AllocatePseudoRegister( ex.Type );
                }
            }

            return res;
        }
    }
}

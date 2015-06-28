//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public sealed partial class ArmV4
    {
        public class Optimizations
        {
            //
            // State
            //

            ArmPlatform m_owner;

            //
            // Constructor Methods
            //

            public Optimizations( ArmPlatform owner )
            {
                m_owner = owner;
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.OptimizationHandler(RunOnce=false, RunInSSAForm=true)]
            private void OptimizeIndirectOperators( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                CompactIndirectOperators            ( nc ); if(nc.CanContinue == false) return;
                MergeIndirectOperatorWithIndexUpdate( nc ); if(nc.CanContinue == false) return;
                FuseSpecialRegisterUpdates             ( nc );
            }

            private void CompactIndirectOperators( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var cfg = nc.CurrentCFG;

                using(cfg.GroupLock( cfg.LockSpanningTree(), cfg.LockUseDefinitionChains() ))
                {
                    var def     = cfg.DataFlow_DefinitionChains;
                    var visited = new BitVector();

                    foreach(var op in cfg.FilterOperators< ZeligIR.IndirectOperator >())
                    {
                        //
                        // Peephole optimization.
                        //
                        // Input:
                        //
                        //      <address2> = <address1> +/- <const>
                        //      Load/Store <address2>,<offset>
                        //
                        // Output:
                        //
                        //      Load/Store <address2>,<offset> +/- <const>
                        //
                        var opSrc = ZeligIR.ControlFlowGraphStateForCodeTransformation.FindOrigin( op.FirstArgument, def, visited ) as ZeligIR.BinaryOperator;
                        if(opSrc != null)
                        {
                            ZeligIR.VariableExpression exAddress;
                            long                       val;

                            if(opSrc.IsAddOrSubAgainstConstant( out exAddress, out val ))
                            {
                                var alias = exAddress.AliasedVariable;

                                if(alias is ZeligIR.PseudoRegisterExpression   ||
                                   alias is ZeligIR.PhysicalRegisterExpression  )
                                {
                                    long iNewOffset = op.Offset + val;

                                    if(Math.Abs( iNewOffset ) < m_owner.GetOffsetLimit( op.Type ))
                                    {
                                        Debugging.DebugInfo debugInfo = op.DebugInfo;
                                        TypeRepresentation  td        = op.Type;
                                        ZeligIR.Operator    opNew;

                                        if(op is ZeligIR.StoreIndirectOperator)
                                        {
                                            opNew = ZeligIR.StoreIndirectOperator.New( debugInfo, td, exAddress, op.SecondArgument, op.AccessPath, (int)iNewOffset, false );
                                        }
                                        else
                                        {
                                            opNew = ZeligIR.LoadIndirectOperator.New( debugInfo, td, op.FirstResult, exAddress, op.AccessPath, (int)iNewOffset, op.MayMutateExistingStorage, false );
                                        }

                                        op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                                        nc.MarkAsModified();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.FuseOperators) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.BinaryOperator) )]
            private static void MergeOperationWithShift( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var cfg =                         nc.CurrentCFG;
                var op  = (ZeligIR.BinaryOperator)nc.CurrentOperator;

                switch(op.Alu)
                {
                    case ZeligIR.AbstractBinaryOperator.ALU.ADD:
                    case ZeligIR.AbstractBinaryOperator.ALU.AND:
                    case ZeligIR.AbstractBinaryOperator.ALU.OR:
                    case ZeligIR.AbstractBinaryOperator.ALU.XOR:
                        if(ConvertToBinaryOperatorWithShift( cfg, op, op.SecondArgument, op.FirstArgument ))
                        {
                            nc.MarkAsModified();
                            return;
                        }

                        goto case ZeligIR.AbstractBinaryOperator.ALU.SUB;

                    case ZeligIR.AbstractBinaryOperator.ALU.SUB:
                        if(ConvertToBinaryOperatorWithShift( cfg, op, op.FirstArgument, op.SecondArgument ))
                        {
                            nc.MarkAsModified();
                            return;
                        }
                        break;
                }
            }

            private static bool ConvertToBinaryOperatorWithShift( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg     ,
                                                                  ZeligIR.BinaryOperator                             op      ,
                                                                  ZeligIR.Expression                                 exLeft  ,
                                                                  ZeligIR.Expression                                 exRight )
            {
                if(exLeft is ZeligIR.VariableExpression)
                {
                    var opShift = cfg.FindSingleDefinition( exRight ) as ZeligIR.BinaryOperator;
                    if(opShift != null)
                    {
                        switch(opShift.Alu)
                        {
                            case ZeligIR.AbstractBinaryOperator.ALU.SHL:
                            case ZeligIR.AbstractBinaryOperator.ALU.SHR:
                                var opNew = ARM.BinaryOperatorWithShift.New( op.DebugInfo, op.Alu, op.Signed, opShift.Alu, opShift.Signed, op.FirstResult, exLeft, opShift.FirstArgument, opShift.SecondArgument );

                                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );
                                return true;
                        }
                    }
                }

                return false;
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.BinaryOperatorWithShift) )]
            private static void Handle_BinaryOperatorWithShift( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.Operator op = nc.CurrentOperator;

                SetConstraintOnResultsBasedOnType  ( nc, op.Results   );
                SetConstraintOnArgumentsBasedOnType( nc, op.Arguments );
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            class ScoreBoardForIndirectOperatorWithIndexUpdate
            {
                internal ZeligIR.IndirectOperator   m_opTarget;
                internal ZeligIR.BinaryOperator     m_opVarIndexUpdate;
                internal long                       m_C1;
                internal ZeligIR.VariableExpression m_indexPre;
                internal ZeligIR.VariableExpression m_indexPost;
            }

            private void MergeIndirectOperatorWithIndexUpdate( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var cfg = nc.CurrentCFG;

////            if(cfg.ToString() == "FlowGraph(void Microsoft.VoxSoloFormFactorLoader.Loader::ExitCFI(ushort*))")
////            if(cfg.ToString() == "FlowGraph(void Microsoft.Zelig.Runtime.Memory::Fill(System.UIntPtr,System.UIntPtr,uint))")
////            if(cfg.ToString() == "FlowGraph(void Microsoft.Zelig.Runtime.Memory::CopyNonOverlapping(System.UIntPtr,System.UIntPtr,System.UIntPtr))")
////            {
////                
////            }

                //////////////////////////////////////////// 
                //
                // Before:
                //          Access [ V0 +/- C1 ]
                //          V1 = V0 +/- C1                  << This use must be dominated by Access
                //
                // After:
                //          V1 = Access [ V0 +/- C1 ] !
                // 
                //////////////////////////////////////////// 
                //
                // Before:
                //          Access [ V0 ]
                //          V1 = V0 +/- C1                  << This use must be dominated by Access
                //
                // After:
                //          V1 = Access [ V0 ] +/- C1 !
                // 
                //////////////////////////////////////////// 
                //
                // Before:
                //          V1 = V0 +/- C1             
                //          Access [ V1 ]                   << All the other uses of V1 must be dominated by Access *and* V0 must be alive at Access
                //
                // After:
                //          V1 = Access [ V0 +/- C1 ] !
                // 
                //////////////////////////////////////////// 
                //
                using(cfg.GroupLock( cfg.LockSpanningTree       () ,
                                     cfg.LockDominance          () ,
                                     cfg.LockUseDefinitionChains() ))
                {
                    var dominance = cfg.DataFlow_Dominance;
                    var use       = cfg.DataFlow_UseChains;
                    var def       = cfg.DataFlow_DefinitionChains;
                    var visited   = new BitVector();
                    var workList  = HashTableFactory.NewWithReferenceEquality< ZeligIR.BinaryOperator, ScoreBoardForIndirectOperatorWithIndexUpdate >();

                    foreach(var opTarget in cfg.FilterOperators< ZeligIR.IndirectOperator >())
                    {
                        var varIndex = opTarget.FirstArgument as ZeligIR.VariableExpression;
                        if(varIndex != null)
                        {
                            bool fLoad  = opTarget is ZeligIR.LoadIndirectOperator;
                            bool fStore = opTarget is ZeligIR.StoreIndirectOperator;

                            if(!fLoad && !fStore)
                            {
                                continue;
                            }

                            //
                            // Look for this pattern:
                            //
                            //    Access [ V0 +/- C1 ] or Access [ V0 ]
                            //    V1 = V0 +/- C1
                            //
                            // Or
                            //
                            //    V1 = V0 +/- C1
                            //    Access [ V0 +/- C1 ] or Access [ V0 ]
                            //
                            foreach(var opVarIndexUse in use[varIndex.SpanningTreeIndex])
                            {
                                if(opTarget.BasicBlock != opVarIndexUse.BasicBlock)
                                {
                                    //
                                    // For now, let's limit optimization to same basic block.
                                    //
                                    continue;
                                }

                                ZeligIR.VariableExpression V0;
                                long                       C1;

                                if(opVarIndexUse.IsAddOrSubAgainstConstant( out V0, out C1 ) && V0 == varIndex && Math.Abs( C1 ) < m_owner.GetOffsetLimit( opTarget.Type ))
                                {
                                    ZeligIR.VariableExpression V1 = opVarIndexUse.FirstResult;

                                    if(opTarget.Offset == 0 || opTarget.Offset == C1)
                                    {
                                        //
                                        // Found "V1 = V0 +/- C1"
                                        // It's a possible candidate.
                                        //
                                        var opVarIndexUse2 = (ZeligIR.BinaryOperator)opVarIndexUse;

                                        //--//

                                        if(opVarIndexUse2.SpanningTreeIndex > opTarget.SpanningTreeIndex)
                                        {
                                            //
                                            // Case:
                                            //
                                            //    Access [ V0 +/- C1 ] or Access [ V0 ]
                                            //    V1 = V0 +/- C1
                                            //

                                            //
                                            // Is V0 used after opTarget?
                                            // If so, it's not a good candidate, we'd need to move V0 to another register, undoing the saving from the optimization.
                                            //
                                            foreach(var opOtherUse in use[V0.SpanningTreeIndex])
                                            {
                                                if(opOtherUse == opTarget      ) continue;
                                                if(opOtherUse == opVarIndexUse2) continue;

                                                if(opOtherUse.IsDominatedBy( opTarget, dominance ))
                                                {
                                                    opVarIndexUse2 = null;
                                                    break;
                                                }
                                            }

                                            if(opVarIndexUse2 == null)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            //
                                            // Case:
                                            //
                                            //    V1 = V0 +/- C1
                                            //    Access [ V0 +/- C1 ] or Access [ V0 ]
                                            //

                                            //
                                            // Is V1 used before opTarget?
                                            // If so, it's not a good candidate.
                                            //
                                            foreach(var opOtherUse in use[V1.SpanningTreeIndex])
                                            {
                                                if(opOtherUse.BasicBlock == opTarget.BasicBlock)
                                                {
                                                    var idx = opOtherUse.SpanningTreeIndex;

                                                    if(idx > opVarIndexUse.SpanningTreeIndex && idx < opTarget.SpanningTreeIndex)
                                                    {
                                                        opVarIndexUse2 = null;
                                                        break;
                                                    }
                                                }
                                            }

                                            if(opVarIndexUse2 == null)
                                            {
                                                continue;
                                            }
                                        }

                                        //--//

                                        ScoreBoardForIndirectOperatorWithIndexUpdate scoreBoardOld;

                                        if(workList.TryGetValue( opVarIndexUse2, out scoreBoardOld ))
                                        {
                                            if(scoreBoardOld.m_opTarget.IsDominatedBy( opTarget, dominance ))
                                            {
                                                continue;
                                            }
                                        }

                                        var scoreboard = new ScoreBoardForIndirectOperatorWithIndexUpdate()
                                                         {
                                                            m_opTarget         = opTarget,
                                                            m_opVarIndexUpdate = opVarIndexUse2,
                                                            m_C1               = C1,
                                                            m_indexPre         = V0,
                                                            m_indexPost        = V1,
                                                         };

                                        workList[opVarIndexUse2] = scoreboard;
                                    }
                                }
                            }

                            //
                            // Look for this pattern:
                            //
                            //    V1 = V0 +/- C1             
                            //    Access [ V1 ]                   << All the other uses of V1 must be dominated by Access *and* V0 must be alive at Access
                            //
                            if(opTarget.Offset == 0)
                            {
                                var opVarIndexDef = ZeligIR.ControlFlowGraphStateForCodeTransformation.FindOrigin( opTarget.FirstArgument, def, visited ) as ZeligIR.BinaryOperator;
                                if(opVarIndexDef != null)
                                {
                                    ZeligIR.VariableExpression V0;
                                    long                       C1;

                                    if(opVarIndexDef.IsAddOrSubAgainstConstant( out V0, out C1 ) && Math.Abs( C1 ) < m_owner.GetOffsetLimit( opTarget.Type ))
                                    {
                                        //
                                        // Found "V1 = V0 +/- C1"
                                        //
                                        // It's a possible candidate.
                                        //

                                        //
                                        // Is V1 used between opVarIndexDef and opTarget?
                                        // If so, it's not a good candidate.
                                        //
                                        foreach(var opOtherUse in use[varIndex.SpanningTreeIndex])
                                        {
                                            if(opOtherUse.BasicBlock == opTarget.BasicBlock)
                                            {
                                                var idx = opOtherUse.SpanningTreeIndex;

                                                if(idx > opVarIndexDef.SpanningTreeIndex && idx < opTarget.SpanningTreeIndex)
                                                {
                                                    continue;
                                                }

                                                varIndex = null;
                                                break;
                                            }
                                        }

                                        if(varIndex == null)
                                        {
                                            continue;
                                        }

                                        //--//

                                        ScoreBoardForIndirectOperatorWithIndexUpdate scoreBoardOld;

                                        if(workList.TryGetValue( opVarIndexDef, out scoreBoardOld ))
                                        {
                                            if(scoreBoardOld.m_opTarget.IsDominatedBy( opTarget, dominance ))
                                            {
                                                continue;
                                            }
                                        }

                                        var scoreboard = new ScoreBoardForIndirectOperatorWithIndexUpdate()
                                                         {
                                                            m_opTarget         = opTarget,
                                                            m_opVarIndexUpdate = opVarIndexDef,
                                                            m_C1               = C1,
                                                            m_indexPre         = V0,
                                                            m_indexPost        = varIndex,
                                                         };

                                        workList[opVarIndexDef] = scoreboard;
                                    }
                                }
                            }
                        }
                    }

                    foreach(var scoreboard in workList.Values)
                    {
                        var opTarget         = scoreboard.m_opTarget;
                        var opVarIndexUpdate = scoreboard.m_opVarIndexUpdate;

                        if(opTarget        .IsDetached || 
                           opVarIndexUpdate.IsDetached  )
                        {
                            continue;
                        }

                        var indexPre  = MoveToPseudoRegister( cfg, opTarget, scoreboard.m_indexPre );
                        var indexPost = scoreboard.m_indexPost;

                        var di     = opTarget.DebugInfo;
                        var td     = opTarget.Type;
                        var ap     = opTarget.AccessPath;
                        var offset = opTarget.Offset;

                        ZeligIR.Operator opNew;

                        if(opTarget is ZeligIR.LoadIndirectOperator)
                        {
                            opNew = ARM.LoadIndirectOperatorWithIndexUpdate.New( di, td, opTarget.FirstResult, indexPost, indexPre, ap, (int)scoreboard.m_C1, opTarget.MayMutateExistingStorage, offset == 0 );

                            opNew.AddAnnotation( ZeligIR.RegisterCouplingConstraintAnnotation.Create( nc.TypeSystem, 1, true, 0, false ) );
                        }
                        else
                        {
                            opNew = ARM.StoreIndirectOperatorWithIndexUpdate.New( di, td, indexPost, indexPre, opTarget.SecondArgument, ap, (int)scoreboard.m_C1, opTarget.MayMutateExistingStorage, offset == 0 );

                            opNew.AddAnnotation( ZeligIR.RegisterCouplingConstraintAnnotation.Create( nc.TypeSystem, 0, true, 0, false ) );
                        }

                        opTarget.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );
                        
                        opVarIndexUpdate.Delete();

                        nc.MarkAsModified();
                    }
                }
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            class ScoreBoardForFuseSpecialRegisterUpdates
            {
                internal ZeligIR.Expression               m_src;
                internal long                             m_C1;
                internal ZeligIR.SingleAssignmentOperator m_target;
            }

            private void FuseSpecialRegisterUpdates( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var cfg = nc.CurrentCFG;

                using(cfg.GroupLock( cfg.LockSpanningTree       () ,
                                     cfg.LockDominance          () ,
                                     cfg.LockUseDefinitionChains() ))
                {
                    var workList = HashTableFactory.NewWithReferenceEquality< ZeligIR.SingleAssignmentOperator, ScoreBoardForFuseSpecialRegisterUpdates >();

                    FuseSpecialRegisterUpdates( cfg, workList, EncodingDefinition.c_register_sp );
                    FuseSpecialRegisterUpdates( cfg, workList, EncodingDefinition.c_register_lr );

                    foreach(var scoreboard in workList.Values)
                    {
                        var opTarget = scoreboard.m_target;

                        if(opTarget.IsDetached)
                        {
                            continue;
                        }

                        ZeligIR.BinaryOperator opNew;

                        if(scoreboard.m_C1 >= 0)
                        {
                            opNew = ZeligIR.BinaryOperator.New( opTarget.DebugInfo, ZeligIR.AbstractBinaryOperator.ALU.ADD, false, false, opTarget.FirstResult, scoreboard.m_src, nc.TypeSystem.CreateConstant( (uint)scoreboard.m_C1 ) );
                        }
                        else
                        {
                            opNew = ZeligIR.BinaryOperator.New( opTarget.DebugInfo, ZeligIR.AbstractBinaryOperator.ALU.SUB, false, false, opTarget.FirstResult, scoreboard.m_src, nc.TypeSystem.CreateConstant( (uint)-scoreboard.m_C1 ) );
                        }

                        opTarget.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                        nc.MarkAsModified();
                    }
                }
            }

            private void FuseSpecialRegisterUpdates( ZeligIR.ControlFlowGraphStateForCodeTransformation                                             cfg            ,
                                                     GrowOnlyHashTable< ZeligIR.SingleAssignmentOperator, ScoreBoardForFuseSpecialRegisterUpdates > workList       ,
                                                     uint                                                                                           targetEncoding )
            {
                var dominance = cfg.DataFlow_Dominance;
                var use       = cfg.DataFlow_UseChains;
                var def       = cfg.DataFlow_DefinitionChains;

                foreach(var opTarget in cfg.FilterOperators< ZeligIR.SingleAssignmentOperator >())
                {
                    var regTarget = ZeligIR.Abstractions.RegisterDescriptor.ExtractFromExpression( opTarget.FirstResult );
                    if(regTarget != null && regTarget.Encoding == targetEncoding)
                    {
                        var varSrc1 = opTarget.FirstArgument as ZeligIR.VariableExpression;
                        if(varSrc1 != null)
                        {
                            var                        opSrc1 = def[varSrc1.SpanningTreeIndex][0];
                            ZeligIR.VariableExpression varSrc2;
                            long                       C1;

                            if(opSrc1.IsAddOrSubAgainstConstant( out varSrc2, out C1 ))
                            {
                                var opSrc2 = def[varSrc2.SpanningTreeIndex][0] as ZeligIR.SingleAssignmentOperator;
                                if(opSrc2 != null)
                                {
                                    var varOrigin = opSrc2.FirstArgument;
                                    var regOrigin = ZeligIR.Abstractions.RegisterDescriptor.ExtractFromExpression( varOrigin );
                                    if(regOrigin != null && regOrigin.Encoding == targetEncoding)
                                    {
                                        var scoreboard = new ScoreBoardForFuseSpecialRegisterUpdates()
                                                         {
                                                             m_src    = varOrigin,
                                                             m_C1     = C1,
                                                             m_target = opTarget,
                                                         };

                                        workList[opTarget] = scoreboard;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.IndirectOperatorWithIndexUpdate) )]
            private static void Handle_IndirectOperatorWithIndexUpdate( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.Operator op = nc.CurrentOperator;

                SetConstraintOnResultsBasedOnType  ( nc, op.Results   );
                SetConstraintOnArgumentsBasedOnType( nc, op.Arguments );
            }
        }
    }
}

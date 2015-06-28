//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public static class StaticSingleAssignmentForm
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        public static bool ShouldTransformInto( ControlFlowGraphStateForCodeTransformation cfg )
        {
            return ComputeCandidatesForTransformIntoSSA( cfg ) != null;
        }

        //--//

        public static void RemovePiOperators( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "RemovePiOperators-Pre" );

            foreach(var op in cfg.FilterOperators< PiOperator >())
            {
                var opNew = SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, op.FirstArgument );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
            }

            cfg.TraceToFile( "RemovePiOperators-Post" );
        }

        public static void InsertPiOperators( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "InsertPiOperators-Pre" );

            TypeSystemForCodeTransformation ts = cfg.TypeSystem;

            foreach(var op in cfg.FilterOperators< ConditionCodeConditionalControlOperator >())
            {
                InsertPiOperators( ts, op );
            }

            cfg.TraceToFile( "InsertPiOperators-Post" );
        }

        private static void InsertPiOperators( TypeSystemForCodeTransformation         ts     ,
                                               ConditionCodeConditionalControlOperator flowOp )
        {
            PiOperator.Relation relation = PiOperator.ConvertRelation( flowOp.Condition );
            if(relation != PiOperator.Relation.Invalid)
            {
                CompareOperator cmpOp = flowOp.GetPreviousOperator() as CompareOperator;

                if(cmpOp != null && cmpOp.FirstResult == flowOp.FirstArgument)
                {
                    Expression exLeft  = cmpOp.FirstArgument;
                    Expression exRight = cmpOp.SecondArgument;

                    //
                    // Generate the constraints for the "comparison is true" branch.
                    //
                    AddPiOperator( flowOp, flowOp.TargetBranchTaken, exLeft , exLeft, exRight, relation );
                    AddPiOperator( flowOp, flowOp.TargetBranchTaken, exRight, exLeft, exRight, relation );

                    //
                    // Generate the constraints for the "comparison is false" branch.
                    //
                    relation = PiOperator.NegateRelation( relation );

                    AddPiOperator( flowOp, flowOp.TargetBranchNotTaken, exLeft , exLeft, exRight, relation );
                    AddPiOperator( flowOp, flowOp.TargetBranchNotTaken, exRight, exLeft, exRight, relation );
                }
            }
        }

        private static void AddPiOperator( Operator            opSrc       ,
                                           BasicBlock          bbDst       ,
                                           Expression          ex          ,
                                           Expression          cmpLeft     ,
                                           Expression          cmpRight    ,
                                           PiOperator.Relation cmpRelation )
        {
            VariableExpression var = ex as VariableExpression;

            if(var != null)
            {
                //
                // Make sure that we put the Pi operator in a basic block immediately dominated by the location of the comparison.
                //
                if(bbDst.Predecessors.Length > 1)
                {
                    bbDst = opSrc.BasicBlock.InsertNewSuccessor( bbDst );
                }

                var opDst = FirstOperatorAfterPiOperators( bbDst, var );
                if(opDst != null)
                {
                    PiOperator opNew = PiOperator.New( opSrc.DebugInfo, var, var, cmpLeft, cmpRight, cmpRelation );

                    opDst.AddOperatorBefore( opNew );
                }
            }
        }

        private static Operator FirstOperatorAfterPiOperators( BasicBlock         bb  ,
                                                               VariableExpression var )
        {
            foreach(Operator op in bb.Operators)
            {
                if(var == null)
                {
                    return op;
                }

                var pi = op as PiOperator;
                if(pi == null)
                {
                    return op;
                }

                if(pi.FirstArgument == var)
                {
                    var = null; // Return the next operator.
                }
            }

            throw TypeConsistencyErrorException.Create( "Cannot find non-PI operator in {0}", bb );
        }

        //--//

        private static bool[] ComputeCandidatesForTransformIntoSSA( ControlFlowGraphStateForCodeTransformation cfg )
        {
            using(new PerformanceCounters.ContextualTiming( cfg, "ComputeCandidatesForTransformIntoSSA" ))
            {
                Operator[][] defChains = cfg.DataFlow_DefinitionChains;
                BitVector[]  liveness  = cfg.DataFlow_LivenessAtBasicBlockEntry;
                int          varNum    = defChains.Length;

                //
                // Are we already in SSA form?
                //
                // We need to convert into SSA form only if:
                //  1) A variable is defined twice or more.
                //  2) A varialbe is defined once and it's an argument.
                //
                bool[] candidate      = new bool[varNum];
                bool   fShouldProcess = false;

                for(int varIdx = 0; varIdx < varNum; varIdx++)
                {
                    Operator[] defs = defChains[varIdx];

                    if(defs.Length > 1)
                    {
                        candidate[varIdx] = true;
                        fShouldProcess = true;
                    }
                }

                return fShouldProcess ? candidate : null;
            }
        }

        public static bool ConvertInto( ControlFlowGraphStateForCodeTransformation cfg )
        {
            using(new PerformanceCounters.ContextualTiming( cfg, "ConvertIntoSSA" ))
            {
                cfg.TraceToFile( "ConvertInto-Pre" );

                //
                // To properly capture the dependencies between variables and exception handlers,
                // we need to create a basic block boundary at each exception site.
                //
                SplitBasicBlocksAtExceptionSites.Execute( cfg );

                using(cfg.GroupLock( cfg.LockSpanningTree       () ,
                                     cfg.LockUseDefinitionChains() ,
                                     cfg.LockLiveness           () ))
                {
                    bool[] candidate = ComputeCandidatesForTransformIntoSSA( cfg );
                    if(candidate == null)
                    {
                        return false;
                    }

                    cfg.TraceToFile( "ConvertInto-Entry" );

                    using(cfg.LockDominance())
                    {
                        VariableExpression[]    variables = cfg.DataFlow_SpanningTree_Variables;
                        Operator[][]            defChains = cfg.DataFlow_DefinitionChains;
                        BitVector[]             liveness  = cfg.DataFlow_LivenessAtBasicBlockEntry;
                        int                     varNum    = variables.Length;
                        GrowOnlySet< Operator > newPhis   = SetFactory.NewWithReferenceEquality< Operator >();

                        //
                        // Compute Iterated Dominance Frontier for each variable assigned.
                        //
                        BasicBlock[]        basicBlocks         = cfg.DataFlow_SpanningTree_BasicBlocks;
                        BasicBlock[]        immediateDominators = cfg.DataFlow_ImmediateDominators;
                        BitVector[]         dominanceFrontier   = cfg.DataFlow_DominanceFrontier;

                        int                 bbNum               = basicBlocks.Length;
                        Operator[]          phiInsertionOps     = new Operator[bbNum];

                        //
                        // Insert Phi-operators.
                        //
                        BitVector vec          = new BitVector( bbNum );
                        BitVector vecDF_N      = new BitVector( bbNum );
                        BitVector vecDF_Nplus1 = new BitVector( bbNum );

                        for(int varIdx = 0; varIdx < varNum; varIdx++)
                        {
                            if(candidate[varIdx])
                            {
                                VariableExpression var  = variables[varIdx];
                                Operator[]         defs = defChains[varIdx];

                                vec.ClearAll();

                                //
                                // Build S
                                //
                                foreach(Operator op in defs)
                                {
                                    vec.Set( op.BasicBlock.SpanningTreeIndex );
                                }

                                if(vec.Cardinality > 1)
                                {
                                    //
                                    // Compute DF^1(S)
                                    //
                                    vecDF_N.ClearAll();

                                    foreach(int bbIdx in vec)
                                    {
                                        vecDF_N.OrInPlace( dominanceFrontier[bbIdx] );
                                    }

                                    //
                                    // Build DF+(S), by computing DF^(N+1)(S) = DF(S union DF^N(S)).
                                    //
                                    while(true)
                                    {
                                        vecDF_Nplus1.ClearAll();

                                        //
                                        // DF(S)
                                        //
                                        foreach(int bbIdx in vec)
                                        {
                                            vecDF_Nplus1.OrInPlace( dominanceFrontier[bbIdx] );
                                        }

                                        //
                                        // DF(DF^N(S))
                                        //
                                        foreach(int bbIdx in vecDF_N)
                                        {
                                            vecDF_Nplus1.OrInPlace( dominanceFrontier[bbIdx] );
                                        }

                                        if(vecDF_Nplus1 == vecDF_N)
                                        {
                                            break;
                                        }

                                        vecDF_N.Assign( vecDF_Nplus1 );
                                    }

                                    //
                                    // For each node in DF+(S), where <var> is live at entry, add a phi operator.
                                    //
                                    foreach(int bbIdx in vecDF_N)
                                    {
                                        if(liveness[bbIdx][varIdx])
                                        {
                                            PhiOperator phiOp = PhiOperator.New( null, var );

                                            newPhis.Insert( phiOp );

                                            var phiInsertionOp = phiInsertionOps[bbIdx];
                                            if(phiInsertionOp == null)
                                            {
                                                phiInsertionOp = basicBlocks[bbIdx].GetFirstDifferentOperator( typeof(PhiOperator) );

                                                phiInsertionOps[bbIdx] = phiInsertionOp;
                                            }

                                            phiInsertionOp.AddOperatorBefore( phiOp );
                                        }
                                    }
                                }
                            }
                        }

                        //
                        // Rename variables.
                        //
                        using(new PerformanceCounters.ContextualTiming( cfg, "ProcessInDominatorTreeOrder" ))
                        {
                            ProcessInDominatorTreeOrder( cfg, basicBlocks, immediateDominators, variables, candidate, newPhis );
                        }
                    }
                }

#if DEBUG
                foreach(Operator[] defChainPost in cfg.DataFlow_DefinitionChains)
                {
                    CHECKS.ASSERT( defChainPost.Length <= 1, "Found a variable still defined multiple times after SSA conversion" );
                }
#endif

                cfg.TraceToFile( "ConvertInto-Post" );

                PurgeUselessPhiOperators( cfg );

                cfg.TraceToFile( "ConvertInto-PostPurge" );

                while(Transformations.SimplifyConditionCodeChecks.Execute( cfg ))
                {
                    Transformations.RemoveDeadCode.Execute( cfg, false );
                }

                while(Transformations.CommonMethodRedundancyElimination.Execute( cfg ))
                {
                }

                cfg.DropDeadVariables();

                cfg.TraceToFile( "ConvertInto-Done" );

                return true;
            }
        }

        private static void PurgeUselessPhiOperators( ControlFlowGraphStateForCodeTransformation cfg )
        {
            Operator[]           operators    = cfg.DataFlow_SpanningTree_Operators;
            VariableExpression[] variables    = cfg.DataFlow_SpanningTree_Variables;
            VariableExpression[] variablesMap = new VariableExpression[variables.Length];
            bool                 fLoop        = true;
            bool                 fGot         = false;

            //
            // In 'variablesMap', collect all the variables that are assigned through an identity phi operator.
            //
            while(fLoop)
            {
                fLoop = false;

                foreach(Operator op in operators)
                {
                    if(op is PhiOperator)
                    {
                        VariableExpression lhs = op.FirstResult;

                        if(variablesMap[lhs.SpanningTreeIndex] == null)
                        {
                            VariableExpression var = null;

                            foreach(Expression ex in op.Arguments)
                            {
                                VariableExpression rhs = ex as VariableExpression;

                                if(rhs != null)
                                {
                                    VariableExpression rhsMapped = variablesMap[rhs.SpanningTreeIndex];

                                    if(rhsMapped != null)
                                    {
                                        rhs = rhsMapped;
                                    }

                                    if(var == null)
                                    {
                                        var = rhs;
                                    }
                                    else if(var != rhs)
                                    {
                                        var = null;
                                        break;
                                    }
                                }
                            }

                            if(var != null)
                            {
                                //
                                // If other variables were mapped to 'lhs', update the mapping.
                                //
                                for(int varIdx = variablesMap.Length; --varIdx >= 0;)
                                {
                                    if(variablesMap[varIdx] == lhs)
                                    {
                                        variablesMap[varIdx] = var;
                                    }
                                }

                                variablesMap[lhs.SpanningTreeIndex] = var;
                                fLoop = true;
                                fGot  = true;
                            }
                        }
                    }
                }
            }

            if(fGot)
            {
                //
                // We found some nilpotent phi operators, let's get rid of them.
                // This reduces the number of variables we need to track.
                //
                foreach(Operator op in operators)
                {
                    if(op is PhiOperator)
                    {
                        VariableExpression lhs = op.FirstResult;

                        if(lhs != null && variablesMap[lhs.SpanningTreeIndex] != null)
                        {
                            op.Delete();
                        }
                        else
                        {
                            foreach(Expression ex in op.Arguments)
                            {
                                var rhs = ex as VariableExpression;
                                if(rhs != null)
                                {
                                    VariableExpression rhsMapped = variablesMap[rhs.SpanningTreeIndex];
                                    if(rhsMapped != null)
                                    {
                                        op.SubstituteUsage( rhs, rhsMapped );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //--//

        internal class BasicBlockState
        {
            //
            // State
            //

            internal BasicBlock              m_bb;
            internal BasicBlockEdge[]        m_successors;
            internal PhiVariableExpression[] m_stackAtExit;

            //
            // Constructor Methods
            //

            internal BasicBlockState( BasicBlock bb )
            {
                m_bb         = bb;
                m_successors = bb.Successors;
            }
        }

        private static void ProcessInDominatorTreeOrder( ControlFlowGraphStateForCodeTransformation cfg                 ,
                                                         BasicBlock[]                               basicBlocks         ,
                                                         BasicBlock[]                               immediateDominators ,
                                                         VariableExpression[]                       variables           ,
                                                         bool[]                                     candidate           ,
                                                         GrowOnlySet< Operator >                    newPhis             )
        {
            int                      bbNum            = basicBlocks.Length;
            int                      varNum           = variables  .Length;
            BasicBlockState[]        bbStates         = new BasicBlockState[bbNum];
            Stack< BasicBlockState > pending          = new Stack< BasicBlockState >( bbNum + 2 );
            PhiVariableExpression[]  stack            = new PhiVariableExpression[varNum];
            int                  []  renameHistory    = new int                  [varNum];
            int                      renameHistoryKey = 1;  


            //
            // Process from the end of the spanning tree, so that the entry basic block will be at the top of the stack.
            //
            for(int idx = bbNum; --idx >= 0; )
            {
                BasicBlockState bbs = new BasicBlockState( basicBlocks[idx] );

                bbStates[idx] = bbs;

                pending.Push( bbs );
            }

            //
            // Walk through the basic blocks, in dominator order, and update all the variables with corresponding phi versions.
            //
            while(pending.Count > 0)
            {
                BasicBlockState bbs = pending.Pop();
                BasicBlock      bb  = bbs.m_bb;

                int idx = bb.SpanningTreeIndex;

                if(bbs.m_stackAtExit != null)
                {
                    //
                    // Already processed.
                    //
                    continue;
                }

                PhiVariableExpression[] localStack;
                BasicBlockState         bbsIDOM = bbStates[immediateDominators[idx].SpanningTreeIndex] ;

                if(bbsIDOM == bbs)
                {
                    //
                    // This is an entry node, so we should use the initial state of the variable stack.
                    //
                    localStack = ArrayUtility.CopyNotNullArray( stack );
                }
                else if(bbsIDOM.m_stackAtExit != null)
                {
                    //
                    // Use the state of the variable stack at the exit of the dominator basic block as our input.
                    //
                    localStack = ArrayUtility.CopyNotNullArray( bbsIDOM.m_stackAtExit );
                }
                else
                {
                    //
                    // Process the immediate dominator before the current basic block.
                    //
                    pending.Push( bbs     );
                    pending.Push( bbsIDOM );
                    continue;
                }

                //
                // Rename all the variables.
                //
                var entryStack = ArrayUtility.CopyNotNullArray( localStack );

                foreach(Operator op in bb.Operators)
                {
                    foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation>())
                    {
                        CreateNewVariable( cfg, variables, localStack, candidate, renameHistory, renameHistoryKey, an.Target );
                    }

                    renameHistoryKey++;

                    foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation>())
                    {
                        SubstituteDefinition( localStack, candidate, op, an );
                    }

                    //--//

                    SubstituteUsage( localStack, entryStack, candidate, op );

                    //--//

                    foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation>())
                    {
                        CreateNewVariable( cfg, variables, localStack, candidate, renameHistory, renameHistoryKey, an.Target );
                    }

                    renameHistoryKey++;

                    foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation>())
                    {
                        SubstituteDefinition( localStack, candidate, op, an );
                    }

                    //--//

                    foreach(var lhs in op.Results)
                    {
                        CreateNewVariable( cfg, variables, localStack, candidate, renameHistory, renameHistoryKey, lhs );
                    }

                    renameHistoryKey++;

                    SubstituteDefinition( localStack, candidate, op );
                }

                bbs.m_stackAtExit = localStack;

                //
                // Queue successors for processing.
                //
                foreach(BasicBlockEdge edge in bbs.m_successors)
                {
                    BasicBlockState bbsNext = bbStates[edge.Successor.SpanningTreeIndex];

                    if(bbsNext.m_stackAtExit == null)
                    {
                        pending.Push( bbsNext );
                    }
                }
            }

            //
            // Now that all the variables have been updated, we can fill the phi operators.
            //
            foreach(BasicBlockState bbs in bbStates)
            {
                foreach(BasicBlockEdge edge in bbs.m_successors)
                {
                    BasicBlock bbNext = edge.Successor;

                    foreach(Operator op in bbNext.Operators)
                    {
                        PhiOperator phiOp = op as PhiOperator;

                        if(phiOp != null && newPhis.Contains( phiOp ))
                        {
                            PhiVariableExpression phiVar = (PhiVariableExpression)phiOp.FirstResult;
                            VariableExpression    var    = phiVar.Target;
                            VariableExpression    input  = bbs.m_stackAtExit[var.SpanningTreeIndex];

                            /// 
                            /// TODO: Verify that this is the correct thing to do.  For exception handlers
                            /// the variables of the outer scope are set to NULL for the current bbs.m_stackAtExit
                            /// This code gets the variable in the target basic blocks m_stackAtExit
                            /// 
                            if(input == null)
                            {
                                input = bbStates[bbNext.SpanningTreeIndex].m_stackAtExit[var.SpanningTreeIndex];
                            }

                            CHECKS.ASSERT( input != null, "Inputs to the phi operator have to be defined in method '{0}':\r\n  {1}", op.BasicBlock.Owner.Method.ToShortString(), phiOp );

                            phiOp.AddEffect( input, bbs.m_bb );
                        }
                    }
                }
            }
        }

        private static void CreateNewVariable( ControlFlowGraphStateForCodeTransformation cfg              ,
                                               VariableExpression[]                       variables        ,
                                               PhiVariableExpression[]                    localStack       ,
                                               bool[]                                     candidate        ,
                                               int[]                                      renameHistory    ,
                                               int                                        renameHistoryKey ,
                                               VariableExpression                         lhs              )
        {
            int varIdx = lhs.SpanningTreeIndex;

            if(candidate[varIdx] && renameHistory[varIdx] != renameHistoryKey)
            {
                PhiVariableExpression phiVar = cfg.AllocatePhiVariable( lhs );

                localStack   [varIdx] = phiVar;
                renameHistory[varIdx] = renameHistoryKey;


                PhysicalRegisterExpression reg = lhs.AliasedVariable as PhysicalRegisterExpression;
                if(reg != null)
                {
                    //
                    // Create a new phi variables for all the various typed versions of this physical register.
                    //
                    foreach(VariableExpression reg2 in variables)
                    {
                        int regIdx = reg2.SpanningTreeIndex;

                        if(localStack[regIdx] == null && renameHistory[regIdx] != renameHistoryKey && reg.IsTheSamePhysicalEntity( reg2.AliasedVariable ))
                        {
                            localStack   [regIdx] = cfg.AllocatePhiVariable( reg2 );
                            renameHistory[regIdx] = renameHistoryKey;
                        }
                    }
                }
            }
        }

        private static void SubstituteDefinition( PhiVariableExpression[] localStack ,
                                                  bool[]                  candidate  ,
                                                  Operator                op         ,
                                                  InvalidationAnnotation  an         )
        {
            var lhs    = an.Target;
            int varIdx = lhs.SpanningTreeIndex;

            if(candidate[varIdx])
            {
                var                    newVar = localStack[varIdx];
                InvalidationAnnotation anNew;

                if(an is PreInvalidationAnnotation)
                {
                    anNew = PreInvalidationAnnotation.Create( null, newVar );
                }
                else
                {
                    anNew = PostInvalidationAnnotation.Create( null, newVar );
                }

                op.SubstituteAnnotation( an, anNew );
            }
        }

        private static void SubstituteDefinition( PhiVariableExpression[] localStack ,
                                                  bool[]                  candidate  ,
                                                  Operator                op         )
        {
            foreach(var lhs in op.Results)
            {
                int varIdx = lhs.SpanningTreeIndex;

                if(candidate[varIdx])
                {
                    op.SubstituteDefinition( lhs, localStack[varIdx] );
                }
            }
        }

        private static void SubstituteUsage( PhiVariableExpression[] localStack ,
                                             PhiVariableExpression[] entryStack ,
                                             bool[]                  candidate  ,
                                             Operator                op         )
        {
            foreach(Expression rhs in op.Arguments)
            {
                VariableExpression var = rhs as VariableExpression;

                if(var != null)
                {
                    int varIdx = var.SpanningTreeIndex;

                    if(candidate[varIdx])
                    {
                        //
                        // If a variable is an argument, it won't be defined on any path from entry,
                        // so its lot in localStack will be empty. It's OK, just use the original value.
                        //
                        PhiVariableExpression phiVar;

                        //
                        // Phi and Pi operators are parallel operators.
                        // That's equivalent to using the state of the variables at the entry of the basic block.
                        //
                        if(op is PhiOperator ||
                           op is PiOperator   )
                        {
                            phiVar = entryStack[varIdx];
                        }
                        else
                        {
                            phiVar = localStack[varIdx];
                        }

                        CHECKS.ASSERT( phiVar != null, "Found use of phi-candidate variable that is not reachable by any of its definitions: {0}", op );

                        op.SubstituteUsage( var, phiVar );
                    }
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//

        public static bool ConvertOut( ControlFlowGraphStateForCodeTransformation cfg             ,
                                       bool                                       fAllowPseudoReg )
        {
            using(new PerformanceCounters.ContextualTiming( cfg, "ConvertOutSSA" ))
            {
                bool fShouldProcess = false;

                //
                // First of all, are we still in SSA form?
                // Look for any PhiOperators or PhiVariable.
                //
                foreach(Operator op in cfg.DataFlow_SpanningTree_Operators)
                {
                    if(op is PhiOperator ||
                       op is PiOperator   )
                    {
                        fShouldProcess = true;
                        break;
                    }
                }

                if(fShouldProcess == false)
                {
                    foreach(VariableExpression var in cfg.DataFlow_SpanningTree_Variables)
                    {
                        if(var is PhiVariableExpression)
                        {
                            fShouldProcess = true;
                            break;
                        }
                    }
                }

                if(fShouldProcess)
                {
                    cfg.TraceToFile( "ConvertOut-Pre" );

                    RemovePiOperators                     ( cfg                  );
                    ConvertPhiOperatorsIntoCopies         ( cfg                  );
                    ConvertPhiVariablesIntoNormalVariables( cfg, fAllowPseudoReg );

                    cfg.TraceToFile( "ConvertOut-Post" );

                    while(Transformations.CommonMethodRedundancyElimination.Execute( cfg ))
                    {
                    }

                    cfg.DropDeadVariables();

                    cfg.TraceToFile( "ConvertOut-Done" );
                }

                return fShouldProcess;
            }
        }

        //--//

        internal class InsertionState
        {
            //
            // State
            //

            internal BasicBlock                       m_basicBlockSource;
            internal BasicBlock                       m_basicBlockTarget;
            internal BasicBlock                       m_basicBlockInserted;
            internal List< SingleAssignmentOperator > m_assignments;

            //
            // Constructor Methods
            //

            internal InsertionState( BasicBlock bbSource ,
                                     BasicBlock bbTarget )
            {
                m_basicBlockSource = bbSource;
                m_basicBlockTarget = bbTarget;
                m_assignments      = new List< SingleAssignmentOperator >();
            }

            internal void AllocateBasicBlock()
            {
                m_basicBlockInserted = m_basicBlockSource.InsertNewSuccessor( m_basicBlockTarget );
            }

            internal void Add( Microsoft.Zelig.Debugging.DebugInfo debugInfo ,
                               VariableExpression                  targetVar ,
                               Expression                          sourceEx  )
            {
                m_assignments.Add( SingleAssignmentOperator.New( debugInfo, targetVar, sourceEx ) );
            }
                    
            internal void ScheduleCopies( ControlFlowGraphStateForCodeTransformation cfg )
            {
                //
                // First, get rid of non-interfering copies.
                //
                while(true)
                {
                    InsertNonInterferingCopies();

                    bool fDone = true;

                    for(int pos = 0; pos < m_assignments.Count; pos++)
                    {
                        SingleAssignmentOperator op = m_assignments[pos];
                        if(op != null)
                        {
                            //
                            // At this point, only items in a cyclic graph are left.
                            //
                            // To break the loop, we have to:
                            //  1) create a temporary,
                            //  2) copy the source value into it,
                            //  3) remove the original copy from the set,
                            //  4) schedule all the other copies,
                            //  5) insert back the assignment to the original destination, now from the temporary.
                            //

                            VariableExpression destinationVar =                     op.FirstResult;
                            VariableExpression sourceVar      = (VariableExpression)op.FirstArgument;
                            VariableExpression tmp            =                     AllocateTemporary( cfg, sourceVar.AliasedVariable );

                            m_basicBlockInserted.AddOperator( SingleAssignmentOperator.New( op.DebugInfo, tmp, sourceVar ) );

                            m_assignments[pos] = null;

                            InsertNonInterferingCopies();

                            CHECKS.ASSERT( IsUsedByAnotherCopy( op, destinationVar ) == false, "Oops" );

                            m_assignments[pos] = SingleAssignmentOperator.New( op.DebugInfo, destinationVar, tmp );

                            fDone = false; // One less candidate, try looping again.
                            break;
                        }
                    }

                    if(fDone)
                    {
                        break;
                    }
                }
            }

            //--//

            private VariableExpression AllocateTemporary( ControlFlowGraphStateForCodeTransformation cfg ,
                                                          VariableExpression                         var )
            {
                if(var is PhysicalRegisterExpression)
                {
                    PhysicalRegisterExpression      regVar  = (PhysicalRegisterExpression)var;
                    Abstractions.RegisterDescriptor regDesc = cfg.TypeSystem.PlatformAbstraction.GetScratchRegister();

                    var = cfg.AllocateTypedPhysicalRegister( regVar.Type, regDesc, regVar.DebugName, regVar.SourceVariable, regVar.SourceOffset );
                }

                return cfg.AllocatePhiVariable( var );
            }

            private void InsertNonInterferingCopies()
            {
                while(true)
                {
                    bool fDone = true;

                    for(int pos = 0; pos < m_assignments.Count; pos++)
                    {
                        SingleAssignmentOperator op = m_assignments[pos];
                        if(op != null)
                        {
                            if(IsUsedByAnotherCopy( op, op.FirstResult ) == false)
                            {
                                m_basicBlockInserted.AddOperator( op );

                                m_assignments[pos] = null;

                                fDone = false; // One less candidate, try looping again.
                            }
                        }
                    }

                    if(fDone)
                    {
                        break;
                    }
                }
            }

            private bool IsUsedByAnotherCopy( SingleAssignmentOperator opSrc ,
                                              VariableExpression       var   )
            {
                for(int pos = 0; pos < m_assignments.Count; pos++)
                {
                    var op = m_assignments[pos];
                    if(op != null && op != opSrc) // Skip source operator.
                    {
                        if(var.IsTheSamePhysicalEntity( op.FirstArgument ))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private static void ConvertPhiOperatorsIntoCopies( ControlFlowGraphStateForCodeTransformation cfg )
        {
            List< InsertionState > insertionPoints = new List< InsertionState >();


            //
            // Build the table with all the copy operations that have to be scheduled.
            //
            foreach(var phiOp in cfg.FilterOperators< PhiOperator >())
            {
                Expression[]       rhs       = phiOp.Arguments;
                BasicBlock[]       origins   = phiOp.Origins;
                BasicBlock         target    = phiOp.BasicBlock;
                VariableExpression targetVar = phiOp.FirstResult;

                for(int pos = 0; pos < origins.Length; pos++)
                {
                    Expression     sourceEx       = rhs    [pos];
                    BasicBlock     source         = origins[pos];
                    InsertionState insertionPoint = null;

                    foreach(InsertionState insertionPoint2 in insertionPoints)
                    {
                        if(insertionPoint2.m_basicBlockSource == source &&
                           insertionPoint2.m_basicBlockTarget == target  )
                        {
                            insertionPoint = insertionPoint2;
                            break;
                        }
                    }

                    if(insertionPoint == null)
                    {
                        insertionPoint = new InsertionState( source, target );

                        insertionPoints.Add( insertionPoint );
                    }

                    insertionPoint.Add( phiOp.DebugInfo, targetVar, sourceEx );
                }

                phiOp.Delete();
            }

            //
            // Create a new basic block and redirect the source to it.
            //
            foreach(InsertionState insertionPoint in insertionPoints)
            {
                insertionPoint.AllocateBasicBlock();
            }

            foreach(InsertionState insertionPoint in insertionPoints)
            {
                insertionPoint.ScheduleCopies( cfg );
            }
        }

        //--//

        internal class VariableState
        {
            //
            // State
            //

            internal int                         m_index;
            internal VariableExpression.Property m_properties;
            internal PhiVariableExpression[]     m_phiVariables;
            internal VariableExpression          m_mappedTo;
            internal Operator[]                  m_definitions;
            internal BitVector                   m_livenessMap;

            //
            // Constructor Methods
            //

            internal VariableState( int index )
            {
                m_index        = index;
                m_phiVariables = PhiVariableExpression.SharedEmptyArray;
            }

            internal static VariableState Create( GrowOnlyHashTable< VariableExpression, VariableState > lookup ,
                                                  VariableExpression                                     var    )
            {
                VariableState res;

                if(lookup.TryGetValue( var, out res ) == false)
                {
                    res = new VariableState( var.SpanningTreeIndex );

                    lookup[var] = res;
                }

                return res;
            }

            internal bool IsThereInterference( GrowOnlyHashTable< VariableExpression, VariableState > lookup    ,
                                               BitVector                                              tmp       ,
                                               PhiVariableExpression                                  phiTarget ,
                                               PhiVariableExpression[]                                phis      )
            {
                //
                // We have to find out if there's any overlap in the liveness of different phi variables.
                // If there isn't, we can directly convert them back to registers.
                // Otherwise we need to add extra assignments.
                //
                foreach(PhiVariableExpression phi in phis)
                {
                    if(phi != phiTarget)
                    {
                        tmp.And( m_livenessMap, lookup[phi].m_livenessMap );
                        if(tmp.Cardinality != 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private static void ConvertPhiVariablesIntoNormalVariables( ControlFlowGraphStateForCodeTransformation cfg             ,
                                                                    bool                                       fAllowPseudoReg )
        {
            VariableExpression[]          variables           = cfg.DataFlow_SpanningTree_Variables;
            VariableExpression.Property[] varProps            = cfg.DataFlow_PropertiesOfVariables;
            Operator[][]                  defChains           = cfg.DataFlow_DefinitionChains;
            BitVector[]                   variableLivenessMap = cfg.DataFlow_VariableLivenessMap;

            GrowOnlyHashTable< VariableExpression, VariableState > lookup = HashTableFactory.NewWithReferenceEquality< VariableExpression, VariableState >();

            //
            // Collect all redefinitions of variables.
            //
            foreach(VariableExpression var in variables)
            {
                VariableState         vs;
                PhiVariableExpression phiVar = var as PhiVariableExpression;
                if(phiVar != null)
                {
                    vs = VariableState.Create( lookup, phiVar.AliasedVariable );

                    vs.m_phiVariables = ArrayUtility.AppendToNotNullArray( vs.m_phiVariables, phiVar );
                }

                //--//

                vs = VariableState.Create( lookup, var );

                int idx = var.SpanningTreeIndex;

                vs.m_properties  = varProps           [idx];
                vs.m_livenessMap = variableLivenessMap[idx];
                vs.m_definitions = defChains          [idx];
            }

            //
            // Keep track of all the substitutions.
            //
            // Then go through the Subroutines and Invalidate operators and
            // update the values back to PhysicalRegister and StackLocations.
            // If needed, add assignment operators in front of subroutine calls.
            //
            foreach(VariableExpression var in lookup.Keys)
            {
                VariableState           vs   = lookup[var];
                PhiVariableExpression[] phis = vs.m_phiVariables;

                if(phis.Length == 1)
                {
                    //
                    // Simple case, only one definition, substitute back for the original variable.
                    //
                    lookup[ phis[0] ].m_mappedTo = var;
                }
                else
                {
                    VariableExpression.DebugInfo debugInfo = var.AliasedVariable.DebugName;
                    BitVector                    tmp       = new BitVector();

                    foreach(PhiVariableExpression phi in phis)
                    {
                        VariableState      vsPhi  = lookup[phi];
                        VariableExpression newVar = null;

                        if(var is PhysicalRegisterExpression)
                        {
                            //
                            // We have to find out if there's any overlap in the liveness of different phi variables.
                            // If there isn't, we can directly convert them back to registers.
                            // Otherwise we need to add extra assignments.
                            //
                            if(vsPhi.IsThereInterference( lookup, tmp, phi, phis ) == false)
                            {
                                newVar = var;
                            }
                            else if(vsPhi.m_definitions.Length == 1 && vsPhi.m_definitions[0] is InitialValueOperator)
                            {
                                //
                                // This is an argument. Keep it as is.
                                //
                                newVar = var;
                            }
                            else
                            {
                                //
                                // There's some aliasing problem with this register.
                                // Create a new temporary, add an assignment at the original definition site and propagate that value.
                                //
                                if(fAllowPseudoReg)
                                {
                                    newVar = cfg.AllocatePseudoRegister( var.Type, debugInfo );
                                }
                                else
                                {
                                    newVar = var;
                                }
                            }
                        }
                        else if(var is ConditionCodeExpression)
                        {
                            CHECKS.ASSERT( (vsPhi.m_properties & VariableExpression.Property.AddressTaken) == 0, "Cannot take address of condition code: {0}", phi );

                            //
                            // We have to find out if there's any overlap in the liveness of different phi variables.
                            // If there isn't, we can directly convert them back to registers.
                            // Otherwise we need to add extra assignments.
                            //
                            if(vsPhi.IsThereInterference( lookup, tmp, phi, phis ) == false)
                            {
                                newVar = var;
                            }
                            else if(vsPhi.m_definitions.Length == 1 && vsPhi.m_definitions[0] is InitialValueOperator)
                            {
                                //
                                // This is an argument. Keep it as is.
                                //
                                newVar = var;
                            }
                            else
                            {
                                //
                                // There's some aliasing problem with this register.
                                // Create a new temporary, add an assignment at the original definition site and propagate that value.
                                //
                                if(fAllowPseudoReg)
                                {
                                    newVar = cfg.AllocatePseudoRegister( var.Type, debugInfo );
                                }
                                else
                                {
                                    newVar = var;
                                }
                            }
                        }
                        else if(var is StackLocationExpression)
                        {
                            StackLocationExpression stack = (StackLocationExpression)var;

                            if(vsPhi.IsThereInterference( lookup, tmp, phi, phis ) == false)
                            {
                                newVar = var;
                            }
                            else if(vsPhi.m_definitions.Length == 1 && vsPhi.m_definitions[0] is InitialValueOperator)
                            {
                                //
                                // This is an argument. Keep it as is.
                                //
                                newVar = var;
                            }
                            else
                            {
                                //
                                // There's some aliasing problem with this stack location.
                                // Create a new temporary, add an assignment at the original definition site and propagate that value.
                                //
                                if((vsPhi.m_properties & VariableExpression.Property.AddressTaken) != 0)
                                {
                                    newVar = cfg.AllocateLocalStackLocation( stack.Type, debugInfo, stack.SourceVariable, stack.SourceOffset );
                                }
                                else
                                {
                                    if(fAllowPseudoReg)
                                    {
                                        newVar = cfg.AllocatePseudoRegister( stack.Type, debugInfo, stack.SourceVariable, stack.SourceOffset );
                                    }
                                    else
                                    {
                                        newVar = var;
                                    }
                                }
                            }
                        }
                        else if(var is PseudoRegisterExpression)
                        {
                            PseudoRegisterExpression pseudo = (PseudoRegisterExpression)var;

                            if((vsPhi.m_properties & VariableExpression.Property.AddressTaken) != 0)
                            {
                                newVar = cfg.AllocateLocalStackLocation( pseudo.Type, debugInfo, pseudo.SourceVariable, pseudo.SourceOffset );
                            }
                            else
                            {
                                if(fAllowPseudoReg)
                                {
                                    newVar = cfg.AllocatePseudoRegister( pseudo.Type, debugInfo, pseudo.SourceVariable, pseudo.SourceOffset );
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Expecting a physical expression instead of {0}", var );
                                }
                            }
                        }
                        else
                        {
                            //
                            // WARNING: Convert to or from SSA before mapping to machine words is untested...
                            //
                            if(var is TemporaryVariableExpression)
                            {
                                newVar = cfg.AllocateTemporary( var.Type, debugInfo );
                            }
                            else
                            {
                                newVar = cfg.AllocateLocal( var.Type, debugInfo  );
                            }
                        }

                        vsPhi.m_mappedTo = newVar;
                    }
                }
            }

            foreach(Operator op in cfg.DataFlow_SpanningTree_Operators)
            {
                SubstitutePhiVariables( op, lookup );
            }
        }

        private static void SubstitutePhiVariables( Operator                                               op     ,
                                                    GrowOnlyHashTable< VariableExpression, VariableState > lookup )
        {
            bool fCall = (op is SubroutineOperator);

            foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation >())
            {
                var var    = an.Target;
                var newVar = FindSubstituteForPhiVariable( lookup, var );

                if(newVar != null)
                {
                    VariableExpression origVar = var.AliasedVariable;

                    newVar = AdjustForCallingConvention( origVar, newVar );

                    if(newVar != origVar)
                    {
                        op.AddOperatorBefore( SingleAssignmentOperator.New( op.DebugInfo, origVar, newVar ) );
                    }

                    var anNew = PreInvalidationAnnotation.Create( null, origVar );

                    op.SubstituteAnnotation( an, anNew );
                }
            }

            foreach(Expression ex in op.Arguments)
            {
                var var = ex as VariableExpression;
                if(var != null)
                {
                    var newVar = FindSubstituteForPhiVariable( lookup, var );
                    if(newVar != null)
                    {
                        if(fCall)
                        {
                            VariableExpression origVar = var.AliasedVariable;

                            newVar = AdjustForCallingConvention( origVar, newVar );

                            if(newVar != origVar)
                            {
                                op.AddOperatorBefore( SingleAssignmentOperator.New( op.DebugInfo, origVar, newVar ) );
                            }

                            op.SubstituteUsage( var, origVar );
                        }
                        else
                        {
                            op.SubstituteUsage( var, newVar );
                        }
                    }
                }
            }

            foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation >())
            {
                var var    = an.Target;
                var newVar = FindSubstituteForPhiVariable( lookup, var );

                if(newVar != null)
                {
                    VariableExpression origVar = var.AliasedVariable;

                    newVar = AdjustForCallingConvention( origVar, newVar );

                    if(newVar != origVar)
                    {
                        op.AddOperatorAfter( SingleAssignmentOperator.New( op.DebugInfo, newVar, origVar ) );
                    }

                    var anNew = PostInvalidationAnnotation.Create( null, origVar );

                    op.SubstituteAnnotation( an, anNew );
                }
            }

            foreach(var var in op.Results)
            {
                var newVar = FindSubstituteForPhiVariable( lookup, var );
                if(newVar != null)
                {
                    op.SubstituteDefinition( var, newVar );
                }
            }
        }

        private static VariableExpression FindSubstituteForPhiVariable( GrowOnlyHashTable< VariableExpression, VariableState > lookup ,
                                                                        VariableExpression                                     var    )
        {
            VariableState vs;

            if(lookup.TryGetValue( var, out vs ))
            {
                return vs.m_mappedTo;
            }

            return null;
        }

        private static VariableExpression AdjustForCallingConvention( VariableExpression origVar ,
                                                                      VariableExpression newVar  )
        {
            if(origVar is PhysicalRegisterExpression)
            {
                return origVar;
            }
            else if(origVar is ConditionCodeExpression)
            {
                return origVar;
            }
            else if(origVar is StackLocationExpression)
            {
                StackLocationExpression stack = (StackLocationExpression)origVar;

                if(stack.StackPlacement == StackLocationExpression.Placement.Out)
                {
                    return stack;
                }
            }

            return newVar;
        }

        //
        // Access Methods
        //
    }
}

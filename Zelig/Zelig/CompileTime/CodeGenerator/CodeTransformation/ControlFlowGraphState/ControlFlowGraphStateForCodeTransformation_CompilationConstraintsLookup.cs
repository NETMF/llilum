//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation
    {
        struct MergeHelper
        {
            //
            // State
            //

            CompilationConstraints[] m_array1;
            CompilationConstraints[] m_array2;
            CompilationConstraints[] m_arrayRes;

            //
            // Helper Methods
            //

            internal void Init( CompilationConstraints[] array1 ,
                                CompilationConstraints[] array2 )
            {
                m_array1   = array1;
                m_array2   = array2;
                m_arrayRes = SharedEmptyCompilationConstraintsArray;
            }

            internal void Apply( params CompilationConstraints[] ccFilter )
            {
                foreach(CompilationConstraints cc in ccFilter)
                {
                    if(HasCompilationConstraint( m_array1, cc ) ||
                       HasCompilationConstraint( m_array2, cc )  )
                    {
                        m_arrayRes = AddCompilationConstraint( m_arrayRes, cc );
                        return;
                    }
                }
            }

            //
            // Access Methods
            //

            internal CompilationConstraints[] Result
            {
                get
                {
                    return m_arrayRes;
                }
            }
        }

        struct ConvertHelper
        {
            //
            // State
            //

            private  CompilationConstraints[] m_ccPre;
            private  CompilationConstraints[] m_ccPost;
            internal CompilationConstraints[] m_ccResult;

            //
            // Helper Methods
            //

            internal void Init( CompilationConstraints[] ccPre  ,
                                CompilationConstraints[] ccPost )
            {
                m_ccPre    = ccPre;
                m_ccPost   = ccPost;
                m_ccResult = ControlFlowGraphState.SharedEmptyCompilationConstraintsArray;
            }

            internal void Apply( params CompilationConstraints[] ccFilter )
            {
                foreach(CompilationConstraints cc in ccFilter)
                {
                    if(HasCompilationConstraint( m_ccPost, cc ))
                    {
                        m_ccResult = AddCompilationConstraint( m_ccResult, cc );
                        return;
                    }
                }

                foreach(CompilationConstraints cc in ccFilter)
                {
                    if(HasCompilationConstraint( m_ccPre, cc ))
                    {
                        m_ccResult = AddCompilationConstraint( m_ccResult, cc );
                        return;
                    }
                }
            }
        }

        struct DeltaHelper
        {
            //
            // State
            //

            private  CompilationConstraints[] m_ccPre;
            private  CompilationConstraints[] m_ccPost;
            internal CompilationConstraints[] m_ccSet;
            internal CompilationConstraints[] m_ccReset;
            internal bool                     m_fNotEmpty;

            //
            // Helper Methods
            //

            internal void Init( CompilationConstraints[] ccPre  ,
                                CompilationConstraints[] ccPost )
            {
                m_ccPre     = ccPre;
                m_ccPost    = ccPost;
                m_ccSet     = ControlFlowGraphState.SharedEmptyCompilationConstraintsArray;
                m_ccReset   = ControlFlowGraphState.SharedEmptyCompilationConstraintsArray;
                m_fNotEmpty = false;
            }

            internal void Apply( params CompilationConstraints[] ccFilter )
            {
                CompilationConstraints ccPre;
                CompilationConstraints ccPost;
                bool                   fGotPre  = HasAnyCompilationConstraint( m_ccPre , out ccPre , ccFilter );
                bool                   fGotPost = HasAnyCompilationConstraint( m_ccPost, out ccPost, ccFilter );

                if(fGotPre && fGotPost)
                {
                    if(ccPre == ccPost)
                    {
                        return;
                    }
                }

                if(fGotPre)
                {
                    m_ccReset   = ControlFlowGraphState.AddCompilationConstraint( m_ccReset, ccPre  );
                    m_fNotEmpty = true;
                }

                if(fGotPost)
                {
                    m_ccSet     = ControlFlowGraphState.AddCompilationConstraint( m_ccSet, ccPost );
                    m_fNotEmpty = true;
                }
            }

            static bool Find(     CompilationConstraints[] ccTarget ,
                                  CompilationConstraints[] ccFilter ,
                              out CompilationConstraints   match    )
            {
                foreach(CompilationConstraints cc in ccFilter)
                {
                    if(HasCompilationConstraint( ccTarget, cc ))
                    {
                        match = cc;
                        return true;
                    }
                }

                match = default(CompilationConstraints);

                return false;
            }
        }

        //--//

        //
        // State
        //

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        public bool EnforceCompilationConstraints( List< CallOperator > toBeInlined )
        {
            GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > ht = HashTableFactory.NewWithReferenceEquality< BasicBlock, CompilationConstraints[] >();

            return PropagateCompilationConstraints( ht, this.EntryBasicBlock, this.CompilationConstraintsArray, toBeInlined );
        }

        public GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > PropagateCompilationConstraints( CompilationConstraints[] ccStart )
        {
            GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > ht = HashTableFactory.NewWithReferenceEquality< BasicBlock, CompilationConstraints[] >();

            if(PropagateCompilationConstraints( ht, this.EntryBasicBlock, ccStart, null ))
            {
                return ht;
            }

            return null;
        }

        private static bool PropagateCompilationConstraints( GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > ht          ,
                                                             BasicBlock                                                bb          ,
                                                             CompilationConstraints[]                                  ccArray     ,
                                                             List< CallOperator >                                      toBeInlined )
        {
            CompilationConstraints[] ccArrayOld;

            if(ht.TryGetValue( bb, out ccArrayOld ))
            {
                if(SameCompilationConstraints( ccArrayOld, ccArray ))
                {
                    return true;
                }

                //--//

                MergeHelper helper = new MergeHelper();

                helper.Init( ccArray, ccArrayOld );

                helper.Apply( CompilationConstraints.Allocations_OFF ,
                              CompilationConstraints.Allocations_ON  );

                helper.Apply( CompilationConstraints.StackAccess_OFF ,
                              CompilationConstraints.StackAccess_ON  );

                helper.Apply( CompilationConstraints.BoundsChecks_OFF_DEEP ,
                              CompilationConstraints.BoundsChecks_OFF      ,
                              CompilationConstraints.BoundsChecks_ON       );

                helper.Apply( CompilationConstraints.NullChecks_OFF_DEEP ,
                              CompilationConstraints.NullChecks_OFF      ,
                              CompilationConstraints.NullChecks_ON       );

                ccArray = helper.Result;
            }

            ht[bb] = ccArray;

            ccArray = PropagateCompilationConstraintsThroughOperators( bb, ccArray, null, toBeInlined );
            if(ccArray == null)
            {
                return false;
            }

            foreach(BasicBlockEdge edge in bb.Successors)
            {
                if(!PropagateCompilationConstraints( ht, edge.Successor, ccArray, toBeInlined ))
                {
                    return false;
                }
            }

            return true;
        }

        private static CompilationConstraints[] PropagateCompilationConstraintsThroughOperators( BasicBlock               bb          ,
                                                                                                 CompilationConstraints[] ccArray     ,
                                                                                                 Operator                 opStopAt    ,
                                                                                                 List< CallOperator >     toBeInlined )
        {
            foreach(Operator op in bb.Operators)
            {
                if(op == opStopAt)
                {
                    break;
                }

                if(op is CallOperator)
                {
                    CallOperator call = (CallOperator)op;

                    if(toBeInlined != null)
                    {
                        if(HasCompilationConstraint( ccArray, CompilationConstraints.StackAccess_OFF ))
                        {
                            if(call.CallType == CallOperator.CallKind.Virtual)
                            {
                                throw TypeConsistencyErrorException.Create( "Found a virtual method call '{0}' in a StackNotAvailable context: '{1}'", call.TargetMethod, bb.Owner.Method );
                            }

                            if(call.TargetMethod.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.NoInline ))
                            {
                                //
                                // TODO: Should we ignore the attribute??
                                //
                            }
                            else
                            {
                                toBeInlined.Add( call );
                                return null;
                            }
                        }

                        if(HasCompilationConstraint( ccArray, CompilationConstraints.Allocations_OFF ))
                        {
                            //
                            // TODO: This cannot be properly processed here, we'd need to have fully expanded the callee to analyze it properly...
                            //
                        }
                    }

                    if(call.TargetMethod.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.CanAllocateOnReturn ))
                    {
                        ccArray = RemoveCompilationConstraint( ccArray, CompilationConstraints.Allocations_OFF );
                        ccArray = AddCompilationConstraint   ( ccArray, CompilationConstraints.Allocations_ON  );
                    }
                }
                else
                {
                    if(toBeInlined != null)
                    {
                        if(op.MayAllocateStorage && HasCompilationConstraint( ccArray, CompilationConstraints.Allocations_OFF ))
                        {
                            throw TypeConsistencyErrorException.Create( "Allocating operator '{0}' found in a CannotAllocate context: '{1}'", op, bb.Owner.Method );
                        }
                    }
                }

                if(op is CompilationConstraintsOperator)
                {
                    CompilationConstraintsOperator ccOp = (CompilationConstraintsOperator)op;

                    ccArray = ccOp.TransformCompilationConstraints( ccArray );
                }
            }

            return ccArray;
        }

        //--//

        public static CompilationConstraints[] ComposeCompilationConstraints( CompilationConstraints[] ccPre  ,
                                                                              CompilationConstraints[] ccPost )
        {
            ConvertHelper cnvHelper = new ConvertHelper();

            cnvHelper.Init( ccPre, ccPost );

            cnvHelper.Apply( CompilationConstraints.Allocations_OFF ,
                             CompilationConstraints.Allocations_ON  );

            cnvHelper.Apply( CompilationConstraints.StackAccess_OFF ,
                             CompilationConstraints.StackAccess_ON  );

            cnvHelper.Apply( CompilationConstraints.BoundsChecks_OFF_DEEP ,
                             CompilationConstraints.BoundsChecks_OFF      ,
                             CompilationConstraints.BoundsChecks_ON       );

            cnvHelper.Apply( CompilationConstraints.NullChecks_OFF_DEEP ,
                             CompilationConstraints.NullChecks_OFF      ,
                             CompilationConstraints.NullChecks_ON       );

            return cnvHelper.m_ccResult;
        }

        public static bool ComputeDeltaBetweenCompilationConstraints(     CompilationConstraints[] ccPre   ,
                                                                          CompilationConstraints[] ccPost  ,
                                                                      out CompilationConstraints[] ccSet   ,
                                                                      out CompilationConstraints[] ccReset )
        {
            DeltaHelper dltHelper = new DeltaHelper();

            dltHelper.Init( ccPre, ccPost );

            dltHelper.Apply( CompilationConstraints.Allocations_OFF ,
                             CompilationConstraints.Allocations_ON  );

            dltHelper.Apply( CompilationConstraints.StackAccess_OFF ,
                             CompilationConstraints.StackAccess_ON  );

            dltHelper.Apply( CompilationConstraints.BoundsChecks_OFF_DEEP ,
                             CompilationConstraints.BoundsChecks_OFF      ,
                             CompilationConstraints.BoundsChecks_ON       );

            dltHelper.Apply( CompilationConstraints.NullChecks_OFF_DEEP ,
                             CompilationConstraints.NullChecks_OFF      ,
                             CompilationConstraints.NullChecks_ON       );

            ccSet   = dltHelper.m_ccSet;
            ccReset = dltHelper.m_ccReset;

            return dltHelper.m_fNotEmpty;
        }

        //--//

        public CompilationConstraints[] CompilationConstraintsAtBasicBlockEntry( BasicBlock bb )
        {
            GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > ht = PropagateCompilationConstraints( this.CompilationConstraintsArray );

            if(ht == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot propagate compilation constraints in {0}", this );
            }

            return CompilationConstraintsAtBasicBlockEntry( ht, bb );
        }

        public static CompilationConstraints[] CompilationConstraintsAtBasicBlockEntry( GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > ht ,
                                                                                        BasicBlock                                                bb )
        {
            CompilationConstraints[] res;

            if(ht.TryGetValue( bb, out res ) == false)
            {
                res = ControlFlowGraphState.SharedEmptyCompilationConstraintsArray;
            }

            return res;
        }

        public static CompilationConstraints[] CompilationConstraintsAtBasicBlockExit( GrowOnlyHashTable< BasicBlock, CompilationConstraints[] > ht ,
                                                                                       BasicBlock                                                bb )
        {
            CompilationConstraints[] res = CompilationConstraintsAtBasicBlockEntry( ht, bb );

            res = PropagateCompilationConstraintsThroughOperators( bb, res, null, null );
            if(res == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot propagate compilation constraints in {0} to basic block {1}", bb.Owner, bb );
            }

            return res;
        }

        public CompilationConstraints[] CompilationConstraintsAtOperator( Operator op )
        {
            CompilationConstraints[] res = CompilationConstraintsAtBasicBlockEntry( op.BasicBlock );

            res = PropagateCompilationConstraintsThroughOperators( op.BasicBlock, res, op, null );
            if(res == null)
            {
                throw TypeConsistencyErrorException.Create( "Cannot propagate compilation constraints in {0} to operator {1}", this, op );
            }

            return res;
        }
    }
}

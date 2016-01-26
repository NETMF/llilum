//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#if DEBUG
#define TRACK_BASICBLOCK_IDENTITY
#define ENABLE_INLINE_DEBUG_INFO
#else
//#define TRACK_BASICBLOCK_IDENTITY
#endif


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;
    using System.Diagnostics;

    public abstract class BasicBlock
    {
        public static readonly BasicBlock[] SharedEmptyArray = new BasicBlock[0];

        public enum Qualifier
        {
            Entry              ,
            PrologueStart      ,
            PrologueEnd        ,
            EntryInjectionStart,
            EntryInjectionEnd  ,
            Normal             ,
            Exception          ,
            ExitInjectionStart ,
            ExitInjectionEnd   ,
            EpilogueStart      ,
            EpilogueEnd        ,
            Exit               ,
        }

        //
        // State
        //

#if TRACK_BASICBLOCK_IDENTITY
        protected static int                   s_identity;
#endif
        public           int                   m_identity;

        //--//


        protected ControlFlowGraphState        m_owner;
        protected int                          m_index;
        protected Operator[]                   m_operators;
        protected ExceptionHandlerBasicBlock[] m_protectedBy;
        protected Qualifier                    m_qualifier;

        //
        // Created through ResetFlowInformation/UpdateFlowInformation
        //
        protected BasicBlockEdge[]             m_predecessors;
        protected BasicBlockEdge[]             m_successors;

        //
        // Constructor Methods
        //

        protected BasicBlock() // Default constructor required by TypeSystemSerializer.
        {
#if TRACK_BASICBLOCK_IDENTITY
            m_identity = s_identity++;
#endif

            m_index    = -1;
        }

        protected BasicBlock( ControlFlowGraphState owner ) : this()
        {
            m_owner        = owner;
            m_operators    = Operator                  .SharedEmptyArray;
            m_protectedBy  = ExceptionHandlerBasicBlock.SharedEmptyArray;
            m_qualifier    = Qualifier.Normal;

            m_predecessors = BasicBlockEdge            .SharedEmptyArray;
            m_successors   = BasicBlockEdge            .SharedEmptyArray;

            owner.Register( this );
        }

        //--//

        //
        // Helper Methods
        //

        public abstract BasicBlock Clone( CloningContext context );

        public BasicBlock RegisterAndCloneState( CloningContext context ,
                                                 BasicBlock     clone   )
        {
            context.Register( this, clone );

            CloneState( context, clone );

            return clone;
        }

        public virtual void CloneState( CloningContext context ,
                                        BasicBlock     clone   )
        {
            clone.m_qualifier   =                m_qualifier;
            clone.m_operators   = context.Clone( m_operators   );
            clone.m_protectedBy = context.Clone( m_protectedBy );
        }

        //--//

        internal void BumpVersion()
        {
            m_owner.BumpVersion();
        }

        internal void ResetFlowInformation()
        {
            m_predecessors = BasicBlockEdge.SharedEmptyArray;
            m_successors   = BasicBlockEdge.SharedEmptyArray;
        }

        internal void UpdateFlowInformation()
        {
            //
            // NOTICE!
            //
            // It's important that we link first the normal edges and then the exception ones.
            // This way, when we build the spanning tree, it's guaranteed that the tree edges go
            // through the normal execution path and not through the exceptional one.
            //
            ControlOperator ctrl = this.FlowControl;
            if(ctrl != null)
            {
                ctrl.UpdateSuccessorInformationInner();
            }

            foreach(BasicBlock protectedBy in m_protectedBy)
            {
                this.LinkToExceptionBasicBlock( protectedBy );
            }
        }

        //--//--//

        public void LinkToNormalBasicBlock( BasicBlock successor )
        {
            CHECKS.ASSERT( successor is ExceptionHandlerBasicBlock == false, "Cannot link from flow control to an exception basic block: {0} => {1}", this, successor );

            LinkToInner( successor );
        }

        public void LinkToExceptionBasicBlock( BasicBlock successor )
        {
            CHECKS.ASSERT( successor is ExceptionHandlerBasicBlock == true, "Cannot use a normal basic block for exception handling: {0} => {1}", this, successor );

            LinkToInner( successor );
        }

        private void LinkToInner( BasicBlock successor )
        {
            foreach(BasicBlockEdge edge in this.m_successors)
            {
                CHECKS.ASSERT( edge.Predecessor == this, "Found inconsistency in basic block linkage" );

                if(edge.Successor == successor)
                {
                    CHECKS.ASSERT( ArrayUtility.FindReferenceInNotNullArray( successor.m_predecessors, edge ) >= 0, "Found inconsistency in basic block linkage" );

                    return;
                }
            }

            BasicBlockEdge newEdge = new BasicBlockEdge( this, successor );

            successor.m_predecessors = ArrayUtility.AppendToNotNullArray( successor.m_predecessors, newEdge );
            this     .m_successors   = ArrayUtility.AppendToNotNullArray( this     .m_successors  , newEdge );
        }

        public void Delete()
        {
            m_owner.Deregister( this );

            m_owner = null;
        }

        public BasicBlock InsertNewSuccessor( BasicBlock oldBB )
        {
            return InsertNewSuccessor( oldBB, new NormalBasicBlock( m_owner ) );
        }

        public BasicBlock InsertNewSuccessor( BasicBlock oldBB ,
                                              BasicBlock newBB )
        {
            this.FlowControl.SubstituteTarget( oldBB, newBB );

            newBB.AddOperator( UnconditionalControlOperator.New( null, oldBB ) );

            newBB.NotifyNewPredecessor( this );

            return newBB;
        }

        public BasicBlock InsertNewPredecessor()
        {
            return InsertNewPredecessor( new NormalBasicBlock( m_owner ) );
        }

        public BasicBlock InsertNewPredecessor( BasicBlock newBB )
        {
            BasicBlockEdge[] edges = this.Predecessors;

            newBB.AddOperator( UnconditionalControlOperator.New( null, this ) );

            foreach(BasicBlockEdge edge in edges)
            {
                edge.Predecessor.FlowControl.SubstituteTarget( this, newBB );
            }

            newBB.NotifyNewPredecessor( this );

            return newBB;
        }

        //--//

        private void NotifyMerge( BasicBlock entryBB )
        {
            foreach(BasicBlockEdge edge in this.Successors)
            {
                BasicBlock succ = edge.Successor;
                
                foreach(Operator op in succ.Operators)
                {
                    op.NotifyMerge( entryBB, this );
                }
            }
        }

        private void NotifyNewPredecessor( BasicBlock oldBB )
        {
            foreach(BasicBlockEdge edge in this.Successors)
            {
                BasicBlock succ = edge.Successor;
                
                foreach(Operator op in succ.Operators)
                {
                    op.NotifyNewPredecessor( oldBB, this );
                }
            }
        }

        //--//

        public BasicBlockEdge FindForwardEdge( BasicBlock successor )
        {
            foreach(BasicBlockEdge edge in m_successors)
            {
                if(edge.Successor == successor)
                {
                    return edge;
                }
            }

            return null;
        }

        public BasicBlockEdge FindBackwardEdge( BasicBlock predecessor )
        {
            foreach(BasicBlockEdge edge in m_predecessors)
            {
                if(edge.Predecessor == predecessor)
                {
                    return edge;
                }
            }

            return null;
        }

        public bool IsProtectedBy( BasicBlock handler )
        {
            foreach(BasicBlock bb in m_protectedBy)
            {
                if(bb == handler)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldIncludeInScheduling( BasicBlock bbNext )
        {
            var ctrl = this.FlowControl;

            return ctrl != null ? ctrl.ShouldIncludeInScheduling( bbNext ) : false;
        }

        //--//

        public void SetProtectedBy( ExceptionHandlerBasicBlock bb )
        {
            BumpVersion();

            m_protectedBy = ArrayUtility.AddUniqueToNotNullArray( m_protectedBy, bb );
        }

        public bool SubstituteProtectedBy( ExceptionHandlerBasicBlock oldBB ,
                                           ExceptionHandlerBasicBlock newBB )
        {
            int pos = ArrayUtility.FindInNotNullArray( m_protectedBy, oldBB );

            if(pos >= 0)
            {
                m_protectedBy = ArrayUtility.ReplaceAtPositionOfNotNullArray( m_protectedBy, pos, newBB );

                BumpVersion();

                return true;
            }

            return false;
        }

        //--//

        public bool IsDominatedBy( BasicBlock  bbTarget  ,
                                   BitVector[] dominance )
        {
            return dominance[this.SpanningTreeIndex][bbTarget.SpanningTreeIndex];
        }

        public Operator GetOperator( Type target )
        {
            foreach(Operator op in m_operators)
            {
                if(target.IsInstanceOfType( op ))
                {
                    return op;
                }
            }

            return null;
        }

        public Operator GetFirstDifferentOperator( Type target )
        {
            foreach(Operator op in m_operators)
            {
                if(target.IsInstanceOfType( op ) == false)
                {
                    return op;
                }
            }

            return null;
        }

        public void AddOperator( Operator oper )
        {
            CheckInliningAssert(oper);
            CHECKS.ASSERT(!(oper is ControlOperator) || this.FlowControl == null, "Cannot add two control operators to a basic block");
            CHECKS.ASSERT(oper.BasicBlock == null, "Adding operator already part of another code sequence");
            CHECKS.ASSERT(oper.GetBasicBlockIndex() < 0, "Adding operator already present in code sequence");

            BumpVersion();

            oper.BasicBlock = this;

            int pos = m_operators.Length;

            if (oper is ControlOperator)
            {
                //
                // Make sure the ControlOperator is always the last one in the basic block.
                //
            }
            else if (pos > 0)
            {
                Operator ctrl = m_operators[pos - 1];

                if (ctrl is ControlOperator)
                {
                    //
                    // Insert before the control operator.
                    //
                    pos -= 1;
                }
            }

            m_operators = ArrayUtility.InsertAtPositionOfNotNullArray(m_operators, pos, oper);
        }

        [Conditional("ENABLE_INLINE_DEBUG_INFO")]
        private void CheckInliningAssert(Operator oper)
        {
            // The reason for this set of checks is to catch cases where the scope, stored in
            // the DebugInfo for an operator doesn't match the method that owns the block 
            // when an operator is added to the block. This is a case of effective inlining
            // but the InliningPathAnnotation isn't updated to reflect that occured. Once
            // all the cases where that can occur is known, a proper plan for dealing with
            // them can be defined and implemented.
            //
            // This rather contrived set of reflection magic is due to the fact that the
            // annotation system is based on concrete types rather than interfaces. This
            // assembly cannot reference the InliningPathAnnotation without creating a
            // circular dependency. Operator.GetAnnotation<T>() has the constraint:
            //     where T: Annotation
            // so IInliningPathAnnotation can't be used as T...
            //
            // REVIEW: consider adding IAnnotation interface and using that in the constraint
            // for the GetAnnotation method while adding the interface to Annotation class.
            // The IAnnotation would simple mirror the existing public API surface of the
            // existing Annotation class. Doing so involves minor updates to all of the 
            // classes derived from Annotation along with the TranformationContextForIR
            // abstract class (and thus all derived types) so they handle transforming
            // IAnnotation and IAnnotation[] instead of the abstract type Annotation.
            var annotationsField = oper.GetType().GetField("m_annotations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var annotations = (Annotation[])annotationsField.GetValue(oper);
            var inliningPath = System.Linq.Enumerable.SingleOrDefault(System.Linq.Enumerable.OfType<IInliningPathAnnotation>(annotations));

            // test conditions on the annotation
            // check operator state first as inliningpath is irrelevant if this doesn't match
            if (oper.DebugInfo != null && oper.DebugInfo.Scope != null)
                Debug.Assert(Owner.Method.Equals(oper.DebugInfo.Scope));

            if (inliningPath != null)
                Debug.Assert(inliningPath.DebugInfoPath[inliningPath.DebugInfoPath.Length - 1].Scope.Equals(Owner.Method));
        }

        internal void AddAsFirstOperator( Operator oper )
        {
            AddOperatorBefore( oper, m_operators[0] );
        }

        internal void AddOperatorBefore( Operator oper       ,
                                         Operator operTarget )
        {
            CheckInliningAssert(oper);
            CHECKS.ASSERT( oper       != null                  , "Missing source operator"                                        );
            CHECKS.ASSERT( operTarget != null                  , "Missing target operator"                                        );
            CHECKS.ASSERT( oper is ControlOperator == false    , "Control operator can only be added at the end of a basic block" );
            CHECKS.ASSERT( oper      .BasicBlock == null       , "Adding operator already part of another code sequence"          );
            CHECKS.ASSERT( operTarget.BasicBlock == this       , "Target operator does not belong to this basic block"            );
            CHECKS.ASSERT( operTarget.GetBasicBlockIndex() >= 0, "Target operator does not belong to this basic block"            );
            CHECKS.ASSERT( oper      .GetBasicBlockIndex() <  0, "Adding operator already present in code sequence"               );

            BumpVersion();

            oper.BasicBlock = this;

            int pos = operTarget.GetBasicBlockIndex();

            m_operators = ArrayUtility.InsertAtPositionOfNotNullArray( m_operators, pos, oper );
        }

        internal void AddOperatorAfter( Operator operTarget ,
                                        Operator oper       )
        {
            CHECKS.ASSERT( operTarget != null                    , "Missing target operator"                      );
            CHECKS.ASSERT( operTarget is ControlOperator == false, "Target operator cannot be a control operator" );

            AddOperatorBefore( oper, operTarget.GetNextOperator() );
        }

        internal void RemoveOperator( Operator oper )
        {
            CHECKS.ASSERT( oper.BasicBlock == this       , "Removing operator part of another code sequence"   );
            CHECKS.ASSERT( oper.GetBasicBlockIndex() >= 0, "Removing operator not part of current basic block" );

            BumpVersion();

            int pos = oper.GetBasicBlockIndex();

            m_operators = ArrayUtility.RemoveAtPositionFromNotNullArray( m_operators, pos );

            oper.BasicBlock = null;
        }

        internal void SubstituteOperator( Operator                   opOld ,
                                          Operator                   opNew ,
                                          Operator.SubstitutionFlags flags )
        {
            CHECKS.ASSERT( opOld.BasicBlock == this                                , "Target operator does not belong to this basic block"                          );
            CHECKS.ASSERT( opNew.BasicBlock == null                                , "Substituting operator already part of another code sequence"                  );
            CHECKS.ASSERT( (opOld is ControlOperator) == (opNew is ControlOperator), "Change substitute between incompatible operators ({0} <=> {1})", opOld, opNew );

            BumpVersion();

            if((flags & Operator.SubstitutionFlags.CopyAnnotations) != 0)
            {
                opNew.CopyAnnotations( opOld );
            }

            int pos = opOld.GetBasicBlockIndex();

            m_operators = ArrayUtility.ReplaceAtPositionOfNotNullArray( m_operators, pos, opNew );

            opOld.BasicBlock = null;
            opNew.BasicBlock = this;
        }

        public void Merge( BasicBlock bbNext )
        {
            CHECKS.ASSERT( this.Owner == bbNext.Owner, "Cannot merge basic blocks from different flow graphs!" );

            ControlOperator ctrl = this.FlowControl;

            CHECKS.ASSERT(                                ctrl is UnconditionalControlOperator &&
                           ((UnconditionalControlOperator)ctrl).TargetBranch == bbNext         &&
                                                          bbNext.Predecessors.Length == 1       , "Cannot merge two non-consecutive basic blocks" );

            bbNext.NotifyMerge( this );

            ctrl.Delete();

            m_operators = ArrayUtility.AppendNotNullArrayToNotNullArray( m_operators, bbNext.Operators );

            foreach(Operator op in bbNext.Operators)
            {
                op.BasicBlock = this;
            }

            this.NotifyNewPredecessor( bbNext );
        }

        public NormalBasicBlock SplitAtOperator( Operator oper            ,
                                                 bool     fRemoveOperator ,
                                                 bool     fAddFlowControl )
        {
            CHECKS.ASSERT( this is NormalBasicBlock  , "Cannot split non-normal basic block"                 );
            CHECKS.ASSERT( !(oper is ControlOperator), "Cannot split on control operator"                    );
            CHECKS.ASSERT( oper.BasicBlock == this   , "Target operator does not belong to this basic block" );

            BumpVersion();

            //
            // Create new basic block, copying the exception handler sets.
            //
            NormalBasicBlock bb = NormalBasicBlock.CreateWithSameProtection( this );

            switch(this.Annotation)
            {
                case Qualifier.EntryInjectionEnd:
                    this.Annotation = BasicBlock.Qualifier.Normal;
                    bb  .Annotation = BasicBlock.Qualifier.EntryInjectionEnd;
                    break;

                case Qualifier.ExitInjectionEnd:
                    this.Annotation = BasicBlock.Qualifier.Normal;
                    bb  .Annotation = BasicBlock.Qualifier.ExitInjectionEnd;
                    break;
            }

            //
            // Redistribute operators, redirect post ones to the new basic block, add branch from pre ones.
            //
            int pos = oper.GetBasicBlockIndex();
            int pos2;

            if(fRemoveOperator)
            {
                Operator next = oper.GetNextOperator();

                if(next != null)
                {
                    pos2 = next.GetBasicBlockIndex();
                }
                else
                {
                    pos2 = m_operators.Length;
                }
            }
            else
            {
                pos2 = pos;
            }

            Operator[] preOps  = ArrayUtility.ExtractSliceFromNotNullArray( m_operators, 0   ,                      pos  );
            Operator[] postOps = ArrayUtility.ExtractSliceFromNotNullArray( m_operators, pos2, m_operators.Length - pos2 );

            this.m_operators = preOps;
            bb  .m_operators = postOps;

            foreach(Operator op in postOps)
            {
                op.Retarget( bb );
            }

            if(fAddFlowControl)
            {
                AddOperator( UnconditionalControlOperator.New( null, bb ) );
            }

            if(fRemoveOperator)
            {
                oper.ClearState();
            }

            bb.NotifyNewPredecessor( this );

            return bb;
        }

        //--//

        //
        // Example:
        //
        //        bb.PerformActionOnValues( delegate( Expression ex )
        //        {
        //            TemporaryVariableExpression var = ex as TemporaryVariableExpression;
        //
        //            if(var != null)
        //            {
        //                SetTemporary( var );
        //            }
        //        } );
        //

        public void PerformActionOnValues( Action< Expression > action )
        {
            foreach(var op in m_operators)
            {
                foreach(var ex in op.Results)
                {
                    if(ex != null)
                    {
                        action( ex );
                    }
                }

                foreach(var ex in op.Arguments)
                {
                    if(ex != null)
                    {
                        action( ex );
                    }
                }
            }
        }

        //--//

        public virtual void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            context.Transform( ref m_owner       );
            context.Transform( ref m_index       );
            context.Transform( ref m_operators   );
            context.Transform( ref m_protectedBy );
            context.Transform( ref m_qualifier   );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public ControlFlowGraphState Owner
        {
            get
            {
                return m_owner;
            }
        }

        public int SpanningTreeIndex
        {
            get
            {
                return m_index;
            }

            set
            {
                m_index = value;
            }
        }

        public Operator[] Operators
        {
            get
            {
                return m_operators;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlockEdge[] Predecessors
        {
            get
            {
                m_owner.UpdateFlowInformation();

                return m_predecessors;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlockEdge[] Successors
        {
            get
            {
                m_owner.UpdateFlowInformation();

                return m_successors;
            }
        }

        public ExceptionHandlerBasicBlock[] ProtectedBy
        {
            get
            {
                return m_protectedBy;
            }
        }

        public Qualifier Annotation
        {
            get
            {
                return m_qualifier;
            }

            set
            {
                m_qualifier = value;
            }
        }


        public ControlOperator FlowControl
        {
            get
            {
                int len = m_operators.Length;
                if(len > 0)
                {
                    Operator op = m_operators[len-1];
                    if(op is ControlOperator)
                    {
                        return (ControlOperator)op;
                    }
                }

                return null;
            }

            set
            {
                BumpVersion();

                value.BasicBlock = this;

                int len = m_operators.Length;
                if(len > 0)
                {
                    Operator op = m_operators[len-1];
                    if(op is ControlOperator)
                    {
                        //
                        // Update existing control operator.
                        //
                        m_operators[len-1] = value;
                        return;
                    }
                }

                m_operators = ArrayUtility.AppendToNotNullArray( m_operators, (Operator)value );
            }
        }

        public BasicBlock FirstPredecessor
        {
            get
            {
                return this.Predecessors[0].Predecessor;
            }
        }

        public BasicBlock FirstSuccessor
        {
            get
            {
                return this.Successors[0].Successor;
            }
        }

        public Operator FirstOperator
        {
            get
            {
                return m_operators[0];
            }
        }

        public Operator LastOperator
        {
            get
            {
                return m_operators[m_operators.Length - 1];
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToString()
        {
            string fmt = (m_owner == null) ? "{0}" : "{0} of {1}";

            return string.Format( fmt, this.ToShortString(), m_owner );
        }

        public abstract string ToShortString();

        //--//

        protected string ToShortStringInner( string prefix )
        {
            return string.Format( "{0}({1})", prefix, m_index );
        }
    }
}
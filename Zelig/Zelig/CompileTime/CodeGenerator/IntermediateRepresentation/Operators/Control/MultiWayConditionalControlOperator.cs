//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class MultiWayConditionalControlOperator : ConditionalControlOperator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // State
        //

        private BasicBlock[] m_targets;

        //
        // Constructor Methods
        //

        private MultiWayConditionalControlOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static MultiWayConditionalControlOperator New( Debugging.DebugInfo debugInfo      ,
                                                              Expression          rhs            ,
                                                              BasicBlock          targetNotTaken ,
                                                              BasicBlock[]        targets        )
        {
            MultiWayConditionalControlOperator res = new MultiWayConditionalControlOperator( debugInfo );

            res.SetRhs( rhs );

            res.m_targetBranchNotTaken = targetNotTaken;
            res.m_targets              = targets;

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new MultiWayConditionalControlOperator( m_debugInfo ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            MultiWayConditionalControlOperator clone2 = (MultiWayConditionalControlOperator)clone;

            clone2.m_targets = context.Clone( m_targets );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_targets );

            context.Pop();
        }

        //--//

        protected override void UpdateSuccessorInformation()
        {
            base.UpdateSuccessorInformation();

            foreach(BasicBlock target in m_targets)
            {
                m_basicBlock.LinkToNormalBasicBlock( target );
            }
        }

        public override bool ShouldIncludeInScheduling( BasicBlock bbNext )
        {
            if(base.ShouldIncludeInScheduling( bbNext ) == false)
            {
                return false;
            }

            return m_targetBranchNotTaken == bbNext;
        }

        //--//

        public override bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB )
        {
            bool fChanged = base.SubstituteTarget( oldBB, newBB );

            BasicBlock[] targets = m_targets;

            for(int pos = targets.Length; --pos >=0; )
            {
                if(targets[pos] == oldBB)
                {
                    if(m_targets == targets)
                    {
                        targets = ArrayUtility.CopyNotNullArray( m_targets );
                    }

                    targets[pos] = newBB;
                }
            }

            if(m_targets != targets)
            {
                BumpVersion();

                m_targets = targets;

                fChanged = true;
            }

            return fChanged;
        }

        //--//

        public override bool Simplify( Operator[][]                  defChains  ,
                                       Operator[][]                  useChains  ,
                                       VariableExpression.Property[] properties )
        {
            if(base.Simplify( defChains, useChains, properties ))
            {
                return true;
            }

            var exConst = FindConstantOrigin( this.FirstArgument, defChains, useChains, properties );

            if(exConst != null && exConst.IsValueInteger)
            {
                ulong val;

                if(exConst.GetAsRawUlong( out val ))
                {
                    BasicBlock branch;

                    if(val >= (ulong)m_targets.Length)
                    {
                        branch = m_targetBranchNotTaken;
                    }
                    else
                    {
                        branch = m_targets[(int)val];
                    }

                    UnconditionalControlOperator opNew = UnconditionalControlOperator.New( this.DebugInfo, branch );

                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                    return true;
                }
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock[] Targets
        {
            get
            {
                return m_targets;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "MultiWayConditionalControlOperator(" );

            base.InnerToString( sb );

            sb.Append( " Targets: ( " );

            for(int i = 0; i < m_targets.Length; i++)
            {
                if(i != 0) sb.Append( ", " );

                sb.Append( m_targets[i].SpanningTreeIndex );
            }

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for(int i = 0; i < m_targets.Length; i++)
            {
                BasicBlock bb = m_targets[i];

                if(i != 0) sb.Append( ", " );

                sb.AppendFormat( "{0}", dumper.CreateLabel( bb ) );
            }

            return dumper.FormatOutput( "switch {0} to {1}\n" + "goto {2}", this.FirstArgument, sb.ToString(), m_targetBranchNotTaken );
        }
    }
}
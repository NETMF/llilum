//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PhiOperator : Operator
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

        private BasicBlock[] m_origins;

        //
        // Constructor Methods
        //

        private PhiOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_origins = BasicBlock.SharedEmptyArray;
        }

        //--//

        public static PhiOperator New( Debugging.DebugInfo debugInfo ,
                                       VariableExpression  ex        )
        {
            PhiOperator res = new PhiOperator( debugInfo );

            res.SetLhs( ex );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new PhiOperator( m_debugInfo ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            PhiOperator clone2 = (PhiOperator)clone;

            clone2.m_origins = context.Clone( m_origins );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_origins );

            context.Pop();
        }

        //--//

        public override void NotifyMerge( BasicBlock entryBB ,
                                          BasicBlock exitBB  )
        {
            CHECKS.ASSERT( entryBB != exitBB, "Cannot merge basic block with itself" );

            //
            // If we have a contribution from 'exitBB', it will dominate any contribution from 'entryBB'.
            // Remove those contributions.
            //
            if(ArrayUtility.FindReferenceInNotNullArray( m_origins, exitBB ) >= 0)
            {
                int pos;

                while((pos = ArrayUtility.FindReferenceInNotNullArray( m_origins, entryBB )) >= 0)
                {
                    this.Arguments = ArrayUtility.RemoveAtPositionFromNotNullArray( this.Arguments, pos );
                    this.Origins   = ArrayUtility.RemoveAtPositionFromNotNullArray( this.Origins  , pos );
                }
            }
        }

        public override void NotifyNewPredecessor( BasicBlock oldBB ,
                                                   BasicBlock newBB )
        {
            CHECKS.ASSERT( oldBB != newBB, "Cannot change a predecessor with itself" );

            int pos;

            while((pos = ArrayUtility.FindReferenceInNotNullArray( m_origins, oldBB )) >= 0)
            {
                m_origins[pos] = newBB;
            }
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            //
            // The Rvalues are used to keep a reference to the previous definitions of a variable,
            // so we cannot change them!!
            //
            return false;
        }

        //--//

        internal void AddEffect( VariableExpression input  ,
                                 BasicBlock         source )
        {
            CHECKS.ASSERT( ArrayUtility.FindReferenceInNotNullArray( m_origins, source ) < 0, "SSA effect added more than once from the same BasicBlock: {0} from {1}", input, source );

            this.Arguments = ArrayUtility.AppendToNotNullArray( this.Arguments, (Expression)input  );
            this.Origins   = ArrayUtility.AppendToNotNullArray( this.Origins  ,             source );
        }

        internal void RemoveEffect( BasicBlock source )
        {
            int pos = ArrayUtility.FindReferenceInNotNullArray( m_origins, source );

            if(pos >= 0)
            {
                this.Arguments = ArrayUtility.RemoveAtPositionFromNotNullArray( this.Arguments, pos );
                this.Origins   = ArrayUtility.RemoveAtPositionFromNotNullArray( this.Origins  , pos );
            }
        }

        internal bool AdjustLinkage()
        {
            bool fModified = false;

            foreach(BasicBlock origin in m_origins)
            {
                if(origin.SpanningTreeIndex < 0)
                {
                    RemoveEffect( origin );
                    fModified = true;
                }
            }

            return fModified;
        }

        internal void SubstituteOrigin( BasicBlock oldSource ,
                                        BasicBlock newSource )
        {
            BasicBlock[] origins = m_origins;

            for(int pos = origins.Length; --pos >=0; )
            {
                if(origins[pos] == oldSource)
                {
                    if(m_origins == origins)
                    {
                        origins = ArrayUtility.CopyNotNullArray( m_origins );
                    }

                    origins[pos] = newSource;
                }
            }

            if(m_origins != origins)
            {
                BumpVersion();

                m_origins = origins;
            }
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock[] Origins
        {
            get
            {
                return m_origins;
            }

            private set
            {
                m_origins = value;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "PhiOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            bool fFirst = true;

            for(int pos = 0; pos < this.Arguments.Length; pos++)
            {
                Expression ex  = this.Arguments[pos];
                BasicBlock src = this.Origins  [pos];

                string fmt;

                if(fFirst)
                {
                    fFirst = false;

                    fmt = "{0} from {1}";
                }
                else
                {
                    fmt = ", {0} from {1}";
                }

                sb.Append( dumper.FormatOutput( fmt, ex, src ) );
            }

            return dumper.FormatOutput( "{0} = phi( {1} )", this.FirstResult, sb.ToString() );
        }
    }
}
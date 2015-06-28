//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ConditionCodeConditionalControlOperator : ConditionalControlOperator
    {
        //
        // State
        //

        private BasicBlock                         m_targetBranchTaken;
        private ConditionCodeExpression.Comparison m_condition;

        //
        // Constructor Methods
        //

        private ConditionCodeConditionalControlOperator( Debugging.DebugInfo                debugInfo    ,
                                                         OperatorCapabilities               capabilities ,
                                                         ConditionCodeExpression.Comparison condition    ) : base( debugInfo, capabilities, OperatorLevel.Lowest )
        {
            m_condition = condition;
        }

        //--//

        public static ConditionCodeConditionalControlOperator New( Debugging.DebugInfo                debugInfo   ,
                                                                   ConditionCodeExpression.Comparison condition   ,
                                                                   VariableExpression                 rhs         ,
                                                                   BasicBlock                         targetFalse ,
                                                                   BasicBlock                         targetTrue  )
        {
            CHECKS.ASSERT( rhs.AliasedVariable is ConditionCodeExpression, "We need a condition code as input: {0}", rhs );

            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotThrow                       |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            ConditionCodeConditionalControlOperator res = new ConditionCodeConditionalControlOperator( debugInfo, capabilities, condition );

            res.SetRhs( rhs );

            res.m_targetBranchNotTaken = targetFalse;
            res.m_targetBranchTaken    = targetTrue;

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new ConditionCodeConditionalControlOperator( m_debugInfo, m_capabilities, m_condition ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            ConditionCodeConditionalControlOperator clone2 = (ConditionCodeConditionalControlOperator)clone;

            clone2.m_targetBranchTaken = context.Clone( m_targetBranchTaken );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_targetBranchTaken );
            context2.Transform( ref m_condition         );

            context2.Pop();
        }

        //--//

        protected override void UpdateSuccessorInformation()
        {
            base.UpdateSuccessorInformation();

            m_basicBlock.LinkToNormalBasicBlock( m_targetBranchTaken );
        }

        //--//

        public override bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB )
        {
            bool fChanged = base.SubstituteTarget( oldBB, newBB );

            if(m_targetBranchTaken == oldBB)
            {
                m_targetBranchTaken = newBB;

                BumpVersion();

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
            if(exConst != null)
            {
                ConditionCodeExpression.ConstantResult cr = exConst.Value as ConditionCodeExpression.ConstantResult;
                if(cr != null)
                {
                    object left  = cr.LeftValue;
                    object right = cr.RightValue;
                    bool   fTaken;

                    if(left is ulong && right is ulong)
                    {
                        ulong valL = (ulong)left;
                        ulong valR = (ulong)right;

                        switch(m_condition)
                        {
                            case ConditionCodeExpression.Comparison.Equal                   : fTaken =       valL ==       valR; break;
                            case ConditionCodeExpression.Comparison.NotEqual                : fTaken =       valL !=       valR; break;
                                                                                                                     
                            case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame: fTaken =       valL >=       valR; break;
                            case ConditionCodeExpression.Comparison.UnsignedLowerThan       : fTaken =       valL <        valR; break;
                            case ConditionCodeExpression.Comparison.UnsignedHigherThan      : fTaken =       valL >        valR; break;
                            case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame : fTaken =       valL <=       valR; break; 
                                                                                        
                            case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual: fTaken = (long)valL >= (long)valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThan          : fTaken = (long)valL <  (long)valR; break;
                            case ConditionCodeExpression.Comparison.SignedGreaterThan       : fTaken = (long)valL >  (long)valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThanOrEqual   : fTaken = (long)valL <= (long)valR; break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Unexpected input to {0}", this );
                        }
                    }
                    else if(left is double && right is double)
                    {
                        double valL = (double)left;
                        double valR = (double)right;

                        switch(m_condition)
                        {
                            case ConditionCodeExpression.Comparison.Equal                   : fTaken = valL == valR; break;
                            case ConditionCodeExpression.Comparison.NotEqual                : fTaken = valL != valR; break;
                                                                                                               
////                        case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame: fTaken = valL >= valR; break;
////                        case ConditionCodeExpression.Comparison.UnsignedLowerThan       : fTaken = valL <  valR; break;
////                        case ConditionCodeExpression.Comparison.UnsignedHigherThan      : fTaken = valL >  valR; break;
////                        case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame : fTaken = valL <= valR; break; 
                                                                                        
                            case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual: fTaken = valL >= valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThan          : fTaken = valL <  valR; break;
                            case ConditionCodeExpression.Comparison.SignedGreaterThan       : fTaken = valL >  valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThanOrEqual   : fTaken = valL <= valR; break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Unexpected input to {0}", this );
                        }
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unexpected input to {0}", this );
                    }

                    UnconditionalControlOperator opNew = UnconditionalControlOperator.New( this.DebugInfo, fTaken ? m_targetBranchTaken : m_targetBranchNotTaken );

                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                    return true;
                }

                throw TypeConsistencyErrorException.Create( "Unexpected input to {0}", this );
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock TargetBranchTaken
        {
            get
            {
                return m_targetBranchTaken;
            }
        }

        public ConditionCodeExpression.Comparison Condition
        {
            get
            {
                return m_condition;
            }

            set
            {
                m_condition = value;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "ConditionCodeConditionalControlOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Taken: {0}", m_targetBranchTaken.SpanningTreeIndex );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "if {0} is {1} then goto {2} else goto {3}", this.FirstArgument, m_condition, m_targetBranchTaken, m_targetBranchNotTaken );
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CompareConditionalControlOperator : ConditionalControlOperator
    {
        //
        // State
        //

        private CompareAndSetOperator.ActionCondition m_condition;
        private bool                                  m_fSigned;
        private BasicBlock                            m_targetBranchTaken;

        //
        // Constructor Methods
        //

        private CompareConditionalControlOperator( Debugging.DebugInfo                   debugInfo    ,
                                                   OperatorCapabilities                  capabilities ,
                                                   CompareAndSetOperator.ActionCondition condition    ,
                                                   bool                                  fSigned      ) : base( debugInfo, capabilities, OperatorLevel.ConcreteTypes_NoExceptions )
        {
            m_condition = condition;
            m_fSigned   = fSigned;
        }

        //--//

        public static CompareConditionalControlOperator New( Debugging.DebugInfo                   debugInfo   ,
                                                             CompareAndSetOperator.ActionCondition condition   ,
                                                             bool                                  fSigned     ,
                                                             Expression                            rhsLeft     ,
                                                             Expression                            rhsRight    ,
                                                             BasicBlock                            targetFalse ,
                                                             BasicBlock                            targetTrue  )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotThrow                       |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            switch(condition)
            {
                case CompareAndSetOperator.ActionCondition.EQ:
                case CompareAndSetOperator.ActionCondition.NE:
                    capabilities |= OperatorCapabilities.IsCommutative;
                    break;

                default:
                    capabilities |= OperatorCapabilities.IsNonCommutative;
                    break;
            }

            CompareConditionalControlOperator res = new CompareConditionalControlOperator( debugInfo, capabilities, condition, fSigned );

            res.SetRhs( rhsLeft, rhsRight );

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
            return RegisterAndCloneState( context, new CompareConditionalControlOperator( m_debugInfo, m_capabilities, m_condition, m_fSigned ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            CompareConditionalControlOperator clone2 = (CompareConditionalControlOperator)clone;

            clone2.m_targetBranchTaken = context.Clone( m_targetBranchTaken );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_condition         );
            context.Transform( ref m_fSigned           );
            context.Transform( ref m_targetBranchTaken );

            context.Pop();
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
            var exL = FindConstantOrigin( this.FirstArgument , defChains, useChains, properties );
            var exR = FindConstantOrigin( this.SecondArgument, defChains, useChains, properties );
            
            if(exL != null && exR != null)
            {
                bool res;

                if(exL.IsValueInteger)
                {
                    if(!exR.IsValueInteger)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot mix integer and floating-point values in the same operation: {0}", this );
                    }

                    if(this.Signed)
                    {
                        long valL;
                        long valR;

                        if(exL.GetAsSignedInteger( out valL ) == false ||
                           exR.GetAsSignedInteger( out valR ) == false  )
                        {
                            return false;
                        }

                        switch(m_condition)
                        {
                            case CompareAndSetOperator.ActionCondition.EQ: res = (valL == valR); break;
                            case CompareAndSetOperator.ActionCondition.GE: res = (valL >= valR); break; 
                            case CompareAndSetOperator.ActionCondition.GT: res = (valL >  valR); break;
                            case CompareAndSetOperator.ActionCondition.LE: res = (valL <= valR); break;
                            case CompareAndSetOperator.ActionCondition.LT: res = (valL <  valR); break;
                            case CompareAndSetOperator.ActionCondition.NE: res = (valL != valR); break;

                            default:
                                return false;
                        }
                    }
                    else
                    {
                        ulong valL;
                        ulong valR;

                        if(exL.GetAsUnsignedInteger( out valL ) == false ||
                           exR.GetAsUnsignedInteger( out valR ) == false  )
                        {
                            return false;
                        }

                        switch(m_condition)
                        {
                            case CompareAndSetOperator.ActionCondition.EQ: res = (valL == valR); break;
                            case CompareAndSetOperator.ActionCondition.GE: res = (valL >= valR); break; 
                            case CompareAndSetOperator.ActionCondition.GT: res = (valL >  valR); break;
                            case CompareAndSetOperator.ActionCondition.LE: res = (valL <= valR); break;
                            case CompareAndSetOperator.ActionCondition.LT: res = (valL <  valR); break;
                            case CompareAndSetOperator.ActionCondition.NE: res = (valL != valR); break;

                            default:
                                return false;
                        }
                    }
                }
                else if(exL.IsValueFloatingPoint)
                {
                    if(!exR.IsValueFloatingPoint)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot mix integer and floating-point values in the same operation: {0}", this );
                    }

                    object valL = exL.Value;
                    object valR = exR.Value;

                    if(valL is float)
                    {
                        if(!(valR is float))
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot mix single and double values in the same operation: {0}", this );
                        }

                        float valL2 = (float)valL;
                        float valR2 = (float)valR;

                        switch(m_condition)
                        {
                            case CompareAndSetOperator.ActionCondition.EQ: res = (valL2 == valR2); break;
                            case CompareAndSetOperator.ActionCondition.GE: res = (valL2 >= valR2); break; 
                            case CompareAndSetOperator.ActionCondition.GT: res = (valL2 >  valR2); break;
                            case CompareAndSetOperator.ActionCondition.LE: res = (valL2 <= valR2); break;
                            case CompareAndSetOperator.ActionCondition.LT: res = (valL2 <  valR2); break;
                            case CompareAndSetOperator.ActionCondition.NE: res = (valL2 != valR2); break;

                            default:
                                return false;
                        }
                    }
                    else if(valL is double)
                    {
                        if(!(valR is double))
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot mix single and double values in the same operation: {0}", this );
                        }

                        double valL2 = (double)valL;
                        double valR2 = (double)valR;

                        switch(m_condition)
                        {
                            case CompareAndSetOperator.ActionCondition.EQ: res = (valL2 == valR2); break;
                            case CompareAndSetOperator.ActionCondition.GE: res = (valL2 >= valR2); break; 
                            case CompareAndSetOperator.ActionCondition.GT: res = (valL2 >  valR2); break;
                            case CompareAndSetOperator.ActionCondition.LE: res = (valL2 <= valR2); break;
                            case CompareAndSetOperator.ActionCondition.LT: res = (valL2 <  valR2); break;
                            case CompareAndSetOperator.ActionCondition.NE: res = (valL2 != valR2); break;

                            default:
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                this.SubstituteWithOperator( UnconditionalControlOperator.New( this.DebugInfo, res ? this.TargetBranchTaken : this.TargetBranchNotTaken ), SubstitutionFlags.Default );
                return true;
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public CompareAndSetOperator.ActionCondition Condition
        {
            get
            {
                return m_condition;
            }
        }

        public CompareAndSetOperator.ActionCondition InvertedCondition
        {
            get
            {
                switch(m_condition)
                {
                    case CompareAndSetOperator.ActionCondition.EQ: return CompareAndSetOperator.ActionCondition.NE;
                    case CompareAndSetOperator.ActionCondition.GE: return CompareAndSetOperator.ActionCondition.LT;
                    case CompareAndSetOperator.ActionCondition.GT: return CompareAndSetOperator.ActionCondition.LE;
                    case CompareAndSetOperator.ActionCondition.LE: return CompareAndSetOperator.ActionCondition.GT;
                    case CompareAndSetOperator.ActionCondition.LT: return CompareAndSetOperator.ActionCondition.GE;
                    case CompareAndSetOperator.ActionCondition.NE: return CompareAndSetOperator.ActionCondition.EQ;
                }

                throw TypeConsistencyErrorException.Create( "Unexpected condition value: {0}", m_condition );
            }
        }

        public bool Signed
        {
            get
            {
                return m_fSigned;
            }
        }

        public BasicBlock TargetBranchTaken
        {
            get
            {
                return m_targetBranchTaken;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "CompareConditionalControlOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Cond: {0}", m_condition );

            sb.AppendFormat( " Signed: {0}", m_fSigned );

            sb.AppendFormat( " Taken: {0}", m_targetBranchTaken.SpanningTreeIndex );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "if {0} {1}{2} {3} then goto {4} else goto {5}", this.FirstArgument, m_condition, m_fSigned ? ".signed" : ".unsigned", this.SecondArgument, m_targetBranchTaken, m_targetBranchNotTaken );
        }
    }
}
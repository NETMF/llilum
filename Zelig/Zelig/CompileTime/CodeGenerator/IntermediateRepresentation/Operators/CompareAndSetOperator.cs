//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CompareAndSetOperator : Operator
    {
        public enum ActionCondition
        {
            EQ = 0,
            GE = 1,
            GT = 2,
            LE = 3,
            LT = 4,
            NE = 5,
        }

        //
        // State
        //

        private ActionCondition m_condition;
        private bool            m_fSigned;

        //
        // Constructor Methods
        //

        private CompareAndSetOperator( Debugging.DebugInfo  debugInfo    ,
                                       OperatorCapabilities capabilities ,
                                       ActionCondition      condition    ,
                                       bool                 fSigned      ) : base( debugInfo, capabilities, OperatorLevel.ConcreteTypes_NoExceptions )
        {
            m_condition = condition;
            m_fSigned   = fSigned;
        }

        //--//

        public static CompareAndSetOperator New( Debugging.DebugInfo debugInfo ,
                                                 ActionCondition     condition ,
                                                 bool                fSigned   ,
                                                 VariableExpression  lhs       ,
                                                 Expression          rhsLeft   ,
                                                 Expression          rhsRight  )
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
                case ActionCondition.EQ:
                case ActionCondition.NE:
                    capabilities |= OperatorCapabilities.IsCommutative;
                    break;

                default:
                    capabilities |= OperatorCapabilities.IsNonCommutative;
                    break;
            }

            CompareAndSetOperator res = new CompareAndSetOperator( debugInfo, capabilities, condition, fSigned );

            res.SetLhs( lhs               );
            res.SetRhs( rhsLeft, rhsRight );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new CompareAndSetOperator( m_debugInfo, m_capabilities, m_condition, m_fSigned ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_condition );
            context.Transform( ref m_fSigned   );

            context.Pop();
        }

        //--//

        public override ConstantExpression CanEvaluate( Operator[][]                  defChains  ,
                                                        Operator[][]                  useChains  ,
                                                        VariableExpression.Property[] properties )
        {
            var exL = FindConstantOrigin( this.FirstArgument , defChains, useChains, properties );
            var exR = FindConstantOrigin( this.SecondArgument, defChains, useChains, properties );
            
            if(exL != null && exR != null)
            {
                if(exL.IsValueInteger)
                {
                    if(!exR.IsValueInteger)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot mix integer and floating-point values in the same operation: {0}", this );
                    }

                    bool res;

                    if(this.Signed)
                    {
                        long valL;
                        long valR;

                        if(exL.GetAsSignedInteger( out valL ) == false ||
                           exR.GetAsSignedInteger( out valR ) == false  )
                        {
                            return null;
                        }

                        switch(m_condition)
                        {
                            case ActionCondition.EQ: res = (valL == valR); break;
                            case ActionCondition.GE: res = (valL >= valR); break; 
                            case ActionCondition.GT: res = (valL >  valR); break;
                            case ActionCondition.LE: res = (valL <= valR); break;
                            case ActionCondition.LT: res = (valL <  valR); break;
                            case ActionCondition.NE: res = (valL != valR); break;

                            default:
                                return null;
                        }
                    }
                    else
                    {
                        ulong valL;
                        ulong valR;

                        if(exL.GetAsUnsignedInteger( out valL ) == false ||
                           exR.GetAsUnsignedInteger( out valR ) == false  )
                        {
                            return null;
                        }

                        switch(m_condition)
                        {
                            case ActionCondition.EQ: res = (valL == valR); break;
                            case ActionCondition.GE: res = (valL >= valR); break; 
                            case ActionCondition.GT: res = (valL >  valR); break;
                            case ActionCondition.LE: res = (valL <= valR); break;
                            case ActionCondition.LT: res = (valL <  valR); break;
                            case ActionCondition.NE: res = (valL != valR); break;

                            default:
                                return null;
                        }
                    }

                    return new ConstantExpression( this.FirstResult.Type, res );
                }

                if(exL.IsValueFloatingPoint)
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
                        bool  res;

                        switch(m_condition)
                        {
                            case ActionCondition.EQ: res = (valL2 == valR2); break;
                            case ActionCondition.GE: res = (valL2 >= valR2); break; 
                            case ActionCondition.GT: res = (valL2 >  valR2); break;
                            case ActionCondition.LE: res = (valL2 <= valR2); break;
                            case ActionCondition.LT: res = (valL2 <  valR2); break;
                            case ActionCondition.NE: res = (valL2 != valR2); break;

                            default:
                                return null;
                        }

                        return new ConstantExpression( this.FirstResult.Type, res );
                    }

                    if(valL is double)
                    {
                        if(!(valR is double))
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot mix single and double values in the same operation: {0}", this );
                        }

                        double valL2 = (double)valL;
                        double valR2 = (double)valR;
                        bool   res;

                        switch(m_condition)
                        {
                            case ActionCondition.EQ: res = (valL2 == valR2); break;
                            case ActionCondition.GE: res = (valL2 >= valR2); break; 
                            case ActionCondition.GT: res = (valL2 >  valR2); break;
                            case ActionCondition.LE: res = (valL2 <= valR2); break;
                            case ActionCondition.LT: res = (valL2 <  valR2); break;
                            case ActionCondition.NE: res = (valL2 != valR2); break;

                            default:
                                return null;
                        }

                        return new ConstantExpression( this.FirstResult.Type, res );
                    }
                }
            }

            return null;
        }

        //--//

        //
        // Access Methods
        //

        public ActionCondition Condition
        {
            get
            {
                return m_condition;
            }
        }

        public ActionCondition InvertedCondition
        {
            get
            {
                switch(m_condition)
                {
                    case ActionCondition.EQ: return ActionCondition.NE;
                    case ActionCondition.GE: return ActionCondition.LT;
                    case ActionCondition.GT: return ActionCondition.LE;
                    case ActionCondition.LE: return ActionCondition.GT;
                    case ActionCondition.LT: return ActionCondition.GE;
                    case ActionCondition.NE: return ActionCondition.EQ;
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

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "CompareAndSetOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Cond: {0}", m_condition );

            sb.AppendFormat( " Signed: {0}", m_fSigned );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = {1} {2}{3} {4}", this.FirstResult, this.FirstArgument, m_condition, m_fSigned ? ".signed" : ".unsigned", this.SecondArgument );
        }
   }
}
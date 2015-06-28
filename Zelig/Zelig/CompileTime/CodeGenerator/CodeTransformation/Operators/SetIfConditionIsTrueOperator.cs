//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class SetIfConditionIsTrueOperator : Operator
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

        private ConditionCodeExpression.Comparison m_condition;

        //
        // Constructor Methods
        //

        private SetIfConditionIsTrueOperator( Debugging.DebugInfo                debugInfo ,
                                              ConditionCodeExpression.Comparison condition ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_condition = condition;
        }

        //--//

        public static SetIfConditionIsTrueOperator New( Debugging.DebugInfo                debugInfo ,
                                                        ConditionCodeExpression.Comparison condition ,
                                                        VariableExpression                 lhs       ,
                                                        VariableExpression                 rhs       )
        {
            CHECKS.ASSERT( rhs.AliasedVariable is ConditionCodeExpression, "Invalid type for condition code: {0}", rhs );

            var res = new SetIfConditionIsTrueOperator( debugInfo, condition );

            res.SetLhs( lhs );
            res.SetRhs( rhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new SetIfConditionIsTrueOperator( m_debugInfo, m_condition ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_condition );

            context2.Pop();
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
                    bool   fSet;

                    if(left is ulong && right is ulong)
                    {
                        ulong valL = (ulong)left;
                        ulong valR = (ulong)right;

                        switch(m_condition)
                        {
                            case ConditionCodeExpression.Comparison.Equal                   : fSet =       valL ==       valR; break;
                            case ConditionCodeExpression.Comparison.NotEqual                : fSet =       valL !=       valR; break;
                                                                                                                     
                            case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame: fSet =       valL >=       valR; break;
                            case ConditionCodeExpression.Comparison.UnsignedLowerThan       : fSet =       valL <        valR; break;
                            case ConditionCodeExpression.Comparison.UnsignedHigherThan      : fSet =       valL >        valR; break;
                            case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame : fSet =       valL <=       valR; break; 
                                                                                        
                            case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual: fSet = (long)valL >= (long)valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThan          : fSet = (long)valL <  (long)valR; break;
                            case ConditionCodeExpression.Comparison.SignedGreaterThan       : fSet = (long)valL >  (long)valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThanOrEqual   : fSet = (long)valL <= (long)valR; break;

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
                            case ConditionCodeExpression.Comparison.Equal                   : fSet = valL == valR; break;
                            case ConditionCodeExpression.Comparison.NotEqual                : fSet = valL != valR; break;
                                                                                                               
////                        case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame: fSet = valL >= valR; break;
////                        case ConditionCodeExpression.Comparison.UnsignedLowerThan       : fSet = valL <  valR; break;
////                        case ConditionCodeExpression.Comparison.UnsignedHigherThan      : fSet = valL >  valR; break;
////                        case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame : fSet = valL <= valR; break; 
                                                                                        
                            case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual: fSet = valL >= valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThan          : fSet = valL <  valR; break;
                            case ConditionCodeExpression.Comparison.SignedGreaterThan       : fSet = valL >  valR; break;
                            case ConditionCodeExpression.Comparison.SignedLessThanOrEqual   : fSet = valL <= valR; break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Unexpected input to {0}", this );
                        }
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unexpected input to {0}", this );
                    }

                    var opNew = SingleAssignmentOperator.New( this.DebugInfo, this.FirstResult, m_basicBlock.Owner.TypeSystemForIR.CreateConstant( fSet ? 1 : 0 ) );

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
            sb.Append( "SetIfConditionIsTrueOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Cond: {0}", m_condition );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = set if {1} is {2}", this.FirstResult, this.FirstArgument, m_condition );
        }
    }
}
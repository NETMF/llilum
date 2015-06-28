//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CompareOperator : Operator
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

        //
        // Constructor Methods
        //

        private CompareOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static CompareOperator New( Debugging.DebugInfo debugInfo ,
                                           VariableExpression  lhs       ,
                                           Expression          rhsLeft   ,
                                           Expression          rhsRight  )
        {
            CHECKS.ASSERT( lhs.AliasedVariable is ConditionCodeExpression, "Expecting a ConditionCodeExpression for the 'lhs' parameter, got '{0}'", lhs );

            CompareOperator res = new CompareOperator( debugInfo );

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
            return RegisterAndCloneState( context, new CompareOperator( m_debugInfo ) );
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
                object objL;
                object objR;

                if(exL.IsValueInteger && exR.IsValueInteger)
                {
                    ulong valL;
                    ulong valR;

                    exL.GetAsRawUlong( out valL );
                    exR.GetAsRawUlong( out valR );

                    objL = valL;
                    objR = valR;
                }
                else if(exL.IsValueFloatingPoint && exR.IsValueFloatingPoint)
                {
                    double valL;
                    double valR;

                    exL.GetFloatingPoint( out valL );
                    exR.GetFloatingPoint( out valR );

                    objL = valL;
                    objR = valR;
                }
                else
                {
                    return null;
                }

                return new ConstantExpression( this.FirstResult.Type, new ConditionCodeExpression.ConstantResult( objL, objR ) );
            }

            return null;
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "CompareOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = compare {1} <-> {2}", this.FirstResult, this.FirstArgument, this.SecondArgument );
        }
   }
}
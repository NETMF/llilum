//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LongCompareOperator : Operator
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

        private LongCompareOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static LongCompareOperator New( Debugging.DebugInfo debugInfo  ,
                                               VariableExpression  lhs        ,
                                               Expression          rhsLeftLo  ,
                                               Expression          rhsLeftHi  ,
                                               Expression          rhsRightLo ,
                                               Expression          rhsRightHi )
        {
            CHECKS.ASSERT( lhs.AliasedVariable is ConditionCodeExpression, "Invalid type for condition code: {0}", lhs );

            var res = new LongCompareOperator( debugInfo );

            res.SetLhs     ( lhs                                          );
            res.SetRhsArray( rhsLeftLo, rhsLeftHi, rhsRightLo, rhsRightHi );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new LongCompareOperator( m_debugInfo ) );
        }

        //--//

        public override ConstantExpression CanEvaluate( Operator[][]                  defChains  ,
                                                        Operator[][]                  useChains  ,
                                                        VariableExpression.Property[] properties )
        {
            var exLlo = FindConstantOrigin( this.FirstArgument , defChains, useChains, properties );
            var exLhi = FindConstantOrigin( this.SecondArgument, defChains, useChains, properties );
            var exRlo = FindConstantOrigin( this.ThirdArgument , defChains, useChains, properties );
            var exRhi = FindConstantOrigin( this.FourthArgument, defChains, useChains, properties );

            if(exLlo != null && exLhi != null && exRlo != null && exRhi != null)
            {
                object objL;
                object objR;

                if(exLlo.IsValueInteger && exLhi.IsValueInteger && exRlo.IsValueInteger && exRhi.IsValueInteger)
                {
                    ulong valLlo;
                    ulong valLhi;
                    ulong valRlo;
                    ulong valRhi;

                    exLlo.GetAsRawUlong( out valLlo );
                    exLhi.GetAsRawUlong( out valLhi );
                    exRlo.GetAsRawUlong( out valRlo );
                    exRhi.GetAsRawUlong( out valRhi );

                    objL = valLlo | (valLhi << 32);
                    objR = valRlo | (valRhi << 32);
                }
                else
                {
                    return null;
                }

                return new ConstantExpression( this.FirstResult.Type, new ConditionCodeExpression.ConstantResult( objL, objR ) );
            }

            return null;
        }

        internal Expression[] IsCompareToNull( out bool fNullOnRight )
        {
            if(this.FirstArgument .IsEqualToZero() &&
               this.SecondArgument.IsEqualToZero()  )
            {
                fNullOnRight = false;

                return new Expression[] { this.ThirdArgument, this.FourthArgument };
            }

            if(this.ThirdArgument .IsEqualToZero() &&
               this.FourthArgument.IsEqualToZero()  )
            {
                fNullOnRight = true;

                return new Expression[] { this.FirstArgument, this.SecondArgument };
            }

            fNullOnRight = false;

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
            sb.Append( "LongCompareOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = longcompare {1}:{2} <-> {3}:{4}", this.FirstResult, this.FirstArgument, this.SecondArgument, this.ThirdArgument, this.FourthArgument );
        }
   }
}
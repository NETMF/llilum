//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class TruncateOperator : ConversionOperator
    {
        //
        // Constructor Methods
        //

        private TruncateOperator( Debugging.DebugInfo  debugInfo       ,
                                  OperatorCapabilities capabilities    ,
                                  OperatorLevel        level           ,
                                  uint                 destinationSize ,
                                  bool                 fOverflow       ) : base( debugInfo, capabilities, level, destinationSize, fOverflow )
        {
        }

        //--//

        public static TruncateOperator New( Debugging.DebugInfo debugInfo       ,
                                            uint                destinationSize ,
                                            bool                fOverflow       ,
                                            VariableExpression  lhs             ,
                                            Expression          rhs             )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            if(fOverflow)
            {
                capabilities |= OperatorCapabilities.MayThrow;
                level         = OperatorLevel.ConcreteTypes;
            }
            else
            {
                capabilities |= OperatorCapabilities.DoesNotThrow;
                level         = OperatorLevel.Lowest;
            }

            TruncateOperator res = new TruncateOperator( debugInfo, capabilities, level, destinationSize, fOverflow );

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
            return RegisterAndCloneState( context, new TruncateOperator( m_debugInfo, m_capabilities, m_level, this.SignificantSize, this.CheckOverflow ) );
        }

        //--//

        public override ConstantExpression CanEvaluate( Operator[][]                  defChains  ,
                                                        Operator[][]                  useChains  ,
                                                        VariableExpression.Property[] properties )
        {
            var ex = FindConstantOrigin( this.FirstArgument, defChains, useChains, properties );
            if(ex != null)
            {
                if(ex.IsValueInteger)
                {
                    ulong val;
                    
                    if(ex.GetAsRawUlong( out val ) == false)
                    {
                        return null;
                    }

                    int   bits        = 8 * (int)this.SignificantSize;
                    ulong valModified = val & ((1ul << bits) - 1ul);

                    if(this.CheckOverflow)
                    {
                        if(valModified != val)
                        {
                            throw TypeConsistencyErrorException.Create( "Found an overflow: cannot truncate '{0}' from {1} bits without losing precision", ex, bits );
                        }
                    }

                    return new ConstantExpression( this.FirstResult.Type, valModified );
                }
            }

            return null;
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "Truncate(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = truncate {1} to {2} bits {3}", this.FirstResult, this.FirstArgument, 8 * this.SignificantSize, this.CheckOverflow ? " with overflow check" : "" );
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ZeroExtendOperator : ConversionOperator
    {
        //
        // Constructor Methods
        //

        private ZeroExtendOperator( Debugging.DebugInfo  debugInfo    ,
                                    OperatorCapabilities capabilities ,
                                    OperatorLevel        level        ,
                                    uint                 sourceSize   ,
                                    bool                 fOverflow    ) : base( debugInfo, capabilities, level, sourceSize, fOverflow )
        {
        }

        //--//

        public static ZeroExtendOperator New( Debugging.DebugInfo debugInfo  ,
                                              uint                sourceSize ,
                                              bool                fOverflow  ,
                                              VariableExpression  lhs        ,
                                              Expression          rhs        )
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

            ZeroExtendOperator res = new ZeroExtendOperator( debugInfo, capabilities, level, sourceSize, fOverflow );

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
            return RegisterAndCloneState( context, new ZeroExtendOperator( m_debugInfo, m_capabilities, m_level, this.SignificantSize, this.CheckOverflow ) );
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
                            throw TypeConsistencyErrorException.Create( "Found an overflow: cannot zero extend '{0}' from {1} bits without losing precision", ex, bits );
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
            sb.Append( "ZeroExtend(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = zeroextend {1} from {2} bits {3}", this.FirstResult, this.FirstArgument, 8 * this.SignificantSize, this.CheckOverflow ? " with overflow check" : "" );
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class UnaryOperator : AbstractUnaryOperator
    {
        //
        // Constructor Methods
        //

        private UnaryOperator( Debugging.DebugInfo  debugInfo    ,
                               OperatorCapabilities capabilities ,
                               OperatorLevel        level        ,
                               ALU                  alu          ,
                               bool                 fSigned      ,
                               bool                 fOverflow    ) : base( debugInfo, capabilities, level, alu, fSigned, fOverflow )
        {
        }

        //--//

        public static UnaryOperator New( Debugging.DebugInfo debugInfo ,
                                         ALU                 alu       ,
                                         bool                fSigned   ,
                                         bool                fOverflow ,
                                         VariableExpression  lhs       ,
                                         Expression          rhs       )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            if(fOverflow) capabilities |= OperatorCapabilities.MayThrow;
            else          capabilities |= OperatorCapabilities.DoesNotThrow;

            OperatorLevel level = fOverflow ? OperatorLevel.ConcreteTypes : OperatorLevel.Lowest;

            var res = new UnaryOperator( debugInfo, capabilities, level, alu, fSigned, fOverflow );

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
            return RegisterAndCloneState( context, new UnaryOperator( m_debugInfo, m_capabilities, m_level, m_alu, m_fSigned, m_fOverflow ) );
        }

        //--//

        public override ConstantExpression CanEvaluate( Operator[][]                  defChains  ,
                                                        Operator[][]                  useChains  ,
                                                        VariableExpression.Property[] properties )
        {
            var ex = FindConstantOrigin( this.FirstArgument, defChains, useChains, properties );
            if(ex != null)
            {
                //
                // BUGBUG: Overflow checking is ignored!
                //
                if(ex.IsValueInteger)
                {
                    ulong val;
                    
                    if(ex.GetAsRawUlong( out val ) == false)
                    {
                        return null;
                    }

                    switch(m_alu)
                    {
                        case ALU.NEG:
                            val = 0 - val;
                            break;

                        case ALU.NOT:
                            val = ~val;
                            break;

                        default:
                            return null;
                    }

                    return new ConstantExpression( ex.Type, val );
                }

                if(ex.IsValueFloatingPoint)
                {
                    object val = ex.Value;

                    if(val is float)
                    {
                        float val2 = (float)val;

                        switch(m_alu)
                        {
                            case ALU.NEG:
                                val2 = -val2;
                                break;

                            default:
                                return null;
                        }

                        return new ConstantExpression( ex.Type, val2 );
                    }

                    if(val is double)
                    {
                        double val2 = (double)val;

                        switch(m_alu)
                        {
                            case ALU.NEG:
                                val2 = -val2;
                                break;

                            default:
                                return null;
                        }

                        return new ConstantExpression( ex.Type, val2 );
                    }
                }
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
            sb.Append( "UnaryOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = {1} {2}{3}", this.FirstResult, m_alu, this.FirstArgument, this.MayThrow ? "  MayThrow" : "" );
        }
    }
}
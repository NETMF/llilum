//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class BinaryOperator : AbstractBinaryOperator
    {
        //
        // Constructor Methods
        //

        private BinaryOperator( Debugging.DebugInfo  debugInfo    ,
                                OperatorCapabilities capabilities ,
                                OperatorLevel        level        ,
                                ALU                  alu          ,
                                bool                 fSigned      ,
                                bool                 fOverflow    ) : base( debugInfo, capabilities, level, alu, fSigned, fOverflow )
        {
        }

        //--//

        public static BinaryOperator New( Debugging.DebugInfo debugInfo ,
                                          ALU                 alu       ,
                                          bool                fSigned   ,
                                          bool                fOverflow ,
                                          VariableExpression  lhs       ,
                                          Expression          rhsLeft   ,
                                          Expression          rhsRight  )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            switch(alu)
            {
                case ALU.ADD:
                case ALU.MUL:
                case ALU.AND:
                case ALU.OR:
                case ALU.XOR:
                    capabilities |= OperatorCapabilities.IsCommutative;
                    break;

                default:
                    capabilities |= OperatorCapabilities.IsNonCommutative;
                    break;
            }

            if(fOverflow) capabilities |= OperatorCapabilities.MayThrow;
            else          capabilities |= OperatorCapabilities.DoesNotThrow;

            OperatorLevel level = fOverflow ? OperatorLevel.ConcreteTypes : OperatorLevel.Lowest;

            var res = new BinaryOperator( debugInfo, capabilities, level, alu, fSigned, fOverflow );

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
            return RegisterAndCloneState( context, new BinaryOperator( m_debugInfo, m_capabilities, m_level, m_alu, m_fSigned, m_fOverflow ) );
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
                //
                // BUGBUG: Overflow checking is ignored!
                //
                if(exL.IsValueInteger)
                {
                    if(!exR.IsValueInteger)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot mix integer and floating-point values in the same operation: {0}", this );
                    }

                    ulong valL;
                    ulong valR;

                    if(exL.GetAsRawUlong( out valL ) == false ||
                       exR.GetAsRawUlong( out valR ) == false  )
                    {
                        return null;
                    }

                    switch(m_alu)
                    {
                        case ALU.ADD:
                            valL = valL + valR;
                            break;

                        case ALU.SUB:
                            valL = valL - valR;
                            break;

                        case ALU.MUL:
                        case ALU.DIV:
                        case ALU.REM:
                            {
                                bool negate = false;

                                if(exL.IsValueSigned)
                                {
                                    long svalL = (long)valL;
                                    if(svalL < 0)
                                    {
                                        valL   = (ulong)-svalL;
                                        negate = !negate;
                                    }
                                }

                                if(exR.IsValueSigned)
                                {
                                    long svalR = (long)valR;
                                    if(svalR < 0)
                                    {
                                        valR   = (ulong)-svalR;
                                        negate = !negate;
                                    }
                                }

                                switch(m_alu)
                                {
                                    case ALU.MUL:
                                        valL = valL * valR;
                                        break;

                                    case ALU.DIV:
                                        if(valR == 0)
                                        {
                                            throw TypeConsistencyErrorException.Create( "Found division by zero: {0}", this );
                                        }

                                        valL = valL / valR;
                                        break;

                                    case ALU.REM:
                                        if(valR == 0)
                                        {
                                            throw TypeConsistencyErrorException.Create( "Found remainder by zero: {0}", this );
                                        }

                                        valL = valL % valR;
                                        break;
                                }

                                if(negate)
                                {
                                    long svalL = (long)valL;

                                    valL = (ulong)-svalL;
                                }
                            }
                            break;

                        case ALU.AND:
                            valL = valL & valR;
                            break;

                        case ALU.OR :
                            valL = valL | valR;
                            break;

                        case ALU.XOR:
                            valL = valL ^ valR;
                            break;

                        case ALU.SHL:
                            valL <<= (int)valR;
                            break;

                        case ALU.SHR:
                            if(exL.IsValueSigned)
                            {
                                long svalL = (long)valL;

                                valL = (ulong)(svalL >> (int)valR);
                            }
                            else
                            {
                                valL = valL >> (int)valR;
                            }
                            break;

                        default:
                            return null;
                    }

                    return new ConstantExpression( this.FirstResult.Type, valL );
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

                        switch(m_alu)
                        {
                            case ALU.ADD:
                                valL2 = valL2 + valR2;
                                break;

                            case ALU.SUB:
                                valL2 = valL2 - valR2;
                                break;

                            case ALU.MUL:
                                valL2 = valL2 * valR2;
                                break;

                            case ALU.DIV:
                                // Divide by zero for float/double is a valid option it should return NaN
                                //if(valR2 == 0)
                                //{
                                //    throw TypeConsistencyErrorException.Create( "Found division by zero: {0}", this );
                                //}

                                valL2 = valL2 / valR2;
                                break;

                            case ALU.REM:
                                // Divide by zero for float/double is a valid option it should return NaN
                                //if(valR2 == 0)
                                //{
                                //    throw TypeConsistencyErrorException.Create( "Found remainder by zero: {0}", this );
                                //}

                                valL2 = valL2 % valR2;
                                break;

                            default:
                                return null;
                        }

                        return new ConstantExpression( this.FirstResult.Type, valL2 );
                    }

                    if(valL is double)
                    {
                        if(!(valR is double))
                        {
                            throw TypeConsistencyErrorException.Create( "Cannot mix single and double values in the same operation: {0}", this );
                        }

                        double valL2 = (double)valL;
                        double valR2 = (double)valR;

                        switch(m_alu)
                        {
                            case ALU.ADD:
                                valL2 = valL2 + valR2;
                                break;

                            case ALU.SUB:
                                valL2 = valL2 - valR2;
                                break;

                            case ALU.MUL:
                                valL2 = valL2 * valR2;
                                break;

                            case ALU.DIV:
                                // Divide by zero for float/double is a valid option it should return NaN
                                //if(valR2 == 0)
                                //{
                                //    throw TypeConsistencyErrorException.Create( "Found division by zero: {0}", this );
                                //}

                                valL2 = valL2 / valR2;
                                break;

                            case ALU.REM:
                                // Divide by zero for float/double is a valid option it should return NaN
                                //if(valR2 == 0)
                                //{
                                //    throw TypeConsistencyErrorException.Create( "Found remainder by zero: {0}", this );
                                //}

                                valL2 = valL2 % valR2;
                                break;

                            default:
                                return null;
                        }

                        return new ConstantExpression( this.FirstResult.Type, valL2 );
                    }
                }
            }

            return null;
        }

        public override bool Simplify( Operator[][]                  defChains  ,
                                       Operator[][]                  useChains  ,
                                       VariableExpression.Property[] properties )
        {
            if(base.Simplify( defChains, useChains, properties ))
            {
                return true;
            }

            var rhs1 = this.FirstArgument;
            var rhs2 = this.SecondArgument;

            var exConst1 = FindConstantOrigin( rhs1, defChains, useChains, properties );
            var exConst2 = FindConstantOrigin( rhs2, defChains, useChains, properties );

            if(exConst2 != null)
            {
                if(Simplify( rhs1, exConst2 ))
                {
                    return true;
                }
            }

            if(exConst1 != null && this.IsCommutative)
            {
                if(Simplify( rhs2, exConst1 ))
                {
                    return true;
                }
            }

            return false;
        }

        private bool Simplify( Expression         rhs     ,
                               ConstantExpression exConst )
        {
            if(exConst != null && exConst.IsValueInteger)
            {
                ulong valU;

                if(exConst.GetAsRawUlong( out valU ))
                {
                    long            val = (long)valU;
                    TypeSystemForIR ts  =       m_basicBlock.Owner.TypeSystemForIR;
                    var             lhs =       this.FirstResult;

                    if(val == 0)
                    {
                        switch(this.Alu)
                        {
                            case ALU.ADD:
                            case ALU.SUB:
                            case ALU.OR:
                            case ALU.XOR:
                            case ALU.SHL:
                            case ALU.SHR:
                                {
                                    var opNew = SingleAssignmentOperator.New( this.DebugInfo, lhs, rhs );

                                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                    return true;
                                }

                            case ALU.MUL:
                            case ALU.AND:
                                {
                                    ConstantExpression exConst2 = ts.CreateConstant( ts.WellKnownTypes.System_Int32, 0 );

                                    var opNew = SingleAssignmentOperator.New( this.DebugInfo, lhs, exConst2 );

                                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                    return true;
                                }
                        }
                    }
                    else if(val == 1)
                    {
                        switch(this.Alu)
                        {
                            case ALU.MUL:
                            case ALU.DIV:
                                {
                                    var opNew = SingleAssignmentOperator.New( this.DebugInfo, lhs, rhs );

                                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                    return true;
                                }

                            case ALU.REM:
                                {
                                    ConstantExpression exConst2 = ts.CreateConstant( ts.WellKnownTypes.System_Int32, 0 );

                                    var opNew = SingleAssignmentOperator.New( this.DebugInfo, lhs, exConst2 );

                                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                    return true;
                                }
                        }
                    }

                    //
                    // Look for simple shifts between values of the same type.
                    //
                    var  tdLhs = lhs.Type;
                    var  tdRhs = rhs.Type;
                    bool fCheckSimpleShift;

                    if(tdLhs == tdRhs)
                    {
                        fCheckSimpleShift = true;
                    }
                    else if(tdLhs.ValidLayout && tdRhs.ValidLayout && tdLhs.Size == tdRhs.Size)
                    {
                        fCheckSimpleShift = true;
                    }
                    else
                    {
                        fCheckSimpleShift = false;
                    }

                    if(fCheckSimpleShift)
                    {
                        if(val > 0 && val < uint.MaxValue)
                        {
                            uint val2 = (uint)val;
                            int  pos  = BitVector.GetPositionOfFirstBitSet( val2 );

                            val2 &= ~(1u << pos);
                            if(val2 == 0)
                            {
                                //
                                // It's a power of two.
                                //

                                switch(this.Alu)
                                {
                                    case ALU.MUL:
                                        {
                                            //
                                            // It's just a shift.
                                            //
                                            ConstantExpression exConst2 = ts.CreateConstant( ts.WellKnownTypes.System_Int32, pos );

                                            BinaryOperator opNew = BinaryOperator.New( this.DebugInfo, ALU.SHL, this.Signed, this.CheckOverflow, lhs, rhs, exConst2 );

                                            this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                            return true;
                                        }

                                    case ALU.DIV:
                                        {
                                            //
                                            // It's just a shift.
                                            //
                                            ConstantExpression exConst2 = ts.CreateConstant( ts.WellKnownTypes.System_Int32, pos );

                                            BinaryOperator opNew = BinaryOperator.New( this.DebugInfo, ALU.SHR, this.Signed, this.CheckOverflow, lhs, rhs, exConst2 );

                                            this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                            return true;
                                        }

                                    case ALU.REM:
                                        {
                                            //
                                            // It's just a mask.
                                            //
                                            ConstantExpression exConst2 = ts.CreateConstant( ts.WellKnownTypes.System_Int32, (1 << pos ) - 1 );

                                            BinaryOperator opNew = BinaryOperator.New( this.DebugInfo, ALU.AND, this.Signed, this.CheckOverflow, lhs, rhs, exConst2 );

                                            this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                            return true;
                                        }
                                }
                            }
                        }
                    }
                }
            }

            return false;
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
            sb.Append( "BinaryOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = {1} {2} {3}{4}", this.FirstResult, this.FirstArgument, m_alu, this.SecondArgument, this.MayThrow ? "  MayThrow" : "" );
        }
    }
}
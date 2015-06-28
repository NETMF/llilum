//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ConvertOperator : Operator
    {
        //
        // State
        //

        private TypeRepresentation.BuiltInTypes m_kindInput;
        private TypeRepresentation.BuiltInTypes m_kindOutput;
        private bool                            m_fOverflow;

        //
        // Constructor Methods
        //

        private ConvertOperator( Debugging.DebugInfo             debugInfo    ,
                                 OperatorCapabilities            capabilities ,
                                 OperatorLevel                   level        ,
                                 TypeRepresentation.BuiltInTypes kindInput    ,
                                 TypeRepresentation.BuiltInTypes kindOutput   ,
                                 bool                            fOverflow    ) : base( debugInfo, capabilities, level )
        {
            m_kindInput  = kindInput;
            m_kindOutput = kindOutput;
            m_fOverflow  = fOverflow;
        }

        //--//

        public static ConvertOperator New( Debugging.DebugInfo             debugInfo  ,
                                           TypeRepresentation.BuiltInTypes kindInput  ,
                                           TypeRepresentation.BuiltInTypes kindOutput ,
                                           bool                            fOverflow  ,
                                           VariableExpression              lhs        ,
                                           Expression                      rhs        )
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

            OperatorLevel level = fOverflow ? OperatorLevel.ConcreteTypes : (lhs.Type.IsNumeric && rhs.Type.IsNumeric ? OperatorLevel.ScalarValues : OperatorLevel.ConcreteTypes_NoExceptions);

            ConvertOperator res = new ConvertOperator( debugInfo, capabilities, level, kindInput, kindOutput, fOverflow );

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
            return RegisterAndCloneState( context, new ConvertOperator( m_debugInfo, m_capabilities, m_level, m_kindInput, m_kindOutput, m_fOverflow ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_kindInput  );
            context.Transform( ref m_kindOutput );
            context.Transform( ref m_fOverflow  );

            context.Pop();
        }

        //--//

        public override ConstantExpression CanEvaluate( Operator[][]                  defChains  ,
                                                        Operator[][]                  useChains  ,
                                                        VariableExpression.Property[] properties )
        {
            //
            // TODO: We should change the condition code to the outcome of the operation.
            //
            if(this.Results.Length != 1)
            {
                return null;
            }

            var ex = FindConstantOrigin( this.FirstArgument, defChains, useChains, properties );
            if(ex != null)
            {
                //
                // BUGBUG: Overflow checking is ignored!
                //
                object valIN  = ex.Value;
                object valOUT = null;

                switch(m_kindOutput)
                {
                    case TypeRepresentation.BuiltInTypes.R4:
                        if(valIN is int)
                        {
                            int val = (int)valIN;

                            valOUT = (float)val;
                        }
                        else if(valIN is uint)
                        {
                            uint val = (uint)valIN;

                            valOUT = (float)val;
                        }
                        else if(valIN is long)
                        {
                            long val = (long)valIN;

                            valOUT = (float)val;
                        }
                        else if(valIN is ulong)
                        {
                            ulong val = (ulong)valIN;

                            valOUT = (float)val;
                        }
                        else if(valIN is float)
                        {
                            valOUT = (float)valIN;
                        }
                        else if(valIN is double)
                        {
                            double val = (double)valIN;

                            valOUT = (float)val;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.R8:
                        if(valIN is int)
                        {
                            int val = (int)valIN;

                            valOUT = (double)val;
                        }
                        else if(valIN is uint)
                        {
                            uint val = (uint)valIN;

                            valOUT = (double)val;
                        }
                        else if(valIN is long)
                        {
                            long val = (long)valIN;

                            valOUT = (double)val;
                        }
                        else if(valIN is ulong)
                        {
                            ulong val = (ulong)valIN;

                            valOUT = (double)val;
                        }
                        else if(valIN is float)
                        {
                            float val = (float)valIN;

                            valOUT = (double)val;
                        }
                        else if(valIN is double)
                        {
                            valOUT = (double)valIN;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.I4:
                        if(valIN is float)
                        {
                            float val = (float)valIN;

                            valOUT = (int)val;
                        }
                        else if(valIN is double)
                        {
                            double val = (double)valIN;

                            valOUT = (int)val;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.U4:
                        if(valIN is float)
                        {
                            float val = (float)valIN;

                            valOUT = (uint)val;
                        }
                        else if(valIN is double)
                        {
                            double val = (double)valIN;

                            valOUT = (uint)val;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.I8:
                        if(valIN is float)
                        {
                            float val = (float)valIN;

                            valOUT = (ulong)val;
                        }
                        else if(valIN is double)
                        {
                            double val = (double)valIN;

                            valOUT = (ulong)val;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.U8:
                        if(valIN is float)
                        {
                            float val = (float)valIN;

                            valOUT = (long)val;
                        }
                        else if(valIN is double)
                        {
                            double val = (double)valIN;

                            valOUT = (long)val;
                        }
                        break;
                }

                if(valOUT != null)
                {
                    return new ConstantExpression( this.FirstResult.Type, valOUT );
                }
            }

            return null;
        }

        //--//

        //
        // Access Methods
        //

        public TypeRepresentation.BuiltInTypes InputKind
        {
            get
            {
                return m_kindInput;
            }
        }

        public TypeRepresentation.BuiltInTypes OutputKind
        {
            get
            {
                return m_kindOutput;
            }
        }

        public bool CheckOverflow
        {
            get
            {
                return m_fOverflow;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "ConvertOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Input: {0}" , m_kindInput  );
            sb.AppendFormat( " Output: {0}", m_kindOutput );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = convert {1} from {2} to {3}{4}", this.FirstResult, this.FirstArgument, m_kindInput, m_kindOutput, this.MayThrow ? "  MayThrow" : "" );
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class MethodRepresentationOperator : Operator
    {
        //
        // State
        //

        private MethodRepresentation m_md;

        //
        // Constructor Methods
        //

        private MethodRepresentationOperator( Debugging.DebugInfo  debugInfo    ,
                                              OperatorCapabilities capabilities ,
                                              OperatorLevel        level        ,
                                              MethodRepresentation md           ) : base( debugInfo, capabilities, level )
        {
            m_md = md;
        }

        //--//

        public static MethodRepresentationOperator New( Debugging.DebugInfo  debugInfo ,
                                                        MethodRepresentation md        ,
                                                        VariableExpression   lhs       )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotThrow                       |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            MethodRepresentationOperator res = new MethodRepresentationOperator( debugInfo, capabilities, OperatorLevel.Lowest, md );

            res.SetLhs( lhs );

            return res;
        }

        public static MethodRepresentationOperator New( Debugging.DebugInfo  debugInfo ,
                                                        MethodRepresentation md        ,
                                                        VariableExpression   lhs       ,
                                                        Expression           rhsObject )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.MayThrow                           |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            MethodRepresentationOperator res = new MethodRepresentationOperator( debugInfo, capabilities, OperatorLevel.ConcreteTypes, md ); // 'rhsObject' may be null.

            res.SetLhs( lhs       );
            res.SetRhs( rhsObject );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            MethodRepresentation md = context.ConvertMethod( m_md );

            return RegisterAndCloneState( context, new MethodRepresentationOperator( m_debugInfo, m_capabilities, m_level, md ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_md );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public MethodRepresentation Method
        {
            get
            {
                return m_md;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "MethodRepresentationOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " {0}", m_md );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            if(this.Arguments.Length == 0)
            {
                return dumper.FormatOutput( "{0} = descriptor for static method {1}", this.FirstResult, m_md );
            }
            else
            {
                return dumper.FormatOutput( "{0} = descriptor for method {1} on {2}", this.FirstResult, m_md, this.FirstArgument );
            }
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class UnboxOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.MayThrow                           |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // State
        //

        private TypeRepresentation m_td;

        //
        // Constructor Methods
        //

        private UnboxOperator( Debugging.DebugInfo debugInfo ,
                               TypeRepresentation  td        ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented )
        {
            m_td = td;
        }

        //--//

        public static UnboxOperator New( Debugging.DebugInfo debugInfo ,
                                         TypeRepresentation  td        ,
                                         VariableExpression  lhs       ,
                                         Expression          rhs       )
        {
            // If we are dealing with a boxed type in the R-value, then the underlying type must match. Refernce types are just OK
            CHECKS.ASSERT( rhs.Type is BoxedValueTypeRepresentation ? lhs.Type is ManagedPointerTypeRepresentation && lhs.Type.ContainedType == rhs.Type.ContainedType : true, "Incompatible types for unbox operator: {0} <=> {1}", lhs.Type, rhs.Type );

            UnboxOperator res = new UnboxOperator( debugInfo, td );

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
            return RegisterAndCloneState( context, new UnboxOperator( m_debugInfo, m_td ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_td );

            context.Pop();
        }

        //
        // Access Methods
        //

        public TypeRepresentation Type
        {
            get
            {
                return m_td;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "UnboxOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Class: {0}", m_td );
            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = unbox {1} as {2}", this.FirstResult, this.FirstArgument, m_td );
        }
   }
}

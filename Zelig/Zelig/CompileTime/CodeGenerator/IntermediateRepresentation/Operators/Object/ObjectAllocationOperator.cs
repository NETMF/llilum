//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ObjectAllocationOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.MayAllocateStorage                 |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.MayThrow                           | // Out of memory.
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // State
        //

        private TypeRepresentation   m_td;
        private MethodRepresentation m_md;

        //
        // Constructor Methods
        //

        private ObjectAllocationOperator( Debugging.DebugInfo  debugInfo ,
                                          TypeRepresentation   td        ,
                                          MethodRepresentation md        ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented )
        {
            m_td = td;
            m_md = md;
        }

        //--//

        public static ObjectAllocationOperator New( Debugging.DebugInfo debugInfo ,
                                                    TypeRepresentation  td        ,
                                                    VariableExpression  lhs       )
        {
            ObjectAllocationOperator res = new ObjectAllocationOperator( debugInfo, td, null );

            res.SetLhs( lhs );

            return res;
        }

        public static ObjectAllocationOperator New( Debugging.DebugInfo  debugInfo ,
                                                    MethodRepresentation md        ,
                                                    VariableExpression   lhs       ,
                                                    Expression[]         rhs       )
        {
            ObjectAllocationOperator res = new ObjectAllocationOperator( debugInfo, md.OwnerType, md );

            res.SetLhs     ( lhs );
            res.SetRhsArray( rhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            TypeRepresentation   td = context.ConvertType  ( m_td );
            MethodRepresentation md = context.ConvertMethod( m_md );

            return RegisterAndCloneState( context, new ObjectAllocationOperator( m_debugInfo, td, md ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_td );
            context.Transform( ref m_md );

            context.Pop();
        }

        //--//

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
            sb.Append( "ObjectAllocationOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " {0}", m_td );

            if(m_md != null)
            {
                sb.AppendFormat( " - {0}", m_md );
            }

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = new {1}", this.FirstResult, m_td );
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public class VectorHack_Finalize : VectorHack_Base
    {
        //
        // State
        //

        private ZeligIR.Abstractions.RegisterDescriptor m_resultBankBase;

        //
        // Constructor Methods
        //

        private VectorHack_Finalize( Debugging.DebugInfo                     debugInfo      ,
                                     int                                     size           ,
                                     ZeligIR.Abstractions.RegisterDescriptor resultBankBase ) : base( debugInfo, size )
        {
            m_resultBankBase = resultBankBase;
        }

        //--//

        public static VectorHack_Finalize New( Debugging.DebugInfo                     debugInfo      ,
                                               int                                     size           ,
                                               ZeligIR.Abstractions.RegisterDescriptor resultBankBase ,
                                               VariableExpression                      lhs            )
        {
            VectorHack_Finalize res = new VectorHack_Finalize( debugInfo, size, resultBankBase );

            res.SetLhs( lhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new VectorHack_Finalize( m_debugInfo, this.Size, m_resultBankBase ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_resultBankBase );

            context2.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public ZeligIR.Abstractions.RegisterDescriptor ResultBankBase
        {
            get
            {
                return m_resultBankBase;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "VectorHack_Finalize(Size={0},Base={1}", this.Size, m_resultBankBase.Mnemonic );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = VectorHack_Finalize(Size={1},Base={2})", this.FirstResult, this.Size, m_resultBankBase.Mnemonic );
        }
    }
}
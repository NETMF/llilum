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


    public class VectorHack_MultiplyAndAccumulate : VectorHack_Base
    {
        //
        // State
        //

        private ZeligIR.Abstractions.RegisterDescriptor m_leftBankBase;
        private ZeligIR.Abstractions.RegisterDescriptor m_rightBankBase;
        private ZeligIR.Abstractions.RegisterDescriptor m_resultBankBase;

        //
        // Constructor Methods
        //

        private VectorHack_MultiplyAndAccumulate( Debugging.DebugInfo                     debugInfo      ,
                                                  int                                     size           ,
                                                  ZeligIR.Abstractions.RegisterDescriptor leftBankBase   ,
                                                  ZeligIR.Abstractions.RegisterDescriptor rightBankBase  ,
                                                  ZeligIR.Abstractions.RegisterDescriptor resultBankBase ) : base( debugInfo, size )
        {
            m_leftBankBase   = leftBankBase;
            m_rightBankBase  = rightBankBase;
            m_resultBankBase = resultBankBase;
        }

        //--//

        public static VectorHack_MultiplyAndAccumulate New( Debugging.DebugInfo                     debugInfo      ,
                                                            int                                     size           ,
                                                            ZeligIR.Abstractions.RegisterDescriptor leftBankBase   ,
                                                            ZeligIR.Abstractions.RegisterDescriptor rightBankBase  ,
                                                            ZeligIR.Abstractions.RegisterDescriptor resultBankBase )
        {
            VectorHack_MultiplyAndAccumulate res = new VectorHack_MultiplyAndAccumulate( debugInfo, size, leftBankBase, rightBankBase, resultBankBase );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new VectorHack_MultiplyAndAccumulate( m_debugInfo, this.Size, m_leftBankBase, m_rightBankBase, m_resultBankBase ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_leftBankBase   );
            context2.Transform( ref m_rightBankBase  );
            context2.Transform( ref m_resultBankBase );

            context2.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public ZeligIR.Abstractions.RegisterDescriptor LeftBankBase
        {
            get
            {
                return m_leftBankBase;
            }
        }

        public ZeligIR.Abstractions.RegisterDescriptor RightBankBase
        {
            get
            {
                return m_rightBankBase;
            }
        }

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
            sb.AppendFormat( "VectorHack_MultiplyAndAccumulate(Size={0},LeftBase={1},RightBase={2},ResultBase={3}", this.Size, m_leftBankBase.Mnemonic, m_rightBankBase.Mnemonic, m_resultBankBase.Mnemonic );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "VectorHack_MultiplyAndAccumulate(Size={0},LeftBase={1},RightBase={2},ResultBase={3})", this.Size, m_leftBankBase.Mnemonic, m_rightBankBase.Mnemonic, m_resultBankBase.Mnemonic );
        }
    }
}
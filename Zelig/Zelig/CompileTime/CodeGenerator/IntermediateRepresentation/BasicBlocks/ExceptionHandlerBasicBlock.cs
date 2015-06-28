//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class ExceptionHandlerBasicBlock : BasicBlock
    {
        public static new readonly ExceptionHandlerBasicBlock[] SharedEmptyArray = new ExceptionHandlerBasicBlock[0];

        //
        // State
        //

        private ExceptionClause[] m_handlerFor;

        //
        // Constructor Methods
        //

        public ExceptionHandlerBasicBlock( ControlFlowGraphState owner ) : base( owner )
        {
            m_handlerFor = ExceptionClause.SharedEmptyArray;
        }

        //--//

        //
        // Helper Methods
        //

        public override BasicBlock Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new ExceptionHandlerBasicBlock( context.ControlFlowGraphDestination ) );
        }

        public override void CloneState( CloningContext context ,
                                         BasicBlock     clone   )
        {
            base.CloneState( context, clone );

            ExceptionHandlerBasicBlock cloneEh = (ExceptionHandlerBasicBlock)clone;

            cloneEh.m_handlerFor = context.Clone( m_handlerFor );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_handlerFor );

            context.Pop();
        }

        //--//

        public void SetAsHandlerFor( ExceptionClause ec )
        {
            BumpVersion();

            m_handlerFor = ArrayUtility.AddUniqueToNotNullArray( m_handlerFor, ec );
        }

        public void SubstituteHandlerFor( ExceptionClause ecOld ,
                                          ExceptionClause ecNew )
        {
            int pos = ArrayUtility.FindInNotNullArray( m_handlerFor, ecOld );

            if(pos >= 0)
            {
                m_handlerFor = ArrayUtility.ReplaceAtPositionOfNotNullArray( m_handlerFor, pos, ecNew );

                BumpVersion();
            }
        }

        //--//

        //
        // Access Methods
        //

        public ExceptionClause[] HandlerFor
        {
            get
            {
                return m_handlerFor;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToShortString()
        {
            return ToShortStringInner( "ExceptionBasicBlock" );
        }
    }
}
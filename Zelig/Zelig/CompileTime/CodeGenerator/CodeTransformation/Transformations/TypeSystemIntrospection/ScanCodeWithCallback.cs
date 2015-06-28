//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class ScanCodeWithCallback : ScanCode
    {
        internal enum CallbackResult
        {
            Proceed           ,
            SkipToNextOperator,
            Stop              ,
        }

        internal delegate CallbackResult Callback( Operator op, object target );

        //
        // State
        //

        private Callback m_callback;
        private Operator m_operatorToSkip;


        //
        // Constructor Methods
        //

        private ScanCodeWithCallback( TypeSystemForCodeTransformation typeSystem     ,
                                      object                          scanOriginator ,
                                      Callback                        callback       ) : base( typeSystem, scanOriginator )
        {
            m_callback = callback;
        }

        //
        // Helper Methods
        //

        internal static bool Execute( TypeSystemForCodeTransformation typeSystem     ,
                                      object                          scanOriginator ,
                                      ControlFlowGraphState           cfg            ,
                                      Callback                        callback       )
        {
            ScanCodeWithCallback csc = new ScanCodeWithCallback( typeSystem, scanOriginator, callback );

            return csc.ProcessMethod( cfg );
        }

        //--//

        public override void Push( object ctx )
        {
            base.Push( ctx );
        }

        public override void Pop()
        {
            object obj = TopContext();

            base.Pop();

            if(m_operatorToSkip != null && obj == m_operatorToSkip)
            {
                if(TopContext() != obj) // Deal with recursion.
                {
                    m_operatorToSkip = null;
                }
            }
        }

        protected override bool PerformAction( Operator op     ,
                                               object   target )
        {
            if(m_operatorToSkip != null && op == m_operatorToSkip)
            {
                return true;
            }

            CallbackResult result = m_callback( op, target );

            if(result == CallbackResult.SkipToNextOperator && m_operatorToSkip == null)
            {
                m_operatorToSkip = op;
            }

            return (result != CallbackResult.Stop);
        }
    }
}

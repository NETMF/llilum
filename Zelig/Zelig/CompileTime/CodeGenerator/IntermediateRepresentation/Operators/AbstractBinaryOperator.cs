//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class AbstractBinaryOperator : Operator
    {
        public enum ALU
        {
            ADD = 0,
            SUB = 1,
            MUL = 2,
            DIV = 3,
            REM = 4,
            AND = 5,
            OR  = 6,
            XOR = 7,
            SHL = 8,
            SHR = 9,
        }

        //
        // State
        //

        protected ALU  m_alu;
        protected bool m_fSigned;
        protected bool m_fOverflow;

        //
        // Constructor Methods
        //

        protected AbstractBinaryOperator( Debugging.DebugInfo  debugInfo    ,
                                          OperatorCapabilities capabilities ,
                                          OperatorLevel        level        ,
                                          ALU                  alu          ,
                                          bool                 fSigned      ,
                                          bool                 fOverflow    ) : base( debugInfo, capabilities, level )
        {
            m_alu       = alu;
            m_fSigned   = fSigned;
            m_fOverflow = fOverflow;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_alu       );
            context.Transform( ref m_fSigned   );
            context.Transform( ref m_fOverflow );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public ALU Alu
        {
            get
            {
                return m_alu;
            }
        }

        public bool Signed
        {
            get
            {
                return m_fSigned;
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
            base.InnerToString( sb );

            sb.AppendFormat( " ALU: {0}"     , m_alu       );
            sb.AppendFormat( " Signed: {0}"  , m_fSigned   );
            sb.AppendFormat( " Overflow: {0}", m_fOverflow );
        }
    }
}
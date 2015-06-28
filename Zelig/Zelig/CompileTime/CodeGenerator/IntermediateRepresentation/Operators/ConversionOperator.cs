//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class ConversionOperator : Operator
    {
        //
        // State
        //

        private uint m_significantSize;
        private bool m_fOverflow;

        //
        // Constructor Methods
        //

        protected ConversionOperator( Debugging.DebugInfo  debugInfo       ,
                                      OperatorCapabilities capabilities    ,
                                      OperatorLevel        level           ,
                                      uint                 significantSize ,
                                      bool                 fOverflow       ) : base( debugInfo, capabilities, level )
        {
            m_significantSize = significantSize;
            m_fOverflow       = fOverflow;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_significantSize );
            context.Transform( ref m_fOverflow       );

            context.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            return exOld.Type == exNew.Type;
        }

        //--//

        //
        // Access Methods
        //

        public uint SignificantSize
        {
            get
            {
                return m_significantSize;
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

            sb.AppendFormat( " SignificantSize: {0}", m_significantSize );

            sb.AppendFormat( " Overflow: {0}", m_fOverflow );
        }
    }
}
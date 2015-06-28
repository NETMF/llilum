//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public abstract class VectorHack_Base : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // State
        //

        private int m_size;

        //
        // Constructor Methods
        //

        protected VectorHack_Base( Debugging.DebugInfo debugInfo ,
                                   int                 size      ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_size = size;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_size );

            context2.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            return false;
        }

        //
        // Access Methods
        //

        public override bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return true;
            }
        }

        public int Size
        {
            get
            {
                return m_size;
            }
        }

        //
        // Debug Methods
        //

    }
}
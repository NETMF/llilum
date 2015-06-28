//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;
    using Microsoft.Zelig.CodeGeneration.IR;


    internal sealed class ArmCodeRelocation_ExternMethod : ZeligIR.ImageBuilders.CodeRelocation
    {
        //
        // State
        //

        bool m_fConditional;
        ExternalCallOperator.IExternalCallContext m_context;

        //
        // Constructor Methods
        //

        private ArmCodeRelocation_ExternMethod() // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmCodeRelocation_ExternMethod( ZeligIR.ImageBuilders.SequentialRegion region ,
                                           uint                                     offset       ,
                                           ZeligIR.BasicBlock                       target       ,
                                           ZeligIR.ImageBuilders.CompilationState   cs           ,
                                           ZeligIR.Operator                         origin       ,
                                           int                                      skew         ,
                                           bool                                     fConditional ,
                                           ExternalCallOperator.IExternalCallContext context      ) : base( region, offset, sizeof(uint), target, cs, origin, skew )
        {
            m_fConditional = fConditional;
            m_context = context;
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( ZeligIR.TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_fConditional );

            context.Pop();
        }

        //--//

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            lowerBound = m_skew + (int.MinValue >> 8) * sizeof(uint);
            upperBound = m_skew + (int.MaxValue >> 8) * sizeof(uint);
        }

        protected override bool ApplyRelocation( uint srcAddress ,
                                                 uint dstAddress )
        {
            m_context.UpdateRelocation( this );

            return true;
        }

        protected override void NotifyFailedRelocation()
        {
        }

        //
        // Access Methods
        //

        public bool IsConditional
        {
            get
            {
                return m_fConditional;
            }
        }
    }
}

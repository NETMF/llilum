//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public abstract class CodeRelocation : ImageAnnotation
    {
        //
        // State
        //

        protected CompilationState m_cs;
        protected Operator         m_origin;
        protected int              m_skew;

        //
        // Constructor Methods
        //

        protected CodeRelocation() // Default constructor required by TypeSystemSerializer.
        {
        }

        protected CodeRelocation( SequentialRegion region ,
                                  uint             offset ,
                                  uint             size   ,
                                  object           target ,
                                  CompilationState cs     ,
                                  Operator         origin ,
                                  int              skew   ) : base( region, offset, size, target )
        {
            m_cs     = cs;
            m_origin = origin;
            m_skew   = skew;
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_cs     );
            context.Transform( ref m_origin );
            context.Transform( ref m_skew   );

            context.Pop();
        }

        //--//

        public override bool IsCompileTimeAnnotation
        {
            get { return true; }
        }

        public override bool ApplyRelocation()
        {
            uint dstAddress = m_region.Owner.Resolve     ( m_target );
            uint srcAddress = m_region.GetAbsoluteAddress( m_offset );

            if(CanRelocateToAddress( srcAddress, dstAddress ))
            {
                if(ApplyRelocation( srcAddress, dstAddress ))
                {
                    return true;
                }
            }

            NotifyFailedRelocation();

            return false;
        }

        protected abstract bool ApplyRelocation( uint srcAddress ,
                                                 uint dstAddress );

        protected abstract void NotifyFailedRelocation();

        //--//

        protected Exception NotImplemented()
        {
            return m_cs.NotImplemented();
        }

        //
        // Access Methods
        //

        public Operator Origin
        {
            get
            {
                return m_origin;
            }
        }

        public int Skew
        {
            get
            {
                return m_skew;
            }
        }
    }
}

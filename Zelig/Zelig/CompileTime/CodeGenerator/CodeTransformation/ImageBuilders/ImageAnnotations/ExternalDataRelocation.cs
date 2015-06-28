//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;



    public sealed class ExternalPointerRelocation : ImageAnnotation
    {
        //
        // State
        //
        private uint m_externalDataOffset;

        //
        // Constructor Methods
        //

        private ExternalPointerRelocation() // Default constructor required by TypeSystemSerializer.
        {
        }

        public ExternalPointerRelocation( SequentialRegion region,
                               uint offset,
                               uint externalDataOffset,
                               object target )
            : base( region, offset, sizeof( uint ), target )
        {
            m_externalDataOffset = externalDataOffset;
        }

        //
        // Helper Methods
        //

        //--//

        public override bool IsCompileTimeAnnotation
        {
            get { return true; }
        }

        public override void GetAllowedRelocationRange( out int lowerBound,
                                                        out int upperBound )
        {
            lowerBound = int.MinValue;
            upperBound = int.MaxValue;
        }

        public override void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            m_target = null;

            base.ApplyTransformation( context );

            context.Transform( ref m_externalDataOffset );

            context.Pop();
        }

        public override bool ApplyRelocation()
        {
            uint val = m_region.Owner.Resolve( m_target );

            if(m_target is ExternalCallOperator)
            {
                val = (uint)(val + ( (ExternalCallOperator)m_target ).Context.OperatorOffset);
            }
            else
            {
                val += m_externalDataOffset;
            }

            m_region.Write( m_offset, val );

            return true;
        }
    }

}

//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public class CodeConstant
    {
        public static readonly CodeConstant[] SharedEmptyArray = new CodeConstant[0];

        //
        // State
        //

        ImageAnnotation  m_source;
        object           m_target;
        SequentialRegion m_region;

        //
        // Constructor Methods
        //

        internal CodeConstant( object target )
        {
            m_target = target;
        }

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.Transform( ref m_source );
            context.Transform( ref m_target );
            context.Transform( ref m_region );

            context.Pop();
        }

        //
        // Access Methods
        //

        public ImageAnnotation Source
        {
            get
            {
                return m_source;
            }

            set
            {
                m_source = value;
            }
        }

        public object Target
        {
            get
            {
                return m_target;
            }
        }

        public SequentialRegion Region
        {
            get
            {
                return m_region;
            }

            set
            {
                m_region = value;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return string.Format( "Pointer to {0}", m_target );
        }
    }
}
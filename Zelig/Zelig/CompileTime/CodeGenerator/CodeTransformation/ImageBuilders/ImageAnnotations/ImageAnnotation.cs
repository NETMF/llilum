//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public abstract class ImageAnnotation
    {
        //
        // State
        //

        protected SequentialRegion m_region;
        protected uint             m_offset;
        protected uint             m_size;
        protected object           m_target;

        //
        // Constructor Methods
        //

        protected ImageAnnotation() // Default constructor required by TypeSystemSerializer.
        {
        }

        protected ImageAnnotation( SequentialRegion region ,
                                   uint             offset ,
                                   uint             size   ,
                                   object           target )
        {
            m_region = region;
            m_offset = offset;
            m_size   = size;
            m_target = target;

            m_region.AddImageAnnotation( this );
        }

        //
        // Helper Methods
        //

        public virtual void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.Transform( ref m_region );
            context.Transform( ref m_offset );
            context.Transform( ref m_size   );
            context.Transform( ref m_target );

            context.Pop();
        }

        //--//

        public bool CanRelocateToAddress( uint srcAddress ,
                                          uint dstAddress )
        {
            int diff = (int)(dstAddress - srcAddress);
            int lowerBound;
            int upperBound;

            this.GetAllowedRelocationRange( out lowerBound, out upperBound );

            return (lowerBound <= diff && diff <= upperBound);
        }

        public abstract void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound );

        public abstract bool ApplyRelocation();

        public abstract bool IsCompileTimeAnnotation { get; }

        //
        // Access Methods
        //

        public SequentialRegion Region
        {
            get
            {
                return m_region;
            }
        }

        public uint InsertionAddress
        {
            get
            {
                return m_region.BaseAddress.ToUInt32() + m_offset;
            }
        }

        public uint Offset
        {
            get
            {
                return m_offset;
            }

            set
            {
                m_offset = value;
            }
        }

        public uint Size
        {
            get
            {
                return m_size;
            }
        }

        public object Target
        {
            get
            {
                return m_target;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return string.Format( "{0} at {1}", this.GetType(), m_offset );
        }

        public bool IsScalar
        {
            get
            {
                if(m_target is BasicBlock          ||
                   m_target is SequentialRegion    ||
                   m_target is FieldRepresentation  )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal virtual void Dump( System.IO.TextWriter textWriter )
        {
            if(m_target is BasicBlock)
            {
                BasicBlock bb = (BasicBlock)m_target;

                textWriter.Write( "{0} of '{1}'", m_region.Dumper.CreateLabel( bb ), bb.Owner.Method.ToShortString() );
            }
            else if(m_target is SequentialRegion)
            {
                SequentialRegion regTarget = (SequentialRegion)m_target;

                textWriter.Write( "{0}", regTarget.Context );
            }
            else if(m_target is FieldRepresentation)
            {
                FieldRepresentation fd = (FieldRepresentation)m_target;

                textWriter.Write( "{0}", fd.ToShortString() );
            }
            else
            {
                textWriter.Write( "{0}", m_target );
            }
        }
    }
}

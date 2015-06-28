//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class BlockCopyPropagationAnnotation : Annotation
    {
        //
        // State
        //

        private int  m_index;
        private bool m_fIsResult;
        private bool m_fIsTemporary;

        //
        // Constructor Methods
        //

        private BlockCopyPropagationAnnotation( int  index        ,
                                                bool fIsResult    ,
                                                bool fIsTemporary )
        {
            m_index        = index;
            m_fIsResult    = fIsResult;
            m_fIsTemporary = fIsTemporary;
        }

        public static BlockCopyPropagationAnnotation Create( TypeSystemForIR ts           ,
                                                             int             index        ,
                                                             bool            fIsResult    ,
                                                             bool            fIsTemporary )
        {
            return (BlockCopyPropagationAnnotation)MakeUnique( ts, new BlockCopyPropagationAnnotation( index, fIsResult, fIsTemporary ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is BlockCopyPropagationAnnotation)
            {
                var other = (BlockCopyPropagationAnnotation)obj;

                if(m_index        == other.m_index        &&
                   m_fIsResult    == other.m_fIsResult    &&
                   m_fIsTemporary == other.m_fIsTemporary  )
                {
                    return true;
                }

            }

            return false;
        }

        public override int GetHashCode()
        {
            return 0x7D112541 ^ m_index;
        }

        //
        // Helper Methods
        //

        public override Annotation Clone( CloningContext context )
        {
            return this; // Nothing to change.
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );
            
            base.ApplyTransformation( context );

            context.Transform( ref m_index        );
            context.Transform( ref m_fIsResult    );
            context.Transform( ref m_fIsTemporary );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public int Index
        {
            get
            {
                return m_index;
            }
        }

        public bool IsResult
        {
            get
            {
                return m_fIsResult;
            }
        }

        public bool IsTemporary
        {
            get
            {
                return m_fIsTemporary;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<BlockCopyPropagationAnnotation: {0}({1}){2}>", m_fIsResult ? "LHS" : "RHS", m_index, m_fIsTemporary ? " Temporary" : "" );
        }
    }
}
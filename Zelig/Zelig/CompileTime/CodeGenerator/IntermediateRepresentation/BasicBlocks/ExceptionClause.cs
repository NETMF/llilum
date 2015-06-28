//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class ExceptionClause
    {
        public static readonly ExceptionClause[] SharedEmptyArray = new ExceptionClause[0];

        //
        // Internal Classes
        //

        public enum ExceptionFlag
        {
            None    = 0x0000,
            Filter  = 0x0001,
            Finally = 0x0002,
            Fault   = 0x0004,
        }

        //
        // State
        //

        private ExceptionFlag      m_flags;
        private TypeRepresentation m_classObject;

        //
        // Constructor Methods
        //

        public ExceptionClause( ExceptionFlag      flags       ,
                                TypeRepresentation classObject )
        {
            m_flags       = flags;
            m_classObject = classObject;
        }

        //--//

        //
        // Helper Methods
        //

        public ExceptionClause Clone( CloningContext context )
        {
            TypeRepresentation classObject = context.ConvertType( m_classObject );

            ExceptionClause clone = new ExceptionClause( m_flags, classObject );

            context.Register( this, clone );

            return clone;
        }

        //--//

        public void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            context.Transform( ref m_flags       );
            context.Transform( ref m_classObject );

            context.Pop();
        }

        //
        // Access Methods
        //

        public ExceptionFlag Flags
        {
            get
            {
                return m_flags;
            }
        }

        public TypeRepresentation ClassObject
        {
            get
            {
                return m_classObject;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "EHClause(" );

            sb.Append( m_flags       );
            sb.Append( ","           );
            sb.Append( m_classObject );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class CallOperator : Operator
    {
        public enum CallKind
        {
            Direct            = 0,
            Indirect          = 1,
            Virtual           = 2,
            Overridden        = 3,
            OverriddenNoCheck = 4,
        }

        //
        // State
        //

        protected CallKind             m_callType;
        protected MethodRepresentation m_md;

        //
        // Constructor Methods
        //

        protected CallOperator( Debugging.DebugInfo  debugInfo    ,
                                OperatorCapabilities capabilities ,
                                OperatorLevel        level        ,
                                CallKind             callType     ,
                                MethodRepresentation md           ) : base( debugInfo, capabilities, level )
        {
            m_callType = callType;
            m_md       = md;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_callType );
            context.Transform( ref m_md       );

            context.Pop();
        }

        //--//

        internal static bool MatchSignature( MethodRepresentation md       ,
                                             Expression[]         rhs      ,
                                             CallKind             callType )
        {
            int offset;
            int i;

            switch(callType)
            {
                case CallKind.OverriddenNoCheck:
                    i      = md.ThisPlusArguments.Length; // Skip all, they might not match.
                    offset = 0;
                    break;

                case CallKind.Overridden:
                    i      = 1; // Skip 'this' on overridden, it won't match.
                    offset = 0;
                    break;

                case CallKind.Indirect:
                    i      = 1; // Skip the 'this' pointer, which could be of any type. Required for delegate support.
                    offset = 1; // Skip first rhs value, it's the method pointer.
                    break;

                default:
                    i      = 0;
                    offset = 0;
                    break;
            }

            if(md.ThisPlusArguments.Length + offset != rhs.Length)
            {
                return false;
            }

            for(; i < md.ThisPlusArguments.Length; i++)
            {
                TypeRepresentation td = md.ThisPlusArguments[i];
                Expression         ex = rhs[i+offset];

                if(!td.CanBeAssignedFrom( ex.Type, null ))
                {
                    // Allow passing null object pointers as any type. REVIEW: This should be unnecessary, but there
                    // still exist places in the system that fail to coerce null pointers to the correct type. This test
                    // should be considered a short term patch while we sort out those few remaining cases.
                    var constEx = ex as ConstantExpression;
                    if( (constEx != null) &&
                        (constEx.Value == null) &&
                        !(td is ValueTypeRepresentation) )
                    {
                        continue;
                    }

                    // Ignore scalar type incompatibilities. The eval stack extends scalars to 32 bits, and we'll
                    // convert these to the correct type during a later phase anyway.
                    if((td is ScalarTypeRepresentation) && (ex.Type is ScalarTypeRepresentation))
                    {
                        continue;
                    }

                    return false;
                }
            }

            return true;
        }

        //--//

        //
        // Access Methods
        //

        public CallKind CallType
        {
            get
            {
                return m_callType;
            }
        }

        public MethodRepresentation TargetMethod
        {
            get
            {
                return m_md;
            }

            set
            {
                CHECKS.ASSERT( m_md.MatchSignature( value, null ) && value.MatchSignature( m_md, null ), "Cannot swap incompatible methods '{0}' and '{1}'", m_md, value );

                m_md = value;
            }
        }

        //--//

        protected static bool HasPointerArguments( Expression[] rhs    ,
                                                   int          offset )
        {
            for(; offset < rhs.Length; offset++)
            {
                Expression ex = rhs[offset];

                if(ex.Type is PointerTypeRepresentation)
                {
                    return true;
                }
            }

            return false;
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            base.InnerToString( sb );

            sb.AppendFormat( " {0}", m_callType );
            sb.AppendFormat( " {0}", m_md       );
        }
    }
}
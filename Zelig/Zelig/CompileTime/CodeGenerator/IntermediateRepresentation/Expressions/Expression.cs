//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


#if DEBUG
#define TRACK_EXPRESSION_IDENTITY
//#define TRACK_EXPRESSION_IDENTITY_DUMP
#else
//#define TRACK_EXPRESSION_IDENTITY
//#define TRACK_EXPRESSION_IDENTITY_DUMP
#endif

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public enum CanBeNull
    {
        Unknown,
        No     ,
        Yes    ,
    }

    public abstract class Expression    
    {
        public static readonly Expression[] SharedEmptyArray = new Expression[0];

        //
        // State
        //

#if TRACK_EXPRESSION_IDENTITY
        protected static int         s_identity;
#endif
        public           int         m_identity;

        //--//

        protected TypeRepresentation m_type;
        protected int                m_index;

        //
        // Constructor Methods
        //

        protected Expression() // Default constructor required by TypeSystemSerializer.
        {
#if TRACK_EXPRESSION_IDENTITY
            m_identity = s_identity++;
#endif

            m_index = -1;
        }

        protected Expression( TypeRepresentation type ) : this()
        {
            m_type  = type;
        }

        //
        // Equality Methods
        //

        public virtual bool IsTheSamePhysicalEntity( Expression ex )
        {
            return Object.ReferenceEquals( this, ex );
        }

        public virtual bool IsTheSameAggregate( Expression ex )
        {
            return Object.ReferenceEquals( this, ex );
        }

        //--//

        //
        // Helper Methods
        //

        public abstract Expression Clone( CloningContext context );

        protected Expression RegisterAndCloneState( CloningContext context ,
                                                    Expression     clone   )
        {
            context.Register( this, clone );

            CloneState( context, clone );

            return clone;
        }

        protected virtual void CloneState( CloningContext context ,
                                           Expression     clone   )
        {
        }

        //--//

        public virtual void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            context.Transform( ref m_type );

            context.Pop();
        }

        //--//

        public virtual Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            if(m_type.ValidLayout == false)
            {
                return Operator.OperatorLevel.ConcreteTypes;
            }

            return Operator.OperatorLevel.ConcreteTypes_NoExceptions;
        }

        //--//

        public bool IsEqualToZero()
        {
            var exConst = this as ConstantExpression;
            if(exConst != null)
            {
                if(exConst.Value == null)
                {
                    return true;
                }

                ulong val;

                if(exConst.GetAsRawUlong( out val ) && val == 0)
                {
                    return true;
                }
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public TypeRepresentation Type
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_type;
            }
        }

        public int SpanningTreeIndex
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_index;
            }

            set
            {
                m_index = value;
            }
        }

        //--//

        public StackEquivalentType StackEquivalentType
        {
            get
            {
                return m_type.StackEquivalentType;
            }
        }

        public abstract CanBeNull CanBeNull
        {
            get;
        }

        public abstract bool CanTakeAddress
        {
            get;
        }

        public bool CanPointToMemory
        {
            get
            {
                return m_type.CanPointToMemory;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            InnerToString( sb );

            return sb.ToString();
        }

        public abstract void InnerToString( System.Text.StringBuilder sb );

        protected void AppendIdentity( System.Text.StringBuilder sb )
        {
#if TRACK_EXPRESSION_IDENTITY_DUMP
            sb.AppendFormat( "{{ID:{0}}}", m_identity );
#endif
        }
    }
}
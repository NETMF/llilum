//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class RegisterCouplingConstraintAnnotation : Annotation
    {
        //
        // State
        //

        private int  m_varIndex1;
        private bool m_fIsResult1;

        private int  m_varIndex2;
        private bool m_fIsResult2;
 
        //
        // Constructor Methods
        //

        private RegisterCouplingConstraintAnnotation( int  index1     ,
                                                      bool fIsResult1 ,
                                                      int  index2     ,
                                                      bool fIsResult2 )
        {
            m_varIndex1   = index1;
            m_fIsResult1  = fIsResult1;
            m_varIndex2   = index2;
            m_fIsResult2  = fIsResult2;
        }

        public static RegisterCouplingConstraintAnnotation Create( TypeSystemForIR ts         ,
                                                                   int             index1     ,
                                                                   bool            fIsResult1 ,
                                                                   int             index2     ,
                                                                   bool            fIsResult2 )
        {
            return (RegisterCouplingConstraintAnnotation)MakeUnique( ts, new RegisterCouplingConstraintAnnotation( index1, fIsResult1, index2, fIsResult2 ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is RegisterCouplingConstraintAnnotation)
            {
                var other = (RegisterCouplingConstraintAnnotation)obj;

                if(m_varIndex1  == other.m_varIndex1  &&
                   m_fIsResult1 == other.m_fIsResult1 &&
                   m_varIndex2  == other.m_varIndex2  &&
                   m_fIsResult2 == other.m_fIsResult2  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 0x0E47C2CA ^ m_varIndex1 ^ (m_varIndex2 << 10);
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
            var context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );
            
            base.ApplyTransformation( context2 );

            context2.Transform( ref m_varIndex1  );
            context2.Transform( ref m_fIsResult1 );
            context2.Transform( ref m_varIndex2  );
            context2.Transform( ref m_fIsResult2 );

            context2.Pop();
        }

        //--//

        public void ExtractTargets(     Operator           op   ,
                                    out VariableExpression var1 ,
                                    out VariableExpression var2 )
        {
            var1 = m_fIsResult1 ? op.Results[m_varIndex1] : (op.Arguments[m_varIndex1] as VariableExpression);
            var2 = m_fIsResult2 ? op.Results[m_varIndex2] : (op.Arguments[m_varIndex2] as VariableExpression);
        }

        public VariableExpression FindCoupledExpression( Operator           op ,
                                                         VariableExpression ex )
        {
            VariableExpression var1;
            VariableExpression var2;

            ExtractTargets( op, out var1, out var2 );

            if(var1 == ex) return var2;
            if(var2 == ex) return var1;

            return null;
        }

        //--//

        //
        // Access Methods
        //

        public int VarIndex1
        {
            get
            {
                return m_varIndex1;
            }
        }

        public bool IsResult1
        {
            get
            {
                return m_fIsResult1;
            }
        }

        public int VarIndex2
        {
            get
            {
                return m_varIndex2;
            }
        }

        public bool IsResult2
        {
            get
            {
                return m_fIsResult2;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<RegisterCouplingConstraintAnnotation: {0}({1}) <=> {2}({3})>", m_fIsResult1 ? "LHS" : "RHS", m_varIndex1, m_fIsResult2 ? "LHS" : "RHS", m_varIndex2 );
        }
    }
}
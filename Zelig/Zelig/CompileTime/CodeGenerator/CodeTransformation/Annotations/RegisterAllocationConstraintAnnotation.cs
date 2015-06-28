//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class RegisterAllocationConstraintAnnotation : Annotation
    {
        //
        // State
        //

        private int                        m_varIndex;
        private bool                       m_fIsResult;
        private Abstractions.RegisterClass m_constraint;

        //
        // Constructor Methods
        //

        private RegisterAllocationConstraintAnnotation( int                        index      ,
                                                        bool                       fIsResult  ,
                                                        Abstractions.RegisterClass constraint )
        {
            m_varIndex   = index;
            m_fIsResult  = fIsResult;
            m_constraint = constraint;
        }

        public static RegisterAllocationConstraintAnnotation Create( TypeSystemForIR            ts         ,
                                                                     int                        index      ,
                                                                     bool                       fIsResult  ,
                                                                     Abstractions.RegisterClass constraint )
        {
            return (RegisterAllocationConstraintAnnotation)MakeUnique( ts, new RegisterAllocationConstraintAnnotation( index, fIsResult, constraint ) );
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is RegisterAllocationConstraintAnnotation)
            {
                RegisterAllocationConstraintAnnotation other = (RegisterAllocationConstraintAnnotation)obj;

                if(m_varIndex   == other.m_varIndex   &&
                   m_fIsResult  == other.m_fIsResult  &&
                   m_constraint == other.m_constraint  )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 0x0F88DAC4 ^ m_varIndex;
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

            context2.Transform( ref m_varIndex   );
            context2.Transform( ref m_fIsResult  );
            context2.Transform( ref m_constraint );

            context2.Pop();
        }

        //--//

        public static bool ShouldLhsBeMovedToPseudoRegister( Operator op    ,
                                                             int      index )
        {
            if(VariableExpression.ExtractAliased( op.Results[index] ) is StackLocationExpression)
            {
                foreach(var an in op.FilterAnnotations< RegisterAllocationConstraintAnnotation >())
                {
                    if(an.IsResult == true && an.VarIndex == index)
                    {
                        if((an.Constraint & ~Abstractions.RegisterClass.Address) != Abstractions.RegisterClass.None)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool ShouldRhsBeMovedToPseudoRegister( Operator op    ,
                                                             int      index )
        {
            if(VariableExpression.ExtractAliased( op.Arguments[index] ) is StackLocationExpression)
            {
                foreach(var an in op.FilterAnnotations< RegisterAllocationConstraintAnnotation >())
                {
                    if(an.IsResult == false && an.VarIndex == index)
                    {
                        if((an.Constraint & ~Abstractions.RegisterClass.Address) != Abstractions.RegisterClass.None)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //--//

        public static Abstractions.RegisterClass ComputeConstraintsForLHS( Operator           op ,
                                                                           VariableExpression ex )
        {
            Abstractions.RegisterClass constraint = Abstractions.RegisterClass.None;

            if(op != null)
            {
                foreach(var an in op.FilterAnnotations< RegisterAllocationConstraintAnnotation >())
                {
                    if(an.IsResult == true && op.Results[an.VarIndex] == ex)
                    {
                        constraint |= an.Constraint;
                    }
                }
            }

            return constraint;
        }

        public static Abstractions.RegisterClass ComputeConstraintsForRHS( Operator   op ,
                                                                           Expression ex )
        {
            Abstractions.RegisterClass constraint = Abstractions.RegisterClass.None;

            if(op != null)
            {
                foreach(var an in op.FilterAnnotations< RegisterAllocationConstraintAnnotation >())
                {
                    if(an.IsResult == false && op.Arguments[an.VarIndex] == ex)
                    {
                        constraint |= an.Constraint;
                    }
                }
            }

            return constraint;
        }

        public static Abstractions.RegisterClass ComputeConstraintsForLHS( Operator op       ,
                                                                           int      lhsIndex )
        {
            Abstractions.RegisterClass constraint = Abstractions.RegisterClass.None;

            if(op != null)
            {
                foreach(var an in op.FilterAnnotations< RegisterAllocationConstraintAnnotation >())
                {
                    if(an.IsResult == true && an.VarIndex == lhsIndex)
                    {
                        constraint |= an.Constraint;
                    }
                }
            }

            return constraint;
        }
        
        public static Abstractions.RegisterClass ComputeConstraintsForRHS( Operator op       ,
                                                                           int      rhsIndex )
        {
            Abstractions.RegisterClass constraint = Abstractions.RegisterClass.None;

            if(op != null)
            {
                foreach(var an in op.FilterAnnotations< RegisterAllocationConstraintAnnotation >())
                {
                    if(an.IsResult == false && an.VarIndex == rhsIndex)
                    {
                        constraint |= an.Constraint;
                    }
                }
            }

            return constraint;
        }

        //--//

        //
        // Access Methods
        //

        public int VarIndex
        {
            get
            {
                return m_varIndex;
            }
        }

        public bool IsResult
        {
            get
            {
                return m_fIsResult;
            }
        }

        public Abstractions.RegisterClass Constraint
        {
            get
            {
                return m_constraint;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<RegisterAllocationConstraintAnnotation: {0}({1}) = {2}>", m_fIsResult ? "LHS" : "RHS", m_varIndex , m_constraint );
        }
    }
}
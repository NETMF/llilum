//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


#if DEBUG
#define TRACK_OPERATOR_IDENTITY
#else
//#define TRACK_OPERATOR_IDENTITY
#endif

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public abstract class Operator
    {
        [Flags]
        public enum SubstitutionFlags
        {
            Default         = 0x00000001,
            CopyAnnotations = 0x00000001,
        }

        //
        // It's important to notice that both the positive and negative cases are explicitly expressed.
        //
        // This acts as a sort of parity-checking on the definition of an operator's capabilities,
        // ensuring that whenever a new capability or a new operator is added, all the bases are covered.
        //
        [Flags]
        public enum OperatorCapabilities
        {
            IsCommutative                      = 0x00000001,
            IsNonCommutative                   = 0x00000002,

            MayMutateExistingStorage           = 0x00000004,
            DoesNotMutateExistingStorage       = 0x00000008,

            MayAllocateStorage                 = 0x00000010,
            DoesNotAllocateStorage             = 0x00000020,

            MayReadExistingMutableStorage      = 0x00000040,
            DoesNotReadExistingMutableStorage  = 0x00000080,

            MayThrow                           = 0x00000100,
            DoesNotThrow                       = 0x00000200,

            MayReadThroughPointerOperands      = 0x00000400,
            DoesNotReadThroughPointerOperands  = 0x00000800,

            MayWriteThroughPointerOperands     = 0x00001000,
            DoesNotWriteThroughPointerOperands = 0x00002000,

            MayCapturePointerOperands          = 0x00004000,
            DoesNotCapturePointerOperands      = 0x00008000,

            IsMetaOperator                     = 0x00010000,

            MutuallyExclusive                  = IsCommutative                      |
                                                 MayMutateExistingStorage           |
                                                 MayAllocateStorage                 |
                                                 MayReadExistingMutableStorage      |
                                                 MayThrow                           |
                                                 MayReadThroughPointerOperands      |
                                                 MayWriteThroughPointerOperands     |
                                                 MayCapturePointerOperands          ,
        }

        public enum OperatorLevel
        {
            Registers                 , // Instead of using Variables, use only Registers.
            StackLocations            , // Instead of using Variables, use only Registers and StackLocations.
            ScalarValues              , // All the variables have been split into word-sized fragments that could fit in a machine register.
            ConcreteTypes_NoExceptions, // All implicit exception checks are converted into explict ones.
            ConcreteTypes             , // Nothing but types and concrete (non-virtual) method calls.
            ObjectOriented            , // Static fields, allocations, casts have been removed.
            FullyObjectOriented       , // Everything coming directly from byte code.
    
            Lowest  = Registers          ,
            Highest = FullyObjectOriented,
        }

        public interface IOperatorLevelHelper
        {
            bool FitsInPhysicalRegister( TypeRepresentation td );
        }

        //--//

        public static readonly Operator[]   SharedEmptyArray      = new Operator[0];
        public static readonly Operator[][] SharedEmptyArrayArray = new Operator[0][];

        //
        // State
        //

#if TRACK_OPERATOR_IDENTITY
        protected static int           s_identity;
#endif
        public           int           m_identity;

        //--//

        protected Debugging.DebugInfo  m_debugInfo;
        protected OperatorCapabilities m_capabilities;
        protected OperatorLevel        m_level;

        protected int                  m_index;
        protected BasicBlock           m_basicBlock;
        private   VariableExpression[] m_lhs;
        private   Expression[]         m_rhs;
        protected Annotation[]         m_annotations;

        //
        // Constructor Methods
        //

        protected Operator() // Default constructor required by TypeSystemSerializer.
        {
#if TRACK_OPERATOR_IDENTITY
            m_identity = s_identity++;
#endif

            ClearState();
        }

        protected Operator( Debugging.DebugInfo  debugInfo    ,
                            OperatorCapabilities capabilities ,
                            OperatorLevel        level        ) : this()
        {
            if(CHECKS.ENABLED) // Avoid expensive boxing of "capabilities" below.
            {
                uint oddMask  =  (uint)capabilities       & (uint)OperatorCapabilities.MutuallyExclusive;
                uint evenMask = ((uint)capabilities >> 1) & (uint)OperatorCapabilities.MutuallyExclusive;

                if((oddMask ^ evenMask) != (uint)OperatorCapabilities.MutuallyExclusive ||
                   (oddMask & evenMask) != 0x00000                                       )
                {
                    CHECKS.ASSERT( false, "Capabilities are not paired correctly: {0}", capabilities );
                }
            }

            m_debugInfo    = debugInfo;
            m_capabilities = capabilities;
            m_level        = level;
        }

        //--//

        //
        // Helper Methods
        //

        public abstract Operator Clone( CloningContext context );

        protected Operator RegisterAndCloneState( CloningContext context ,
                                                  Operator       clone   )
        {
            context.Register( this, clone );

            CloneState( context, clone );

            return clone;
        }

        protected virtual void CloneState( CloningContext context ,
                                           Operator       clone   )
        {
            clone.m_index       =                m_index;
            clone.m_basicBlock  = context.Clone( m_basicBlock  );
            clone.m_lhs         = context.Clone( m_lhs         );
            clone.m_rhs         = context.Clone( m_rhs         );
            clone.m_annotations = context.Clone( m_annotations );
        }

        //--//

        public OperatorLevel GetLevel( IOperatorLevelHelper helper )
        {
            OperatorLevel level = m_level;
            OperatorLevel level2;

            foreach(var ex in m_lhs)
            {
                level2 = ex.GetLevel( helper );
                if(level < level2)
                {
                    level = level2;
                }
            }

            foreach(var ex in m_rhs)
            {
                level2 = ex.GetLevel( helper );
                if(level < level2)
                {
                    level = level2;
                }
            }

            foreach(var an in m_annotations)
            {
                level2 = an.GetLevel( helper );
                if(level < level2)
                {
                    level = level2;
                }
            }

            return level;
        }

        protected void SetLhs( VariableExpression lhs )
        {
            SetLhsArray( lhs );
        }

        protected void SetLhs( VariableExpression lhs1 ,
                               VariableExpression lhs2 )
        {
            SetLhsArray( lhs1, lhs2 );
        }

        protected void SetLhs( VariableExpression lhs1 ,
                               VariableExpression lhs2 ,
                               VariableExpression lhs3 )
        {
            SetLhsArray( lhs1, lhs2, lhs3 );
        }

        protected void SetLhsArray( params VariableExpression[] lhs )
        {
            CHECKS.ASSERT( this.IsDetached, "Cannot set LHS after associating operator with basic block." );

            m_lhs = lhs;
        }

        protected void SetRhs( Expression rhs )
        {
            SetRhsArray( rhs );
        }

        protected void SetRhs( Expression rhs1 ,
                               Expression rhs2 )
        {
            SetRhsArray( rhs1, rhs2 );
        }

        protected void SetRhs( Expression rhs1 ,
                               Expression rhs2 ,
                               Expression rhs3 )
        {
            SetRhsArray( rhs1, rhs2, rhs3 );
        }

        protected void SetRhsArray( params Expression[] rhs )
        {
            CHECKS.ASSERT( this.IsDetached, "Cannot set LHS after associating operator with basic block." );

            m_rhs = rhs;
        }

        protected void BumpVersion()
        {
            m_basicBlock.BumpVersion();
        }

        //--//

        public virtual void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            context.Transform( ref m_debugInfo    );
            context.Transform( ref m_capabilities );
            context.Transform( ref m_level        );

            context.Transform( ref m_basicBlock   );
            context.Transform( ref m_lhs          );
            context.Transform( ref m_rhs          );
            context.Transform( ref m_annotations  );

            context.Pop();
        }

        internal void Retarget( BasicBlock owner )
        {
            m_basicBlock = owner;
        }

        public virtual void NotifyMerge( BasicBlock entryBB ,
                                         BasicBlock exitBB  )
        {
        }

        public virtual void NotifyNewPredecessor( BasicBlock oldBB ,
                                                  BasicBlock newBB )
        {
        }

        //--//

        public bool AddAnnotation( Annotation an )
        {
            if(ArrayUtility.FindInNotNullArray( m_annotations, an ) >= 0)
            {
                return false;
            }

            if(this.IsDetached == false)
            {
                BumpVersion();
            }

            m_annotations = ArrayUtility.AppendToNotNullArray( m_annotations, an );

            return true;
        }

        public void RemoveAnnotation( Annotation an )
        {
            if(this.IsDetached == false)
            {
                BumpVersion();
            }

            m_annotations = ArrayUtility.RemoveUniqueFromNotNullArray( m_annotations, an );
        }

        public void CopyAnnotations( Operator op )
        {
            foreach(Annotation an in op.Annotations)
            {
                AddAnnotation( an );
            }
        }

        public bool HasAnnotation< T >() where T : Annotation
        {
            return GetAnnotation< T >() != null;
        }

        public T GetAnnotation< T >() where T : Annotation
        {
            foreach(Annotation an in m_annotations)
            {
                T res = an as T;
                if(res != null)
                {
                    return res;
                }
            }

            return null;
        }

        public AnnotationEnumeratorProvider< T > FilterAnnotations<T>() where T : Annotation
        {
            return new AnnotationEnumeratorProvider< T >( m_annotations );
        }

        public struct AnnotationEnumeratorProvider< T > where T : Annotation
        {
            //
            // State
            //

            private readonly Annotation[] m_values;

            //
            // Constructor Methods
            //

            internal AnnotationEnumeratorProvider( Annotation[] values )
            {
                m_values = values;
            }

            //
            // Helper Methods
            //

            public AnnotationEnumerator< T > GetEnumerator()
            {
                return new AnnotationEnumerator< T >( m_values );
            }
        }

        public struct AnnotationEnumerator< T > where T : Annotation
        {
            //
            // State
            //

            private readonly Annotation[] m_values;
            private T                     m_current;
            private int                   m_index;

            //
            // Constructor Methods
            //

            internal AnnotationEnumerator( Annotation[] values )
            {
                m_values  = values;
                m_current = null;
                m_index   = 0;
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                while(m_index < m_values.Length)
                {
                    m_current = m_values[m_index++] as T;
                    if(m_current != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            public T Current
            {
                get
                {
                    return m_current;
                }
            }
        }
        
        //--//

        public void AddOperatorBefore( Operator op )
        {
            m_basicBlock.AddOperatorBefore( op, this );
        }

        public void AddOperatorAfter( Operator op )
        {
            m_basicBlock.AddOperatorAfter( this, op );
        }

        //--//

        public void SubstituteWithOperator( Operator          op    ,
                                            SubstitutionFlags flags )
        {
            m_basicBlock.SubstituteOperator( this, op, flags );
        }

        //--//

        public BasicBlock SubstituteWithSubGraph( BasicBlock entry ,
                                                  BasicBlock exit  )
        {
            CHECKS.ASSERT(                 entry.Annotation == BasicBlock.Qualifier.Normal, "Entry must be a normal basic block" );
            CHECKS.ASSERT( exit == null || exit .Annotation == BasicBlock.Qualifier.Normal, "Exit must be a normal basic block"  );

            BasicBlock current = this.BasicBlock;
            BasicBlock split   = current.SplitAtOperator( this, true, true );

            //
            // Redirect flow control to go through the sub graph.
            //
            /**************/ current.FlowControl = UnconditionalControlOperator.New( null, entry );
            if(exit != null) exit   .FlowControl = UnconditionalControlOperator.New( null, split );

            return (exit != null) ? split : null;
        }

        public void SubstituteAnnotation( InvalidationAnnotation anOld ,
                                          InvalidationAnnotation anNew )
        {
            CHECKS.ASSERT( anNew != null , "Cannot substitute annotation with null pointer: {0}"   , this ); 
            CHECKS.ASSERT( anOld != anNew, "Expecting different annotations for substituition: {0}", this );

            var annotations = m_annotations;

            for(int i = 0; i < annotations.Length; i++)
            {
                if(annotations[i] == anOld)
                {
                    BumpVersion();

                    if(m_annotations == annotations)
                    {
                        //
                        // In-place updates are not allowed, we have to guarantee that the values returned by 'Operator.Rhs' don't change.
                        //
                        m_annotations = ArrayUtility.CopyNotNullArray( annotations );
                    }

                    m_annotations[i] = anNew;
                }
            }
        }

        public void SubstituteDefinition( VariableExpression exOld ,
                                          VariableExpression exNew )
        {
            CHECKS.ASSERT( exNew != null , "Cannot substitute definition with null pointer: {0}"   , this ); 
            CHECKS.ASSERT( exOld != exNew, "Expecting different expressions for substituition: {0}", this );

            var lhs = m_lhs;

            for(int i = 0; i < lhs.Length; i++)
            {
                if(lhs[i] == exOld)
                {
                    BumpVersion();

                    if(m_lhs == lhs)
                    {
                        //
                        // In-place updates are not allowed, we have to guarantee that the values returned by 'Operator.Rhs' don't change.
                        //
                        m_lhs = ArrayUtility.CopyNotNullArray( lhs );
                    }

                    m_lhs[i] = exNew;
                }
            }
        }

        public void SubstituteUsage( Expression exOld ,
                                     Expression exNew )
        {
            CHECKS.ASSERT( exNew != null , "Cannot substitute usage with null pointer: {0}"        , this ); 
            CHECKS.ASSERT( exOld != exNew, "Expecting different expressions for substituition: {0}", this );

            Expression[] rhs = m_rhs;

            for(int i = 0; i < rhs.Length; i++)
            {
                if(rhs[i] == exOld)
                {
                    BumpVersion();

                    if(m_rhs == rhs)
                    {
                        //
                        // In-place updates are not allowed, we have to guarantee that the values returned by 'Operator.Rhs' don't change.
                        //
                        m_rhs = ArrayUtility.CopyNotNullArray( rhs );
                    }

                    m_rhs[i] = exNew;
                }
            }
        }

        public void SubstituteUsage( int        index ,
                                     Expression exNew )
        {
            CHECKS.ASSERT( 0 <= index && index < m_rhs.Length, "Index of usage to substitute is invalid: {0} {1}", index, this ); 

            CHECKS.ASSERT( m_rhs[index] != exNew, "Expecting different expressions for substituition: {0}", this );

            BumpVersion();

            //
            // In-place updates are not allowed, we have to guarantee that the values returned by 'Operator.Rhs' don't change.
            //
            m_rhs = ArrayUtility.CopyNotNullArray( m_rhs );

            m_rhs[index] = exNew;
        }

        public bool IsSourceOfExpression( Expression target )
        {
            foreach(var ex in m_lhs)
            {
                if(ex == target)
                {
                    return true;
                }
            }

            foreach(var an in m_annotations)
            {
                var anInv = an as InvalidationAnnotation;
                if(anInv != null && anInv.Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool CanPropagateCopy( Expression exOld ,
                                              Expression exNew )
        {
            return true;
        }

        public void MoveBefore( Operator opDst )
        {
            if(m_basicBlock != null)
            {
                m_basicBlock.RemoveOperator( this );
            }

            opDst.AddOperatorBefore( this );
        }

        public void Delete()
        {
            if(m_basicBlock != null)
            {
                m_basicBlock.RemoveOperator( this );

                CHECKS.ASSERT( this.IsDetached, "Deleting operator {0} from basic block {1} failed!", this, m_basicBlock );
            }

            ClearState();
        }

        internal void ClearState()
        {
            m_index       = -1;
            m_basicBlock  = null;
            m_lhs         = VariableExpression.SharedEmptyArray;
            m_rhs         = Expression        .SharedEmptyArray;
            m_annotations = Annotation        .SharedEmptyArray;
        }

        //--//

        public int GetBasicBlockIndex()
        {
            if(m_basicBlock != null)
            {
                return ArrayUtility.FindReferenceInNotNullArray( m_basicBlock.Operators, this );
            }

            return -1;
        }

        public Operator GetPreviousOperator()
        {
            if(m_basicBlock != null)
            {
                Operator[] ops = m_basicBlock.Operators;
                int        pos = ArrayUtility.FindReferenceInNotNullArray( ops, this );

                pos -= 1;

                if(pos >= 0)
                {
                    return ops[pos];
                }
            }

            return null;
        }

        public Operator GetNextOperator()
        {
            if(m_basicBlock != null)
            {
                Operator[] ops = m_basicBlock.Operators;
                int        pos = ArrayUtility.FindReferenceInNotNullArray( ops, this );

                pos += 1;

                if(pos < ops.Length)
                {
                    return ops[pos];
                }
            }

            return null;
        }

        //--//

        public bool SameDebugInfo( Operator op )
        {
            if(op != null)
            {
                return this.m_debugInfo == op.m_debugInfo;
            }

            return false;
        }

        //--//

        protected static ConstantExpression FindConstantOrigin( Expression                    ex         ,
                                                                Operator[][]                  defChains  ,
                                                                Operator[][]                  useChains  ,
                                                                VariableExpression.Property[] properties )
        {
            BitVector visited = null;

            while(true)
            {
                var res = ex as ConstantExpression;
                if(res != null)
                {
                    return res;
                }

                if((properties[ex.SpanningTreeIndex] & VariableExpression.Property.AddressTaken) == 0)
                {
                    Operator singleDef = ControlFlowGraphState.CheckSingleDefinition( defChains, (VariableExpression)ex );
                    if(singleDef != null)
                    {
                        if(singleDef is AbstractAssignmentOperator && singleDef.Arguments.Length == 1)
                        {
                            if(visited == null)
                            {
                                visited = new BitVector();
                            }

                            if(visited.Set( singleDef.SpanningTreeIndex ) == false)
                            {
                                //
                                // Detected loop.
                                //
                                return null;
                            }

                            ex = singleDef.FirstArgument;
                            continue;
                        }
                    }
                }

                return null;
            }
        }

        public virtual bool Simplify( Operator[][]                  defChains  ,
                                      Operator[][]                  useChains  ,
                                      VariableExpression.Property[] properties )
        {
            if(this.Results.Length == 1)
            {
                ConstantExpression exConst = this.CanEvaluate( defChains, useChains, properties );
                if(exConst != null)
                {
                    var opNew = SingleAssignmentOperator.New( this.DebugInfo, this.FirstResult, exConst );

                    this.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                    return true;
                }
            }

            return false;
        }

        public virtual ConstantExpression CanEvaluate( Operator[][]                  defChains  ,
                                                       Operator[][]                  useChains  ,
                                                       VariableExpression.Property[] properties )
        {
            return null;
        }

        public void EnsureConstantToTheRight()
        {
            if(this.IsCommutative)
            {
                Expression[] rhs = m_rhs;

                if(rhs.Length == 2)
                {
                    Expression left  = rhs[0];
                    Expression right = rhs[1];

                    if( (left  is ConstantExpression) &&
                       !(right is ConstantExpression)  )
                    {
                        m_rhs = new Expression[2];

                        m_rhs[0] = right;
                        m_rhs[1] = left;
                    }
                }
            }
        }

        public Expression IsBinaryOperationAgainstConstant( out ConstantExpression exConst       ,
                                                            out bool               fConstOnRight )
        {
            Expression[] rhs = m_rhs;

            if(rhs.Length == 2)
            {
                Expression         left  = rhs[0];
                Expression         right = rhs[1];
                ConstantExpression ex;

                ex = left as ConstantExpression;
                if(ex != null)
                {
                    exConst       = ex;
                    fConstOnRight = false;

                    return right;
                }

                ex = right as ConstantExpression;
                if(ex != null)
                {
                    exConst       = ex;
                    fConstOnRight = true;

                    return left;
                }
            }

            exConst       = null;
            fConstOnRight = false;

            return null;
        }

        public Expression IsBinaryOperationAgainstZeroValue( out bool fNullOnRight )
        {
            Expression[] rhs = m_rhs;

            if(rhs.Length == 2)
            {
                if(rhs[0].IsEqualToZero())
                {
                    fNullOnRight = false;

                    return rhs[1];
                }

                if(rhs[1].IsEqualToZero())
                {
                    fNullOnRight = true;

                    return rhs[0];
                }
            }

            fNullOnRight = false;

            return null;
        }

        public bool IsAddOrSubAgainstConstant( out VariableExpression var      ,
                                               out long               constVal )
        {
            var opBin = this as BinaryOperator;
            if(opBin != null)
            {
                ConstantExpression exConst;
                bool               fConstOnRight;

                var = opBin.IsBinaryOperationAgainstConstant( out exConst, out fConstOnRight ) as VariableExpression;
                if(var != null)
                {
                    if(exConst.GetAsSignedInteger( out constVal ))
                    {
                        switch(opBin.Alu)
                        {
                            case AbstractBinaryOperator.ALU.ADD:
                                return true;

                            case AbstractBinaryOperator.ALU.SUB:
                                if(fConstOnRight)
                                {
                                    constVal = -constVal;
                                    return true;
                                }
                                break;
                        }                         
                    }
                }
            }

            var      = null;
            constVal = 0;

            return false;
        }

        public bool IsDominatedBy( Operator    opTarget  ,
                                   BitVector[] dominance )
        {
            var bbTarget = opTarget.BasicBlock;
            var bbSource = this    .BasicBlock;

            if(bbSource.IsDominatedBy( bbTarget, dominance ))
            {
                if(bbTarget != bbSource)
                {
                    return true;
                }
                
                return opTarget.SpanningTreeIndex < this.SpanningTreeIndex;
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public Debugging.DebugInfo DebugInfo
        {
            get
            {
                return m_debugInfo;
            }
        }

        public OperatorCapabilities Capabilities
        {
            get
            {
                return m_capabilities;
            }
        }

        public BasicBlock BasicBlock
        {
            get
            {
                return m_basicBlock;
            }

            internal set
            {
                m_basicBlock = value;
            }
        }

        public int SpanningTreeIndex
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_index;
            }

            [System.Diagnostics.DebuggerHidden]
            set
            {
                m_index = value;
            }
        }

        public VariableExpression[] Results
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_lhs;
            }
        }

        public VariableExpression FirstResult
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_lhs[0];
            }
        }

        public VariableExpression SecondResult
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_lhs[1];
            }
        }

        public VariableExpression ThirdResult
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_lhs[2];
            }
        }

        public Expression[] Arguments
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_rhs;
            }

            [System.Diagnostics.DebuggerHidden]
            protected set
            {
                m_rhs = value;
            }
        }

        public Expression FirstArgument
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_rhs[0];
            }
        }

        public Expression SecondArgument
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_rhs[1];
            }
        }

        public Expression ThirdArgument
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_rhs[2];
            }
        }

        public Expression FourthArgument
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_rhs[3];
            }
        }

        public virtual bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return false;
            }
        }

        public Annotation[] Annotations
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_annotations;
            }
        }

        public bool IsDetached
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return m_basicBlock == null;
            }
        }

        //--//

        public bool IsCommutative
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.IsCommutative) != 0;
            }
        }

        public bool MayMutateExistingStorage
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayMutateExistingStorage) != 0;
            }
        }

        public bool MayAllocateStorage
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayAllocateStorage) != 0;
            }
        }

        public bool MayReadExistingMutableStorage
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayReadExistingMutableStorage) != 0;
            }
        }

        public bool MayThrow
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayThrow) != 0;
            }
        }

        public bool MayReadThroughPointerOperands
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayReadThroughPointerOperands) != 0;
            }
        }

        public bool MayWriteThroughPointerOperands
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayWriteThroughPointerOperands) != 0;
            }
        }

        public bool MayCapturePointerOperands
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.MayCapturePointerOperands) != 0;
            }
        }

        public bool IsMetaOperator
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return (m_capabilities & OperatorCapabilities.IsMetaOperator) != 0;
            }
        }

        public virtual bool PerformsNoActions
        {
            get
            {
                return false;
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

        public virtual void InnerToString( System.Text.StringBuilder sb )
        {
            if(m_lhs.Length > 0)
            {
                sb.Append( " Res: (" );

                for(int i = 0; i < m_lhs.Length; i++)
                {
                    if(i != 0) sb.Append( ", " );

                    sb.Append( m_lhs[i] );
                }

                sb.Append( ")" );
            }

            if(m_rhs.Length > 0)
            {
                sb.Append( " Arg: (" );

                for(int i = 0; i < m_rhs.Length; i++)
                {
                    if(i != 0) sb.Append( ", " );

                    sb.Append( m_rhs[i] );
                }

                sb.Append( ")" );
            }

            if(this.MayThrow)
            {
                sb.Append( " MayThrow" );
            }
        }

        public abstract string FormatOutput( IIntermediateRepresentationDumper dumper );

        public string ToPrettyString()
        {
            return m_basicBlock.Owner.ToPrettyString( this );
        }
    }
}
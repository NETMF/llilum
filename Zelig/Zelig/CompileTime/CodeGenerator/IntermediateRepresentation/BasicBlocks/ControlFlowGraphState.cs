//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define CACHEINFO_ENABLE_TRACKING_THREADS
//#define CACHEINFO_DEBUG_THREADING_ISSUES


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class ControlFlowGraphState
    {
        public static CompilationConstraints[] SharedEmptyCompilationConstraintsArray = new CompilationConstraints[0];

        public abstract class CachedInfo : IDisposable
        {
            public static CachedInfo[] SharedEmptyArray = new CachedInfo[0];

            //
            // State
            //

            protected ControlFlowGraphState         m_owner;
            private   int                           m_version;
            private   int                           m_lockCount;

#if CACHEINFO_ENABLE_TRACKING_THREADS
            private   System.Threading.Thread       m_lockOwner;
#if CACHEINFO_DEBUG_THREADING_ISSUES
            protected System.Diagnostics.StackTrace m_lockTrace;
#endif
#endif

            //
            // Constructor Methods
            //

            protected CachedInfo()
            {
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
                Unlock();
            }

            public void Lock()
            {
#if CACHEINFO_ENABLE_TRACKING_THREADS
                System.Threading.Thread thisThread   = System.Threading.Thread.CurrentThread;
                System.Threading.Thread activeThread = System.Threading.Interlocked.CompareExchange( ref m_lockOwner, thisThread, null );

                CHECKS.ASSERT( activeThread == null || activeThread == thisThread, "Lock on {0} for method '{1}' already claimed", this.GetType().FullName, m_owner.Method.ToShortString() );

#if CACHEINFO_DEBUG_THREADING_ISSUES
                if(m_lockCount == 0)
                {
                    m_lockTrace = new System.Diagnostics.StackTrace();
                }
#endif

#endif

                m_lockCount++;
            }

            public void Unlock()
            {
#if CACHEINFO_ENABLE_TRACKING_THREADS
                CHECKS.ASSERT( System.Threading.Thread.CurrentThread == m_lockOwner, "Lock on {0} not owned by current thread", this.GetType().FullName );
#endif

                CHECKS.ASSERT( m_lockCount > 0, "Underflow for lock on {0}", this.GetType().FullName );

                m_lockCount--;

#if CACHEINFO_ENABLE_TRACKING_THREADS
                if(m_lockCount == 0)
                {
                    m_lockOwner = null;

#if CACHEINFO_DEBUG_THREADING_ISSUES
                    m_lockTrace = null;
#endif
                }
#endif
            }

            public void RefreshIfNeeded()
            {
                ThreadLockInfo.Assert( m_owner.Method );

                if(m_version != m_owner.m_version)
                {
                    if(m_lockCount != 0)
                    {
                        throw TypeConsistencyErrorException.Create( "Detected attempt to modify state of Control Flow Graph for {0}", this.GetType().FullName );
                    }

                    Update();

                    m_version = m_owner.m_version;
                }
            }

            protected abstract void Update();

            //
            // Access Methods
            //

            internal ControlFlowGraphState Owner
            {
                set
                {
                    m_owner = value;
                }
            }
        }

        //--//

        class ThreadLockInfo : IDisposable
        {
            internal class ExceptionInfo : IDisposable
            {
                //
                // Constructor Methods
                //

                internal ExceptionInfo( MethodRepresentation md )
                {
                    s_lockException = md;
                }

                //
                // Helper Methods
                //

                public void Dispose()
                {
                    s_lockException = null;
                }
            }

            //
            // State
            //

            [ThreadStatic] private static MethodRepresentation s_lock;
            [ThreadStatic] private static MethodRepresentation s_lockException;

            //
            // Constructor Methods
            //

            internal ThreadLockInfo( MethodRepresentation md )
            {
                Assert( md );

                s_lock = md;
            }

            internal ThreadLockInfo( MethodRepresentation md  ,
                                     MethodRepresentation md2 )
            {
                Assert( md );

                s_lock          = md;
                s_lockException = md2;
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
                s_lock          = null;
                s_lockException = null;
            }

            internal static void Assert( MethodRepresentation md )
            {
                if(s_lock == null || md == s_lock || md == s_lockException)
                {
                    return;
                }

                throw TypeConsistencyErrorException.Create( "Detected attempt to access state of Control Flow Graph for '{0}' while thread is locked to access only '{1}'", md.ToShortString(), s_lock.ToShortString() );
            }
        }

        class GroupLockInfo : IDisposable
        {
            //
            // State
            //

            private readonly IDisposable[] m_locks;

            //
            // Constructor Methods
            //

            internal GroupLockInfo( IDisposable[] locks )
            {
                m_locks = locks;
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
                //
                // Release in opposite order.
                //
                for(int i = m_locks.Length; --i >= 0; )
                {
                    m_locks[i].Dispose();
                }
            }
        }

        //--//

        class CachedInfo_FlowInformation : CachedInfo
        {
            //
            // Helper Methods
            //

            protected override void Update()
            {
                using(new PerformanceCounters.ContextualTiming( m_owner, "FlowInformation" ))
                {
                    foreach(BasicBlock bb in m_owner.m_basicBlocks)
                    {
                        bb.ResetFlowInformation();
                    }

                    foreach(BasicBlock bb in m_owner.m_basicBlocks)
                    {
                        bb.UpdateFlowInformation();
                    }
                }
            }
        }

        //--//

        //
        // State
        //

        protected MethodRepresentation m_md;
        protected int                  m_version;
                                       
        protected EntryBasicBlock      m_entryBasicBlock;
        protected ExitBasicBlock       m_exitBasicBlock;
        protected BasicBlock[]         m_basicBlocks; // This is the set of all the basic blocks in the CFG.
        protected VariableExpression   m_returnValue;
        protected VariableExpression[] m_arguments;
        protected VariableExpression[] m_variables;
        protected int                  m_variablesCount;
        protected CachedInfo[]         m_cache;

        //
        // Constructor Methods
        //

        protected ControlFlowGraphState() // Default constructor required by TypeSystemSerializer.
        {
            m_cache = CachedInfo.SharedEmptyArray;
        }

        protected ControlFlowGraphState( MethodRepresentation md )
        {
            m_md          = md;
            m_version     = 1;
                         
            m_basicBlocks = BasicBlock.SharedEmptyArray;
            m_variables   = VariableExpression.SharedEmptyArray;

            m_cache       = CachedInfo.SharedEmptyArray;
        }

        protected ControlFlowGraphState( ControlFlowGraphState source ) : this( source.m_md )
        {
        }

        //--//

        //
        // Helper Methods
        //

        protected virtual void CloneVariables( CloningContext        context ,
                                               ControlFlowGraphState source  )
        {
            VariableExpression var;

            var = source.m_returnValue;
            if(var != null)
            {
                m_returnValue = AllocateTemporary( var.Type, var.DebugName );

                context.Register( var, m_returnValue );
            }

            //--//

            VariableExpression[] args    = source.m_arguments;
            int                  argsNum = args.Length;

            m_arguments = new VariableExpression[argsNum];

            for(int i = 0; i < argsNum; i++)
            {
                var = args[i];

                m_arguments[i] = new ArgumentVariableExpression( var.Type, var.DebugName, i );

                context.Register( var, m_arguments[i] );
            }
        }

        //--//

        protected void TrackVariable( VariableExpression var )
        {
            if(m_variablesCount == m_variables.Length)
            {
                m_variables = ArrayUtility.EnsureSizeOfNotNullArray( m_variables, m_variablesCount + 16 );
            }

            m_variables[m_variablesCount++] = var;
        }

        public LocalVariableExpression AllocateLocal( TypeRepresentation           td        ,
                                                      VariableExpression.DebugInfo debugInfo )
        {
            LocalVariableExpression newLocal = new LocalVariableExpression( td, debugInfo );

            TrackVariable( newLocal );

            return newLocal;
        }

        public TemporaryVariableExpression AllocateTemporary( TypeRepresentation           td        ,
                                                              VariableExpression.DebugInfo debugInfo )
        {
            TemporaryVariableExpression newTmp = new TemporaryVariableExpression( td, debugInfo );

            TrackVariable( newTmp );

            return newTmp;
        }

        public ExceptionObjectVariableExpression AllocateExceptionObjectVariable( TypeRepresentation td )
        {
            ExceptionObjectVariableExpression newEx = new ExceptionObjectVariableExpression( td, null );

            TrackVariable( newEx );

            return newEx;
        }

        //--//

        internal void Register( BasicBlock basicBlock )
        {
            BumpVersion();

            if(basicBlock is EntryBasicBlock)
            {
                CHECKS.ASSERT( m_entryBasicBlock == null, "Entry Basic Block already exists" );

                m_entryBasicBlock = (EntryBasicBlock)basicBlock;
            }
            else if(basicBlock is ExitBasicBlock)
            {
                CHECKS.ASSERT( m_exitBasicBlock == null, "Exit Basic Block already exists" );

                m_exitBasicBlock = (ExitBasicBlock)basicBlock;
            }

            m_basicBlocks = ArrayUtility.AddUniqueToNotNullArray( m_basicBlocks, basicBlock );
        }

        internal void Deregister( BasicBlock basicBlock )
        {
            BumpVersion();

            if(basicBlock == m_entryBasicBlock)
            {
                m_entryBasicBlock = null;
            }
            else if(basicBlock == m_exitBasicBlock)
            {
                m_exitBasicBlock = null;
            }
        }

        internal void BumpVersion()
        {
            m_version++;
        }

        //--//

        public static IDisposable LockThreadToMethod( MethodRepresentation md )
        {
            return new ThreadLockInfo( md );
        }

        public static IDisposable AddExceptionToThreadMethodLock( MethodRepresentation md )
        {
            return new ThreadLockInfo.ExceptionInfo( md );
        }

        public IDisposable GroupLock( params IDisposable[] locks )
        {
            return new GroupLockInfo( locks );
        }

        protected T GetCachedInfo< T >() where T : CachedInfo, new()
        {
            foreach(CachedInfo ci in m_cache)
            {
                if(ci is T)
                {
                    ci.RefreshIfNeeded();

                    return (T)ci;
                }
            }

            T newCI = new T();

            newCI.Owner = this;

            m_cache = ArrayUtility.AppendToNotNullArray( m_cache, newCI );

            newCI.RefreshIfNeeded();

            return newCI;
        }

        public void UpdateFlowInformation()
        {
            GetCachedInfo< CachedInfo_FlowInformation >();
        }

        public IDisposable LockFlowInformation()
        {
            var ci = GetCachedInfo< CachedInfo_FlowInformation >();

            ci.Lock();

            return ci;
        }

        //--//

        protected void InnerApplyTransformation( TransformationContextForIR context )
        {
            context.Transform( ref m_md              );
            context.Transform( ref m_version         );
                                 
            context.Transform( ref m_entryBasicBlock );
            context.Transform( ref m_exitBasicBlock  );
            context.Transform( ref m_basicBlocks     );
            context.Transform( ref m_returnValue     );
            context.Transform( ref m_arguments       );
            context.Transform( ref m_variables       );
            context.Transform( ref m_variablesCount  );
        }

        //--//

        public void PerformActionOnOperators( Action<Operator> action )
        {
            foreach(BasicBlock bb in m_basicBlocks)
            {
                foreach(Operator op in bb.Operators)
                {
                    action( op );
                }
            }
        }

        //--//

        public BasicBlock FirstBasicBlock
        {
            get
            {
                if(m_basicBlocks != null && m_basicBlocks.Length > 0)
                {
                    return m_basicBlocks[0];
                }

                return CreateFirstNormalBasicBlock();
            }
        }

        public NormalBasicBlock CreateFirstNormalBasicBlock()
        {
            //
            // Important: get 'NormalizedEntryBasicBlock' before allocating the new basic block, or it will be reclaimed!!
            //
            var bbPrev = this.NormalizedEntryBasicBlock;
            var bbNext = this.NormalizedExitBasicBlock;
            var bb     = new NormalBasicBlock( this );

            bbPrev.FlowControl = UnconditionalControlOperator.New( null, bb     );
            bb    .FlowControl = UnconditionalControlOperator.New( null, bbNext );

            return bb;
        }

        public NormalBasicBlock CreateLastNormalBasicBlock()
        {
            //
            // Important: get 'NormalizedExitBasicBlock' before allocating the new basic block, or it will be reclaimed!!
            //
            var bbNext = this.NormalizedExitBasicBlock;
            var bb     = new NormalBasicBlock( this );

            bb.FlowControl = UnconditionalControlOperator.New( null, bbNext );

            return bb;
        }

        public void AddReturnOperator()
        {
            //
            // Create proper flow control for exit basic block.
            //
            ControlOperator op;

            if(m_returnValue != null)
            {
                op = ReturnControlOperator.New( m_returnValue );
            }
            else
            {
                op = ReturnControlOperator.New();
            }

            m_exitBasicBlock.AddOperator( op );
        }

        public Operator GenerateVariableInitialization( Debugging.DebugInfo debugInfo ,
                                                        VariableExpression  var       )
        {
            return GenerateVariableInitialization( debugInfo, var, var.Type, false );
        }

        public abstract Operator GenerateVariableInitialization( Debugging.DebugInfo debugInfo       ,
                                                                 Expression          var             ,
                                                                 TypeRepresentation  td              ,
                                                                 bool                fThroughPointer );

        public abstract BasicBlock GetInjectionPoint( BasicBlock.Qualifier qualifier );

        //--//

        public virtual void RenumberVariables()
        {
            int numLocal = 0;
            int numTmp   = 0;
            int numEx    = 0;

            foreach(VariableExpression var in m_variables)
            {
                if(var is LocalVariableExpression)
                {
                    var.Number = numLocal++;
                }
                else if(var is TemporaryVariableExpression)
                {
                    var.Number = numTmp++;
                }
                else if(var is ExceptionObjectVariableExpression)
                {
                    var.Number = numEx++;
                }
            }
        }

        //--//

        public static bool SameCompilationConstraints( CompilationConstraints[] ccArray1 ,
                                                       CompilationConstraints[] ccArray2 )
        {
            int len1 = ccArray1.Length;
            int len2 = ccArray2.Length;

            if(len1 != len2)
            {
                return false;
            }

            for(int i = 0; i < len1; i++)
            {
                if(ccArray1[i] != ccArray2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static CompilationConstraints[] AddCompilationConstraint( CompilationConstraints[] ccArray ,
                                                                         CompilationConstraints   cc      )
        {
            int pos = 0;

            for(; pos < ccArray.Length; pos++)
            {
                CompilationConstraints cc2 = ccArray[pos];

                if(cc2 == cc)
                {
                    return ccArray;
                }

                if(cc2 > cc)
                {
                    break;
                }
            }

            return ArrayUtility.InsertAtPositionOfNotNullArray( ccArray, pos, cc );
        }

        public static CompilationConstraints[] RemoveCompilationConstraint( CompilationConstraints[] ccArray ,
                                                                            CompilationConstraints   cc      )
        {
            for(int pos = 0; pos < ccArray.Length; pos++)
            {
                CompilationConstraints cc2 = ccArray[pos];

                if(cc2 == cc)
                {
                    return ArrayUtility.RemoveAtPositionFromNotNullArray( ccArray, pos );
                }

                if(cc2 > cc)
                {
                    break;
                }
            }

            return ccArray;
        }

        public static bool HasCompilationConstraint( CompilationConstraints[] ccArray ,
                                                     CompilationConstraints   cc      )
        {
            for(int pos = 0; pos < ccArray.Length; pos++)
            {
                CompilationConstraints cc2 = ccArray[pos];

                if(cc2 == cc)
                {
                    return true;
                }

                if(cc2 > cc)
                {
                    break;
                }
            }

            return false;
        }

        public static bool HasAnyCompilationConstraint(        CompilationConstraints[] ccTarget ,
                                                        params CompilationConstraints[] ccFilter )
        {
            CompilationConstraints match;

            return HasAnyCompilationConstraint( ccTarget, out match, ccFilter );
        }

        public static bool HasAnyCompilationConstraint(        CompilationConstraints[] ccTarget ,
                                                        out    CompilationConstraints   match    ,
                                                        params CompilationConstraints[] ccFilter )
        {
            foreach(CompilationConstraints cc in ccFilter)
            {
                if(HasCompilationConstraint( ccTarget, cc ))
                {
                    match = cc;
                    return true;
                }
            }

            match = default(CompilationConstraints);

            return false;
        }

        //--//

        public static Operator CheckSingleUse( Operator[][]       useChains ,
                                               VariableExpression var       )
        {
            Operator[] uses = useChains[var.SpanningTreeIndex];

            if(uses.Length == 1)
            {
                return uses[0];
            }

            return null;
        }

        public static Operator CheckSingleDefinition( Operator[][]       defChains ,
                                                      VariableExpression var       )
        {
            Operator[] defs = defChains[var.SpanningTreeIndex];

            if(defs.Length == 1)
            {
                return defs[0];
            }

            return null;
        }

        public static Operator CheckSingleDefinition( GrowOnlyHashTable< VariableExpression, Operator > defLookup ,
                                                      Expression                                        ex        )
        {
            VariableExpression var = ex as VariableExpression;
            if(var != null)
            {
                Operator def;

                if(defLookup.TryGetValue( var, out def  ))
                {
                    return def;
                }
            }

            return null;
        }

        //--//

        //
        // Access Methods
        //

        public abstract TypeSystemForIR TypeSystemForIR
        {
            get;
        }

        public CompilationConstraints[] CompilationConstraintsArray
        {
            get
            {
                CompilationConstraints[]                 res = SharedEmptyCompilationConstraintsArray;
                MethodRepresentation.BuildTimeAttributes bta = m_md.BuildTimeFlags;

                if     ((bta & MethodRepresentation.BuildTimeAttributes.CanAllocate            ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.Allocations_ON        );
                else if((bta & MethodRepresentation.BuildTimeAttributes.CannotAllocate         ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.Allocations_OFF       );

                if     ((bta & MethodRepresentation.BuildTimeAttributes.StackAvailable         ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.StackAccess_ON        );
                else if((bta & MethodRepresentation.BuildTimeAttributes.StackNotAvailable      ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.StackAccess_OFF       );
                                                                                                                                  
                if     ((bta & MethodRepresentation.BuildTimeAttributes.EnableBoundsChecks     ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.BoundsChecks_ON       );
                else if((bta & MethodRepresentation.BuildTimeAttributes.DisableBoundsChecks    ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.BoundsChecks_OFF      );
                else if((bta & MethodRepresentation.BuildTimeAttributes.DisableDeepBoundsChecks) != 0) res = AddCompilationConstraint( res, CompilationConstraints.BoundsChecks_OFF_DEEP );
                                                                                                                                  
                if     ((bta & MethodRepresentation.BuildTimeAttributes.EnableNullChecks       ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.NullChecks_ON         );
                else if((bta & MethodRepresentation.BuildTimeAttributes.DisableNullChecks      ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.NullChecks_OFF        );
                else if((bta & MethodRepresentation.BuildTimeAttributes.DisableDeepNullChecks  ) != 0) res = AddCompilationConstraint( res, CompilationConstraints.NullChecks_OFF_DEEP   );

                return res;
            }
        }

        public MethodRepresentation Method
        {
            get
            {
                return m_md;
            }
        }

        public EntryBasicBlock EntryBasicBlock
        {
            get
            {
                return m_entryBasicBlock;
            }
        }

        public ExitBasicBlock ExitBasicBlock
        {
            get
            {
                return m_exitBasicBlock;
            }
        }

        public BasicBlock NormalizedEntryBasicBlock
        {
            get
            {
                return this.GetInjectionPoint( BasicBlock.Qualifier.EntryInjectionStart );
            }
        }

        public BasicBlock NormalizedExitBasicBlock
        {
            get
            {
                return this.GetInjectionPoint( BasicBlock.Qualifier.EpilogueStart );
            }
        }

        public VariableExpression ReturnValue
        {
            get
            {
                return m_returnValue;
            }
        }

        public VariableExpression[] Arguments
        {
            get
            {
                return m_arguments;
            }
        }

        public int Version
        {
            get
            {
                return m_version;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return string.Format( "FlowGraph({0})", m_md.ToShortString() );
        }

        public void Dump( IIntermediateRepresentationDumper dumper )
        {
            dumper.DumpGraph( this );
        }

        public abstract string ToPrettyString( Operator op );
    }
}

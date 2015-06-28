//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CallsDataBase
    {
        internal class Entry
        {
            //
            // State
            //

            int                                        m_version;
            ControlFlowGraphStateForCodeTransformation m_cfg;
            ControlFlowGraphStateForCodeTransformation m_clonedCFG;
            List< CallOperator >                       m_callsFromThisMethod;
            List< CallOperator >                       m_callsToThisMethod;

            //
            // Constructor Methods
            //

            internal Entry( ControlFlowGraphStateForCodeTransformation cfg )
            {
                m_cfg                 = cfg;
                m_version             = -1;
                m_callsFromThisMethod = new List< CallOperator >();
                m_callsToThisMethod   = new List< CallOperator >();
            }

            //
            // Helper Methods
            //

            internal void Reset()
            {
                m_callsFromThisMethod.Clear();
                m_callsToThisMethod  .Clear();
            }

            internal void AddCallFromThisMethod( CallOperator call )
            {
                lock(this)
                {
                    m_callsFromThisMethod.Add( call );
                }
            }

            internal void AddCallToThisMethod( CallOperator call )
            {
                lock(this)
                {
                    m_callsToThisMethod.Add( call );
                }
            }

            internal void AnalyzeInlining()
            {
                if(m_version != m_cfg.Version)
                {
                    m_version = m_cfg.Version;

                    bool fInline;

                    MethodRepresentation md = m_cfg.Method;

                    if(md.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.NoInline ))
                    {
                        fInline = false;
                    }
                    else if(md.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.Inline ))
                    {
                        fInline = true;
                    }
                    else
                    {
                        bool fCallToConstructor = false;
                        int  iCall              = 0;
                        int  iGetter            = 0;
                        int  iSetter            = 0;
                        int  iOther             = 0;

                        var set   = SetFactory.NewWithReferenceEquality< BasicBlock >();
                        var queue = new Queue                          < BasicBlock >();

                        var bbStart = m_cfg.NormalizedEntryBasicBlock;
                        var bbEnd   = m_cfg.NormalizedExitBasicBlock;

                        set  .Insert ( bbStart );
                        queue.Enqueue( bbStart );

                        while(queue.Count > 0)
                        {
                            var bb = queue.Dequeue();

                            foreach(var edge in bb.Successors)
                            {
                                var bbNext = edge.Successor;

                                if(bbNext != bbEnd && set.Insert( bbNext ) == false)
                                {
                                    queue.Enqueue( bbNext );
                                }
                            }

                            foreach(Operator op in bb.Operators)
                            {
                                if(op is CallOperator)
                                {
                                    CallOperator call = (CallOperator)op;

                                    if(call.TargetMethod is ConstructorMethodRepresentation)
                                    {
                                        fCallToConstructor = true;
                                    }
                                    else
                                    {
                                        iCall++;
                                    }
                                }
                                else if(op is LoadFieldOperator)
                                {
                                    iGetter++;
                                }
                                else if(op is StoreFieldOperator)
                                {
                                    iSetter++;
                                }
                                else if(op is NopOperator                  ||
                                        op is UnconditionalControlOperator ||
                                        op is AbstractAssignmentOperator   ||
                                        op is ReturnControlOperator         )
                                {
                                    // Ignore them.
                                }
                                else
                                {
                                    iOther++;
                                }
                            }
                        }

                        if(!fCallToConstructor && iGetter == 0 && iSetter == 0 && iOther == 0)
                        {
                            fInline = true; // Noop method.
                        }
                        else if(fCallToConstructor && iGetter == 0 && iSetter == 0 && iOther == 0)
                        {
                            fInline = true; // Empty constructor.
                        }
                        else if(!fCallToConstructor && iGetter == 1 && iSetter == 0 && iOther < 2)
                        {
                            fInline = true; // Simple getter.
                        }
                        else if(!fCallToConstructor && iGetter == 0 && iSetter == 1 && iOther == 0)
                        {
                            fInline = true; // Simple setter.
                        }
                        else if(!fCallToConstructor && iGetter == 0 && iSetter == 0 && iCall == 0 && iOther < 4)
                        {
                            fInline = true; // Simple method with no calls.
                        }
                        else
                        {
                            fInline = false;
                        }
                    }

                    if(fInline)
                    {
                        m_clonedCFG = m_cfg.Clone( null );
                    }
                    else
                    {
                        m_clonedCFG = null;
                    }
                }
            }

            internal void EnsureCloned()
            {
                if(m_clonedCFG == null)
                {
                    m_clonedCFG = m_cfg.Clone( null );
                }
            }

            internal void QueueInlining( GrowOnlySet< ControlFlowGraphStateForCodeTransformation >                             touched  ,
                                         GrowOnlyHashTable< ControlFlowGraphStateForCodeTransformation, List< CallOperator > > workList )
            {
                if(m_clonedCFG != null)
                {
                    for(int pos = m_callsToThisMethod.Count; --pos >= 0; )
                    {
                        bool         fRemove = false;
                        CallOperator call    = m_callsToThisMethod[pos];

                        while(true)
                        {
                            BasicBlock bb = call.BasicBlock;
                            if(bb == null)
                            {
                                //
                                // Dead operator, just remove it.
                                //
                                fRemove = true;
                                break;
                            }

                            var cfg = (ControlFlowGraphStateForCodeTransformation)bb.Owner;
                            if(cfg == m_cfg)
                            {
                                //
                                // Can't self-inline.
                                //
                                break;
                            }

                            //
                            // Detect inlining loops and stop them.
                            //
                            var an = call.GetAnnotation< InliningPathAnnotation >();
                            if(an != null)
                            {
                                if(ArrayUtility.FindInNotNullArray( an.Path, m_clonedCFG.Method ) >= 0)
                                {
                                    break;
                                }
                            }

                            var callType = call.CallType;

                            if(callType == CallOperator.CallKind.Virtual  || 
                               callType == CallOperator.CallKind.Indirect  )
                            {
                                break;
                            }

                            var cfgInlineSite = (ControlFlowGraphStateForCodeTransformation)call.BasicBlock.Owner;

                            touched.Insert( cfgInlineSite );

                            HashTableWithListFactory.AddUnique( workList, cfgInlineSite, call );
                            fRemove = true;
                            break;
                        }

                        if(fRemove)
                        {
                            m_callsToThisMethod.RemoveAt( pos );
                        }
                    }
                }
            }

            //
            // Access Methods
            //

            internal ControlFlowGraphStateForCodeTransformation Cfg
            {
                get
                {
                    return m_cfg;
                }
            }

            internal ControlFlowGraphStateForCodeTransformation ClonedCfg
            {
                get
                {
                    return m_clonedCFG;
                }
            }

            internal List< CallOperator > CallFromThisMethod
            {
                get
                {
                    return m_callsFromThisMethod;
                }
            }

            internal List< CallOperator > CallToThisMethod
            {
                get
                {
                    return m_callsToThisMethod;
                }
            }
        }

        //
        // State
        //

        private GrowOnlyHashTable< ControlFlowGraphStateForCodeTransformation, Entry > m_entries;
        private GrowOnlySet      < CallOperator                                      > m_forcedInlines;

        //
        // Constructor Methods
        //

        public CallsDataBase()
        {
            m_entries       = HashTableFactory.NewWithReferenceEquality< ControlFlowGraphStateForCodeTransformation, Entry >();
            m_forcedInlines = SetFactory      .NewWithReferenceEquality< CallOperator                                      >();
        }

        //
        // Helper Methods
        //

        public void RegisterCallSite( CallOperator call )
        {
            var cfgFrom = (ControlFlowGraphStateForCodeTransformation)      call.BasicBlock.Owner;
            var cfgTo   = TypeSystemForCodeTransformation.GetCodeForMethod( call.TargetMethod );

            Entry enFrom = GetEntry( cfgFrom, true );

            enFrom.AddCallFromThisMethod( call );

            if(cfgTo != null)
            {
                Entry enTo = GetEntry( cfgTo, true );

                enTo.AddCallToThisMethod( call );
            }
        }

        public void QueueForForcedInlining( CallOperator call )
        {
            lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
            {
                m_forcedInlines.Insert( call );
            }
        }

        public void ClearCallSites()
        {
            m_entries.Clear();
        }

        public void ResetCallSites()
        {
            foreach(Entry en in m_entries.Values)
            {
                en.Reset();
            }
        }

        public List< CallOperator > CallsToMethod( MethodRepresentation md )
        {
            var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

            Entry en = GetEntry( cfg, false );

            return (en != null) ? en.CallToThisMethod : null;
        }

        public List< CallOperator > CallsFromMethod( MethodRepresentation md )
        {
            var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );

            Entry en = GetEntry( cfg, false );

            return (en != null) ? en.CallFromThisMethod : null;
        }

        //--//

        public void Analyze( TypeSystemForCodeTransformation typeSystem )
        {
            ResetCallSites();

            ParallelTransformationsHandler.EnumerateFlowGraphs( typeSystem, delegate( ControlFlowGraphStateForCodeTransformation cfg )
            {
                foreach(var call in cfg.FilterOperators< CallOperator >())
                {
                    RegisterCallSite( call );
                }
            } );
        }

        public void AnalyzeForInlining()
        {
            foreach(Entry en in m_entries.Values)
            {
                en.AnalyzeInlining();
            }
        }

        public GrowOnlySet< ControlFlowGraphStateForCodeTransformation > ExecuteInlining( TypeSystemForCodeTransformation typeSystem )
        {
            var workList = HashTableFactory.NewWithReferenceEquality< ControlFlowGraphStateForCodeTransformation, List< CallOperator > >();
            var touched  = SetFactory      .NewWithReferenceEquality< ControlFlowGraphStateForCodeTransformation                       >();
            var touched2 = touched.CloneSettings();

            while(true)
            {
                workList.Clear();
                touched2.Clear();

                foreach(Entry en in m_entries.Values)
                {
                    en.QueueInlining( touched2, workList );
                }

                foreach(CallOperator call in m_forcedInlines)
                {
                    if(!call.IsDetached)
                    {
                        var cfgInlineSite = (ControlFlowGraphStateForCodeTransformation)call.BasicBlock.Owner;
                        var cfgTarget     = TypeSystemForCodeTransformation.GetCodeForMethod( call.TargetMethod );

                        touched2.Insert( cfgInlineSite );

                        var en = GetEntry( cfgTarget, true );
                        en.EnsureCloned();

                        HashTableWithListFactory.AddUnique( workList, cfgInlineSite, call );
                    }
                }

                if(workList.Count == 0)
                {
                    break;
                }

                //--//

                var lockList = HashTableFactory.NewWithReferenceEquality< ControlFlowGraphStateForCodeTransformation, IDisposable >();

                foreach(var cfg in workList.Keys)
                {
                    foreach(var call in workList[cfg])
                    {
                        var   cfgTarget = TypeSystemForCodeTransformation.GetCodeForMethod( call.TargetMethod );
                        Entry en;

                        m_entries.TryGetValue( cfgTarget, out en );

                        var cfgCloned = en.ClonedCfg;

                        if(lockList.ContainsKey( cfgCloned ) == false)
                        {
                            var bbEntry = cfgCloned.NormalizedEntryBasicBlock;
                            var bbExit  = cfgCloned.NormalizedExitBasicBlock;

                            lockList[cfgTarget] = cfgCloned.LockFlowInformation();
                        }
                    }
                }

                ParallelTransformationsHandler.EnumerateFlowGraphs( typeSystem, delegate( ControlFlowGraphStateForCodeTransformation cfg )
                {
                    List< CallOperator > calls;
                    bool                 fGot = false;

                    if(workList.TryGetValue( cfg, out calls ))
                    {
                        foreach(var call in calls)
                        {
                            var   cfgTarget = TypeSystemForCodeTransformation.GetCodeForMethod( call.TargetMethod );
                            Entry en;

                            m_entries.TryGetValue( cfgTarget, out en );

                            cfg.TraceToFile( "InlineCall" );
        
                            using(ControlFlowGraphState.AddExceptionToThreadMethodLock( call.TargetMethod ))
                            {
                                Transformations.InlineCall.Execute( call, en.ClonedCfg, null );
                            }
        
                            cfg.TraceToFile( "InlineCall-Post" );

                            fGot = true;
                        }

                        if(fGot)
                        {
                            Transformations.CommonMethodRedundancyElimination.Execute( cfg );
            
                            cfg.DropDeadVariables();
                        }
                    }
                } );

                foreach(var disp in lockList.Values)
                {
                    disp.Dispose();
                }

                touched.Merge( touched2 );
            }

            return touched;
        }

        //--//

        Entry GetEntry( ControlFlowGraphStateForCodeTransformation cfg       ,
                        bool                                       fAllocate )
        {
            lock(TypeSystemForCodeTransformation.Lock) // It's called from multiple threads during parallel phase executions.
            {
                Entry entry;

                if(m_entries.TryGetValue( cfg, out entry ) == false && fAllocate)
                {
                    entry = new Entry( cfg );

                    m_entries[cfg] = entry;
                }

                return entry;
            }
        }
    }
}

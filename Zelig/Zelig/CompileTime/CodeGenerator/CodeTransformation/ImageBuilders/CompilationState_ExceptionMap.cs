//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public partial class CompilationState
    {
        class ExceptionMapBuilder
        {
            //
            // State
            //

            ExceptionMap       m_exceptionMap;
            SequentialRegion   m_firstRegion;
            SequentialRegion   m_lastRegion;
            ExceptionMap.Range m_currentRange;

            //
            // Constructor Methods
            //

            internal ExceptionMapBuilder( ExceptionMap em )
            {
                m_exceptionMap = em;
            }

            //
            // Helper Methods
            //

            internal void Compress( DataManager      dm  ,
                                    SequentialRegion reg )
            {
                BasicBlock         bb    = (BasicBlock)reg.Context;
                ExceptionMap.Range range = m_currentRange;

                range.Handlers = ExceptionMap.Handler.SharedEmptyArray;

                foreach(ExceptionHandlerBasicBlock ehBB in bb.ProtectedBy)
                {
                    foreach(ExceptionClause ec in ehBB.HandlerFor)
                    {
                        ExceptionMap.Handler hnd  = new ExceptionMap.Handler();
                        TypeRepresentation   td   = ec.ClassObject;
                        bool                 fAdd = true;

                        hnd.HandlerCode = dm.CreateCodePointer( ehBB );

                        if(td != null)
                        {
                            hnd.Filter = td.VirtualTable;
                        }

                        uint handlerAddress = reg.Owner.Resolve( ehBB );

                        for(int i = 0; i < range.Handlers.Length; i++)
                        {
                            VTable filter = range.Handlers[i].Filter;

                            if(filter == null)
                            {
                                //
                                // There's already a catch-all, no need to extend the set of handlers.
                                //
                                fAdd = false;
                                break;
                            }

                            if(filter.TypeInfo.IsSuperClassOf( td, null ))
                            {
                                //
                                // There's already a less-specific catch, no need to extend the set of handlers.
                                //
                                fAdd = false;
                                break;
                            }

                            //
                            // Same handlers? Then this could be a less-specific catch, update the handler.
                            //
                            object ehBB2           = dm.GetCodePointerFromUniqueID( range.Handlers[i].HandlerCode.Target );
                            uint   handlerAddress2 = reg.Owner.Resolve( ehBB2 );

                            if(handlerAddress == handlerAddress2)
                            {
                                range.Handlers[i].Filter = hnd.Filter;

                                fAdd = false;
                                break;
                            }
                        }

                        if(fAdd)
                        {
                            hnd.HandlerCode = dm.CreateCodePointer( ehBB );

                            range.Handlers = ArrayUtility.AppendToNotNullArray( range.Handlers, hnd );
                        }
                    }
                }

                if(m_currentRange.SameContents( ref range ) == false)
                {
                    Emit();
                }
                else if(m_lastRegion != null)
                {
                    uint startAddress = reg         .BaseAddress.ToUInt32();
                    uint endAddress   = m_lastRegion.EndAddress .ToUInt32();

                    if(endAddress != startAddress)
                    {
                        CHECKS.ASSERT( endAddress < startAddress, "Incorrect ordering of regions: {0} <=> {1}", reg, m_lastRegion );

                        Emit();
                    }
                }

                m_currentRange = range;

                if(m_firstRegion == null)
                {
                    m_firstRegion        = reg;
                    m_currentRange.Start = reg.BaseAddress;
                }
                m_lastRegion = reg;

            }

            internal void Emit()
            {
                if(m_firstRegion != null)
                {
                    if(m_currentRange.Handlers.Length > 0)
                    {
                        m_currentRange.End = m_lastRegion.EndAddress;

                        if(m_currentRange.Start != m_currentRange.End)
                        {
                            //
                            // Only emit non-empty ranges that have handlers.
                            //
                            m_exceptionMap.Ranges = ArrayUtility.AppendToNotNullArray( m_exceptionMap.Ranges, m_currentRange );
                        }
                    }

                    m_firstRegion = null;
                    m_lastRegion  = null;
                }
            }
        }

        //
        // State
        //

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        internal bool CreateExceptionHandlingTables()
        {
            SequentialRegion[] regions = GetSortedCodeRegions();

            //--//

            ExceptionMap em = new ExceptionMap();

            em.Ranges = ExceptionMap.Range.SharedEmptyArray;

            DataManager         dm  = m_cfg.TypeSystem.DataManagerInstance;
            ExceptionMapBuilder emb = new ExceptionMapBuilder( em );

            foreach(SequentialRegion reg in regions)
            {
                emb.Compress( dm, reg );
            }

            emb.Emit();

            //--//

            if(em.Ranges.Length == 0)
            {
                em = null;
            }

            ExceptionMap emOld = m_cfg.Method.CodeMap.ExceptionMap;

            bool fModified = ExceptionMap.SameContents( em, emOld ) == false;
            if(fModified)
            {
                m_cfg.Method.CodeMap.ExceptionMap = em;
            }

            return fModified;
        }
    }
}
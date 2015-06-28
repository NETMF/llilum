//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmCompilationState
    {
        class CodeMapBuilder
        {
            class Delta
            {
                //
                // State
                //

                internal uint        Address;
                internal GCInfo.Kind PtrKind;
                internal ushort      StorageNumber;
                internal bool        IsRegister;
                internal bool        IsAlive;

                //
                // Helper Methods
                //

                internal bool IsMatch( GCInfo.Kind ptrKind       ,
                                       ushort      storageNumber ,
                                       bool        isRegister    )
                {
                    return (this.PtrKind       == ptrKind       &&
                            this.StorageNumber == storageNumber &&
                            this.IsRegister    == isRegister     );
                }

                //
                // Debug Methods
                //

                public override string ToString()
                {
                    return string.Format( "0x{0:X8}: {1} {2}{3} {4}", this.Address, this.PtrKind, this.IsRegister ? "Reg" : "Stack", this.StorageNumber, this.IsAlive ? "ALIVE" : "DEAD" );
                }
            }

            //
            // State
            //

            CodeMap                                m_codeMap;
            ZeligIR.ImageBuilders.SequentialRegion m_firstRegion;
            ZeligIR.ImageBuilders.SequentialRegion m_lastRegion;

            List< Delta >                          m_deltas;
            CodeMap.EncodedStackWalk[]             m_workingArray;
            int                                    m_workingPos;

            //
            // Constructor Methods
            //

            internal CodeMapBuilder( CodeMap cm )
            {
                m_codeMap = cm;

                m_deltas  = new List< Delta >();
            }

            //
            // Helper Methods
            //

            internal void Compress( ArmCompilationState                    cs  ,
                                    ZeligIR.ImageBuilders.SequentialRegion reg )
            {
                uint startAddress = reg.BaseAddress.ToUInt32();

                if(m_lastRegion != null)
                {
                    uint endAddress = m_lastRegion.EndAddress.ToUInt32();

                    if(endAddress != startAddress)
                    {
                        CHECKS.ASSERT( endAddress < startAddress, "Incorrect ordering of regions: {0} <=> {1}", reg, m_lastRegion );

                        Emit( cs );
                    }
                    else if(reg.Context is ZeligIR.EntryBasicBlock)
                    {
                        //
                        // Make sure the entry point to a method is always at the start of a range.
                        //
                        Emit( cs );
                    }
                }

                if(m_firstRegion == null)
                {
                    m_firstRegion = reg;
                }
                m_lastRegion = reg;

                //
                // We assume that all the variables are dead on entry to a basic block.
                //
                for(int idx = 0; idx < cs.m_variables.Length; idx++)
                {
                    CompressAddVariable( cs, cs.m_variables[idx], startAddress, false );
                }

                foreach(ZeligIR.ImageBuilders.ImageAnnotation an in reg.AnnotationList)
                {
                    ZeligIR.ImageBuilders.TrackVariableLifetime tvl = an as ZeligIR.ImageBuilders.TrackVariableLifetime;
                    if(tvl != null)
                    {
                        CompressAddVariable( cs, (ZeligIR.VariableExpression)tvl.Target, startAddress + tvl.Offset, tvl.IsAlive );
                    }
                }
            }

            internal void CompressAddVariable( ArmCompilationState        cs       ,
                                               ZeligIR.VariableExpression var      ,
                                               uint                       address  ,
                                               bool                       fIsAlive )
            {
                if(ZeligIR.ImageBuilders.TrackVariableLifetime.ShouldTrackAsAPointer( var ))
                {
                    GCInfo.Kind ptrKind = var.Type.ClassifyAsPointer();

                    if(var is ZeligIR.PhysicalRegisterExpression)
                    {
                        ZeligIR.PhysicalRegisterExpression varReg = (ZeligIR.PhysicalRegisterExpression)var;

                        if(varReg.RegisterDescriptor.InIntegerRegisterFile)
                        {
                            Add( address, ptrKind, (int)varReg.RegisterDescriptor.Encoding, true, fIsAlive );
                        }
                    }
                    else if(var is ZeligIR.StackLocationExpression)
                    {
                        Add( address, ptrKind, (cs.GetOffsetOfStackLocation( var ) / sizeof(uint)), false, fIsAlive );
                    }
                    else
                    {
                        CHECKS.ASSERT( false, "Unexpected variable tracked as pointer: {0}", var );
                    }
                }
            }

            internal void Emit( ArmCompilationState cs )
            {
                if(m_firstRegion != null)
                {
                    CodeMap.Range      rng = new CodeMap.Range();
                    ZeligIR.BasicBlock bb  = (ZeligIR.BasicBlock)m_firstRegion.Context;

                    rng.Flags = cs.m_headerFlags;

                    if(bb is ZeligIR.EntryBasicBlock)
                    {
                        rng.Flags |= CodeMap.Flags.EntryPoint;

                        if(bb.Owner.Method.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.BottomOfCallStack ))
                        {
                            rng.Flags |= CodeMap.Flags.BottomOfCallStack;
                        }
                    }
                    else if(bb is ZeligIR.ExceptionHandlerBasicBlock)
                    {
                        rng.Flags |= CodeMap.Flags.ExceptionHandler;
                    }
                    else
                    {
                        rng.Flags |= CodeMap.Flags.NormalCode;
                    }

                    rng.Start = m_firstRegion.BaseAddress;
                    rng.End   = m_lastRegion .EndAddress;

                    //--//

                    uint        lastAddress    = rng.Start.ToUInt32();
                    GCInfo.Kind currentPtrKind = GCInfo.Kind.Heap;

                    m_workingArray = null;
                    m_workingPos   = 0;

                    foreach(Delta dt in m_deltas)
                    {
                        //
                        // Move forward the opcode pointer.
                        //
                        if(dt.Address != lastAddress)
                        {
                            CHECKS.ASSERT( dt.Address > lastAddress, "Incorrect delta sequence" );

                            uint diff = (dt.Address - lastAddress) / sizeof(uint);
                            while(diff > 0)
                            {
                                uint count = Math.Min( diff, (uint)CodeMap.EncodedStackWalk.SkipMax );

                                AddTrackingInfo( CodeMap.EncodedStackWalk.SkipToOpcode, CodeMap.EncodedStackWalk.SkipMask, count );

                                diff -= count;
                            }

                            lastAddress = dt.Address;
                        }

                        if(dt.PtrKind != currentPtrKind)
                        {
                            uint val;

                            switch(dt.PtrKind)
                            {
                                case GCInfo.Kind.Heap:
                                    val = (uint)CodeMap.EncodedStackWalk.TrackingHeapPointers;
                                    break;

                                case GCInfo.Kind.Internal:
                                    val = (uint)CodeMap.EncodedStackWalk.TrackingInternalPointers;
                                    break;

                                case GCInfo.Kind.Potential:
                                    val = (uint)CodeMap.EncodedStackWalk.TrackingPotentialPointers;
                                    break;

                                default:
                                    throw TypeConsistencyErrorException.Create( "Unexcepted pointer kind: {0}", dt.PtrKind );
                            }

                            AddTrackingInfo( CodeMap.EncodedStackWalk.SetMode, CodeMap.EncodedStackWalk.ModeMask, val );

                            currentPtrKind = dt.PtrKind;
                        }

                        if(dt.IsRegister)
                        {
                            CodeMap.EncodedStackWalk effect = dt.IsAlive ? CodeMap.EncodedStackWalk.EnterRegisterSet : CodeMap.EncodedStackWalk.LeaveRegisterSet;

                            AddTrackingInfo( effect, CodeMap.EncodedStackWalk.RegisterMask, dt.StorageNumber );
                        }
                        else
                        {
                            CodeMap.EncodedStackWalk effect = dt.IsAlive ? CodeMap.EncodedStackWalk.EnterStack : CodeMap.EncodedStackWalk.LeaveStack;

                            ushort offset = dt.StorageNumber;
                            if(offset <= (ushort)CodeMap.EncodedStackWalk.StackOffsetMax)
                            {
                                AddTrackingInfo( effect, CodeMap.EncodedStackWalk.StackOffsetMask, offset );
                            }
                            else if(offset < 0xFF)
                            {
                                AddTrackingInfo( effect, CodeMap.EncodedStackWalk.StackOffsetMask, (uint)CodeMap.EncodedStackWalk.StackOffset8BitExtender );
                                AddTrackingInfo( (CodeMap.EncodedStackWalk)offset );
                            }
                            else
                            {
                                AddTrackingInfo( effect, CodeMap.EncodedStackWalk.StackOffsetMask, (uint)CodeMap.EncodedStackWalk.StackOffset16BitExtender );
                                AddTrackingInfo( (CodeMap.EncodedStackWalk)(offset >> 0) );
                                AddTrackingInfo( (CodeMap.EncodedStackWalk)(offset >> 8) );
                            }
                        }
                    }

                    if(m_workingArray != null)
                    {
                        rng.StackWalk = ArrayUtility.ExtractSliceFromNotNullArray( m_workingArray, 0, m_workingPos );
                    }

                    m_codeMap.Ranges = ArrayUtility.AppendToNotNullArray( m_codeMap.Ranges, rng );

                    //--//

                    m_deltas.Clear();

                    m_firstRegion = null;
                    m_lastRegion  = null;
                }
            }

            //--//

            private void Add( uint        address       ,
                              GCInfo.Kind ptrKind       ,
                              int         storageNumber ,
                              bool        isRegister    ,
                              bool        isAlive       )
            {
                Add( address, ptrKind, (ushort)storageNumber, isRegister, isAlive );
            }

            private void Add( uint        address       ,
                              GCInfo.Kind ptrKind       ,
                              ushort      storageNumber ,
                              bool        isRegister    ,
                              bool        isAlive       )
            {
                int pos = m_deltas.Count;

                while(--pos >= 0)
                {
                    Delta dt = m_deltas[pos];

                    if(dt.IsMatch( ptrKind, storageNumber, isRegister ))
                    {
                        if(dt.IsAlive == isAlive)
                        {
                            //
                            // Nothing change, just exit.
                            //
                            return;
                        }

                        if(dt.Address == address)
                        {
                            //
                            // Since we found a toggling at the target offset,
                            // make sure we don't have a previous delta with the same state.
                            // If so, remove the current delta, since it would do nothing.
                            //
                            for(int pos2 = pos; --pos2 >= 0; )
                            {
                                Delta dt2 = m_deltas[pos2];

                                if(dt2.IsMatch( ptrKind, storageNumber, isRegister ))
                                {
                                    if(dt2.IsAlive == isAlive)
                                    {
                                        //
                                        // Found a delta with the same direction, remove the later one.
                                        //
                                        m_deltas.RemoveAt( pos );
                                        return;
                                    }

                                    break;
                                }
                            }

                            //
                            // Update liveness.
                            //
                            dt.IsAlive = isAlive;
                            return;
                        }

                        break;
                    }
                }

                //
                // Initial state is dead anyway, don't create a redundant delta.
                //
                if(pos < 0 && isAlive == false)
                {
                    return;
                }

                Delta dtNew = new Delta();

                dtNew.Address       = address;
                dtNew.PtrKind       = ptrKind;
                dtNew.StorageNumber = storageNumber;
                dtNew.IsRegister    = isRegister;
                dtNew.IsAlive       = isAlive;

                m_deltas.Add( dtNew );
            }

            //--//

            private void AddTrackingInfo( CodeMap.EncodedStackWalk effect ,
                                          CodeMap.EncodedStackWalk mask   ,
                                          uint                     val    )
            {
                CHECKS.ASSERT( val <= 0xFF && ((CodeMap.EncodedStackWalk)val & ~mask) == 0, "Invalid range for tracking info: {0} does not fit in {1}", val, mask );

                AddTrackingInfo( (effect | ((CodeMap.EncodedStackWalk)val & mask)) );
            }

            private void AddTrackingInfo( CodeMap.EncodedStackWalk val )
            {
                if(m_workingArray == null)
                {
                    m_workingArray = new CodeMap.EncodedStackWalk[128];
                }
                else if(m_workingPos >= m_workingArray.Length)
                {
                    m_workingArray = ArrayUtility.IncreaseSizeOfNotNullArray( m_workingArray, 128 );
                }

                m_workingArray[m_workingPos++] = val;
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

        public override bool CreateCodeMaps()
        {
            //
            // We don't need to worry about how we get to a certain opcode,
            // the point of CodeMap.Range is to describe the pointers alive at each opcode.
            //
            // So play the TrackPointerLifetime history for each region, computing the "final" state at each offset.
            // If it's different from the previous one, emit the delta info.
            //

            ZeligIR.ImageBuilders.SequentialRegion[] regions = GetSortedCodeRegions();

            //--//

            CodeMap cm = new CodeMap();

            cm.Target = m_cfg.Method;
            cm.Ranges = CodeMap.Range.SharedEmptyArray;

            CodeMapBuilder cmb = new CodeMapBuilder( cm );

            foreach(ZeligIR.ImageBuilders.SequentialRegion reg in regions)
            {
                cmb.Compress( this, reg );
            }

            cmb.Emit( this );

            //--//

            CodeMap cmOld = m_cfg.Method.CodeMap;

            if(cmOld != null)
            {
                cm.ExceptionMap = cmOld.ExceptionMap;
            }

            if(cmOld == null || CodeMap.SameContents( cmOld, cm ) == false)
            {
                m_cfg.Method.CodeMap = cm;

                return true;
            }

            return false;
        }
    }
}
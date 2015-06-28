//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_BASICBLOCK_SCHEDULING

namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public partial class CompilationState
    {
#if DEBUG_BASICBLOCK_SCHEDULING
        const string sDebugTarget = 
            "FlowGraph(void Microsoft.Zelig.Runtime.Bootstrap::EntryPoint())";
////        "FlowGraph(int Microsoft.Zelig.Runtime.Processor::Delay(int))";
////        "FlowGraph(void Microsoft.Zelig.Runtime.Memory::Dirty(System.UIntPtr,System.UIntPtr))";
////        "FlowGraph(void Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.ProcessorARMv4::VectorsTable())";
////        "FlowGraph(void Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.ProcessorARMv4WithContextSwitch::LongJump(Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.ProcessorARMv4WithContextSwitch.Context.RegistersOnStack&))";
////        "FlowGraph(void Microsoft.Zelig.Runtime.MarkAndSweepCollector::Sweep())";
#endif

        [Flags]
        enum SlotEdgeFlavor
        {
            Inbound               = 0x00000001,
            Outbound              = 0x00000002,
            SourceIsInternal      = 0x00000004,
            DestinationIsInternal = 0x00000008,
        }

        class SlotEdge
        {
            //
            // State
            //

            internal readonly AtomicSlot     m_source;
            internal readonly AtomicSlot     m_destination;
            internal readonly SlotEdgeFlavor m_flavor;

            //
            // Constructor Methods
            //

            internal SlotEdge( AtomicSlot     source      ,
                               AtomicSlot     destination ,
                               SlotEdgeFlavor flavor      )
            {
                m_source      = source;
                m_destination = destination;
                m_flavor      = flavor;
            }

            //
            // AccessMethods
            //

            internal bool IsInbound
            {
                get
                {
                    return (m_flavor & SlotEdgeFlavor.Inbound) != 0;
                }
            }

            internal bool IsOutbound
            {
                get
                {
                    return (m_flavor & SlotEdgeFlavor.Outbound) != 0;
                }
            }

            internal bool IsBoundary
            {
                get
                {
                    return (m_flavor & (SlotEdgeFlavor.SourceIsInternal | SlotEdgeFlavor.DestinationIsInternal)) == 0;
                }
            }
        }

        abstract class Slot
        {
            //
            // State
            //

            protected readonly CompilationState m_owner;
            private   readonly List< SlotEdge > m_edges;
                     
            private            CompoundSlot     m_superSlot;
            protected          int              m_weight;
            private            int              m_schedulingIndex;

            //
            // Constructor Methods
            //

            protected Slot( CompilationState owner )
            {
                m_owner           = owner;
                m_edges           = new List< SlotEdge >();

                m_schedulingIndex = -1;

                owner.RegisterTopLevel( this );
            }

            //
            // Helper Methods
            //

            protected static void AddConnection( SlotEdge edge )
            {
                AddConnection( edge.m_source, edge.m_destination );
            }

            protected static void AddConnection( AtomicSlot source      ,
                                                 AtomicSlot destination )
            {
                if(destination.Target is ExceptionHandlerBasicBlock)
                {
                    // For scheduling purposes, only include edges to normal basic blocks.
                }
                else
                {
                    Slot topLevelSrc = source     .TopLevel;
                    Slot topLevelDst = destination.TopLevel;

                    if(topLevelSrc == topLevelDst)
                    {
                        // Don't include internal connections.
                        return;
                    }

                    topLevelSrc.AddOutboundConnection( source, destination );
                    topLevelDst.AddInboundConnection ( source, destination );
                }
            }

            private void AddInboundConnection( AtomicSlot source      ,
                                               AtomicSlot destination )
            {
                CHECKS.ASSERT( this == destination.TopLevel, "Not an inbound connection" );

                if(FindEdge( source, destination ) == null)
                {
                    SlotEdgeFlavor flavor = SlotEdgeFlavor.Inbound;

                    if(source != source.TopLevel.LastSubSlot)
                    {
                        flavor |= SlotEdgeFlavor.SourceIsInternal;
                    }

                    if(destination != this.FirstSubSlot)
                    {
                        flavor |= SlotEdgeFlavor.DestinationIsInternal;
                    }

                    m_edges.Add( new SlotEdge( source, destination, flavor ) );
                }
            }

            private void AddOutboundConnection( AtomicSlot source      ,
                                                AtomicSlot destination )
            {
                CHECKS.ASSERT( this == source.TopLevel, "Not an outbound connection" );

                if(FindEdge( source, destination ) == null)
                {
                    SlotEdgeFlavor flavor = SlotEdgeFlavor.Outbound;

                    if(source != this.LastSubSlot)
                    {
                        flavor |= SlotEdgeFlavor.SourceIsInternal;
                    }

                    if(destination != destination.TopLevel.FirstSubSlot)
                    {
                        flavor |= SlotEdgeFlavor.DestinationIsInternal;
                    }

                    m_edges.Add( new SlotEdge( source, destination, flavor ) );
                }
            }

            internal SlotEdge FindEdge( AtomicSlot source      ,
                                        AtomicSlot destination )
            {
                foreach(var edge in m_edges)
                {
                    if(edge.m_source      == source      &&
                       edge.m_destination == destination  )
                    {
                        return edge;
                    }
                }

                return null;
            }

            internal bool HasBinaryInput( out SlotEdge edge1     ,
                                          out SlotEdge edge2     ,
                                              bool     fColdCode )
            {
                edge1 = null;
                edge2 = null;

                int count = 0;

                foreach(var edge in m_edges)
                {
                    if(edge.IsInbound)
                    {
                        if(edge.m_source.IsColdCode != fColdCode)
                        {
                            continue;
                        }

                        switch(count++)
                        {
                            case 0: edge1 = edge; break;
                            case 1: edge2 = edge; break;
                        }
                    }
                }

                return count == 2;
            }

            internal bool HasBinaryOutput( out SlotEdge edge1     ,
                                           out SlotEdge edge2     ,
                                               bool     fColdCode )
            {
                edge1 = null;
                edge2 = null;

                int count = 0;

                foreach(var edge in m_edges)
                {
                    if(edge.IsOutbound)
                    {
                        if(edge.m_destination.IsColdCode != fColdCode)
                        {
                            continue;
                        }

                        switch(count++)
                        {
                            case 0: edge1 = edge; break;
                            case 1: edge2 = edge; break;
                        }
                    }
                }

                return count == 2;
            }

            //--//

            internal abstract IEnumerable< AtomicSlot > Enumerate();

            //--//

            internal CompoundSlot Merge( Slot slot )
            {
#if DEBUG_BASICBLOCK_SCHEDULING
                if(m_owner.m_cfg.ToString() == sDebugTarget)
                {
                    Console.WriteLine( "#####################################" );
                    Console.WriteLine( "Left : {0}", this );
                    Console.WriteLine( "Right: {0}", slot );
    
                    Console.WriteLine( "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<" );
                    Console.WriteLine( "{0}", new System.Diagnostics.StackTrace() );
                    Console.WriteLine( ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" );
                }
#endif

                return new CompoundSlot( m_owner, this, slot );
            }

            internal bool IsInSet( GrowOnlySet< Slot > slots )
            {
                if(slots == null)
                {
                    return true;
                }

                Slot slot = this;

                while(slot != null)
                {
                    if(slots.Contains( slot ))
                    {
                        return true;
                    }

                    slot = slot.SuperSlot;
                }

                return false;
            }


            internal GrowOnlySet< Slot > InboundEdges( bool fBoundaryEdgesOnly )
            {
                GrowOnlySet< Slot > set = SetFactory.NewWithReferenceEquality< Slot >();

                foreach(var edge in m_edges)
                {
                    if(edge.IsInbound)
                    {
                        if(fBoundaryEdgesOnly && edge.IsBoundary == false)
                        {
                            continue;
                        }

                        set.Insert( edge.m_source );
                    }
                }

                return set;
            }

            internal GrowOnlySet< Slot > OutboundEdges( bool fBoundaryEdgesOnly )
            {
                GrowOnlySet< Slot > set = SetFactory.NewWithReferenceEquality< Slot >();

                foreach(var edge in m_edges)
                {
                    if(edge.IsOutbound)
                    {
                        if(fBoundaryEdgesOnly && edge.IsBoundary == false)
                        {
                            continue;
                        }

                        set.Insert( edge.m_destination );
                    }

                }

                return set;
            }

            internal Slot FindSingleInboundEdge( GrowOnlySet< Slot > slots              ,
                                                 bool                fSameColdness      ,
                                                 bool                fBoundaryEdgesOnly )
            {
                Slot res = null;

                foreach(var edge in m_edges)
                {
                    if(edge.IsInbound)
                    {
                        Slot slot = edge.m_source;

                        if(fSameColdness && this.IsColdCode != slot.IsColdCode)
                        {
                            continue;
                        }

                        if(slot.IsInSet( slots ))
                        {
                            if(res != null)
                            {
                                return null;
                            }

                            if(fBoundaryEdgesOnly && edge.IsBoundary == false)
                            {
                                return null;
                            }

                            res = slot;
                        }
                    }
                }

                return res;
            }

            internal Slot FindSingleOutboundEdge( GrowOnlySet<Slot> slots              ,
                                                  bool              fSameColdness      ,
                                                  bool              fBoundaryEdgesOnly )
            {
                Slot res = null;

                foreach(var edge in m_edges)
                {
                    if(edge.IsOutbound)
                    {
                        Slot slot = edge.m_destination;

                        if(fSameColdness && this.IsColdCode != slot.IsColdCode)
                        {
                            continue;
                        }

                        if(slot.IsInSet( slots ))
                        {
                            if(fBoundaryEdgesOnly && edge.IsBoundary == false)
                            {
                                return null;
                            }

                            if(res != null)
                            {
                                return null;
                            }

                            res = slot;
                        }
                    }
                }

                if(fBoundaryEdgesOnly && res != null)
                {
                    res = res.TopLevel;
                }

                return res;
            }

            //--//

            //
            // Access Methods
            //

            internal List< SlotEdge > Edges
            {
                get
                {
                    return m_edges;
                }
            }

            internal int Weight
            {
                get
                {
                    return m_weight;
                }
            }

            internal int SchedulingIndex
            {
                get
                {
                    return m_schedulingIndex;
                }

                set
                {
                    m_schedulingIndex = value;
                }
            }

            internal bool IsTopLevel
            {
                get
                {
                    return this == this.TopLevel;
                }
            }

            internal bool IsColdCode
            {
                get
                {
                    return m_owner.IsTransitivelyColdCode( this.FirstSubSlot.Target );
                }
            }

            internal Slot TopLevel
            {
                get
                {
                    return m_superSlot != null ? m_superSlot.TopLevel : this;
                }
            }

            internal CompoundSlot SuperSlot
            {
                get
                {
                    return m_superSlot;
                }

                set
                {
                    m_superSlot = value;

                    if(m_superSlot == null)
                    {
                        m_owner.RegisterTopLevel( this );
                    }
                    else
                    {
                        m_owner.UnregisterTopLevel( this );
                    }
                }
            }

            internal abstract AtomicSlot FirstSubSlot
            {
                get;
            }

            internal abstract AtomicSlot LastSubSlot
            {
                get;
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendFormat( "Slots:" );
                sb.AppendLine();

                foreach(var slot in this.Enumerate())
                {
                    foreach(var sub in slot.ToString().Split( Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries ))
                    {
                        sb.AppendFormat( "  >>  {0}{1}", sub, Environment.NewLine );
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        class CompoundSlot : Slot
        {
            //
            // State
            //

            private readonly Slot m_left;
            private readonly Slot m_right;

            //
            // Constructor Methods
            //

            internal CompoundSlot( CompilationState owner ,
                                   Slot             left  ,
                                   Slot             right ) : base( owner )
            {
                CHECKS.ASSERT( left .SuperSlot == null, "Cannot merge slot twice: {0}", left  );
                CHECKS.ASSERT( right.SuperSlot == null, "Cannot merge slot twice: {0}", right );

                m_left  = left;
                m_right = right;

                //--//

////            Console.WriteLine( "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<" );
////            Console.WriteLine( "Left {0}" , left );
////            Console.WriteLine( "----------------------------------------" );
////            Console.WriteLine( "Right {0}", right );

                //
                // Redistribute the slots.
                //
                m_left .SuperSlot = this;
                m_right.SuperSlot = this;

                m_weight = Math.Max( m_left.Weight, m_right.Weight );

                //
                // Merge the slot edges.
                //
                CopyConnections( left .Edges );
                CopyConnections( right.Edges );

////            Console.WriteLine( "########################################" );
////            Console.WriteLine( "Result {0}", res );
////            Console.WriteLine( ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" );
            }

            //
            // Helper Methods
            //

            private static void CopyConnections( List< SlotEdge > list )
            {
                foreach(var edge in list)
                {
                    AddConnection( edge );
                }
            }

            internal override IEnumerable< AtomicSlot > Enumerate()
            {
                foreach(var slot in m_left.Enumerate())
                {
                    yield return slot;
                }

                foreach(var slot in m_right.Enumerate())
                {
                    yield return slot;
                }
            }

            //
            // Access Methods
            //

            internal override AtomicSlot FirstSubSlot
            {
                get
                {
                    return m_left.FirstSubSlot;
                }
            }

            internal override AtomicSlot LastSubSlot
            {
                get
                {
                    return m_right.LastSubSlot;
                }
            }
        }


        class AtomicSlot : Slot
        {
            internal static readonly AtomicSlot[] SharedEmptyArray = new AtomicSlot[0];

            //
            // State
            //

            private readonly BitVector m_zeroLengthPredecessors;
            private readonly int       m_targetIdx;

            //
            // Constructor Methods
            //

            internal AtomicSlot( CompilationState owner     ,
                                 int              targetIdx ,
                                 int              weight    ) : base( owner )
            {
                m_zeroLengthPredecessors = new BitVector();
                m_targetIdx              = targetIdx;
                m_weight                 = weight;
            }

            //
            // Helper Methods
            //

            internal void AddInboundConnection( AtomicSlot source )
            {
                AddConnection( source, this );
            }

            internal void AddOutboundConnection( AtomicSlot destination )
            {
                AddConnection( this, destination );
            }

            internal override IEnumerable< AtomicSlot > Enumerate()
            {
                yield return this;
            }

            //
            // Access Methods
            //

            internal BitVector ZeroLengthPredecessors
            {
                get
                {
                    return m_zeroLengthPredecessors;
                }
            }

            internal override AtomicSlot FirstSubSlot
            {
                get
                {
                    return this;
                }
            }

            internal override AtomicSlot LastSubSlot
            {
                get
                {
                    return this;
                }
            }

            internal int TargetIndex
            {
                get
                {
                    return m_targetIdx;
                }
            }

            internal BasicBlock Target
            {
                get
                {
                    return m_owner.m_basicBlocks[m_targetIdx];
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendFormat( "<<<<<<<<<" );
                sb.AppendLine();
                foreach(var bbIdx in m_zeroLengthPredecessors)
                {
                    sb.AppendFormat( "PreTarget: {0}", m_owner.m_basicBlocks[bbIdx] );
                    sb.AppendLine();
                }

                sb.AppendFormat( "Target: {0}", this.Target );
                sb.AppendLine();

                sb.AppendFormat( "Weight: {0}", m_weight );
                sb.AppendLine();

                sb.AppendFormat( ">>>>>>>>>" );

                return sb.ToString();
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

        private void CollectSchedulingPreferences()
        {
#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
            }
#endif

            foreach(var bb in m_basicBlocks)
            {
                //
                // Mark exception entry code as cold.
                //
                if(bb is ExceptionHandlerBasicBlock)
                {
                    SetColdCode( bb );
                }

                if(bb.FlowControl is DeadControlOperator)
                {
                    SetColdCode( bb );
                }

                //--//

                uint size = 0;

                foreach(Operator op in bb.Operators)
                {
                    size += EmitCodeForBasicBlock_EstimateMinimumSize( op );

                    var call = op as SubroutineOperator;
                    if(call != null && call.TargetMethod.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.NoReturn ))
                    {
                        SetColdCode( bb );
                    }
                }

                if(size == 0)
                {
                    SetZeroLengthCandidate( bb );
                }
            }
        }

        //--//

        private void AllocateSchedulingSlots()
        {
#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
            }
#endif

            foreach(var bb in m_basicBlocks)
            {
                var slot = GetSchedulingSlot( bb );

                foreach(var edge in bb.Predecessors)
                {
                    var bbPrev = edge.Predecessor;

                    if(bbPrev.ShouldIncludeInScheduling( bb ))
                    {
                        var slotPrev = GetSchedulingSlot( bbPrev );

                        slot.AddInboundConnection( slotPrev );
                    }
                }

                foreach(var edge in bb.Successors)
                {
                    var bbNext = edge.Successor;

                    if(bb.ShouldIncludeInScheduling( bbNext ))
                    {
                        var slotNext = GetSchedulingSlot( bbNext );

                        slot.AddOutboundConnection( slotNext );
                    }
                }
            }

            AllocateArrayOfLoopExitSlots();
        }

        private void AllocateArrayOfLoopExitSlots()
        {
            BitVector candidates = new BitVector();

            foreach(var entry in m_naturalLoops.Loops)
            {
                candidates.OrInPlace( entry.m_exitPoints );
            }

            m_schedulingLoopExits = new AtomicSlot[candidates.Cardinality];

            int idxLoop = 0;

            foreach(var idx in candidates)
            {
                m_schedulingLoopExits[idxLoop++] = m_schedulingLookup[idx];
            }
        }

        //--//

        private void OrderBasicBlocks()
        {
#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
            }
#endif

            while(true)
            {
                if(CoalesceSlots( ToSet( m_topLevelSlots ) ))
                {
                    continue;
                }

                if(OrderBasicBlocksInLoops())
                {
                    continue;
                }

                if(OrderBasicBlocksInSet( ToSet( m_topLevelSlots ) ))
                {
                    continue;
                }

                break;
            }

#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
            }
#endif
        }

        private bool OrderBasicBlocksInLoops()
        {
            foreach(var entry in m_naturalLoops.Loops)
            {
                if(OrderBasicBlocksInLoop( entry ))
                {
                    return true;
                }
            }

            return false;
        }

        private bool OrderBasicBlocksInLoop( IR.DataFlow.ControlTree.NaturalLoops.Entry entry )
        {
            if(OrderBasicBlocksInLoop( entry, false ))
            {
                return true;
            }

            if(OrderBasicBlocksInLoop( entry, true ))
            {
                return true;
            }

            return false;
        }

        private bool OrderBasicBlocksInLoop( IR.DataFlow.ControlTree.NaturalLoops.Entry entry     ,
                                             bool                                       fColdCode )
        {
            GrowOnlySet< Slot > slots = SetFactory.NewWithReferenceEquality< Slot >();

            foreach(var idx in entry.BasicBlocks)
            {
                var slot = m_schedulingLookup[idx];

                if(slot.IsColdCode != fColdCode)
                {
                    continue;
                }

                slots.Insert( slot.TopLevel );
            }

            return OrderBasicBlocksInSet( slots );
        }

        //--//--//

        private bool OrderBasicBlocksInSet( GrowOnlySet< Slot > slots )
        {
            if(slots.Count <= 1)
            {
                return false;
            }

            if(DetectCodePattern_LoopExit( slots ))
            {
                return true;
            }

            if(DetectCodePattern_IfBlock( slots ))
            {
                return true;
            }

            if(CoalesceSlots( slots ))
            {
                return true;
            }

            return false;
        }

        //--//

        private bool DetectCodePattern_LoopExit( GrowOnlySet< Slot > slots )
        {
            if(DetectCodePattern_LoopExit( slots, true ))
            {
                return true;
            }

            if(DetectCodePattern_LoopExit( slots, false ))
            {
                return true;
            }

            return false;
        }

        private bool DetectCodePattern_LoopExit( GrowOnlySet< Slot > slots     ,
                                                 bool                fColdCode )
        {
            foreach(var slot in SortByWeight( slots, true ))
            {
                if(slot.IsColdCode != fColdCode)
                {
                    continue;
                }

                var lastSlot = slot.LastSubSlot;

                foreach(var loop in m_naturalLoops.Loops)
                {
                    if(loop.ExitPoints[lastSlot.TargetIndex])
                    {
                        foreach(var slotPre in Filter( slot.InboundEdges( false ), slots, fColdCode ))
                        {
                            slotPre.Merge( slot );

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //--//

        private bool DetectCodePattern_IfBlock( GrowOnlySet< Slot > slots )
        {
            if(DetectCodePattern_IfBlock( slots, false ))
            {
                return true;
            }

            if(DetectCodePattern_IfBlock( slots, true ))
            {
                return true;
            }

            return false;
        }

        private bool DetectCodePattern_IfBlock( GrowOnlySet< Slot > slots     ,
                                                bool                fColdCode )
        {
            foreach(var slot in SortByWeight( slots, true ))
            {
                if(slot.IsColdCode != fColdCode)
                {
                    continue;
                }

                SlotEdge edgeIf1;
                SlotEdge edgeIf2;

                if(slot.HasBinaryOutput( out edgeIf1, out edgeIf2, fColdCode ))
                {
                    Slot dst1 = edgeIf1.m_destination.TopLevel;
                    Slot dst2 = edgeIf2.m_destination.TopLevel;

                    if(TryToLinearizeIF( slot, dst1, dst2, fColdCode ))
                    {
                        return true;
                    }

                    if(TryToLinearizeIF( slot, dst2, dst1, fColdCode ))
                    {
                        return true;
                    }

                    if(TryToLinearizeIFTHENELSE( slots, slot, dst1, dst2, fColdCode ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryToLinearizeIF( Slot slotA     ,
                                       Slot slotB     ,
                                       Slot slotC     ,
                                       bool fColdCode )
        {
            SlotEdge edge1;
            SlotEdge edge2;

            if(slotC.HasBinaryInput( out edge1, out edge2, fColdCode ))
            {
                if(edge1.m_source.TopLevel == slotA && edge2.m_source.TopLevel == slotB)
                {
                    slotA.Merge( slotB );

                    return true;
                }
            }

            return false;
        }

        private bool TryToLinearizeIFTHENELSE( GrowOnlySet< Slot > slots     ,
                                               Slot                slotIf    ,
                                               Slot                slotThen  ,
                                               Slot                slotElse  ,
                                               bool                fColdCode )
        {
            Slot slotThenOut = slotThen.FindSingleOutboundEdge( slots, true, false );
            Slot slotElseOut = slotElse.FindSingleOutboundEdge( slots, true, false );

            if(slotThenOut != null && slotElseOut != null)
            {
                slotThenOut = slotThenOut.TopLevel;
                slotElseOut = slotElseOut.TopLevel;

                if(slotThenOut == slotElseOut)
                {
                    //
                    // BUGBUG: Select which way to merge the if/then/else block: if/then/else or if/else/then??
                    //

                    slotIf.Merge( slotThen );

                    return true;
                }
            }

            return false;
        }

        //--//--//

        private bool CoalesceSlots( GrowOnlySet< Slot > slots )
        {
            foreach(var slot in slots)
            {
                var slotNext = slot.FindSingleOutboundEdge( slots, true, true );

                if(slotNext != null && slotNext.FindSingleInboundEdge( slots, true, true ) == slot)
                {
                    //
                    // Only merge slots with the same "coldness" flavor.
                    //
                    if(slot.IsColdCode == slotNext.IsColdCode)
                    {
                        slot.Merge( slotNext );

                        return true;
                    }
                }
            }

            return false;
        }

        //--//--//

        private void AssignSchedulingIndex()
        {
#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
                Console.WriteLine( "Start Scheduling for {0}:", m_cfg );
                Console.WriteLine();
        
                foreach(var slotTopLevel in m_topLevelSlots)
                {
                    Console.WriteLine( "#######" );
                    Console.WriteLine( "{0}", slotTopLevel );
                }
        
                Console.WriteLine( "-------------------" );
            }
#endif

            int index = 0;

            index = AssignSchedulingIndex( index, false );
            index = AssignSchedulingIndex( index, true  );


#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
                Console.WriteLine( "Completed Scheduling for {0}:", m_cfg );
                Console.WriteLine();
        
                Slot[] slots = ArrayUtility.CopyNotNullArray( m_topLevelSlots.ToArray() );
        
                Array.Sort( slots, (x, y) => x.FirstSubSlot.SchedulingIndex.CompareTo( y.FirstSubSlot.SchedulingIndex ) );
        
                foreach(var slotTopLevel in slots)
                {
                    Console.WriteLine( "#######" );
                    Console.WriteLine( "{0}", slotTopLevel );
                }
        
                Console.WriteLine( "-------------------" );
            }
#endif
        }

        private int AssignSchedulingIndex( int  index     ,
                                           bool fColdCode )
        {
#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
            }
#endif

            GrowOnlySet< Slot > visited = SetFactory.NewWithReferenceEquality< Slot>();

            foreach(var slot in m_schedulingLookup)
            {
                index = AssignSchedulingIndex( index, fColdCode, slot, visited );
            }

            return index;
        }

        private int AssignSchedulingIndex( int                 index     ,
                                           bool                fColdCode ,
                                           Slot                slot      ,
                                           GrowOnlySet< Slot > visited   )
        {
            var slotTopLevel = slot.TopLevel;

            if(fColdCode == slotTopLevel.IsColdCode)
            {
                bool fSchedule = true;

                foreach(var edge in slotTopLevel.Edges)
                {
                    if(edge.IsInbound)
                    {
                        if(visited.Contains( edge.m_source.TopLevel ) == false)
                        {
                            fSchedule = false;
                        }
                    }
                }

                //
                // Only assign a scheduling number after all the inbound edges have been visited.
                //
                if(fSchedule)
                {
                    foreach(var slotSub in slotTopLevel.Enumerate())
                    {
                        if(slotSub.SchedulingIndex < 0)
                        {
                            slotSub.SchedulingIndex = index++;
                        }
                    }
                }
            }

            if(visited.Insert( slotTopLevel ) == false)
            {
                foreach(var edge in slotTopLevel.Edges)
                {
                    if(edge.IsOutbound)
                    {
                        index = AssignSchedulingIndex( index, fColdCode, edge.m_destination, visited );
                    }
                }
            }

            return index;
        }

        //--//

        public void SetZeroLengthCandidate( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            m_vecZeroLengthCandidates.Set( bb.SpanningTreeIndex );
        }
    
        public bool IsZeroLengthCandidate( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return m_vecZeroLengthCandidates[bb.SpanningTreeIndex];
        }
    
        public void SetColdCode( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            m_vecColdCode.Set( bb.SpanningTreeIndex );
        }

        public bool IsColdCode( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return m_vecColdCode[bb.SpanningTreeIndex];
        }

        public bool IsReachableFromColdCode( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return m_vecReachableFromColdCode[bb.SpanningTreeIndex];
        }

        public bool IsReachableFromEntry( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return m_vecReachableFromEntry[bb.SpanningTreeIndex];
        }

        public bool IsTransitivelyColdCode( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return IsReachableFromColdCode( bb ) && !IsReachableFromEntry( bb );
        }
    
        public int SchedulingIndex( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return GetSchedulingSlot( bb ).SchedulingIndex;
        }

        public SequentialRegion GetRegion( BasicBlock bb )
        {
            CHECKS.ASSERT( m_cfg == bb.Owner, "'{0}' does not belong to '{1}'", bb, m_cfg );

            return m_basicBlockToRegions[bb.SpanningTreeIndex];
        }

        //--//

        private AtomicSlot GetSchedulingSlot( BasicBlock bb )
        {
            int        bbIdx = bb.SpanningTreeIndex;
            AtomicSlot slot  = m_schedulingLookup[bbIdx];

            if(slot == null)
            {
                if(m_vecZeroLengthCandidates[bbIdx])
                {
                    foreach(var edge in bb.Successors)
                    {
                        var bbNext = edge.Successor;

                        if(bbNext is ExceptionHandlerBasicBlock)
                        {
                            // Only consider normal basic blocks.
                        }
                        else
                        {
                            slot = GetSchedulingSlot( bbNext );

                            slot.ZeroLengthPredecessors.Set( bbIdx );
                            break;
                        }
                    }
                }

                if(slot == null)
                {
                    //
                    // Bias the slot based on hot/cold split.
                    //
                    int weight = IsTransitivelyColdCode( bb ) ? -1000000 : 1000000;

                    //
                    // The ordering algorithm is based on dominance, so it works backwards.
                    // Adding the spanning tree index position skews the weights in the right way (exit before entry).
                    //
                    weight += bbIdx;

                    //
                    // Finally multiply the loop depth position by the size of the spanning tree, to avoid interfering with the previous point.
                    //
                    weight += m_basicBlocks.Length * (int)Math.Pow( 8, m_naturalLoops.GetDepthOfBasicBlock( bb ) );

                    slot = new AtomicSlot( this, bbIdx, weight );

                }

                m_schedulingLookup[bb.SpanningTreeIndex] = slot;
            }

            return slot;
        }

        private void RegisterTopLevel( Slot slot )
        {
            m_topLevelSlots.Add( slot );
        }

        private void UnregisterTopLevel( Slot slot )
        {
            m_topLevelSlots.Remove( slot );
        }

        //--//

        private List< Slot > ToList( IEnumerable< Slot > collection )
        {
            List< Slot > lst = new List< Slot >();

            foreach(var slot in collection)
            {
                lst.Add( slot );
            }

            return lst;
        }
        
        private GrowOnlySet< Slot > ToSet( IEnumerable< Slot > collection )
        {
            GrowOnlySet< Slot > set = SetFactory.NewWithReferenceEquality< Slot >();

            foreach(var slot in collection)
            {
                set.Insert( slot );
            }

            return set;
        }

        private IEnumerable< Slot > Filter( GrowOnlySet< Slot > set       ,
                                            GrowOnlySet< Slot > slots     ,
                                            bool                fColdCode )
        {
            return Filter( set.ToArray(), slots, fColdCode );
        }

        private IEnumerable< Slot > Filter( IEnumerable< Slot > collection ,
                                            GrowOnlySet< Slot > slots      ,
                                            bool                fColdCode  )
        {
            foreach(var slot in collection)
            {
                if(slot.IsColdCode == fColdCode)
                {
                    if(slot.IsInSet( slots ))
                    {
                        yield return slot.TopLevel;
                    }
                }
            }
        }

        private IEnumerable< Slot > SortByWeight( GrowOnlySet< Slot > set ,
                                                  bool                fUp )
        {
            return SortByWeight( set.ToArray(), fUp );
        }

        private IEnumerable< Slot > SortByWeight( IEnumerable< Slot > collection ,
                                                  bool                fUp        )
        {
            List< Slot > lst = ToList( collection );

            if(fUp)
            {
                lst.Sort( (x, y) => x.Weight.CompareTo( y.Weight ) );
            }
            else
            {
                lst.Sort( (x, y) => y.Weight.CompareTo( x.Weight ) );
            }

            return lst;
        }

        private IEnumerable< Slot > EnumerateLoopExits()
        {
            return m_schedulingLoopExits;
        }

        //--//

        internal void AssignAbsoluteAddresses( bool fIncludeColdCode )
        {
            AtomicSlot[] slots = ArrayUtility.CopyNotNullArray( m_schedulingLookup );

            Array.Sort( slots, (x, y) => x.SchedulingIndex.CompareTo( y.SchedulingIndex ) );

#if DEBUG_BASICBLOCK_SCHEDULING
            if(m_cfg.ToString() == sDebugTarget)
            {
            }
#endif

            foreach(var slot in slots)
            {
                SequentialRegion reg = GetRegion( slot.Target );
                if(reg.IsBaseAddressAssigned == false)
                {
                    if(fIncludeColdCode || !IsTransitivelyColdCode( slot.Target ))
                    {
                        if(reg.Size == 0)
                        {
                            //
                            // We cannot have a zero-length slot, because it will be aliased to random places.
                            // This can cause malfunctions during stack walking,
                            // because the stack unwinder will think it landed in a different method.
                            // We need to add a bit of garbage to make the stack crawler happy.
                            //
                            FixupEmptyRegion( reg );
                        }

                        reg.AssignAbsoluteAddress();

                        foreach(var bbIdx2 in slot.ZeroLengthPredecessors)
                        {
                            SequentialRegion reg2 = m_basicBlockToRegions[bbIdx2];

                            if(reg2.IsBaseAddressAssigned == false)
                            {
                                reg2.BaseAddress = reg.BaseAddress;
                            }
                        }

                        m_owner.PushRegionConstantsToPendingList( reg );
                    }
                }
            }
        }
    }
}
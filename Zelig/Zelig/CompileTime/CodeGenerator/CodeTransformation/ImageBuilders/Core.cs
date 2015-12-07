//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define ARMv7M_BUILD__LLVM_IR_ONLY


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using Microsoft.Zelig.CodeGeneration.IR.CompilationSteps;


    public sealed class Core
    {
        public delegate bool SequentialRegionEnumerationCallback( SequentialRegion reg );

        public delegate bool ImageAnnotationEnumerationCallback( ImageAnnotation an );

        //--//

        public enum BranchEncodingLevel
        {
            //
            // Don't emit any branch, the source and destination are adjacent.
            //
            Skip,

            //
            // Single-opcode encoding for branch.
            //
            // Example:
            //
            //   BR #rel-address
            //
            ShortBranch,

            //
            // Single-opcode indirect jump.
            //
            // Example:
            //
            //   LDR PC, [PC,#const]
            //
            NearRelativeLoad,

            //
            // Multi-opcode indirect jump.
            //
            // Example:
            //
            //   MOV R12, #const1
            //   OR  R12, #const2
            //   ...
            //   LDR PC, [PC,R12]
            //
            FarRelativeLoad,

            //--//

            Max = FarRelativeLoad,
        }

        public enum ConstantAddressEncodingLevel
        {
            //
            // The constant is encoded in the opcode.
            //
            // Example:
            //
            //   MOV <reg>,#const
            //
            Immediate,

            //
            // The constant is encoded in the opcode.
            //
            // Example:
            //
            //   MOV <reg>,#const1
            //   OR  <reg>,<reg>,#const2
            //
            SmallLoad,

            //
            // Single-opcode indirect jump.
            //
            // Example:
            //
            //   LDR <reg>, [PC,#const]
            //
            NearRelativeLoad,

            //
            // Multi-opcode indirect load.
            //
            // Example:
            //
            //   MOV R12, #const1
            //   OR  R12, #const2
            //   ...
            //   LDR <reg>, [PC,R12]
            //
            FarRelativeLoad16Bit,
            FarRelativeLoad24Bit,
            FarRelativeLoad32Bit,

            //--//

            Max = FarRelativeLoad32Bit,
        }

        //--//

        class MemoryRange
        {
            //
            // State
            //

            List< byte[] > m_chunks;

            //
            // Constructor Methods
            //

            internal MemoryRange( )
            {
                m_chunks = new List<byte[]>( );
            }

            internal MemoryRange( byte[] chunk )
                : this( )
            {
                Append( chunk );
            }

            //
            // Helper Methods
            //

            internal void Rewind( int length )
            {
                for( int pos = m_chunks.Count; --pos >= 0; )
                {
                    byte[] chunk    = m_chunks[ pos ];
                    int    chunkLen = chunk.Length;

                    if( chunkLen <= length )
                    {
                        m_chunks.RemoveAt( pos );

                        length -= chunkLen;

                        if( length <= 0 )
                        {
                            break;
                        }
                    }
                    else
                    {
                        m_chunks[ pos ] = ArrayUtility.ExtractSliceFromNotNullArray( chunk, 0, chunkLen - length );

                        break;
                    }
                }
            }

            internal void Append( byte[] chunk )
            {
                m_chunks.Add( chunk );
            }

            internal bool Match( int offset,
                                 byte[] payload,
                                 int payloadOffset,
                                 int length )
            {
                int offsetEnd  = offset + length;
                int chunkStart = 0;

                foreach( byte[] chunk in m_chunks )
                {
                    int chunkLen = chunk.Length;
                    int chunkEnd = chunkStart + chunkLen;

                    int matchStart = Math.Max( chunkStart, offset );
                    int matchEnd   = Math.Min( chunkEnd, offsetEnd );

                    //
                    // Is there any overlap?
                    //
                    if( matchStart < matchEnd )
                    {
                        if( ArrayUtility.ByteArrayEquals( chunk, matchStart - chunkStart, payload, payloadOffset + ( matchStart - offset ), matchEnd - matchStart ) == false )
                        {
                            return false;
                        }
                    }

                    chunkStart += chunkLen;
                }

                return true;
            }

            internal byte[] ToArray( )
            {
                int size = 0;

                foreach( byte[] chunk in m_chunks )
                {
                    size += chunk.Length;
                }

                byte[] res    = new byte[ size ];
                int    offset = 0;

                foreach( byte[] chunk in m_chunks )
                {
                    Array.Copy( chunk, 0, res, offset, chunk.Length );

                    offset += chunk.Length;
                }

                return res;
            }
        }

        class SortedLinkerHeap
        {
            internal class HeapRange
            {
                //
                // State
                //

                internal UIntPtr                  Start;
                internal UIntPtr                  End;
                internal string                   SectionName;
                internal Runtime.MemoryAttributes Attributes;
                internal Runtime.MemoryUsage      Usage;
                internal Type                     ExtensionHandler;

                //
                // Constructor Methods
                //

                internal HeapRange( UIntPtr start,
                                    UIntPtr end,
                                    string sectionName,
                                    Runtime.MemoryAttributes attributes,
                                    Runtime.MemoryUsage usage,
                                    Type extensionHandler )
                {
                    this.Start = start;
                    this.End = end;
                    this.SectionName = sectionName;
                    this.Attributes = attributes;
                    this.Usage = usage;
                    this.ExtensionHandler = extensionHandler;
                }

                //
                // Helper Methods
                //

                internal HeapRange Clone( )
                {
                    return new HeapRange( this.Start, this.End, this.SectionName, this.Attributes, this.Usage, this.ExtensionHandler );
                }

                internal HeapRange SliceAsAllocated( UIntPtr ptrStart,
                                                     UIntPtr ptrEnd )
                {
                    return new HeapRange( ptrStart, ptrEnd, this.SectionName, this.Attributes | Runtime.MemoryAttributes.Allocated, this.Usage, this.ExtensionHandler );
                }

                internal HeapRange SliceAsNotAllocated( UIntPtr ptrStart,
                                                        UIntPtr ptrEnd )
                {
                    return new HeapRange( ptrStart, ptrEnd, this.SectionName, this.Attributes & ~Runtime.MemoryAttributes.Allocated, this.Usage, this.ExtensionHandler );
                }

                internal Runtime.Memory.Range ToMemoryRange( )
                {
                    return new Runtime.Memory.Range( this.Start, this.End, this.SectionName, this.Attributes, this.Usage, this.ExtensionHandler );
                }

                internal static HeapRange FromMemoryRange( Runtime.Memory.Range rng )
                {
                    return new HeapRange( rng.Start, rng.End, rng.SectionName, rng.Attributes & ~Runtime.MemoryAttributes.Allocated, rng.Usage, rng.ExtensionHandler );
                }
            }

            //
            // State
            //

            internal List< HeapRange > m_heap;

            //
            // Constructor Methods
            //

            internal SortedLinkerHeap( List<Runtime.Memory.Range> memoryBlocks )
            {
                m_heap = new List<HeapRange>( );

                ImportBlocks( memoryBlocks );
            }

            internal SortedLinkerHeap( SortedLinkerHeap other )
            {
                m_heap = new List<HeapRange>( );

                foreach( HeapRange rng in other.m_heap )
                {
                    m_heap.Add( rng.Clone( ) );
                }
            }

            //
            // Helper Methods
            //

            internal List<Runtime.Memory.Range> Commit( )
            {
                List< Runtime.Memory.Range > lst = new List<Runtime.Memory.Range>( );

                foreach( HeapRange heapRng in m_heap )
                {
                    lst.Add( heapRng.ToMemoryRange( ) );
                }

                return lst;
            }

            internal void RebuildFromRegions( List<Runtime.Memory.Range> memoryBlocks,
                                              SequentialRegion[] regions )
            {
                m_heap.Clear( );

                ImportBlocks( memoryBlocks );

                //--//

                SequentialRegion firstRegion = null;
                SequentialRegion lastRegion  = null;

                foreach( SequentialRegion reg in regions )
                {
                    if( firstRegion == null )
                    {
                        firstRegion = reg;
                    }
                    else if( AddressMath.IsGreaterThanOrEqual( lastRegion.EndAddress, reg.BaseAddress ) == false )
                    {
                        FixupSpan( firstRegion.BaseAddress, lastRegion.EndAddress );

                        firstRegion = reg;
                    }

                    lastRegion = reg;
                }

                if( firstRegion != null )
                {
                    FixupSpan( firstRegion.BaseAddress, lastRegion.EndAddress );
                }
            }

            private void FixupSpan( UIntPtr regStart, UIntPtr regEnd )
            {
                for( int pos = 0; pos < m_heap.Count; pos++ )
                {
                    HeapRange                rng     = m_heap[ pos ];
                    Runtime.MemoryAttributes rngKind = rng.Attributes;

                    if( ( rngKind & Runtime.MemoryAttributes.Allocated ) == 0 )
                    {
                        UIntPtr ptrStart = AddressMath.Max( rng.Start, regStart );
                        UIntPtr ptrEnd   = AddressMath.Min( rng.End, regEnd );

                        if( AddressMath.IsLessThan( ptrStart, ptrEnd ) )
                        {
                            ExtractChunk( pos, rng, ptrStart, ptrEnd );

                            //
                            // Restart the loop, in case the region spans multiple free heap clusters.
                            //
                            pos = -1;
                            continue;
                        }
                    }
                }
            }

            internal bool Allocate( uint size,
                                        uint offset,
                                        Abstractions.PlacementRequirements pr,
                                    out UIntPtr address )
            {
                for( int pos = 0; pos < m_heap.Count; pos++ )
                {
                    HeapRange rng = m_heap[ pos ];

                    if( ( rng.Attributes & Runtime.MemoryAttributes.Allocated ) == 0 )
                    {
                        if(pr.IsCompatible( rng.SectionName, rng.Attributes, rng.Usage ) )
                        {
                            //
                            // Chunk is free and compatible.
                            //

                            uint    offsetForAlignment = ( uint )( offset + pr.AlignmentOffset );
                            uint    sizeForAlignment   =        size - offsetForAlignment;
                            UIntPtr ptrStart;
                            UIntPtr ptrEnd;

                            if( pr.AllocateFromHighAddress )
                            {
                                ptrStart = AddressMath.Decrement( rng.End, sizeForAlignment );

                                ptrStart = AddressMath.AlignToBoundary( ptrStart, pr.Alignment );

                                ptrEnd = AddressMath.Increment( ptrStart, sizeForAlignment );

                                //
                                // AlignToBoundary might move the block past the end of the range, try to adjust for this.
                                //
                                if( AddressMath.IsGreaterThan( ptrEnd, rng.End ) )
                                {
                                    ptrStart = AddressMath.Decrement( ptrStart, pr.Alignment );
                                    ptrEnd = AddressMath.Decrement( ptrEnd, pr.Alignment );
                                }
                            }
                            else
                            {
                                ptrStart = AddressMath.Increment( rng.Start, offsetForAlignment );

                                ptrStart = AddressMath.AlignToBoundary( ptrStart, pr.Alignment );

                                ptrEnd = AddressMath.Increment( ptrStart, sizeForAlignment );
                            }

                            if( AddressMath.IsGreaterThanOrEqual( ptrStart, rng.Start ) &&
                               AddressMath.IsLessThanOrEqual( ptrEnd, rng.End ) )
                            {
                                //
                                // Found a fit. Update the heap state.
                                //
                                address = AddressMath.Decrement( ptrStart, offsetForAlignment );

                                ExtractChunk( pos, rng, address, ptrEnd );

                                return true;
                            }
                        }
                    }
                }

                address = new UIntPtr( );

                return false;
            }

            //--//

            private void ImportBlocks( List<Runtime.Memory.Range> memoryBlocks )
            {
                foreach( var rng in memoryBlocks )
                {
                    HeapRange heapRange = HeapRange.FromMemoryRange( rng );
                    int       pos       = 0;

                    while( pos < m_heap.Count )
                    {
                        HeapRange heapRange2 = m_heap[ pos ];

                        if( AddressMath.IsLessThan( heapRange.Start, heapRange2.Start ) )
                        {
                            break;
                        }

                        pos++;
                    }

                    m_heap.Insert( pos, heapRange );
                }
            }

            private void ExtractChunk( int pos,
                                       HeapRange rng,
                                       UIntPtr ptrStart,
                                       UIntPtr ptrEnd )
            {
                CHECKS.ASSERT( AddressMath.IsLessThanOrEqual( rng.Start, ptrStart ), "The start of the target range [{0:X8}-{1:X8}] does not belong to the chunk [{2:X8}-{3:X8}]", ptrStart.ToUInt32( ), ptrEnd.ToUInt32( ), rng.Start.ToUInt32( ), rng.End.ToUInt32( ) );
                CHECKS.ASSERT( AddressMath.IsGreaterThanOrEqual( rng.End, ptrEnd ), "The end of the target range [{0:X8}-{1:X8}] does not belong to the chunk [{2:X8}-{3:X8}]", ptrStart.ToUInt32( ), ptrEnd.ToUInt32( ), rng.Start.ToUInt32( ), rng.End.ToUInt32( ) );

                HeapRange                rngPrev        = CanMergeWithPrevious( pos, rng );
                HeapRange                rngNext        = CanMergeWithNext( pos, rng );
                bool                     fFlushToBottom = ( ptrStart == rng.Start );
                bool                     fFlushToTop    = ( ptrEnd == rng.End );

                if( fFlushToBottom && fFlushToTop )
                {
                    //
                    // Convert the whole chunk to allocated.
                    //
                    rng.Attributes |= Runtime.MemoryAttributes.Allocated;

                    if( rngNext != null )
                    {
                        rng.End = rngNext.End;

                        m_heap.RemoveAt( pos + 1 );
                    }

                    if( rngPrev != null )
                    {
                        rngPrev.End = rng.End;

                        m_heap.RemoveAt( pos );
                    }
                }
                else if( fFlushToBottom )
                {
                    if( rngPrev != null )
                    {
                        //
                        // Update the boundary between allocated/unallocated chunks.
                        //
                        rngPrev.End = ptrEnd;
                        rng.Start = ptrEnd;
                    }
                    else
                    {
                        HeapRange newAllocatedRng = rng.SliceAsAllocated( ptrStart, ptrEnd );

                        m_heap.Insert( pos, newAllocatedRng );

                        rng.Start = ptrEnd;
                    }
                }
                else if( fFlushToTop )
                {
                    if( rngNext != null )
                    {
                        //
                        // Update the boundary between unallocated/allocated chunks.
                        //
                        rngNext.Start = ptrStart;
                        rng.End = ptrStart;
                    }
                    else
                    {
                        HeapRange newAllocatedRng = rng.SliceAsAllocated( ptrStart, ptrEnd );

                        m_heap.Insert( pos + 1, newAllocatedRng );

                        rng.End = ptrStart;
                    }
                }
                else
                {
                    HeapRange newAllocatedRng   = rng.SliceAsAllocated( ptrStart, ptrEnd );
                    HeapRange newUnallocatedRng = rng.SliceAsNotAllocated( ptrEnd, rng.End );

                    m_heap.Insert( pos + 1, newAllocatedRng );
                    m_heap.Insert( pos + 2, newUnallocatedRng );

                    rng.End = ptrStart;
                }
            }

            private HeapRange CanMergeWithPrevious( int pos,
                                                    HeapRange rng )
            {
                if( pos > 0 )
                {
                    HeapRange rngPrev = m_heap[ pos - 1 ];

                    if( rngPrev.End == rng.Start &&
                       rngPrev.SectionName == rng.SectionName &&
                       rngPrev.Attributes == ( rng.Attributes | Runtime.MemoryAttributes.Allocated ) &&
                       rngPrev.Usage == rng.Usage )
                    {
                        return rngPrev;
                    }
                }

                return null;
            }

            private HeapRange CanMergeWithNext( int pos,
                                                HeapRange rng )
            {
                if( pos + 1 < m_heap.Count )
                {
                    HeapRange rngNext = m_heap[ pos + 1 ];

                    if( rngNext.Start == rng.End &&
                       rngNext.SectionName == rng.SectionName &&
                       rngNext.Attributes == ( rng.Attributes | Runtime.MemoryAttributes.Allocated ) &&
                       rngNext.Usage == rng.Usage )
                    {
                        return rngNext;
                    }
                }

                return null;
            }
        }

        //--//

        //
        // State
        //

        private TypeSystemForCodeTransformation                                                               m_typeSystem;
        private List             <                                             Runtime.Memory.Range         > m_memoryBlocks;
        private List             <                                             Runtime.Memory.Range         > m_availableMemory;
        private SortedLinkerHeap                                                                              m_sortedLinkerHeapForCode;
        private SortedLinkerHeap                                                                              m_sortedLinkerHeap;

        private GrowOnlyHashTable< ControlFlowGraphStateForCodeTransformation , CompilationState            > m_methodCompilationState;
        private GrowOnlyHashTable< object                                     , List< CodeConstant >        > m_codeConstants;
        private GrowOnlyHashTable< DataManager.DataDescriptor                 , SequentialRegion            > m_dataDescriptors;
        private GrowOnlyHashTable<ExternalDataDescriptor.IExternalDataContext , SequentialRegion            > m_externalDataRegions;

        private CodeMap.ReverseIndex[]                                                                        m_reverseCodeMapIndex;

        private SourceCodeTracker                                                                             m_sourceCodeTracker;

        //                                                                                          
        // These fields are not persisted.                                                          
        //                                                                                          

        private GrowOnlyHashTable< Operator                                  , BranchEncodingLevel          > m_relocationConstraintsForUnconditionalBranches;
        private GrowOnlyHashTable< Operator                                  , BranchEncodingLevel          > m_relocationConstraintsForConditionalBranches;
        private GrowOnlyHashTable< Operator                                  , ConstantAddressEncodingLevel > m_relocationConstraintsForConstants;

        private Queue            <                                             CompilationState             > m_pendingCode;
        private Queue            <                                             DataManager.DataDescriptor   > m_pendingDataDescriptors;
        private Queue            <                                             DataManager.DataDescriptor   > m_pendingExternalDataDescriptors;

        private GrowOnlyHashTable< Operator                                  , List< BasicBlock   >         > m_adjacencyNeeds;
        private GrowOnlyHashTable< SequentialRegion                          , List< CodeConstant >         > m_regionToCodeConstants;
        private List             <                                             CodeConstant                 > m_pendingConstants;

        private InstructionSet                                                                                m_encoder;

        //
        // Constructor Methods
        //

        private Core( ) // Default constructor required by TypeSystemSerializer.
        {
            m_memoryBlocks = new List<Runtime.Memory.Range>( );
            m_availableMemory = new List<Runtime.Memory.Range>( );

            m_methodCompilationState = HashTableFactory.NewWithReferenceEquality<ControlFlowGraphStateForCodeTransformation, CompilationState>( );
            m_codeConstants = HashTableFactory.New<object, List<CodeConstant>>( );
            m_dataDescriptors = HashTableFactory.NewWithReferenceEquality<DataManager.DataDescriptor, SequentialRegion>( );
            m_externalDataRegions = HashTableFactory.New<ExternalDataDescriptor.IExternalDataContext, SequentialRegion>( );

            m_sourceCodeTracker = new SourceCodeTracker( );

            m_relocationConstraintsForUnconditionalBranches = HashTableFactory.NewWithReferenceEquality<Operator, BranchEncodingLevel>( );
            m_relocationConstraintsForConditionalBranches = HashTableFactory.NewWithReferenceEquality<Operator, BranchEncodingLevel>( );
            m_relocationConstraintsForConstants = HashTableFactory.NewWithReferenceEquality<Operator, ConstantAddressEncodingLevel>( );

            m_pendingCode = new Queue<CompilationState>( );
            m_pendingDataDescriptors = new Queue<DataManager.DataDescriptor>( );
            m_pendingExternalDataDescriptors = new Queue<DataManager.DataDescriptor>( );

            m_adjacencyNeeds = HashTableFactory.NewWithReferenceEquality<Operator, List<BasicBlock>>( );
            m_regionToCodeConstants = HashTableFactory.NewWithReferenceEquality<SequentialRegion, List<CodeConstant>>( );
            m_pendingConstants = new List<CodeConstant>( );
        }

        internal Core( TypeSystemForCodeTransformation typeSystem )
            : this( )
        {
            m_typeSystem = typeSystem;

            SourceCodeTracker sct = typeSystem.GetSourceCodeTracker( );
            if( sct != null )
            {
                m_sourceCodeTracker.Merge( sct );
            }
        }

        //
        // Helper Methods
        //

        public void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            CHECKS.ASSERT( m_pendingCode.Count == 0, "Cannot apply transformation to ImageBuilder while there are pending compilations" );
            CHECKS.ASSERT( m_pendingDataDescriptors.Count == 0, "Cannot apply transformation to ImageBuilder while there are pending compilations" );

            context.Push( this );

            context.Transform( ref m_typeSystem );
            context.Transform( ref m_memoryBlocks );
            context.Transform( ref m_availableMemory );

            context.Transform( ref m_methodCompilationState );
            context.Transform( ref m_codeConstants );
            context.Transform( ref m_dataDescriptors );
            context.Transform( ref m_externalDataRegions );
            context.TransformGeneric( ref m_sourceCodeTracker );

            context.TransformGeneric( ref m_reverseCodeMapIndex );

            context.Pop( );
        }

        //--//

        internal void DropCompileTimeObjects( )
        {
            foreach( var reg in this.SortedRegions )
            {
                var lst = reg.AnnotationList;

                for( int i = 0; i < lst.Count; i++ )
                {
                    var an = lst[ i ];

                    if( an.IsCompileTimeAnnotation )
                    {
                        lst.RemoveAt( i-- );
                    }
                }
            }
        }

        //--//

        public InstructionSet GetInstructionSetProvider( )
        {
            if( m_encoder == null )
            {
                m_encoder = m_typeSystem.PlatformAbstraction.GetInstructionSetProvider( );
            }

            return m_encoder;
        }

        //--//

        public CompilationState TryToGetCompilationState( BasicBlock bb )
        {
            return TryToGetCompilationState( ( ControlFlowGraphStateForCodeTransformation )bb.Owner );
        }

        public CompilationState GetCompilationState( BasicBlock bb )
        {
            return GetCompilationState( ( ControlFlowGraphStateForCodeTransformation )bb.Owner );
        }

        //--//

        public CompilationState TryToGetCompilationState( ControlFlowGraphStateForCodeTransformation cfg )
        {
            CompilationState res;

            m_methodCompilationState.TryGetValue( cfg, out res );

            return res;
        }

        public CompilationState GetCompilationState( ControlFlowGraphStateForCodeTransformation cfg )
        {
            CompilationState res = TryToGetCompilationState( cfg );

            if( res == null )
            {
                throw TypeConsistencyErrorException.Create( "Unexpected reference to method '{0}'", cfg.Method.ToShortString( ) );
            }

            return res;
        }

        //--//

        public SequentialRegion AddObject( DataManager.DataDescriptor dd )
        {
            SequentialRegion reg;

            if( m_dataDescriptors.TryGetValue( dd, out reg ) == false )
            {
                reg = new SequentialRegion( this, dd, m_typeSystem.PlatformAbstraction.GetMemoryRequirements( dd ) );

                m_dataDescriptors[ dd ] = reg;

                m_pendingDataDescriptors.Enqueue( dd );
            }

            return reg;
        }

        public SequentialRegion AddExternalData( ExternalDataDescriptor.IExternalDataContext ctx, uint size )
        {
            SequentialRegion reg;

            if( m_externalDataRegions.TryGetValue( ctx, out reg ) == false )
            {
                ExternalDataDescriptor edd = new ExternalDataDescriptor( m_typeSystem.DataManagerInstance, ctx, DataManager.Attributes.Mutable, null );

                reg = new SequentialRegion( this, edd, m_typeSystem.PlatformAbstraction.GetMemoryRequirements( edd ) );

                m_externalDataRegions[ ctx ] = reg;

                m_pendingExternalDataDescriptors.Enqueue( edd );
            }

            return reg;
        }

        //--//

        internal bool ProcessPending( )
        {
            bool fNoNewMethod = true;

            while( true )
            {
                if( m_pendingCode.Count > 0 )
                {
                    CompilationState cs = m_pendingCode.Dequeue( );

                    cs.CompileMethod( );

                    fNoNewMethod = false;
                    continue;
                }

                if( m_pendingDataDescriptors.Count > 0 )
                {
                    DataManager.DataDescriptor dd = m_pendingDataDescriptors.Dequeue( );

#if ARMv7M_BUILD__LLVM_IR_ONLY
                    if( TypeSystem.PlatformAbstraction.PlatformName != "LLVM" )
#endif
                    {
                        dd.Write( m_dataDescriptors[ dd ] );
                    }

                    continue;
                }

                if( m_pendingExternalDataDescriptors.Count > 0 )
                {
                    ExternalDataDescriptor dd = ( ExternalDataDescriptor )m_pendingExternalDataDescriptors.Dequeue( ); 
                    
#if ARMv7M_BUILD__LLVM_IR_ONLY
                    if( TypeSystem.PlatformAbstraction.PlatformName != "LLVM" )
#endif
                    {
                        dd.Write( m_externalDataRegions[ dd.ExternContext ] );
                    }

                    continue;
                }

                break;
            }

            return fNoNewMethod;
        }

        //--//

        internal void RestartCompilation( )
        {
            foreach( var cs in m_methodCompilationState.Values )
            {
                cs.Dispose( );
            }

            //--//

            // reset the import data sections
            ImplementExternalMethods.ResetExternalDataDescriptors( );

            m_methodCompilationState.Clear( );
            m_codeConstants.Clear( );
            m_dataDescriptors.Clear( );
            m_externalDataRegions.Clear( );

            m_pendingCode.Clear( );
            m_pendingDataDescriptors.Clear( );
            m_pendingExternalDataDescriptors.Clear( );

            m_adjacencyNeeds.Clear( );
            m_regionToCodeConstants.Clear( );
            m_pendingConstants.Clear( );

            m_sortedLinkerHeapForCode = new SortedLinkerHeap( m_memoryBlocks );

            //--//

            m_sortedLinkerHeap = m_sortedLinkerHeapForCode;
        }

        public void CompileBasicBlock( BasicBlock bb )
        {
            CompileMethod( ( ControlFlowGraphStateForCodeTransformation )bb.Owner );
        }

        public void CompileMethod( ControlFlowGraphStateForCodeTransformation cfg )
        {
            if( cfg != null && m_methodCompilationState.ContainsKey( cfg ) == false )
            {
                CompilationState cs = m_typeSystem.PlatformAbstraction.CreateCompilationState( this, cfg );

                m_methodCompilationState[ cfg ] = cs;

                m_pendingCode.Enqueue( cs );
            }
        }

        public CodeConstant CompileCodeConstant( object entity,
                                                 SequentialRegion regCode )
        {
            CodeConstant     cc  = new CodeConstant( entity );
            SequentialRegion reg = new SequentialRegion( this, cc, regCode.PlacementRequirements );

            if( entity is DataManager.DataDescriptor )
            {
                DataManager.DataDescriptor dd = ( DataManager.DataDescriptor )entity;

                if( dd.Context is ValueTypeRepresentation )
                {
                                        
#if ARMv7M_BUILD__LLVM_IR_ONLY
                    if( TypeSystem.PlatformAbstraction.PlatformName != "LLVM" )
#endif
                    {
                        dd.Write( reg );
                    }
                }
                else
                {
                    SequentialRegion.Section sec = reg.GetSectionOfVariableSize( sizeof( uint ) );

                    sec.WritePointerToDataDescriptor( dd );
                }
            }
            else if( entity is BasicBlock )
            {
                BasicBlock bb = ( BasicBlock )entity;

                SequentialRegion.Section sec = reg.GetSectionOfVariableSize( sizeof( uint ) );

                sec.WritePointerToBasicBlock( bb );
            }
            else
            {
                SequentialRegion.Section sec = reg.GetSectionOfVariableSize( sizeof( uint ) );

                if( sec.WriteGeneric( entity ) == false )
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected value for code constant: {0}", entity );
                }
            }

            cc.Region = reg;

            HashTableWithListFactory.AddUnique( m_regionToCodeConstants, regCode, cc );

            CompilationState cs = GetCompilationState( ( BasicBlock )regCode.Context );
            cs.TrackRegion( reg );

            return cc;
        }

        public void RecordAdjacencyNeed( Operator op,
                                         BasicBlock bbTarget )
        {
            HashTableWithListFactory.AddUnique( m_adjacencyNeeds, op, bbTarget );
        }

        //--//

        internal bool AssignAbsoluteAddressesToCode( )
        {
            //
            // We want to deal only with code at this phase.
            // The data descriptors will be recreated back later.
            //
            m_dataDescriptors.Clear( );

            //--//

            //
            // First of all, assign an absolute address to some hardware handler, so they are placed at the beginning of image.
            //
            bool fDone = true;


            fDone &= AssignAbsoluteAddresses( GetHandler( Runtime.HardwareException.VectorTable ), true );
            fDone &= AssignAbsoluteAddresses( GetHandler( Runtime.HardwareException.Bootstrap   ), false );

            //--//

            //
            // Assign an address to the "hot" code first.
            //
            foreach( ControlFlowGraphStateForCodeTransformation cfg in m_methodCompilationState.Keys )
            {
                fDone &= AssignAbsoluteAddresses( cfg, false );
            }

            //
            // Then to "cold" code.
            //
            foreach( ControlFlowGraphStateForCodeTransformation cfg in m_methodCompilationState.Keys )
            {
                fDone &= AssignAbsoluteAddresses( cfg, true );
            }

            //--//

            EnsureNoUnassignedRegions( );

            fDone &= VerifyAdjacencyNeeds( );

            return fDone;
        }

        private bool AssignAbsoluteAddresses( ControlFlowGraphStateForCodeTransformation cfg,
                                              bool fIncludeColdCode )
        {
            if(cfg != null)
            {
                CompilationState cs = GetCompilationState( cfg.EntryBasicBlock );

                cs.AssignAbsoluteAddresses( fIncludeColdCode );

                return FlushConstants( );
            }

            return true;
        }

        internal void RewindToDataDescriptorsPhase( )
        {
            m_sortedLinkerHeap = new SortedLinkerHeap( m_sortedLinkerHeapForCode );

            m_dataDescriptors.Clear( );
        }

        internal bool ImportDataDescriptors( )
        {
            GrowOnlySet< DataManager.DataDescriptor > dataDescriptors = SetFactory.NewWithReferenceEquality<DataManager.DataDescriptor>( );

            EnumerateImageAnnotations( delegate( ImageAnnotation an )
            {
                if( an.Target is DataManager.DataDescriptor )
                {
                    DataManager.DataDescriptor dd = ( DataManager.DataDescriptor )an.Target;

                    dataDescriptors.Insert( dd );
                }

                return true;
            } );

            foreach( DataManager.DataDescriptor dd in dataDescriptors )
            {
                AddObject( dd );
            }

            ExternalDataDescriptor.IExternalDataContext[] edcs = m_externalDataRegions.KeysToArray( );

            m_externalDataRegions.Clear( );

            foreach( ExternalDataDescriptor.IExternalDataContext edc in edcs )
            {
                AddExternalData( edc, ( uint )edc.RawData.Length );
            }

            return ProcessPending( );
        }

        internal void AssignAbsoluteAddressesToDataDescriptors( )
        {
            foreach( DataManager.DataDescriptor dd in m_dataDescriptors.Keys )
            {
                m_dataDescriptors[ dd ].AssignAbsoluteAddress( );
            }

            foreach( ExternalDataDescriptor.IExternalDataContext dd in m_externalDataRegions.Keys )
            {
                m_externalDataRegions[ dd ].AssignAbsoluteAddress( );
            }

            //--//

            EnsureNoUnassignedRegions( );
        }

        [System.Diagnostics.Conditional( "DEBUG" )]
        private void EnsureNoUnassignedRegions( )
        {
            EnumerateRegions( delegate( SequentialRegion reg )
            {
                CHECKS.ASSERT( reg.IsBaseAddressAssigned || reg.Context is ExternalDataDescriptor, "Found region not assigned an address: {0}", reg );

                return true;
            } );
        }

        //--//

        private ControlFlowGraphStateForCodeTransformation GetHandler( Runtime.HardwareException he )
        {
            MethodRepresentation md = m_typeSystem.TryGetHandler( he );
            if( md != null )
            {
                return TypeSystemForCodeTransformation.GetCodeForMethod( md );
            }

            return null;
        }

        //--//

        internal void PushRegionConstantsToPendingList( SequentialRegion reg )
        {
            List< CodeConstant > lst;

            if( m_regionToCodeConstants.TryGetValue( reg, out lst ) )
            {
                foreach( CodeConstant ccToAssign in lst )
                {
                    if( FindDuplicateForConstant( ccToAssign ) == false )
                    {
                        m_pendingConstants.Add( ccToAssign );
                    }
                }
            }
        }

        private bool FindDuplicateForConstant( CodeConstant ccToAssign )
        {
            SequentialRegion     ccRegToAssign = ccToAssign.Region;
            ImageAnnotation      anToAssign    = ccToAssign.Source;
            List< CodeConstant > lstAssigned;

            if( m_codeConstants.TryGetValue( ccToAssign.Target, out lstAssigned ) )
            {
                foreach( CodeConstant cc in lstAssigned )
                {
                    SequentialRegion ccReg = cc.Region;

                    if( ccReg.PlacementRequirements.IsCompatible( ccRegToAssign.PlacementRequirements ) )
                    {
                        if( anToAssign.CanRelocateToAddress( anToAssign.InsertionAddress, ccReg.ExternalAddress ) )
                        {
                            ccRegToAssign.BaseAddress = ccReg.BaseAddress;

                            lstAssigned.Add( ccToAssign );
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool FlushConstants( )
        {
            bool fDone = true;

            foreach( CodeConstant ccToAssign in m_pendingConstants )
            {
                if( FindDuplicateForConstant( ccToAssign ) == false )
                {
                    SequentialRegion regToAssign = ccToAssign.Region;
                    ImageAnnotation  anToAssign  = ccToAssign.Source;

                    regToAssign.AssignAbsoluteAddress( );

                    if( anToAssign.ApplyRelocation( ) )
                    {
                        HashTableWithListFactory.AddUnique( m_codeConstants, ccToAssign.Target, ccToAssign );
                    }
                    else
                    {
                        fDone = false;
                    }
                }
            }

            m_pendingConstants.Clear( );

            return fDone;
        }

        private bool VerifyAdjacencyNeeds( )
        {
            bool fDone = true;

            foreach( Operator op in m_adjacencyNeeds.Keys )
            {
                var regSource  = GetAssociatedRegion( op.BasicBlock );
                var endAddress = regSource.EndAddress;

                foreach( BasicBlock bb in m_adjacencyNeeds[ op ] )
                {
                    var regTarget = GetAssociatedRegion( bb );

                    if( endAddress != regTarget.BaseAddress )
                    {
                        IncreaseEncodingLevelForBranch( op, bb, false );

                        fDone = false;
                        break;
                    }
                }
            }

            return fDone;
        }


        //--//

        internal bool ApplyRelocation( )
        {
            bool fRes = true;

            EnumerateRegions( delegate( SequentialRegion reg )
            {
                fRes &= reg.ApplyRelocation( );

                return true;
            } );

            return fRes;
        }

        internal bool CreateCodeMaps( )
        {
            bool fModified = false;

            foreach( CompilationState cs in m_methodCompilationState.Values )
            {
                fModified |= cs.CreateCodeMaps( );
            }

            if( fModified == false )
            {
                BuildReverseCodeMapIndex( );

                //
                // Only generate the lookup index if the corresponding field is referenced by the application.
                //
                FieldRepresentation fd = m_typeSystem.WellKnownFields.CodeMap_LookupAddress;
                if( fd != null )
                {
                    InstanceFieldRepresentation fd2 = m_typeSystem.GetEntityFromGlobalRoot( fd );
                    if( fd2 != null )
                    {
                        DataManager.ObjectDescriptor gr = m_typeSystem.GenerateGlobalRoot( );
                        DataManager.ArrayDescriptor  ad = ( DataManager.ArrayDescriptor )gr.Get( fd2 );

                        CodeMap.ReverseIndex[] arrayOld = ad != null ? ( CodeMap.ReverseIndex[] )ad.Source : null;
                        CodeMap.ReverseIndex[] arrayNew = m_reverseCodeMapIndex;

                        if( CodeMap.ReverseIndex.SameArrayContents( arrayOld, arrayNew ) == false )
                        {
                            gr.ConvertAndSet( fd2, DataManager.Attributes.Constant, null, arrayNew );

                            fModified = true;
                        }
                    }
                }
            }

            return fModified == false;
        }

        internal bool CreateExceptionHandlingTables( )
        {
            bool fModified = false;

            foreach( CompilationState cs in m_methodCompilationState.Values )
            {
                fModified |= cs.CreateExceptionHandlingTables( );
            }

            return fModified == false;
        }

        internal bool CreateAvailableMemoryTables( )
        {
            bool fModified = false;

            //
            // BUGBUG #89: the m_forcedDevirtalizations member in TypeSystem currently does not serialization process!!!
            //
            if(m_typeSystem.ForcedDevirtualizations != null)
            {
                //
                // Only generate the lookup index if the corresponding field is referenced by the application.
                //
                InstanceFieldRepresentation fd = ( InstanceFieldRepresentation )m_typeSystem.WellKnownFields.Memory_m_availableMemory;
                if(fd != null)
                {
                    TypeRepresentation td = fd.OwnerType;
                    TypeRepresentation tdNew;

                    if(m_typeSystem.ForcedDevirtualizations.TryGetValue( td, out tdNew ) == false)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot devirtualize type '{0}'", td );
                    }

                    DataManager.ObjectDescriptor od = m_typeSystem.GenerateSingleton( tdNew, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation );
                    DataManager.ArrayDescriptor  ad = ( DataManager.ArrayDescriptor )od.Get( fd );

                    Runtime.Memory.Range[] arrayOld = ad != null ? ( Runtime.Memory.Range[] )ad.Source : null;
                    Runtime.Memory.Range[] arrayNew = m_availableMemory.ToArray( );

                    Array.Sort( arrayNew, ( left, right ) => AddressMath.Compare( left.Start, right.Start ) );

                    if(Runtime.Memory.Range.ArrayEquals( arrayOld, arrayNew ) == false)
                    {
                        od.ConvertAndSet( fd, DataManager.Attributes.Constant, null, arrayNew );

                        fModified = true;
                    }
                }
            }

            return fModified == false;
        }

        internal bool CreateImageRelocationData( )
        {
            bool fModified = false;

            //
            // Only generate the lookup index if the corresponding field is referenced by the application.
            //
            InstanceFieldRepresentation fd = ( InstanceFieldRepresentation )m_typeSystem.WellKnownFields.Memory_m_relocationInfo;
            if( fd != null )
            {
                TypeRepresentation td = fd.OwnerType;
                TypeRepresentation tdNew;

                if( m_typeSystem.ForcedDevirtualizations.TryGetValue( td, out tdNew ) == false )
                {
                    throw TypeConsistencyErrorException.Create( "Cannot devirtualize type '{0}'", td );
                }

                DataManager.ObjectDescriptor od = m_typeSystem.GenerateSingleton( tdNew, DataManager.Attributes.Constant | DataManager.Attributes.SuitableForConstantPropagation );
                DataManager.ArrayDescriptor  ad = ( DataManager.ArrayDescriptor )od.Get( fd );

                Runtime.Memory.RelocationInfo[] arrayOld = ad != null ? ( Runtime.Memory.RelocationInfo[] )ad.Source : null;
                Runtime.Memory.RelocationInfo[] arrayNew = BuildImageRelocationData( );

                if( Runtime.Memory.RelocationInfo.ArrayEquals( arrayOld, arrayNew ) == false )
                {
                    Abstractions.PlacementRequirements pr = new Abstractions.PlacementRequirements( sizeof( uint ), 0 );

                    pr.AddConstraint( Runtime.MemoryUsage.Bootstrap );

                    od.PlacementRequirements = pr;

                    od.ConvertAndSet( fd, DataManager.Attributes.Constant, pr, arrayNew );

                    InstanceFieldRepresentation fd2 = ( InstanceFieldRepresentation )m_typeSystem.WellKnownFields.RelocationInfo_m_data;
                    if( fd2 != null )
                    {
                        DataManager.ArrayDescriptor adSub = ( DataManager.ArrayDescriptor )od.Get( fd );

                        adSub.RefreshValues( null );

                        pr = new Abstractions.PlacementRequirements( sizeof( uint ), 0 );

                        pr.AddConstraint( Runtime.MemoryUsage.Relocation );

                        for( int i = 0; i < arrayNew.Length; i++ )
                        {
                            DataManager.ObjectDescriptor odSub = ( DataManager.ObjectDescriptor )adSub.Get( i );

                            odSub.RefreshValues( null );

                            DataManager.ArrayDescriptor adSub2 = ( DataManager.ArrayDescriptor )odSub.Get( fd2 );
                            if( adSub2 != null )
                            {
                                adSub2.PlacementRequirements = pr;
                            }
                        }
                    }

                    fModified = true;
                }
            }

            return fModified == false;
        }

        internal void CommitMemoryMap( )
        {
            m_availableMemory = m_sortedLinkerHeap.Commit( );
        }

        internal void CacheSourceCore( )
        {
            EnumerateImageAnnotations( delegate( ImageAnnotation an )
            {
                Operator op = an.Target as IR.Operator;

                if( op != null )
                {
                    Debugging.DebugInfo di = op.DebugInfo;

                    if( di != null )
                    {
                        SourceCodeTracker.SourceCode sc = m_sourceCodeTracker.GetSourceCode( di.SrcFileName );
                        if( sc != null )
                        {
                            sc.UsingCachedValues = true;
                        }
                    }
                }

                return true;
            } );
        }

        //--//

        private void BuildReverseCodeMapIndex( )
        {
            List< CodeMap.ReverseIndex > lst         = new List<CodeMap.ReverseIndex>( );
            UIntPtr                      lastAddress = new UIntPtr( );
            CodeMap                      lastCodeMap = null;

            foreach( SequentialRegion reg in this.SortedRegions )
            {
                BasicBlock bb = reg.Context as BasicBlock;

                if( bb != null )
                {
                    CodeMap codeMap = bb.Owner.Method.CodeMap;

                    if( codeMap != lastCodeMap )
                    {
                        CodeMap.ReverseIndex idx;

                        idx.Address = reg.BaseAddress;
                        idx.Code = codeMap;

                        lst.Add( idx );

                        lastCodeMap = codeMap;
                    }

                    lastAddress = reg.EndAddress;
                }
            }

            if( lastCodeMap != null )
            {
                CodeMap.ReverseIndex idx;

                idx.Address = lastAddress;
                idx.Code = null;

                lst.Add( idx );
            }

            //--//

            m_reverseCodeMapIndex = lst.ToArray( );
        }

        private Runtime.Memory.RelocationInfo[] BuildImageRelocationData( )
        {
            List< Runtime.Memory.RelocationInfo > lst         = new List<Runtime.Memory.RelocationInfo>( );
            UIntPtr                               baseAddress = new UIntPtr( );
            UIntPtr                               lastAddress = new UIntPtr( );
            MemoryRange                           buffer      = null;

            foreach( SequentialRegion reg in this.SortedRegions )
            {
                if( ShouldRelocate( reg.BaseAddress.ToUInt32( ) ) )
                {
                    if( buffer != null )
                    {
                        int distance = AddressMath.Distance( lastAddress, reg.BaseAddress );

                        if( distance > 0 )
                        {
                            AddRelocation( lst, baseAddress, buffer );

                            buffer = null;
                        }
                        else if( distance < 0 )
                        {
                            //
                            // Overlap, rewind the buffer.
                            //
                            buffer.Rewind( -distance );

                            lastAddress = reg.BaseAddress;
                        }
                    }

                    if( buffer == null )
                    {
                        baseAddress = reg.BaseAddress;
                        buffer = new MemoryRange( );
                    }

                    lastAddress = reg.PayloadEndAddress;
                    buffer.Append( reg.ToArray( ) );
                }
            }

            if( buffer != null )
            {
                AddRelocation( lst, baseAddress, buffer );
            }

            return lst.ToArray( );
        }

        private static void AddRelocation( List<Runtime.Memory.RelocationInfo> lst,
                                           UIntPtr baseAddress,
                                           MemoryRange buffer )
        {
            byte[] data8  = buffer.ToArray( );
            uint[] data32 = new uint[ ( data8.Length + sizeof( uint ) - 1 ) / sizeof( uint ) ];

            Buffer.BlockCopy( data8, 0, data32, 0, data8.Length );

            int size     = data32.Length;
            int pos      = 0;
            int posStart = 0;

            while( true )
            {
                while( pos < size )
                {
                    if( data32[ pos ] == 0 )
                    {
                        break;
                    }

                    pos++;
                }

                int posZero = pos;

                while( posZero < size )
                {
                    if( data32[ posZero ] != 0 )
                    {
                        break;
                    }

                    posZero++;
                }

                if( posZero < size && posZero - pos < 4 )
                {
                    //
                    // Run of zeros is too short, look for the next one.
                    //
                    pos = posZero;
                    continue;
                }

                if( posStart != pos )
                {
                    lst.Add( new Runtime.Memory.RelocationInfo( AddressMath.Increment( baseAddress, ( uint )posStart * sizeof( uint ) ), data32, posStart, pos - posStart ) );

                    posStart = pos;
                }

                if( posStart != posZero )
                {
                    lst.Add( new Runtime.Memory.RelocationInfo( AddressMath.Increment( baseAddress, ( uint )posStart * sizeof( uint ) ), ( uint )( posZero - posStart ) ) );

                    pos = posZero;
                    posStart = posZero;
                }

                if( posStart == size )
                {
                    break;
                }
            }
        }

        //--//

        public List<Configuration.Environment.ImageSection> CollectImage( )
        {
            List< Configuration.Environment.ImageSection > lst = new List<Configuration.Environment.ImageSection>( );

            SequentialRegion[]   regArray               = this.SortedRegions;
            Runtime.Memory.Range currentBlock           = null;
            MemoryRange          currentRange           = null;
            uint                 currentRangeAddress    = 0;
            uint                 currentRangeAddressEnd = 0;


            Abstractions.Platform pa = m_typeSystem.PlatformAbstraction;

            for( int idx = 0; idx < regArray.Length; idx++ )
            {
                SequentialRegion reg = regArray[ idx ];

                if( reg.Size > 0 )
                {
                    byte[]               payload            = reg.ToArray( );
                    uint                 executeAddress     = reg.BaseAddress.ToUInt32( );
                    Runtime.Memory.Range rng                = FindMemoryBlock( executeAddress );
                    uint                 physicalAddress    = executeAddress;
                    uint                 physicalAddressEnd;

                    if( rng.ExtensionHandler != null )
                    {
                        object hnd = Activator.CreateInstance( rng.ExtensionHandler );

                        if( hnd is Configuration.Environment.IMemoryMapper )
                        {
                            Configuration.Environment.IMemoryMapper itf = ( Configuration.Environment.IMemoryMapper )hnd;

                            physicalAddress = itf.GetUncacheableAddress( physicalAddress );
                        }
                    }

                    physicalAddressEnd = physicalAddress + ( uint )payload.Length;

                    if( rng != null )
                    {
                        bool fMerge  = false;
                        bool fAppend = true; // If no current range or disjoint ranges, append new one.

                        if( currentBlock != null && currentBlock.Equals( rng ) )
                        {
                            CHECKS.ASSERT( currentRangeAddress <= physicalAddress, "Internal failure: regions not sorted out of SortedRegion: {0:X8} preceeds {1:X8}", currentRangeAddress, physicalAddress );

                            int gap = ( int )( physicalAddress - currentRangeAddressEnd );

                            if( gap == 0 ) // Adjacent?
                            {
                                //
                                // Concatenate.
                                //
                                fMerge = true;
                            }
                            else if( gap < 0 ) // Overlapped?
                            {
                                int gapEnd = ( int )( physicalAddressEnd - currentRangeAddressEnd );
                                int offset = ( int )( physicalAddress - currentRangeAddress );

                                if( gapEnd <= 0 ) // Fully contained?
                                {
                                    //
                                    // Verify the payload matches.
                                    //
                                    if( currentRange.Match( offset, payload, 0, payload.Length ) == false )
                                    {
                                        throw TypeConsistencyErrorException.Create( "Inconsistency image: overlapping regions have different payloads" );
                                    }

                                    fAppend = false;
                                }
                                else // Then partially contained.
                                {
                                    //
                                    // Verify the overlapping payload matches and merge the rest.
                                    //
                                    if( currentRange.Match( offset, payload, 0, payload.Length - gapEnd ) == false )
                                    {
                                        throw TypeConsistencyErrorException.Create( "Inconsistency image: overlapping regions have different payloads" );
                                    }

                                    payload = ArrayUtility.ExtractSliceFromNotNullArray( payload, payload.Length - gapEnd, gapEnd );

                                    fMerge = true;
                                }
                            }
                        }

                        if( fAppend )
                        {
                            if( fMerge )
                            {
                                currentRange.Append( payload );
                                currentRangeAddressEnd += ( uint )payload.Length;
                            }
                            else
                            {
                                AppendToImageIfNeeded( lst, currentBlock, currentRange, currentRangeAddress );

                                currentBlock = rng;
                                currentRange = new MemoryRange( payload );
                                currentRangeAddress = physicalAddress;
                                currentRangeAddressEnd = physicalAddressEnd;
                            }
                        }
                    }
                }
            }

            AppendToImageIfNeeded( lst, currentBlock, currentRange, currentRangeAddress );

            return lst;
        }

        private void AppendToImageIfNeeded( List<Configuration.Environment.ImageSection> lst,
                                            Runtime.Memory.Range currentBlock,
                                            MemoryRange currentRange,
                                            uint currentRangeAddress )
        {
            if( currentBlock != null )
            {
                lst.Add( new Configuration.Environment.ImageSection( currentRangeAddress, currentRange.ToArray( ), currentBlock.SectionName, currentBlock.Attributes, currentBlock.Usage ) );
            }
        }

        //--//

        internal Runtime.Memory.Range FindMemoryBlock( uint address )
        {
            for( int i = 0; i < m_memoryBlocks.Count; i++ )
            {
                Runtime.Memory.Range mr = m_memoryBlocks[ i ];

                if( mr.Contains( address ) )
                {
                    return mr;
                }
            }

            return null;
        }

        internal bool ShouldRelocate( uint address )
        {
            for( int i = 0; i < m_memoryBlocks.Count; i++ )
            {
                Runtime.Memory.Range mr = m_memoryBlocks[ i ];

                if( mr.Contains( address ) )
                {
                    if( ( mr.Usage & Runtime.MemoryUsage.Relocation ) != 0 ||
                       ( mr.Attributes & Runtime.MemoryAttributes.LoadedAtEntrypoint ) != 0 )
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool ReserveRangeOfMemory( uint size,
                                                uint offset,
                                                Abstractions.PlacementRequirements pr,
                                            out UIntPtr address )
        {
            return m_sortedLinkerHeap.Allocate( size, offset, pr, out address );
        }

        //--//

        public BranchEncodingLevel GetEncodingLevelForBranch( Operator op,
                                                              BasicBlock bb,
                                                              bool fConditional )
        {
            var                 ht = fConditional ? m_relocationConstraintsForConditionalBranches : m_relocationConstraintsForUnconditionalBranches;
            BranchEncodingLevel val;

            if( ht.TryGetValue( op, out val ) == false )
            {
                val = fConditional ? BranchEncodingLevel.ShortBranch : BranchEncodingLevel.Skip;
            }

            //
            // Branch to self? Don't remove.
            //
            if( op.BasicBlock == bb )
            {
                if( val == BranchEncodingLevel.Skip ) val++;
            }

            return val;
        }

        public void IncreaseEncodingLevelForBranch( Operator op,
                                                    BasicBlock bb,
                                                    bool fConditional )
        {
            IncreaseEncodingLevelForBranch( op, bb, fConditional, BranchEncodingLevel.Skip );
        }

        public void IncreaseEncodingLevelForBranch( Operator op,
                                                    BasicBlock bb,
                                                    bool fConditional,
                                                    BranchEncodingLevel minLevel )
        {
            BranchEncodingLevel val = GetEncodingLevelForBranch( op, bb, fConditional );

            if( val < minLevel )
            {
                val = minLevel;
            }

            if( val != BranchEncodingLevel.Max )
            {
                val++;
            }

            var ht = fConditional ? m_relocationConstraintsForConditionalBranches : m_relocationConstraintsForUnconditionalBranches;

            ht[ op ] = val;
        }

        //--//

        public ConstantAddressEncodingLevel GetEncodingLevelForConstant( Operator op )
        {
            ConstantAddressEncodingLevel val;

            if( m_relocationConstraintsForConstants.TryGetValue( op, out val ) == false )
            {
                val = ConstantAddressEncodingLevel.Immediate;
            }

            return val;
        }

        public void IncreaseEncodingLevelForConstant( Operator op )
        {
            IncreaseEncodingLevelForConstant( op, ConstantAddressEncodingLevel.Immediate );
        }

        public void IncreaseEncodingLevelForConstant( Operator op,
                                                      ConstantAddressEncodingLevel minLevel )
        {
            ConstantAddressEncodingLevel val = GetEncodingLevelForConstant( op );

            if( val < minLevel )
            {
                val = minLevel;
            }

            if( val != ConstantAddressEncodingLevel.Max ) val++;

            m_relocationConstraintsForConstants[ op ] = val;
        }

        //--//

        public uint Resolve( object marker )
        {
            SequentialRegion reg = GenericGetAssociatedRegion( marker );

            if( reg == null )
            {
                throw TypeConsistencyErrorException.Create( "Cannot resolve relocation information for {0}", marker );
            }

            return reg.ExternalAddress;
        }

        private SequentialRegion GenericGetAssociatedRegion( object marker )
        {
            SequentialRegion reg;

            if( marker is CodePointer )
            {
                CodePointer cp = ( CodePointer )marker;

                marker = m_typeSystem.DataManagerInstance.GetCodePointerFromUniqueID( cp.Target );
            }

            if( marker is SequentialRegion )
            {
                reg = ( SequentialRegion )marker;
            }
            else if( marker is ControlFlowGraphStateForCodeTransformation )
            {
                reg = GetAssociatedRegion( ( ControlFlowGraphStateForCodeTransformation )marker );
            }
            else if( marker is BasicBlock )
            {
                reg = GetAssociatedRegion( ( BasicBlock )marker );
            }
            else if( marker is DataManager.DataDescriptor )
            {
                reg = GetAssociatedRegion( ( DataManager.DataDescriptor )marker );
            }
            else if( marker is ExternalDataDescriptor.IExternalDataContext )
            {
                reg = GetAssociatedRegion( ( ExternalDataDescriptor.IExternalDataContext )marker );
            }
            else if( marker is Operator )
            {
                reg = GetAssociatedRegion( ( ( Operator )marker ).BasicBlock );
            }
            else if( marker is CodeConstant )
            {
                CodeConstant cc = ( CodeConstant )marker;

                reg = cc.Region;
            }
            else
            {
                reg = null;
            }

            return reg;
        }

        public SequentialRegion GetAssociatedRegion( ControlFlowGraphStateForCodeTransformation cfg )
        {
            return GetAssociatedRegion( cfg.EntryBasicBlock );
        }

        public SequentialRegion GetAssociatedRegion( BasicBlock bb )
        {
            CompilationState cs = TryToGetCompilationState( bb );

            return cs != null ? cs.GetRegion( bb ) : null;
        }

        public SequentialRegion GetAssociatedRegion( DataManager.DataDescriptor dd )
        {
            SequentialRegion reg;

            m_dataDescriptors.TryGetValue( dd, out reg );

            return reg;
        }

        public SequentialRegion GetAssociatedRegion( ExternalDataDescriptor.IExternalDataContext edc )
        {
            SequentialRegion reg;

            m_externalDataRegions.TryGetValue( edc, out reg );

            return reg;
        }

        public SequentialRegion GetAssociatedRegion( ConstantExpression ex )
        {
            return GenericGetAssociatedRegion( ex.Value );
        }

        public List<SequentialRegion> GetAssociatedRegions( ControlFlowGraphStateForCodeTransformation cfg,
                                                              bool fIncludeConstants )
        {
            List< SequentialRegion > lst = new List<SequentialRegion>( );

            CompilationState cs = TryToGetCompilationState( cfg );
            if( cs != null )
            {
                foreach( SequentialRegion reg in cs.AssociatedRegions )
                {
                    if( reg.Context is CodeConstant )
                    {
                        if( fIncludeConstants == false )
                        {
                            continue;
                        }
                    }

                    lst.Add( reg );
                }
            }

            return lst;
        }

        //--//

        public static SequentialRegion[] SortRegions( List<SequentialRegion> lst )
        {
            SequentialRegion[] regArray = lst.ToArray( );

            SortRegions( regArray );

            return regArray;
        }

        public static void SortRegions( SequentialRegion[] regArray )
        {
            Array.Sort( regArray, delegate( SequentialRegion left, SequentialRegion right )
            {
                int res = left.ExternalAddress.CompareTo( right.ExternalAddress );
                if( res != 0 )
                {
                    return res;
                }

                res = left.EndAddress.ToUInt32( ).CompareTo( right.EndAddress.ToUInt32( ) );
                if( res != 0 )
                {
                    return res;
                }

                BasicBlock bbLeft  = left.Context as BasicBlock;
                BasicBlock bbRight = right.Context as BasicBlock;

                if( bbLeft != null && bbRight != null )
                {
                    res = bbLeft.SpanningTreeIndex.CompareTo( bbRight.SpanningTreeIndex );
                    if( res != 0 )
                    {
                        return res;
                    }
                }

                return 0;
            } );
        }

        public static SequentialRegion ResolveAddressToRegion( SequentialRegion[] regArray,
                                                               uint address )
        {
            int low  = 0;
            int high = regArray.Length - 1;

            while( low <= high )
            {
                int mid = ( high + low ) / 2;

                SequentialRegion reg = regArray[ mid ];

                if( address < reg.BaseAddress.ToUInt32( ) )
                {
                    high = mid - 1;
                }
                else if( address >= reg.EndAddress.ToUInt32( ) )
                {
                    low = mid + 1;
                }
                else
                {
                    return reg;
                }
            }

            return null;
        }

        //--//

        public bool EnumerateRegions( SequentialRegionEnumerationCallback callback )
        {
            foreach( CompilationState cs in m_methodCompilationState.Values )
            {
                foreach( var reg in cs.BasicBlockRegions )
                {
                    if( callback( reg ) == false )
                    {
                        return false;
                    }
                }
            }

            foreach( object obj in m_codeConstants.Keys )
            {
                foreach( CodeConstant cc in m_codeConstants[ obj ] )
                {
                    if( callback( cc.Region ) == false )
                    {
                        return false;
                    }
                }
            }

            foreach( SequentialRegion reg in m_dataDescriptors.Values )
            {
                if( callback( reg ) == false )
                {
                    return false;
                }
            }

            foreach( SequentialRegion reg in m_externalDataRegions.Values )
            {
                if( callback( reg ) == false )
                {
                    return false;
                }
            }

            return true;
        }

        public bool EnumerateImageAnnotations( ImageAnnotationEnumerationCallback callback )
        {
            return EnumerateRegions( delegate( SequentialRegion reg )
            {
                foreach( ImageAnnotation an in reg.AnnotationList )
                {
                    if( callback( an ) == false )
                    {
                        return false;
                    }
                }

                return true;
            } );
        }

        //--//

        //
        // Access Methods
        //

        public TypeSystemForCodeTransformation TypeSystem
        {
            get
            {
                return m_typeSystem;
            }
        }

        public SequentialRegion Bootstrap
        {
            get
            {
                ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( m_typeSystem.GetHandler( Runtime.HardwareException.Reset ) );
                ////            ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( m_typeSystem.GetHandler( Runtime.HardwareException.Bootstrap ) );

                return GetAssociatedRegion( cfg );
            }
        }

        public List<Runtime.Memory.Range> MemoryBlocks
        {
            get
            {
                return m_memoryBlocks;
            }
        }

        public List<Runtime.Memory.Range> AvailableMemory
        {
            get
            {
                return m_availableMemory;
            }
        }

        public SourceCodeTracker SourceCodeTracker
        {
            get
            {
                return m_sourceCodeTracker;
            }
        }

        public CodeMap.ReverseIndex[] ReverseCodeMapIndex
        {
            get
            {
                return m_reverseCodeMapIndex;
            }
        }

        //--//

        public SequentialRegion[] SortedRegions
        {
            get
            {
                List< SequentialRegion > res = new List<SequentialRegion>( );

                EnumerateRegions( delegate( SequentialRegion reg )
                {
                    res.Add( reg );

                    return true;
                } );

                SequentialRegion[] regArray = res.ToArray( );

                SortRegions( regArray );

                return regArray;
            }
        }

        //
        // Debug Methods
        //

        class Statistics
        {
            internal string m_text;
            internal int    m_size;
            internal int    m_count;
        }

        class CatalogItem
        {
            //
            // State
            //

            object                 m_context;
            int                    m_hashCode;
            int                    m_subIndex;
            string                 m_filePrefix;
            string                 m_fileSuffix;
            System.IO.StreamWriter m_stream;

            //
            // Constructor Methods
            //

            private CatalogItem( )
            {
            }

            //
            // Helper Methods
            //

            internal static void Flush( GrowOnlyHashTable<int, List<CatalogItem>> catalog )
            {
                GrowOnlySet< object > set = SetFactory.NewWithReferenceEquality<object>( );

                foreach( var lst in catalog.Values )
                {
                    foreach( CatalogItem item in lst )
                    {
                        set.Insert( item.m_stream );
                    }
                }

                foreach( System.IO.StreamWriter stream in set )
                {
                    stream.Flush( );
                    stream.Close( );
                }
            }

            internal static System.IO.StreamWriter GenerateStream( GrowOnlyHashTable<int, List<CatalogItem>> catalog,
                                                                   string baseDirectory,
                                                                   string filePrefix,
                                                                   string fileSuffix,
                                                                   object context,
                                                                   int hashCode )
            {
                CatalogItem newItem = new CatalogItem( );

                newItem.m_context = context;
                newItem.m_hashCode = hashCode;
                newItem.m_filePrefix = filePrefix;
                newItem.m_fileSuffix = fileSuffix;

                //--//

                List< CatalogItem > lst = HashTableWithListFactory.Create( catalog, hashCode );

                foreach( CatalogItem item in lst )
                {
                    if( item.m_filePrefix == filePrefix &&
                       item.m_fileSuffix == fileSuffix &&
                       item.m_context.ToString( ) == context.ToString( ) )
                    {
                        newItem.m_stream = item.m_stream;
                        newItem.m_subIndex = item.m_subIndex;
                        break;
                    }

                    newItem.m_subIndex = Math.Max( newItem.m_subIndex, item.m_subIndex + 1 );
                }

                lst.Add( newItem );

                //--//

                if( newItem.m_stream == null )
                {
                    newItem.m_stream = new System.IO.StreamWriter( System.IO.Path.Combine( baseDirectory, newItem.FileName ), false, System.Text.Encoding.ASCII );
                }
                else
                {
                    newItem.m_stream.WriteLine( );
                    newItem.m_stream.WriteLine( "############################################################" );
                    newItem.m_stream.WriteLine( );
                }

                return newItem.m_stream;
            }

            //
            // Debug Methods
            //

            internal string FileName
            {
                get
                {
                    string fmt = ( m_subIndex == 0 ) ? "{0}{1:X8}{3}" : "{0}{1:X8}_{2}{3}";

                    return string.Format( fmt, m_filePrefix, m_hashCode, m_subIndex, m_fileSuffix );
                }
            }

            public override string ToString( )
            {
                return string.Format( "{0} => {1}", m_context, this.FileName );
            }
        }

        public void Disassemble( string baseDirectory )
        {
            GrowOnlyHashTable< int, List< CatalogItem > > catalog = HashTableFactory.New<int, List<CatalogItem>>( );

            if( System.IO.Directory.Exists( baseDirectory ) )
            {
                System.IO.Directory.Delete( baseDirectory, true );
            }

            System.IO.Directory.CreateDirectory( baseDirectory );

            //--//

            Disassemble_Code( baseDirectory, catalog );
            Disassemble_Data( baseDirectory, catalog );
            Disassemble_Stats( baseDirectory, catalog );

            CatalogItem.Flush( catalog );

            //--//

            using( System.IO.StreamWriter textWriter = new System.IO.StreamWriter( System.IO.Path.Combine( baseDirectory, "__Index.txt" ), false, System.Text.Encoding.ASCII ) )
            {
                List< string > output = new List<string>( );

                foreach( List< CatalogItem > lst in catalog.Values )
                {
                    foreach( CatalogItem item in lst )
                    {
                        output.Add( item.ToString( ) );
                    }
                }

                output.Sort( );

                foreach( var text in output )
                {
                    textWriter.WriteLine( text );
                }
            }
        }

        private void Disassemble_Code( string baseDirectory,
                                       GrowOnlyHashTable<int, List<CatalogItem>> catalog )
        {
            foreach( ControlFlowGraphStateForCodeTransformation cfg in m_methodCompilationState.Keys )
            {
                CompilationState     cs   = m_methodCompilationState[ cfg ];
                MethodRepresentation md   = cfg.Method;
                CodeMap              cm   = md.CodeMap;
                string               name = md.ToShortString( );

                System.IO.StreamWriter textWriter = CatalogItem.GenerateStream( catalog, baseDirectory, "Code_", ".asm", cfg, name.GetHashCode( ) );

                textWriter.WriteLine( "{0}", name );

                if( cm != null )
                {
                    textWriter.WriteLine( );
                    textWriter.WriteLine( "    CodeMap:" );
                    textWriter.WriteLine( );

                    foreach( CodeMap.Range rng in cm.Ranges )
                    {
                        bool fFirst = true;

                        foreach( string txt in rng.ToString( ).Split( new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            if( fFirst )
                            {
                                fFirst = false;

                                textWriter.WriteLine( "        {0}", txt );
                            }
                            else
                            {
                                textWriter.WriteLine( "            {0}", txt );
                            }
                        }

                        textWriter.WriteLine( );
                    }

                    ExceptionMap em = cm.ExceptionMap;

                    if( em != null )
                    {
                        textWriter.WriteLine( );
                        textWriter.WriteLine( "    ExceptionMap:" );
                        textWriter.WriteLine( );

                        foreach( ExceptionMap.Range rng in em.Ranges )
                        {
                            ExceptionMap.Handler[] handlers = rng.Handlers;

                            textWriter.WriteLine( "        Range[0x{0:X8}-0x{1:X8} {2} Entries]", rng.Start.ToUInt32( ), rng.End.ToUInt32( ), handlers != null ? handlers.Length : 0 );

                            if( handlers != null )
                            {
                                for( int pos = 0; pos < handlers.Length; pos++ )
                                {
                                    ExceptionMap.Handler hnd     = handlers[ pos ];
                                    VTable               vt      = hnd.Filter;
                                    uint                 address = Resolve( hnd.HandlerCode );

                                    textWriter.WriteLine( "            Handler 0x{0:X8} for {1}", address, vt != null ? vt.TypeInfo.FullNameWithAbbreviation : "<all>" );
                                }
                            }

                            textWriter.WriteLine( );
                        }
                    }
                }

                foreach( SequentialRegion reg in Core.SortRegions( cs.AssociatedRegions ) )
                {
                    reg.Dump( textWriter );
                }
            }
        }

        private void Disassemble_Data( string baseDirectory,
                                       GrowOnlyHashTable<int, List<CatalogItem>> catalog )
        {
            foreach( DataManager.DataDescriptor dd in m_dataDescriptors.Keys )
            {
                SequentialRegion reg  = m_dataDescriptors[ dd ];
                string           name = dd.ToString( );

                System.IO.StreamWriter textWriter = CatalogItem.GenerateStream( catalog, baseDirectory, "Data_", ".asm", dd, name.GetHashCode( ) );

                textWriter.WriteLine( "{0}", name );
                textWriter.WriteLine( );

                reg.Dump( textWriter );
            }
        }

        private void Disassemble_Stats( string baseDirectory,
                                        GrowOnlyHashTable<int, List<CatalogItem>> catalog )
        {
            GrowOnlyHashTable< string, Statistics > statistics = HashTableFactory.New<string, Statistics>( );

            foreach( SequentialRegion reg in this.SortedRegions )
            {
                object     context = reg.Context;
                string     text;
                Statistics stat;

                if( context is BasicBlock )
                {
                    BasicBlock bb2 = ( BasicBlock )context;

                    text = "Code for " + bb2.Owner.Method.ToShortString( );
                }
                else
                {
                    text = context.ToString( );
                }

                if( statistics.TryGetValue( text, out stat ) == false )
                {
                    stat = new Statistics( );
                    stat.m_text = text;

                    statistics[ text ] = stat;
                }

                stat.m_size += ( int )reg.Size; ;
                stat.m_count += 1;
            }

            //--//

            using( System.IO.StreamWriter textWriter = new System.IO.StreamWriter( System.IO.Path.Combine( baseDirectory, "Summary.txt" ), false, System.Text.Encoding.ASCII ) )
            {
                Statistics[] stats = statistics.ValuesToArray( );

                Array.Sort( stats, delegate( Statistics left, Statistics right )
                {
                    int res = right.m_size.CompareTo( left.m_size );

                    if( res == 0 )
                    {
                        res = left.m_text.CompareTo( right.m_text );
                    }

                    return res;
                } );

                foreach( Statistics stat in stats )
                {
                    textWriter.WriteLine( "{0,8} byte(s) for {1,4} entries of type {2}", stat.m_size, stat.m_count, stat.m_text );
                }
            }
        }

        //--//

        public void DisassembleInOneFile( string fileName )
        {
            using( System.IO.StreamWriter textWriter = new System.IO.StreamWriter( fileName, false, System.Text.Encoding.ASCII, 1024 * 1024 ) )
            {
                GrowOnlyHashTable< string, Statistics > statistics = HashTableFactory.New<string, Statistics>( );

                foreach( SequentialRegion reg in this.SortedRegions )
                {
                    object context = reg.Context;

                    {
                        string     text;
                        Statistics stat;

                        if( context is BasicBlock )
                        {
                            BasicBlock bb2 = ( BasicBlock )context;

                            text = "Code for " + bb2.Owner.Method.ToShortString( );
                        }
                        else
                        {
                            text = context.ToString( );
                        }

                        if( reg.PlacementRequirements.Alignment > 0x100 )
                        {
                            text += "[TLB]";
                        }

                        if( reg.PlacementRequirements.Usages != null &&
                            reg.PlacementRequirements.Usages.Length > 0 &&
                            reg.PlacementRequirements.Usages[ 0 ] == Runtime.MemoryUsage.Relocation
                          )
                        {
                            text += "[Relocation]";
                        }

                        if( statistics.TryGetValue( text, out stat ) == false )
                        {
                            stat = new Statistics( );
                            stat.m_text = text;

                            statistics[ text ] = stat;
                        }

                        stat.m_size += ( int )reg.Size; ;
                        stat.m_count += 1;
                    }

                    //--//

                    EntryBasicBlock bb = context as EntryBasicBlock;
                    if( bb != null )
                    {
                        MethodRepresentation md = bb.Owner.Method;
                        CodeMap              cm = md.CodeMap;

                        if( cm != null )
                        {
                            textWriter.WriteLine( );
                            textWriter.WriteLine( "    CodeMap for {0}", md.ToShortString( ) );
                            textWriter.WriteLine( );

                            foreach( CodeMap.Range rng in cm.Ranges )
                            {
                                bool fFirst = true;

                                foreach( string txt in rng.ToString( ).Split( new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries ) )
                                {
                                    if( fFirst )
                                    {
                                        fFirst = false;

                                        textWriter.WriteLine( "        {0}", txt );
                                    }
                                    else
                                    {
                                        textWriter.WriteLine( "            {0}", txt );
                                    }
                                }

                                textWriter.WriteLine( );
                            }

                            ExceptionMap em = cm.ExceptionMap;

                            if( em != null )
                            {
                                textWriter.WriteLine( );
                                textWriter.WriteLine( "    ExceptionMap for {0}", md.ToShortString( ) );
                                textWriter.WriteLine( );

                                foreach( ExceptionMap.Range rng in em.Ranges )
                                {
                                    ExceptionMap.Handler[] handlers = rng.Handlers;

                                    textWriter.WriteLine( "        Range[0x{0:X8}-0x{1:X8} {2} Entries]", rng.Start.ToUInt32( ), rng.End.ToUInt32( ), handlers != null ? handlers.Length : 0 );

                                    if( handlers != null )
                                    {
                                        for( int pos = 0; pos < handlers.Length; pos++ )
                                        {
                                            ExceptionMap.Handler hnd     = handlers[ pos ];
                                            VTable               vt      = hnd.Filter;
                                            uint                 address = Resolve( hnd.HandlerCode );

                                            textWriter.WriteLine( "            Handler 0x{0:X8} for {1}", address, vt != null ? vt.TypeInfo.FullNameWithAbbreviation : "<all>" );
                                        }
                                    }

                                    textWriter.WriteLine( );
                                }
                            }
                        }
                    }

                    reg.Dump( textWriter );
                }

                //--//

                textWriter.WriteLine( );
                textWriter.WriteLine( "    Summary:" );
                textWriter.WriteLine( );

                Statistics[] stats = statistics.ValuesToArray( );

                Array.Sort( stats, delegate( Statistics left, Statistics right )
                {
                    int res = right.m_size.CompareTo( left.m_size );

                    if( res == 0 )
                    {
                        res = left.m_text.CompareTo( right.m_text );
                    }

                    return res;
                } );

                foreach( Statistics stat in stats )
                {
                    textWriter.WriteLine( "        {0,8} for {1,4} entries of type {2}", stat.m_size, stat.m_count, stat.m_text );
                }

                textWriter.WriteLine( );
            }
        }
    }
}
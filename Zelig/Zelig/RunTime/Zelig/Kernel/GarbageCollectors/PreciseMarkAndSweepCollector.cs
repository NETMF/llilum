//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class PreciseMarkAndSweepCollector : MarkAndSweepCollector
    {
        class PreciseStackWalker : TS.CodeMapDecoderCallback, MarkAndSweepStackWalker
        {
            //
            // State
            //

            const int c_StackMarkSize = 8 * sizeof( uint );

            PreciseMarkAndSweepCollector m_owner;
            UIntPtr m_pc;

            uint m_registerMask_Scratched;
            uint m_registerMask_Heap;
            uint m_registerMask_Internal;
            uint m_registerMask_Potential;
            uint m_stackMask_Heap;
            uint m_stackMask_Internal;
            uint m_stackMask_Potential;
            uint m_stackLow;
            uint m_stackHigh;
            bool m_done;

            //
            // Constructor Methods
            //

            internal PreciseStackWalker( PreciseMarkAndSweepCollector owner )
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            public override bool RegisterEnter( UIntPtr address,
                                                uint num,
                                                PointerKind kind )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                uint mask = 1u << (int)num;

                switch(kind)
                {
                    case PointerKind.Heap: m_registerMask_Heap |= mask; break;
                    case PointerKind.Internal: m_registerMask_Internal |= mask; break;
                    case PointerKind.Potential: m_registerMask_Potential |= mask; break;
                }

                return true;
            }

            public override bool RegisterLeave( UIntPtr address,
                                                uint num )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                uint mask = ~( 1u << (int)num );

                m_registerMask_Heap &= mask;
                m_registerMask_Internal &= mask;
                m_registerMask_Potential &= mask;

                return true;
            }

            public override bool StackEnter( UIntPtr address,
                                             uint offset,
                                             PointerKind kind )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                if(m_stackLow <= offset && offset < m_stackHigh)
                {
                    uint mask = ( 1u << (int)( offset - m_stackLow ) );

                    switch(kind)
                    {
                        case PointerKind.Heap: m_stackMask_Heap |= mask; break;
                        case PointerKind.Internal: m_stackMask_Internal |= mask; break;
                        case PointerKind.Potential: m_stackMask_Potential |= mask; break;
                    }
                }

                if(offset >= m_stackHigh)
                {
                    m_done = false;
                }

                return true;
            }

            public override bool StackLeave( UIntPtr address,
                                             uint offset )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                if(m_stackLow <= offset && offset < m_stackHigh)
                {
                    uint mask = ~( 1u << (int)( offset - m_stackLow ) );

                    m_stackMask_Heap &= mask;
                    m_stackMask_Internal &= mask;
                    m_stackMask_Potential &= mask;
                }

                if(offset >= m_stackHigh)
                {
                    m_done = false;
                }

                return true;
            }

            //--//

            public unsafe void Process( Processor.Context ctx )
            {
                //
                // All registers should be considered.
                //
                m_registerMask_Scratched = 0;

                while(true)
                {
                    m_pc = ctx.ProgramCounter;
                    m_stackLow = 0;
                    m_stackHigh = c_StackMarkSize;

                    //--//

                    TS.CodeMap cm = TS.CodeMap.ResolveAddressToCodeMap( m_pc );

                    BugCheck.Assert( cm != null, BugCheck.StopCode.UnwindFailure );

                    int idx = cm.FindRange( m_pc );

                    BugCheck.Assert( idx >= 0, BugCheck.StopCode.UnwindFailure );

                    while(true)
                    {
                        m_registerMask_Heap = 0;
                        m_registerMask_Internal = 0;
                        m_registerMask_Potential = 0;
                        m_stackMask_Heap = 0;
                        m_stackMask_Internal = 0;
                        m_stackMask_Potential = 0;
                        m_done = true;

                        cm.Ranges[ idx ].Decode( this );

                        //
                        // On the first pass, mark the objects pointed to by live registers.
                        //
                        if(m_stackLow == 0)
                        {
                            uint set = m_registerMask_Heap | m_registerMask_Internal | m_registerMask_Potential;

                            set &= ~m_registerMask_Scratched;

                            if(set != 0)
                            {
                                uint mask = 1u;

                                for(int regNum = 0; regNum < 16; regNum++, mask <<= 1)
                                {
                                    if(( set & mask ) != 0)
                                    {
                                        UIntPtr ptr = ctx.GetRegisterByIndex( (uint)regNum );

                                        if(ptr != UIntPtr.Zero)
                                        {
                                            if(( m_registerMask_Heap & mask ) != 0)
                                            {
                                                m_owner.VisitHeapObject( ptr );
                                            }
                                            else
                                            {
                                                m_owner.VisitInternalPointer( ptr );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            uint set = m_stackMask_Heap | m_stackMask_Internal | m_stackMask_Potential;

                            if(set != 0)
                            {
                                uint mask = 1u;

                                for(int offset = 0; offset < c_StackMarkSize; offset++, mask <<= 1)
                                {
                                    if(( set & mask ) != 0)
                                    {
                                        UIntPtr* stack = (UIntPtr*)ctx.StackPointer.ToPointer( );
                                        UIntPtr ptr = stack[ m_stackLow + offset ];

                                        if(ptr != UIntPtr.Zero)
                                        {
                                            if(( m_stackMask_Heap & mask ) != 0)
                                            {
                                                m_owner.VisitHeapObject( ptr );
                                            }
                                            else
                                            {
                                                m_owner.VisitInternalPointer( ptr );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(m_done)
                        {
                            break;
                        }

                        m_stackLow += c_StackMarkSize;
                        m_stackHigh += c_StackMarkSize;
                    }

                    //
                    // Exclude scratched registers from the set of live registers.
                    //
                    m_registerMask_Scratched = ctx.ScratchedIntegerRegisters;

                    if(ctx.Unwind( ) == false)
                    {
                        break;
                    }
                }
            }

        }

        //--//

        protected override int MarkStackForObjectsSize
        {
            get { return 1024; }
        }
        protected override int MarkStackForArraysSize
        {
            get { return 128; }
        }

        protected override MarkAndSweepStackWalker CreateStackWalker( )
        {
            return new PreciseStackWalker( this );
        }
    }
}

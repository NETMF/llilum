//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.AllowCompileTimeIntrospection]
    public class CodeMap
    {
        [Flags]
        [TS.AllowCompileTimeIntrospection]
        public enum Flags : uint
        {
            NormalCode              = 0x00000001,
            EntryPoint              = 0x00000002,
            ExceptionHandler        = 0x00000004,
            InterruptHandler        = 0x00000008,
                               
            ColdSection             = 0x00000010,
                               
            BottomOfCallStack       = 0x00000020,

            HasStackAdjustment      = 0x10000000,
            HasFpStatusRegisterSave = 0x20000000,
            HasFpRegisterSave       = 0x40000000,
            HasIntRegisterSave      = 0x80000000,
        }

        [Flags]
        [TS.AllowCompileTimeIntrospection]
        public enum EncodedStackWalk : byte
        {
            RegisterMask              = 0x0F,
            NotARegister              = 0x0F, // Since PC is 15, and there's no need to track PC changes.
                                    
            StackOffsetMask           = 0x1F, // Offset in WORDs.
            StackOffsetMax            = 0x1D, // Offset in WORDs.
            StackOffset8BitExtender   = 0x1E, // Next Pointer Tracking is a full 8bit stack offset.
            StackOffset16BitExtender  = 0x1F, // Next Pointer Tracking is a full 16bit stack offset.
                                    
            ModeMask                  = 0x1F,
            TrackingHeapPointers      = 0x00, // This is the default.
            TrackingInternalPointers  = 0x01,
            TrackingPotentialPointers = 0x02,

            SkipMask                  = 0x1F, // How many opcodes to skip.
            SkipMax                   = 0x1F,
                                     
            EffectMask                = 0xE0,
            EnterRegisterSet          = 0x00,
            LeaveRegisterSet          = 0x20,
            EnterStack                = 0x40,
            LeaveStack                = 0x60,
            SetMode                   = 0x80,
            EffectReserved2           = 0xA0,
            SkipToOpcode              = 0xE0,
        }

        [TS.AllowCompileTimeIntrospection]
        public struct Range
        {
            public static readonly Range[] SharedEmptyArray = new Range[0];

            //--//

            //
            // State
            //

            public Flags              Flags;
            public UIntPtr            Start;
            public UIntPtr            End;
            public EncodedStackWalk[] StackWalk;

            //
            // Equality Methods
            //

            public bool SameContents( ref Range other )
            {
                if(this.Flags == other.Flags &&
                   this.Start == other.Start &&
                   this.End   == other.End    )
                {
                    return EncodedStackWalkArrayEquals( this.StackWalk, other.StackWalk );
                }

                return false;
            }

            //
            // Helper Methods
            //

            public bool Contains( UIntPtr address )
            {
                return AddressMath.IsInRange( address, this.Start, this.End );
            }

            public void Decode( CodeMapDecoderCallback callback )
            {
                if(this.StackWalk != null)
                {
                    UIntPtr                            address = this.Start;
                    CodeMapDecoderCallback.PointerKind kind    = CodeMapDecoderCallback.PointerKind.Heap; // This is the default.

                    for(int pos = 0; pos < this.StackWalk.Length; )
                    {
                        EncodedStackWalk val       = this.StackWalk[pos++];
                        EncodedStackWalk effectVal = val & EncodedStackWalk.EffectMask;

                        switch(effectVal)
                        {
                            case EncodedStackWalk.EnterRegisterSet:
                                if(callback.RegisterEnter( address, (uint)(val & EncodedStackWalk.RegisterMask), kind ) == false)
                                {
                                    return;
                                }
                                break;

                            case EncodedStackWalk.LeaveRegisterSet:
                                if(callback.RegisterLeave( address, (uint)(val & EncodedStackWalk.RegisterMask) ) == false)
                                {
                                    return;
                                }
                                break;

                            case EncodedStackWalk.EnterStack:
                            case EncodedStackWalk.LeaveStack:
                                {
                                    uint offset = (uint)(val & EncodedStackWalk.StackOffsetMask);
                                    switch(offset)
                                    {
                                        case (uint)EncodedStackWalk.StackOffset8BitExtender:
                                            offset = (uint)this.StackWalk[pos++];
                                            break;

                                        case (uint)EncodedStackWalk.StackOffset16BitExtender:
                                            offset  = (uint)this.StackWalk[pos++]; 
                                            offset |= (uint)this.StackWalk[pos++] << 8;
                                            break;
                                    }

                                    if(effectVal == EncodedStackWalk.EnterStack)
                                    {
                                        if(callback.StackEnter( address, offset, kind ) == false)
                                        {
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if(callback.StackLeave( address, offset ) == false)
                                        {
                                            return;
                                        }
                                    }
                                }
                                break;

                            case EncodedStackWalk.SetMode:
                                switch(val & EncodedStackWalk.ModeMask)
                                {
                                    case EncodedStackWalk.TrackingHeapPointers:
                                        kind = CodeMapDecoderCallback.PointerKind.Heap;
                                        break;

                                    case EncodedStackWalk.TrackingInternalPointers:
                                        kind = CodeMapDecoderCallback.PointerKind.Internal;
                                        break;

                                    case EncodedStackWalk.TrackingPotentialPointers:
                                        kind = CodeMapDecoderCallback.PointerKind.Potential;
                                        break;
                                }
                                break;

                            case EncodedStackWalk.SkipToOpcode:
                                address = AddressMath.Increment( address, (uint)(val & EncodedStackWalk.SkipMask) * sizeof(uint) );
                                break;
                        }
                    }
                }
            }

            public static bool EncodedStackWalkArrayEquals( EncodedStackWalk[] s ,
                                                            EncodedStackWalk[] d )
            {
                int sLen = s != null ? s.Length : 0;
                int dLen = d != null ? d.Length : 0;

                if(sLen == dLen)
                {
                    for(int i = 0; i < sLen; i++)
                    {
                        if(s[i] != d[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }


            //
            // Debug Methods
            //

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendFormat( "Range[0x{0:X8}-0x{1:X8} {2} {3} Entries]", this.Start.ToUInt32(), this.End.ToUInt32(), this.Flags, this.StackWalk != null ? this.StackWalk.Length : 0 );

                if(this.StackWalk != null)
                {
                    uint address = this.Start.ToUInt32();

                    for(int pos = 0; pos < this.StackWalk.Length; )
                    {
                        EncodedStackWalk val       = this.StackWalk[pos++];
                        EncodedStackWalk effectVal = val & EncodedStackWalk.EffectMask;

                        sb.Append( Environment.NewLine );

                        switch(effectVal)
                        {
                            case EncodedStackWalk.EnterRegisterSet:
                                sb.AppendFormat( "Enter Reg{0}", (uint)(val & EncodedStackWalk.RegisterMask) );
                                break;

                            case EncodedStackWalk.LeaveRegisterSet:
                                sb.AppendFormat( "Leave Reg{0}", (uint)(val & EncodedStackWalk.RegisterMask) );
                                break;

                            case EncodedStackWalk.EnterStack:
                            case EncodedStackWalk.LeaveStack:
                                {
                                    uint offset = (uint)(val & EncodedStackWalk.StackOffsetMask);
                                    switch(offset)
                                    {
                                        case (uint)EncodedStackWalk.StackOffset8BitExtender:
                                            offset = (uint)this.StackWalk[pos++];
                                            break;

                                        case (uint)EncodedStackWalk.StackOffset16BitExtender:
                                            offset  = (uint)this.StackWalk[pos++]; 
                                            offset |= (uint)this.StackWalk[pos++] << 8;
                                            break;
                                    }

                                    if(effectVal == EncodedStackWalk.EnterStack)
                                    {
                                        sb.AppendFormat( "Enter Stack{0}", offset );
                                    }
                                    else
                                    {
                                        sb.AppendFormat( "Leave Stack{0}", offset );
                                    }
                                }
                                break;

                            case EncodedStackWalk.SetMode:
                                switch(val & EncodedStackWalk.ModeMask)
                                {
                                    case EncodedStackWalk.TrackingHeapPointers:
                                        sb.Append( "Switch to Tracking Heap Pointers" );
                                        break;

                                    case EncodedStackWalk.TrackingInternalPointers:
                                        sb.Append( "Switch to Tracking Internal Pointers" );
                                        break;

                                    case EncodedStackWalk.TrackingPotentialPointers:
                                        sb.Append( "Switch to Tracking Potential Pointers" );
                                        break;
                                }
                                break;

                            case EncodedStackWalk.SkipToOpcode:
                                address += (uint)(val & EncodedStackWalk.SkipMask) * sizeof(uint);

                                sb.AppendFormat( "Skip to 0x{0:X8}", address );
                                break;
                        }
                    }
                }

                return sb.ToString();
            }
        }

        //
        // All the CodeMap instances are put in an address-sorted array, adding an extra entry with no CodeMap to act as a sentinel.
        // This array will be used to resolve an address to a piece of code.
        // A CodeMap instance can appear more than once, if it's been split into multiple chunks.
        //
        [TS.AllowCompileTimeIntrospection]
        public struct ReverseIndex
        {
            //
            // State
            //

            public UIntPtr Address;
            public CodeMap Code;

            //
            // Equality Methods
            //

            public static bool SameArrayContents( ReverseIndex[] left  ,
                                                  ReverseIndex[] right )
            {
                int leftLen  = left  != null ? left .Length : 0;
                int rightLen = right != null ? right.Length : 0;

                if(leftLen == rightLen)
                {
                    for(int i = 0; i < leftLen; i++)
                    {
                        if(left[i].SameContents( ref right[i] ) == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public bool SameContents( ref ReverseIndex other )
            {
                if(this.Address == other.Address)
                {
                    return CodeMap.SameContents( this.Code, other.Code );
                }

                return false;
            }
        }

        //--//

        //
        // State
        //

        [WellKnownField( "CodeMap_LookupAddress"   )] public static ReverseIndex[]       LookupAddress;

        [WellKnownField( "CodeMap_Target"          )] public        MethodRepresentation Target;
        [WellKnownField( "CodeMap_Ranges"          )] public        Range[]              Ranges;
        [WellKnownField( "CodeMap_ExceptionMap"    )] public        ExceptionMap         ExceptionMap;

        //
        // Equality Methods
        //

        public static bool SameContents( CodeMap left  ,
                                         CodeMap right )
        {
            if(Object.ReferenceEquals( left, right ))
            {
                return true;
            }

            if(left != null && right != null)
            {
                if(Object.ReferenceEquals( left.Target, right.Target ))
                {
                    if(ExceptionMap.SameContents( left.ExceptionMap, right.ExceptionMap ))
                    {
                        Range[] leftRanges  = left .Ranges;
                        Range[] rightRanges = right.Ranges;
                        int     leftLen     = leftRanges  != null ? leftRanges .Length : 0;
                        int     rightLen    = rightRanges != null ? rightRanges.Length : 0;

                        if(leftLen == rightLen)
                        {
                            for(int i = 0; i < leftLen; i++)
                            {
                                if(leftRanges[i].SameContents( ref rightRanges[i] ) == false)
                                {
                                    return false;
                                }
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool SameContents( CodeMap other )
        {
            if(Object.ReferenceEquals( this.Target, other.Target ))
            {
                if(ExceptionMap.SameContents( this.ExceptionMap, other.ExceptionMap ))
                {
                    Range[] thisRanges  = this .Ranges;
                    Range[] rightRanges = other.Ranges;
                    int     thisLen     = thisRanges  != null ? thisRanges .Length : 0;
                    int     rightLen    = rightRanges != null ? rightRanges.Length : 0;

                    if(thisLen == rightLen)
                    {
                        for(int i = 0; i < thisLen; i++)
                        {
                            if(thisRanges[i].SameContents( ref rightRanges[i] ) == false)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        //
        // Helper Methods
        //

        [NoInline]
        public int FindRange( UIntPtr address )
        {
            for(int i = 0; i < this.Ranges.Length; i++)
            {
                if(this.Ranges[i].Contains( address ))
                {
                    return i;
                }
            }

            return -1;
        }

        [NoInline]
        public static CodeMap ResolveAddressToCodeMap( UIntPtr address )
        {
            return ResolveAddressToCodeMap( address, CodeMap.LookupAddress );
        }

        [NoInline]
        [DisableBoundsChecks]
        public static CodeMap ResolveAddressToCodeMap( UIntPtr                address ,
                                                       CodeMap.ReverseIndex[] table   )
        {
            if(table != null)
            {
                int low  = 0;
                int high = table.Length - 2; // The last item is sentinel, so we don't need to check "mid+1 < length".

                while(low <= high)
                {
                    int mid = (high + low) / 2;

                    if(AddressMath.IsLessThan( address, table[mid].Address ))
                    {
                        high = mid - 1;
                    }
                    else if(AddressMath.IsGreaterThanOrEqual( address, table[mid+1].Address ))
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        return table[mid].Code;
                    }
                }
            }

            return null;
        }
    }
}

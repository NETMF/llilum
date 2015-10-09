//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LLVM

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class Memory
    {
        [TS.AllowCompileTimeIntrospection]
        public class Range
        {
            public enum SubstractionAction
            {
                RemoveNothing   ,
                RemoveStart     ,
                RemoveMiddle    ,
                RemoveEnd       ,
                RemoveEverything,
            }

            //
            // State
            //

            public readonly UIntPtr          Start;
            public readonly UIntPtr          End;
            public readonly string           SectionName;
            public readonly MemoryAttributes Attributes;
            public readonly MemoryUsage      Usage;
            public readonly Type             ExtensionHandler;

            //
            // Constructor Methods
            //

            public Range( UIntPtr          start            ,
                          UIntPtr          end              ,
                          string           sectionName      ,
                          MemoryAttributes attributes       ,
                          MemoryUsage      usage            ,
                          Type             extensionHandler )
            {
                this.Start            = start;
                this.End              = end;
                this.SectionName      = sectionName;
                this.Attributes       = attributes;
                this.Usage            = usage;
                this.ExtensionHandler = extensionHandler;
            }

            //
            // Equality Methods
            //

            public bool Equals( Range other )
            {
                if(this.Start      == other.Start      &&
                   this.End        == other.End        &&
                   this.Attributes == other.Attributes  )
                {
                    return true;
                }

                return false;
            }

            public static bool ArrayEquals( Range[] left  ,
                                            Range[] right )
            {
                int leftLen  = left  != null ? left .Length : 0;
                int rightLen = right != null ? right.Length : 0;

                if(leftLen == rightLen)
                {
                    for(int i = 0; i < leftLen; i++)
                    {
                        if(left[i].Equals( right[i] ) == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            //
            // Helper Methods
            //

            public Range CloneSettings( UIntPtr start ,
                                        UIntPtr end   )
            {
                return new Runtime.Memory.Range( start, end, this.SectionName, this.Attributes, this.Usage, this.ExtensionHandler );
            }

            public bool Contains( uint address )
            {
                return AddressMath.IsInRange( new UIntPtr( address ), this.Start, this.End );
            }

            public SubstractionAction ComputeSubstraction( UIntPtr blockStart ,
                                                           UIntPtr blockEnd   )
            {
                bool fIsStartToTheLeft  = AddressMath.IsGreaterThanOrEqual( this.Start, blockStart );
                bool fIsStartToTheRight = AddressMath.IsLessThanOrEqual   ( this.End  , blockStart );
                bool fIsEndToTheLeft    = AddressMath.IsGreaterThanOrEqual( this.Start, blockEnd   );
                bool fIsEndToTheRight   = AddressMath.IsLessThanOrEqual   ( this.End  , blockEnd   );

                if(fIsEndToTheLeft || fIsStartToTheRight)
                {
                    return SubstractionAction.RemoveNothing;
                }

                if(fIsStartToTheLeft && fIsEndToTheRight)
                {
                    return SubstractionAction.RemoveEverything;
                }

                if(!fIsStartToTheLeft && !fIsEndToTheRight)
                {
                    return SubstractionAction.RemoveMiddle;
                }

                return fIsStartToTheLeft ? SubstractionAction.RemoveStart : SubstractionAction.RemoveEnd;
            }

            //
            // Access Methods
            //

            public uint Size
            {
                get
                {
                    return AddressMath.RangeSize( this.Start, this.End );
                }
            }
        }

        [TS.AllowCompileTimeIntrospection]
        public class RelocationInfo
        {
            //
            // State
            //

                                                           private UIntPtr m_destination;
                                                           private uint    m_size;
            [TS.WellKnownField( "RelocationInfo_m_data" )] private uint[]  m_data;

            //
            // Constructor Methods
            //

            public RelocationInfo( UIntPtr destination ,
                                   uint[]  data        ,
                                   int     offset      ,
                                   int     count       ) 
            {
                m_destination =       destination;
                m_size        = (uint)count;
                m_data        =       ArrayUtility.ExtractSliceFromNotNullArray( data, offset, count );
            }

            public RelocationInfo( UIntPtr destination ,
                                   uint    clearSize   ) 
            {
                m_destination = destination;
                m_size        = clearSize;
                m_data        = null;
            }

            //
            // Equality Methods
            //

            public bool Equals( ref RelocationInfo other )
            {
                if(this.m_destination == other.m_destination &&
                   this.m_size        == other.m_size         )
                {
                    return ArrayUtility.UIntArrayEquals( this.m_data, other.m_data );
                }

                return false;
            }

            public static bool ArrayEquals( RelocationInfo[] left  ,
                                            RelocationInfo[] right )
            {
                int leftLen  = left  != null ? left .Length : 0;
                int rightLen = right != null ? right.Length : 0;

                if(leftLen == rightLen)
                {
                    for(int i = 0; i < leftLen; i++)
                    {
                        if(left[i].Equals( ref right[i] ) == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            //
            // Access Methods
            //

            public bool IsEraseBlock
            {
                get
                {
                    return m_data == null;
                }
            }

            public uint SizeInWords
            {
                get
                {
                    return m_size;
                }
            }

            public unsafe UIntPtr Start
            {
                get
                {
                    ArrayImpl array = ArrayImpl.CastAsArray( m_data );

                    return new UIntPtr( array.GetDataPointer() );
                }
            }

            public UIntPtr End
            {
                get
                {
                    return AddressMath.Increment( this.Start, m_size * sizeof(uint) );
                }
            }

            public UIntPtr Destination
            {
                get
                {
                    return m_destination;
                }
            }
        }

        //--//

        //
        // State
        //

#pragma warning disable 649 // These fields are populated at code generation.
        [TS.WellKnownField( "Memory_m_availableMemory" )] private readonly Range[]          m_availableMemory;
        [TS.WellKnownField( "Memory_m_relocationInfo"  )] private readonly RelocationInfo[] m_relocationInfo;
#pragma warning restore 649

        //
        // Helper Methods
        //

        public abstract void InitializeMemory();

        [DisableNullChecks( ApplyRecursively=true )]
        [DisableBoundsChecks( ApplyRecursively=true )]
        [MemoryUsage(MemoryUsage.Bootstrap)]
        public virtual unsafe void ExecuteImageRelocation()
        {
            RelocationInfo[] array = m_relocationInfo;

            for(int i = 0; i < array.Length; i++)
            {
                RelocationInfo ri = array[i];

                if(ri.IsEraseBlock)
                {
                    uint* dst   = (uint*)ri.Destination.ToPointer();
                    uint  count = ri.SizeInWords;

                    while(count != 0)
                    {
                        *dst++ = 0;
                        count--;
                    }
                }
                else
                {
                    Memory.CopyNonOverlapping( ri.Start, ri.End, ri.Destination );
                }
            }
        }

        //--//

        [DisableNullChecks]
        public static void Zero( UIntPtr start ,
                                 UIntPtr end   )
        {
            Fill( start, end, 0 );
        }

        public static void Dirty( UIntPtr start ,
                                  UIntPtr end   )
        {
            Fill( start, end, 0xDEADBEEF );
        }

        [DisableNullChecks]
        public static unsafe void Fill( UIntPtr start ,
                                        UIntPtr end   ,
                                        uint    value )
        {
            uint* startPtr = (uint*)start.ToPointer();
            uint* endPtr   = (uint*)end  .ToPointer();
            
            BugCheck.Assert( startPtr != null, BugCheck.StopCode.HeapCorruptionDetected );
            BugCheck.Assert( endPtr   != null, BugCheck.StopCode.HeapCorruptionDetected );

            endPtr -= 7;

            while(startPtr < endPtr)
            {
                startPtr[0] = value;
                startPtr[1] = value;
                startPtr[2] = value;
                startPtr[3] = value;
                startPtr[4] = value;
                startPtr[5] = value;
                startPtr[6] = value;
                startPtr[7] = value;

                startPtr += 8;
            }

            uint leftOver = (uint)((endPtr + 7) - startPtr);

            switch(leftOver)
            {
                case 7: startPtr[6] = value; goto case 6;
                case 6: startPtr[5] = value; goto case 5;
                case 5: startPtr[4] = value; goto case 4;
                case 4: startPtr[3] = value; goto case 3;
                case 3: startPtr[2] = value; goto case 2;
                case 2: startPtr[1] = value; goto case 1;
                case 1: startPtr[0] = value; break;
            }
        }

        [DisableNullChecks( ApplyRecursively=true )]
        public static unsafe void CopyNonOverlapping( UIntPtr srcStart ,
                                                      UIntPtr srcEnd   ,
                                                      UIntPtr dstStart )
        {
            uint* srcPtr = (uint*)srcStart.ToPointer();
            uint* endPtr = (uint*)srcEnd  .ToPointer();
            uint* dstPtr = (uint*)dstStart.ToPointer();

            while(srcPtr < endPtr)
            {
                *dstPtr++ = *srcPtr++;
            }
        }

        //
        // Access Methods
        //

        public static extern Memory Instance
        {
            [SingletonFactory(ReadOnly=true)]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public Range[] AvailableMemory
        {
            get
            {
                return m_availableMemory;
            }
        }

        public RelocationInfo[] RelocationData
        {
            get
            {
                return m_relocationInfo;
            }
        }
    }
}

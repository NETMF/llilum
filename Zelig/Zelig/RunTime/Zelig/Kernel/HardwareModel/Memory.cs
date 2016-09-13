//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


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
                    UIntPtr destinationEnd = AddressMath.Increment( ri.Destination, ri.SizeInWords * sizeof( uint ) );
                    Memory.Zero(ri.Destination, destinationEnd);
                }
                else
                {
                    Buffer.InternalMemoryMove(
                        (byte*)ri.Start.ToPointer(),
                        (byte*)ri.Destination.ToPointer(),
                        (int)AddressMath.RangeSize( ri.Start, ri.End ) );
                }
            }
        }

        //--//

        [Inline]
        [DisableNullChecks]
        public static void Zero( UIntPtr start ,
                                 UIntPtr end   )
        {
            Fill( start, end, 0 );
        }

        [Inline]
        public static void Dirty( UIntPtr start ,
                                  UIntPtr end   )
        {
            Fill( start, end, 0xDD );
        }

        [Inline]
        [DisableNullChecks]
        public static unsafe void Fill( UIntPtr start ,
                                        UIntPtr end   ,
                                        byte    value )
        {
            byte* startPtr = (byte*)start.ToPointer();
            byte* endPtr   = (byte*)end.  ToPointer();

            Fill( startPtr, (int)(endPtr - startPtr), value );
        }

        [NoInline]
        [TS.WellKnownMethod( "Microsoft_Zelig_Runtime_Memory_Fill" )]
        public static unsafe void Fill( byte* dst,
                                        int size,
                                        byte value )
        {
            byte* end = dst + size;
            while( dst < end )
            {
                *dst = value;
                ++dst;
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

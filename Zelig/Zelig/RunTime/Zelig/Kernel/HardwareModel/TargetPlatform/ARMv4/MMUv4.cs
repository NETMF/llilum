//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv4
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public static class MMUv4
    {
        public const uint FirstLevelKind__Fault     = 0x00;
        public const uint FirstLevelKind__Coarse    = 0x11;
        public const uint FirstLevelKind__Section   = 0x12;
        public const uint FirstLevelKind__Fine      = 0x13;
        public const uint FirstLevelKind__MASK      = 0x13;

        public const int  FirstLevelKind__AP__shift = 10;

        public const uint FirstLevelKind__Cacheable = 1U << 3;
        public const uint FirstLevelKind__Buffered  = 1U << 2;

        //--//

        public const uint CoarseKind__Fault = 0x00;
        public const uint CoarseKind__Large = 0x01;
        public const uint CoarseKind__Small = 0x02;
        public const uint CoarseKind__Tiny  = 0x03;
        public const uint CoarseKind__MASK  = 0x03;

        public const uint CoarseKind__Cacheable = 1U << 3;
        public const uint CoarseKind__Buffered  = 1U << 2;

        public const int  CoarseKind__AP0__shift =  4;
        public const int  CoarseKind__AP1__shift =  6;
        public const int  CoarseKind__AP2__shift =  8;
        public const int  CoarseKind__AP3__shift = 10;

        //--//

        public const uint AP__NoAccess = 0;
        public const uint AP__Client   = 1;
        public const uint AP__Reserved = 2;
        public const uint AP__Manager  = 3;
        public const uint AP__MASK     = 3;

        //--//

        private const uint c_TLB_FirstLevelSlots   =               4 * 1024;
        private const uint c_TLB_FirstLevelSize    =  c_TLB_FirstLevelSlots * sizeof(uint);
        private const uint c_TLB_FirstLevelOffset  =            1024 * 1024;

        public  const uint c_TLB_SecondLevelSlots  =                    256;
        public  const uint c_TLB_SecondLevelSize   = c_TLB_SecondLevelSlots * sizeof(uint);
        private const uint c_TLB_SecondLevelOffset =               4 * 1024;

        public  const uint c_SectionSize           =            1024 * 1024;
        public  const uint c_CoarsePageSize        =               4 * 1024;

        //
        // State
        //

        [AlignmentRequirements( c_TLB_FirstLevelSize, sizeof(uint) )]
        static uint[] s_TLB_FirstLevel = new uint[c_TLB_FirstLevelSlots];

        //--//

        public static unsafe void EnableTLB()
        {
#if SLOW
            const uint mask = Coprocessor15.c_ControlRegister__ICache |
                              Coprocessor15.c_ControlRegister__DCache ;
#else
            const uint mask = Coprocessor15.c_ControlRegister__ICache |
                              Coprocessor15.c_ControlRegister__DCache |
                              Coprocessor15.c_ControlRegister__MMU    ;
#endif

            //
            // Disable MMU and caches.
            //
            Coprocessor15.ResetControlRegisterBits( mask );

            Coprocessor15.InvalidateMMUTLBs();

            Coprocessor15.InvalidateCaches();
            Coprocessor15.DrainWriteBuffer();

            Coprocessor15.SetDomainAccessControl( 0xFFFFFFFF );

            fixed(uint* ptr = s_TLB_FirstLevel)
            {
                Coprocessor15.SetTranslationTableBaseRegister( ptr );
            }

            //
            // Enable MMU and caches.
            //
            Coprocessor15.SetControlRegisterBits( mask );
            ProcessorARMv4.Nop();
            ProcessorARMv4.Nop();

            //
            // Invalidate MMU TLBs.
            //
            Coprocessor15.InvalidateMMUTLBs();
        }

        public static void ClearTLB()
        {
            for(int pos = 0; pos < s_TLB_FirstLevel.Length; pos++)
            {
                s_TLB_FirstLevel[pos] = FirstLevelKind__Fault;
            }
        }

        public static void ConfigureRangeAsSections( UIntPtr startAddress    ,
                                                     UIntPtr endAddress      ,
                                                     UIntPtr physicalAddress ,
                                                     uint    AP              ,
                                                     bool    fCacheable      ,
                                                     bool    fBuffered       )
        {
            uint bits = FirstLevelKind__Section | ((AP & AP__MASK) << FirstLevelKind__AP__shift);

            if(fCacheable) bits |= FirstLevelKind__Cacheable;
            if(fBuffered ) bits |= FirstLevelKind__Buffered;

            for(int diff = AddressMath.Distance( startAddress, endAddress ); diff > 0; diff -= (int)c_TLB_FirstLevelOffset)
            {
                UIntPtr baseAddress = AddressMath.AlignToLowerBoundary        ( physicalAddress, c_SectionSize          );
                uint    index       = AddressMath.IndexRelativeToLowerBoundary( startAddress   , c_TLB_FirstLevelOffset );

                s_TLB_FirstLevel[index] = baseAddress.ToUInt32() | bits;

                startAddress     = AddressMath.Increment( startAddress   , c_TLB_FirstLevelOffset );
                physicalAddress  = AddressMath.Increment( physicalAddress, c_TLB_FirstLevelOffset );
            }
        }

        public static unsafe void ConfigureRangeAsCoarsePages( UIntPtr startAddress          ,
                                                               UIntPtr endAddress            ,
                                                               uint[]  secondLevelDescriptor )
        {
            fixed(uint* physicalAddressPtr = secondLevelDescriptor)
            {
                UIntPtr physicalAddress = new UIntPtr( physicalAddressPtr );

                uint bits = FirstLevelKind__Coarse;

                for(int diff = AddressMath.Distance( startAddress, endAddress ); diff > 0; diff -= (int)c_TLB_FirstLevelOffset)
                {
                    UIntPtr baseAddress = AddressMath.AlignToLowerBoundary        ( physicalAddress, c_TLB_SecondLevelSize  );
                    uint    index       = AddressMath.IndexRelativeToLowerBoundary( startAddress   , c_TLB_FirstLevelOffset );

                    s_TLB_FirstLevel[index] = baseAddress.ToUInt32() | bits;

                    startAddress    = AddressMath.Increment( startAddress   , c_TLB_FirstLevelOffset );
                    physicalAddress = AddressMath.Increment( physicalAddress, c_TLB_FirstLevelOffset );
                }
            }
        }

        public static void ConfigureSecondLevelAsSmallPages( UIntPtr startAddress          ,
                                                             UIntPtr endAddress            ,
                                                             UIntPtr physicalAddress       ,
                                                             uint[]  secondLevelDescriptor ,
                                                             uint    AP                    ,
                                                             bool    fCacheable            ,
                                                             bool    fBuffered             )
        {
            uint bits = CoarseKind__Small;

            bits |= (AP & AP__MASK) << CoarseKind__AP0__shift;
            bits |= (AP & AP__MASK) << CoarseKind__AP1__shift;
            bits |= (AP & AP__MASK) << CoarseKind__AP2__shift;
            bits |= (AP & AP__MASK) << CoarseKind__AP3__shift;

            if(fCacheable) bits |= CoarseKind__Cacheable;
            if(fBuffered ) bits |= CoarseKind__Buffered;

            for(int diff = AddressMath.Distance( startAddress, endAddress ); diff > 0; diff -= (int)c_CoarsePageSize)
            {
                UIntPtr baseAddress = AddressMath.AlignToLowerBoundary        ( physicalAddress, c_CoarsePageSize );
                uint    index       = AddressMath.IndexRelativeToLowerBoundary( startAddress   , c_CoarsePageSize );

                secondLevelDescriptor[index] = baseAddress.ToUInt32() | bits;

                startAddress    = AddressMath.Increment( startAddress   , c_CoarsePageSize );
                physicalAddress = AddressMath.Increment( physicalAddress, c_CoarsePageSize );
            }
        }

        //--//

        public static void AddCacheableSection( uint start    ,
                                                uint end      ,
                                                uint physical )
        {
            ConfigureRangeAsSections( new UIntPtr( start ), new UIntPtr( end ), new UIntPtr( physical ), MMUv4.AP__Manager, true, true );
        }

        public static void AddUncacheableSection( uint start    ,
                                                  uint end      ,
                                                  uint physical )
        {
            ConfigureRangeAsSections( new UIntPtr( start ), new UIntPtr( end ), new UIntPtr( physical ), MMUv4.AP__Manager, false, false );
        }

        public static void AddCoarsePages( uint   start ,
                                           uint   end   ,
                                           uint[] table )
        {
            MMUv4.ConfigureRangeAsCoarsePages( new UIntPtr( start ), new UIntPtr( end ), table );
        }

        public static void AddCacheableCoarsePages( uint   start    ,
                                                    uint   end      ,
                                                    uint   physical ,
                                                    uint[] table    )
        {
            ConfigureSecondLevelAsSmallPages( new UIntPtr( start ), new UIntPtr( end ), new UIntPtr( physical ), table, MMUv4.AP__Manager, true, true );
        }
    }
}

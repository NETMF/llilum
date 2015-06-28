//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;

    #region VOX PLATFORM
    [DisplayName( "Custom" )]
    public sealed class Custom : ProductCategory
    {
        [AllowedOptions( typeof( MM9691LP ) )]
        [Defaults( "CoreClockFrequency", 30000000UL )]
        [Defaults( "PeripheralsClockFrequency", 30000000UL )]
        [Defaults( "RealTimeClockFrequency", 32768UL )]
        public ProcessorCategory Processor;

        [Requires( "WordSize", 32 )]
        [Requires( "WordSize", 16 )]
        [Defaults( "BaseAddress", 0x10000000U )]
        [Defaults( "ConnectedToBus", typeof( MM9691LP.CacheController ) )]
        public FlashMemoryCategory FlashChip;

        [Defaults( "BaseAddress", 0x20000000U )]
        public DisplayCategory Display;
    }

    //--//--//--//

    [DisplayName( "VOX Solo Form Factor Board" )]
    public sealed class VoxSoloFormFactor : ProductCategory
    {
        class VoxSoloFormFactorLoader : NorFlashJtagLoaderCategory
        {
            public VoxSoloFormFactorLoader( )
                : base( "VoxSoloFormFactorLoader", Microsoft.Zelig.ProductConfiguration.Loaders.Microsoft_VoxSoloFormFactorLoader )
            {
            }
        }

        [AllowedOptions( typeof( VoxSoloFormFactorLoader ) )]
        [Defaults( "CanSetBreakpointsDuringReset", true )]
        [Defaults( "DriverName", "ARM7TDMI-S" )]
        public JtagLoaderCategory Loader;

        [AllowedOptions( typeof( MM9691LP ) )]
        [Defaults( "CoreClockFrequency", 30000000UL )]
        [Defaults( "PeripheralsClockFrequency", 30000000UL )]
        [Defaults( "RealTimeClockFrequency", 32768UL )]
        public ProcessorCategory Processor;

        [AllowedOptions( typeof( S29WS128N ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
        [Requires( "WordSize", 16 )]
        [Defaults( "ConnectedToBus", typeof( MM9691LP.CacheController ) )]
        public FlashMemoryCategory FlashChip;

        [AllowedOptions( typeof( SED15E0 ) )]
        [Defaults( "BaseAddress", 0x20000000U )]
        [Defaults( "ConnectedToBus", typeof( MM9691LP.CacheController ) )]
        public DisplayCategory Display;

        [HardwareModel( typeof( Emulation.ArmProcessor.VoxSoloFormFactor.Interop ), HardwareModelAttribute.Kind.Interop )]
        public InteropCategory[] Interops;
    }

    [DisplayName( "VOX Solo Form Factor Board For RAM" )]
    public sealed class VoxSoloFormFactorForRAM : ProductCategory
    {
        class VoxSoloFormFactorLoader : RamJtagLoaderCategory
        {
            public VoxSoloFormFactorLoader( )
            {
            }
        }

        [AllowedOptions( typeof( VoxSoloFormFactorLoader ) )]
        [Defaults( "CanSetBreakpointsDuringReset", true )]
        [Defaults( "DriverName", "ARM7TDMI-S" )]
        public JtagLoaderCategory Loader;

        [AllowedOptions( typeof( MM9691LP ) )]
        [Defaults( "CoreClockFrequency", 30000000UL )]
        [Defaults( "PeripheralsClockFrequency", 30000000UL )]
        [Defaults( "RealTimeClockFrequency", 32768UL )]
        public ProcessorCategory Processor;

        [AllowedOptions( typeof( S29WS128N ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
        [Requires( "WordSize", 16 )]
        [Defaults( "ConnectedToBus", typeof( MM9691LP.CacheController ) )]
        public FlashMemoryCategory FlashChip;

        [AllowedOptions( typeof( SED15E0 ) )]
        [Defaults( "BaseAddress", 0x20000000U )]
        [Defaults( "ConnectedToBus", typeof( MM9691LP.CacheController ) )]
        public DisplayCategory Display;

        [HardwareModel( typeof( Emulation.ArmProcessor.VoxSoloFormFactor.Interop ), HardwareModelAttribute.Kind.Interop )]
        public InteropCategory[] Interops;
    }

    [DisplayName( "VOX Solo Form Factor Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV4 ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( VoxSoloFormFactor ) )]
    [Defaults( "MemoryMap", typeof( VoxSoloFormFactorMemoryMap ) )]
    public sealed class VoxSoloFormFactorCompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "VOX Solo Form Factor RAM Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV4 ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( VoxSoloFormFactorForRAM ) )]
    [Defaults( "MemoryMap", typeof( VoxSoloFormFactorRamMemoryMap ) )]
    public sealed class VoxSoloFormFactorRamCompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "VOX Solo Form Factor Memory Map" )]
    public sealed class VoxSoloFormFactorMemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.Relocation, ExtensionHandler = typeof( MM9691LP.MemoryMapper ) )]
        [AllowedOptions( typeof( S29WS128N ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
        public FlashMemoryCategory FlashChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [MemorySection( Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.Heap |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( MM9691LP.InternalRAM768KB ) )]
        public RamMemoryCategory RamChip;
    }

    [DisplayName( "VOX Solo Form Factor RAM Memory Map" )]
    public sealed class VoxSoloFormFactorRamMemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.Heap |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( MM9691LP.InternalRAM768KB ) )]
        [Defaults( "BaseAddress", 0x08000000U )]
        public RamMemoryCategory RamChip;

        [AllowedOptions( typeof( S29WS064J ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
        public FlashMemoryCategory FlashChip;
    }

    //--//

    [DisplayName( "VOX Solo Form Factor Loader Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV4 ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( VoxSoloFormFactorForRAM ) )]
    [Defaults( "MemoryMap", typeof( VoxSoloFormFactorLoaderMemoryMap ) )]
    public sealed class VoxSoloFormFactorLoaderCompilationSetup : LoaderCompilationSetupCategory
    {
    }

    [DisplayName( "VOX Solo Form Factor Loader Memory Map" )]
    public sealed class VoxSoloFormFactorLoaderMemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( MM9691LP.InternalRAM768KB ) )]
        [Defaults( "BaseAddress", 0x08000000U )]
        public RamMemoryCategory RamChip;

        [AllowedOptions( typeof( S29WS128N ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
        public FlashMemoryCategory FlashChip;
    }

    #endregion // VOX_PLATFORM

    //--//--//--//

    #region LPC3180 Platform

    [DisplayName( "NOHAU LPC3180 Board for RAM" )]
    public sealed class NohauLPC3180ForRAM : ProductCategory
    {
        class NohauLPC3180ForRAMLoader : RamJtagLoaderCategory
        {
            public NohauLPC3180ForRAMLoader( )
            {
            }
        }

        [AllowedOptions( typeof( NohauLPC3180ForRAMLoader ) )]
        [Defaults( "CanSetBreakpointsDuringReset", true )]
        [Defaults( "DriverName", "ARM9EJ-S" )]
        [Defaults( "Speed", 12000 )]
        public JtagLoaderCategory Loader;

        [AllowedOptions( typeof( LPC3180 ) )]
        [Defaults( "CoreClockFrequency", 208000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 32768UL )]
        [Defaults( "AHBClockFrequency", 104000000UL )]
        [Defaults( "DRAMWideBus", true )]
        public ProcessorCategory Processor;

        //--//

        [AllowedOptions( typeof( MT48H8M16LF ) )]
        [Defaults( "BaseAddress", 0x80000000U )]
        public RamMemoryCategory ExternalRamChip;

        [AllowedOptions( typeof( ST_NAND256A ) )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public FlashMemoryCategory ExternalFlashChip;
    }

    public abstract class NohauLPC3180Abstract : ProductCategory
    {
        class NohauLPC3180Loader : NandFlashJtagLoaderCategory
        {
            class ExtraDeploymentSteps : Emulation.Hosting.ExtraDeploymentSteps
            {
                //
                // Helper Methods
                //

                public override List<ImageSection> TransformImage( List<ImageSection> image )
                {
                    const uint baseIRAM    = 0x08000000;
                    const uint baseAddress = 0xA0000000;
                    const uint pageSize    = 512;

                    List< ImageSection > iramSections  = new List<ImageSection>( );
                    List< ImageSection > nandSections  = new List<ImageSection>( );
                    List< ImageSection > modifiedImage = new List<ImageSection>( );

                    foreach( ImageSection sec in image )
                    {
                        if( ( sec.Attributes & Runtime.MemoryAttributes.LoadedAtEntrypoint ) != 0 &&
                           ( sec.Usage & Runtime.MemoryUsage.Relocation ) == 0 )
                        {
                            iramSections.Add( sec );
                        }
                        else if( sec.NeedsRelocation == false )
                        {
                            nandSections.Add( sec );
                        }
                    }

                    uint lowestAddress  = uint.MaxValue;
                    uint highestAddress = uint.MinValue;

                    foreach( ImageSection sec in iramSections )
                    {
                        uint address = sec.Address;

                        lowestAddress = Math.Min( lowestAddress, address );
                        highestAddress = Math.Max( highestAddress, address + ( uint )sec.Payload.Length );
                    }

                    byte numPages = ( byte )( ( highestAddress + pageSize - 1 ) / pageSize );

                    modifiedImage.Add( CreateSection( baseAddress, CreateICF( numPages ) ) );

                    foreach( ImageSection sec in iramSections )
                    {
                        modifiedImage.Add( CreateSection( baseAddress + pageSize + ( sec.Address - baseIRAM ), sec.Payload ) );
                    }

                    modifiedImage.AddRange( nandSections );

                    return modifiedImage;
                }

                public override void ExecuteToEntryPoint( Emulation.Hosting.AbstractHost owner,
                                                          uint entryPointAddress )
                {
                    Emulation.Hosting.ProcessorControl svcPC;

                    if( owner.GetHostingService( out svcPC ) )
                    {
                        List< Emulation.Hosting.Breakpoint > lst = new List<Emulation.Hosting.Breakpoint>( );
                        Emulation.Hosting.Breakpoint         bp  = new Emulation.Hosting.Breakpoint( entryPointAddress, null, null );

                        bp.IsActive = true;

                        lst.Add( bp );

                        svcPC.Execute( lst );
                    }
                }

                //--//

                private static byte[] CreateICF( byte size )
                {
                    //  0x00000000  F0 FF FF FF F0 FF FF FF F0 FF FF FF F0 FF FF FF ................ << ICR
                    //  0x00000010  04 FF FF FF FB FF FF FF 04 FF FF FF FB FF FF FF ................ << Size (interleaved with ~Size)
                    //  0x00000020  04 FF FF FF FB FF FF FF 04 FF FF FF FB FF FF FF ................ << Size (interleaved with ~Size)
                    //  0x00000030  AA 00 00 00 FF FF FF FF FF FF FF FF FF FF FF FF ................ << OK marker

                    byte[] payload = new byte[ 512 ];

                    for( int i = 0; i < 512; i++ )
                    {
                        payload[ i ] = 0xFF;
                    }

                    for( int i = 0; i < 0x10; i += 4 )
                    {
                        payload[ i ] = 0xF0; // ICR
                    }

                    size += 1; // Include ICF block in the count.

                    for( int i = 0x10; i < 0x30; i += 8 )
                    {
                        payload[ i ] = size;
                        payload[ i + 4 ] = ( byte )~size;
                    }

                    payload[ 0x30 ] = 0xAA;
                    payload[ 0x31 ] = 0x00;
                    payload[ 0x32 ] = 0x00;
                    payload[ 0x33 ] = 0x00;

                    return payload;
                }

                private static ImageSection CreateSection( uint address,
                                                           byte[] payload )
                {
                    const Runtime.MemoryAttributes attrib = Runtime.MemoryAttributes.FLASH |
                                                            Runtime.MemoryAttributes.ExternalMemory |
                                                            Runtime.MemoryAttributes.BlockBasedMemory;
                    const Runtime.MemoryUsage      usage  = Runtime.MemoryUsage.Booter |
                                                            Runtime.MemoryUsage.Relocation;

                    return new ImageSection( address, payload, null, attrib, usage );
                }
            }

            public NohauLPC3180Loader( )
                : base( "NohauLPC3180Loader", Microsoft.Zelig.ProductConfiguration.Loaders.Microsoft_NohauLPC3180Loader )
            {
            }

            //--//

            protected override object GetServiceInner( Type t )
            {
                if( t == typeof( Emulation.Hosting.ExtraDeploymentSteps ) )
                {
                    return new ExtraDeploymentSteps( );
                }

                return base.GetServiceInner( t );
            }
        }

        [AllowedOptions( typeof( NohauLPC3180Loader ) )]
        [Defaults( "CanSetBreakpointsDuringReset", true )]
        [Defaults( "DriverName", "ARM9EJ-S" )]
        [Defaults( "Speed", 12000 )]
        public JtagLoaderCategory Loader;

        //--//

        [AllowedOptions( typeof( MT48H8M16LF ) )]
        [Defaults( "BaseAddress", 0x80000000U )]
        public RamMemoryCategory ExternalRamChip;

        [AllowedOptions( typeof( ST_NAND256A ) )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public FlashMemoryCategory ExternalFlashChip;
    }

    [DisplayName( "NOHAU LPC3180 Board" )]
    public sealed class NohauLPC3180 : NohauLPC3180Abstract
    {
        [AllowedOptions( typeof( LPC3180 ) )]
        [Defaults( "CoreClockFrequency", 208000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 32768UL )]
        [Defaults( "AHBClockFrequency", 104000000UL )]
        [Defaults( "DRAMWideBus", true )]
        public ProcessorCategory Processor;
    }

    [DisplayName( "NOHAU LPC3180 Board For Solo" )]
    public sealed class NohauLPC3180Solo : NohauLPC3180Abstract
    {
        [AllowedOptions( typeof( LPC3180 ) )]
        [Defaults( "CoreClockFrequency", 104000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 32768UL )]
        [Defaults( "AHBClockFrequency", 104000000UL )]
        [Defaults( "DRAMWideBus", false )]
        public ProcessorCategory Processor;
    }

    //--//

    [DisplayName( "NOHAU LPC3180 Loader Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV5_VFP ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( NohauLPC3180ForRAM ) )]
    [Defaults( "MemoryMap", typeof( NohauLPC3180LoaderMemoryMap ) )]
    public sealed class NohauLPC3180LoaderCompilationSetup : LoaderCompilationSetupCategory
    {
    }

    [DisplayName( "NOHAU LPC3180 Loader Memory Map" )]
    public sealed class NohauLPC3180LoaderMemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( LPC3180.InternalRAM64KB ) )]
        [Defaults( "BaseAddress", 0x08000000U )]
        public RamMemoryCategory RamChip;
    }

    //--//

    [DisplayName( "NOHAU LPC3180 Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV5_VFP ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( NohauLPC3180 ) )]
    [Defaults( "MemoryMap", typeof( NohauLPC3180MemoryMap ) )]
    public sealed class NohauLPC3180CompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "NOHAU LPC3180 For Solo Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV5_VFP ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( NohauLPC3180Solo ) )]
    [Defaults( "MemoryMap", typeof( NohauLPC3180MemoryMap ) )]
    public sealed class NohauLPC3180SoloCompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "NOHAU LPC3180 Memory Map" )]
    public sealed class NohauLPC3180MemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( LPC3180.InternalRAM64KB ) )]
        [ReserveBlock( 64 * 1024 - 4096, 4096, Reason = "This area is used by the ROM bootloader" )]
        [Defaults( "BaseAddress", 0x08000000U )]
        public RamMemoryCategory InternalRamChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory )]
        [MemorySection( Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.Heap |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( MT48H8M16LF ) )]
        [ReserveBlock( 1 * 8 * 1024 * 1024, 1 * 8 * 1024 * 1024, Reason = "This area is set aside for the Persistence Storage" )]
        [Defaults( "BaseAddress", 0x80000000U )]
        public RamMemoryCategory ExternalRamChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.BlockBasedMemory )]
        [MemorySection( Runtime.MemoryUsage.Booter |
                        Runtime.MemoryUsage.Relocation )]
        [ReserveBlock( 0x00000000, 2 * 32 * 1024, Reason = "This area is copied into internal RAM by LPC3180 during bootstrap" )]
        [AllowedOptions( typeof( ST_NAND256A ) )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public FlashMemoryCategory ExternalFlashChip;
    }

    #endregion //LPC3180 Platform

    //--//--//--//

    #region iMote2 Platform

    [DisplayName( "iMote2 Board for RAM" )]
    public sealed class iMote2ForRAM : ProductCategory
    {
        class iMote2ForRAMLoader : RamJtagLoaderCategory
        {
            public iMote2ForRAMLoader( )
            {
            }
        }

        [AllowedOptions( typeof( iMote2ForRAMLoader ) )]
        [Defaults( "DriverName", "PXA27x" )]
        [Defaults( "Speed", 12000 )]
        public JtagLoaderCategory Loader;

        [AllowedOptions( typeof( PXA27x ) )]
        [Defaults( "CoreClockFrequency", 416000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 1000000UL )]
        public ProcessorCategory Processor;

        //--//

        [Defaults( "BaseAddress", 0x5C000000U )]
        public RamMemoryCategory InternalRam;

        [AllowedOptions( typeof( MT48H16M16LF ) )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public RamMemoryCategory ExternalRamChip;

        [AllowedOptions( typeof( I28F256L18_16 ) )]
        [Defaults( "BaseAddress", 0x02000000U )]
        public FlashMemoryCategory ExternalFlashChip;
    }

    [DisplayName( "iMote2 Board" )]
    public sealed class iMote2 : ProductCategory
    {
        class iMote2Loader : XScaleNorFlashJTagLoaderCategory
        {
            public iMote2Loader( )
                : base( "iMote2Loader", Microsoft.Zelig.ProductConfiguration.Loaders.Microsoft_iMote2Loader )
            {
            }
        }

        [AllowedOptions( typeof( iMote2Loader ) )]
        [Defaults( "DriverName", "PXA27x" )]
        [Defaults( "Speed", 12000 )]
        public JtagLoaderCategory Loader;

        [AllowedOptions( typeof( PXA27x ) )]
        [Defaults( "CoreClockFrequency", 416000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 1000000UL )]
        public ProcessorCategory Processor;

        //--//

        [Defaults( "BaseAddress", 0x5C000000U )]
        public RamMemoryCategory InternalRam;

        [AllowedOptions( typeof( MT48H16M16LF ) )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public RamMemoryCategory ExternalRamChip;

        [AllowedOptions( typeof( I28F256L18_16 ) )]
        [Defaults( "BaseAddress", 0x02000000U )]
        public FlashMemoryCategory ExternalFlashChip;
    }

    //--//

    [DisplayName( "iMote2 Loader Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV4 ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( iMote2ForRAM ) )]
    [Defaults( "MemoryMap", typeof( iMote2LoaderMemoryMap ) )]
    public sealed class iMote2LoaderCompilationSetup : LoaderCompilationSetupCategory
    {
    }

    [DisplayName( "iMote2 Loader Memory Map" )]
    public sealed class iMote2LoaderMemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( PXA27x.InternalRAM256KB ) )]
        [Defaults( "BaseAddress", 0x5C000000U )]
        public RamMemoryCategory RamChip;
    }

    //--//

    [DisplayName( "iMote2 Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV4 ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.ArmCallingConvention ) )]
    [Defaults( "Product", typeof( iMote2 ) )]
    [Defaults( "MemoryMap", typeof( iMote2MemoryMap ) )]
    public sealed class iMote2CompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "iMote2 Memory Map" )]
    public sealed class iMote2MemoryMap : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.Relocation )]
        [AllowedOptions( typeof( I28F256L18_16 ) )]
        [Defaults( "BaseAddress", 0x02000000U )]
        public FlashMemoryCategory ExternalFlashChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [MemorySection( Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable |
                        Runtime.MemoryUsage.Code )]
        [AllowedOptions( typeof( PXA27x.InternalRAM256KB ) )]
        [Defaults( "BaseAddress", 0x5C000000U )]
        public RamMemoryCategory InternalRamChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory )]
        [MemorySection( Runtime.MemoryUsage.Heap |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( MT48H16M16LF ) )]
        [ReserveBlock( 2 * 8 * 1024 * 1024, 2 * 8 * 1024 * 1024, Reason = "This area is set aside for the Persistence Storage" )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public RamMemoryCategory ExternalRamChip;
    }

    [DisplayName( "iMote2 Memory Map with Code in FLASH" )]
    public sealed class iMote2MemoryMapForCodeInFlash : MemoryMapCategory
    {
        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                               Runtime.MemoryAttributes.LoadedAtEntrypoint )]
        [MemorySection( Runtime.MemoryUsage.Bootstrap |
                        Runtime.MemoryUsage.Code |
                        Runtime.MemoryUsage.DataRO |
                        Runtime.MemoryUsage.Relocation )]
        [AllowedOptions( typeof( I28F256L18_16 ) )]
        [Defaults( "BaseAddress", 0x02000000U )]
        public FlashMemoryCategory ExternalFlashChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.InternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory |
                                               Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [MemorySection( Runtime.MemoryUsage.Stack |
                        Runtime.MemoryUsage.VectorsTable )]
        [AllowedOptions( typeof( PXA27x.InternalRAM256KB ) )]
        [Defaults( "BaseAddress", 0x5C000000U )]
        public RamMemoryCategory InternalRamChip;

        [MergeEnumDefaults( "Characteristics", Runtime.MemoryAttributes.ExternalMemory |
                                               Runtime.MemoryAttributes.RandomAccessMemory )]
        [MemorySection( Runtime.MemoryUsage.Heap |
                        Runtime.MemoryUsage.DataRW )]
        [AllowedOptions( typeof( MT48H16M16LF ) )]
        [ReserveBlock( 2 * 8 * 1024 * 1024, 2 * 8 * 1024 * 1024, Reason = "This area is set aside for the Persistence Storage" )]
        [Defaults( "BaseAddress", 0xA0000000U )]
        public RamMemoryCategory ExternalRamChip;
    }

    #endregion //iMote2 Platform

    //--//--//--//

    #region LLVM Platform

    [DisplayName( "LLVM Hosted" )]
    public sealed class LLVMHosted : ProductCategory
    {
        [Defaults( "CoreClockFrequency", 416000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 1000000UL )]
        public ProcessorCategory Processor;
    }

    [DisplayName( "LLVM Hosted Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMPlatform ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMCallingConvention ) )]
    [Defaults( "Product", typeof( LLVMHosted ) )]
    [Defaults( "MemoryMap", typeof( LLVMHostedMemoryMap ) )]
    public sealed class LLVMHostedCompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "LLVM Hosted Memory Map" )]
    public sealed class LLVMHostedMemoryMap : MemoryMapCategory
    {
    }

    #endregion //LLVM Platform
}

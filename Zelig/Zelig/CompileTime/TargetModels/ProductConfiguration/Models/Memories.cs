//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [DisplayName( "Intel 8MB NOR Flash" )]
    [Defaults    ( "SizeInBytes"    , 8 * 1024 * 1024                             )]
    [Defaults    ( "WordSize"       , 32                                          )]
    [Defaults    ( "WaitStates"     , 2                                           )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.FLASH              |
                                      Runtime.MemoryAttributes.RandomAccessMemory |
                                      Runtime.MemoryAttributes.ExternalMemory     )]
    [HardwareModel(typeof(Emulation.ArmProcessor.Simulator.MemoryHandler), HardwareModelAttribute.Kind.Memory)]
    public sealed class I28F320W18_32 : FlashMemoryCategory
    {
    }

    [DisplayName( "Intel 32MB NOR Flash" )]
    [Defaults    ( "SizeInBytes"    , 32 * 1024 * 1024                            )]
    [Defaults    ( "WordSize"       , 16                                          )]
    [Defaults    ( "WaitStates"     , 2                                           )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.FLASH              |
                                      Runtime.MemoryAttributes.RandomAccessMemory |
                                      Runtime.MemoryAttributes.ExternalMemory     )]
    [HardwareModel(typeof(Emulation.ArmProcessor.Simulator.MemoryHandler), HardwareModelAttribute.Kind.Memory)]
    public sealed class I28F256L18_16 : FlashMemoryCategory
    {
    }

    [DisplayName("Spansion 8MB NOR Flash")]
    [Defaults    ( "SizeInBytes"    , 8 * 1024 * 1024                             )]
    [Defaults    ( "WordSize"       , 16                                          )]
    [Defaults    ( "WaitStates"     , 2                                           )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.FLASH              |
                                      Runtime.MemoryAttributes.RandomAccessMemory |
                                      Runtime.MemoryAttributes.ExternalMemory     )]
    [HardwareModel(typeof(Emulation.ArmProcessor.FlashMemory.S29WS064), HardwareModelAttribute.Kind.Memory)]
    public sealed class S29WS064J : FlashMemoryCategory
    {
    }

    [DisplayName( "Spansion 16MB NOR Flash" )]
    [Defaults    ( "SizeInBytes"    , 16 * 1024 * 1024                            )]
    [Defaults    ( "WordSize"       , 16                                          )]
    [Defaults    ( "WaitStates"     , 2                                           )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.FLASH              |
                                      Runtime.MemoryAttributes.RandomAccessMemory |
                                      Runtime.MemoryAttributes.ExternalMemory     )]
    [HardwareModel(typeof(Emulation.ArmProcessor.FlashMemory.S29WS128N), HardwareModelAttribute.Kind.Memory)]
    public sealed class S29WS128N : FlashMemoryCategory
    {
    }

    [DisplayName( "ST 256Mb NAND FLASH" )]
    [Defaults    ( "SizeInBytes"    , 32 * 1024 * 1024                          )]
    [Defaults    ( "WordSize"       , 8                                         )]
    [Defaults    ( "WaitStates"     , 0                                         )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.FLASH            |
                                      Runtime.MemoryAttributes.BlockBasedMemory |
                                      Runtime.MemoryAttributes.ExternalMemory   )]
////[HardwareModel(typeof(Emulation.ArmProcessor.NandFlashMemory.ST_NAND256A), HardwareModelAttribute.Kind.Memory)]
    public sealed class ST_NAND256A : FlashMemoryCategory
    {
    }

    [DisplayName( "Micron 16MB Mobile SDRAM" )]
    [Defaults    ( "SizeInBytes"    , 1 * 16 * 1024 * 1024                        )]
    [Defaults    ( "WordSize"       , 1 * 16                                      )]
    [Defaults    ( "WaitStates"     , 0                                           )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM                |
                                      Runtime.MemoryAttributes.RandomAccessMemory |
                                      Runtime.MemoryAttributes.ExternalMemory     )]
////[HardwareModel(typeof(Emulation.ArmProcessor.FlashMemory.MT48H8M16LF), HardwareModelAttribute.Kind.Memory)]
    public sealed class MT48H8M16LF : RamMemoryCategory
    {
    }

    [DisplayName( "Micron 32MB Mobile SDRAM" )]
    [Defaults    ( "SizeInBytes"    , 1 * 32 * 1024 * 1024                        )]
    [Defaults    ( "WordSize"       , 1 * 16                                      )]
    [Defaults    ( "WaitStates"     , 0                                           )]
    [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM                |
                                      Runtime.MemoryAttributes.RandomAccessMemory |
                                      Runtime.MemoryAttributes.ExternalMemory     )]
////[HardwareModel(typeof(Emulation.ArmProcessor.FlashMemory.MT48H8M16LF), HardwareModelAttribute.Kind.Memory)]
    public sealed class MT48H16M16LF : RamMemoryCategory
    {
    }
}

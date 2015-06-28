////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>

//--//
#ifndef _ZeligRAM_BS_Driver
#define _ZeligRAM_BS_Driver 1

extern "C" 
{
    UINT32 Extern__Storage_Write( UINT32 address, UINT8* buffer, UINT32 offset, UINT32 len );
    UINT32 Extern__Storage_Read( UINT32 address, UINT8* buffer, UINT32 offset, UINT32 len );
    UINT32 Extern__Storage_Memset( UINT32 address, UINT8 data, UINT32 len );
    UINT32 Extern__Storage_IsErased( UINT32 address, UINT32 len );
    UINT32 Extern__Storage_EraseBlock( UINT32 address, UINT32 len );
}

//--//

struct ZeligRAM_BS_Driver
{
    static BOOL ChipInitialize( void* context );

    static BOOL ChipUnInitialize( void* context );

    static const BlockDeviceInfo* GetDeviceInfo( void* context );

    static BOOL Read( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff );

    static BOOL Write( void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite );

    static BOOL Memset( void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes );

    static BOOL GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    static BOOL IsBlockErased( void* context, ByteAddress BlockStart, UINT32 BlockLength );

    static BOOL EraseBlock( void* context, ByteAddress Address );

    static void SetPowerState( void* context, UINT32 State );

    static UINT32 MaxSectorWrite_uSec( void* context );

    static UINT32 MaxBlockErase_uSec( void* context );

//--//

    static BOOL ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey );

    static BOOL ReadProductID( void* context, FLASH_WORD& ManufacturerCode, FLASH_WORD& DeviceCode );
};

//--//

#endif // _ZeligRAM_BS_Driver


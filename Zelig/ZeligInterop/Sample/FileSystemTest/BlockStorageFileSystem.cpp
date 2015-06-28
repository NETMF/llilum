////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\FS\FAT\FAT_FS.h>
#include "BlockStorageFileSystem.h"

//--//

extern FILESYSTEM_DRIVER_INTERFACE g_FAT32_FILE_SYSTEM_DriverInterface;
extern STREAM_DRIVER_INTERFACE     g_FAT32_STREAM_DriverInterface;

//#pragma pack(1)

//static UINT32 s_RamBuffer[(5*1024*1024 + 512)/sizeof(UINT32)];

#define FS_SIZE         (2 * 8 * 1024 * 1024)
#define FS_BASE_ADDR    (0xA0000000 + FS_SIZE)
#define FS_BLOCK_SIZE   0x20000

static const BlockRange s_BlockRanges[] = 
{
    {
        BlockRange::BLOCKTYPE_FILESYSTEM,
        0,
        FS_SIZE / FS_BLOCK_SIZE - 2,
        //sizeof(s_RamBuffer) / 512 - 2,
    },
    {
        BlockRange::BLOCKTYPE_CONFIG,
        FS_SIZE / FS_BLOCK_SIZE - 1,
        FS_SIZE / FS_BLOCK_SIZE - 1,
        //sizeof(s_RamBuffer) / 512 - 1, 
        //sizeof(s_RamBuffer) / 512 - 1,
    }
    
};

static const BlockRegionInfo s_RamRegionInfo =
{
    //0xA0000000,
    FS_BASE_ADDR,
    //sizeof(s_RamBuffer)/512,
    FS_SIZE / FS_BLOCK_SIZE,
    FS_BLOCK_SIZE,
    2,
    s_BlockRanges,
};

static const BlockDeviceInfo s_RamDeviceInfo =
{
    {FALSE, TRUE, FALSE, FALSE, TRUE},
    100,
    500,
    4,
    //sizeof(s_RamBuffer),
    FS_SIZE,
    1,
    &s_RamRegionInfo,
};

struct IBlockStorageDevice g_ZeligRAM_BS_DeviceTable = 
{                          
    &ZeligRAM_BS_Driver::ChipInitialize,
    &ZeligRAM_BS_Driver::ChipUnInitialize,
    &ZeligRAM_BS_Driver::GetDeviceInfo,
    &ZeligRAM_BS_Driver::Read,
    &ZeligRAM_BS_Driver::Write,
    &ZeligRAM_BS_Driver::Memset,    
    &ZeligRAM_BS_Driver::GetSectorMetadata,
    &ZeligRAM_BS_Driver::SetSectorMetadata,
    &ZeligRAM_BS_Driver::IsBlockErased,
    &ZeligRAM_BS_Driver::EraseBlock,
    &ZeligRAM_BS_Driver::SetPowerState,
    &ZeligRAM_BS_Driver::MaxSectorWrite_uSec,
    &ZeligRAM_BS_Driver::MaxBlockErase_uSec,    
};

FILESYSTEM_INTERFACES g_AvailableFSInterfaces[] =
{
    { &g_FAT32_FILE_SYSTEM_DriverInterface  , &g_FAT32_STREAM_DriverInterface },
};

const size_t g_InstalledFSCount = 1;

static FileSystemVolume* s_volumes[10];
static int s_volumeIdx = 0;


static UINT32 s_MallocBytes[1024*1024/sizeof(UINT32)];
//static int  s_MallocIndex = 0;

struct FreeList 
{
    FreeList* pNext;
    UINT32*   pData;
    UINT32    size;
};

static FreeList s_freeList = {NULL, NULL, 0};

BlockStorageDevice g_RamBlockStorageDevice;
FileSystemVolume g_RamFileSysVolume;

//#pragma unpack

extern "C"
{
    void* private_malloc( size_t len )
    {
        FreeList* ptr;
        FreeList* ptrPrev = NULL;

        while(len & 0x3)
        {
            len++;
        }
        
        len += 4;

        if(len < sizeof(FreeList))
        {
            len = sizeof(FreeList);
        }
        
        if(s_freeList.pData == NULL)
        {
            s_freeList.pData = &s_MallocBytes[0];
            s_freeList.size  = sizeof(s_MallocBytes);
            s_freeList.pNext = NULL;
        }

        ptr = &s_freeList;

        while(ptr->size < len)
        {
            if(ptr->pNext == NULL) { while(true); }

            ptrPrev = ptr;
            ptr = ptr->pNext;
        }

        ptr->pData[0] = len;
        void* ret = (void*)&ptr->pData[1];

        if(ptrPrev == NULL)
        {
            s_freeList.pData = (UINT32*)&ptr->pData[len];
            s_freeList.size -= len;
        }
        else
        {
            ptrPrev->pNext = ptr->pNext;
        }

        return ret;
    }

    void private_free( void* ptrFree )
    {
        FreeList* ptr = &s_freeList;
        FreeList* ptrPrev = NULL;
        UINT32 size = ((UINT32*)ptrFree)[-1];


        while(ptr != NULL && ptr->pData < ptrFree)
        {
            ptrPrev = ptr;

            ptr = ptr->pNext;
        }

        if(ptrPrev->size + (UINT32)ptrPrev->pData == (UINT32)ptrFree - 4)
        {
            ptrPrev->size += size;
        }
        else
        {
            ptr = (FreeList*)&((UINT32*)ptrFree)[-1];
            
            ptr->pNext = ptrPrev->pNext;
            ptrPrev->pNext = ptr;
            ptr->size = size;
            ptr->pData = (UINT32*)ptr;
        }
    }
    
    void* private_realloc( void * ptr, size_t len )
    {
        UINT32 size = ((UINT32*)ptr)[-1];
        void* ret;

        if(len < size)
        {
            return ptr;
        }

        ret = private_malloc(len);

        memcpy(ret, ptr, size);

        private_free(ptr);

        return ret;
    }

    void debug_printf( const char* format, ... )
    {
    }
}


BOOL ZeligRAM_BS_Driver::ChipInitialize( void* context )
{
    return TRUE;
}

BOOL ZeligRAM_BS_Driver::ChipUnInitialize( void* context )
{
    return TRUE;
}

const BlockDeviceInfo* ZeligRAM_BS_Driver::GetDeviceInfo( void* context )
{
    return (BlockDeviceInfo*)context;    
}

BOOL  ZeligRAM_BS_Driver::ChipReadOnly( void* context, BOOL On, UINT32 ProtectionKey )
{
    return TRUE;
}


BOOL  ZeligRAM_BS_Driver::Read( void* context, ByteAddress StartSector, UINT32 NumBytes, BYTE * pSectorBuff)
{
    if(StartSector < FS_BASE_ADDR || (StartSector + NumBytes) > (FS_BASE_ADDR + FS_SIZE)) return FALSE;

    return 1 == Extern__Storage_Read( StartSector, pSectorBuff, 0, NumBytes );
}

BOOL ZeligRAM_BS_Driver::Write(void* context, ByteAddress Address, UINT32 NumBytes, BYTE * pSectorBuff, BOOL ReadModifyWrite)
{
    if(Address < FS_BASE_ADDR || (Address + NumBytes) > (FS_BASE_ADDR + FS_SIZE)) return FALSE;

    return 1 == Extern__Storage_Write( Address, pSectorBuff, 0, NumBytes );
}

BOOL ZeligRAM_BS_Driver::Memset(void* context, ByteAddress Address, UINT8 Data, UINT32 NumBytes)
{
    if(Address < FS_BASE_ADDR || (Address + NumBytes) > (FS_BASE_ADDR + FS_SIZE)) return FALSE;

    return 1 == Extern__Storage_Memset( Address, Data, NumBytes );
}


BOOL ZeligRAM_BS_Driver::GetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return FALSE;
}

BOOL ZeligRAM_BS_Driver::SetSectorMetadata(void* context, ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
{
    return FALSE;
}


BOOL ZeligRAM_BS_Driver::IsBlockErased( void* context, ByteAddress BlockStart, UINT32 BlockLength )
{
    if(BlockStart < FS_BASE_ADDR || (BlockStart + BlockLength) > (FS_BASE_ADDR + FS_SIZE)) return FALSE;

    return 1 == Extern__Storage_IsErased( BlockStart, FS_BLOCK_SIZE );
}


BOOL ZeligRAM_BS_Driver::EraseBlock( void* context, ByteAddress Sector )
{
    if(Sector < FS_BASE_ADDR || (Sector + FS_BLOCK_SIZE) > (FS_BASE_ADDR + FS_SIZE)) return FALSE;

    return 1 == Extern__Storage_EraseBlock( Sector, FS_BLOCK_SIZE );    
}



void ZeligRAM_BS_Driver::SetPowerState( void* context, UINT32 State )
{
    return ;
}

UINT32 ZeligRAM_BS_Driver::MaxSectorWrite_uSec( void* context )
{
    return 100;
}


UINT32 ZeligRAM_BS_Driver::MaxBlockErase_uSec( void* context )
{
    return 500;
    
}


//--// 


extern "C"
{

void AddRamBlockStorageFileStream()
{    
    FileSystemVolume* pFSVolume;
    FAT_LogicDisk* pLogicDisk;

    BlockStorageList::Initialize();

    BlockStorageList::AddDevice( &g_RamBlockStorageDevice, &g_ZeligRAM_BS_DeviceTable, (void*)&s_RamDeviceInfo, TRUE );

    //BlockStorageList::InitializeDevices();

    FS_Initialize();

    FileSystemVolumeList::Initialize();

    FileSystemVolumeList::AddVolume( &g_RamFileSysVolume, "ROOT", 0, 0, &g_FAT32_STREAM_DriverInterface, &g_FAT32_FILE_SYSTEM_DriverInterface, &g_RamBlockStorageDevice, 0, FALSE );

    pFSVolume = FileSystemVolumeList::FindVolume("ROOT", 4);
    if (pFSVolume)
    {
        pLogicDisk = FAT_LogicDisk::Initialize(&(pFSVolume->m_volumeId));
        if (pLogicDisk == NULL)
        {
            pFSVolume->Format("", FORMAT_PARAMETER_FORCE_FAT16);  
         
            pLogicDisk = FAT_LogicDisk::Initialize(&(pFSVolume->m_volumeId));
        }
        else
        {
            pLogicDisk->Uninitialize();
        }
    }

    //FileSystemVolumeList::InitializeVolumes();    
}

struct FileInfo
{
    UINT32 Attributes;
    INT64 CreationTime;
    INT64 LastAccessTime;
    INT64 LastWriteTime;
    INT64 Size;
};


HRESULT Format            ( UINT32 volumeHandle, LPCWSTR volumeLabel, UINT32 parameters )
{
    char lbl[128];
    int idx = 0;
    const WCHAR* src = (const WCHAR*)volumeLabel;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;
    
    FileSystemVolume* pVol = s_volumes[volumeHandle];

    while(*src != L'\0' && idx < 127)
    {
        lbl[idx++] = (char)*src++;
    }
    lbl[idx] = '\0';

    pVol->UninitializeVolume();

    hr = pVol->Format( lbl, parameters );

    if(SUCCEEDED(hr))
    {
        pVol->InitializeVolume();
    }
    
    return hr;
}

HRESULT FindOpen          ( UINT32 volumeHandle, LPCWSTR path, UINT32* findHandle )
{
    FileSystemVolume* pVol;
        
    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;
    
    pVol = s_volumes[volumeHandle];

    return pVol->FindOpen(path, findHandle);
}
HRESULT FindNext          ( UINT32 volumeHandle, UINT32 findHandle, FileInfo* findData, LPWSTR fileName, INT32* fileLen, BOOL* found )
{
    FS_FILEINFO fi;
    FileSystemVolume* pVol;
    HRESULT hr;
    INT32 len;

    UINT16 fileNameBuf[256];

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    fi.FileName = fileNameBuf;
    fi.FileNameSize = ARRAYSIZE(fileNameBuf);
    
    pVol = s_volumes[volumeHandle];

    hr = pVol->FindNext(findHandle, &fi, found );

    len = *fileLen;
    *fileLen = 0;

    if(SUCCEEDED(hr)&& *found)
    {
        WCHAR* src = (WCHAR*)fi.FileName;
        WCHAR* dst = fileName; 

        findData->Attributes     = fi.Attributes;
        findData->CreationTime   = fi.CreationTime;
        findData->LastAccessTime = fi.LastAccessTime;
        findData->LastWriteTime  = fi.LastWriteTime;

        if(fi.FileNameSize/sizeof(WCHAR) < len)
        {
            len = fi.FileNameSize/sizeof(WCHAR);
        }

        while(*src != L'\0' && len-->1)
        {
            *dst++ = *src++;
            fileLen[0] += 1;
        }

        *dst = L'\0';
    }

    return hr;
}
HRESULT FindClose         ( UINT32 volumeHandle, UINT32 findHandle )
{
    FileSystemVolume* pVol;
    HRESULT hr;
    
    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->FindClose(findHandle);

    return hr;
}
HRESULT GetFileInfo       ( UINT32 volumeHandle, LPCWSTR path, FileInfo* fileInfo, BOOL* found )
{
    FS_FILEINFO fi;
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    memset(&fi, 0, sizeof(fi));

    hr = pVol->GetFileInfo(path, &fi, found);

    if(SUCCEEDED(hr)&& *found)
    {
        fileInfo->Attributes     = fi.Attributes;
        fileInfo->CreationTime   = fi.CreationTime;
        fileInfo->LastAccessTime = fi.LastAccessTime;
        fileInfo->LastWriteTime  = fi.LastWriteTime;
        fileInfo->Size           = fi.Size;
    }
        
    return hr;
}
HRESULT CreateDirectory   ( UINT32 volumeHandle, LPCWSTR path )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->CreateDirectory( path );

    return hr;
}
HRESULT Move              ( UINT32 volumeHandle, LPCWSTR oldPath, LPCWSTR newPath )
{
    FileSystemVolume* pVol;
    HRESULT hr;
    
    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Move( oldPath, newPath );

    return hr;
}
HRESULT Delete            ( UINT32 volumeHandle, LPCWSTR path )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Delete( path );

    return hr;
}
HRESULT GetAttributes     ( UINT32 volumeHandle, LPCWSTR path, UINT32* attributes )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->GetAttributes( path, attributes );

    return hr;
}
HRESULT SetAttributes     ( UINT32 volumeHandle, LPCWSTR path, UINT32 attributes )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->SetAttributes( path, attributes );

    return hr;
}
HRESULT GetDriverDetails  ( UINT32 volumeHandle, UINT32 handle, STREAM_DRIVER_DETAILS* details )
{
    FileSystemVolume* pVol;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    *details = *pVol->DriverDetails();

    return S_OK;
}
HRESULT Open              ( UINT32 volumeHandle, LPCWSTR path, UINT32* handle )
{
    FileSystemVolume* pVol;
    HRESULT hr;
    
    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Open( path, handle );

    return hr;
}
HRESULT Close             ( UINT32 volumeHandle, UINT32 handle )
{
    FileSystemVolume* pVol = s_volumes[volumeHandle];

    HRESULT hr = pVol->Close( handle );

    return hr;
}
HRESULT Read              ( UINT32 volumeHandle, UINT32 handle, BYTE* buffer, int offset, int count, int* bytesRead )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Read( handle, &buffer[offset], count, bytesRead );

    if(*bytesRead < 0)
    {
        *bytesRead = 0;
    }

    return hr;
}
HRESULT Write             ( UINT32 volumeHandle, UINT32 handle, BYTE* buffer, int offset, int count, int* bytesWritten )
{
    FileSystemVolume* pVol;
    HRESULT hr;
    
    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Write( handle, &buffer[offset], count, bytesWritten );

    if(*bytesWritten < 0)
    {
        *bytesWritten = 0;
        hr = CLR_E_FILE_IO;
    }
    

    return hr;
}
HRESULT Flush             ( UINT32 volumeHandle, UINT32 handle )
{
    FileSystemVolume* pVol;
    HRESULT hr; 

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Flush( handle );

    return hr;
}
HRESULT Seek              ( UINT32 volumeHandle, UINT32 handle, INT64 offset, UINT32 origin, INT64* position )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->Seek( handle, offset, origin, position );

    return hr;
}
HRESULT GetLength         ( UINT32 volumeHandle, UINT32 handle, INT64* length )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->GetLength( handle, length );

    return hr;
}
HRESULT SetLength( UINT32 volumeHandle, UINT32 handle, INT64 length )
{
    FileSystemVolume* pVol;
    HRESULT hr;

    if(volumeHandle >= s_volumeIdx) return CLR_E_FILE_IO;

    pVol = s_volumes[volumeHandle];

    hr = pVol->SetLength( handle, length );

    return hr;
}

HRESULT GetVolumes( UINT32* volume, INT32* volumeCount )
{
    INT32 cnt;
    
    if(volumeCount == NULL) return CLR_E_FILE_IO;
    
    if(volume == NULL)
    {
        *volumeCount = s_volumeIdx;
        return S_OK;
    }

    cnt = *volumeCount;

    if(cnt > s_volumeIdx)
    {
        cnt = s_volumeIdx;
        *volumeCount = cnt;
    }

    for(int i=0; i<cnt; i++)
    {
        volume[i] = i;
    }
    
    return S_OK;
}

UINT32 FindVolume( LPCWSTR nameSpace, int len )
{
    UINT32 idx = (UINT32)-1;
    char lbl[128];
    int i = 0;
    const WCHAR* src = (const WCHAR*)nameSpace;
    
    while(*src != L'\0' && i < 127)
    {
        lbl[i++] = (char)*src++;
    }
    lbl[i] = '\0';

    if(s_volumeIdx > 0)
    {
        for(int x=0; x<s_volumeIdx; x++)
        {
            FileSystemVolume* pVol = s_volumes[x];

            if(0 == hal_stricmp( pVol->m_nameSpace, lbl ))
            {
                return x;
            }
        }
    }
    
    {
        FileSystemVolume* pVol = FileSystemVolumeList::FindVolume( lbl, i );

        if(pVol != NULL)
        {
            idx = s_volumeIdx++;
            s_volumes[idx] = pVol;
        }
    }

    return idx;
}


HRESULT GetName( UINT32 volumeHandle, wchar_t*  name, INT32* nameLen )
{
    char* pName;
    int idx = 0;
    int len = *nameLen;

    if(volumeHandle >= s_volumeIdx)
    {
        return CLR_E_FILE_IO;
    }

    pName = s_volumes[volumeHandle]->m_nameSpace;

    while(pName[idx] != '\0' && len-- > 0)
    {
        name[idx] = pName[idx];
        idx++;
    }

    *nameLen = idx;

    return S_OK;
}
HRESULT GetLabel( UINT32 volumeHandle, wchar_t* label, INT32* labelLen )
{
    int idx = 0;
    int len = *labelLen;
    FileSystemVolume *pVol;
    char szLabel[16];
    HRESULT hr = S_OK;

    if(volumeHandle >= s_volumeIdx)
    {
        return CLR_E_FILE_IO;
    }

    pVol = s_volumes[volumeHandle];

    hr = g_FAT32_FILE_SYSTEM_DriverInterface.GetVolumeLabel( &pVol->m_volumeId, (LPSTR)szLabel, ARRAYSIZE(szLabel) );

    if(SUCCEEDED(hr))
    {
        if(len > ARRAYSIZE(szLabel)-1) 
        {
            len = ARRAYSIZE(szLabel) - 1;
        }
        
        while(szLabel[idx] != '\0' && len-- > 0)
        {
            label[idx] = szLabel[idx];
            idx++;
        }

        label[idx] = L'\0';

        *labelLen = idx;
    }

    return S_OK;
}
HRESULT GetFileSystem( UINT32 volumeHandle, wchar_t* fileSystem, INT32* fileSystemLen )
{
    const char* pName;
    int idx = 0;
    int len = *fileSystemLen;

    if(volumeHandle >= s_volumeIdx)
    {
        return CLR_E_FILE_IO;
    }

    pName = s_volumes[volumeHandle]->m_fsDriver->Name;

    while(pName[idx] != '\0' && len-- > 0)
    {
        fileSystem[idx] = pName[idx];
        idx++;
    }

    *fileSystemLen = idx;

    return S_OK;
}
HRESULT GetSize( UINT32 volumeHandle, INT64* totalFreeSpace, INT64* totalSize )
{
    if(volumeHandle >= s_volumeIdx)
    {
        return CLR_E_FILE_IO;
    }

    return s_volumes[volumeHandle]->GetSizeInfo(totalSize, totalFreeSpace);
}

}



// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass( typeof( System.IO.VolumeInfo ) )]
    public class VolumeInfoImpl
    {
        uint m_volumeHandle;
        static bool s_isFsInit = false;
        static object s_lock = new object();

        private static void EnsureInit()
        {
            if(!s_isFsInit)
            {
                lock(s_lock)
                {
                    if(!s_isFsInit)
                    {
                        FileSystemVolumeList.AddRamBlockStorageFileStream();
                        s_isFsInit = true;
                    }
                }
            }
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_VolumeInfoLibrary )]
        [DiscardTargetImplementation]
        public VolumeInfoImpl( String volumeName )
        {
            EnsureInit();

            m_volumeHandle = FileSystemVolumeList.FindVolume( volumeName, volumeName.Length );
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_VolumeInfoLibrary )]
        [DiscardTargetImplementation]
        internal VolumeInfoImpl( uint volumePtr )
        {
            EnsureInit();

            m_volumeHandle = volumePtr;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_VolumeInfoLibrary )]
        public void Refresh()
        {
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_VolumeInfoLibrary )]
        public static VolumeInfo[] GetVolumes()
        {
            int cnt = 0;

            EnsureInit();

            HR.ThrowOnFailure( FileSystemVolumeList.GetVolumes( null, ref cnt ) );

            uint[] volIds = new uint[cnt];

            if(cnt > 0)
            {
                HR.ThrowOnFailure( FileSystemVolumeList.GetVolumes( volIds, ref cnt ) );
            }

            VolumeInfo[] vols = new VolumeInfo[cnt];

            for(int i = 0; i < cnt; i++)
            {
                vols[i] = new VolumeInfo( volIds[i] );
            }

            return vols;
        }

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_VolumeInfoLibrary )]
        //public static String[] GetFileSystems()
        //{
        //    return new String[0];
        //}

        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_VolumeInfoLibrary )]
        //public void FlushAll()
        //{
        //}

        public string Name
        {
            get
            {
                char[] name = new char[32];
                int len = name.Length;

                HR.ThrowOnFailure( FileSystemVolume.GetName( m_volumeHandle, name, ref len ) );

                return new string( name, 0, len );
            }
        }

        public string FileSystem
        {
            get
            {
                char[] name = new char[32];
                int len = name.Length;

                HR.ThrowOnFailure( FileSystemVolume.GetFileSystem( m_volumeHandle, name, ref len ) );

                return new string( name, 0, len );
            }
        }

        public string VolumeLabel
        {
            get
            {
                char[] name = new char[32];
                int len = name.Length;

                HR.ThrowOnFailure( FileSystemVolume.GetLabel( m_volumeHandle, name, ref len ) );

                return new string( name, 0, len );
            }
        }

        public long TotalSize
        {
            get
            {
                long retVal = 0, avail = 0;

                HR.ThrowOnFailure( FileSystemVolume.GetSize( m_volumeHandle, ref avail, ref retVal ) );

                return retVal;
            }
        }


        //public uint FileSystemFlags
        //{
        //    get { return 0; }
        //}
        //public uint DeviceFlags
        //{
        //    get { return 0; }
        //}
        //public uint SerialNumber
        //{
        //    get { return 0; }
        //}
        public long TotalFreeSpace
        {
            get 
            {
                long retVal = 0, avail = 0;

                HR.ThrowOnFailure( FileSystemVolume.GetSize( m_volumeHandle, ref avail, ref retVal ) );

                return avail;
            }
        }

    }

    [ExtendClass( typeof( System.IO.RemovableMedia ) )]
    public static class RemovableMediaImpl
    {
        //[RT.ImportedMethodReference( RT.ImportedMethodReferenceAttribute.InteropFileType.ArmELF, c_RemovableMediaLibrary )]
        private static void MountRemovableVolumes()
        {
        }
    }
}

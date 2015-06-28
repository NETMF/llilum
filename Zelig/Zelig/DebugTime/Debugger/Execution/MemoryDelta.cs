//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_MEMORYDELTA_PREFETCHES


namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg                = Microsoft.Zelig.Configuration.Environment;


    public class MemoryDelta
    {
        class Cluster
        {
            //
            // State
            //

            internal const int c_Size = 512;

            internal bool[] m_valid;
            internal byte[] m_data;

            //
            // Constructor Methods
            //

            internal Cluster()
            {
                m_valid = new bool[c_Size];
                m_data  = new byte[c_Size];
            }

            internal Cluster( byte[] data )
            {
                m_valid = new bool[c_Size];
                m_data  = data;

                for(int i = 0 ; i < c_Size; i++)
                {
                    m_valid[i] = true;
                }
            }

            //
            // Helper Methods
            //

            internal static uint GetBase( uint address )
            {
                return address & ~(uint)(c_Size - 1);
            }

            internal static uint GetOffset( uint address )
            {
                return address % (uint)c_Size; 
            }

            internal static Cluster GetCluster( GrowOnlyHashTable< uint, Cluster > ht      ,
                                                uint                               address ,
                                                bool                               fCreate )
            {
                Cluster res;
                uint    index = address / c_Size;

                if(ht.TryGetValue( index, out res ) == false)
                {
                    if(fCreate)
                    {
                        res = new Cluster();

                        ht[index] = res;
                    }
                }

                return res;
            }

            internal static void SetCluster( GrowOnlyHashTable< uint, Cluster > ht      ,
                                             uint                               address ,
                                             byte[]                             data    )
            {
                DumpCluster( address, data );

                uint index = address / c_Size;

                ht[index] = new Cluster( data );
            }

            internal bool IsValid( uint address )
            {
                uint offset = GetOffset( address );

                return m_valid[offset];
            }

            [System.Diagnostics.Conditional( "DEBUG_MEMORYDELTA_PREFETCHES" )]
            private static void DumpCluster( uint   address ,
                                             byte[] data    )
            {
                Console.WriteLine( "GetCluster" );

                for(int i = 0; i < data.Length; i += 4)
                {
                    if(i % 32 == 0)
                    {
                        Console.Write( "0x{0:X8}: ", address + i );
                    }

                    Console.Write( "0x{0:X2}{1:X2}{2:X2}{3:X2}, ", data[i+3], data[i+2], data[i+1], data[i] );

                    if(i % 32 == 32 - 4)
                    {
                        Console.WriteLine( "" );
                    }
                }

                Console.WriteLine( "" );
            }

            //
            // Access Methods
            //

            internal byte this[uint address]
            {
                get
                {
                    uint offset = GetOffset( address );

                    return m_data[offset];
                }

                set
                {
                    uint offset = GetOffset( address );

                    m_data [offset] = value;
                    m_valid[offset] = true;
                }
            }
        }

        //
        // State
        //

        ImageInformation                   m_imageInformation;
        Emulation.Hosting.AbstractHost     m_host;
        Emulation.Hosting.MemoryProvider   m_mem;
        bool                               m_fRunning;
                                                                             
        GrowOnlyHashTable< uint, Cluster > m_snapshot;
        GrowOnlyHashTable< uint, Cluster > m_snapshotPrevious;

        //
        // Constructor Methods
        //

        internal MemoryDelta( ImageInformation               imageInformation ,
                              Emulation.Hosting.AbstractHost host             )
        {
            Synchronize( imageInformation, host );
        }

        //
        // Helper Methods
        //

        public void RegisterForNotification( bool fEntering ,
                                             bool fExiting  )
        {
            ProcessorHost svc;

            if(m_host.GetHostingService( out svc ))
            {
                svc.RegisterForNotification( this, fEntering, fExiting );
            }
        }

        public void UnregisterForNotification( bool fEntering ,
                                               bool fExiting  )
        {
            ProcessorHost svc;

            if(m_host.GetHostingService( out svc ))
            {
                svc.UnregisterForNotification( this, fEntering, fExiting );
            }
        }

        internal void EnteringExecuting()
        {
            m_fRunning = true;
        }

        internal void ExitingRunning()
        {
            CopyPreviousValues();

            m_snapshot = HashTableFactory.New< uint, Cluster >();

            m_fRunning = false;
        }

        //--//

        public void FlushCache()
        {
            m_snapshot.Clear();
        }

        //--//

        public bool GetUInt8(     uint address ,
                              out byte result  )
        {
            bool fChanged;

            return GetUInt8( address, true, out result, out fChanged );
        }

        public bool GetUInt8(     uint address        ,
                                  bool fUpdateHistory ,
                              out byte result         ,
                              out bool fChanged       )
        {
            fChanged = false;

            TryToLookAhead( m_snapshot, address, address + 128 );

            if(ReadSnapshot( address, out result, m_snapshot ) == false)
            {
                if(m_fRunning)
                {
                    return false;
                }

                if(m_mem.GetUInt8( address, out result ) == false)
                {
                    return false;
                }

                if(fUpdateHistory)
                {
                    WriteSnapshot( address, result, m_snapshot );
                }
            }

            byte previousResult;

            if(ReadSnapshot( address, out previousResult, m_snapshotPrevious ) && previousResult != result)
            {
                fChanged = true;
            }

            return true;
        }

        //--//

        public bool GetUInt16(     uint   address ,
                               out ushort result  )
        {
            bool fChanged;

            return GetUInt16( address, true, out result, out fChanged );
        }

        public bool GetUInt16(     uint   address        ,
                                   bool   fUpdateHistory ,
                               out ushort result         ,
                               out bool   fChanged       )
        {
            fChanged = false;

            TryToLookAhead( m_snapshot, address, address + 128 );

            if(ReadSnapshot( address, out result, m_snapshot ) == false)
            {
                if(m_fRunning)
                {
                    return false;
                }

                if(m_mem.GetUInt16( address, out result ) == false)
                {
                    return false;
                }

                if(fUpdateHistory)
                {
                    WriteSnapshot( address, result, m_snapshot );
                }
            }

            ushort previousResult;

            if(ReadSnapshot( address, out previousResult, m_snapshotPrevious ) && previousResult != result)
            {
                fChanged = true;
            }

            return true;
        }

        //--//

        public bool GetUInt32(     uint address ,
                               out uint result  )
        {
            bool fChanged;

            return GetUInt32( address, true, out result, out fChanged );
        }

        public bool GetUInt32(     uint address        ,
                                   bool fUpdateHistory ,
                               out uint result         ,
                               out bool fChanged       )
        {
            fChanged = false;

            TryToLookAhead( m_snapshot, address, address + 128 );

            if(ReadSnapshot( address, out result, m_snapshot ) == false)
            {
                if(m_fRunning)
                {
                    return false;
                }

                if(m_mem.GetUInt32( address, out result ) == false)
                {
                    return false;
                }

                if(fUpdateHistory)
                {
                    WriteSnapshot( address, result, m_snapshot );
                }
            }

            uint previousResult;

            if(ReadSnapshot( address, out previousResult, m_snapshotPrevious ) && previousResult != result)
            {
                fChanged = true;
            }

            return true;
        }

        //--//

        public bool GetUInt64(     uint  address ,
                               out ulong result  )
        {
            bool fChanged;

            return GetUInt64( address, true, out result, out fChanged );
        }

        public bool GetUInt64(     uint  address        ,
                                   bool  fUpdateHistory ,
                               out ulong result         ,
                               out bool  fChanged       )
        {
            fChanged = false;

            TryToLookAhead( m_snapshot, address, address + 128 );

            if(ReadSnapshot( address, out result, m_snapshot ) == false)
            {
                if(m_fRunning)
                {
                    return false;
                }

                if(m_mem.GetUInt64( address, out result ) == false)
                {
                    return false;
                }

                if(fUpdateHistory)
                {
                    WriteSnapshot( address, result, m_snapshot );
                }
            }

            ulong previousResult;

            if(ReadSnapshot( address, out previousResult, m_snapshotPrevious ) && previousResult != result)
            {
                fChanged = true;
            }

            return true;
        }

        //--//

        public bool GetBlock(     uint   address ,
                                  int    size    ,
                              out byte[] result  )
        {
            bool fChanged;

            return GetBlock( address, size, true, out result, out fChanged );
        }

        public bool GetBlock(     uint   address        ,
                                  int    size           ,
                                  bool   fUpdateHistory ,
                              out byte[] result         ,
                              out bool   fChanged       )
        {
            fChanged = false;

            result = new byte[size];

            for(int pos = 0; pos < size; pos++)
            {
                bool fChangedVal;

                if(GetUInt8( address, fUpdateHistory, out result[pos], out fChangedVal ) == false)
                {
                    return false;
                }

                address += sizeof(byte);
            }

            return true;
        }

        //--//

        public bool SetUInt8( uint address ,
                              byte result  )
        {
            if(m_fRunning)
            {
                return false;
            }

            if(m_mem.SetUInt8( address, result ) == false)
            {
                return false;
            }

            //
            // Mark the address as changed.
            //
            WriteSnapshot( address,        result, m_snapshot         );
            WriteSnapshot( address, (byte)~result, m_snapshotPrevious );

            return true;
        }

        public bool SetUInt16( uint   address ,
                               ushort result  )
        {
            if(m_fRunning)
            {
                return false;
            }

            if(m_mem.SetUInt16( address, result ) == false)
            {
                return false;
            }

            //
            // Mark the address as changed.
            //
            WriteSnapshot( address,          result, m_snapshot         );
            WriteSnapshot( address, (ushort)~result, m_snapshotPrevious );

            return true;
        }

        public bool SetUInt32( uint address ,
                               uint result  )
        {
            if(m_fRunning)
            {
                return false;
            }

            if(m_mem.SetUInt32( address, result ) == false)
            {
                return false;
            }

            //
            // Mark the address as changed.
            //
            WriteSnapshot( address,        result, m_snapshot         );
            WriteSnapshot( address, (uint)~result, m_snapshotPrevious );

            return true;
        }

        public bool SetUInt64( uint  address ,
                               ulong result  )
        {
            if(m_fRunning)
            {
                return false;
            }

            if(m_mem.SetUInt64( address, result ) == false)
            {
                return false;
            }

            //
            // Mark the address as changed.
            //
            WriteSnapshot( address,         result, m_snapshot         );
            WriteSnapshot( address, (ulong)~result, m_snapshotPrevious );

            return true;
        }

        //--//

        public bool SetBlock( uint   address ,
                              byte[] result  )
        {
            for(int pos = 0; pos < result.Length; pos++)
            {
                if(SetUInt8( address, result[pos] ) == false)
                {
                    return false;
                }

                address += sizeof(byte);
            }

            return true;
        }

        //--//

        private static void WriteSnapshot( uint                               address ,
                                           byte                               value   ,
                                           GrowOnlyHashTable< uint, Cluster > ht      )
        {
            var cluster = Cluster.GetCluster( ht, address, true );

            cluster[address] = value;
        }

        private static void WriteSnapshot( uint                               address ,
                                           ushort                             value   ,
                                           GrowOnlyHashTable< uint, Cluster > ht      )
        {
            WriteSnapshot( address               , (byte)(value     ), ht );
            WriteSnapshot( address + sizeof(byte), (byte)(value >> 8), ht );
        }

        private static void WriteSnapshot( uint                               address ,
                                           uint                               value   ,
                                           GrowOnlyHashTable< uint, Cluster > ht      )
        {
            WriteSnapshot( address                 , (ushort)(value      ), ht );
            WriteSnapshot( address + sizeof(ushort), (ushort)(value >> 16), ht );
        }

        private static void WriteSnapshot( uint                               address ,
                                           ulong                              value   ,
                                           GrowOnlyHashTable< uint, Cluster > ht      )
        {
            WriteSnapshot( address               , (uint)(value      ), ht );
            WriteSnapshot( address + sizeof(uint), (uint)(value >> 32), ht );
        }

        //--//

        private void TryToLookAhead( GrowOnlyHashTable< uint, Cluster > ht           ,
                                     uint                               startAddress ,
                                     uint                               endAddress   )
        {
            if(m_fRunning == false)
            {
                startAddress = Cluster.GetBase( startAddress );

                while(startAddress < endAddress)
                {
                    var cluster = Cluster.GetCluster( ht, startAddress, false );
                    if(cluster == null)
                    {
                        bool fOk = false;

                        foreach(Cfg.MemoryCategory mem in m_imageInformation.Configuration.SearchValues< Cfg.MemoryCategory >())
                        {
                            if((mem.Characteristics & RT.MemoryAttributes.RandomAccessMemory) != 0)
                            {
                                if(startAddress >= mem.BaseAddress && startAddress < mem.BaseAddress + mem.SizeInBytes)
                                {
                                    fOk = true;
                                    break;
                                }
                            }
                        }

                        if(fOk)
                        {
                            byte[] res;

                            if(m_mem.GetBlock( startAddress, Cluster.c_Size, sizeof(uint), out res ))
                            {
                                Cluster.SetCluster( ht, startAddress, res );
                            }
                        }
                    }

                    startAddress += Cluster.c_Size;
                }
            }
        }

        private bool ReadSnapshot(     uint                               address ,
                                   out byte                               value   ,
                                       GrowOnlyHashTable< uint, Cluster > ht      )
        {
            var cluster = Cluster.GetCluster( ht, address, false );
            if(cluster != null)
            {
                if(cluster.IsValid( address ))
                {
                    value = cluster[address];
                    return true;
                }
            }
            
            value = 0;
            return false;
        }

        private bool ReadSnapshot(     uint                               address ,
                                   out ushort                             value   ,
                                       GrowOnlyHashTable< uint, Cluster > ht      )
        {
            byte partLo;
            byte partHi;

            if(ReadSnapshot( address               , out partLo, ht ) &&
               ReadSnapshot( address + sizeof(byte), out partHi, ht )  )
            {
                value = (ushort)(((uint)partHi << 8) | (uint)partLo);
                return true;
            }

            value = 0;
            return false;
        }

        private bool ReadSnapshot(     uint                               address ,
                                   out uint                               value   ,
                                       GrowOnlyHashTable< uint, Cluster > ht      )
        {
            ushort partLo;
            ushort partHi;

            if(ReadSnapshot( address                 , out partLo, ht ) &&
               ReadSnapshot( address + sizeof(ushort), out partHi, ht )  )
            {
                value = (((uint)partHi << 16) | (uint)partLo);
                return true;
            }

            value = 0;
            return false;
        }

        private bool ReadSnapshot(     uint                               address ,
                                   out ulong                              value   ,
                                       GrowOnlyHashTable< uint, Cluster > ht      )
        {
            uint partLo;
            uint partHi;

            if(ReadSnapshot( address               , out partLo, ht ) &&
               ReadSnapshot( address + sizeof(uint), out partHi, ht )  )
            {
                value = (((ulong)partHi << 32) | (ulong)partLo);
                return true;
            }

            value = 0;
            return false;
        }

        //--//

        internal void Synchronize( ImageInformation               imageInformation ,
                                   Emulation.Hosting.AbstractHost host             )
        {
            if(m_imageInformation != imageInformation ||
               m_host             != host              )
            {
                m_imageInformation = imageInformation;
                m_host             = host;

                host.GetHostingService( out m_mem );

                m_snapshot = HashTableFactory.New< uint, Cluster >();

                CopyPreviousValues();
            }
        }

        private void CopyPreviousValues()
        {
            m_snapshotPrevious = m_snapshot;
        }

        //--//

        public TS.TypeRepresentation LocateType( ref uint address )
        {
            IR.ImageBuilders.SequentialRegion reg;
            uint                              offset;

            reg = m_imageInformation.FindRegion( address - sizeof(uint), out offset );
            if(reg != null)
            {
                IR.DataManager.DataDescriptor dd = reg.Context as IR.DataManager.DataDescriptor;

                if(dd != null)
                {
                    address = reg.ExternalAddress;

                    return dd.Context;
                }
                else
                {
                    foreach(IR.ImageBuilders.ImageAnnotation an in reg.AnnotationList)
                    {
                        if(an.Offset == offset && an.Target is TS.FieldRepresentation)
                        {
                            TS.FieldRepresentation fd = (TS.FieldRepresentation)an.Target;

                            return fd.FieldType;
                        }
                    }
                }
            }

            return null;
        }

        public TS.TypeRepresentation LookForVirtualTable( uint address )
        {
            uint addressHeader = m_imageInformation.GetVirtualTablePointerAddress( address );

            if(addressHeader != 0)
            {
                uint vTableAddress;
                bool fChanged;

                if(GetUInt32( addressHeader, false, out vTableAddress, out fChanged ))
                {
                    return m_imageInformation.GetTypeFromVirtualTable( vTableAddress );
                }
            }

            return null;
        }

        public string ExtractString( uint address )
        {
            return m_imageInformation.GetStringFromMemory( this, address );
        }

        //
        // Access Methods
        //

        public ImageInformation ImageInformation
        {
            get
            {
                return m_imageInformation;
            }
        }

        public Emulation.Hosting.AbstractHost Host
        {
            get
            {
                return m_host;
            }
        }
    }
}

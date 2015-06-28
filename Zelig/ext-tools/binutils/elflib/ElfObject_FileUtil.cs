using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Microsoft.binutils.elflib
{
    public partial class ElfObject
    {
        public enum LibraryProvider
        {
            RVCT,
            GCC,
            GCCOP,
        }

        public enum SystemArchitecture
        {
            ArmV4,
            ThumbV4,
            ArmV5,
            Thumb2,
            Thumb1
        }

        public enum FloatingPointUnit
        {
            None,
            SoftVFP,
            VFP,
        }

        public enum Endian
        {
            Big,
            Little,
        }

        public abstract class DeviceType
        {
            public abstract LibraryProvider    Provider       {get;}
            public abstract SystemArchitecture Architecture   {get;}
            public abstract FloatingPointUnit  FPU            {get;}
            public abstract Endian             Endianness     {get;}

            public virtual bool CheckFile( string filePath )
            {
                return true;
            }
        }

        public class RvctDeviceType : DeviceType
        {
            SystemArchitecture m_architecture;
            FloatingPointUnit m_FPU;
            Endian m_endianness;

            public override LibraryProvider    Provider     { get { return LibraryProvider.RVCT; } }
            public override SystemArchitecture Architecture { get { return m_architecture;       } }
            public override FloatingPointUnit FPU { get { return m_FPU; } }
            public override Endian Endianness { get { return m_endianness; } }

            enum RootFile
            {
                c,    // ISO C and C++ basic runtime support.
                f,    // IEEE compliant library with a fixed rounding mode (Round to nearest) and no inexact exceptions.
                fj,   // IEEE compliant library with a fixed rounding mode (Round to nearest) and no exceptions (--fpmode ieee_no_fenv).
                fz,   // Behaves like the fj library, but additionally flushes denormals and infinities to zero (--fpmode std and --fpmode fast).
                      // This library behaves like the ARM VFP in Fast mode. This is the default.
                g,    // IEEE compliant library with configurable rounding mode and all IEEE exceptions.
                m,    // Transcendental math functions.
                h,    // Compiler support (helper) library. See Helper libraries.
                cpp,  // Rogue Wave C++ library.
                cpprt,// The ARM C++ runtime libraries.
                mc,   // Non compliant ISO C micro-library basic runtime support.
                mf,   // Non compliant IEEE 754 micro-library support.
            }

            public RvctDeviceType( SystemArchitecture arch, FloatingPointUnit fpu, Endian endian )
            {
                m_architecture = arch;
                m_FPU          = fpu;
                m_endianness   = endian;
            }

            public RvctDeviceType()
            {
                m_architecture = SystemArchitecture.ArmV5;
                m_FPU          = FloatingPointUnit.None;
                m_endianness   = Endian.Little;
            }

            public override bool CheckFile(string filePath)
            {
                //root_<arch><fpu><stack><entrant>.<endian>
                string test = Path.GetFileName(filePath.ToLower());
                bool isPossibleArmFile = false;

                foreach(string en in Enum.GetNames( typeof(RootFile) ))
                {
                    if(test.StartsWith( en + "_" ))
                    {
                        isPossibleArmFile = true;
                        break;
                    }
                }

                if(!isPossibleArmFile) return true;

                string arch = "";
                string fpu = "";
                string end = "";

                switch(m_architecture)
                {
                    case SystemArchitecture.ArmV4:
                        arch = "[4|a]?";
                        break;
                    case SystemArchitecture.ThumbV4:
                        arch = "t?";
                        break;
                    case SystemArchitecture.ArmV5:
                        arch = "5?";
                        break;
                    case SystemArchitecture.Thumb2:
                        arch = "w?";
                        break;
                    case SystemArchitecture.Thumb1:
                        arch = "p?";
                        break;
                }

                switch(m_FPU)
                {
                    case FloatingPointUnit.VFP:
                        fpu = "v?";
                        break;
                    case FloatingPointUnit.SoftVFP:
                        fpu = "s?";
                        break;
                }
                switch(m_endianness)
                {
                    case Endian.Big:
                        end = "b";
                        break;
                    case Endian.Little:
                        end = "l";
                        break;
                }

                //root_<arch><fpu><stack><entrant>.<endian>
                string c_Fmt = ".*_" + arch + fpu + "u?" + "e?" + "\\." + end;

                Regex reg = new Regex(c_Fmt);

                return reg.IsMatch(test);
            }
        }

        public class FileUtil
        {
            public static DeviceType DeviceConfiguration = new RvctDeviceType();

            public static ElfObject[] Parse(string path)
            {
                List<ElfObject> elfs = new List<ElfObject>();
                FileStream fs;
                long elfOffset = 0;

                if(!DeviceConfiguration.CheckFile( path )) 
                    return elfs.ToArray();

                try
                {
                    fs = File.OpenRead(path);

                    while( SeekToElfHeader( fs ) )
                    {
                        var elf = new ElfObject();

                        elf.m_file = path;

                        elf.m_header = ReadElfHeader( fs, ref elfOffset );

                        if( elf.m_header.e_ident.EI_DATA != EI_DATA.ELFDATA2LSB ) break;
                        if( elf.m_header.e_machine != e_machine.EM_ARM ) break;
                        if( elf.m_header.e_type != e_type.ET_REL ) break;

                        // Process Section Headers
                        var shdrs = ReadSectionHeaders( elf.m_header, fs, elfOffset );

                        for( UInt16 i = 0; i < shdrs.Length; i++ )
                        {
                            switch( shdrs[i].sh_type )
                            {
                                case sh_type.SHT_NOBITS:
                                    elf.m_sections.Add( new ElfSection( elf, shdrs[i], i, new byte[shdrs[i].sh_size], shdrs[i].sh_offset ) );
                                    break;

                                case sh_type.SHT_GROUP:
                                    //TODO: Better handle GROUPS (since they can share the same symbol name)
                                    ElfSection sec = new ElfSection( elf, shdrs[i], i, ReadSectionData( shdrs[i], fs, elfOffset ), shdrs[i].sh_offset );
                                    elf.m_groups.Add( sec );
                                    elf.m_sections.Add( sec );
                                    break;

                                case sh_type.SHT_SYMTAB:
                                    elf.m_sections.Add( new SymbolTable( elf, shdrs[i], i, ReadSectionEntries<Elf32_Sym>( shdrs[i], fs, elfOffset ) ) );
                                    break;

                                case sh_type.SHT_STRTAB:
                                    elf.m_sections.Add( new StringTable( elf, shdrs[i], i, ReadSectionData( shdrs[i], fs, elfOffset ) ) );
                                    break;

                                case sh_type.SHT_REL:
                                    if( shdrs[i].sh_entsize == Marshal.SizeOf( typeof( Elf32_Rel ) ) )
                                    {
                                        elf.m_sections.Add( new RelocationSection( elf, shdrs[i], i, ReadSectionEntries<Elf32_Rel>( shdrs[i], fs, elfOffset ) ) );
                                    }
                                    else
                                    {
                                        elf.m_sections.Add( new RelocationSection( elf, shdrs[i], i, ReadSectionEntries<Elf32_Rela>( shdrs[i], fs, elfOffset ) ) );
                                    }
                                    break;

                                default:
                                    elf.m_sections.Add( new ElfSection( elf, shdrs[i], i, ReadSectionData( shdrs[i], fs, elfOffset ), shdrs[i].sh_offset ) );
                                    break;
                            }
                        }
                        elf.m_nextSectionIndex = (UInt16)shdrs.Length;

                        // Process Program Headers
                        if( elf.m_header.e_phnum > 0 )
                        {
                            foreach( var phdr in ReadProgramHeaders( elf.m_header, fs, elfOffset ) )
                            {
                                elf.m_segments.Add( new ElfSegment( elf, phdr ) );
                            }
                        }

                        elf.BuildReferences();

                        if( elf.m_symbolTable.BuildAttributes.Count == 0 || 
                            //(elf.m_symbolTable.BuildAttributes.Contains( "ARM_ISAv4" ) &&
                            !elf.m_symbolTable.BuildAttributes.Contains( "RWPI" ) &&
                            !elf.m_symbolTable.BuildAttributes.Contains( "FPIC" )// &&
                            //!elf.m_symbolTable.BuildAttributes.Contains( "VFPv2" ) &&
                            //!elf.m_symbolTable.BuildAttributes.Contains( "VFPv3" )
                            ) //||
                            //elf.m_symbolTable.BuildAttributes.Contains( "ARM_ISAv5" ) )
                        {
                            elfs.Add( elf );
                        }
                    }
                }
                catch
                {
                }

                return elfs.ToArray();
            }

            public static void GenerateFile(string path, ElfObject elf)
            {
                var ehdrsize  = Marshal.SizeOf(typeof (Elf32_Ehdr));
                var phdrs     = new Elf32_Phdr[elf.m_segments.Count];
                var shdrs     = new Elf32_Shdr[elf.m_sections.Count];
                var raw       = new byte[CalcRawByteSize(elf.m_sections)];
                int rawIndex  = 0;
                int rawOffset = 0;
                int i         = 0;
                
                elf.m_header.e_phnum = (ushort) phdrs.Length;
                elf.m_header.e_phoff = (phdrs.Length > 0) ? (uint)ehdrsize : 0;

                rawOffset = ehdrsize + phdrs.Length * elf.m_header.e_phentsize;

                elf.m_header.e_shnum = (ushort) shdrs.Length;
                elf.m_header.e_shoff = (uint)(rawOffset + raw.Length);

                var usedList = new List<ElfSection>();
                foreach (var segment in elf.GetSortedSegmentList())
                {
                    uint segSize   = 0;
                    uint segOffset = int.MaxValue;

                    foreach (var section in segment.ReferencedSections)
                    {
                        usedList.Add(section);

                        if (section.Raw.Length > 0)
                        {
                            Buffer.BlockCopy(section.Raw, 0, raw, 
                                rawIndex, section.Raw.Length);

                            section.m_header.sh_offset = (uint)(rawOffset + rawIndex);
                            section.m_header.sh_size   = (uint) section.Raw.Length;

                            segSize += (uint)section.Raw.Length;

                            rawIndex += section.Raw.Length;

                            segOffset = Math.Min(segOffset, section.m_header.sh_offset);
                        }
                    }

                    segment.m_header.p_filesz = segSize;
                    segment.m_header.p_memsz  = segSize;
                    segment.m_header.p_offset = segOffset;

                    phdrs[i] = segment.m_header;
                    i++;
                }

                i = 0;
                foreach (var section in elf.Sections)
                {
                    if (!usedList.Contains(section))
                    {
                        Buffer.BlockCopy(section.Raw, 0, raw, 
                            rawIndex, section.Raw.Length);

                        section.m_header.sh_offset = (uint)(rawOffset + rawIndex);
                        section.m_header.sh_size   = (uint) section.Raw.Length;

                        rawIndex += section.Raw.Length;
                    }

                    shdrs[i] = section.m_header;
                    i++;
                }

                FileStream fs = null;

                try
                {
                    fs = File.Open(path, FileMode.Create);

                    WriteElfHeader( elf.m_header, fs );

                    //WriteStructureArray(phdrs, fs);
                    foreach (var phdr in phdrs)
                    {
                        WriteStructure(phdr, fs);
                    }
                    
                    fs.Write(raw, 0, raw.Length);

                    //WriteStructureArray(shdrs, fs);
                    foreach (var shdr in shdrs)
                    {
                        WriteStructure(shdr, fs);
                    }
                    
                    fs.Close();
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Dispose();   
                    }
                }
            }

            private static uint CalcRawByteSize(IEnumerable<ElfSection> sections)
            {
                uint total = 0;

                foreach (var section in sections)
                {
                    total += (uint)section.Raw.Length;
                }

                return total;
            }

            internal static bool SeekToElfHeader(Stream reader)
            {
                bool foundElf = false;
                int state = 0;
                int by;

                while( !foundElf && -1 != ( by = reader.ReadByte() ) )
                {
                    switch( by )
                    {
                        case 0x7F:
                            state = ( state == 0 ) ? state + 1 : 0;
                            break;
                        case 0x45:
                            state = ( state == 1 ) ? state + 1 : 0;
                            break;
                        case 0x4C:
                            state = ( state == 2 ) ? state + 1 : 0;
                            break;
                        case 0x46:
                            if( state == 3 )
                            {
                                foundElf = true;
                                reader.Seek( -4, SeekOrigin.Current );
                            }
                            else
                            {
                                state = 0;
                            }
                            break;
                        default:
                            state = 0;
                            break;
                    }
                }

                return foundElf;
            }

            internal static Elf32_Ehdr ReadElfHeader(Stream reader, ref long elfOffset)
            {
                if( !SeekToElfHeader(reader)) throw new ArgumentException();

                elfOffset = reader.Position;

                var size = Marshal.SizeOf(typeof (Elf32_Ehdr));

                var ptr  = Marshal.AllocHGlobal(size);

                var buff = new byte[size];

                reader.Read(buff, 0, buff.Length);

                Marshal.Copy(buff, 0, ptr, buff.Length);

                var hdr = (Elf32_Ehdr)Marshal.PtrToStructure(ptr, typeof (Elf32_Ehdr));

                Marshal.FreeHGlobal(ptr);

                return hdr;
            }

            internal static void WriteElfHeader(Elf32_Ehdr hdr, Stream writer)
            {
                var size = Marshal.SizeOf(typeof (Elf32_Ehdr));

                writer.Seek(0, SeekOrigin.Begin);

                var ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(hdr, ptr, false);

                var raw = new byte[size];

                Marshal.Copy(ptr, raw, 0, size);

                writer.Write(raw, 0, raw.Length);

                Marshal.FreeHGlobal(ptr);
            }

            internal static void WriteStructure<T>(T val, Stream writer)
                where T : struct 
            {
                var size = Marshal.SizeOf(typeof (T));
                
                var ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(val, ptr, false);

                var raw = new byte[size];

                Marshal.Copy(ptr, raw, 0, size);

                writer.Write(raw, 0, raw.Length);

                Marshal.FreeHGlobal(ptr);
            }

            //internal static void WriteStructureArray<T>(T[] arr, Stream writer)
            //    where T : struct 
            //{
            //    var size = Marshal.SizeOf( typeof(T)) * arr.Length;
                
            //    var ptr = Marshal.AllocHGlobal(size);

            //    Marshal.StructureToPtr(arr[0], ptr, false);

            //    var raw = new byte[size];

            //    Marshal.Copy(ptr, raw, 0, size);

            //    writer.Write(raw, 0, raw.Length);

            //    //test
                
            //    var copyback = new T[arr.Length];

            //    var handle   = GCHandle.Alloc(copyback, GCHandleType.Pinned);
                
            //    var ptr2      = handle.AddrOfPinnedObject();

            //    var buff     = new byte[size];

            //    Marshal.Copy(raw,
            //                 0,
            //                 ptr2,
            //                 buff.Length);

            //    handle.Free();

            //    // end test
                
            //    Marshal.FreeHGlobal(ptr);
            //}

            internal static Elf32_Phdr[] ReadProgramHeaders(Elf32_Ehdr hdr, Stream reader, long elfOffset)
            {
                if( hdr.e_phnum == 0 ) return new Elf32_Phdr[0];

                var hdrs    = new Elf32_Phdr[hdr.e_phnum];

                var handle  = GCHandle.Alloc(hdrs, GCHandleType.Pinned);
                
                var ptr     = handle.AddrOfPinnedObject();

                var buff    = new byte[hdr.e_phentsize * hdr.e_phnum];

                reader.Seek(hdr.e_phoff + elfOffset, SeekOrigin.Begin);

                reader.Read(buff, 0, buff.Length);

                Marshal.Copy(buff,
                             0,
                             ptr,
                             buff.Length);

                handle.Free();

                return hdrs;
            }

            internal static Elf32_Shdr[] ReadSectionHeaders(Elf32_Ehdr hdr, Stream reader, long elfOffset)
            {
                var sections = new Elf32_Shdr[hdr.e_shnum];

                var handle   = GCHandle.Alloc(sections, GCHandleType.Pinned);
                
                var ptr      = handle.AddrOfPinnedObject();

                var buff     = new byte[hdr.e_shentsize * hdr.e_shnum];

                reader.Seek(hdr.e_shoff + elfOffset, SeekOrigin.Begin);

                reader.Read(buff, 0, buff.Length);

                Marshal.Copy(buff,
                             0,
                             ptr,
                             buff.Length);

                handle.Free();

                return sections;
            }

            internal static T[] ReadSectionEntries<T>(Elf32_Shdr hdr, Stream reader, long elfOffset)
            {
                var entries = new T[hdr.sh_size / hdr.sh_entsize];

                var handle  = GCHandle.Alloc(entries, GCHandleType.Pinned);

                var ptr     = handle.AddrOfPinnedObject();

                var buff    = new byte[hdr.sh_size];

                reader.Seek(hdr.sh_offset + elfOffset, SeekOrigin.Begin);

                reader.Read(buff, 0, buff.Length);

                Marshal.Copy(buff, 0, ptr, buff.Length);

                handle.Free();

                return entries;
            }

            internal static byte[] ReadSectionData(Elf32_Shdr hdr, Stream reader, long elfOffset)
            {
                var buff = new byte[hdr.sh_size];

                reader.Seek(hdr.sh_offset + elfOffset, SeekOrigin.Begin);

                reader.Read(buff, 0, buff.Length);

                return buff;
            }
        }
    }
}


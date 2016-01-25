//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ExternalMethodImporters
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.Elf;
    using Microsoft.Zelig.CodeGeneration.IR.ImageBuilders;
    using Microsoft.Zelig.CodeGeneration.IR.Abstractions;
    using System.IO;
    using Microsoft.Zelig.TargetModel.ArmProcessor;

    /// <summary>
    /// 
    /// </summary>
    public class ArmElfExternalCallContext : ArmElfContext, ExternalCallOperator.IExternalCallContext
    {
        private static InstructionSet         s_iset;
        //--//
        private readonly string               m_methodName;
        private readonly string               m_filePath;
        private          Operator             m_owner;
        private readonly ElfSection           m_elfSection;
        private          Debugging.DebugInfo  m_dbgInfo;
        private          uint                 m_offset;

        public Operator Owner
        {
            get { return m_owner;  }
            set { m_owner = value; }
        }

        internal ElfSection ElfSection
        {
            get { return m_elfSection; }
        }

        public uint OperatorOffset
        {
            get
            {
                return m_offset;
            }
        }

        public static void Initialize(TypeSystemForCodeTransformation typeSystem)
        {
            s_iset = typeSystem.PlatformAbstraction.GetInstructionSetProvider();

            ElfObject.SystemArchitecture arch = ElfObject.SystemArchitecture.ArmV5;
            ElfObject.FloatingPointUnit fpu = ElfObject.FloatingPointUnit.None;
            ElfObject.Endian endian = ElfObject.Endian.Little;

            switch(typeSystem.PlatformAbstraction.PlatformVersion)
            {
                case InstructionSetVersion.Platform_Version__ARMv4:
                    arch = ElfObject.SystemArchitecture.ArmV4;
                    break;

                case InstructionSetVersion.Platform_Version__ARMv5:
                    arch = ElfObject.SystemArchitecture.ArmV5;
                    break;

                case InstructionSetVersion.Platform_Version__ARMv6M:
                    arch = ElfObject.SystemArchitecture.Thumb1;
                    break;

                case InstructionSetVersion.Platform_Version__ARMv7M:
                    arch = ElfObject.SystemArchitecture.Thumb2;
                    break;

                default:
                    throw new ArgumentException("Unknown or unsupported platform version");
            }

            switch(typeSystem.PlatformAbstraction.PlatformVFP)
            {
                case InstructionSetVersion.Platform_VFP__NoVFP:
                    fpu = ElfObject.FloatingPointUnit.None;
                    break;

                case InstructionSetVersion.Platform_VFP__SoftVFP:
                    fpu = ElfObject.FloatingPointUnit.SoftVFP;
                    break;

                case InstructionSetVersion.Platform_VFP__HardVFP:
                    fpu = ElfObject.FloatingPointUnit.VFP;
                    break;

                default:
                    throw new ArgumentException("Unknown or unsupported FP mode");
            }

            if(typeSystem.PlatformAbstraction.PlatformBigEndian)
            {
                endian = ElfObject.Endian.Big;
            }

            ElfObject.FileUtil.DeviceConfiguration = new ElfObject.RvctDeviceType( arch, fpu, endian );

            GlobalContext.ResetState();
        }

        public static void Reset()
        {
            GlobalContext.ResetState();
        }

        public static void ResetExternCalls()
        {
            GlobalContext.ResetExternalCalls();
        }

        private ArmElfExternalCallContext( string methodName, ElfSection section )
        {
            m_methodName  = methodName;
            m_filePath    = section.Parent.FileName;
            m_owner       = null;
            m_elfSection  = section;

        }

        public static object GetCodeForMethod( string methodName, out uint methodOffset )
        {
            return GlobalContext.GetCodeForMethod( methodName, out methodOffset );
        }

        private static ElfSection FindElfSection( string filePath, string symName )
        {
            ElfSection elfSection = GlobalContext.ParseElfFileForSymbol( symName, filePath );

            if(elfSection == null)
            {
                elfSection = GlobalContext.FindExternSymbol( symName );
            }

            return elfSection;
        }

        public static ArmElfExternalCallContext[] Load( string filePath, string methodName, BasicBlock owner )
        {
            ArmElfExternalCallContext        item = null;
            Queue<ArmElfExternalCallContext> queue  = new Queue<ArmElfExternalCallContext>();
            List<ArmElfExternalCallContext>  lst  = new List<ArmElfExternalCallContext>();
            HashSet<string> dataSymLookup = new HashSet<string>();
            ElfSection section;

            GlobalContext.IntializeExportMethods( owner.Owner.TypeSystemForIR );

            string path = Path.GetFullPath( filePath ).ToLower();

            section = FindElfSection( path, methodName );

            if(section != null)
            {
                lst.Add( item = new ArmElfExternalCallContext( methodName, section ) );

                GlobalContext.RegisterExternalMethodContext( section.Name, item );

                foreach(string alias in item.m_elfSection.Aliases.Keys)
                {
                    GlobalContext.RegisterExternalMethodContext( alias, item );
                }

                Console.WriteLine( "{0,-30}: {1}", Path.GetFileName(section.Parent.FileName), methodName );

                queue.Enqueue( item );

                while(queue.Count > 0)
                {
                    item = queue.Dequeue();

                    foreach(SectionReference sr in item.m_elfSection.References)
                    {
                        string symName = sr.SectionName;

                        if(!GlobalContext.HasExternalMethodContext( symName ) &&
                           !GlobalContext.IsManagedCodeSymbol( symName ) &&
                           !dataSymLookup.Contains( symName ))
                        {
                            section = FindElfSection( sr.Section.Parent.FileName, symName );

                            if(section == null) continue;

                            ArmElfExternalCallContext tmp = new ArmElfExternalCallContext( symName, section );

                            if(!tmp.m_elfSection.IsDataSection)
                            {
                                lst.Add( tmp );
                            }

                            queue.Enqueue( tmp );

                            if(!tmp.m_elfSection.IsDataSection)
                            {
                                GlobalContext.RegisterExternalMethodContext( tmp.m_elfSection.Name, tmp );

                                foreach(string alias in tmp.m_elfSection.Aliases.Keys)
                                {
                                    GlobalContext.RegisterExternalMethodContext( alias, tmp );
                                }

                                Console.WriteLine( "{0,-30}: {1}", Path.GetFileName(section.Parent.FileName), symName );
                            }
                            else
                            {
                                dataSymLookup.Add( tmp.m_methodName );
                            }
                        }
                    }
                }
            }

            return lst.ToArray();
        }

        public Debugging.DebugInfo DebugInfo
        {
            get
            {
                if(m_dbgInfo == null)
                {
                    if(m_elfSection.DebugEntries.Count > 0)
                    {
                        DebugInfoEntry die = m_elfSection.DebugEntries[0];

                        if(die.m_lines.Count > 0)
                        {
                            DebugLineEntry dle = die.m_lines[0];

                            if(dle.m_files.Count > 0)
                            {
                                object arg;
                                int startLine = 1;
                                int startCol = 1;

                                if(dle.m_opcode.Count >= 3)
                                {
                                    // opcode 2 is the start line
                                    arg = dle.m_opcode[2].Arg;
                                    if(arg is uint)
                                    {
                                        startLine = (int)(uint)arg;
                                    }
                                    else if(arg is int)
                                    {
                                        startLine = (int)arg;
                                    }
                                    // opcode 1 is the start column
                                    arg = dle.m_opcode[1].Arg;
                                    if(arg is uint)
                                    {
                                        startCol = (int)(uint)arg;
                                    }
                                    else if(arg is int)
                                    {
                                        startCol = (int)arg;
                                    }
                                }
                                else
                                {
                                    // TODO: find a way to include macros with no line number opcodes
                                    return null;
                                }

                                string file = dle.m_files[0].m_name;

                                if(dle.m_includeDirs.Count > 0)
                                {
                                    if(!File.Exists( file ))
                                    {
                                        foreach(string dir in dle.m_includeDirs)
                                        {
                                            if(File.Exists( dir + file ))
                                            {
                                                file = dir + file;
                                                break;
                                            }
                                        }
                                    }
                                }

                                m_dbgInfo = new Debugging.DebugInfo(
                                                file,
                                                m_elfSection.Name,
                                                startLine,
                                                startCol,
                                                startLine + 1,
                                                startCol + 1 );
                            }
                        }
                    }
                }

                return m_dbgInfo;
            }
        }

        public void UpdateRelocation( object relocation )
        {
            ImageBuilders.CodeRelocation reloc = (ImageBuilders.CodeRelocation)relocation;

            m_elfSection.AddressInMemory = reloc.InsertionAddress;
        }

        public void AddExternalData( ImageBuilders.Core coreBuilder, ElfSection[] dataSections )
        {
            GlobalContext.AddGlobalDataSections( dataSections, coreBuilder, this );
        }

        public void AddExternalData( ImageBuilders.Core coreBuilder, SectionReference sr )
        {
            if(sr == null) return;

            if(!string.IsNullOrEmpty( sr.Section.Name ))
            {
                AddExternalData( coreBuilder, sr.Section.Parent.DataEntries );
            }
            else
            {
                ElfSection ownerSection = GlobalContext.FindExternSymbol( sr.SectionName );

                if(ownerSection != null)
                {
                    AddExternalData( coreBuilder, ownerSection.Parent.DataEntries );

                    sr.Section = ownerSection;
                }
            }
        }

        public bool PerformCodeLinkage( object region, object section, object core )
        {
            try
            {
                ImageBuilders.Core coreBuilder = core as ImageBuilders.Core;

                SequentialRegion sqr = (SequentialRegion)region;
                SequentialRegion.Section sec = (SequentialRegion.Section)section;
                Dictionary<ElfSection, uint> sectionPlacementMap = new Dictionary<ElfSection, uint>();

                GlobalContext.IntializeExportMethods( m_owner.BasicBlock.Owner.TypeSystemForIR );

                AddExternalData( coreBuilder, m_elfSection.Parent.DataEntries );

                sec.AlignToWord();

                m_offset = sec.Offset;

                sec.Write( m_elfSection.Raw );

                sec.AlignToWord();

                sectionPlacementMap[m_elfSection] = 0;

                foreach(SectionReference sr in m_elfSection.References)
                {
                    AddExternalData( coreBuilder, sr );

                    ParseReferences( sqr, m_offset, sr, sectionPlacementMap );
                }
            }
            catch
            {
                Console.WriteLine( "ERROR" );
                return false;
            }
            return true;
        }

        private void ParseReferences(
            SequentialRegion sqr,
            uint sectionBaseAddr,
            SectionReference sr,
            Dictionary<ElfSection, uint> secPlacementMap )
        {
            foreach(SectionReference.SymbolReference symRef in sr.CallOffsets)
            {
                uint                  callOffset = symRef.Offset;
                uint                  op         = BitConverter.ToUInt32( m_elfSection.Raw, (int)callOffset );
                InstructionSet.Opcode opcode     = s_iset.Decode( op );
                RelocationEntry       reloc      = symRef.RelocationRef;
                Symbol                sym        = reloc.ReferencedSymbol;
                SymbolType            st         = sym.Type;
                SymbolBinding         sb         = sym.Binding;
                string                symName    = sym.Name;

                callOffset += sectionBaseAddr;

                if(st == SymbolType.STT_FUNC || st == SymbolType.STT_NOTYPE)
                {
                    if(opcode is InstructionSet.Opcode_Branch)
                    {
                        uint methodOffset;
                        object target = GlobalContext.GetCodeForMethod( symName, out methodOffset );

                        if(target != null)
                        {
                            switch(reloc.Type)
                            {
                                case RelocationType.R_ARM_JUMP24:
                                case RelocationType.R_ARM_CALL:
                                    new ExternMethodCallRelocation( sqr, callOffset, UpdateExternalMethodCall, methodOffset, target, EncodingDefinition.c_PC_offset );
                                    break;

                                default:
                                    Console.WriteLine( "NOT IMPLEMENTED: relocation type: " + reloc.Type.ToString() );
                                    throw new InvalidOperationException( "ERROR: Unhandled relocation type: " + reloc.Type.ToString() );
                            }
                        }
                        else
                        {
                            Console.WriteLine( "UNABLE TO FIND EXTERNAL CALL REFERENCE " + symName );
                        }
                    }
                    else if(opcode is InstructionSet.Opcode_DataProcessing_2)
                    {
                        uint dataOffset = 0;

                        object ctx = GlobalContext.GetExternalDataContext( symName, symRef, op, out dataOffset );

                        if(ctx == null)
                        {
                            ctx = GlobalContext.GetCodeForMethod( symName, out dataOffset );
                        }

                        if(ctx != null)
                        {
                            switch(reloc.Type)
                            {
                                case RelocationType.R_ARM_ABS32:
                                    new ExternalPointerRelocation( sqr, callOffset, dataOffset, ctx );
                                    break;

                                default:
                                    Console.WriteLine( "NOT IMPLEMENTED: relocation type: " + reloc.Type.ToString() );
                                    throw new InvalidOperationException( "ERROR: Unhandled relocation type: " + reloc.Type.ToString() );
                            }
                        }
                        else
                        {
                            Console.WriteLine( "UNABLE TO FIND EXTERNAL DATA REFERENCE " + symName );
                        }
                    }
                    else
                    {
                        Console.WriteLine( "NOT IMPLEMENTED: " + opcode.ToString() );
                    }
                }
                else if(st == SymbolType.STT_SECTION || st == SymbolType.STT_OBJECT)
                {
                    uint dataOffset;

                    ExternalDataDescriptor.IExternalDataContext ctx = GlobalContext.GetExternalDataContext( symName, symRef, op, out dataOffset );

                    if(ctx != null)
                    {
                        switch(reloc.Type)
                        {
                            case RelocationType.R_ARM_ABS32:
                                new ExternalPointerRelocation( sqr, callOffset, dataOffset, ctx ); 
                                break;

                            default:
                                Console.WriteLine( "NOT IMPLEMENTED: relocation type: " + reloc.Type.ToString() );
                                throw new InvalidOperationException( "ERROR: Unhandled relocation type: " + reloc.Type.ToString() );
                        }
                    }
                    else
                    {
                        Console.WriteLine( "ERROR: Unable to find symbol: " + symName );
                    }
                }
                else
                {
                    Console.WriteLine( "NOT IMPLEMENTED: " + st.ToString() );
                }
            }
        }

        private uint UpdateExternalMethodCall( uint opcode, uint callerAddress, uint calleeAddress )
        {
            InstructionSet.Opcode op = s_iset.Decode( opcode );

            if(op is InstructionSet.Opcode_Branch)
            {
                InstructionSet.Opcode_Branch branch = (InstructionSet.Opcode_Branch)op;

                branch.Prepare( branch.ConditionCodes, (int)calleeAddress - EncodingDefinition.c_PC_offset - (int)callerAddress, branch.IsLink );

                return branch.Encode();
            }
            else
            {
                Console.WriteLine( "ERROR: UNEXPECTED OPCODE!!!!" );
            }

            return opcode;
        }
    }
}

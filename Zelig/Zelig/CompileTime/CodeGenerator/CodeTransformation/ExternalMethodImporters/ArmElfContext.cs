//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ExternalMethodImporters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Zelig.Elf;
    using Microsoft.Zelig.CodeGeneration.IR.ImageBuilders;
    using Microsoft.Zelig.Runtime.TypeSystem;
    using System.IO;


    public abstract class ArmElfContext
    {
        protected class ArmElfGlobalContext
        {
            private ArmElfGlobalContext()
            {
                m_symToCallContext = new Dictionary<string, ArmElfExternalCallContext>();

                m_fileNameToElfObject = new Dictionary<string, ElfObject[]>();
                m_methodToArmElfSection = new Dictionary<string, ElfSection>();

                ///
                /// There can be more than one method (or alias) per ArmElfExternalCallContext with its
                /// own offset, therefore the following two items can not be combined without creating 
                /// yet another class
                ///
                m_methodToElfSectionOffset = new Dictionary<string, uint>();
                m_methodToExternalCallCtx = new Dictionary<string, ArmElfExternalCallContext>();
                ///
                /// There can be more than one data symbol (or alias) per ArmElfExternalCallContext with
                /// its own offset, therefore the following two items can not be combined without creating 
                /// yet another class
                ///
                m_symbolToGlobalDataOffset = new Dictionary<string, uint>();
                m_symbolToGlobalDataCtx = new Dictionary<string, ArmElfExternalDataContext>();

                m_symbolToArmElfFile = new Dictionary<string, string>();
                m_elfSectionToSeqRegion = new Dictionary<ElfSection, SequentialRegion>();

                m_methodNameToMethodDef = new Dictionary<string, MethodRepresentation>();
            }

            public static ArmElfGlobalContext Instance
            {
                get
                {
                    if(s_instance == null)
                    {
                        lock(s_lock)
                        {
                            if(s_instance == null)
                            {
                                s_instance = new ArmElfGlobalContext();
                            }
                        }
                    }

                    return s_instance;
                }
            }

            public void ResetExternalCalls()
            {
                m_symToCallContext.Clear();
            }

            public void ResetState()
            {
                m_methodToElfSectionOffset.Clear();

                m_symbolToGlobalDataCtx.Clear();
                m_symbolToGlobalDataOffset.Clear();

                m_methodNameToMethodDef.Clear();
                m_elfSectionToSeqRegion.Clear();
            }

            public bool HasExternalMethodContext( string symName )
            {
                return m_symToCallContext.ContainsKey( symName );
            }

            public bool IsManagedCodeSymbol( string symName )
            {
                return m_methodNameToMethodDef.ContainsKey( symName );
            }

            public void RegisterExternalMethodContext( string symName, ArmElfExternalCallContext ctx )
            {
                m_symToCallContext[symName] = ctx;
            }

            public object GetCodeForMethod( string methodName, out uint methodOffset )
            {
                methodOffset = 0;

                if(m_symToCallContext.ContainsKey( methodName ))
                {
                    ArmElfExternalCallContext ctx = m_symToCallContext[methodName];

                    if(ctx.ElfSection.Aliases.ContainsKey( methodName ))
                    {
                        methodOffset = ctx.ElfSection.Aliases[methodName];
                    }

                    return m_symToCallContext[methodName].Owner;
                }

                if(m_methodNameToMethodDef.ContainsKey( methodName ))
                {
                    return m_methodNameToMethodDef[methodName].Code;
                }

                return null;
            }

            public ExternalDataDescriptor.IExternalDataContext GetExternalDataContext( string symName, SectionReference.SymbolReference symRef, uint opcode, out uint dataOffset )
            {
                ExternalDataDescriptor.IExternalDataContext retVal = null;
                dataOffset = 0;

                if(m_elfSectionToSeqRegion.ContainsKey( symRef.Owner.Section ))
                {
                    Symbol        sym = symRef.RelocationRef.ReferencedSymbol;
                    bool hasOffset = false;
                   

                    dataOffset = opcode;

                    foreach(string key in symRef.Owner.Section.Aliases.Keys)
                    {
                        if(key == symName)
                        {
                            dataOffset += symRef.Owner.Section.Aliases[key];
                            hasOffset = true;
                            break;
                        }
                    }

                    if(!hasOffset)
                    {
                        dataOffset += sym.SymbolDef.st_value;
                    }

                    ExternalDataDescriptor edd = m_elfSectionToSeqRegion[symRef.Owner.Section].Context as ExternalDataDescriptor;

                    retVal = edd.ExternContext;

                    m_symbolToGlobalDataCtx[symName] = retVal as ArmElfExternalDataContext;
                    m_symbolToGlobalDataOffset[symName] = dataOffset;
                }
                else if(m_symbolToGlobalDataCtx.ContainsKey( symName ))
                {
                    dataOffset = m_symbolToGlobalDataOffset[symName];

                    retVal = m_symbolToGlobalDataCtx[symName];

                    symRef.Owner.Section = (ElfSection)retVal.DataSection;
                }

                return retVal;
            }

            public void AddGlobalDataSections( ElfSection[] dataSections, ImageBuilders.Core coreBuilder, ArmElfExternalCallContext owner ) // SequentialRegion region )
            {
                foreach(ElfSection dataSec in dataSections)
                {
                    if(!m_elfSectionToSeqRegion.ContainsKey( dataSec ))
                    {
                        m_elfSectionToSeqRegion[dataSec] = coreBuilder.AddExternalData( new ArmElfExternalDataContext( dataSec, owner ), dataSec.Header.sh_size );
                    }
                }
            }

            public bool IsSymbolPlaced( string symbolName, ElfSection section )
            {
                if(m_methodToExternalCallCtx.ContainsKey( symbolName )) return true;
                if(m_methodNameToMethodDef.ContainsKey( symbolName )) return true;
                if(m_symbolToGlobalDataCtx.ContainsKey( symbolName ))
                {
                    ArmElfExternalDataContext ctx = m_symbolToGlobalDataCtx[symbolName];

                    if(ctx.Section == section)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void RegisterMethodCall( string methodName, uint sectionOffset, SectionReference sectionRef, ArmElfExternalCallContext ctx )
            {
                m_methodToElfSectionOffset[methodName] = sectionOffset;
                m_methodToExternalCallCtx[methodName] = ctx;

                foreach(string alias in sectionRef.Section.Aliases.Keys)
                {
                    m_methodToElfSectionOffset[alias] = sectionOffset + sectionRef.Section.Aliases[alias];
                    m_methodToExternalCallCtx[alias] = ctx;
                }
            }

            public ElfSection ParseElfFileForSymbol( string symName, string filePath )
            {
                ElfSection retVal = null;

                ElfObject[] objs = null; 

                if(m_methodToArmElfSection.ContainsKey( symName ))
                {
                    return m_methodToArmElfSection[symName];
                }

                string key = Path.GetFullPath( filePath ).ToLower();

                if(GlobalContext.m_fileNameToElfObject.ContainsKey( key ))
                {
                    objs = GlobalContext.m_fileNameToElfObject[key];
                }
                else
                {
                    objs = ElfObject.FileUtil.Parse( filePath );

                    GlobalContext.m_fileNameToElfObject[key] = objs;
                }


                foreach(ElfObject obj in objs)
                {
                    foreach(ElfSection sec in obj.Sections)
                    {
                        string name = sec.Name;

                        // Remove this check when the elf parser does a better job distinguishing between sections
                        if(!name.StartsWith( ".rel" ) && !name.StartsWith( ".debug" ))
                        {
                            if(!m_symbolToArmElfFile.ContainsKey( name ))
                            {
                                m_symbolToArmElfFile[name] = filePath.ToLower();
                            }

                            foreach(string alias in sec.Aliases.Keys)
                            {
                                if(!m_symbolToArmElfFile.ContainsKey( alias ))
                                {
                                    m_symbolToArmElfFile[alias] = filePath.ToLower();
                                }
                            }

                            // add all sections so that we can just do a lookup next time around
                            if(retVal == null)
                            {
                                if(0 == name.CompareTo( symName ) || sec.Aliases.ContainsKey( symName ))
                                {
                                    retVal = sec;
                                }
                            }
                        }
                    }
                }

                return retVal;
            }

            public ElfSection FindExternSymbol( string symName )
            {
                ElfSection retVal = null;

                if(m_methodNameToMethodDef.ContainsKey( symName ))
                {
                    return null;
                }

                if(m_methodToArmElfSection.ContainsKey( symName ))
                {
                    return m_methodToArmElfSection[symName];
                }

                if(m_symbolToArmElfFile.ContainsKey( symName ))
                {
                    retVal = ParseElfFileForSymbol( symName, m_symbolToArmElfFile[symName] );

                    if(retVal == null)
                    {
                        m_symbolToArmElfFile.Remove( symName );
                    }
                }

                if(retVal == null)
                {
                    foreach(string file in ExternalCallOperator.NativeImportLibraries)
                    {
                        if(m_symbolToArmElfFile.ContainsValue( file.ToLower() )) continue;

                        string val = file.ToLower();

                        retVal = ParseElfFileForSymbol( symName, file.ToLower() );

                        if(retVal != null) break;
                    }
                }

                if(retVal == null)
                {
                    foreach(string dir in ExternalCallOperator.NativeImportDirectories)
                    {
                        if(!Directory.Exists( dir )) continue;

                        foreach(string file in Directory.GetFiles( dir ))
                        {
                            if(m_symbolToArmElfFile.ContainsValue( file.ToLower() )) continue;

                            retVal = ParseElfFileForSymbol( symName, file );

                            if(retVal != null) break;
                        }

                        if(retVal != null) break;
                    }
                }

                if(retVal != null)
                {
                    m_methodToArmElfSection[symName] = retVal;
                }

                return retVal;
            }

            public void IntializeExportMethods( TypeSystemForIR typeSystemForIR )
            {
                if(m_methodNameToMethodDef.Count == 0)
                {
                    List<MethodRepresentation> exportedMethods = typeSystemForIR.ExportedMethods;

                    foreach(MethodRepresentation md in exportedMethods)
                    {
                        m_methodNameToMethodDef[md.Name] = md;
                    }
                }
            }

            //--//

            private Dictionary<string, ArmElfExternalCallContext> m_symToCallContext;

            private Dictionary<string, ElfObject[]              > m_fileNameToElfObject;
            private Dictionary<string, ElfSection               > m_methodToArmElfSection;

            private Dictionary<string, uint                     > m_methodToElfSectionOffset;
            private Dictionary<string, ArmElfExternalCallContext> m_methodToExternalCallCtx;
            private Dictionary<string, uint                     > m_symbolToGlobalDataOffset;
            private Dictionary<string, ArmElfExternalDataContext> m_symbolToGlobalDataCtx;
            private Dictionary<string, string                   > m_symbolToArmElfFile;
            private Dictionary<ElfSection, SequentialRegion     > m_elfSectionToSeqRegion;

            private Dictionary<string, MethodRepresentation     > m_methodNameToMethodDef;
            private static object                                 s_lock = new object();
            private static ArmElfGlobalContext                    s_instance;

        }

        protected static ArmElfGlobalContext GlobalContext
        {
            get
            {
                return ArmElfGlobalContext.Instance;
            }
        }
    }
}

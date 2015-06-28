//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public class Parser
    {
        internal delegate int IndexReader( ArrayReader reader );

        internal class TableSchema
        {
            //
            // State
            //

            internal readonly int            m_rows;
            internal          ArrayReader    m_reader;
            internal          int            m_rowSize;
            internal          ColumnSchema[] m_columns;

            //
            // Constructor Methods
            //

            internal TableSchema( int rows )
            {
                m_rows  = rows;
            }
        }

        internal class ColumnSchema
        {
            //
            // State
            //

            internal byte        m_size;
            internal byte        m_offset;
            internal IndexReader m_reader;
        }

        //--//

        //
        // Same secondary tables don't have a pointer to the primary table (FieldDef, MethodDef, ParamDef, Event, and Property).
        //
        // This generic builds a lookup table as the primary table is parsed.
        //
        class ReverseListLookup<T>
        {
            //
            // State
            //

            int currentIndex;
            T   current;
            T[] list;

            //
            // Constructor Methods
            //

            internal ReverseListLookup( int count )
            {
                this.currentIndex = 0;
                this.list         = new T[count];
            }

            internal void Add( T   item  ,
                               int index )
            {
                index--;

                while(this.currentIndex < index)
                {
                    this.list[this.currentIndex++] = this.current;
                }

                this.current = item;
            }

            internal void Complete()
            {
                while(this.currentIndex < this.list.Length)
                {
                    this.list[this.currentIndex++] = this.current;
                }
            }

            internal T Get( int index )
            {
                index--;

                return list[index];
            }
        }

        //--//

        private const int METAMODEL_MAJOR_VER_A     = 1;
        private const int METAMODEL_MINOR_VER_A1    = 0;
        private const int METAMODEL_MINOR_VER_A2    = 1;

        private const int METAMODEL_MAJOR_VER_B     = 2;
        private const int METAMODEL_MINOR_VER_B     = 0;

        private const int HEAPBITS_MASK_STRINGS     = 0x01;
        private const int HEAPBITS_MASK_GUID        = 0x02;
        private const int HEAPBITS_MASK_BLOB        = 0x04;
        private const int HEAPBITS_MASK_PADDING_BIT = 0x08;
        private const int HEAPBITS_MASK_DELTA_ONLY  = 0x20;
        private const int HEAPBITS_MASK_EXTRA_DATA  = 0x40;
        private const int HEAPBITS_MASK_HAS_DELETE  = 0x80;

        private static readonly byte[][] TableColumnKinds = new byte[(int)TokenType.Count][]
        {
            // rModuleCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Guid,
                (byte) ColumnKindId.Guid,
                (byte) ColumnKindId.Guid
            },
            // rTypeRefCols
            new byte[]
            {
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.ResolutionScope,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.String
            },
            // rTypeDefCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.TypeDefOrRef,
                (byte) TokenType.Field,
                (byte) TokenType.Method
            },
            // rFieldPtrCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) TokenType.Field
            ////},
            // rFieldCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Blob
            },
            // rMethodPtrCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) TokenType.Method
            ////},
            // rMethodCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Blob,
                (byte) TokenType.Param
            },
            // rParamPtrCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) TokenType.Param
            ////},
            // rParamCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.String
            },
            // rInterfaceImplCols
            new byte[]
            {
                (byte) TokenType.TypeDef,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.TypeDefOrRef
            },
            // rMemberRefCols
            new byte[]
            {
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.MemberRefParent,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Blob
            },
            // rConstantCols
            new byte[]
            {
                (byte) ColumnKindId.Byte,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.HasConstant,
                (byte) ColumnKindId.Blob
            },
            // rCustomAttributeCols
            new byte[]
            {
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.HasCustomAttribute,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.CustomAttributeType,
                (byte) ColumnKindId.Blob
            },
            // rFieldMarshalCols
            new byte[]
            {
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.HasFieldMarshal,
                (byte) ColumnKindId.Blob
            },
            // rDeclSecurityCols
            new byte[]
            {
                (byte) ColumnKindId.Short,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.HasDeclSecurity,
                (byte) ColumnKindId.Blob
            },
            // rClassLayoutCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.ULong,
                (byte) TokenType.TypeDef
            },
            // rFieldLayoutCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) TokenType.Field
            },
            // rStandAloneSigCols
            new byte[]
            {
                (byte) ColumnKindId.Blob
            },
            // rEventMapCols
            new byte[]
            {
                (byte) TokenType.TypeDef,
                (byte) TokenType.Event
            },
            // rEventPtrCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) TokenType.Event
            ////},
            // rEventCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.TypeDefOrRef
            },
            // rPropertyMapCols
            new byte[]
            {
                (byte) TokenType.TypeDef,
                (byte) TokenType.Property
            },
            // rPropertyPtrCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) TokenType.Property
            ////},
            // rPropertyCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Blob
            },
            // rMethodSemanticsCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) TokenType.Method,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.HasSemantic
            },
            // rMethodImplCols
            new byte[]
            {
                (byte) TokenType.TypeDef,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.MethodDefOrRef,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.MethodDefOrRef
            },
            // rModuleRefCols
            new byte[]
            {
                (byte) ColumnKindId.String
            },
            // rTypeSpecCols
            new byte[]
            {
                (byte) ColumnKindId.Blob
            },
            // rImplMapCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.MemberForwarded,
                (byte) ColumnKindId.String,
                (byte) TokenType.ModuleRef
            },
            // rFieldRVACols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) TokenType.Field
            },
            // rENCLogCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) ColumnKindId.ULong,
            ////    (byte) ColumnKindId.ULong
            ////},
            // rENCMapCols
            null, // Not Supported
            ////new byte[]
            ////{
            ////    (byte) ColumnKindId.ULong
            ////},
            // rAssemblyCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.Blob,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.String
            },
            // rAssemblyProcessorCols
            new byte[]
            {
                (byte) ColumnKindId.ULong
            },
            // rAssemblyOSCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.ULong
            },
            // rAssemblyRefCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.Blob,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Blob
            },
            // rAssemblyRefProcessorCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) TokenType.AssemblyRef
            },
            // rAssemblyRefOSCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.ULong,
                (byte) TokenType.AssemblyRef
            },
            // rFileCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.Blob
            },
            // rExportedTypeCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.Implementation
            },
            // rManifestResourceCols
            new byte[]
            {
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.ULong,
                (byte) ColumnKindId.String,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.Implementation
            },
            // rNestedClassCols
            new byte[]
            {
                (byte) TokenType.TypeDef,
                (byte) TokenType.TypeDef
            },
            // rGenericParamCols
            new byte[]
            {
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.UShort,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.TypeOrMethodDef,
                (byte) ColumnKindId.String,
            },
            // rMethodSpecCols
            new byte[]
            {
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.MethodDefOrRef,
                (byte) ColumnKindId.Blob
            },
            // rGenericParamConstraintCols
            new byte[]
            {
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.GenericParam,
                (byte) ColumnKindId.CodedToken + (byte) CodeToken.TypeDefOrRef
            },
        };

        // These are for informational use only!  Not used anywhere.
        private static readonly String[][] TableColumnNames = new String[(int)TokenType.Count][]
        {
            // rModuleColNames
            new String[] { "Generation", "Name", "Mvid", "EncId", "EncBaseId" },
            // rTypeRefColNames
            new String[] { "ResolutionScope", "Name", "Namespace" },
            // rTypeDefColNames
            new String[] { "Flags", "Name", "Namespace", "Extends", "FieldList", "MethodList" },
            // rFieldPtrColNames
            new String[] { "Field" },
            // rFieldColNames
            new String[] { "Flags", "Name", "Signature" },
            // rMethodPtrColNames
            new String[] { "Method" },
            // rMethodColNames
            new String[] { "RVA", "ImplFlags", "Flags", "Name", "Signature", "ParamList" },
            // rParamPtrColNames
            new String[] { "Param" },
            // rParamColNames
            new String[] { "Flags", "Sequence", "Name" },
            // rInterfaceImplColNames
            new String[] { "Class", "Interface" },
            // rMemberRefColNames
            new String[] { "Class", "Name", "Signature" },
            // rConstantColNames
            new String[] { "Type", "Parent", "Value" },
            // rCustomAttributeColNames
            new String[] { "Parent", "Type", "Value" },
            // rFieldMarshalColNames
            new String[] { "Parent", "NativeType" },
            // rDeclSecurityColNames
            new String[] { "Action", "Parent", "PermissionSet" },
            // rClassLayoutColNames
            new String[] { "PackingSize", "ClassSize", "Parent" },
            // rFieldLayoutColNames
            new String[] { "OffSet", "Field" },
            // rStandAloneSigColNames
            new String[] { "Signature" },
            // rEventMapColNames
            new String[] { "Parent", "EventList" },
            // rEventPtrColNames
            new String[] { "Event" },
            // rEventColNames
            new String[] { "EventFlags", "Name", "EventType" },
            // rPropertyMapColNames
            new String[] { "Parent", "PropertyList" },
            // rPropertyPtrColNames
            new String[] { "Property" },
            // rPropertyColNames
            new String[] { "PropFlags", "Name", "Type" },
            // rMethodSemanticsColNames
            new String[] { "Semantic", "Method", "Association" },
            // rMethodImplColNames
            new String[] { "Class", "MethodBody", "MethodDeclaration" },
            // rModuleRefColNames
            new String[] { "Name" },
            // rTypeSpecColNames
            new String[] { "Signature" },
            // rImplMapColNames
            new String[] { "MappingFlags", "MemberForwarded", "ImportName", "ImportScope" },
            // rFieldRVAColNames
            new String[] { "RVA", "Field" },
            // rENCLogColNames
            new String[] { "Token", "FuncCode" },
            // rENCMapColNames
            new String[] { "Token" },
            // rAssemblyColNames
            new String[] { "HashAlgId", "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKey", "Name", "Locale" },
            // rAssemblyProcessorColNames
            new String[] { "Processor" },
            // rAssemblyOSColNames
            new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion" },
            // rAssemblyRefColNames
            new String[] { "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKeyOrToken", "Name", "Locale", "HashValue" },
            // rAssemblyRefProcessorColNames
            new String[] { "Processor", "AssemblyRef" },
            // rAssemblyRefOSColNames
            new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion", "AssemblyRef" },
            // rFileColNames
            new String[] { "Flags", "Name", "HashValue" },
            // rExportedTypeColNames
            new String[] { "Flags", "TypeDefId", "TypeName", "TypeNamespace", "Implementation" },
            // rManifestResourceColNames
            new String[] { "Offset", "Flags", "Name", "Implementation" },
            // rNestedClassColNames
            new String[] { "NestedClass", "EnclosingClass" },
            // rGenericParamColNames
            new String[] { "Number", "Flags", "Owner", "Name" },
            // rMethodSpecNames
            new String[] { "Method", "Instantiation" },
            // rGenericParamConstraintColNames
            new String[] { "Owner", "Constraint" },
        };

        private static readonly int[] BitsForCountArray = new int[]
        {
            0,1,1,2,2,3,3,3,3,4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5
        };

        private static readonly int[] MaskForCountArray = new int[]
        {
            0,1,1,3,3,7,7,7,7,0xF,0xF,0xF,0xF,0xF,0xF,0xF,0xF,0x1F,0x1F,0x1F,
            0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F,0x1F
        };

        private static readonly TokenType[][] CodeTokenTypeLists = new TokenType[(int)CodeToken.Count][]
        {
            // TypeDefOrRef
            new TokenType[] { TokenType.TypeDef,
                              TokenType.TypeRef,
                              TokenType.TypeSpec },
            // HasConstant
            new TokenType[] { TokenType.FieldDef,
                              TokenType.ParamDef,
                              TokenType.Property },
            // HasCustomAttribute
            new TokenType[] { TokenType.MethodDef,
                              TokenType.FieldDef,
                              TokenType.TypeRef,
                              TokenType.TypeDef,
                              TokenType.ParamDef,
                              TokenType.InterfaceImpl,
                              TokenType.MemberRef,
                              TokenType.Module,
                              TokenType.Permission,
                              TokenType.Property,
                              TokenType.Event,
                              TokenType.Signature,
                              TokenType.ModuleRef,
                              TokenType.TypeSpec,
                              TokenType.Assembly,
                              TokenType.AssemblyRef,
                              TokenType.File,
                              TokenType.ExportedType,
                              TokenType.ManifestResource},
            // HasFieldMarshal
            new TokenType[] { TokenType.FieldDef,
                              TokenType.ParamDef },
            // HasDeclSecurity
            new TokenType[] { TokenType.TypeDef,
                              TokenType.MethodDef,
                              TokenType.Assembly },
            // MemberRefParent
            new TokenType[] { TokenType.Illegal,
                              TokenType.TypeRef,
                              TokenType.ModuleRef,
                              TokenType.MethodDef,
                              TokenType.TypeSpec },
            // HasSemantic
            new TokenType[] { TokenType.Event,
                              TokenType.Property },
            // MethodDefOrRef
            new TokenType[] { TokenType.MethodDef,
                              TokenType.MemberRef },
            // MemberForwarded
            new TokenType[] { TokenType.FieldDef,
                              TokenType.MethodDef },
            // Implementation
            new TokenType[] { TokenType.File,
                              TokenType.AssemblyRef,
                              TokenType.ExportedType },
            // CustomAttributeType
            new TokenType[] { TokenType.Illegal,
                              TokenType.Illegal,
                              TokenType.MethodDef,
                              TokenType.MemberRef,
                              TokenType.Illegal },
            // ResolutionScope
            new TokenType[] { TokenType.Module,
                              TokenType.ModuleRef,
                              TokenType.AssemblyRef,
                              TokenType.TypeRef },
            // TypeOrMethodDef
            new TokenType[] { TokenType.TypeDef,
                              TokenType.MethodDef },
            // GenericParam
            new TokenType[] { TokenType.GenericParam },
        };


        protected enum MetaDataFormat : byte
        {
            Invalid  ,
            ReadOnly ,
            ReadWrite,
            ICR
        }

        private enum ColumnKindId
        {
            RowIdMax      =  63,
            CodedToken    =  64,
            CodedTokenMax =  95,
            Short         =  96,
            UShort        =  97,
            Long          =  98,
            ULong         =  99,
            Byte          = 100,
            String        = 101,
            Guid          = 102,
            Blob          = 103
        };

        //
        // Section 24.2.6 of ECMA spec, Partition II
        //
        private enum CodeToken : byte
        {
            TypeDefOrRef       , // TypeDef, TypeRef, TypeSpec
            HasConstant        , // FieldDef, ParamDef, Property
            HasCustomAttribute , // Every table except CustomAttribute
            HasFieldMarshal    , // FieldDef, ParamDef
            HasDeclSecurity    , // TypeDef, MethodDef, Assembly
            MemberRefParent    , // TypeRef, ModuleRef, MethodDef, TypeSpec
            HasSemantic        , // Event, Property
            MethodDefOrRef     , // MethodDef, MethodRef
            MemberForwarded    , // FieldDef, MethodDef
            Implementation     , // File, AssemblyRef, ExportedType
            CustomAttributeType, // MethodDef, MethodRef
            ResolutionScope    , // Module, ModuleRef, AssemblyRef, TypeRef
            TypeOrMethodDef    , // TypeDef, MethodDef
            GenericParam       , // GenericParam
            Count
        };

        //
        // State
        //

        private readonly ISymbolResolverHelper                     symbolHelper;
        private readonly PELoader                                  peLoader;
        private readonly ArrayReader                               imageReader;
        private readonly ArrayReader                               metaDataReader;
        private readonly StorageStream                             dataStream;
        private readonly StorageStream                             stringPoolStream;
        private readonly StorageStream                             userBlobPoolStream;
        private readonly StorageStream                             guidPoolStream;
        private readonly StorageStream                             blobPoolStream;

        // Schema fields (for logical tables)
        private          int                                       reserved;
        private          byte                                      majorVersion;
        private          byte                                      minorVersion;
        private          byte                                      heapBits;
        private          byte                                      rowId;
        private          long                                      maskValid;
        private          long                                      maskSorted;
        private          int                                       extraData;

        //--//

        private          TableSchema[]                             schemaArray = new TableSchema[(int)TokenType.Count];

        private          byte                                      stringIndexSize;
        private          byte                                      guidIndexSize;
        private          byte                                      blobIndexSize;

        private          ArrayReader                               stringStreamReader;
        private          ArrayReader                               blobStreamReader;
        private          ArrayReader                               userStringStreamReader;
        private          ArrayReader                               resourceReader;
        private          Guid[]                                    guidArray;

        private          MetaData                                  metaDataObject;

        private          ReverseListLookup<MetaDataTypeDefinition> lookup_fieldIndexToType;
        private          ReverseListLookup<MetaDataTypeDefinition> lookup_methodIndexToType;
        private          ReverseListLookup<MetaDataTypeDefinition> lookup_eventIndexToType;
        private          ReverseListLookup<MetaDataTypeDefinition> lookup_propertyIndexToType;
        private          ReverseListLookup<MetaDataMethod>         lookup_paramIndexToMethod;

        //
        // Constructor Methods
        //

        internal Parser( String                name         ,
                         ISymbolResolverHelper symbolHelper ,
                         PELoader              peLoader     ,
                         MetaDataLoaderFlags   flags        )
        {
            int metaDataOffset = peLoader.getMetaDataOffset();
            int metaDataSize   = peLoader.getMetaDataSize  ();

            this.symbolHelper   = symbolHelper;
            this.peLoader       = peLoader;
            this.imageReader    = peLoader.getStream();
            this.metaDataReader = new ArrayReader( this.imageReader, metaDataOffset, metaDataSize );
            this.metaDataObject = new MetaData( name );

            StorageSignature storageSignature = new StorageSignature( this.metaDataReader );
            StorageHeader    storageHeader    = new StorageHeader   ( this.metaDataReader );

            // The pointer is at the first stream in the list
            MetaDataFormat format = MetaDataFormat.Invalid;

            // Lightning/Src/MD/Runtime/MDInternalDisp.cpp
            for(int i = 0; i < storageHeader.streamCount; i++)
            {
                StorageStream storageStream = new StorageStream( this.metaDataReader );

                switch(storageStream.name)
                {
                    case StorageStream.COMPRESSED_MODEL:
                        if(format == MetaDataFormat.Invalid ||
                           format == MetaDataFormat.ICR)
                        {
                            format          = MetaDataFormat.ReadOnly;
                            this.dataStream = storageStream;
                        }
                        break;

                    case StorageStream.ENC_MODEL:
                        if(format == MetaDataFormat.Invalid ||
                           format == MetaDataFormat.ICR)
                        {
                            format          = MetaDataFormat.ReadWrite;
                            this.dataStream = storageStream;
                        }
                        break;

                    case StorageStream.SCHEMA:
                        if(format == MetaDataFormat.Invalid)
                        {
                            format          = MetaDataFormat.ICR;
                            this.dataStream = storageStream;
                        }
                        break;

                    case StorageStream.STRING_POOL:
                        this.stringPoolStream = storageStream;
                        break;

                    case StorageStream.BLOB_POOL:
                        this.blobPoolStream = storageStream;
                        break;

                    case StorageStream.USER_BLOB_POOL:
                        this.userBlobPoolStream = storageStream;
                        break;

                    case StorageStream.VARIANT_POOL:
                        // It doesn't look like we are using this stream, ever
                        break;

                    case StorageStream.GUID_POOL:
                        this.guidPoolStream = storageStream;
                        break;

                    default:
                        throw IllegalMetaDataFormatException.Create( "Unknown stream name '{0}'", storageStream.name );
                }
            }


            if(format != MetaDataFormat.ReadOnly)
            {
                throw IllegalMetaDataFormatException.Create( "Unsupported format: {0}", format );
            }

            this.LoadStreams();

            this.LoadMethods( flags );

            this.metaDataObject.m_entryPoint = (MetaDataMethod)this.getObjectFromToken( peLoader.getEntryPoint() );

            this.metaDataObject.m_imageBase = peLoader.getImageBase();
        }

        //
        // Helper Methods
        //

        private void LoadStreams()
        {
            this.metaDataReader.Rewind();

            // Read the stream of GUIDs
            {
                ArrayReader reader = new ArrayReader( this.metaDataReader, guidPoolStream.offset, guidPoolStream.size );

                const int sizeOfGuid = 16;

                int guidCount = guidPoolStream.size / sizeOfGuid;

                this.guidArray = new Guid[guidCount];
                for(int i = 0; i < guidCount; i++)
                {
                    this.guidArray[i] = new Guid( reader.ReadUInt8Array( sizeOfGuid ) );
                }
            }

            // Read the stream of strings
            if(this.stringPoolStream != null && this.stringPoolStream.size > 0)
            {
                this.stringStreamReader = new ArrayReader( this.metaDataReader, stringPoolStream.offset, stringPoolStream.size );
            }

            // Read the stream of blobs
            if(this.blobPoolStream != null && this.blobPoolStream.size > 0)
            {
                this.blobStreamReader = new ArrayReader( this.metaDataReader, blobPoolStream.offset, blobPoolStream.size );
            }

            // Read the stream of user strings, if there is one
            if(this.userBlobPoolStream != null && this.userBlobPoolStream.size > 0)
            {
                this.userStringStreamReader = new ArrayReader( this.metaDataReader, userBlobPoolStream.offset, userBlobPoolStream.size );
            }

            // Read the resource data, if there is any
            {
                int resourceOffset = peLoader.getResourceOffset();
                int resourceSize   = peLoader.getResourceSize();

                if(resourceOffset > 0 && resourceSize > 0)
                {
                    this.resourceReader = new ArrayReader( this.peLoader.getStream(), resourceOffset, resourceSize );
                }
            }

            // Read the schema
            {
                ArrayReader reader = new ArrayReader( this.metaDataReader, dataStream.offset, dataStream.size );

                // First read the fixed fields (CMiniMdSchemaBase)
                this.ReadSchemaBase( reader );

                // Read the variable fields (this is the compressed part)
                int  count = (int)TokenType.Count;
                long mask  = this.maskValid;

                for(int dst = 0; dst < count; dst++)
                {
                    this.schemaArray[dst] = new TableSchema( (mask & 1) != 0 ? reader.ReadInt32() : 0 );

                    mask >>= 1;
                }

                // Skip the counters we don't understand
                for(int dst = count; dst < 64; dst++)
                {
                    if((mask & 1) != 0)
                    {
                        reader.Seek( 4 );
                    }

                    mask >>= 1;
                }

                // Retrieve any extra data
                if((this.heapBits & HEAPBITS_MASK_EXTRA_DATA) != 0)
                {
                    this.extraData = reader.ReadInt32();
                }

                if((this.majorVersion != METAMODEL_MAJOR_VER_A || (this.minorVersion != METAMODEL_MINOR_VER_A1 &&
                                                                   this.minorVersion != METAMODEL_MINOR_VER_A2) ) &&
                   (this.majorVersion != METAMODEL_MAJOR_VER_B ||  this.minorVersion != METAMODEL_MINOR_VER_B   )  )
                {
                    throw IllegalMetaDataFormatException.Create( "Unknown version {0}.{1}", this.majorVersion, this.minorVersion );
                }

                // Compute the width of all the coded tokens
                this.InitializeRowInfo();

                // Read the data into the tables
                this.InitializeMetaDataTables( reader );
            }
        }

        //--//

        private void ReadSchemaBase( ArrayReader reader )
        {
            this.reserved     = reader.ReadInt32(); // Must be zero
            this.majorVersion = reader.ReadUInt8(); // Version numbers
            this.minorVersion = reader.ReadUInt8();
            this.heapBits     = reader.ReadUInt8(); // Bits for heap sizes
            this.rowId        = reader.ReadUInt8(); // log-base-2 of largest rid
            this.maskValid    = reader.ReadInt64(); // Present table counts
            this.maskSorted   = reader.ReadInt64(); // Sorted tables

            if(this.reserved != 0)
            {
                throw IllegalMetaDataFormatException.Create( "Reserved not zero" );
            }
        }

        private void InitializeRowInfo()
        {
            IndexReader reader1 = new IndexReader( ReadIndexUInt8  );
            IndexReader reader2 = new IndexReader( ReadIndexUInt16 );
            IndexReader reader4 = new IndexReader( ReadIndexUInt32 );

            this.stringIndexSize = (byte)(((this.heapBits & HEAPBITS_MASK_STRINGS) != 0) ? 4 : 2);
            this.guidIndexSize   = (byte)(((this.heapBits & HEAPBITS_MASK_GUID   ) != 0) ? 4 : 2);
            this.blobIndexSize   = (byte)(((this.heapBits & HEAPBITS_MASK_BLOB   ) != 0) ? 4 : 2);

            for(int tableIndex = 0; tableIndex < (int)TokenType.Count; tableIndex++)
            {
                byte[] columnKinds = TableColumnKinds[tableIndex];

                if(columnKinds != null)
                {
                    int            columnCount = columnKinds.Length;
                    ColumnSchema[] columns     = new ColumnSchema[columnCount];

                    this.schemaArray[tableIndex].m_columns = columns;

                    byte columnOffset = 0;  // Running size of record
                    for(int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        byte columnKind = columnKinds[columnIndex];
                        byte columnSize;

                        if(columnKind <= (byte)ColumnKindId.RowIdMax)
                        {
                            if(this.schemaArray[columnKind].m_rows > 0xFFFF)
                            {
                                columnSize = 4;
                            }
                            else
                            {
                                columnSize = 2;
                            }
                        }
                        else if(columnKind <= (byte)ColumnKindId.CodedTokenMax)
                        {
                            byte codeToken = (byte)(columnKind - (byte)ColumnKindId.CodedToken);

                            TokenType[] tokenTypeList = CodeTokenTypeLists[codeToken];

                            int maxCount   = 0;
                            int listLength = tokenTypeList.Length;
                            for(int i = 0; i < listLength; i++)
                            {
                                TokenType tokenType = tokenTypeList[i];

                                // Ignore string tokens
                                if(tokenType != TokenType.Illegal)
                                {
                                    int index = (int)tokenType;

                                    if(this.schemaArray[index].m_rows > maxCount)
                                    {
                                        maxCount = this.schemaArray[index].m_rows;
                                    }
                                }
                            }

                            int maxIndex = maxCount << BitsForCountArray[listLength];
                            if(maxIndex > 0xFFFF)
                            {
                                columnSize = 4;
                            }
                            else
                            {
                                columnSize = 2;
                            }
                        }
                        else
                        {
                            switch((ColumnKindId)columnKind)
                            {
                                case ColumnKindId.Byte:
                                    columnSize = 1;
                                    break;

                                case ColumnKindId.Short:
                                case ColumnKindId.UShort:
                                    columnSize = 2;
                                    break;

                                case ColumnKindId.Long:
                                case ColumnKindId.ULong:
                                    columnSize = 4;
                                    break;

                                case ColumnKindId.String:
                                    columnSize = this.stringIndexSize;
                                    break;

                                case ColumnKindId.Guid:
                                    columnSize = this.guidIndexSize;
                                    break;

                                case ColumnKindId.Blob:
                                    columnSize = this.blobIndexSize;
                                    break;

                                default:
                                    throw IllegalMetaDataFormatException.Create( "Unexpected schema kind: {0}", columnKind );
                            }
                        }

                        // Save away the size and offset

                        ColumnSchema cs = new ColumnSchema();

                        cs.m_size   = columnSize;
                        cs.m_offset = columnOffset;

                        switch(columnSize)
                        {
                            case 1:
                                cs.m_reader = reader1;
                                break;

                            case 2:
                                cs.m_reader = reader2;
                                break;

                            case 4:
                                cs.m_reader = reader4;
                                break;

                            default:
                                throw IllegalMetaDataFormatException.Create( "Unexpected schema size: {0}", columnSize );
                        }

                        columns[columnIndex] = cs;

                        // Align to 2 bytes
                        columnSize   += (byte)(columnSize & 1);
                        columnOffset += columnSize;
                    }

                    this.schemaArray[tableIndex].m_rowSize = columnOffset;
                }
                else
                {
                    if(this.schemaArray[tableIndex].m_rows != 0)
                    {
                        throw IllegalMetaDataFormatException.Create( "Unsupported table: {0}", (TokenType)tableIndex );
                    }
                }
            }
        }

        private void InitializeMetaDataTables( ArrayReader reader )
        {
            int count = (int)TokenType.Count;

            this.metaDataObject.m_tables = new MetaDataObject[count][];

            // Create all the readers for the different tables.
            for(int i = 0; i < count; i++)
            {
                TableSchema ts = this.schemaArray[i];

                ts.m_reader = reader.CreateSubsetAndAdvance( ts.m_rows * ts.m_rowSize );
            }

            // Parse what goes in the tables
            this.parseAssemblyRefTable           ();
            this.parseModuleRefTable             ();
            this.parseTypeRefTable               ();

            this.parseAssemblyTable              ();
            this.parseModuleTable                ();
            this.parseFileTable                  ();

            this.parseManifestResourceTable      ();

            this.parseTypeSpecTable              ();
            this.parseTypeDefTable               ();

            this.parseClassLayoutTable           ();
            this.parseNestedClassTable           ();
            this.parseInterfaceImplTable         ();

            this.parseMemberRefTable             ();

            this.parseFieldTable                 ();

            this.parseMethodTable                ();
            this.parseMethodImplTable            ();
            this.parseImplMapTable               ();

            this.parseParamTable                 ();

            this.parseEventMapTable              ();
            this.parseEventTable                 ();
            this.parsePropertyMapTable           ();
            this.parsePropertyTable              ();

            this.parseMethodSemanticsTable       ();

            this.parseConstantTable              ();

            this.parseFieldMarshalTable          ();
            this.parseFieldRVATable              ();
            this.parseFieldLayoutTable           ();

            this.parseCustomAttributeTable       ();
            this.parseDeclSecurityTable          ();

            this.parseGenericParamTable          ();
            this.parseGenericParamConstraintTable();
            this.parseMethodSpecTable            ();

            this.parseStandAloneSigTable         ();
        }

        //--//

        private void LoadMethods( MetaDataLoaderFlags flags )
        {
            PdbInfo.PdbFile pdbFile = null;

            if((flags & MetaDataLoaderFlags.LoadCode     ) != 0 &&
               (flags & MetaDataLoaderFlags.LoadDebugInfo) != 0  )
            {
                if(symbolHelper != null)
                {
                    pdbFile = symbolHelper.ResolveAssemblySymbols( this.metaDataObject.m_name );
                }
            }

            foreach(MetaDataMethod method in this.metaDataObject.m_methodArray)
            {
                if((flags & MetaDataLoaderFlags.LoadCode) != 0)
                {
                    PdbInfo.PdbFunction pdbFunction = null;

                    if(pdbFile != null)
                    {
                        pdbFunction = pdbFile.FindFunction( (uint)method.Token );
                    }

                    method.loadInstructions( this, pdbFunction );
                }
            }
        }

        //--//

        private static int ReadIndexUInt8( ArrayReader reader )
        {
            uint res = reader.ReadUInt8();

            reader.Seek( 1 );

            return (int)res;
        }

        private static int ReadIndexUInt16( ArrayReader reader )
        {
            uint res = reader.ReadUInt16();

            return (int)res;
        }

        private static int ReadIndexUInt32( ArrayReader reader )
        {
            uint res = reader.ReadUInt32();

            return (int)res;
        }

        //--//

        internal String readIndexAsString( ArrayReader reader )
        {
            int index = (int)(this.stringIndexSize == 2 ? reader.ReadUInt16() : reader.ReadUInt32());

            return this.getString( index );
        }

        internal Guid readIndexAsGuid( ArrayReader reader )
        {
            int index = (int)(this.guidIndexSize == 2 ? reader.ReadUInt16() : reader.ReadUInt32());

            if(index == 0) return new Guid();

            return this.guidArray[index-1];
        }

        internal byte[] readIndexAsBlob( ArrayReader reader )
        {
            int index = (int)(this.blobIndexSize == 2 ? reader.ReadUInt16() : reader.ReadUInt32());

            return this.getBlobBytes( index );
        }

        internal int readIndexAsForBlob( ArrayReader reader )
        {
            return (int)(this.blobIndexSize == 2 ? reader.ReadUInt16() : reader.ReadUInt32());
        }

        //--//

        internal String getString( int stringIndex )
        {
            try
            {
                this.stringStreamReader.Position = stringIndex;

                return this.stringStreamReader.ReadZeroTerminatedUTF8String();
            }
            catch
            {
                throw IllegalMetaDataFormatException.Create( "Cannot find string ", stringIndex );
            }
        }

        //--//

        internal byte[] getBlobBytes( int blobIndex )
        {
            this.blobStreamReader.Position = blobIndex;

            int size = (int)this.blobStreamReader.ReadCompressedUInt32();

            return this.blobStreamReader.ReadUInt8Array( size );
        }

        internal ArrayReader getSignature( int signatureIndex )
        {
            this.blobStreamReader.Position = signatureIndex;

            int size = (int)this.blobStreamReader.ReadCompressedUInt32();

            return this.blobStreamReader.CreateSubset( size );
        }

        internal byte[] getResourceBytes( int offset )
        {
            this.resourceReader.Position = offset;

            int size = this.resourceReader.ReadInt32();

            return this.resourceReader.ReadUInt8Array( size );
        }

        internal MarshalSpec getNativeType( int nativeTypeIndex )
        {
            ArrayReader reader = this.getSignature( nativeTypeIndex );

            return MarshalSpec.Create( reader );
        }

        internal Guid getGuid( int guidIndex )
        {
            try
            {
                return this.guidArray[guidIndex-1];
            }
            catch
            {
                throw IllegalMetaDataFormatException.Create( "Cannot find GUID {0}", guidIndex );
            }
        }

        private MetaDataObject getObjectFromTableSet( int       codedIndex ,
                                                      CodeToken codeToken  )
        {
            if(codedIndex == 0)
            {
                return null;
            }

            int codeCount = CodeTokenTypeLists[(int)codeToken].Length;
            int kind      = codedIndex &  MaskForCountArray[codeCount];
            int index     = codedIndex >> BitsForCountArray[codeCount];

            TokenType tokenType = CodeTokenTypeLists[(int)codeToken][kind];

            return this.getObjectFromToken( MetaData.PackToken( tokenType, index ) );
        }

        //
        // Access Methods
        //

        internal String getUserString( int stringIndex )
        {
            this.userStringStreamReader.Position = stringIndex;

            int size = (int)this.userStringStreamReader.ReadCompressedUInt32();

            return this.userStringStreamReader.ReadUInt16String( size / 2 );
        }

        //--//

        internal IMetaDataTypeDefOrRef getTypeDefOrRef( int codedIndex )
        {
            return (IMetaDataTypeDefOrRef)this.getObjectFromTableSet( codedIndex, CodeToken.TypeDefOrRef );
        }

        internal IMetaDataHasConstant getHasConstant( int codedIndex )
        {
            return (IMetaDataHasConstant)this.getObjectFromTableSet( codedIndex, CodeToken.HasConstant );
        }

        internal IMetaDataHasCustomAttribute getHasCustomAttribute( int codedIndex )
        {
            return (IMetaDataHasCustomAttribute)this.getObjectFromTableSet( codedIndex, CodeToken.HasCustomAttribute );
        }

        internal IMetaDataHasFieldMarshal getHasFieldMarshal( int codedIndex )
        {
            return (IMetaDataHasFieldMarshal)this.getObjectFromTableSet( codedIndex, CodeToken.HasFieldMarshal );
        }

        internal IMetaDataHasDeclSecurity getHasDeclSecurity( int codedIndex )
        {
            return (IMetaDataHasDeclSecurity)this.getObjectFromTableSet( codedIndex, CodeToken.HasDeclSecurity );
        }

        internal IMetaDataMemberRefParent getMemberRefParent( int codedIndex )
        {
            return (IMetaDataMemberRefParent)this.getObjectFromTableSet( codedIndex, CodeToken.MemberRefParent );
        }

        internal IMetaDataHasSemantic getHasSemantic( int codedIndex )
        {
            return (IMetaDataHasSemantic)this.getObjectFromTableSet( codedIndex, CodeToken.HasSemantic );
        }

        internal IMetaDataMethodDefOrRef getMethodDefOrRef( int codedIndex )
        {
            return (IMetaDataMethodDefOrRef)this.getObjectFromTableSet( codedIndex, CodeToken.MethodDefOrRef );
        }

        internal IMetaDataMemberForwarded getMemberForwarded( int codedIndex )
        {
            return (IMetaDataMemberForwarded)this.getObjectFromTableSet( codedIndex, CodeToken.MemberForwarded );
        }

        internal IMetaDataImplementation getImplementation( int codedIndex )
        {
            return (IMetaDataImplementation)this.getObjectFromTableSet( codedIndex, CodeToken.Implementation );
        }

        internal IMetaDataCustomAttributeType getCustomAttributeType( int codedIndex )
        {
            return (IMetaDataCustomAttributeType)this.getObjectFromTableSet( codedIndex, CodeToken.CustomAttributeType );
        }

        internal IMetaDataResolutionScope getResolutionScope( int resolutionIndex )
        {
            return (IMetaDataResolutionScope)this.getObjectFromTableSet( resolutionIndex, CodeToken.ResolutionScope );
        }

        internal IMetaDataTypeOrMethodDef getTypeOrMethodDef( int resolutionIndex )
        {
            return (IMetaDataTypeOrMethodDef)this.getObjectFromTableSet( resolutionIndex, CodeToken.TypeOrMethodDef );
        }

        //--//

        internal MetaDataObject getObjectFromToken( int token )
        {
            TokenType type  = MetaData.UnpackTokenAsType ( token );
            int       index = MetaData.UnpackTokenAsIndex( token );

            if(index == 0) return null;

            MetaDataObject[] array = this.metaDataObject.m_tables[(int)type];
            if(array == null)
            {
                throw new Exception( "Missing table for " + type );
            }

            MetaDataObject res = array[index - 1];
            if(res == null)
            {
                if(type == TokenType.TypeSpec)
                {
                    res = this.parseTypeSpecEntry( index );
                }

                if(res == null)
                {
                    throw new Exception( "Missing entry " + index + " in table for " + type );
                }
            }

            return res;
        }

        //--//

        internal void SetFieldIndex( MetaDataTypeDefinition type       ,
                                     int                    fieldIndex )
        {
            this.lookup_fieldIndexToType.Add( type, fieldIndex );
        }

        internal void SetMethodIndex( MetaDataTypeDefinition type        ,
                                      int                    methodIndex )
        {
            this.lookup_methodIndexToType.Add( type, methodIndex );
        }

        internal void SetParamIndex( MetaDataMethod method     ,
                                     int            paramIndex )
        {
            this.lookup_paramIndexToMethod.Add( method, paramIndex );
        }

        internal MetaDataTypeDefinition GetTypeFromFieldIndex( int fieldIndex )
        {
            return this.lookup_fieldIndexToType.Get( fieldIndex );
        }

        internal MetaDataTypeDefinition GetTypeFromMethodIndex( int methodIndex )
        {
            return this.lookup_methodIndexToType.Get( methodIndex );
        }

        internal MetaDataTypeDefinition GetTypeFromEventIndex( int eventIndex )
        {
            return this.lookup_eventIndexToType.Get( eventIndex );
        }

        internal MetaDataTypeDefinition GetTypeFromPropertyIndex( int propertyIndex )
        {
            return this.lookup_propertyIndexToType.Get( propertyIndex );
        }

        internal MetaDataMethod GetMethodFromParamIndex( int paramIndex )
        {
            return this.lookup_paramIndexToMethod.Get( paramIndex );
        }

        //--//

        // Methods for initializing the tables and table objects

        private TableSchema InitializeTableNoAlloc<T>(     TokenType tbl ,
                                                       ref T[]       res ) where T : MetaDataObject
        {
            TableSchema ts = this.schemaArray[(int)tbl];

            int count = ts.m_rows;
            T[] array = new T[count];

            res = array;

            this.metaDataObject.m_tables[(int)tbl] = array;

            return ts;
        }

        private TableSchema InitializeTable<T>(     TokenType                     tbl     ,
                                                ref T[]                           res     ,
                                                    MetaDataObject.CreateInstance creator ) where T : MetaDataObject
        {
            TableSchema ts = this.schemaArray[(int)tbl];

            int count = ts.m_rows;
            T[] array = new T[count];

            res = array;

            this.metaDataObject.m_tables[(int)tbl] = array;

            for(int i = 0; i < count; i++)
            {
                array[i] = (T)creator( i+1 );
            }

            return ts;
        }

        private void InitializeTableAndLoad<T>(     TokenType                     tbl     ,
                                                ref T[]                           res     ,
                                                    MetaDataObject.CreateInstance creator ) where T : MetaDataObject
        {
            TableSchema ts = InitializeTable( tbl, ref res, creator );

            ArrayReader reader = ts.m_reader; reader.Rewind();

            foreach(T o in res)
            {
                o.Parse( this, ts, reader );
            }
        }

        private MetaDataObject parseTypeSpecEntry( int index )
        {
            TableSchema ts = this.schemaArray[(int)TokenType.TypeSpec];

            ArrayReader reader = ts.m_reader; reader.Rewind();

            reader.Seek( ts.m_rowSize * (index-1) );

            MetaDataTypeSpec item = (MetaDataTypeSpec)MetaDataTypeSpec.GetCreator()( index );

            this.metaDataObject.m_typeSpecArray[index-1] = item;

            item.Parse( this, ts, reader );

            return item;
        }

        //--//

        private T Fetch<T>( TokenType tbl   ,
                            int       index ,
                            T[]       array ) where T : MetaDataObject
        {
            if(index == 0) return null;

            if(array == null)
            {
                throw IllegalMetaDataFormatException.Create( "Forward reference to {0} {1}", tbl, index );
            }

            T item = array[index-1];

            if(item == null)
            {
                if(tbl == TokenType.TypeSpec)
                {
                    item = (T)this.parseTypeSpecEntry( index );
                }

                if(item == null)
                {
                    throw IllegalMetaDataFormatException.Create( "Forward reference to {0} {1}", tbl, index );
                }
            }

            return item;
        }

        internal MetaDataTypeDefinition getTypeDef( int typeDefIndex )
        {
            return Fetch( TokenType.TypeDef, typeDefIndex, this.metaDataObject.m_typeDefArray );
        }

        internal MetaDataField getField( int fieldIndex )
        {
            return Fetch( TokenType.Field, fieldIndex, this.metaDataObject.m_fieldArray );
        }

        internal MetaDataMethod getMethod( int methodIndex )
        {
            return Fetch( TokenType.Method, methodIndex, this.metaDataObject.m_methodArray );
        }

        internal MetaDataModuleRef getModuleRef( int moduleRefIndex )
        {
            return Fetch( TokenType.ModuleRef, moduleRefIndex , this.metaDataObject.m_moduleRefArray );
        }

        internal MetaDataGenericParam getGenericParam( int genericParamIndex )
        {
            return Fetch( TokenType.GenericParam, genericParamIndex, this.metaDataObject.m_genericParamArray );
        }

        //--//

        // 0x00
        private void parseModuleTable()
        {
            InitializeTableAndLoad( TokenType.Module, ref this.metaDataObject.m_moduleArray, MetaDataModule.GetCreator() );
        }

        // 0x01
        private void parseTypeRefTable()
        {
            InitializeTableAndLoad( TokenType.TypeRef, ref this.metaDataObject.m_typeRefArray, MetaDataTypeReference.GetCreator() );
        }

        // 0x02
        private void parseTypeDefTable()
        {
            int fieldCount  = this.schemaArray[(int)TokenType.Field ].m_rows;
            int methodCount = this.schemaArray[(int)TokenType.Method].m_rows;

            this.lookup_fieldIndexToType  = new ReverseListLookup<MetaDataTypeDefinition>( fieldCount  );
            this.lookup_methodIndexToType = new ReverseListLookup<MetaDataTypeDefinition>( methodCount );

            InitializeTableAndLoad( TokenType.TypeDef, ref this.metaDataObject.m_typeDefArray, MetaDataTypeDefinition.GetCreator() );

            this.lookup_fieldIndexToType .Complete();
            this.lookup_methodIndexToType.Complete();
        }

        // 0x04
        private void parseFieldTable()
        {
            InitializeTableAndLoad( TokenType.Field, ref this.metaDataObject.m_fieldArray, MetaDataField.GetCreator() );
        }

        // 0x06
        private void parseMethodTable()
        {
            int paramCount = this.schemaArray[(int)TokenType.Param].m_rows;

            this.lookup_paramIndexToMethod = new ReverseListLookup<MetaDataMethod>( paramCount );

            InitializeTableAndLoad( TokenType.Method, ref this.metaDataObject.m_methodArray, MetaDataMethod.GetCreator() );

            this.lookup_paramIndexToMethod.Complete();
        }

        // 0x08
        private void parseParamTable()
        {
            InitializeTableAndLoad( TokenType.Param, ref this.metaDataObject.m_paramArray, MetaDataParam.GetCreator() );
        }

        // 0x09
        private void parseInterfaceImplTable()
        {
            TableSchema ts = this.schemaArray[(int)TokenType.InterfaceImpl];

            IndexReader classReader     = ts.m_columns[0].m_reader;
            IndexReader interfaceReader = ts.m_columns[1].m_reader;

            for(int i = 0; i < ts.m_rows; i++)
            {
                int classIndex     = classReader    ( ts.m_reader );
                int interfaceIndex = interfaceReader( ts.m_reader );

                MetaDataTypeDefinition classObject     = this.getTypeDef     ( classIndex     );
                IMetaDataTypeDefOrRef  interfaceObject = this.getTypeDefOrRef( interfaceIndex );

                classObject.AddInterface( interfaceObject );
            }
        }

        // 0x0A
        private void parseMemberRefTable()
        {
            InitializeTableAndLoad( TokenType.MemberRef, ref this.metaDataObject.m_memberRefArray, MetaDataMemberRef.GetCreator() );
        }

        // 0x0B
        private void parseConstantTable()
        {
            InitializeTableAndLoad( TokenType.Constant, ref this.metaDataObject.m_constantArray, MetaDataConstant.GetCreator() );
        }

        // 0x0C
        private void parseCustomAttributeTable()
        {
            InitializeTableAndLoad( TokenType.CustomAttribute, ref this.metaDataObject.m_customAttributeArray, MetaDataCustomAttribute.GetCreator() );
        }

        // 0x0D
        private void parseFieldMarshalTable()
        {
            InitializeTableAndLoad( TokenType.FieldMarshal, ref this.metaDataObject.m_fieldMarshalArray, MetaDataFieldMarshal.GetCreator() );
        }

        // 0x0E
        private void parseDeclSecurityTable()
        {
            InitializeTableAndLoad( TokenType.DeclSecurity, ref this.metaDataObject.m_declSecurityArray, MetaDataDeclSecurity.GetCreator() );
        }

        // 0x0F
        private void parseClassLayoutTable()
        {
            InitializeTableAndLoad( TokenType.ClassLayout, ref this.metaDataObject.m_classLayoutArray, MetaDataClassLayout.GetCreator() );
        }

        // 0x10
        private void parseFieldLayoutTable()
        {
            InitializeTableAndLoad( TokenType.FieldLayout, ref this.metaDataObject.m_fieldLayoutArray, MetaDataFieldLayout.GetCreator() );
        }

        // 0x11
        private void parseStandAloneSigTable()
        {
            InitializeTableAndLoad( TokenType.StandAloneSig, ref this.metaDataObject.m_standAloneSigArray, MetaDataStandAloneSig.GetCreator() );
        }

        // 0x12
        private void parseEventMapTable()
        {
            int eventCount = this.schemaArray[(int)TokenType.Event].m_rows;

            this.lookup_eventIndexToType = new ReverseListLookup<MetaDataTypeDefinition>( eventCount );

            TableSchema ts = this.schemaArray[(int)TokenType.EventMap];

            IndexReader parentReader    = ts.m_columns[0].m_reader;
            IndexReader eventListReader = ts.m_columns[1].m_reader;
            ArrayReader reader          = ts.m_reader;

            for(int i = 0; i < ts.m_rows; i++)
            {
                int parentIndex    = parentReader   ( reader );
                int eventListIndex = eventListReader( reader );

                this.lookup_eventIndexToType.Add( this.getTypeDef( parentIndex ), eventListIndex );
            }

            this.lookup_eventIndexToType.Complete();
        }

        // 0x14
        private void parseEventTable()
        {
            InitializeTableAndLoad( TokenType.Event, ref this.metaDataObject.m_eventArray, MetaDataEvent.GetCreator() );
        }

        // 0x15
        private void parsePropertyMapTable()
        {
            int propertyCount = this.schemaArray[(int)TokenType.Property].m_rows;

            this.lookup_propertyIndexToType = new ReverseListLookup<MetaDataTypeDefinition>( propertyCount );

            TableSchema ts = this.schemaArray[(int)TokenType.PropertyMap];

            IndexReader parentReader       = ts.m_columns[0].m_reader;
            IndexReader propertyListReader = ts.m_columns[1].m_reader;
            ArrayReader reader             = ts.m_reader;

            for(int i = 0; i < ts.m_rows; i++)
            {
                int parentIndex       = parentReader   ( reader );
                int propertyListIndex = propertyListReader( reader );

                this.lookup_propertyIndexToType.Add( this.getTypeDef( parentIndex ), propertyListIndex );
            }

            this.lookup_propertyIndexToType.Complete();
        }

        // 0x17
        private void parsePropertyTable()
        {
            InitializeTableAndLoad( TokenType.Property, ref this.metaDataObject.m_propertyArray, MetaDataProperty.GetCreator() );
        }

        // 0x18
        private void parseMethodSemanticsTable()
        {
            InitializeTableAndLoad( TokenType.MethodSemantics, ref this.metaDataObject.m_methodSemanticsArray, MetaDataMethodSemantics.GetCreator() );
        }

        // 0x19
        private void parseMethodImplTable()
        {
            InitializeTableAndLoad( TokenType.MethodImpl, ref this.metaDataObject.m_methodImplArray, MetaDataMethodImpl.GetCreator() );
        }

        // 0x1A
        private void parseModuleRefTable()
        {
            InitializeTableAndLoad( TokenType.ModuleRef, ref this.metaDataObject.m_moduleRefArray, MetaDataModuleRef.GetCreator() );
        }

        // 0x1B
        private void parseTypeSpecTable()
        {
            //
            // TypeSpec is the exception to the rule.
            // Since TypeSpec can point to TypeDef and viceversa, we need to break the loop.
            // The entries from the TypeSpec table are parsed on demand, instead of in a batch.
            //
            InitializeTableNoAlloc( TokenType.TypeSpec, ref this.metaDataObject.m_typeSpecArray );
        }

        // 0x1C
        private void parseImplMapTable()
        {
            InitializeTableAndLoad( TokenType.ImplMap, ref this.metaDataObject.m_implMapArray, MetaDataImplMap.GetCreator() );
        }

        // 0x1D
        private void parseFieldRVATable()
        {
            InitializeTableAndLoad( TokenType.FieldRVA, ref this.metaDataObject.m_fieldRVAArray, MetaDataFieldRVA.GetCreator() );
        }

        // 0x20
        private void parseAssemblyTable()
        {
            InitializeTableAndLoad( TokenType.Assembly, ref this.metaDataObject.m_assemblyArray, MetaDataAssembly.GetCreator() );

            if(this.metaDataObject.m_assemblyArray.Length != 1)
            {
                throw IllegalMetaDataFormatException.Create( "More than one record in the Assembly table" );
            }
        }

        // 0x23
        private void parseAssemblyRefTable()
        {
            InitializeTableAndLoad( TokenType.AssemblyRef, ref this.metaDataObject.m_assemblyRefArray, MetaDataAssemblyRef.GetCreator() );
        }

        // 0x26
        private void parseFileTable()
        {
            InitializeTableAndLoad( TokenType.File, ref this.metaDataObject.m_fileArray, MetaDataFile.GetCreator() );
        }

        // 0x28
        private void parseManifestResourceTable()
        {
            InitializeTableAndLoad( TokenType.ManifestResource, ref this.metaDataObject.m_manifestResourceArray, MetaDataManifestResource.GetCreator() );
        }

        // 0x29
        private void parseNestedClassTable()
        {
            TableSchema ts = this.schemaArray[(int)TokenType.NestedClass];

            IndexReader nestedClassReader    = ts.m_columns[0].m_reader;
            IndexReader enclosingClassReader = ts.m_columns[1].m_reader;
            ArrayReader reader               = ts.m_reader;

            for(int i = 0; i < ts.m_rows; i++)
            {
                int nestedClassIndex    = nestedClassReader   ( reader );
                int enclosingClassIndex = enclosingClassReader( reader );

                MetaDataTypeDefinition nestedClass    = this.getTypeDef( nestedClassIndex    );
                MetaDataTypeDefinition enclosingClass = this.getTypeDef( enclosingClassIndex );

                enclosingClass.AddNestedClass( nestedClass );
            }
        }

        // 0x2A
        private void parseGenericParamTable()
        {
            InitializeTableAndLoad( TokenType.GenericParam, ref this.metaDataObject.m_genericParamArray, MetaDataGenericParam.GetCreator() );
        }

        // 0x2B
        private void parseMethodSpecTable()
        {
            InitializeTableAndLoad( TokenType.MethodSpec, ref this.metaDataObject.m_methodSpecArray, MetaDataMethodSpec.GetCreator() );
        }

        // 0x2C
        private void parseGenericParamConstraintTable()
        {
            TableSchema ts = this.schemaArray[(int)TokenType.GenericParamConstraint];

            IndexReader ownerReader      = ts.m_columns[0].m_reader;
            IndexReader constraintReader = ts.m_columns[1].m_reader;
            ArrayReader reader           = ts.m_reader;

            for(int i = 0; i < ts.m_rows; i++)
            {
                int ownerIndex      = ownerReader     ( reader );
                int constraintIndex = constraintReader( reader );

                MetaDataGenericParam  owner      = this.getGenericParam( ownerIndex      );
                IMetaDataTypeDefOrRef constraint = this.getTypeDefOrRef( constraintIndex );

                owner.AddGenericParamConstraint( constraint );
            }
        }

        //--//

        //
        // Access Methods
        //

        internal MetaData MetaData
        {
            get
            {
                return this.metaDataObject;
            }
        }

        //--//

        internal ArrayReader ImageReaderAt( int offset )
        {
            return new ArrayReader( this.imageReader, offset );
        }

        internal ArrayReader ImageReaderAtVirtualAddress( int va )
        {
            int address = this.peLoader.VaToOffsetSafe( va );

            if(address == -1) return null;

            return ImageReaderAt( address );
        }

        // Nested Classes

        private class StorageSignature
        {
            private const int STORAGE_MAGIC_SIG  = 0x424A5342;
            private const int FILE_MAJOR_VERSION = 1;
            private const int FILE_MINOR_VERSION = 1;

            //
            // State
            //

            internal readonly int    signature;
            internal readonly short  majorVersion;
            internal readonly short  minorVersion;
            internal readonly int    extraData;
            internal readonly String versionString;

            //
            // Constructor Methods
            //

            internal StorageSignature( ArrayReader reader )
            {
                this.signature     = reader.ReadInt32();
                this.majorVersion  = reader.ReadInt16();
                this.minorVersion  = reader.ReadInt16();
                this.extraData     = reader.ReadInt32();
                int length         = reader.ReadInt32();
                this.versionString = reader.ReadUInt8String( length );

                if(this.signature != StorageSignature.STORAGE_MAGIC_SIG)
                {
                    throw IllegalMetaDataFormatException.Create( "Don't know signature 0x{0:X8}", this.signature );
                }

                if(this.majorVersion != StorageSignature.FILE_MAJOR_VERSION ||
                   this.minorVersion != StorageSignature.FILE_MINOR_VERSION)
                {
                    throw IllegalMetaDataFormatException.Create( "Unknown version" );
                }
            }
        }

        private class StorageHeader
        {
            internal const Byte STGHDR_EXTRADATA = 0x01;

            //
            // State
            //

            internal Byte   flags;
            internal Byte   pad;
            internal short  streamCount;
            internal byte[] extra;

            //
            // Constructor Methods
            //

            internal StorageHeader( ArrayReader reader )
            {
                this.flags       = reader.ReadUInt8();
                this.pad         = reader.ReadUInt8();
                this.streamCount = reader.ReadInt16();

                if((this.flags & STGHDR_EXTRADATA) != 0)
                {
                    int count = reader.ReadInt32();

                    this.extra = reader.ReadUInt8Array( count );
                }
            }
        }

        private class StorageStream
        {
            internal const String COMPRESSED_MODEL = "#~";
            internal const String ENC_MODEL        = "#-";
            internal const String SCHEMA           = "#Schema";
            internal const String STRING_POOL      = "#Strings";
            internal const String BLOB_POOL        = "#Blob";
            internal const String USER_BLOB_POOL   = "#US";
            internal const String VARIANT_POOL     = "#Variants";
            internal const String GUID_POOL        = "#GUID";

            //
            // State
            //

            internal int    offset;
            internal int    size;
            internal String name;

            //
            // Constructor Methods
            //

            internal StorageStream( ArrayReader reader )
            {
                this.offset = reader.ReadInt32();
                this.size   = reader.ReadInt32();

                int startPosition = reader.Position;

                this.name = reader.ReadZeroTerminatedUInt8String();

                //
                // Align to word boundary.
                //
                while((reader.Position - startPosition) % 4 != 0)
                {
                    reader.ReadUInt8();
                }
            }

            //
            // Output Methods
            //

            public override String ToString()
            {
                return "StorageStream(" + this.name + "," + this.offset + "," + this.size + ")";
            }
        }
    }
}

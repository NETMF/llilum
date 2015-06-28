//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

// Holder of MetaData information

/**********************************************************************
 *
 * The Microsoft.Zelig.MetaData.Importer.MetaData* classes (except MetaData)
 * represent entries in the big table of meta data stored
 * away in URT binaries.  Each class represents data from one kind of
 * entry in the table.  Some of the information in other classes may
 * have been incorporated into the representatives (e.g., the
 * MetaDataMethod objects have lists of fields and methods).
 *
 * The MetaData objects contain the arrays of MetaData* objects
 * representing the metadata from a single URT binary.
 *
 * The Parser class is used to load meta data information from
 * binaries.  A consumer of meta data information need not inspect
 * Parser.cs.
 *
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    [Flags]
    public enum MetaDataLoaderFlags
    {
        Default                    = 0x00,
        LoadCode                   = 0x01,
        LoadSectionsFromExecutable = 0x02,
        LoadDebugInfo              = 0x04,
        Messages                   = 0x08,
    }

    public sealed class MetaData : IMetaDataObject
    {
        internal class ObjectComparer : IEqualityComparer< IMetaDataNormalize >
        {
            public bool Equals( IMetaDataNormalize x ,
                                IMetaDataNormalize y )
            {
                return Object.ReferenceEquals( x, y );
            }

            public int GetHashCode( IMetaDataNormalize obj )
            {
                return obj != null ? obj.GetHashCode() : 0;
            }
        }

        internal class SignatureComparer : IEqualityComparer< IMetaDataNormalizeSignature >
        {
            public bool Equals( IMetaDataNormalizeSignature x ,
                                IMetaDataNormalizeSignature y )
            {
                return Object.ReferenceEquals( x, y );
            }

            public int GetHashCode( IMetaDataNormalizeSignature obj )
            {
                return obj != null ? obj.GetHashCode() : 0;
            }
        }

        //
        // State
        //

        internal String                            m_name;
        internal MetaDataMethod                    m_entryPoint;
        internal long                              m_imageBase;

        internal MetaDataObject[][]                m_tables;
        internal MetaDataModule                []  m_moduleArray;
        internal MetaDataTypeReference         []  m_typeRefArray;
        internal MetaDataTypeDefinition        []  m_typeDefArray;
        internal MetaDataField                 []  m_fieldArray;
        internal MetaDataMethod                []  m_methodArray;
        internal MetaDataParam                 []  m_paramArray;
        internal MetaDataMemberRef             []  m_memberRefArray;
        internal MetaDataConstant              []  m_constantArray;
        internal MetaDataCustomAttribute       []  m_customAttributeArray;
        internal MetaDataFieldMarshal          []  m_fieldMarshalArray;
        internal MetaDataDeclSecurity          []  m_declSecurityArray;
        internal MetaDataClassLayout           []  m_classLayoutArray;
        internal MetaDataFieldLayout           []  m_fieldLayoutArray;
        internal MetaDataStandAloneSig         []  m_standAloneSigArray;
        internal MetaDataEvent                 []  m_eventArray;
        internal MetaDataProperty              []  m_propertyArray;
        internal MetaDataMethodSemantics       []  m_methodSemanticsArray;
        internal MetaDataMethodImpl            []  m_methodImplArray;
        internal MetaDataModuleRef             []  m_moduleRefArray;
        internal MetaDataTypeSpec              []  m_typeSpecArray;
        internal MetaDataImplMap               []  m_implMapArray;
        internal MetaDataFieldRVA              []  m_fieldRVAArray;
        internal MetaDataAssembly              []  m_assemblyArray;
        internal MetaDataAssemblyRef           []  m_assemblyRefArray;
        internal MetaDataFile                  []  m_fileArray;
        internal MetaDataManifestResource      []  m_manifestResourceArray;
        internal MetaDataGenericParam          []  m_genericParamArray;
        internal MetaDataMethodSpec            []  m_methodSpecArray;

        //
        // Constructor Methods
        //

        public static MetaData loadMetaData( String                name         ,
                                             ISymbolResolverHelper symbolHelper ,
                                             PELoader              peLoader     )
        {
            return loadMetaData( name, symbolHelper, peLoader, MetaDataLoaderFlags.Default );
        }

        public static MetaData loadMetaData( String                name         ,
                                             ISymbolResolverHelper symbolHelper ,
                                             PELoader              peLoader     ,
                                             MetaDataLoaderFlags   flags        )
        {
            Parser parser = new Parser( name, symbolHelper, peLoader, flags );

            return parser.MetaData;
        }

        internal MetaData( String name )
        {
            m_name = name;
        }

        //--//

        internal void AllocateNormalizedObject( MetaDataResolver.AssemblyPair pair    ,
                                                MetaDataNormalizationContext  context )
        {
            //
            // Already processed this phase?
            //
            if(pair.Phase >= context.Phase)
            {
                return;
            }

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfAssemblyReferences:
                    {
                        Normalized.MetaDataAssembly asmlNew;

                        context.GetNormalizedObject( this.Assembly, out asmlNew, MetaDataNormalizationMode.Allocate );

                        pair.Normalized = asmlNew;

                        foreach(MetaDataAssemblyRef asmlRef in m_assemblyRefArray)
                        {
                            Normalized.MetaDataAssembly asmlRefNew;

                            context.GetNormalizedObject( asmlRef, out asmlRefNew, MetaDataNormalizationMode.Allocate );
                        }
                    }
                    pair.Phase = MetaDataNormalizationPhase.ResolutionOfAssemblyReferences;
                    return;
            }

            throw context.InvalidPhase( this );
        }

        internal bool ExecuteNormalizationPhase( MetaDataResolver.AssemblyPair pair    ,
                                                 MetaDataNormalizationContext  context )
        {
            //
            // Already processed this phase?
            //
            if(pair.Phase >= context.Phase)
            {
                return true;
            }

            Normalized.MetaDataAssembly  asml = pair.Normalized;

            context = context.Push( asml );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ConversionOfResources:
                    {
                        foreach(MetaDataManifestResource res in m_manifestResourceArray)
                        {
                            Normalized.MetaDataManifestResource resNew;

                            context.GetNormalizedObject( res, out resNew, MetaDataNormalizationMode.Allocate );
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.CreationOfTypeDefinitions:
                    {
                        foreach(MetaDataTypeDefinition td in m_typeDefArray)
                        {
                            Normalized.MetaDataTypeDefinitionBase tdNew;

                            context.GetNormalizedObject( td, out tdNew, MetaDataNormalizationMode.Allocate );
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.LinkingOfNestedClasses         :
                case MetaDataNormalizationPhase.DiscoveryOfBuiltInTypes        :
                case MetaDataNormalizationPhase.CreationOfTypeHierarchy        :
                case MetaDataNormalizationPhase.CompletionOfTypeNormalization  :
                case MetaDataNormalizationPhase.CreationOfFieldDefinitions     :
                case MetaDataNormalizationPhase.CreationOfMethodDefinitions    :
                case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                    {
                        foreach(MetaDataTypeDefinition td in m_typeDefArray)
                        {
                            context.ProcessPhase( td );
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.CreationOfMethodImplDefinitions:
                    {
                        foreach(MetaDataMethodImpl mi in m_methodImplArray)
                        {
                            Normalized.MetaDataMethodImpl miNew;

                            context.GetNormalizedObject( mi, out miNew, MetaDataNormalizationMode.Allocate );
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.CompletionOfMethodImplNormalization:
                    {
                        foreach(MetaDataMethodImpl mi in m_methodImplArray)
                        {
                            context.ProcessPhase( mi );
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.CreationOfSpecialArrayMethods:
                    {
                        Normalized.MetaDataTypeDefinitionArray[] tdToProcess = asml.PendingArrays.ToArray();
                        bool                                     fDone       = true;

                        asml.PendingArrays.Clear();

                        foreach(Normalized.MetaDataTypeDefinitionArray array in tdToProcess)
                        {
                            if(array.m_methods == null)
                            {
                                fDone = false;

                                asml.OtherTypes.Add( array );

                                if(array is Normalized.MetaDataTypeDefinitionArrayMulti)
                                {
                                    Normalized.MetaDataTypeDefinitionArrayMulti arrayMulti = (Normalized.MetaDataTypeDefinitionArrayMulti)array;

                                    context.CreateSpecialMethods( arrayMulti );
                                }

                                context.ImplementSpecialInterfaces( array );
                            }
                        }

                        if(fDone == false)
                        {
                            return false;
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.ResolutionOfTypeReferences:
                    {
                        foreach(MetaDataTypeReference trRef in m_typeRefArray)
                        {
                            Normalized.MetaDataTypeDefinitionBase tr;

                            context.GetNormalizedObject( trRef, out tr, MetaDataNormalizationMode.Allocate );
                        }
                    }
                    break;

                case MetaDataNormalizationPhase.ResolutionOfEntryPoint:
                    {
                        context.GetNormalizedObject( m_entryPoint, out asml.m_entryPoint, MetaDataNormalizationMode.Default );
                    }
                    break;

                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.ProcessPhase( this.Assembly );

                        foreach(MetaDataTypeDefinition td in m_typeDefArray)
                        {
                            context.ProcessPhase( td );
                        }
                    }
                    break;

                default:
                    throw context.InvalidPhase( this );
            }

            pair.Phase = context.Phase;

            return true;
        }

        //--//

        public static int PackToken( TokenType tbl   ,
                                     int       index )
        {
            return (int)tbl << 24 | (index & 0x00FFFFFF);
        }

        public static TokenType UnpackTokenAsType( int token )
        {
            return (TokenType)(token >> 24);
        }

        public static int UnpackTokenAsIndex( int token )
        {
            return token & 0x00FFFFFF;
        }

        //--//

        //
        // Access Methods
        //

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public MetaDataMethod EntryPoint
        {
            get
            {
                return m_entryPoint;
            }
        }

        public long ImageBase
        {
            get
            {
                return m_imageBase;
            }
        }

        public MetaDataModule[] Modules
        {
            get
            {
                return m_moduleArray;
            }
        }

        public MetaDataTypeReference[] TypeRefs
        {
            get
            {
                return m_typeRefArray;
            }
        }

        public MetaDataTypeDefinition[] TypeDefs
        {
            get
            {
                return m_typeDefArray;
            }
        }

        public MetaDataField[] Fields
        {
            get
            {
                return m_fieldArray;
            }
        }

        public MetaDataMethod[] Methods
        {
            get
            {
                return m_methodArray;
            }
        }

        public MetaDataParam[] Parameters
        {
            get
            {
                return m_paramArray;
            }
        }

        public MetaDataMemberRef[] MemberRefs
        {
            get
            {
                return m_memberRefArray;
            }
        }

        public MetaDataConstant[] Constants
        {
            get
            {
                return m_constantArray;
            }
        }

        public MetaDataCustomAttribute[] CustomAttributes
        {
            get
            {
                return m_customAttributeArray;
            }
        }

        public MetaDataFieldMarshal[] FieldMarshals
        {
            get
            {
                return m_fieldMarshalArray;
            }
        }

        public MetaDataDeclSecurity[] DeclSecurities
        {
            get
            {
                return m_declSecurityArray;
            }
        }

        public MetaDataClassLayout[] ClassLayouts
        {
            get
            {
                return m_classLayoutArray;
            }
        }

        public MetaDataFieldLayout[] FieldLayouts
        {
            get
            {
                return m_fieldLayoutArray;
            }
        }

        public MetaDataStandAloneSig[] StandAloneSigs
        {
            get
            {
                return m_standAloneSigArray;
            }
        }

        public MetaDataEvent[] Events
        {
            get
            {
                return m_eventArray;
            }
        }

        public MetaDataProperty[] Properties
        {
            get
            {
                return m_propertyArray;
            }
        }

        public MetaDataMethodSemantics[] MethodSemanticss
        {
            get
            {
                return m_methodSemanticsArray;
            }
        }

        public MetaDataMethodImpl[] MethodImpls
        {
            get
            {
                return m_methodImplArray;
            }
        }

        public MetaDataModuleRef[] ModuleRefs
        {
            get
            {
                return m_moduleRefArray;
            }
        }

        public MetaDataTypeSpec[] TypeSpecs
        {
            get
            {
                return m_typeSpecArray;
            }
        }

        public MetaDataImplMap[] ImplMaps
        {
            get
            {
                return m_implMapArray;
            }
        }

        public MetaDataFieldRVA[] FieldRVAs
        {
            get
            {
                return m_fieldRVAArray;
            }
        }

        public MetaDataAssembly Assembly
        {
            get
            {
                return m_assemblyArray[0];
            }
        }

        public MetaDataAssemblyRef[] AssemblyRefs
        {
            get
            {
                return m_assemblyRefArray;
            }
        }

        public MetaDataFile[] files
        {
            get
            {
                return m_fileArray;
            }
        }

        public MetaDataManifestResource[] ManifestResources
        {
            get
            {
                return m_manifestResourceArray;
            }
        }

        public MetaDataGenericParam[] GenericParams
        {
            get
            {
                return m_genericParamArray;
            }
        }

        public MetaDataMethodSpec[] MethodSpecs
        {
            get
            {
                return m_methodSpecArray;
            }
        }

        //--//

        public override String ToString()
        {
            return "MetaData(" + m_name + ")";
        }
    }
}

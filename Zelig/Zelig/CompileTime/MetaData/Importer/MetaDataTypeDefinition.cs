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
    public sealed class MetaDataTypeDefinition : MetaDataObject,
        IMetaDataTypeDefOrRef,
        IMetaDataHasDeclSecurity,
        IMetaDataTypeOrMethodDef,
        IMetaDataNormalize
    {
        //
        // State
        //

        private TypeAttributes               m_flags;
        private IMetaDataTypeDefOrRef        m_extends;

        private string                       m_name;
        private string                       m_nameSpace;

        private MetaDataTypeDefinition       m_enclosingClass;
        private List<MetaDataTypeDefinition> m_nestedClasses;

        private List<MetaDataGenericParam>   m_genericParamList;

        private List<IMetaDataTypeDefOrRef>  m_interfaces;
        private List<MetaDataField>          m_fields;
        private List<MetaDataMethod>         m_methods;

        private List<MetaDataEvent>          m_events;
        private List<MetaDataProperty>       m_properties;
        private MetaDataClassLayout          m_classLayout;

        //
        // Constructor Methods
        //

        private MetaDataTypeDefinition( int index ) : base( TokenType.TypeDef, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataTypeDefinition( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader extendsReader = ts.m_columns[3].m_reader;
            Parser.IndexReader fieldReader   = ts.m_columns[4].m_reader;
            Parser.IndexReader methodReader  = ts.m_columns[5].m_reader;
            int                extendsIndex;
            int                fieldIndex;
            int                methodIndex;

            m_flags      = (TypeAttributes)          reader.ReadInt32();
            m_name       = parser.readIndexAsString( reader );
            m_nameSpace  = parser.readIndexAsString( reader );
            extendsIndex =        extendsReader    ( reader );
            fieldIndex   =        fieldReader      ( reader );
            methodIndex  =        methodReader     ( reader );

            m_extends = parser.getTypeDefOrRef( extendsIndex );

            parser.SetFieldIndex ( this, fieldIndex  );
            parser.SetMethodIndex( this, methodIndex );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfTypeDefinitions:
                    {
                        Normalized.MetaDataTypeDefinitionBase tdNew;
                        Normalized.MetaDataAssembly           owner = (Normalized.MetaDataAssembly)context.Value;

                        if(m_genericParamList != null && m_genericParamList.Count > 0)
                        {
                            tdNew = new Normalized.MetaDataTypeDefinitionGeneric( owner, m_token );
                        }
                        else
                        {
                            tdNew = new Normalized.MetaDataTypeDefinition( owner, m_token );
                        }

                        tdNew.m_flags     = m_flags;
                        tdNew.m_name      = m_name;
                        tdNew.m_nameSpace = m_nameSpace;

                        owner.Types.Add( tdNew );

                        context = context.Push( tdNew );

                        if(tdNew is Normalized.MetaDataTypeDefinitionGeneric)
                        {
                            Normalized.MetaDataTypeDefinitionGeneric tdNewG = (Normalized.MetaDataTypeDefinitionGeneric)tdNew;

                            context.GetNormalizedObjectList( m_genericParamList, out tdNewG.m_genericParams, MetaDataNormalizationMode.Default );
                        }

                        return tdNew;
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataTypeDefinitionBase td = (Normalized.MetaDataTypeDefinitionBase)obj;

            context = context.Push( obj );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.LinkingOfNestedClasses:
                    {
                        context.GetNormalizedObject    ( m_enclosingClass, out td.m_enclosingClass, MetaDataNormalizationMode.LookupExisting );
                        context.GetNormalizedObjectList( m_nestedClasses , out td.m_nestedClasses , MetaDataNormalizationMode.LookupExisting );
                    }
                    return;

                case MetaDataNormalizationPhase.DiscoveryOfBuiltInTypes:
                    {
                        ElementTypes elementType = ElementTypes.END;

                        if(m_enclosingClass == null && m_nameSpace == "System" && td.m_owner.Name == "mscorlib")
                        {
                            switch(m_name)
                            {
                                case "Void"          : elementType = ElementTypes.VOID      ; break;
                                case "Boolean"       : elementType = ElementTypes.BOOLEAN   ; break;
                                case "Char"          : elementType = ElementTypes.CHAR      ; break;
                                case "SByte"         : elementType = ElementTypes.I1        ; break;
                                case "Byte"          : elementType = ElementTypes.U1        ; break;
                                case "Int16"         : elementType = ElementTypes.I2        ; break;
                                case "UInt16"        : elementType = ElementTypes.U2        ; break;
                                case "Int32"         : elementType = ElementTypes.I4        ; break;
                                case "UInt32"        : elementType = ElementTypes.U4        ; break;
                                case "Int64"         : elementType = ElementTypes.I8        ; break;
                                case "UInt64"        : elementType = ElementTypes.U8        ; break;
                                case "Single"        : elementType = ElementTypes.R4        ; break;
                                case "Double"        : elementType = ElementTypes.R8        ; break;
                                case "String"        : elementType = ElementTypes.STRING    ; break;
                                case "IntPtr"        : elementType = ElementTypes.I         ; break;
                                case "UIntPtr"       : elementType = ElementTypes.U         ; break;
                                case "Object"        : elementType = ElementTypes.OBJECT    ; break;

                                case "Array"         : elementType = ElementTypes.SZARRAY   ; break;

                                case "ValueType"     : elementType = ElementTypes.VALUETYPE ; break;

                                case "TypedReference": elementType = ElementTypes.TYPEDBYREF; break;
                            }
                        }

                        if(elementType != ElementTypes.END)
                        {
                            context.RegisterBuiltIn( (Normalized.MetaDataTypeDefinition)td, elementType );
                        }
                    }
                    return;

                case MetaDataNormalizationPhase.CreationOfTypeHierarchy:
                    {
                        context.GetNormalizedObject( m_extends, out td.m_extends, MetaDataNormalizationMode.Default );

                        if(td.m_elementType == ElementTypes.END)
                        {
                            if((td.m_flags & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
                            {
                                td.m_elementType = ElementTypes.CLASS;
                            }
                            else if(td.m_extends != null && td.m_extends.m_elementType == ElementTypes.VALUETYPE)
                            {
                                td.m_elementType = ElementTypes.VALUETYPE;
                            }
                            else
                            {
                                td.m_elementType = ElementTypes.CLASS;
                            }
                        }
                    }
                    return;

                case MetaDataNormalizationPhase.CompletionOfTypeNormalization:
                    {
                        context.GetNormalizedObjectList( m_interfaces, out td.m_interfaces, MetaDataNormalizationMode.Default );

                        if(td is Normalized.MetaDataTypeDefinitionGeneric)
                        {
                            context.ProcessPhaseList( m_genericParamList );
                        }

                        context.GetNormalizedObject( m_classLayout, out td.m_classLayout, MetaDataNormalizationMode.Default );
                    }
                    return;

                case MetaDataNormalizationPhase.CreationOfFieldDefinitions:
                    {
                        context.GetNormalizedObjectList( m_fields, out td.m_fields, MetaDataNormalizationMode.Allocate );
                    }
                    return;

                case MetaDataNormalizationPhase.CreationOfMethodDefinitions:
                    {
                        context.GetNormalizedObjectList( m_methods, out td.m_methods, MetaDataNormalizationMode.Allocate );
                    }
                    return;

                case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                    {
                        context.GetNormalizedObjectList( m_events    , out td.m_events    , MetaDataNormalizationMode.Allocate );
                        context.GetNormalizedObjectList( m_properties, out td.m_properties, MetaDataNormalizationMode.Allocate );

                        context.ProcessPhaseList( m_methods );
                    }
                    return;

                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out td.m_customAttributes, MetaDataNormalizationMode.Allocate );

                        context.ProcessPhaseList( m_genericParamList );
                        context.ProcessPhaseList( m_fields           );
                        context.ProcessPhaseList( m_methods          );
                        context.ProcessPhaseList( m_events           );
                        context.ProcessPhaseList( m_properties       );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //--//

        // These are technically not constructor methods, but they are meant
        // to be used to set up the object

        internal void AddGenericParam( MetaDataGenericParam genericParam )
        {
            if(m_genericParamList == null)
            {
                m_genericParamList = new List<MetaDataGenericParam>( 2 );
            }

            if(genericParam.Number != m_genericParamList.Count)
            {
                throw IllegalMetaDataFormatException.Create( "Generic parameters out of order - is this allowed?" );
            }

            m_genericParamList.Add( genericParam );
        }

        internal void AddInterface( IMetaDataTypeDefOrRef itf )
        {
            if(m_interfaces == null)
            {
                m_interfaces = new List<IMetaDataTypeDefOrRef>( 2 );
            }

            m_interfaces.Add( itf );
        }

        internal void AddField( MetaDataField field )
        {
            if(m_fields == null)
            {
                m_fields = new List<MetaDataField>( 2 );
            }

            m_fields.Add( field );
        }

        internal void AddMethod( MetaDataMethod method )
        {
            if(m_methods == null)
            {
                m_methods = new List<MetaDataMethod>( 2 );
            }

            m_methods.Add( method );
        }

        internal void AddNestedClass( MetaDataTypeDefinition nestedClass )
        {
            if(m_nestedClasses == null)
            {
                m_nestedClasses = new List<MetaDataTypeDefinition>( 2 );
            }

            m_nestedClasses.Add( nestedClass );

            nestedClass.m_enclosingClass = this;
        }

        internal void AddEvent( MetaDataEvent eventObject )
        {
            if(m_events == null)
            {
                m_events = new List<MetaDataEvent>( 2 );
            }

            m_events.Add( eventObject );
        }

        internal void AddProperty( MetaDataProperty propertyObject )
        {
            if(m_properties == null)
            {
                m_properties = new List<MetaDataProperty>( 2 );
            }

            m_properties.Add( propertyObject );
        }

        //--//

        internal void SetClassLayout( MetaDataClassLayout classLayout )
        {
            m_classLayout = classLayout;
        }

        //
        // Access Methods
        //

        public TypeAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Namespace
        {
            get
            {
                return m_nameSpace;
            }
        }

        public List<MetaDataGenericParam> GenericParamList
        {
            get
            {
                return m_genericParamList;
            }
        }

        public IMetaDataTypeDefOrRef Extends
        {
            get
            {
                return m_extends;
            }
        }

        public List<IMetaDataTypeDefOrRef> Interfaces
        {
            get
            {
                return m_interfaces;
            }
        }

        public List<MetaDataField> Fields
        {
            get
            {
                return m_fields;
            }
        }

        public List<MetaDataMethod> Methods
        {
            get
            {
                return m_methods;
            }
        }

        public MetaDataTypeDefinition EnclosingClass
        {
            get
            {
                return m_enclosingClass;
            }
        }

        public List<MetaDataTypeDefinition> NestedClasses
        {
            get
            {
                return m_nestedClasses;
            }
        }

        public MetaDataClassLayout ClassLayout
        {
            get
            {
                return m_classLayout;
            }
        }

        public bool IsNestedType
        {
            get
            {
                return m_enclosingClass != null;
            }
        }

        // for non-nested types, returns <Namespace>.<Name>
        // for nested types, returns <enclosing class's FullName>.<Name>
        public override string FullName
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                if(this.IsNestedType)
                {
                    sb.Append( this.EnclosingClass.FullName );
                    sb.Append( "."                          );
                }

                if(this.Namespace.Length != 0)
                {
                    sb.Append( this.Namespace );
                    sb.Append( "."            );
                }

                sb.Append( this.Name );

                return sb.ToString();
            }
        }

        public override string FullNameWithContext
        {
            get
            {
                return "type definition " + this.FullName;
            }
        }

        public static void ParseName(     string   typeName     ,
                                      out string   nameSpace    ,
                                      out string   name         ,
                                      out string[] nested       ,
                                      out string   assemblyName ,
                                      out string   extraInfo    )
        {
            //
            // An external type reference has this structure:
            //
            //  <Namespace>.<Name>[+<Nested>]*[, <AssemblyName>[, Version=<version>[, Culture=<culture>[, PublicKeyToken=<key>]]]]
            //
            int index;

            index = typeName.IndexOf( ',' );
            if(index >= 0)
            {
                extraInfo = typeName.Substring( index+1        ).TrimStart();
                typeName  = typeName.Substring( 0      , index );

                index = extraInfo.IndexOf( ',' );
                if(index >= 0)
                {
                    assemblyName = extraInfo.Substring( 0      , index );
                    extraInfo    = extraInfo.Substring( index+1        ).TrimStart();
                }
                else
                {
                    assemblyName = null;
                }
            }
            else
            {
                assemblyName = null;
                extraInfo    = null;
            }

            // Is this a nested class?
            index = typeName.IndexOf( '+' );
            if(index >= 0)
            {
                string[] split = typeName.Split( '+' );

                nested = new string[split.Length-1];
                Array.Copy( split, 1, nested, 0, nested.Length );

                typeName = split[0];
            }
            else
            {
                nested = null;
            }

            index = typeName.LastIndexOf( '.' );
            if(index >= 0)
            {
                nameSpace = typeName.Substring( 0      , index );
                name      = typeName.Substring( index+1        );
            }
            else
            {
                nameSpace = null;
                name      = typeName;
            }
        }

        public bool IsNameMatch( string   nameSpace ,
                                 string   name      ,
                                 string[] nested    )
        {
            MetaDataTypeDefinition td = this;

            if(nested != null)
            {
                int nestedSize = nested.Length;

                while(nestedSize > 0)
                {
                    if(td.IsNestedType == false)
                    {
                        return false;
                    }

                    if(td.Name != nested[--nestedSize])
                    {
                        return false;
                    }

                    td = td.m_enclosingClass;
                }
            }

            if(td.IsNestedType == true)
            {
                return false;
            }

            if(td.Namespace == nameSpace && td.Name == name)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return "MetaDataTypeDefinition(" + this.FullName + ")";
        }

        public override string ToStringLong()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataTypeDefinition(" );

            sb.Append( ((int)m_flags).ToString( "x" ) ); sb.Append( "," );
            sb.Append( this.FullName ); sb.Append( "," );

            if(m_genericParamList != null && m_genericParamList.Count > 0)
            {
                sb.Append( "GenericParams<" );
                foreach(MetaDataGenericParam param in m_genericParamList)
                {
                    sb.Append( param.ToString() );
                    sb.Append( "," );
                }
                sb.Remove( sb.Length - 1, 1 );
                sb.Append( ">," );
            }

            if(m_extends != null)
            {
                sb.Append( m_extends.ToString() );
            }
            sb.Append( "," );

            if(m_interfaces != null && m_interfaces.Count > 0)
            {
                sb.Append( "interfaces(" );
                foreach(MetaDataObject interfaceObject in m_interfaces)
                {
                    sb.Append( interfaceObject.ToString() );
                    sb.Append( "," );
                }
                sb.Remove( sb.Length - 1, 1 );
                sb.Append( ")" );
            }
            else
            {
                sb.Append( "No interfaces" );
            }
            sb.Append( "," );

            if(m_fields.Count > 0)
            {
                sb.Append( "fields(" );
                foreach(MetaDataField field in m_fields)
                {
                    sb.Append( field.ToString() );
                    sb.Append( "," );
                }
                sb.Remove( sb.Length - 1, 1 );
                sb.Append( ")" );
            }
            else
            {
                sb.Append( "No fields" );
            }
            sb.Append( "," );

            if(m_methods.Count > 0)
            {
                sb.Append( "methods(" );
                foreach(MetaDataMethod method in m_methods)
                {
                    sb.Append( method.ToString() );
                    sb.Append( "," );
                }
                sb.Remove( sb.Length - 1, 1 );
                sb.Append( ")" );
            }
            else
            {
                sb.Append( "No methods" );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

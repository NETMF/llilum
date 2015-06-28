//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public sealed class MetaDataMemberRef : MetaDataObject,
        IMetaDataMethodDefOrRef,
        IMetaDataCustomAttributeType,
        IMetaDataNormalize
    {
        //
        // State
        //

        private IMetaDataMemberRefParent m_classObject;
        private String                   m_name;
        private Signature                m_signature;

        //
        // Constructor Methods
        //

        private MetaDataMemberRef( int index ) : base( TokenType.MemberRef, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataMemberRef( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader classReader = ts.m_columns[0].m_reader;
            int                classIndex;
            int                signatureIndex;

            classIndex     =        classReader       ( reader );
            m_name         = parser.readIndexAsString ( reader );
            signatureIndex = parser.readIndexAsForBlob( reader );

            m_classObject  = parser.getMemberRefParent( classIndex );
            m_signature    = Signature.ParseMemberRef( parser, parser.getSignature( signatureIndex ) );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            Normalized.MetaDataTypeDefinitionAbstract classObjectAbstract;

            context.GetNormalizedObject( m_classObject, out classObjectAbstract, MetaDataNormalizationMode.Default );

            Normalized.MetaDataTypeDefinitionAbstract typeContext   = classObjectAbstract;
            Normalized.MetaDataMethodAbstract         methodContext = context.GetMethodFromContext();

            if(m_signature is Importer.SignatureMethod)
            {
                Importer.SignatureMethod   signature = (Importer.SignatureMethod)m_signature;
                Normalized.SignatureMethod signatureMethod;

                context.GetNormalizedSignature( signature, out signatureMethod, MetaDataNormalizationMode.Default );

                while(classObjectAbstract != null)
                {
                    Normalized.MetaDataMethodBase mdNew = null;

                    if(classObjectAbstract is Normalized.MetaDataTypeDefinition)
                    {
                        Normalized.MetaDataTypeDefinition classObject = (Normalized.MetaDataTypeDefinition)classObjectAbstract;

                        mdNew = Match( classObject.Methods, m_name, signatureMethod );
                    }
                    else if(classObjectAbstract is Normalized.MetaDataTypeDefinitionGeneric)
                    {
                        Normalized.MetaDataTypeDefinitionGeneric classObject = (Normalized.MetaDataTypeDefinitionGeneric)classObjectAbstract;

                        mdNew = Match( classObject.Methods, m_name, signatureMethod );
                    }
                    else if(classObjectAbstract is Normalized.MetaDataTypeDefinitionGenericInstantiation)
                    {
                        Normalized.MetaDataTypeDefinitionGenericInstantiation classObject = (Normalized.MetaDataTypeDefinitionGenericInstantiation)classObjectAbstract;

                        mdNew = Match( classObject.GenericType.Methods, m_name, signatureMethod );
                    }
                    else if(classObjectAbstract is Normalized.MetaDataTypeDefinitionArray)
                    {
                        Normalized.MetaDataTypeDefinitionArray classObject = (Normalized.MetaDataTypeDefinitionArray)classObjectAbstract;

                        mdNew = Match( classObject.Methods, m_name, signatureMethod );
                    }

                    if(mdNew != null)
                    {
                        if(mdNew.IsOpenMethod || (classObjectAbstract is Normalized.MetaDataTypeDefinitionGenericInstantiation))
                        {
                            Normalized.MetaDataMethodWithContext mdNew2 = new Normalized.MetaDataMethodWithContext( typeContext, methodContext, mdNew );

                            return mdNew2.MakeUnique();
                        }

                        return mdNew;
                    }

                    classObjectAbstract = classObjectAbstract.Extends;
                }

                throw UnresolvedExternalReferenceException.Create( this, "Cannot find method {0} with signature {1} in type {2}", m_name, m_signature, m_classObject );
            }
            else if(m_signature is Importer.SignatureField)
            {
                Normalized.SignatureField signatureField;

                context.GetNormalizedSignature( m_signature, out signatureField, MetaDataNormalizationMode.Default );

                while(classObjectAbstract != null)
                {
                    Normalized.MetaDataField fdNew = null;

                    if(classObjectAbstract is Normalized.MetaDataTypeDefinitionBase)
                    {
                        Normalized.MetaDataTypeDefinition classObject = (Normalized.MetaDataTypeDefinition)classObjectAbstract;

                        fdNew = Match( classObject.Fields, m_name, signatureField );
                    }
                    else if(classObjectAbstract is Normalized.MetaDataTypeDefinitionGenericInstantiation)
                    {
                        Normalized.MetaDataTypeDefinitionGenericInstantiation classObject = (Normalized.MetaDataTypeDefinitionGenericInstantiation)classObjectAbstract;

                        fdNew = Match( classObject.GenericType.Fields, m_name, signatureField );
                    }

                    if(fdNew != null)
                    {
                        if(fdNew.IsOpenField || (classObjectAbstract is Normalized.MetaDataTypeDefinitionGenericInstantiation))
                        {
                            Normalized.MetaDataFieldWithContext fdNew2 = new Normalized.MetaDataFieldWithContext( typeContext, fdNew );

                            return fdNew2.MakeUnique();
                        }

                        return fdNew;
                    }

                    classObjectAbstract = classObjectAbstract.Extends;
                }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        private static Normalized.MetaDataMethodBase Match( Normalized.MetaDataMethodBase[] methods   ,
                                                            string                          name      ,
                                                            Normalized.SignatureMethod      signature )
        {
            foreach(Normalized.MetaDataMethodBase md in methods)
            {
                if(md.Match( name, signature ))
                {
                    return md;
                }
            }

            return null;
        }

        private static Normalized.MetaDataField Match( Normalized.MetaDataField[] fields    ,
                                                       string                     name      ,
                                                       Normalized.SignatureField  signature )
        {
            foreach(Normalized.MetaDataField field in fields)
            {
                if(field.Match( name, signature ))
                {
                    return field;
                }
            }

            return null;
        }


        //
        // Access Methods
        //

        public IMetaDataMemberRefParent Class
        {
            get
            {
                return m_classObject;
            }
        }

        public override string FullName
        {
            get
            {
                if(m_classObject != null)
                {
                    return ((MetaDataObject)m_classObject).FullName + "." + this.Name;
                }
                else
                {
                    return this.Name;
                }
            }
        }

        public override string FullNameWithContext
        {
            get
            {
                return "member reference " + this.FullName;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public Signature Signature
        {
            get
            {
                return m_signature;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataMemberRef(" + m_classObject + "," + m_name + "," + m_signature + ")";
        }
    }
}

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
    public sealed class MetaDataTypeReference : MetaDataObject,
        IMetaDataTypeDefOrRef,
        IMetaDataMemberRefParent,
        IMetaDataResolutionScope,
        IMetaDataNormalize
    {
        //
        // State
        //

        private IMetaDataResolutionScope m_resolutionScope;
        private String                   m_name;
        private String                   m_nameSpace;

        //
        // Constructor Methods
        //

        private MetaDataTypeReference( int index ) : base( TokenType.TypeRef, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataTypeReference( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader bigScopeIndexReader = ts.m_columns[0].m_reader;
            int                resolutionScopeIndex;

            resolutionScopeIndex =        bigScopeIndexReader( reader );
            m_name               = parser.readIndexAsString  ( reader );
            m_nameSpace          = parser.readIndexAsString  ( reader );

            m_resolutionScope = parser.getResolutionScope( resolutionScopeIndex );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfTypeReferences:
                    {
                        Normalized.IMetaDataObject scope;

                        context.GetNormalizedObject( m_resolutionScope, out scope, MetaDataNormalizationMode.Default );

                        //
                        // WARNING: This lookup is oblivious of visibility constraints!!
                        //
                        if(scope is Normalized.MetaDataAssembly)
                        {
                            Normalized.MetaDataAssembly asml = (Normalized.MetaDataAssembly)scope;

                            foreach(Normalized.MetaDataTypeDefinitionAbstract td in asml.Types)
                            {
                                if(td is Normalized.MetaDataTypeDefinitionBase)
                                {
                                    Normalized.MetaDataTypeDefinitionBase td2 = (Normalized.MetaDataTypeDefinitionBase)td;

                                    if(td2.Namespace == m_nameSpace &&
                                       td2.Name      == m_name       )
                                    {
                                        return td2;
                                    }
                                }
                            }
                        }
                        else if(scope is Normalized.MetaDataTypeDefinitionBase)
                        {
                            Normalized.MetaDataTypeDefinitionBase td = (Normalized.MetaDataTypeDefinitionBase)scope;

                            foreach(Normalized.MetaDataTypeDefinitionAbstract tdNested in td.NestedClasses)
                            {
                                if(tdNested is Normalized.MetaDataTypeDefinitionBase)
                                {
                                    Normalized.MetaDataTypeDefinitionBase tdNested2 = (Normalized.MetaDataTypeDefinitionBase)tdNested;

                                    if(tdNested2.Name == m_name)
                                    {
                                        return tdNested2;
                                    }
                                }
                            }
                        }

                        throw UnresolvedExternalReferenceException.Create( this, "Cannot resolve external reference '{0}'", this.FullName );
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        // Returns one of MetaData{Module,ModuleRef,AssemblyRef,TypeReference}
        public IMetaDataResolutionScope ResolutionScope
        {
            get
            {
                return m_resolutionScope;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        public String Namespace
        {
            get
            {
                return m_nameSpace;
            }
        }

        public override string FullName
        {
            get
            {
                String fullName;

                if(this.Namespace.Length == 0)
                {
                    fullName = this.Name;
                }
                else
                {
                    fullName = this.Namespace + "." + this.Name;
                }

                if(this.ResolutionScope is MetaDataTypeReference)
                {
                    MetaDataTypeReference typeRef = (MetaDataTypeReference)this.ResolutionScope;

                    fullName = typeRef.FullName + "." + fullName;
                }

                return fullName;
            }
        }


        public override string FullNameWithContext
        {
            get
            {
                return "type reference " + this.FullName;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataTypeReference(" + this.FullName + ")";
        }

        public override String ToStringLong()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataTypeReference(" );

            sb.Append( "[" );
            sb.Append( m_resolutionScope );
            sb.Append( "]" );

            sb.Append( this.FullName );

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

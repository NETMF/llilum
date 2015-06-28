//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public partial class MetaDataMethod : MetaDataObject,
        IMetaDataHasDeclSecurity,
        IMetaDataMethodDefOrRef,
        IMetaDataMemberForwarded,
        IMetaDataCustomAttributeType,
        IMetaDataTypeOrMethodDef,
        IMetaDataNormalize
    {
        //
        // State
        //

        private int                        m_rva;
        private MethodImplAttributes       m_implFlags;
        private MethodAttributes           m_flags;
        private string                     m_name;
        private SignatureMethod            m_signature;

        private List<MetaDataParam>        m_paramList;
        private List<MetaDataGenericParam> m_genericParamList;

        private MetaDataTypeDefinition     m_parent;

        private Instruction[]              m_instructions;
        private EHClause[]                 m_ehTable;
        private int                        m_maxStack;
        private SignatureType[]            m_locals;
        private bool                       m_initLocals;

        // information for debugging
        private Debugging.MethodDebugInfo  m_debugInfo;

        //
        // Constructor Methods
        //

        private MetaDataMethod( int index ) : base( TokenType.Method, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataMethod( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader paramReader = ts.m_columns[5].m_reader;
            int                signatureIndex;
            int                paramIndex;

            m_rva          =                            reader.ReadInt32();
            m_implFlags    = (MethodImplAttributes)     reader.ReadInt16();
            m_flags        = (MethodAttributes)         reader.ReadInt16();
            m_name         = parser.readIndexAsString ( reader );
            signatureIndex = parser.readIndexAsForBlob( reader );
            paramIndex     =        paramReader       ( reader );

            m_signature = SignatureMethod.Parse( parser, parser.getSignature( signatureIndex ) );

            parser.SetParamIndex( this, paramIndex );

            m_parent = parser.GetTypeFromMethodIndex( MetaData.UnpackTokenAsIndex( m_token ) );

            m_parent.AddMethod( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CreationOfMethodDefinitions:
                    {
                        Normalized.MetaDataMethodBase         mdNew;
                        Normalized.MetaDataTypeDefinitionBase owner = (Normalized.MetaDataTypeDefinitionBase)context.GetTypeFromContext();

                        if(m_genericParamList != null && m_genericParamList.Count > 0)
                        {
                            mdNew = new Normalized.MetaDataMethodGeneric( owner, m_token );
                        }
                        else
                        {
                            mdNew = new Normalized.MetaDataMethod( owner, m_token );
                        }

                        //--//

                        mdNew.m_implFlags  = m_implFlags;
                        mdNew.m_flags      = m_flags;
                        mdNew.m_name       = m_name;
                        mdNew.m_maxStack   = m_maxStack;
                        mdNew.m_initLocals = m_initLocals;
                        mdNew.m_debugInfo  = m_debugInfo;

                        context = context.Push( mdNew );

                        if(mdNew is Normalized.MetaDataMethodGeneric)
                        {
                            Normalized.MetaDataMethodGeneric mdNewG = (Normalized.MetaDataMethodGeneric)mdNew;

                            context.GetNormalizedObjectList( m_genericParamList, out mdNewG.m_genericParams, MetaDataNormalizationMode.Default );
                        }

                        context.GetNormalizedSignature     ( m_signature, out mdNew.m_signature, MetaDataNormalizationMode.Default );
                        context.GetNormalizedObjectList    ( m_paramList, out mdNew.m_paramList, MetaDataNormalizationMode.Default );
                        context.GetNormalizedSignatureArray( m_locals   , out mdNew.m_locals   , MetaDataNormalizationMode.Default );

                        return mdNew;
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataMethodBase md = (Normalized.MetaDataMethodBase)obj;

            context = context.Push( obj );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                    {
                        if(m_instructions != null)
                        {
                            md.m_instructions = new Normalized.Instruction[m_instructions.Length];

                            for(int i = 0; i < m_instructions.Length; i++)
                            {
                                md.m_instructions[i] = m_instructions[i].Normalize( context );
                            }
                        }

                        if(m_ehTable != null)
                        {
                            md.m_ehTable = new Normalized.EHClause[m_ehTable.Length];

                            for(int i = 0; i < m_ehTable.Length; i++)
                            {
                                md.m_ehTable[i] = m_ehTable[i].Normalize( context );
                            }
                        }
                    }
                    return;

                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out md.m_customAttributes, MetaDataNormalizationMode.Allocate );

                        context.ProcessPhaseList( m_paramList );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //--//

        // These are technically not constructor methods, but they are meant to
        // be used to set up the object

        internal void AddParam( MetaDataParam param )
        {
            if(m_paramList == null)
            {
                m_paramList = new List<MetaDataParam>( 2 );
            }

            if(m_paramList.Count > 0 && m_paramList[m_paramList.Count-1].Sequence >= param.Sequence)
            {
                throw IllegalMetaDataFormatException.Create( "Parameters out of order - is this allowed?" );
            }

            m_paramList.Add( param );
        }

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

        //--//

        internal void loadInstructions( Parser              mdLoader    ,
                                        PdbInfo.PdbFunction pdbFunction )
        {
            if(m_rva != 0)
            {
                if((m_flags & MethodAttributes.PinvokeImpl    ) == 0 ||
                   (m_flags & MethodAttributes.UnmanagedExport) != 0  )
                {
                    getInstructions( mdLoader, pdbFunction );

                    if(pdbFunction != null)
                    {
                        List<PdbInfo.PdbSlot> list = new List<PdbInfo.PdbSlot>();

                        pdbFunction.CollectSlots( list );

                        int max = -1;

                        foreach(PdbInfo.PdbSlot slot in list)
                        {
                            if(max < slot.Slot) max = (int)slot.Slot;
                        }

                        if(max >= 0)
                        {
                            String[] localVarNames = new String[this.Locals.Length];

                            foreach(PdbInfo.PdbSlot slot in list)
                            {
                                localVarNames[slot.Slot] = slot.Name;
                            }

                            m_debugInfo = new Debugging.MethodDebugInfo( localVarNames );
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException( "Not loading embedded native code for " + this );
                }
            }
        }

        //
        // Access Methods
        //

        public List<MetaDataParam> ParamList
        {
            get
            {
                return m_paramList;
            }
        }

        public List<MetaDataGenericParam> GenericParamList
        {
            get
            {
                return m_genericParamList;
            }
        }

        public Instruction[] Instructions
        {
            get
            {
                return m_instructions;
            }
        }

        public EHClause[] EHTable
        {
            get
            {
                return m_ehTable;
            }
        }

        public int MaxStack
        {
            get
            {
                return m_maxStack;
            }
        }

        public SignatureType[] Locals
        {
            get
            {
                return m_locals;
            }
        }

        public MethodImplAttributes ImplFlags
        {
            get
            {
                return m_implFlags;
            }
        }

        public MethodAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public override string FullName
        {
            get
            {
                if(m_parent != null)
                {
                    return m_parent.FullName + "." + this.Name;
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
                return "method " + this.FullName;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public SignatureMethod Signature
        {
            get
            {
                return (SignatureMethod)m_signature;
            }
        }

        public MetaDataTypeDefinition Parent
        {
            get
            {
                return m_parent;
            }
        }

        public int Rva
        {
            get
            {
                return m_rva;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (m_rva == 0);
            }
        }

        public bool InitLocals
        {
            get
            {
                return m_initLocals;
            }
        }

        //--//

        public Debugging.MethodDebugInfo DebugInformation
        {
            get
            {
                return m_debugInfo;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return "MetaDataMethod(" + this.FullName + ")";
        }

        public override string ToStringLong()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder( "MetaDataMethod(" );

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

            sb.Append( m_rva );
            sb.Append( "," );
            sb.Append( m_implFlags.ToString( "x" ) );
            sb.Append( "," );
            sb.Append( m_flags.ToString( "x" ) );
            sb.Append( "," );

            if(m_parent != null)
            {
                sb.Append( m_parent.FullName );
                sb.Append( "." );
            }

            sb.Append( m_name );
            sb.Append( "," );
            sb.Append( m_signature );
            sb.Append( "," );

            if(m_paramList == null || m_paramList.Count == 0)
            {
                sb.Append( "No parameters" );
            }
            else
            {
                sb.Append( "parameters(" );
                foreach(MetaDataParam param in m_paramList)
                {
                    sb.Append( param.ToString() );
                    sb.Append( "," );
                }
                sb.Remove( sb.Length - 1, 1 );
                sb.Append( ")" );
            }

            sb.Append( ")" );

            return sb.ToString();
        }
    }
}

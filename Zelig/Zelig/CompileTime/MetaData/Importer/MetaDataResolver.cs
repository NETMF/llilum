//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

//#define METADATARESOLVER_DEBUG
#define METADATARESOLVER_DEBUG_OP_EQUALITY


using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Microsoft.Zelig.MetaData
{
    public interface ISymbolResolverHelper
    {
        Importer.PdbInfo.PdbFile ResolveAssemblySymbols( string file );
    }

    public interface IMetaDataResolverHelper
    {
        Importer.MetaData ResolveAssemblyReference( string          name ,
                                                    MetaDataVersion ver  );
    }

    public sealed class MetaDataResolver : Normalized.IMetaDataBootstrap
    {
        internal class AssemblyPair
        {
            //
            // State
            //

            internal Importer.MetaData           Imported;
            internal Normalized.MetaDataAssembly Normalized;
            internal MetaDataNormalizationPhase  Phase;

            //
            // Constructor Methods
            //

            internal AssemblyPair( Importer.MetaData           imported   ,
                                   Normalized.MetaDataAssembly normalized )
            {
                this.Imported   = imported;
                this.Normalized = normalized;
                this.Phase      = MetaDataNormalizationPhase.Uninitialized;
            }
        }

        //--//

        //
        // State
        //

        private IMetaDataResolverHelper                                                                                   m_metaDataResolverHelper;
                                                                                                                  
        private List< Importer.MetaData           >                                                                       m_metaDataInitialSet;
        private List<          AssemblyPair       >                                                                       m_metaDataList;
        private List<          object             >                                                                       m_pendingEntries;
                                                                                                                  
        private Normalized.MetaDataTypeDefinition[]                                                                       m_lookupBuiltin;
        private GrowOnlyHashTable< String                                , Normalized.MetaDataTypeDefinitionBase > m_lookupTypeNames;
        private GrowOnlyHashTable< Importer  .IMetaDataNormalize         , Normalized.IMetaDataObject            > m_lookupEntries;
        private GrowOnlyHashTable< Importer  .IMetaDataNormalizeSignature, Normalized.MetaDataSignature          > m_lookupSignatureEntries;
        private GrowOnlySet      <                                         Normalized.IMetaDataUnique            > m_lookupUniques;
                                                                 
        private Normalized.MetaDataAssembly                                                                               m_assemblyForDelayedParameters;

        //
        // Constructor Methods
        //

        public MetaDataResolver( IMetaDataResolverHelper metaDataResolverHelper )
        {
            Importer.MetaData.ObjectComparer    impComparer = new Importer.MetaData.ObjectComparer   ();
            Importer.MetaData.SignatureComparer sigComparer = new Importer.MetaData.SignatureComparer();

            m_metaDataResolverHelper = metaDataResolverHelper;
            m_metaDataInitialSet     = new List< Importer.MetaData           >();
            m_metaDataList           = new List<          AssemblyPair       >();
            m_pendingEntries         = new List<          object             >();

            m_lookupBuiltin          = new Normalized.MetaDataTypeDefinition[(int)ElementTypes.MAX];
            m_lookupTypeNames        = HashTableFactory.New            < String                                , Normalized.MetaDataTypeDefinitionBase >(             );
            m_lookupEntries          = HashTableFactory.NewWithComparer< Importer  .IMetaDataNormalize         , Normalized.IMetaDataObject            >( impComparer );
            m_lookupSignatureEntries = HashTableFactory.NewWithComparer< Importer  .IMetaDataNormalizeSignature, Normalized.MetaDataSignature          >( sigComparer );
            m_lookupUniques          = SetFactory      .New            <                                         Normalized.IMetaDataUnique            >(             );

            //--//

            m_assemblyForDelayedParameters = new Normalized.MetaDataAssembly( this.UniqueDictionary, 0 );

            m_assemblyForDelayedParameters.m_name    = "<AssemblyForDelayedParameters>";
            m_assemblyForDelayedParameters.m_version = new MetaDataVersion();

            AssemblyPair pair = new AssemblyPair( null, m_assemblyForDelayedParameters );

            m_metaDataList.Add( pair );
        }

        //--//

        [System.Diagnostics.Conditional( "METADATARESOLVER_DEBUG" )]
        void DebugPrint(        string   fmt  ,
                         params object[] args )
        {
            Console.WriteLine( fmt, args );
        }

        //--//

        public void Add( Importer.MetaData metaData )
        {
            m_metaDataInitialSet.Add( metaData );
        }

        //--//

        //[System.Diagnostics.DebuggerHidden]
        internal void RegisterBuiltIn( Normalized.MetaDataTypeDefinition td          ,
                                       ElementTypes                      elementType )
        {
            m_lookupBuiltin[(int)elementType] = td;

            td.m_elementType = elementType;
        }

        //[System.Diagnostics.DebuggerHidden]
        internal Normalized.MetaDataTypeDefinition LookupBuiltIn( ElementTypes elementType )
        {
            Normalized.MetaDataTypeDefinition res = m_lookupBuiltin[(int)elementType];

            if(res != null)
            {
                return res;
            }

            throw Importer.IllegalMetaDataFormatException.Create( "Cannot resolve builtin {0}", elementType );
        }

        //[System.Diagnostics.DebuggerHidden]
        internal Normalized.MetaDataTypeDefinitionBase ResolveName( String                       key     ,
                                                                    MetaDataNormalizationContext context )
        {
            Normalized.MetaDataTypeDefinitionBase res;

            if(m_lookupTypeNames.TryGetValue( key, out res ))
            {
                return res;
            }

            string   nameSpace;
            string   name;
            string[] nested;
            string   assemblyName;
            string   extraInfo;

            Importer.MetaDataTypeDefinition.ParseName( key, out nameSpace, out name, out nested, out assemblyName, out extraInfo );

            if(assemblyName != null)
            {
                AssemblyPair pair = FindAssembly( assemblyName, null, context );

                Normalized.MetaDataTypeDefinitionBase td = ResolveName( pair, nameSpace, name, nested );
                if(td != null)
                {
                    m_lookupTypeNames[key] = td;

                    return td;
                }
            }
            else
            {
                foreach(AssemblyPair pair in m_metaDataList)
                {
                    Normalized.MetaDataTypeDefinitionBase td = ResolveName( pair, nameSpace, name, nested );
                    if(td != null)
                    {
                        m_lookupTypeNames[key] = td;

                        return td;
                    }
                }
            }

            throw Importer.UnresolvedExternalReferenceException.Create( null, "Cannot resolve reference to type '{0}' in '{1}'", key, context );
        }

        private Normalized.MetaDataTypeDefinitionBase ResolveName( AssemblyPair pair      ,
                                                                   string       nameSpace ,
                                                                   string       name      ,
                                                                   string[]     nested    )
        {
            if(pair.Normalized != null)
            {
                return ResolveName( pair.Normalized, nameSpace, name, nested );
            }

            return null;
        }

        private Normalized.MetaDataTypeDefinitionBase ResolveName( Normalized.MetaDataAssembly asml      ,
                                                                   string                      nameSpace ,
                                                                   string                      name      ,
                                                                   string[]                    nested    )
        {
            foreach(Normalized.MetaDataTypeDefinitionAbstract td in asml.Types)
            {
                if(td is Normalized.MetaDataTypeDefinitionBase)
                {
                    Normalized.MetaDataTypeDefinitionBase td2 = (Normalized.MetaDataTypeDefinitionBase)td;

                    if(td2.IsNameMatch( nameSpace, name, nested ))
                    {
                        return td2;
                    }
                }
            }

            return null;
        }

        //--//

        private void TrackRecursion_EnterNormalization( object src )
        {
            if(m_pendingEntries.Contains( src ))
            {
                throw Importer.IllegalMetaDataFormatException.Create( "Detected recursive resolution attempt on: {0}", src );
            }

            DebugPrint( "Normalizing:\r\n    IN :{0}", src );

            m_pendingEntries.Add( src );
        }

        private void TrackRecursion_ExitNormalization( object src ,
                                                       object dst )
        {
            m_pendingEntries.Remove( src );

            DebugPrint( "Normalized:\r\n    IN :{0}\r\n    OUT:{1}", src, dst );
        }

        private void TrackRecursion_EnterProcessing( object src ,
                                                     object dst )
        {
            if(m_pendingEntries.Contains( src ))
            {
                throw Importer.IllegalMetaDataFormatException.Create( "Detected recursive resolution attempt on: {0}", src );
            }

            DebugPrint( "Processing:\r\n    IN :{0}\r\n    OUT:{1}", src, dst );

            m_pendingEntries.Add( src );
        }

        private void TrackRecursion_ExitProcessing( object src ,
                                                    object dst )
        {
            m_pendingEntries.Remove( src );

            DebugPrint( "Processed:\r\n    IN :{0}\r\n    OUT:{1}", src, dst );
        }

        //--//

        internal Normalized.MetaDataAssembly GetAssemblyForDelayedParameters()
        {
            return m_assemblyForDelayedParameters;
        }

        //--//

        //[System.Diagnostics.DebuggerHidden]
        internal Normalized.IMetaDataObject Lookup( Importer.IMetaDataObject     key     ,
                                                    MetaDataNormalizationContext context )
        {
            if(key != null)
            {
                Normalized.IMetaDataObject en;

                if(m_lookupEntries.TryGetValue( (Importer.IMetaDataNormalize)key, out en ))
                {
                    DebugPrint( "Lookup:\r\n    IN :{0}\r\n    OUT:{1}", key, en );

                    return en;
                }
            }

            return null;
        }

        //[System.Diagnostics.DebuggerHidden]
        internal Normalized.IMetaDataObject GetNormalizedObject( Importer.IMetaDataNormalize  obj     ,
                                                                 MetaDataNormalizationContext context ,
                                                                 MetaDataNormalizationMode    mode    )
        {
            if(obj == null)
            {
                return null;
            }

            bool fLookupExisting = (mode & MetaDataNormalizationMode.LookupExisting ) != 0;
            bool fAllocate       = (mode & MetaDataNormalizationMode.Allocate       ) != 0;

            Normalized.IMetaDataObject en;

            if(m_lookupEntries.TryGetValue( obj, out en ))
            {
                if(fLookupExisting)
                {
                    DebugPrint( "Lookup:\r\n    IN :{0}\r\n    OUT:{1}", obj, en );

                    return en;
                }

                throw Importer.IllegalMetaDataFormatException.Create( "Detected recursive allocation attempt on: {0}", obj );
            }

            if(!fAllocate)
            {
                throw Importer.IllegalMetaDataFormatException.Create( "Cannot resolve lookup: {0}", obj );
            }

            //--//

            TrackRecursion_EnterNormalization( obj );

            Normalized.IMetaDataObject res = obj.AllocateNormalizedObject( context );
            if(res == null)
            {
                throw Unresolvable( obj );
            }

            TrackRecursion_ExitNormalization( obj, res );

            //--//

            if(m_lookupEntries.ContainsKey( obj ))
            {
                throw Importer.IllegalMetaDataFormatException.Create( "Internal error: MetaData object redefined => {0}", obj );
            }

            m_lookupEntries.Add( obj, res );

            return res;
        }

        //[System.Diagnostics.DebuggerHidden]
        internal Normalized.MetaDataSignature GetNormalizedObject( Importer.IMetaDataNormalizeSignature obj     ,
                                                                   MetaDataNormalizationContext         context ,
                                                                   MetaDataNormalizationMode            mode    )
        {
            if(obj == null)
            {
                return null;
            }

            bool fLookupExisting = (mode & MetaDataNormalizationMode.LookupExisting ) != 0;
            bool fAllocate       = (mode & MetaDataNormalizationMode.Allocate       ) != 0;

            Normalized.MetaDataSignature en;

            if(m_lookupSignatureEntries.TryGetValue( obj, out en ))
            {
                if(fLookupExisting)
                {
                    DebugPrint( "Lookup:\r\n    IN :{0}\r\n    OUT:{1}", obj, en );

                    return en;
                }

                throw Importer.IllegalMetaDataFormatException.Create( "Detected recursive allocation attempt on: {0}", obj );
            }

            if(!fAllocate)
            {
                throw Importer.IllegalMetaDataFormatException.Create( "Cannot resolve lookup: {0}", obj );
            }

            //--//

            TrackRecursion_EnterNormalization( obj );

            Normalized.MetaDataSignature res = obj.AllocateNormalizedObject( context );
            if(res == null)
            {
                throw Unresolvable( obj );
            }

            TrackRecursion_ExitNormalization( obj, res );

            //--//

            if(m_lookupSignatureEntries.ContainsKey( obj ))
            {
                throw Importer.IllegalMetaDataFormatException.Create( "Internal error: MetaData object redefined => {0}", obj );
            }

            m_lookupSignatureEntries.Add( obj, res );

            return res;
        }

        //[System.Diagnostics.DebuggerHidden]
        internal void ProcessPhase( Importer.IMetaDataObject     obj     ,
                                    MetaDataNormalizationContext context )
        {
            if(obj != null)
            {
                Importer.IMetaDataNormalize obj2 = (Importer.IMetaDataNormalize)obj;
                Normalized.IMetaDataObject  dst  = GetNormalizedObject( obj2, context, MetaDataNormalizationMode.LookupExisting );

                TrackRecursion_EnterProcessing( obj2, dst );

                obj2.ExecuteNormalizationPhase( dst, context );

                TrackRecursion_ExitProcessing( obj2, dst );
            }
        }

        //--//

        public Normalized.MetaDataAssembly[] NormalizedAssemblies
        {
            get
            {
                List< Normalized.MetaDataAssembly > lst = new List< Normalized.MetaDataAssembly >();

                foreach(AssemblyPair pair in m_metaDataList)
                {
                    lst.Add( pair.Normalized );
                }

                return lst.ToArray();
            }
        }

        public Normalized.MetaDataMethodBase ApplicationEntryPoint
        {
            get
            {
                foreach(AssemblyPair pair in m_metaDataList)
                {
                    Normalized.MetaDataMethodBase entryPoint = pair.Normalized.EntryPoint;
                    
                    if(entryPoint != null)
                    {
                        return entryPoint;
                    }
                }

                return null;
            }
        }

        internal GrowOnlySet< Normalized.IMetaDataUnique > UniqueDictionary
        {
            get
            {
                return m_lookupUniques;
            }
        }

        //--//

        //
        // Normalized.IMetaDataBootstrap
        //

        Normalized.MetaDataAssembly Normalized.IMetaDataBootstrap.GetAssembly( string name )
        {
            List<Normalized.MetaDataTypeDefinitionAbstract> res = new List<Microsoft.Zelig.MetaData.Normalized.MetaDataTypeDefinitionAbstract>();

            foreach(AssemblyPair pair in m_metaDataList)
            {
                Normalized.MetaDataAssembly asml = pair.Normalized;

                //
                // BUGBUG: We need a better check.
                //
                if(asml.Name == name)
                {
                    return asml;
                }
            }

            return null;
        }

        Normalized.MetaDataTypeDefinitionAbstract Normalized.IMetaDataBootstrap.ResolveType( Normalized.MetaDataAssembly asml      ,
                                                                                             string                      nameSpace ,
                                                                                             string                      name      )
        {
            return ResolveName( asml, nameSpace, name, null );
        }

        Normalized.MetaDataMethodBase Normalized.IMetaDataBootstrap.GetApplicationEntryPoint()
        {
            return this.ApplicationEntryPoint;
        }

        void Normalized.IMetaDataBootstrap.FilterTypesByCustomAttribute( Normalized.MetaDataTypeDefinitionAbstract                          filter   ,
                                                                         Normalized.IMetaDataBootstrap_FilterTypesByCustomAttributeCallback callback )
        {
            foreach(Normalized.MetaDataAssembly asml in this.NormalizedAssemblies)
            {
                foreach(Normalized.MetaDataTypeDefinitionAbstract td in asml.Types)
                {
                    Normalized.MetaDataCustomAttribute[] caArray = td.CustomAttributes;

                    if(caArray != null)
                    {
                        foreach(Normalized.MetaDataCustomAttribute ca in caArray)
                        {
                            Normalized.MetaDataMethodBase md = ca.Constructor as Normalized.MetaDataMethodBase;

                            if(md != null && md.Owner == filter)
                            {
                                callback( td, ca );
                            }
                        }
                    }
                }
            }
        }

        //--//

        public void ResolveAll()
        {
            MetaDataNormalizationContext context = new MetaDataNormalizationContext( this );
            MetaDataNormalizationPhase   phase;

#if METADATARESOLVER_DEBUG
            System.IO.TextWriter   orig   = Console.Out;
            System.IO.StreamWriter writer = new System.IO.StreamWriter( Environment.ExpandEnvironmentVariables( @"%DEPOTROOT%\ZeligUnitTestResults\MetaDataResolver.txt" ), false, System.Text.Encoding.ASCII );
            Console.SetOut( writer );
#endif

            for(phase = MetaDataNormalizationPhase.ResolutionOfAssemblyReferences; phase < MetaDataNormalizationPhase.Max; phase++)
            //for(phase = MetaDataNormalizationPhase.ResolutionOfAssemblyReferences; phase < MetaDataNormalizationPhase.ResolutionOfCustomAttributes; phase++)
            {
                RunPhase( context, phase );
            }

#if METADATARESOLVER_DEBUG
            Console.SetOut( orig );
            writer.Close();
#endif
        }

        private void RunPhase( MetaDataNormalizationContext context ,
                               MetaDataNormalizationPhase   phase   )
        {
            context.SetPhase( phase );

            if(phase == MetaDataNormalizationPhase.ResolutionOfAssemblyReferences)
            {
                while(m_metaDataInitialSet.Count > 0)
                {
                    Importer.MetaData md = m_metaDataInitialSet[0]; m_metaDataInitialSet.RemoveAt( 0 );

                    EnqueueAssembly( md, context );
                }
            }
            else
            {
                bool fDone;

                do
                {
                    fDone = true;

                    foreach(AssemblyPair pair in m_metaDataList.ToArray())
                    {
                        Importer.MetaData imported = pair.Imported;

                        if(imported != null)
                        {
                            if(imported.ExecuteNormalizationPhase( pair, context ) == false)
                            {
                                fDone = false;
                                break;
                            }
                        }
                    }
                } while(fDone == false);
            }

            context.FlushErrors();
        }

        //--//

        private Exception Unresolvable( object obj )
        {
            return Importer.IllegalMetaDataFormatException.Create( "Cannot resolve reference: {0}", obj );
        }

        //--//

        internal AssemblyPair FindAssembly( Importer.MetaData md )
        {
            foreach(AssemblyPair pair in m_metaDataList)
            {
                if(pair.Imported == md)
                {
                    return pair;
                }
            }

            return null;
        }

        internal AssemblyPair FindAssembly( string                       name    ,
                                            MetaDataVersion              version ,
                                            MetaDataNormalizationContext context )
        {
            Importer.MetaData md = FindImporterAssembly( name, version );

            if(md == null)
            {
                md = m_metaDataResolverHelper.ResolveAssemblyReference( name, version );
                if(md == null)
                {
                    throw Importer.UnresolvedExternalReferenceException.Create( null, "Cannot resolve reference to assembly: {0} {1}", name, version );
                }
            }

            return EnqueueAssembly( md, context );
        }

        private AssemblyPair EnqueueAssembly( Importer.MetaData            md      ,
                                              MetaDataNormalizationContext context )
        {
            AssemblyPair pair = FindAssembly( md );

            if(pair == null)
            {
                pair = new AssemblyPair( md, null );

                m_metaDataList.Add( pair );

                MetaDataNormalizationPhase activePhase = context.Phase;
                MetaDataNormalizationPhase runPhase    = MetaDataNormalizationPhase.ResolutionOfAssemblyReferences;

                context.SetPhase( runPhase );  

                md.AllocateNormalizedObject( pair, context );

                while(++runPhase < activePhase)
                {
                    context.SetPhase( runPhase );  

                    while(md.ExecuteNormalizationPhase( pair, context ) == false)
                    {
                    }
                }

                context.SetPhase( activePhase );
            }

            return pair;
        }

        private Importer.MetaData FindImporterAssembly( string          name    ,
                                                        MetaDataVersion version )
        {
            foreach(AssemblyPair pair in m_metaDataList)
            {
                Importer.MetaData imported = pair.Imported;

                if(imported != null)
                {
                    Importer.MetaDataAssembly asml = imported.Assembly;

                    if(asml.Name == name)
                    {
                        //
                        // BUGBUG: This version check does not cover at all the complexity of versioning...
                        //
                        if(version == null || asml.Version.IsCompatible( version, false ))
                        {
                            return pair.Imported;
                        }
                    }
                }
            }

            for(int i = 0; i < m_metaDataInitialSet.Count; i++)
            {
                Importer.MetaData md = m_metaDataInitialSet[i];

                if(md.Assembly.Name == name)
                {
                    //
                    // BUGBUG: This version check does not cover at all the complexity of versioning...
                    //
                    if(version == null || md.Assembly.Version.IsCompatible( version, false ))
                    {
                        m_metaDataInitialSet.RemoveAt( i );

                        return md;
                    }
                }
            }

            return null;
        }
    }
}

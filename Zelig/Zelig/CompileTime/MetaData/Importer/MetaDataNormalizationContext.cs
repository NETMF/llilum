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

namespace Microsoft.Zelig.MetaData
{
    internal enum MetaDataNormalizationPhase
    {
        Uninitialized                      ,
                                       
        ResolutionOfAssemblyReferences     ,

        ConversionOfResources              ,
                                       
        CreationOfTypeDefinitions          ,
        LinkingOfNestedClasses             ,
        DiscoveryOfBuiltInTypes            ,
        ResolutionOfTypeReferences         ,
        CreationOfTypeHierarchy            ,
        CompletionOfTypeNormalization      ,
                                       
        CreationOfFieldDefinitions         ,
                                       
        CreationOfMethodDefinitions        ,
        CreationOfSpecialArrayMethods      ,
        CompletionOfMethodNormalization    ,

        CreationOfMethodImplDefinitions    ,
        CompletionOfMethodImplNormalization,

        ResolutionOfCustomAttributes       ,

        ResolutionOfEntryPoint             ,
        Max
    }

    [Flags]
    internal enum MetaDataNormalizationMode
    {
        LookupExisting  = 1,
        Allocate        = 2,

        Default         = LookupExisting | Allocate,
    }

    internal class MetaDataNormalizationContext
    {
        //
        // State
        //

        private MetaDataResolver             m_resolver;
        private MetaDataNormalizationPhase   m_phase;
                                            
        private MetaDataNormalizationContext m_outsideContext;
        private Normalized.IMetaDataObject   m_normalized;

        private List< Exception >            m_errors;

        //
        // Constructor Methods
        //

        internal MetaDataNormalizationContext( MetaDataResolver resolver )
        {
            m_resolver = resolver;
            m_errors   = new List< Exception >();
        }

        private MetaDataNormalizationContext( MetaDataNormalizationContext context    ,
                                              Normalized.IMetaDataObject   normalized )
        {
            m_resolver       = context.m_resolver;
            m_phase          = context.m_phase;
            m_errors         = context.m_errors;
            m_outsideContext = context;
            m_normalized     = normalized;
        }

        internal MetaDataNormalizationContext Push( Normalized.IMetaDataObject normalized )
        {
            return new MetaDataNormalizationContext( this, normalized );
        }

        internal MetaDataNormalizationContext Pop()
        {
            return m_outsideContext;
        }

        internal void SetPhase( MetaDataNormalizationPhase phase )
        {
            m_phase = phase;
        }

        internal void FlushErrors()
        {
            if(m_errors.Count == 0)
            {
                return;
            }

            Console.Error.WriteLine( "Failed to resolve external references:" );

            GrowOnlySet< string > errorsHistory = SetFactory.New< string >();
            int                   numErrors     = 0;


            foreach(var ex in m_errors)
            {
                Importer.UnresolvedExternalReferenceException ex2 = ex as Importer.UnresolvedExternalReferenceException;

                if(ex2 != null)
                {
                    string msg = ex.Message;

                    if(!errorsHistory.Insert( msg ))
                    {
                        Console.Error.WriteLine( "{0}", msg );
                        numErrors++;
                    }
                }
            }

            m_errors.Clear();

            throw Importer.SilentCompilationAbortException.Create( "Failed to resolve {0} external references:", numErrors );
        }

        //--//

        internal MetaDataNormalizationPhase Phase
        {
            get
            {
                return m_phase;
            }
        }

        internal Normalized.IMetaDataObject Value
        {
            get
            {
                return m_normalized;
            }
        }

        internal GrowOnlySet< Normalized.IMetaDataUnique > UniqueDictionary
        {
            get
            {
                return m_resolver.UniqueDictionary;
            }
        }

        //--//

        //[System.Diagnostics.DebuggerHidden]
        internal void GetNormalizedObject<S, D>(     S                         src  ,
                                                 out D                         dst  ,
                                                     MetaDataNormalizationMode mode )
            where S : Importer.IMetaDataObject
            where D : Normalized.IMetaDataObject
        {
            if(src != null)
            {
                try
                {
                    Normalized.IMetaDataObject res = m_resolver.GetNormalizedObject( (Importer.IMetaDataNormalize)src, this, mode );

                    if(res is D)
                    {
                        dst = (D)res;
                    }
                    else
                    {
                        throw Importer.IllegalMetaDataFormatException.Create( "Cannot resolve reference '{0}' as type '{1}'", src, typeof(D) );
                    }
                }
                catch(Exception ex)
                {
                    m_errors.Add( ex );

                    switch(Phase)
                    {
                        case MetaDataNormalizationPhase.ResolutionOfAssemblyReferences:
                        case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                        case MetaDataNormalizationPhase.ResolutionOfTypeReferences:
                        case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                            dst = default( D );
                            return;
                    }

                    throw;
                }
            }
            else
            {
                dst = default( D );
            }
        }

        //[System.Diagnostics.DebuggerHidden]
        internal void GetNormalizedObjectList<S, D>(     List<S>                   src  ,
                                                     out D[]                       dst  ,
                                                         MetaDataNormalizationMode mode )
            where S : Importer.IMetaDataObject
            where D : Normalized.IMetaDataObject
        {
            if(src != null)
            {
                dst = new D[src.Count];

                int i = 0;

                foreach(S s in src)
                {
                    D d;

                    this.GetNormalizedObject( s, out d, mode );

                    dst[i++] = d;
                }
            }
            else
            {
                dst = new D[0];
            }
        }

        //--//

        //[System.Diagnostics.DebuggerHidden]
        internal void GetNormalizedSignature<S, D>(     S                         src  ,
                                                    out D                         dst  ,
                                                        MetaDataNormalizationMode mode )
            where S : Importer.Signature
            where D : Normalized.MetaDataSignature
        {
            if(src != null)
            {
                try
                {
                    Normalized.IMetaDataObject res = m_resolver.GetNormalizedObject( (Importer.IMetaDataNormalizeSignature)src, this, mode );

                    if(res is D)
                    {
                        dst = (D)res;
                    }
                    else
                    {
                        throw Importer.IllegalMetaDataFormatException.Create( "Cannot resolve reference '{0}' as type '{1}'", src, typeof(D) );
                    }
                }
                catch(Exception ex)
                {
                    m_errors.Add( ex );

                    switch(Phase)
                    {
                        case MetaDataNormalizationPhase.ResolutionOfAssemblyReferences:
                        case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                        case MetaDataNormalizationPhase.ResolutionOfTypeReferences:
                        case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                            dst = default( D );
                            return;
                    }

                    throw;
                }
            }
            else
            {
                dst = default( D );
            }
        }

        //[System.Diagnostics.DebuggerHidden]
        internal void GetNormalizedSignatureArray<S, D>(     S[]                       src  ,
                                                         out D[]                       dst  ,
                                                             MetaDataNormalizationMode mode )
            where S : Importer.Signature
            where D : Normalized.MetaDataSignature
        {
            if(src != null)
            {
                dst = new D[src.Length];

                int i = 0;

                foreach(S s in src)
                {
                    D d;

                    this.GetNormalizedSignature( s, out d, mode );

                    dst[i++] = d;
                }
            }
            else
            {
                dst = null;
            }
        }

        //--//

        //[System.Diagnostics.DebuggerHidden]
        internal void ProcessPhase( Importer.IMetaDataObject src )
        {
            if(src != null)
            {
                m_resolver.ProcessPhase( src, this );
            }
        }

        //[System.Diagnostics.DebuggerHidden]
        internal void ProcessPhaseList<S>( List<S> src )
            where S : Importer.IMetaDataObject
        {
            if(src != null)
            {
                foreach(S s in src)
                {
                    this.ProcessPhase( s );
                }
            }
        }

        //--//

        internal MetaDataResolver.AssemblyPair FindAssembly( string          name    ,
                                                             MetaDataVersion version )
        {
            return m_resolver.FindAssembly( name, version, this );
        }

        internal Normalized.MetaDataTypeDefinitionBase ResolveName( String key )
        {
            return m_resolver.ResolveName( key, this );
        }

        internal void RegisterBuiltIn( Normalized.MetaDataTypeDefinition td          ,
                                       ElementTypes                      elementType )
        {
            m_resolver.RegisterBuiltIn( td, elementType );
        }

        internal Normalized.MetaDataTypeDefinition LookupBuiltIn( ElementTypes elementType )
        {
            return m_resolver.LookupBuiltIn( elementType );
        }

        //--//

        internal Normalized.MetaDataTypeDefinitionAbstract GetTypeFromContext()
        {
            MetaDataNormalizationContext context = this;

            while(context != null)
            {
                if(context.m_normalized is Normalized.MetaDataMethodBase)
                {
                    Normalized.MetaDataMethodBase res = (Normalized.MetaDataMethodBase)context.m_normalized;

                    return res.m_owner;
                }

                if(context.m_normalized is Normalized.MetaDataTypeDefinitionAbstract)
                {
                    return (Normalized.MetaDataTypeDefinitionAbstract)context.m_normalized;
                }

                context = context.m_outsideContext;
            }

            throw Importer.IllegalMetaDataFormatException.Create( "Cannot get type from context: {0}", m_normalized );
        }

        internal Normalized.MetaDataMethodAbstract GetMethodFromContext()
        {
            MetaDataNormalizationContext context = this;

            while(context != null)
            {
                if(context.m_normalized is Normalized.MetaDataMethodAbstract)
                {
                    return (Normalized.MetaDataMethodAbstract)context.m_normalized;
                }

                context = context.m_outsideContext;
            }

            return null;
        }

        internal Normalized.MetaDataAssembly GetAssemblyFromContext()
        {
            MetaDataNormalizationContext context = this;

            while(context != null)
            {
                if(context.m_normalized is Normalized.MetaDataAssembly)
                {
                    return (Normalized.MetaDataAssembly)context.m_normalized;
                }

                context = context.m_outsideContext;
            }

            throw Importer.IllegalMetaDataFormatException.Create( "Cannot get assembly from context: {0}", m_normalized );
        }

        internal Normalized.MetaDataAssembly GetAssemblyForDelayedParameters()
        {
            return m_resolver.GetAssemblyForDelayedParameters();
        }

        //--//

        internal void ImplementSpecialInterfaces( Normalized.MetaDataTypeDefinitionArray array )
        {
            Normalized.MetaDataTypeDefinitionGenericInstantiation tdItf;

            tdItf = InstantiateInterface( "System.Collections.Generic.IList`1", array );
            ImplementMethodsOnInterface( array, tdItf );
            array.m_interfaces = ArrayUtility.AddUniqueToArray( array.m_interfaces, (Normalized.MetaDataTypeDefinitionAbstract)tdItf );

            tdItf = InstantiateInterface( "System.Collections.Generic.ICollection`1", array );
            ImplementMethodsOnInterface( array, tdItf );
            array.m_interfaces = ArrayUtility.AddUniqueToArray( array.m_interfaces, (Normalized.MetaDataTypeDefinitionAbstract)tdItf );

            tdItf = InstantiateInterface( "System.Collections.Generic.IEnumerable`1", array );
            ImplementMethodsOnInterface( array, tdItf );
        }

        internal Normalized.MetaDataTypeDefinitionGenericInstantiation InstantiateInterface( string                                 name  ,
                                                                                             Normalized.MetaDataTypeDefinitionArray array )
        {
            Normalized.MetaDataTypeDefinitionGeneric itf  = (Normalized.MetaDataTypeDefinitionGeneric)this.ResolveName( name );

            return InstantiateInterface( itf, array );
        }

        internal Normalized.MetaDataTypeDefinitionGenericInstantiation InstantiateInterface( Normalized.MetaDataTypeDefinitionGeneric gen   ,
                                                                                             Normalized.MetaDataTypeDefinitionArray   array )
        {
            Normalized.MetaDataTypeDefinitionGenericInstantiation genInst = new Normalized.MetaDataTypeDefinitionGenericInstantiation( array.Owner, 0 );

            genInst.m_baseType      = gen;
            genInst.m_parameters    = new Normalized.SignatureType[1];
            genInst.m_parameters[0] = Normalized.SignatureType.CreateUnique( array.m_objectType );

            return (Normalized.MetaDataTypeDefinitionGenericInstantiation)genInst.MakeUnique();
        }

        internal void ImplementMethodsOnInterface( Normalized.MetaDataTypeDefinitionArray    cls ,
                                                   Normalized.MetaDataTypeDefinitionAbstract itf )
        {
            if(itf is Normalized.MetaDataTypeDefinitionGenericInstantiation)
            {
                Normalized.MetaDataTypeDefinitionGenericInstantiation itfInst = (Normalized.MetaDataTypeDefinitionGenericInstantiation)itf;

                foreach(Normalized.MetaDataMethodBase md in itfInst.m_baseType.m_methods)
                {
                    Normalized.SignatureType   returnType;
                    Normalized.SignatureType[] parameters;

                    returnType = Substitute( md.m_signature.m_returnType, cls );

                    if(md.m_signature.m_parameters != null)
                    {
                        parameters = new Normalized.SignatureType[md.m_signature.m_parameters.Length];

                        for(int i = 0; i < md.m_signature.m_parameters.Length; i++)
                        {
                            parameters[i] = Substitute( md.m_signature.m_parameters[i], cls );
                        }
                    }
                    else
                    {
                        parameters = null;
                    }

                    Normalized.MetaDataMethodBase mdNew = CreateMethod( cls, md.m_name, returnType, parameters );

                    cls.m_methods = ArrayUtility.AddUniqueToArray( cls.m_methods, mdNew );
                }
            }
        }

        internal Normalized.SignatureType Substitute( Normalized.SignatureType               td  ,
                                                      Normalized.MetaDataTypeDefinitionArray cls )
        {
            Normalized.MetaDataTypeDefinitionAbstract tdInner = td.Type;

            if(tdInner.IsOpenType)
            {
                if(tdInner is Normalized.MetaDataTypeDefinitionDelayed)
                {
                    return Normalized.SignatureType.CreateUnique( cls.m_objectType );
                }

                if(tdInner is Normalized.MetaDataTypeDefinitionArraySz)
                {
                    Normalized.MetaDataTypeDefinitionArraySz array = (Normalized.MetaDataTypeDefinitionArraySz)tdInner;

                    if(array.IsOpenType && array.m_objectType is Normalized.MetaDataTypeDefinitionDelayed)
                    {
                        return Normalized.SignatureType.CreateUnique( cls );
                    }
                }

                if(tdInner is Normalized.MetaDataTypeDefinitionGenericInstantiation)
                {
                    Normalized.MetaDataTypeDefinitionGenericInstantiation inst = (Normalized.MetaDataTypeDefinitionGenericInstantiation)tdInner;

                    return Normalized.SignatureType.CreateUnique( InstantiateInterface( inst.GenericType, cls ) );
                }

                throw Importer.IllegalMetaDataFormatException.Create( "Unknown type in array instantiation: {0}", td );
            }

            return td;
        }

        //--//

        internal void CreateSpecialMethods( Normalized.MetaDataTypeDefinitionArrayMulti array )
        {
            //
            // Synthesize .ctor, Set, Get, and Address methods:
            //
            //               .ctor  ( <int32>*rank         )
            //  <type>       Get    ( <int32>*rank         )
            //  void         Set    ( <int32>*rank, <type> )
            //  ByRef <type> Address( <int32>*rank         )
            //

            Normalized.MetaDataTypeDefinitionByRef byref = new Normalized.MetaDataTypeDefinitionByRef( array.m_owner, 0, ElementTypes.BYREF );

            byref.m_type =  array.m_objectType;

            byref = (Normalized.MetaDataTypeDefinitionByRef)byref.MakeUnique();

            //--//

            Normalized.SignatureType objectType = Normalized.SignatureType.CreateUnique( array.m_objectType );
            Normalized.SignatureType byrefSig   = Normalized.SignatureType.CreateUnique( byref );

            array.m_methods = new Normalized.MetaDataMethodBase[4];

            array.m_methods[0] = CreateMultiArrayMethodVoid( array, ".ctor"                           );
            array.m_methods[1] = CreateMultiArrayMethod    ( array, "Get"    , objectType             );
            array.m_methods[2] = CreateMultiArrayMethodVoid( array, "Set"                , objectType );
            array.m_methods[3] = CreateMultiArrayMethod    ( array, "Address", byrefSig               );
        }

        private Normalized.MetaDataMethodBase CreateMultiArrayMethodVoid(        Normalized.MetaDataTypeDefinitionArrayMulti owner      ,
                                                                                 string                                      name       ,
                                                                          params Normalized.SignatureType[]                  parameters )

        {
            Normalized.MetaDataTypeDefinitionAbstract returnType = this.LookupBuiltIn( ElementTypes.VOID );

            return CreateMultiArrayMethod( owner, name, Normalized.SignatureType.CreateUnique( returnType ), parameters );
        }

        private Normalized.MetaDataMethodBase CreateMultiArrayMethod(        Normalized.MetaDataTypeDefinitionArrayMulti owner      ,
                                                                             string                                      name       ,
                                                                             Normalized.SignatureType                    returnType ,
                                                                      params Normalized.SignatureType[]                  parameters )
        {
            int                                       rank  = (int)owner.m_rank;
            Normalized.MetaDataTypeDefinitionAbstract index = this.LookupBuiltIn( ElementTypes.I4 );
            Normalized.SignatureType[]                parameters2;

            parameters2 = new Normalized.SignatureType[rank + parameters.Length];

            for(int i = 0; i < rank; i++)
            {
                parameters2[i] = Normalized.SignatureType.CreateUnique( index );
            }

            for(int i = 0; i < parameters.Length; i++)
            {
                parameters2[rank+i] = parameters[i];
            }

            return CreateMethod( owner, name, returnType, parameters2 );
        }

        internal Normalized.MetaDataMethodBase CreateMethod(        Normalized.MetaDataTypeDefinitionAbstract owner      ,
                                                                    string                                    name       ,
                                                                    Normalized.SignatureType                  returnType ,
                                                             params Normalized.SignatureType[]                parameters )
        {
            Normalized.SignatureMethod sig = new Normalized.SignatureMethod( 0 );

            sig.m_callingConvention = Normalized.SignatureMethod.CallingConventions.HasThis;
            sig.m_returnType        = returnType;
            sig.m_parameters        = parameters;

            //--//

            Normalized.MetaDataMethodBase mdNew = new Normalized.MetaDataMethod( owner, 0 );

            //--//

            mdNew.m_implFlags = MethodImplAttributes.Managed | MethodImplAttributes.InternalCall;
            mdNew.m_flags     = MethodAttributes.Public        |
                                MethodAttributes.HideBySig     |
                                MethodAttributes.SpecialName   |
                                MethodAttributes.RTSpecialName;
            mdNew.m_name      = name;
            mdNew.m_signature = (Normalized.SignatureMethod)sig.MakeUnique();

            return mdNew;
        }

        //--//

        internal Exception InvalidPhase( object src )
        {
            return Importer.IllegalMetaDataFormatException.Create( "InvalidPhase at {0}", src );
        }
    }
}

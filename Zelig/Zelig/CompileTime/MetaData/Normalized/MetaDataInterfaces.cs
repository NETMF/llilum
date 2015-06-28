//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.Normalized
{
    using System.Collections.Generic;

    //
    // Every class implementing this interface provides a proper implementation of the Equals and GetHashCode methods.
    //
    // The normalizer uses this property to guarantee that there's a single instantiation of an object,
    // so the rest of the system can use referential equality to compare metadata objects.
    //
    public interface IMetaDataUnique
    {
    }

    //
    // Many metadata tables contain tokens of multiple types, see CodeToken.
    //
    // To make the relationships between tables more explicit, instead of using a reference to MetaDataObject,
    // the classes that represent the various tables use references to these interfaces.
    //

    public interface IMetaDataObject
    {
    }

    public interface IMetaDataTypeDefOrRef : IMetaDataObject
    {
    }

    public interface IMetaDataHasConstant : IMetaDataObject
    {
    }

    public interface IMetaDataHasCustomAttribute : IMetaDataObject
    {
    }

    public interface IMetaDataHasFieldMarshal : IMetaDataObject
    {
    }

    public interface IMetaDataHasDeclSecurity : IMetaDataObject
    {
    }

    public interface IMetaDataMemberRefParent : IMetaDataObject
    {
    }

    public interface IMetaDataHasSemantic : IMetaDataObject
    {
    }

    public interface IMetaDataMethodDefOrRef : IMetaDataObject
    {
    }

    public interface IMetaDataMemberForwarded : IMetaDataObject
    {
    }

    public interface IMetaDataCustomAttributeType : IMetaDataObject
    {
    }

    public interface IMetaDataImplementation : IMetaDataObject
    {
    }

    public interface IMetaDataResolutionScope : IMetaDataObject
    {
    }

    public interface IMetaDataTypeOrMethodDef : IMetaDataObject
    {
    }


    public delegate void IMetaDataBootstrap_FilterTypesByCustomAttributeCallback( MetaDataTypeDefinitionAbstract td, MetaDataCustomAttribute ca );


    //
    // Bootstrapping interface for the type system.
    //
    public interface IMetaDataBootstrap
    {
        MetaDataAssembly GetAssembly( string name );

        MetaDataTypeDefinitionAbstract ResolveType( MetaDataAssembly asml, string nameSpace, string name );

        MetaDataMethodBase GetApplicationEntryPoint();

        void FilterTypesByCustomAttribute( MetaDataTypeDefinitionAbstract filter, IMetaDataBootstrap_FilterTypesByCustomAttributeCallback callback );
    }

    //
    // Debug classes
    //
    public interface IMetaDataDumper
    {
        void IndentPush( string s );
        void IndentPop ( string s );

        void WriteLine();

        void WriteLine( string s );

        void WriteLine( string s    ,
                        object arg1 );

        void WriteLine( string s    ,
                        object arg1 ,
                        object arg2 );

        void WriteLine( string s    ,
                        object arg1 ,
                        object arg2 ,
                        object arg3 );

        void WriteLine(        string   s    ,
                        params object[] args );

        void Process( MetaDataObject obj       ,
                      bool           fOnlyOnce );

        bool AlreadyProcessed( MetaDataObject obj );

        MetaDataMethodAbstract         GetContextMethodAndPop( out IMetaDataDumper context );
        MetaDataTypeDefinitionAbstract GetContextTypeAndPop  ( out IMetaDataDumper context );

        IMetaDataDumper PushContext( MetaDataObject obj );
    }
}

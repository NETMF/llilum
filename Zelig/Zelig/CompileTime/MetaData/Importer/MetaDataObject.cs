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
    //
    // Many metadata tables contain tokens of multiple types, see CodeToken.
    //
    // To make the relationships between tables more explicit, instead of using a reference to MetaDataObject,
    // the classes that represent the various tables use references to these interfaces.
    //
    public interface IMetaDataObject
    {
    }

    //
    // MetaDataTypeDefinition, MetaDataTypeReference, MetaDataTypeSpec
    //
    public interface IMetaDataTypeDefOrRef : IMetaDataObject
    {
    }

    //
    // MetaDataProperty, MetaDataField, MetaDataParam
    //
    public interface IMetaDataHasConstant : IMetaDataObject
    {
    }

    //
    // MetaDataObject
    //
    public interface IMetaDataHasCustomAttribute : IMetaDataObject
    {
    }

    //
    // MetaDataField, MetaDataParam
    //
    public interface IMetaDataHasFieldMarshal : IMetaDataObject
    {
    }

    //
    // MetaDataTypeDefinition, MetaDataMethod, MetaDataAssembly
    //
    public interface IMetaDataHasDeclSecurity : IMetaDataObject
    {
    }

    //
    // MetaDataTypeReference, MetaDataTypeSpec
    //
    public interface IMetaDataMemberRefParent : IMetaDataObject
    {
    }

    //
    // MetaDataProperty, MetaDataEvent
    //
    public interface IMetaDataHasSemantic : IMetaDataObject
    {
    }

    //
    // MetaDataMethod, MetaDataMemberRef
    //
    public interface IMetaDataMethodDefOrRef : IMetaDataObject
    {
    }

    //
    // MetaDataMethod, MetaDataField
    //
    public interface IMetaDataMemberForwarded : IMetaDataObject
    {
    }

    //
    // MetaDataMethod, MetaDataMemberRef
    //
    public interface IMetaDataCustomAttributeType : IMetaDataObject
    {
    }

    //
    // MetaDataAssemblyRef, MetaDataFile
    //
    public interface IMetaDataImplementation : IMetaDataObject
    {
    }

    //
    // MetaDataAssemblyRef, MetaDataTypeReference, MetaDataModule, MetaDataModuleRef
    //
    public interface IMetaDataResolutionScope : IMetaDataObject
    {
    }

    //
    // MetaDataTypeDefinition, MetaDataMethod
    //
    public interface IMetaDataTypeOrMethodDef : IMetaDataObject
    {
    }

    internal interface IMetaDataNormalize
    {
        Normalized.MetaDataObject AllocateNormalizedObject( MetaDataNormalizationContext context );

        void ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                        MetaDataNormalizationContext context );
    }

    internal interface IMetaDataNormalizeSignature
    {
        Normalized.MetaDataSignature AllocateNormalizedObject( MetaDataNormalizationContext context );
    }

    //--//

    //
    // This class's primary reason for existence is its place in the class hierarchy.
    //
    // Convenient place to store CustomAttribute information
    //
    public abstract class MetaDataObject : IMetaDataHasCustomAttribute
    {
        internal delegate MetaDataObject CreateInstance( int token );

        internal static int s_debugSequence;

        //
        // State
        //

        internal  int                           m_debugSequence;
        protected int                           m_token;
        private   List<MetaDataCustomAttribute> m_customAttributes;

        //
        // Constructor Methods
        //

        protected MetaDataObject( TokenType tbl   ,
                                  int       index )
        {
            m_debugSequence = s_debugSequence++;

            m_token = MetaData.PackToken( tbl, index );
        }

        internal abstract void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader );

        //--//

        internal void AddCustomAttribute( MetaDataCustomAttribute ca )
        {
            if(m_customAttributes == null)
            {
                m_customAttributes = new List<MetaDataCustomAttribute>( 2 );
            }

            m_customAttributes.Add( ca );
        }

        //--//

        public int Token
        {
            get
            {
                return m_token;
            }
        }

        public List<MetaDataCustomAttribute> CustomAttributes
        {
            get
            {
                return m_customAttributes;
            }
        }

        //
        // Debug Methods
        //

        public virtual String ToStringLong()
        {
            return this.ToString();
        }

        // The name of the meta data object, in a user-comprehensible
        // form.

        public virtual String FullName
        {
            get
            {
                return this.ToString();
            }
        }

        // The name of the meta data object, in a user-comprehensible
        // form, along with the kind of object.
        //
        // For example, a method A.B.f would be printed out as:
        //
        //    method A.B.f

        public virtual String FullNameWithContext
        {
            get
            {
                return this.ToString();
            }
        }
    }
}

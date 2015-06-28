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
    public sealed class MetaDataModuleRef : MetaDataObject,
        IMetaDataResolutionScope,
        IMetaDataNormalize
    {
        //
        // State
        //

        private String m_name;

        //
        // Constructor Methods
        //

        private MetaDataModuleRef( int index ) : base( TokenType.ModuleRef, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataModuleRef( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            m_name = parser.readIndexAsString( reader );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
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

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataModuleRef(" + m_name + ")";
        }
    }
}

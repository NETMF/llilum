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
    public sealed class MetaDataEvent : MetaDataObject,
        IMetaDataHasSemantic,
        IMetaDataNormalize
    {
        //
        // State
        //

        private EventAttributes        m_flags;
        private String                 m_name;
        private IMetaDataTypeDefOrRef  m_eventType;
        private MetaDataTypeDefinition m_owner;

        //
        // Constructor Methods
        //

        private MetaDataEvent( int index ) : base( TokenType.Event, index )
        {
        }

        // Helper methods to work around limitations in generics, see Parser.InitializeTable<T>

        internal static MetaDataObject.CreateInstance GetCreator()
        {
            return new MetaDataObject.CreateInstance( Creator );
        }

        private static MetaDataObject Creator( int index )
        {
            return new MetaDataEvent( index );
        }

        //--//

        internal override void Parse( Parser             parser ,
                                      Parser.TableSchema ts     ,
                                      ArrayReader        reader )
        {
            Parser.IndexReader eventTypeReader = ts.m_columns[2].m_reader;
            int                eventTypeIndex;

            m_flags        = (EventAttributes)         reader.ReadInt16();
            m_name         = parser.readIndexAsString( reader );
            eventTypeIndex =        eventTypeReader  ( reader );

            m_eventType = parser.getTypeDefOrRef( eventTypeIndex );

            m_owner = parser.GetTypeFromEventIndex( MetaData.UnpackTokenAsIndex( m_token ) );

            m_owner.AddEvent( this );
        }

        //
        // IMetaDataNormalize methods
        //

        Normalized.MetaDataObject IMetaDataNormalize.AllocateNormalizedObject( MetaDataNormalizationContext context )
        {
            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.CompletionOfMethodNormalization:
                    {
                        Normalized.MetaDataEvent eventNew = new Normalized.MetaDataEvent( context.GetTypeFromContext(), m_token );

                        eventNew.m_flags = m_flags;
                        eventNew.m_name  = m_name;

                        context.GetNormalizedObject( m_eventType, out eventNew.m_eventType, MetaDataNormalizationMode.Default );

                        return eventNew.MakeUnique();
                    }
            }

            throw context.InvalidPhase( this );
        }

        void IMetaDataNormalize.ExecuteNormalizationPhase( Normalized.IMetaDataObject   obj     ,
                                                           MetaDataNormalizationContext context )
        {
            Normalized.MetaDataEvent ev = (Normalized.MetaDataEvent)obj;

            context = context.Push( obj );

            switch(context.Phase)
            {
                case MetaDataNormalizationPhase.ResolutionOfCustomAttributes:
                    {
                        context.GetNormalizedObjectList( this.CustomAttributes, out ev.m_customAttributes, MetaDataNormalizationMode.Allocate );
                    }
                    return;
            }

            throw context.InvalidPhase( this );
        }

        //
        // Access Methods
        //

        public EventAttributes Flags
        {
            get
            {
                return m_flags;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
        }

        // Returns one of MetaData{TypeDef,TypeRef,TypeSpec}
        public IMetaDataTypeDefOrRef Type
        {
            get
            {
                return m_eventType;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            return "MetaDataEvent(" + m_flags + "," + m_name + "," + m_eventType + ")";
        }
    }
}

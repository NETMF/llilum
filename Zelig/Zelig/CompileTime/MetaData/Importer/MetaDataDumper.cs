//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

namespace Microsoft.Zelig.MetaData
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Reflection;
    using System.Text;

    public class MetaDataDumper : Normalized.IMetaDataDumper, IDisposable
    {
        //
        // State
        //

        MetaDataDumper                           m_sub;
        System.IO.TextWriter                     m_writer;
        int                                      m_indent;
        GrowOnlySet< Normalized.MetaDataObject > m_processed;
        Normalized.MetaDataObject                m_context;

        //
        // Constructor Methods
        //

        public MetaDataDumper( string          name ,
                               MetaDataVersion ver  )
        {
            name = name.Replace( "<", "" );
            name = name.Replace( ">", "" );
            name = name.Replace( ":", "" );

            m_sub       = null;
            m_writer    = new System.IO.StreamWriter( String.Format( "{0}_{1}.{2}.{3}.{4}.txt", name, ver.MajorVersion, ver.MinorVersion, ver.BuildNumber, ver.RevisionNumber ), false, Encoding.ASCII );
            m_indent    = 0;
            m_processed = SetFactory.NewWithReferenceEquality< Normalized.MetaDataObject >();
            m_context   = null;
        }

        private MetaDataDumper( MetaDataDumper            sub     ,
                                Normalized.MetaDataObject context )
        {
            m_sub       = sub;
            m_writer    = sub.m_writer;
            m_indent    = sub.m_indent;
            m_processed = sub.m_processed;
            m_context   = context;
        }

        public void IndentPush( string s )
        {
            WriteLine( s );

            m_indent += 1;
        }

        public void IndentPop( string s )
        {
            m_indent -= 1;

            WriteLine( s );
        }

        public void WriteLine()
        {
            WriteIndentation();

            m_writer.WriteLine();
        }

        public void WriteLine( string s )
        {
            WriteIndentation();

            m_writer.WriteLine( s );
        }

        public void WriteLine( string s    ,
                               object arg1 )
        {
            WriteIndentedLine( s, arg1 );
        }

        public void WriteLine( string s    ,
                               object arg1 ,
                               object arg2 )
        {
            WriteIndentedLine( s, arg1, arg2 );
        }

        public void WriteLine( string s    ,
                               object arg1 ,
                               object arg2 ,
                               object arg3 )
        {
            WriteIndentedLine( s, arg1, arg2, arg3 );
        }

        public void WriteLine(        string   s    ,
                               params object[] args )
        {
            WriteIndentedLine( s, args );
        }

        public void Process( Normalized.MetaDataObject obj       ,
                             bool                      fOnlyOnce )
        {
            if(fOnlyOnce == false || m_processed.Contains( obj ) == false)
            {
                m_processed.Insert( obj );

                obj.Dump( this );
            }
        }

        public bool AlreadyProcessed( Normalized.MetaDataObject obj )
        {
            return m_processed.Contains( obj );
        }

        public Normalized.MetaDataMethodAbstract GetContextMethodAndPop( out Normalized.IMetaDataDumper context )
        {
            MetaDataDumper pThis = this;

            while(pThis != null)
            {
                Normalized.MetaDataObject obj = pThis.m_context;

                if(obj is Normalized.MetaDataMethodAbstract)
                {
                    Normalized.MetaDataMethodAbstract md = (Normalized.MetaDataMethodAbstract)obj;

                    context = pThis.m_sub;

                    if(obj is Normalized.MetaDataMethodGeneric)
                    {
                        while(pThis != null)
                        {
                            if(pThis.m_context is Normalized.MetaDataMethodGenericInstantiation)
                            {
                                Normalized.MetaDataMethodGenericInstantiation inst = (Normalized.MetaDataMethodGenericInstantiation)pThis.m_context;

                                if(inst.BaseMethod.Equals( md ))
                                {
                                    context = pThis.m_sub;
                                    md      = inst;
                                    break;
                                }
                            }

                            pThis = pThis.m_sub;
                        }
                    }

                    return md;
                }

                pThis = pThis.m_sub;
            }

            context = null;

            return null;
        }

        public Normalized.MetaDataTypeDefinitionAbstract GetContextTypeAndPop( out Normalized.IMetaDataDumper context )
        {
            MetaDataDumper pThis = this;

            while(pThis != null)
            {
                Normalized.MetaDataObject obj = pThis.m_context;

                if(obj is Normalized.MetaDataTypeDefinitionAbstract)
                {
                    Normalized.MetaDataTypeDefinitionAbstract td = (Normalized.MetaDataTypeDefinitionAbstract)obj;

                    context = pThis.m_sub;

                    if(obj is Normalized.MetaDataTypeDefinitionGeneric)
                    {
                        while(pThis != null)
                        {
                            if(pThis.m_context is Normalized.MetaDataTypeDefinitionGenericInstantiation)
                            {
                                Normalized.MetaDataTypeDefinitionGenericInstantiation inst = (Normalized.MetaDataTypeDefinitionGenericInstantiation)pThis.m_context;

                                if(inst.GenericType.Equals( td ))
                                {
                                    context = pThis.m_sub;
                                    td      = inst;
                                    break;
                                }
                            }

                            pThis = pThis.m_sub;
                        }
                    }

                    return td;
                }

                pThis = pThis.m_sub;
            }

            context = null;

            return null;
        }

        public Normalized.IMetaDataDumper PushContext( Normalized.MetaDataObject obj )
        {
            return new MetaDataDumper( this, obj );
        }

        //--//

        private void WriteIndentedLine(        string   s    ,
                                        params object[] args )
        {
            WriteIndentation();

            m_writer.WriteLine( s, args );
        }

        private void WriteIndentation()
        {
            for(int i = 0; i < m_indent; i++)
            {
                m_writer.Write( "  " );
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if(m_writer != null)
            {
                m_writer.Dispose();

                m_writer = null;
            }
        }

        #endregion
    }
}

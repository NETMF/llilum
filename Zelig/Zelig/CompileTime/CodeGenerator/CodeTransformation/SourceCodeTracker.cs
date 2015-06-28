//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class SourceCodeTracker : ITransformationContextTarget
    {
        public sealed class SourceCode : ITransformationContextTarget
        {
            //
            // State
            //

            string                   m_file;
            List< string >           m_lines;
            Microsoft.CSharp.Token[] m_tokens;
            bool                     m_fCached;

            //
            // Constructor Methods
            //

            public SourceCode( string file )
            {
                m_file  = file;
                m_lines = new List<string>();

                using(System.IO.StreamReader stream = new System.IO.StreamReader( file ))
                {
                    string line;

                    while((line = stream.ReadLine()) != null)
                    {
                        m_lines.Add( line );
                    }
                }
            }

            //
            // Helper Methods
            //

            void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
            {
                context.Push( this );

                context.Transform( ref m_file    );
                context.Transform( ref m_lines   );
                context.Transform( ref m_fCached );

                context.Pop();
            }

            public string[] ToArray()
            {
                return m_lines.ToArray();
            }

            //
            // Access Methods
            //

            public string File
            {
                get
                {
                    return m_file;
                }
            }

            public bool UsingCachedValues
            {
                get
                {
                    return m_fCached;
                }

                set
                {
                    m_fCached = value;
                }
            }

            public int Count
            {
                get
                {
                    return m_lines.Count;
                }
            }

            public string this[int line]
            {
                get
                {
                    if(line >= 1 && line <= m_lines.Count)
                    {
                        return m_lines[line-1];
                    }

                    return string.Empty;
                }
            }

            public Microsoft.CSharp.Token[] Tokens
            {
                get
                {
                    if(m_tokens == null)
                    {
                        Microsoft.CSharp.NameTable nameTable = new Microsoft.CSharp.NameTable();
                        Microsoft.CSharp.LineMap   lineMap   = new Microsoft.CSharp.LineMap( m_file );

                        Microsoft.CSharp.FileLexer lexer = new Microsoft.CSharp.FileLexer( nameTable );

                        string newLine    = Environment.NewLine;
                        int    newLineLen = newLine.Length;
                        int    len        = 0;

                        for(int line = 0; line < m_lines.Count; line++)
                        {
                            len += m_lines[line].Length + newLineLen;
                        }

                        System.Text.StringBuilder buffer =  new System.Text.StringBuilder( len );

                        for(int line = 0; line < m_lines.Count; line++)
                        {
                            buffer.Append( m_lines[line] );
                            buffer.Append( newLine       );
                        }

                        m_tokens = lexer.Lex( buffer.ToString().ToCharArray(), new System.Collections.Hashtable(), lineMap, true );
                    }

                    return m_tokens;
                }
            }
        }

        public delegate void EmitLine( string format, params object[] args );

        //
        // State
        //

        GrowOnlyHashTable< string, SourceCode > m_lookupSourceCode;
        SourceCode                              m_lastSc;
        Debugging.DebugInfo                     m_lastDebugInfo;
        int                                     m_extraLinesToOutput;

        //
        // Constructor Methods
        //

        public SourceCodeTracker()
        {
            m_lookupSourceCode = HashTableFactory.New< string, SourceCode >();
        }

        //--//

        //
        // Helper Methods
        //

        void ITransformationContextTarget.ApplyTransformation( TransformationContext context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            context2.Transform( ref m_lookupSourceCode );

            context2.Pop();
        }

        public void Merge( SourceCodeTracker sct )
        {
            m_lookupSourceCode.Merge( sct.m_lookupSourceCode );
        }

        public void ResetContext()
        {
            m_lastDebugInfo = null;
        }

        public void Print( Debugging.DebugInfo dbg      ,
                           EmitLine            callback )
        {
            if(dbg != null)
            {
                SourceCode sc = GetSourceCode( dbg.SrcFileName );
                if(sc != null)
                {
                    if(sc != m_lastSc)
                    {
                        callback( "=== {0} ====", dbg.SrcFileName );

                        m_lastSc        = sc;
                        m_lastDebugInfo = null;
                    }

                    if(m_lastDebugInfo != dbg)
                    {
                        int lineNo = dbg.BeginLineNumber;

                        for(int offset = lineNo - m_extraLinesToOutput; offset <= dbg.EndLineNumber; offset++)
                        {
                            string line = sc[offset];

                            if(line != null)
                            {
                                callback( "{0} {1}", offset, line );

                                if(offset == lineNo)
                                {
                                    callback( "{0} {1}^", offset, new string( ' ', dbg.BeginColumn-1 ) );
                                }
                            }
                        }

                        m_lastDebugInfo      = dbg;
                        m_extraLinesToOutput = 0;
                    }
                }
            }
        }

        public SourceCode GetSourceCode( string file )
        {
            SourceCode sc;

            if(m_lookupSourceCode.TryGetValue( file, out sc ))
            {
                return sc;
            }

            try
            {
                if(System.IO.File.Exists( file ))
                {
                    sc = new SourceCode( file );
                }
                else
                {
                    sc = null;
                }
            }
            catch
            {
                sc = null;
            }

            m_lookupSourceCode[file] = sc;

            return sc;
        }

        //
        // Access Methods
        //

        public int ExtraLinesToOutput
        {
            set
            {
                m_extraLinesToOutput = value;
            }
        }
    }
}
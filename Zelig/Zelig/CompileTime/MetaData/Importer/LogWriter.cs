//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public class LogWriter
    {
        //
        // State
        //

        System.IO.TextWriter m_writer;

        //
        // Constructor Methods
        //

        public LogWriter( System.IO.TextWriter writer )
        {
            m_writer = writer;
        }

        public void WriteLine()
        {
            m_writer.WriteLine();
        }

        public void WriteLine( string s )
        {
            m_writer.WriteLine( s );
        }

        public void WriteLine( string s    ,
                               object arg1 )
        {
            m_writer.WriteLine( s, arg1 );
        }

        public void WriteLine( string s    ,
                               object arg1 ,
                               object arg2 )
        {
            m_writer.WriteLine( s, arg1, arg2 );
        }

        public void WriteLine( string s    ,
                               object arg1 ,
                               object arg2 ,
                               object arg3 )
        {
            m_writer.WriteLine( s, arg1, arg2, arg3 );
        }

        public void Flush()
        {
            m_writer.Flush();
        }
    }
}

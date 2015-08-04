//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Tools.IRViewer
{
    using System;
    using System.Collections.Generic;

    public sealed class SourceCodeTracker
    {
        public sealed class SourceCode
        {
            readonly List<string> _lines;

            public SourceCode(string file)
            {
                File = file;
                _lines = new List<string>();

                using (var stream = new System.IO.StreamReader(file))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        _lines.Add(line);
                    }
                }
            }

            public string File { get; }

            public int Count => _lines.Count;

            public string this[int line]
            {
                get
                {
                    if (line >= 1 && line <= _lines.Count)
                    {
                        return _lines[line - 1];
                    }

                    return string.Empty;
                }
            }
        }

        GrowOnlyHashTable<string, SourceCode> _lookupSourceCode;

        public SourceCodeTracker()
        {
            _lookupSourceCode = HashTableFactory.New<string, SourceCode>();
        }

        public SourceCode GetSourceCode(string file)
        {
            SourceCode sc = null;

            if (_lookupSourceCode.TryGetValue(file, out sc))
            {
                return sc;
            }

            if (System.IO.File.Exists(file))
            {
                try
                {
                    sc = new SourceCode(file);
                }
                catch
                {
                    // ignored
                }
            }

            _lookupSourceCode[file] = sc;

            return sc;
        }
    }
}

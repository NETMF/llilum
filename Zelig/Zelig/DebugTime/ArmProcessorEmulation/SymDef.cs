//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class SymDef
    {
        public class SymbolToAddressMap : Dictionary< string, uint >
        {
        }

        public class AddressToSymbolMap : Dictionary< uint, string >
        {
            class AddressComparer : IComparer< KeyValuePair< uint, string > >
            {
                public int Compare( KeyValuePair< uint, string > x ,
                                    KeyValuePair< uint, string > y )
                {
                    return x.Key.CompareTo( y.Key );
                }
            }

            //
            // State
            //

            AddressComparer                m_comparer;
            KeyValuePair< uint, string >[] m_sorted;

            //--///

            public bool FindClosestAddress(     uint address ,
                                            out uint context )
            {
                if(m_comparer == null)
                {
                    m_comparer = new AddressComparer();
                }

                if(m_sorted == null)
                {
                    m_sorted = new KeyValuePair<uint,string>[this.Count];

                    int pos = 0;

                    foreach(KeyValuePair< uint, string> kvp in this)
                    {
                        m_sorted[pos++] = kvp;
                    }

                    Array.Sort( m_sorted, m_comparer );
                }

                if(m_sorted.Length > 0)
                {
                    int i = Array.BinarySearch( m_sorted, new KeyValuePair< uint, string >( address, null ), m_comparer );

                    if(i >= 0)
                    {
                        context = address;
                        return true;
                    }

                    i = ~i - 1;
                    if(i >= 0 && i < m_sorted.Length)
                    {
                        context = m_sorted[i].Key;
                        return true;
                    }
                }

                context = 0;
                return false;
            }
        }

        //--//

        public static void Parse( string                    file           ,
                                  SymDef.SymbolToAddressMap symdef         ,
                                  SymDef.AddressToSymbolMap symdef_Inverse )
        {
            if(System.IO.File.Exists( file ) == false)
            {
                throw new System.IO.FileNotFoundException( String.Format( "Cannot find {0}", file ) );
            }

            using(System.IO.StreamReader reader = new StreamReader( file ))
            {
                Regex  reMatch1 = new Regex( "^0x([0-9a-fA-F]*) ([A-D]) (.*)"                   );
                Regex  reMatch2 = new Regex( "^        0x([0-9a-fA-F]*):    ([0-9a-fA-F]*)    " );
                Regex  reMatch3 = new Regex( "^    ([^ ]+)$"                                    );
                string lastName = null;
                string line;

                while((line = reader.ReadLine()) != null)
                {
                    if(reMatch1.IsMatch( line ))
                    {
                        GroupCollection group = reMatch1.Match( line ).Groups;

                        uint   address = UInt32.Parse( group[1].Value, System.Globalization.NumberStyles.HexNumber );
                        string symbol  =               group[3].Value;

                        symdef        [symbol ] = address;
                        symdef_Inverse[address] =  symbol;
                    }
                    else if(reMatch2.IsMatch( line ))
                    {
                        GroupCollection group = reMatch2.Match( line ).Groups;

                        if(lastName != null)
                        {
                            uint address = UInt32.Parse( group[1].Value, System.Globalization.NumberStyles.HexNumber );

                            symdef        [lastName] = address;
                            symdef_Inverse[address ] = lastName;

                            lastName = null;
                        }
                    }
                    else if(reMatch3.IsMatch( line ))
                    {
                        GroupCollection group = reMatch3.Match( line ).Groups;

                        string name = group[1].Value;

                        switch(name)
                        {
                            case ".text":
                            case "$a":
                            case "$d":
                            case "$p":
                                break;

                            default:
                                if(name.StartsWith( "i." ))
                                {
                                    name = name.Substring( 2 );
                                }

                                lastName = name;
                                break;
                        }
                    }
                }
            }
        }

        //--//

        private static bool Unmangle_ClassName(     string        symbol ,
                                                ref int           index  ,
                                                    StringBuilder name   )
        {
            if(symbol[index] == 'Q' && char.IsDigit( symbol, index+1 ))
            {
                int times = symbol[index+1] - '0';

                index += 2;

                while(times-- > 0)
                {
                    if(Unmangle_ClassName( symbol, ref index, name ) == false) return false;

                    if(times > 0)
                    {
                        name.Append( "::" );
                    }
                }
            }
            else if(char.IsDigit( symbol, index ))
            {
                int len = 0;

                while(index < symbol.Length && char.IsDigit( symbol, index ))
                {
                    len = len * 10 + symbol[index] - '0';

                    index++;
                }

                if(len == 0 || len + index > symbol.Length) return false;

                name.Append( symbol, index, len );

                index += len;
            }

            return true;
        }

        private static bool Unmangle_Parameter(     string       symbol     ,
                                                ref int          index      ,
                                                    List<string> parameters )
        {
            int len = parameters.Count;

            switch(symbol[index++])
            {
                case 'i': parameters.Add( "int"       ); return true;
                case 's': parameters.Add( "short"     ); return true;
                case 'l': parameters.Add( "long"      ); return true;
                case 'x': parameters.Add( "long long" ); return true;
                case 'c': parameters.Add( "char"      ); return true;
                case 'b': parameters.Add( "bool"      ); return true;
                case 'f': parameters.Add( "float"     ); return true;
                case 'd': parameters.Add( "double"    ); return true;
                case 'v': parameters.Add( "void"      ); return true;
                case 'e': parameters.Add( "..."       ); return true;

                //--//

                case 'N': // N<digit><pos>             = Repeat parameter <pos> <digit> times.
                    if(char.IsDigit( symbol, index ))
                    {
                        int num = symbol[index++] - '0';

                        if(char.IsDigit( symbol, index ))
                        {
                            int pos = symbol[index++] - '0' - 1;

                            if(pos < len)
                            {
                                while(num-- > 0)
                                {
                                    parameters.Add( parameters[pos] );
                                }

                                return true;
                            }
                        }
                    }
                    break;

                case 'T': // T<pos>                    = Repeat parameter <pos>
                    if(char.IsDigit( symbol, index ))
                    {
                        int pos = symbol[index++] - '0' - 1;

                        if(pos < len)
                        {
                            parameters.Add( parameters[pos] );

                            return true;
                        }
                    }
                    break;

                case 'P': // P                       = pointer
                    if(Unmangle_Parameter( symbol, ref index, parameters ))
                    {
                        parameters[len] += "*";
                        return true;
                    }
                    break;

                case 'R': // R                       = reference
                    if(Unmangle_Parameter( symbol, ref index, parameters ))
                    {
                        parameters[len] += "&";
                        return true;
                    }
                    break;

                case 'C': // C                       = const
                    //
                    // CP => <type> * const
                    // PC => const <type> *
                    //
                    if(Unmangle_Parameter( symbol, ref index, parameters ))
                    {
                        parameters[len] = "const " + parameters[len];
                        return true;
                    }
                    break;

                case 'U': // U                       = unsigned
                    if(Unmangle_Parameter( symbol, ref index, parameters ))
                    {
                        parameters[len] = "unsigned " + parameters[len];
                        return true;
                    }
                    break;

                case 'Q':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        StringBuilder sb = new StringBuilder();

                        index--;

                        if(Unmangle_ClassName( symbol, ref index, sb ))
                        {
                            parameters.Add( sb.ToString() );
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        private static void Unmangle_SkipPrefix(     string symbol     ,
                                                 ref int    index      ,
                                                 ref string dst        ,
                                                     string prefix     ,
                                                     string substitute )
        {
            if(index + prefix.Length <= symbol.Length)
            {
                if(symbol.Substring( index, prefix.Length ) == prefix)
                {
                    index += prefix.Length;

                    dst += substitute;
                }
            }
        }

        public static void Unmangle(     string name        ,
                                     out string strClass    ,
                                     out string strFunction ,
                                     out string strModifier )
        {
            int           index        = 0;
            StringBuilder functionName = new StringBuilder();
            StringBuilder className    = new StringBuilder();

            strClass    = string.Empty;
            strFunction = string.Empty;
            strModifier = string.Empty;

            Unmangle_SkipPrefix( name, ref index, ref strModifier, "$Ven$AA$L$$", "veneer " );
            Unmangle_SkipPrefix( name, ref index, ref strModifier, "i."         , "inline " );

            name  = name.Substring( index );
            index = 0;

            if(name.Contains( "::" ))
            {
                strClass    = "C# function";
                strFunction = name;
                return;
            }

            if(name.Contains( "Jitter#" ))
            {
                strClass    = "Jitter";
                strFunction = name.Substring( "Jitter#".Length );
                return;
            }

            while(index < name.Length)
            {
                if(name[index] == '_' && index + 1 < name.Length && name[index+1] == '_')
                {
                    int index2 = index + 2;

                    className.Length = 0;

                    if(Unmangle_ClassName( name, ref index2, className ))
                    {
////                    bool fStatic = false;
                        bool fConst  = false;

                        while(index2 < name.Length)
                        {
                            if(name[index2] == 'S')
                            {
                                index2++;
////                            fStatic = true;
                                continue;
                            }

                            if(name[index2] == 'C')
                            {
                                index2++;
                                fConst = true;
                                continue;
                            }

                            break;
                        }

                        if(index2 < name.Length && name[index2++] == 'F')
                        {
                            List<string> parameters = new List<string>();
                            bool         fOk        = true;

                            while(fOk && index2 < name.Length)
                            {
                                fOk = Unmangle_Parameter( name, ref index2, parameters );
                            }

                            if(fOk)
                            {
                                if(className.Length > 0)
                                {
                                    strClass = className.ToString();
                                }
                                else
                                {
                                    strClass = "C++ function";
                                }

                                functionName.Append( "( " );

                                for(int i = 0; i < parameters.Count; i++)
                                {
                                    if(i != 0) functionName.Append( ", " );

                                    functionName.Append( parameters[i] );
                                }

                                functionName.Append( " )" );

                                if(fConst ) functionName.Append( " const"  );
////                            if(fStatic) functionName.Append( " static" );

                                strFunction = functionName.ToString();
                                return;
                            }
                        }
                    }
                }

                functionName.Append( name[index++] );
            }

            strClass    = "C function";
            strFunction = functionName.ToString();
        }
    }
}

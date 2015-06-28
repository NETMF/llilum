using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TestMethodGen
{
    class Program
    {
        enum ReplaceType
        {
            test,
            exception,
        }

        static void Main( string[] args )
        {
            if(args.Length < 2)
            {
                Usage();
                return;
            }

            try
            {
                ReplaceType type;

                if(Enum.TryParse(args[0].ToLower(), out type))
                {
                    string dir = args[1];

                    if(File.Exists( dir ))
                    {
                        switch(type)
                        {
                            case ReplaceType.test:
                                ParseTestFile( dir );
                                break;

                            case ReplaceType.exception:
                                ParseExceptionFile( dir );
                                break;
                        }

                    }
                    else
                    {
                        ParseDirectory( type, dir );
                    }
                }
                else
                {
                    Console.WriteLine( "Error: Invalid type");
                    Usage();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine( e.Message );
                Usage();
            }
        }

        static void ParseDirectory( ReplaceType type, string dir )
        {
            foreach(string file in Directory.GetFiles( dir, "*.cs" ))
            {
                switch(type)
                {
                    case ReplaceType.test:
                        ParseTestFile( file );
                        break;

                    case ReplaceType.exception:
                        ParseExceptionFile( file );
                        break;
                }
            }

            foreach(string subdir in Directory.GetDirectories( dir ))
            {
                if(subdir != "." && subdir != "..")
                {
                    ParseDirectory( type, subdir );
                }
            }
        }

        static void ParseExceptionFile( string file )
        {
            string fileTmp = Path.Combine( Path.GetDirectoryName( file ), Path.GetFileNameWithoutExtension( file ) );
            string ext = Path.GetExtension( file );
            int i = 0;
            Regex exp = new Regex( @"\bthrow\s+new\s+(\w*Exception)\s*\((.*)\)" );
            Regex expIfDef = new Regex( "#if EXCEPTION_STRINGS" );
            Regex expEndif = new Regex( "#endif" );
            Regex expTabs  = new Regex( @"\A(\s+)" );
            Regex expComment = new Regex( @"\A\s*\/\/" );
            Regex expIf = new Regex( @"\A\s*if\s*\(" );
            List<string> tests = new List<string>();
            bool exceptionFound = false;
            bool exceptInitFoundState = false;

            while(File.Exists( fileTmp + i.ToString( "X" ) + ext ))
            {
                i++;
            }

            fileTmp += i.ToString( "X" ) + ext;

            using(TextWriter tw = File.CreateText( fileTmp ))
            using(TextReader tr = File.OpenText( file ))
            {
                while(tr.Peek() != -1)
                {
                    string line = tr.ReadLine();

                    if(exceptInitFoundState)
                    {
                        if(expEndif.IsMatch( line ))
                        {
                            exceptInitFoundState = false;
                        }

                        tw.WriteLine( line );
                    }
                    else if(expIfDef.IsMatch( line ))
                    {
                        exceptInitFoundState = true;

                        tw.WriteLine( line );
                    }
                    else if(!expComment.IsMatch( line ))
                    {
                        Match m = exp.Match( line );

                        if(m.Success && m.Groups[2].Value.Trim().Length > 0)
                        {
                            exceptionFound = true;

                            string exception = m.Groups[1].Value;

                            string tab = "";
                            string indent = "";
                            string epilog = null;
                            // find indent
                            Match mTab = expTabs.Match( line );

                            if(mTab.Success)
                            {
                                tab = mTab.Groups[1].Value;
                            }

                            if(mTab.Index + mTab.Length != m.Index)
                            {
                                tw.WriteLine( line.Substring( 0, m.Index ) );

                                if(expIf.IsMatch( line ))
                                {
                                    indent = "    ";
                                }

                                line = tab + indent + line.Substring( m.Index );
                            }

                            tw.WriteLine( "#if EXCEPTION_STRINGS" );

                            int idxSemi = line.IndexOf( ';' );
                            if(idxSemi == -1)
                            {
                                tw.WriteLine( line );

                                do
                                {
                                    line = tr.ReadLine();
                                    tw.WriteLine( line );
                                } while(line.IndexOf( ';' ) == -1);
                            }
                            else
                            {
                                epilog = line.Substring( idxSemi + 1 ).Trim();
                                tw.WriteLine( line.Substring(0, idxSemi + 1) );
                            }

                            tw.WriteLine( "#else" );
                            if(exception == "ObjectDisposedException")
                            {
                                tw.WriteLine( tab + indent + "throw new " + exception + "( null );" );
                            }
                            else
                            {
                                tw.WriteLine( tab + indent + "throw new " + exception + "();" );
                            }
                            tw.WriteLine( "#endif" );

                            if(!string.IsNullOrEmpty( epilog ))
                            {
                                tw.WriteLine( tab + epilog );
                            }
                        }
                        else
                        {
                            tw.WriteLine( line );
                        }
                    }
                    else
                    {
                        tw.WriteLine( line );
                    }
                }
            }

            if(exceptionFound)
            {
                Console.WriteLine("Modified file: " + file);
                File.Copy( fileTmp, file, true );
            }
            File.Delete( fileTmp );
        }

        static void ParseTestFile( string file )
        {
            string fileTmp = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
            string ext = Path.GetExtension(file);
            int i=0;
            Regex exp = new Regex( @"\s*public\s+MFTestResults\s+([\w+\d+_]+)\(\)" );
            Regex expOpenBrace = new Regex( "{" );
            Regex expCloseBrace = new Regex( "}" );
            Regex expAlreadyParsed = new Regex( @"\s*public\s+MFTestMethod\[\]\s+Tests" );
            Regex expTestStart = new Regex( @"\s*return\s+new\s+MFTestMethod\[\]" );
            List<string> tests = new List<string>();
            int braceCnt = 0;
            bool testsFound = false;
            int testInitFoundState = 0;
            int testInitBraceCount = 0;
            int max = 0;

            while(File.Exists(fileTmp + i.ToString("X") + ext))
            {
                i++;
            }

            fileTmp += i.ToString("X") + ext;

            using(TextWriter tw = File.CreateText( fileTmp ))
            using(TextReader tr = File.OpenText( file ))
            {
                while(tr.Peek() != -1)
                {
                    string line = tr.ReadLine();

                    if(testInitFoundState > 0)
                    {
                        if(testInitFoundState == 1)
                        {
                            if(expTestStart.IsMatch( line ))
                            {
                                testInitFoundState++;
                            }
                            else if(expOpenBrace.IsMatch( line ))
                            {
                                testInitBraceCount++;
                            }
                            continue;
                        }
                        else
                        {
                            string test = line.Trim().TrimEnd( ',' );

                            if(expCloseBrace.IsMatch( test ))
                            {
                                testInitBraceCount--;
                            }
                            else if(expOpenBrace.IsMatch( test ))
                            {
                                testInitBraceCount++;
                            }
                            else if(!string.IsNullOrEmpty( test ))
                            {
                                Regex expMFTest = new Regex( @"\s*new\s+MFTestMethod\s*\(\s*([\w+\d+_]+)," );

                                Match m2 = expMFTest.Match( test );
                                if(m2.Success)
                                {
                                    test = m2.Groups[1].Value;
                                }

                                if(!tests.Contains( test ))
                                {
                                    tests.Add( test );
                                }
                            }

                            if(testInitBraceCount <= 0)
                            {
                                testInitFoundState = 0;
                            }
                        }
                        // make sure we don't re-write old tests
                        continue;
                    }
                    else if(expAlreadyParsed.IsMatch( line ))
                    {
                        testInitFoundState = 1;
                        continue;
                    }

                    MatchCollection ms = expOpenBrace.Matches( line );

                    braceCnt += ms.Count;

                    if(max < braceCnt) max = braceCnt;

                    ms = expCloseBrace.Matches( line );

                    braceCnt -= ms.Count;

                    Match m = exp.Match( line );

                    if(m.Success)
                    {
                        string test = m.Groups[1].Value;

                        if(m.Groups.Count > 1 && !tests.Contains(test))
                        {
                            tests.Add( test );
                        }
                    }

                    if(braceCnt == 1 && max > 1 && tests.Count > 0)
                    {
                        tw.Write(
    @"
        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
" );
                        foreach(string tst in tests)
                        {
                            tw.WriteLine( "                    new MFTestMethod( " + tst + ", " + "\"" + tst + "\" ),"  );
                        }
                        testsFound = true;
                        tests.Clear();
                        max = 0;
                        tw.Write(
    @"                };
             }
        }
" );
                    }

                    tw.WriteLine( line );
                }
            }

            if(testsFound)
            {
                File.Copy( fileTmp, file, true );
            }
            File.Delete( fileTmp );
        }

        static void Usage()
        {
            Console.WriteLine( "Usage: TestMethodGen [test|exceptions] <test path>" );
        }
    }
}

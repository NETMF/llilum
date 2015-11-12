// The MIT License( MIT)
// 
// Copyright( c) 2015 Microsoft
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.MIEngine.Extensions
{
    /// <summary>Represents a parsed registerId and value pair from the MI response</summary>
    public struct RegisterIdValuePair
    {
        public RegisterIdValuePair( int id, uint value )
        {
            Id = id;
            Value = value;
        }

        public int Id  { get; }

        public uint Value { get; }
    }

    /// <summary>Represents a parsed registerId and value pair from the MI response</summary>
    public struct RegisterIdNamePair
    {
        public RegisterIdNamePair( int id, string name )
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }

    /// <summary>Represents the result status of a command response</summary>
    public enum ResultStatus
    {
        Done,
        Connected,
        Error,
        Exit
    }

    /// <summary>Base class for parsed engine results</summary>
    public class MIEngineResult
    {
        protected MIEngineResult( ResultStatus status )
        {
            Status = status;
        }

        /// <summary>Status parsed from the response</summary>
        public ResultStatus Status { get; }
    }

    /// <summary>Result of parsing a '-data-list-register-values x' command</summary>
    public class RegisterValuesResult
        : MIEngineResult
    {
        internal RegisterValuesResult( ResultStatus status, IEnumerable<RegisterIdValuePair> registers )
            : base( status )
        {
            // NOTE: ToList() is run here to support Async scenarios by forcing 
            // the list enumeration at the time this result is created instead
            // of some point later when the Registers property is read on some
            // other thread. 
            Registers = registers.ToList( ).AsReadOnly( );
        }

        public IReadOnlyList<RegisterIdValuePair> Registers { get; }

        public static Task<RegisterValuesResult> ParseAsync( string resultTxt ) => MIEngineResultParsers.ParseRegisterValuesResultAsync( resultTxt );

        public static string GetCommand( ) => GetCommand( new int[0] );

        public static string GetCommand( IReadOnlyList<int> registers )
        {
            var regArg = string.Join( " ", registers.Select( ( i ) => i.ToString( ) ) );
            return $"-data-list-register-values x {regArg}";
        }
    }

    /// <summary>Parses the result of a '-data-list-changed-registers' command</summary>
    public class ChangedRegistersResult
        : MIEngineResult
    {
        internal ChangedRegistersResult( ResultStatus status, IEnumerable<int> registers )
            : base( status )
        {
            // NOTE: ToList() is run here to support Async scenarios by forcing 
            // the list enumeration at the time this result is created instead
            // of some point later when the Registers property is read on some
            // other thread. 
            Registers = registers.ToList( ).AsReadOnly( );
        }

        public IReadOnlyList<int> Registers { get; }

        public static Task<ChangedRegistersResult> ParseAsync( string resultTxt ) => MIEngineResultParsers.ParseChangedRegistersResultAsync( resultTxt );
        public const string Command = "-data-list-changed-registers";
    }

    /// <summary>Parses the result of a '-data-list-register-names' command</summary>
    public class RegisterNamesResult
        : MIEngineResult
    {
        internal RegisterNamesResult( ResultStatus status, IEnumerable<string> names )
            : base( status )
        {
            var regNames = from regId in names.Select( ( n, i ) => new RegisterIdNamePair( i, n ) )
                           where !string.IsNullOrEmpty( regId.Name )
                           select regId;

            // NOTE: ToList() is run here to support Async scenarios by forcing 
            // the list enumeration at the time this result is created instead
            // of some point later when the Names property is read on some
            // other thread. 
            Names = regNames.ToList( ).AsReadOnly( );
        }

        public IReadOnlyList<RegisterIdNamePair> Names { get; }

        public static Task<RegisterNamesResult> ParseAsync( string resultTxt ) => MIEngineResultParsers.ParseRegisterNamesResultAsync( resultTxt );
        public const string Command = "-data-list-register-names";
    }

    /// <summary>Internal parsers for parsing the textual results from commands sent via MIEngine</summary>
    /// <remarks>
    /// <para>The parsers are built using Parser Combinators built from the Sprache parser library (Nuget package - Apache Lic. )
    /// The combinators help simplify the task of parsing the string results into a useable form at runtime whithout the
    /// need of complex Regular Expressions or a full blown ANTLR sylte parser.</para>
    /// <note type="note">It is important to note that the format of the texual response from the MIEngine isn't the actual
    /// GDB Machine Interface (MI) form. The MIEngine will parse the actual MI result to present a human readable response,
    /// which is what these parsers take as input. This is due to the fact that the raw MI form is not exposed outside the
    /// MIEngine itself but the human readable for is thanks to the Debug.MIDebugExec command.</note>
    /// </remarks>
    public static class MIEngineResultParsers
    {
        public static async Task<RegisterValuesResult> ParseRegisterValuesResultAsync( string resultTxt )
        {
            await TaskScheduler.Default;
            return RegisterValuesResultParser.Parse( resultTxt );
        }

        public static async Task<ChangedRegistersResult> ParseChangedRegistersResultAsync( string resultTxt )
        {
            await TaskScheduler.Default;
            return ChangedRegistersResultParser.Parse( resultTxt );
        }

        public static async Task<RegisterNamesResult> ParseRegisterNamesResultAsync( string resultTxt )
        {
            await TaskScheduler.Default;
            return RegisterNamesResultParser.Parse( resultTxt );
        }

        #region Primitive Parser Combinators
        // parses a single hex digit
        private static readonly Parser<char> HexDigit
            = Parse.Digit.Or( Parse.Chars( 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F' ) );

        // parses a register name (doesn't currently allow underscores or dashes but that doesn't appear to be necessary...)
        private static readonly Parser<IOption<string>> RegisterName
            = Parse.Identifier( Parse.Letter, Parse.LetterOrDigit )
                   .Optional( )
                   .Token();

        // parses a decimal integer as an int
        private static readonly Parser<int> DecimalInteger
            = ( from val in Parse.Digit.AtLeastOnce( ).Text( ).Token( )
                select int.Parse( val )
              ).Token( );

        // parses a '0x' prefixed hexadecimal number as an unsigned 32 bit integer
        private static readonly Parser<uint> HexInteger
            = ( from prefix in Parse.String( "0x" )
                from val in HexDigit.AtLeastOnce( ).Text( ).Token( )
                select Convert.ToUInt32( val, 16 )
              ).Token();

        // parses double quoted text
        private static readonly Parser<string> QuotedText
            = ( from open in Parse.Char( '"' )
                from content in Parse.CharExcept( '"' ).Many( ).Text( )
                from close in Parse.Char( '"' )
                select content
              ).Token( );

        // Factory method for building name=value pair parser combinators
        private static Parser<KeyValuePair<string, TValue>> NameValuePair<TValue>( string name, Parser<TValue> valueParser )
        {
            return ( from key in Parse.String( name ).Text( ).Token( )
                     from eq in Parse.Char( '=' ).Token( )
                     from value in valueParser
                     select new KeyValuePair<string, TValue>( key, value )
                   ).Token( );
        }
        #endregion

        #region MIEngine result string parsers
        // parses an MI register id and value pair
        private static readonly Parser<RegisterIdValuePair> RegisterValue
            = from lbrace in Parse.Char( '{' ).Token( )
              from num in NameValuePair( "number", DecimalInteger )
              from comma in Parse.Char( ',' ).Token( )
              from val in NameValuePair( "value", HexInteger )
              from rbrace in Parse.Char( '}' )
              select new RegisterIdValuePair( num.Value, val.Value );

        // parses the MIEngine result line
        private static readonly Parser<ResultStatus> ResultClass
            = from label in Parse.String( "result-class:" ).Token( )
              from kind in Parse.String( "done" ).Return( ResultStatus.Done )
                           .Or( Parse.String( "error" ).Return( ResultStatus.Error ) )
                           .Or( Parse.String( "exit" ).Return( ResultStatus.Exit ) )
                           .Or( Parse.String( "connected" ).Return( ResultStatus.Connected ) )
              from trailing in Parse.WhiteSpace.Optional( )
              from eol in Parse.LineEnd
              select kind;

        // parses the result string from a '-data-list-register-values x' command
        private static readonly Parser<RegisterValuesResult> RegisterValuesResultParser
            = ( from kind in ResultClass
                from label in Parse.String( "register-values:" ).Token( )
                from lbracket in Parse.Char( '[' ).Token( )
                from registers in RegisterValue.DelimitedBy( Parse.Char( ',' ).Token( ) )
                from rbracket in Parse.Char( ']' ).Token( )
                select new RegisterValuesResult( kind, registers )
              ).End( );

        // parses the result string from a '-data-list-changed-registers' command
        private static readonly Parser<ChangedRegistersResult> ChangedRegistersResultParser
            = ( from kind in ResultClass
                from label in Parse.String( "changed-registers:" ).Token( )
                from lbracket in Parse.Char( '[' ).Token( )
                from registers in DecimalInteger.DelimitedBy( Parse.Char( ',' ).Token( ) )
                from rbracket in Parse.Char( ']' ).Token( )
                select new ChangedRegistersResult( kind, registers )
              ).End( );

        // parses the result string from a '-data-list-register-names' command
        private static readonly Parser<RegisterNamesResult> RegisterNamesResultParser
            = ( from kind in ResultClass
                from label in Parse.String( "register-names:" ).Token( )
                from lbracket in Parse.Char( '[' ).Token( )
                from registers in RegisterName.DelimitedBy( Parse.Char( ',' ).Token( ) )
                from rbracket in Parse.Char( ']' ).Token( )
                select new RegisterNamesResult( kind, registers.Select( r=>r.IsEmpty ? string.Empty : r.Get() ) )
              ).End( );

        #endregion
    }
}

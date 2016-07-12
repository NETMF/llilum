using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Llvm.NET.Values;

namespace Llvm.NET.Native
{
    internal enum ValueKind : uint
    {
        Argument              = LLVMValueKind.LLVMValueKindArgumentVal,             // This is an instance of Argument
        BasicBlock            = LLVMValueKind.LLVMValueKindBasicBlockVal,           // This is an instance of BasicBlock
        Function              = LLVMValueKind.LLVMValueKindFunctionVal,             // This is an instance of Function
        GlobalAlias           = LLVMValueKind.LLVMValueKindGlobalAliasVal,          // This is an instance of GlobalAlias
        GlobalVariable        = LLVMValueKind.LLVMValueKindGlobalVariableVal,       // This is an instance of GlobalVariable
        UndefValue            = LLVMValueKind.LLVMValueKindUndefValueVal,           // This is an instance of UndefValue
        BlockAddress          = LLVMValueKind.LLVMValueKindBlockAddressVal,         // This is an instance of BlockAddress
        ConstantExpr          = LLVMValueKind.LLVMValueKindConstantExprVal,         // This is an instance of ConstantExpr
        ConstantAggregateZero = LLVMValueKind.LLVMValueKindConstantAggregateZeroVal,// This is an instance of ConstantAggregateZero
        ConstantDataArray     = LLVMValueKind.LLVMValueKindConstantDataArrayVal,    // This is an instance of ConstantDataArray
        ConstantDataVector    = LLVMValueKind.LLVMValueKindConstantDataVectorVal,   // This is an instance of ConstantDataVector
        ConstantInt           = LLVMValueKind.LLVMValueKindConstantIntVal,          // This is an instance of ConstantInt
        ConstantFP            = LLVMValueKind.LLVMValueKindConstantFPVal,           // This is an instance of ConstantFP
        ConstantArray         = LLVMValueKind.LLVMValueKindConstantArrayVal,        // This is an instance of ConstantArray
        ConstantStruct        = LLVMValueKind.LLVMValueKindConstantStructVal,       // This is an instance of ConstantStruct
        ConstantVector        = LLVMValueKind.LLVMValueKindConstantVectorVal,       // This is an instance of ConstantVector
        ConstantPointerNull   = LLVMValueKind.LLVMValueKindConstantPointerNullVal,  // This is an instance of ConstantPointerNull
        ConstantTokenNone     = LLVMValueKind.LLVMValueKindConstantTokenNoneVal,    // This is an instance of ConstantTokenNone
        MetadataAsValue       = LLVMValueKind.LLVMValueKindMetadataAsValueVal,      // This is an instance of MetadataAsValue
        InlineAsm             = LLVMValueKind.LLVMValueKindInlineAsmVal,            // This is an instance of InlineAsm
        Instruction           = LLVMValueKind.LLVMValueKindInstructionVal,          // This is an instance of Instruction
                                                                                    // Enum values starting at InstructionVal are used for Instructions;

        // instruction values come directly from LLVM Instruction.def which is different from the "stable"
        // LLVM-C API, therefore they are less "stable" and bound to the C++ implementation version and
        // subject to change from version to version.
        Return         = 1 + Instruction, // Terminators
        Branch         = 2 + Instruction,
        Switch         = 3 + Instruction,
        IndirectBranch = 4 + Instruction,
        Invoke         = 5 + Instruction,
        Resume         = 6 + Instruction,
        Unreachable    = 7 + Instruction,
        CleanUpReturn  = 8 + Instruction,
        CatchReturn    = 9 + Instruction,
        CatchSwitch    = 10 + Instruction,

        Add            = 11 + Instruction, // binary operators
        FAdd           = 12 + Instruction,
        Sub            = 13 + Instruction,
        FSub           = 14 + Instruction,
        Mul            = 15 + Instruction,
        FMul           = 16 + Instruction,
        UDiv           = 17 + Instruction,
        SDiv           = 18 + Instruction,
        FDiv           = 19 + Instruction,
        URem           = 20 + Instruction,
        SRem           = 21 + Instruction,
        FRem           = 22 + Instruction,

        Shl            = 23 + Instruction, // Logical Operators
        LShr           = 24 + Instruction,
        AShr           = 25 + Instruction,
        And            = 26 + Instruction,
        Or             = 27 + Instruction,
        Xor            = 28 + Instruction,

        Alloca         = 29 + Instruction, // Memory Operators
        Load           = 30 + Instruction,
        Store          = 31 + Instruction,
        GetElementPtr  = 32 + Instruction,
        Fence          = 33 + Instruction,
        AtomicCmpXchg  = 34 + Instruction,
        AtomicRMW      = 35 + Instruction,

        Trunc          = 36 + Instruction, // cast/conversion operators
        ZeroExtend     = 37 + Instruction,
        SignExtend     = 38 + Instruction,
        FPToUI         = 39 + Instruction,
        FPToSI         = 40 + Instruction,
        UIToFP         = 41 + Instruction,
        SIToFP         = 42 + Instruction,
        FPTrunc        = 43 + Instruction,
        FPExt          = 44 + Instruction,
        PtrToInt       = 45 + Instruction,
        IntToPtr       = 46 + Instruction,
        BitCast        = 47 + Instruction,
        AddrSpaceCast  = 48 + Instruction,

        CleanupPad     = 49 + Instruction, // New Exception pads
        CatchPad       = 50 + Instruction,

        ICmp           = 51 + Instruction,
        FCmp           = 52 + Instruction,
        Phi            = 53 + Instruction,
        Call           = 54 + Instruction,
        Select         = 55 + Instruction,
        UserOp1        = 56 + Instruction,
        UserOp2        = 57 + Instruction,
        VaArg          = 58 + Instruction,
        ExtractElement = 59 + Instruction,
        InsertElement  = 60 + Instruction,
        ShuffleVector  = 61 + Instruction,
        ExtractValue   = 62 + Instruction,
        InsertValue    = 63 + Instruction,
        LandingPad     = 64 + Instruction,

        // Markers:
        ConstantFirstVal = Function,
        ConstantLastVal = ConstantPointerNull
    }

    internal partial struct LLVMVersionInfo
    {
        public override string ToString()
        {
            if( VersionString == IntPtr.Zero )
                return null;

            return Marshal.PtrToStringAnsi( VersionString );
        }

        public static implicit operator Version( LLVMVersionInfo versionInfo )
        {
            return new Version(versionInfo.Major, versionInfo.Minor, versionInfo.Patch);
        }
    }

    // add implicit conversions to/from C# bool for convenience
    internal partial struct LLVMBool
    {
        // sometimes LLVMBool values are actually success/failure codes
        // and thus a zero value actually means success and not false or failure.
        public bool Succeeded => Value == 0;

        public bool Failed => !Succeeded;

        public static implicit operator LLVMBool( bool value ) => new LLVMBool( value ? 1 : 0 );
        public static implicit operator bool( LLVMBool value ) => value.Value != 0;
    }

    internal partial struct LLVMMetadataRef
        : IEquatable<LLVMMetadataRef>
    {
        internal static LLVMMetadataRef Zero = new LLVMMetadataRef( IntPtr.Zero );

        public override int GetHashCode( ) => Pointer.GetHashCode( );

        public override bool Equals( object obj )
        {
            if( obj is LLVMMetadataRef )
                return Equals( ( LLVMMetadataRef )obj );

            if( obj is IntPtr )
                return Pointer.Equals( obj );

            return base.Equals( obj );
        }

        public bool Equals( LLVMMetadataRef other ) => Pointer == other.Pointer;

        public static bool operator ==( LLVMMetadataRef lhs, LLVMMetadataRef rhs ) => lhs.Equals( rhs );
        public static bool operator !=( LLVMMetadataRef lhs, LLVMMetadataRef rhs ) => !lhs.Equals( rhs );
    }

    internal static partial class NativeMethods
    {
        internal static ValueKind GetValueKind( LLVMValueRef valueRef ) => ( ValueKind )GetValueID( valueRef );

        static void FatalErrorHandler( string Reason )
        {
            Trace.TraceError( Reason );
            // LLVM will call exit() upon return from this function
        }

        /// <summary>This method is used to marshal a string when NativeMethods.DisposeMessage() is required on the string allocated from native code</summary>
        /// <param name="msg">POinter to the native code allocated string</param>
        /// <returns>Managed code string marshaled from the native content</returns>
        /// <remarks>
        /// This method will, construct a new managed string containing the test of the string from native code, normalizing
        /// the line endings to the current execution environments line endings (See: <see cref="Environment.NewLine"/>).
        /// </remarks>
        internal static string MarshalMsg( IntPtr msg )
        {
            var retVal = string.Empty;
            if( msg != IntPtr.Zero )
            {
                try
                {
                    retVal = NormalizeLineEndings( msg );
                }
                finally
                {
                    DisposeMessage( msg );
                }
            }
            return retVal;
        }

        /// <summary>Static constructor for NativeMethods</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline" )]
        static NativeMethods()
        {
            // force loading the appropriate architecture specific
            // DLL before any use of the wrapped inter-op APIs to
            // allow building this library as ANYCPU
            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly( ).Location );
            if( Directory.Exists( Path.Combine( path, "LibLLVM") ) )
            {
                LoadWin32Library( libraryPath, "LibLLVM" );
            }
            else
            {
                // fall-back to standard library search paths to allow building
                // CPU specific variants with only one native DLL without needing
                // conditional compilation on this library, which is useful for
                // unit testing or whenever the Nuget packaging isn't desired.
                LoadWin32Library( libraryPath, null );
            }

            // initialize the static fields
            LineEndingNormalizingRegEx = new Regex( "(\r\n|\n\r|\r|\n)" );
            LLVMVersionInfo versionInfo = new LLVMVersionInfo();
            GetVersionInfo(ref versionInfo);
            if( versionInfo.Major != VersionMajor
             || versionInfo.Minor != VersionMinor
             || versionInfo.Patch != VersionPatch
              )
            {
                throw new BadImageFormatException("Mismatched LibLLVM version");
            }
            FatalErrorHandlerDelegate = new Lazy<LLVMFatalErrorHandler>( ( ) => FatalErrorHandler, LazyThreadSafetyMode.PublicationOnly );
            InstallFatalErrorHandler( FatalErrorHandlerDelegate.Value );
        }

        // LLVM doesn't use environment/OS specific line endings, so this will
        // normalize the line endings from strings provided by LLVM into the current
        // environment's normal format.
        internal static string NormalizeLineEndings( IntPtr llvmString )
        {
            if( llvmString == IntPtr.Zero )
                return string.Empty;

            var str = Marshal.PtrToStringAnsi( llvmString );
            return NormalizeLineEndings( str );
        }

        internal static string NormalizeLineEndings( IntPtr llvmString, int len )
        {
            if( llvmString == IntPtr.Zero || len == 0 )
                return string.Empty;

            var str = Marshal.PtrToStringAnsi( llvmString, len );
            return NormalizeLineEndings( str );
        }

        private static string NormalizeLineEndings( string txt )
        {
            // shortcut optimization for environments that match the LLVM assumption
            if( Environment.NewLine.Length == 1 && Environment.NewLine[ 0 ] == '\n' )
                return txt;

            return LineEndingNormalizingRegEx.Replace( txt, Environment.NewLine );
        }

        // lazy initialized singleton unmanaged delegate so it is never collected
        private static Lazy<LLVMFatalErrorHandler> FatalErrorHandlerDelegate;
        private static readonly Regex LineEndingNormalizingRegEx;

        // version info for verification of matched LibLLVM
        private const int VersionMajor = 3;
        private const int VersionMinor = 8;
        private const int VersionPatch = 1;
    }
}

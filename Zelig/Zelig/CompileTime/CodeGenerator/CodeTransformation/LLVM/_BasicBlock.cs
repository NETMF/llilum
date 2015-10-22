using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Types;
using Llvm.NET.Values;
using Microsoft.Zelig.CodeGeneration.IR;
using Microsoft.Zelig.Debugging;
using Microsoft.Zelig.Runtime.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using BasicBlock = Llvm.NET.Values.BasicBlock;

namespace Microsoft.Zelig.LLVM
{
    public class _BasicBlock
    {
        internal _BasicBlock( _Function owner, BasicBlock block )
        {
            Owner = owner;
            Module = Owner.Module;
            LlvmBasicBlock = block;
            IrBuilder = new InstructionBuilder( block );
        }

        private _Value CastToFunctionPointer( _Value val, _Type funcTy )
        {
            _Type timpl = Module.GetOrInsertPointerType( funcTy );

            val = LoadToImmediate( val );

            Value llvmValue = val.LlvmValue;

            llvmValue = IrBuilder.ExtractValue( llvmValue, 0 )
                                 .SetDebugLocation( CurDILocation );

            var resultValue = IrBuilder.BitCast( llvmValue, timpl.DebugType )
                                       .RegisterName( "indirect_function_pointer_cast" )
                                       .SetDebugLocation( CurDILocation );

            return new _Value( Module, timpl, resultValue, false );
       }

        public _Value LoadToImmediate( _Value val )
        {
            if( val.IsImmediate )
                return val;

            var resultValue = IrBuilder.Load( val.LlvmValue )
                                       .RegisterName( "LoadToImmediate" )
                                       .SetDebugLocation( CurDILocation );

            return new _Value( Module, val.Type.UnderlyingType, resultValue, true );
        }

        public void InsertASMString( string ASM )
        {
#if DUMP_INLINED_COMMENTS_AS_ASM_CALLS
            // unported C++ code InlineAsm not yet supported in Llvm.NET (and not really needed here anyway)
            string strASM = (const char*)( Marshal::StringToHGlobalAnsi( ASM ) ).ToPointer( );
            std::vector<llvm::Type*> FuncTy_3_args;
            FunctionType* FuncTy_3 = FunctionType::get( llvm::Type::getVoidTy( owner._pimpl.GetLLVMObject( ).getContext( ) ), FuncTy_3_args, false );
            auto asmFunc = InlineAsm::get( FuncTy_3, strASM, "", true );
            bldr.Call( asmFunc );
#endif
        }

        public void SetDebugInfo( LLVMModuleManager manager, MethodRepresentation method, Operator op )
        {
            // ensure the function has debug information
            var func = Module.GetFunctionWithDebugInfoFor( manager, method );
            Debug.Assert( Owner == func );

            if( op != null )
            {
                SetDebugInfo( op.DebugInfo );
            }
            else
            {
                SetDebugInfo( method.DebugInfo ?? manager.GetDebugInfoFor( method ) ); 
            }
        }

        private void SetDebugInfo( DebugInfo debugInfo )
        {
            if( debugInfo == null )
                return;

            // don't allow inlined code to change the file scope of the location
            // - that either requires a new DILexicalFileBlock (i.e. preprocessor
            // macro expansion) or it requires the location to include the InlinedAt
            // location scoping. In order to build the InlinedAt we'd need the
            // DISubProgram corresponding to the method the operator is inlined
            // from, which the Zelig inlining doesn't currently provide all we get
            // is the source and line data.
            string curPath = Owner.LlvmFunction.DISubProgram.File?.Path ?? string.Empty;
            if( 0 != string.Compare( debugInfo.SrcFileName, curPath, StringComparison.OrdinalIgnoreCase ) )
                return;

            CurDILocation = new DILocation( LlvmBasicBlock.Context
                                          , ( uint )( debugInfo?.BeginLineNumber ?? 0 )
                                          , ( uint )( debugInfo?.BeginColumn ?? 0 )
                                          , Owner.LlvmFunction.DISubProgram
                                          );
        }

        private void InsertStore(Value src, Value dst)
        {
            var ptrType = dst.Type as IPointerType;
            if (src.Type != ptrType.ElementType)
            {
                Console.WriteLine("For \"Ptr must be a pointer to Val type!\" Assert.");
                Console.WriteLine("getOperand(0).getType()");
                Console.WriteLine(src.Type);
                Console.WriteLine("");
                Console.WriteLine("cast<IPointerType>(getOperand(1).getType()).getElementType()");
                Console.WriteLine(ptrType.ElementType);
                Console.WriteLine("");
                throw new ApplicationException();
            }

            IrBuilder.Store(src, dst);
        }

        // review: Is there a good reason the order of params here is reversed from classic src,dst? (same as LLVM API)
        public void InsertStore( _Value dst, _Value src )
        {
            Value llvmSrc = src.LlvmValue;
            Value llvmDst = dst.LlvmValue;
            InsertStore(llvmSrc, llvmDst);
        }

        public void InsertStoreArgument(_Value dst, int index)
        {
            var llvmFunc = (Function)Owner.LlvmValue;
            Value llvmSrc = llvmFunc.Parameters[index];
            Value llvmDst = dst.LlvmValue;
            InsertStore(llvmSrc, llvmDst);
        }

        public void InsertStoreIntoBT( _Value dst, _Value src )
        {
            Value llvmSrc = src.LlvmValue;
            Value llvmDst = dst.LlvmValue;

            if( llvmDst.Type is IPointerType )
            {
                InsertStore( llvmSrc, llvmDst );
            }
            else
            {
                IrBuilder.InsertValue( llvmDst, llvmSrc, 0 );
            }
        }

        public _Value LoadIndirect( _Value val, _Type loadedType )
        {
            val = LoadToImmediate( val );

            _Type pointerType = val.Type;
            Value resultVal;

            _Type underlyingType = pointerType.UnderlyingType;
            if( ( underlyingType != null ) && underlyingType.IsBoxed )
            {
                pointerType = Module.GetOrInsertPointerType( underlyingType.UnderlyingType );
                resultVal = IrBuilder.GetStructElementPointer( val.LlvmValue, 1 );
            }
            else
            {
                pointerType = Module.GetOrInsertPointerType( loadedType );
                resultVal = IrBuilder.BitCast( val.LlvmValue, pointerType.DebugType );
            }

            return new _Value( Module, pointerType, resultVal, false );
        }

        public void InsertMemCpy( _Value dst, _Value src )
        {
            Debug.Assert( src != null && dst != null );
            Debug.Assert( src.IsPointer );
            Debug.Assert( dst.IsPointer );
            Debug.Assert( src.Type == dst.Type );

            Context ctx = Module.LlvmModule.Context;
            Value dstSize = ctx.CreateConstant( dst.Type.UnderlyingType.SizeInBits / 8);
            IrBuilder.MemCpy( Module.LlvmModule, dst.LlvmValue, src.LlvmValue, dstSize, 0, false);
        }

        public void InsertMemCpy( _Value dst, _Value src, _Value size, bool overlapping )
        {
            Debug.Assert( src != null && dst != null && size != null );
            Debug.Assert( src.IsPointer );
            Debug.Assert( dst.IsPointer );
            Debug.Assert( size.IsInteger );

            if( overlapping )
            {
                IrBuilder.MemMove( Module.LlvmModule, dst.LlvmValue, src.LlvmValue, size.LlvmValue, 0, false );
            }
            else
            {
                IrBuilder.MemCpy( Module.LlvmModule, dst.LlvmValue, src.LlvmValue, size.LlvmValue, 0, false );
            }
        }

        public void InsertMemSet( _Value dst, byte value )
        {
            Debug.Assert( dst != null );
            Debug.Assert( dst.IsPointer );

            Context ctx = Module.LlvmModule.Context;
            Value fillValue = ctx.CreateConstant( value );
            Value dstSize = ctx.CreateConstant( dst.Type.UnderlyingType.SizeInBits / 8 );

            IrBuilder.MemSet( Module.LlvmModule, dst.LlvmValue, fillValue, dstSize, 0, false);
        }

        public void InsertMemSet( _Value dst, _Value value, _Value size )
        {
            Debug.Assert( dst != null && size != null && value != null );
            Debug.Assert( dst.IsPointer );
            Debug.Assert( size.IsInteger );
            Debug.Assert( value.IsInteger);

            IrBuilder.MemSet( Module.LlvmModule, dst.LlvmValue, value.LlvmValue, size.LlvmValue, 0, false);
        }

        enum BinaryOperator
        {
            ADD = 0,
            SUB = 1,
            MUL = 2,
            DIV = 3,
            REM = 4,
            AND = 5,
            OR = 6,
            XOR = 7,
            SHL = 8,
            SHR = 9,
        };

        public _Value InsertBinaryOp( int op, _Value a, _Value b, bool isSigned )
        {
            var binOp = ( BinaryOperator )op;
            Debug.Assert( a.IsInteger || a.IsFloatingPoint );
            Debug.Assert( b.IsInteger || b.IsFloatingPoint );
            Value retVal;

            a = LoadToImmediate( a );
            b = LoadToImmediate( b );

            Value loadedA = a.LlvmValue;
            Value loadedB = b.LlvmValue;
            var bldr = IrBuilder;

            if( a.IsInteger && b.IsInteger )
            {
                switch( binOp )
                {
                case BinaryOperator.ADD:
                    retVal = bldr.Add( loadedA, loadedB );
                    break;
                case BinaryOperator.SUB:
                    retVal = bldr.Sub( loadedA, loadedB );
                    break;
                case BinaryOperator.MUL:
                    retVal = bldr.Mul( loadedA, loadedB );
                    break;
                case BinaryOperator.DIV:
                    if( isSigned )
                        retVal = bldr.SDiv( loadedA, loadedB );
                    else
                        retVal = bldr.UDiv( loadedA, loadedB );
                    break;
                case BinaryOperator.REM:
                    if( isSigned )
                        retVal = bldr.SRem( loadedA, loadedB );
                    else
                        retVal = bldr.URem( loadedA, loadedB );
                    break;
                case BinaryOperator.AND:
                    retVal = bldr.And( loadedA, loadedB );
                    break;
                case BinaryOperator.OR:
                    retVal = bldr.Or( loadedA, loadedB );
                    break;
                case BinaryOperator.XOR:
                    retVal = bldr.Xor( loadedA, loadedB );
                    break;
                case BinaryOperator.SHL:
                    retVal = bldr.ShiftLeft( loadedA, loadedB );
                    break;
                case BinaryOperator.SHR:
                    if( isSigned )
                        retVal = bldr.ArithmeticShiftRight( loadedA, loadedB );
                    else
                        retVal = bldr.LogicalShiftRight( loadedA, loadedB );
                    break;
                default:
                    throw new ApplicationException( $"Parameters combination not supported for Binary Operator: {binOp}" );
                }
            }
            else if( a.IsFloatingPoint && b.IsFloatingPoint )
            {
                switch( binOp )
                {
                case BinaryOperator.ADD:
                    retVal = bldr.FAdd( loadedA, loadedB );
                    break;
                case BinaryOperator.SUB:
                    retVal = bldr.FSub( loadedA, loadedB );
                    break;
                case BinaryOperator.MUL:
                    retVal = bldr.FMul( loadedA, loadedB );
                    break;
                case BinaryOperator.DIV:
                    retVal = bldr.FDiv( loadedA, loadedB );
                    break;
                default:
                    throw new ApplicationException( $"Parameters combination not supported for Binary Operator: {binOp}" );
                }
            }
            else
                throw new ApplicationException( $"Parameters combination not supported for Binary Operator: {binOp}" );

            retVal = retVal.SetDebugLocation( CurDILocation );
            return new _Value( Module, a.Type, retVal, true );
        }

        enum UnaryOperator
        {
            NEG = 0,
            NOT = 1,
            FINITE = 2,
        };

        public _Value InsertUnaryOp( int op, _Value val, bool isSigned )
        {
            var unOp = ( UnaryOperator )op;

            Debug.Assert( val.IsInteger || val.IsFloatingPoint );
            val = LoadToImmediate( val );

            Value retVal = val.LlvmValue;

            switch( unOp )
            {
            case UnaryOperator.NEG:
                if( val.IsInteger )
                {
                    retVal = IrBuilder.Neg( retVal );
                }            
                else
                {
                    retVal = IrBuilder.FNeg( retVal );
                }
                break;
            case UnaryOperator.NOT:
                retVal = IrBuilder.Not( retVal );
                break;
            }
            retVal = retVal.SetDebugLocation( CurDILocation );
            return new _Value( Module, val.Type, retVal, true );
        }
        const int SignedBase = 10;
        const int FloatBase = SignedBase + 10;

        static readonly Dictionary<int, Predicate> PredicateMap = new Dictionary<int, Predicate>( )
        {
            [ 0 ] = Predicate.Equal,                  //llvm::CmpInst::ICMP_EQ;
            [ 1 ] = Predicate.UnsignedGreaterOrEqual, //llvm::CmpInst::ICMP_UGE;
            [ 2 ] = Predicate.UnsignedGreater,        //llvm::CmpInst::ICMP_UGT;
            [ 3 ] = Predicate.UnsignedLessOrEqual,    //llvm::CmpInst::ICMP_ULE;
            [ 4 ] = Predicate.UnsignedLess,           //llvm::CmpInst::ICMP_ULT;
            [ 5 ] = Predicate.NotEqual,               //llvm::CmpInst::ICMP_NE;

            [ SignedBase + 0 ] = Predicate.Equal,                //llvm::CmpInst::ICMP_EQ;
            [ SignedBase + 1 ] = Predicate.SignedGreaterOrEqual, //llvm::CmpInst::ICMP_SGE;
            [ SignedBase + 2 ] = Predicate.SignedGreater,        //llvm::CmpInst::ICMP_SGT;
            [ SignedBase + 3 ] = Predicate.SignedLessOrEqual,    //llvm::CmpInst::ICMP_SLE;
            [ SignedBase + 4 ] = Predicate.SignedLess,           //llvm::CmpInst::ICMP_SLT;
            [ SignedBase + 5 ] = Predicate.NotEqual,              //llvm::CmpInst::ICMP_NE;

            [ FloatBase + 0 ] = Predicate.OrderedAndEqual,              //llvm::CmpInst::FCMP_OEQ;
            [ FloatBase + 1 ] = Predicate.OrderedAndGreaterThanOrEqual, //llvm::CmpInst::FCMP_OGE;
            [ FloatBase + 2 ] = Predicate.OrderedAndGreaterThan,        //llvm::CmpInst::FCMP_OGT;
            [ FloatBase + 3 ] = Predicate.OrderedAndLessThanOrEqual,    //llvm::CmpInst::FCMP_OLE;
            [ FloatBase + 4 ] = Predicate.OrderedAndLessThan,           //llvm::CmpInst::FCMP_OLT;
            [ FloatBase + 5 ] = Predicate.OrderedAndNotEqual            //llvm::CmpInst::FCMP_ONE;
        };

        public _Value InsertCmp( int predicate, bool isSigned, _Value valA, _Value valB )
        {
            Debug.Assert( valA.IsInteger || valA.IsFloatingPoint );
            Debug.Assert( valB.IsInteger || valB.IsFloatingPoint );

            valA = LoadToImmediate( valA );
            valB = LoadToImmediate( valB );

            Value llvmValA = valA.LlvmValue;
            Value llvmValB = valB.LlvmValue;

            _Type booleanImpl = Module.GetType( "System.Boolean" );

            if( valA.IsInteger && valB.IsInteger )
            {
                Predicate p = PredicateMap[ predicate + ( isSigned ? SignedBase : 0 ) ];
                var icmp = IrBuilder.Compare( ( IntPredicate )p, llvmValA, llvmValB )
                                    .RegisterName("icmp" );

                var inst = IrBuilder.ZeroExtendOrBitCast( icmp, booleanImpl.DebugType )
                                    .SetDebugLocation( CurDILocation );

                return new _Value( Module, booleanImpl, inst, true );
            }

            if( valA.IsFloatingPoint && valB.IsFloatingPoint )
            {
                Predicate p = PredicateMap[ predicate + FloatBase ];

                var cmp = IrBuilder.Compare( ( RealPredicate )p, llvmValA, llvmValB )
                                   .RegisterName("fcmp" );

                var value = IrBuilder.ZeroExtendOrBitCast( cmp, booleanImpl.DebugType )
                                     .SetDebugLocation( CurDILocation );

                return new _Value( Module, booleanImpl, value, true );
            }

                Console.WriteLine( "valA:" );
                Console.WriteLine( valA.ToString( ) );
                Console.WriteLine( "valB:" );
                Console.WriteLine( valB.ToString( ) );
                throw new ApplicationException( "Parameter combination not supported for CMP Operator." );
            }

        public _Value InsertZExt( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );
            Value retVal = val.LlvmValue;

            if( significantBits != val.Type.SizeInBits )
            {
                retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, Module.LlvmModule.Context.GetIntType( ( uint )significantBits ) )
                                  .SetDebugLocation( CurDILocation );
            }

            retVal = IrBuilder.ZeroExtendOrBitCast( retVal, ty.DebugType )
                              .RegisterName( "zext" )
                              .SetDebugLocation( CurDILocation );

            return new _Value( Module, ty, retVal, true );
        }

        public _Value InsertSExt( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );
            Value retVal = val.LlvmValue;

            if( significantBits != val.Type.SizeInBits )
            {
                retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, Module.LlvmModule.Context.GetIntType( ( uint )significantBits ) )
                                  .SetDebugLocation( CurDILocation );
            }

            retVal = IrBuilder.SignExtendOrBitCast( retVal, ty.DebugType )
                              .RegisterName( "sext" )
                              .SetDebugLocation( CurDILocation );

            return new _Value( Module, ty, retVal, true );
        }

        public _Value InsertTrunc( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );

            Value retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, ty.DebugType )
                                    .SetDebugLocation( CurDILocation );

            return new _Value( Module, ty, retVal, true );
        }

        public _Value InsertBitCast( _Value val, _Type type )
        {
            var bitCast = IrBuilder.BitCast( val.LlvmValue, type.DebugType )
                                   .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, bitCast, true );
        }

        public _Value InsertPointerToInt( _Value val, _Type intType )
        {
            Debug.Assert( val.IsPointer );
            Debug.Assert( val.IsImmediate );

            Value llvmVal = IrBuilder.PointerToInt( val.LlvmValue, intType.DebugType )
                                     .SetDebugLocation( CurDILocation );
            return new _Value( Module, intType, llvmVal, true );
        }

        public _Value InsertIntToPointer( _Value val, _Type pointerType )
        {
            Debug.Assert( val.IsInteger );
            Debug.Assert( val.IsImmediate );

            Value llvmVal = IrBuilder.IntToPointer( val.LlvmValue, (IPointerType)pointerType.DebugType )
                                     .SetDebugLocation( CurDILocation );
            return new _Value( Module, pointerType, llvmVal, true );
        }

        public _Value GetAddressAsUIntPtr( _Value val )
        {
            _Type ptrTy = Module.GetType( "System.UIntPtr" );

            Debug.Assert( val != null );

            return new _Value( Module, ptrTy, val.LlvmValue, true );
        }

        public _Value InsertIntToFP( _Value val, _Type type )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );

            // TODO: This is hard-coded as a signed conversion, but we should respect whether the value is signed.
            var value = IrBuilder.SIToFPCast( val.LlvmValue, type.DebugType )
                                 .RegisterName( "sitofp" )
                                 .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, value, true );
        }

        public _Value InsertFPExt( _Value val, _Type type )
        {
            Debug.Assert( val.IsFloatingPoint );
            val = LoadToImmediate( val );

            var value = IrBuilder.FPExt( val.LlvmValue, type.DebugType )
                                 .RegisterName( "fpext" )
                                 .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, value, true );
        }

        public _Value InsertFPTrunc( _Value val, _Type type )
        {
            Debug.Assert( val.IsFloatingPoint );
            val = LoadToImmediate( val );

            var value = IrBuilder.FPTrunc( val.LlvmValue, type.DebugType )
                                 .RegisterName( "fpext" )
                                 .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, value, true );
        }

        public _Value InsertFPToInt( _Value val, _Type ty )
        {
            Debug.Assert( val.IsFloatingPoint );
            _Type intType = ty;
            val = LoadToImmediate( val );

            // TODO: This is hard-coded as an unsigned conversion, but we should respect whether the value is signed.
            var value = IrBuilder.FPToUICast( val.LlvmValue, intType.DebugType )
                                 .RegisterName( "fptoui" )
                                 .SetDebugLocation( CurDILocation );

            return new _Value( Module, intType, value, true );
        }

        public void InsertUnconditionalBranch( _BasicBlock bb )
        {
            IrBuilder.Branch( bb.LlvmBasicBlock )
                     .SetDebugLocation( CurDILocation );
        }

        public void InsertConditionalBranch( _Value cond, _BasicBlock trueBB, _BasicBlock falseBB )
        {
            Debug.Assert( cond.IsInteger );
            cond = LoadToImmediate( cond );

            //Review: Testing all the bits for now. We need to check its always valid to trunc the condition
            // to the first bit if we want to change it. That being said, most of the times this should be
            // get rid on the instructions conv pass from LLVM.
            Value vA = cond.LlvmValue;
            Value vB = Module.LlvmModule.Context.CreateConstant( cond.Type.DebugType, 0, false );

            Value condVal = IrBuilder.Compare(IntPredicate.NotEqual, vA, vB)
                                     .RegisterName( "icmpe" )
                                     .SetDebugLocation( CurDILocation );

            IrBuilder.Branch( condVal, trueBB.LlvmBasicBlock, falseBB.LlvmBasicBlock )
                     .SetDebugLocation( CurDILocation );
        }

        public void InsertSwitchAndCases( _Value cond, _BasicBlock defaultBB, List<int> casesValues, List<_BasicBlock> casesBBs )
        {
            Debug.Assert( cond.IsInteger );
            cond = LoadToImmediate( cond );
            var si = IrBuilder.Switch( cond.LlvmValue, defaultBB.LlvmBasicBlock, ( uint )casesBBs.Count )
                              .SetDebugLocation( CurDILocation );

            for( int i = 0; i < casesBBs.Count; ++i )
            {
                si.AddCase( Module.LlvmModule.Context.CreateConstant( casesValues[ i ] ), casesBBs[ i ].LlvmBasicBlock );
            }
        }

        private void LoadParams( _Function func, IList<_Value> args, IList<Value> parameters )
        {
            for( int i = 0; i < args.Count; ++i )
            {
                ITypeRef pty = ( ( Function )func.LlvmValue ).Parameters[ i ].Type;
                _Type paramType = _Type.GetTypeImpl( pty );

                _Value argument = LoadToImmediate( args[ i ] );
                Value llvmValue = argument.LlvmValue;

                // IntPtr/UIntPtr can be cast to anything.
                if ( argument.Type.Name == "System.IntPtr" || argument.Type.Name == "System.UIntPtr" )
                {
                    llvmValue = IrBuilder.BitCast( llvmValue, pty );
                }

                // Anything can be cast to IntPtr/UIntPtr.
                if( paramType.Name == "System.IntPtr" || paramType.Name == "System.UIntPtr" )
                {
                    llvmValue = IrBuilder.BitCast( llvmValue, pty );
                }

                parameters.Add( llvmValue );
            }
        }

        public _Value InsertCall( _Function func, List<_Value > args )
        {
            List< Value> parameters = new List<Value>();
            LoadParams( func, args, parameters );

            Value retVal = IrBuilder.Call( func.LlvmValue, parameters )
                                    .SetDebugLocation( CurDILocation );

            var llvmFunc = (Function)(func.LlvmValue);
            if( llvmFunc.ReturnType.IsVoid )
                return null;

            return new _Value( Module, _Type.GetTypeImpl( llvmFunc.ReturnType ), retVal, true );
        }

        public _Value InsertIndirectCall( _Function func, _Value ptr, List<_Value> args )
        {
            List< Value > parameters = new List<Value>();
            LoadParams( func, args, parameters );

            ptr = CastToFunctionPointer( ptr, func.Type );

            Value retVal = IrBuilder.Call( ptr.LlvmValue, parameters )
                                    .SetDebugLocation( CurDILocation );

            var llvmFunc = ( Function )( func.LlvmValue );
            if( llvmFunc.ReturnType.IsVoid )
                return null;

            return new _Value( Module, _Type.GetTypeImpl( llvmFunc.ReturnType ), retVal, true );
        }

        static _Type SetValuesForByteOffsetAccess( _Type ty, List<uint> values, int offset, out string fieldName )
        {
            for( int i = 0; i < ty.Fields.Count; ++i )
            {
                TypeField thisField = ty.Fields[ i ];

                // The first field of a managed object is either its object header or a super-class.
                bool fieldIsParent = ( i == 0 && !ty.IsValueType && ty != _Type.GetTypeImpl( "Microsoft.Zelig.Runtime.ObjectHeader" ) );
                int thisFieldSize = thisField.MemberType.SizeInBits / 8;

                int curOffset = ( int )thisField.Offset;
                int nextOffset = curOffset + thisFieldSize;

                // If the next field is beyond our desired offset, inspect the current field. As offset may index into
                // a nested field, we must recursively inspect each nested field until we find an exact match.
                if ( nextOffset > offset )
                {
                    values.Add( thisField.FinalIdx );
                    fieldName = thisField.Name;

                    // If the current offset isn't an exact match, it must be an offset within a nested field. We also
                    // force inspection of parent classes even on an exact match.
                    if ( fieldIsParent || ( curOffset != offset ) )
                    {
                        return SetValuesForByteOffsetAccess( thisField.MemberType, values, offset - curOffset, out fieldName );
                    }

                    return thisField.MemberType;
                }
            }

            throw new ApplicationException( "Invalid offset for field access." );
        }

        public _Value GetField( _Value obj, int offset )
        {
            Context ctx = Module.LlvmModule.Context;

            // If the given address is *not* a value type on the stack, load it.
            Debug.Assert( !obj.IsImmediate || obj.Type.IsPointer, "Cannot get field address on a loaded value type." );
            if( obj.Type.UnderlyingType.IsPointer )
            {
                obj = LoadToImmediate( obj );
            }

            // Special case: For boxed types, pull out the wrapped element.
            _Type underlyingType = obj.Type.UnderlyingType;
            if( ( underlyingType != null ) && underlyingType.IsBoxed )
            {
                var structType = ( IStructType )underlyingType.DebugType;
                Value[] indexValues = { ctx.CreateConstant( 0 ), ctx.CreateConstant( 1 ) };

                var unbox = IrBuilder.GetElementPtr( obj.LlvmValue, indexValues )
                                     .RegisterName( "unboxedValue" )
                                     .SetDebugLocation( CurDILocation );

                underlyingType = underlyingType.UnderlyingType;
                _Type pointerType = Module.GetOrInsertPointerType( underlyingType );
                obj = new _Value( Module, pointerType, unbox, false );
            }

            // If the object isn't a pointer, revert the LoadToImmediate.
            if( !obj.IsPointer )
            {
                underlyingType = obj.Type;
            }

            // Special case: If this is a basic type, we assume the field is m_value (e.g. UIntPtr.m_value).
            if( underlyingType.IsPrimitiveType )
            {
                Debug.Assert( offset == 0, "Native types can only have one member, and it must be at offset zero." );

                // Reinterpret the object as non-immediate if necessary. We create a new value here instead of modifying
                // the old one as the passed in parameter may be used by subsequent operations.
                if( obj.IsImmediate )
                {
                    obj = new _Value( Module, obj.Type, obj.LlvmValue, false );
                }

                return obj;
            }

            List<uint> values = new List<uint>();
            string fieldName;
            _Type fieldType = SetValuesForByteOffsetAccess( underlyingType, values, offset, out fieldName );

            List<Value> valuesForGep = new List<Value>();
            valuesForGep.Add( ctx.CreateConstant( 0 ) );

            for( int i = 0; i < values.Count; ++i )
            {
                valuesForGep.Add( ctx.CreateConstant( values[ i ] ) );
            }

            var gep = IrBuilder.GetElementPtr( obj.LlvmValue, valuesForGep )
                               .RegisterName( $"{underlyingType.Name}.{fieldName}" )
                               .SetDebugLocation( CurDILocation );

            fieldType = Module.GetOrInsertPointerType( fieldType );
            return new _Value( Module, fieldType, gep, false );
        }

        public _Value IndexLLVMArray( _Value obj, _Value idx )
        {
            idx = LoadToImmediate( idx );

            Value[] idxs = { Module.LlvmModule.Context.CreateConstant( 0 ), idx.LlvmValue };
            Value retVal = IrBuilder.GetElementPtr( obj.LlvmValue, idxs )
                                    .SetDebugLocation( CurDILocation );

            var arrayType = ( IArrayType )obj.Type.UnderlyingType.DebugType;
            _Type pointerType = Module.GetOrInsertPointerType( _Type.GetTypeImpl( arrayType.ElementType ) );
            return new _Value( Module, pointerType, retVal, false );
        }

        public void InsertRet( _Value val )
        {
            if( val == null )
            {
                IrBuilder.Return( )
                         .SetDebugLocation( CurDILocation );
            }
            else
            {
                IrBuilder.Return( LoadToImmediate( val ).LlvmValue )
                         .SetDebugLocation( CurDILocation );
            }
        }

        public _Value InsertAtomicXchg( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicXchg, ptr, val );
        public _Value InsertAtomicAdd( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicAdd, ptr, val );
        public _Value InsertAtomicSub( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicSub, ptr, val );
        public _Value InsertAtomicAnd( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicAnd, ptr, val );
        public _Value InsertAtomicNand( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicNand, ptr, val );
        public _Value InsertAtomicOr( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicOr, ptr, val );
        public _Value InsertAtomicXor( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicXor, ptr, val );
        public _Value InsertAtomicMax( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicMax, ptr, val );
        public _Value InsertAtomicMin( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicMin, ptr, val );
        public _Value InsertAtomicUMax( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicUMax, ptr, val );
        public _Value InsertAtomicUMin( _Value ptr, _Value val ) => InsertAtomicBinaryOp( IrBuilder.AtomicUMin, ptr, val );

        private _Value InsertAtomicBinaryOp( Func<Value, Value, Value> fn, _Value ptr, _Value val)
        {
            // LLVM atomicRMW returns the old value at ptr
            var retVal = fn( ptr.LlvmValue, val.LlvmValue );

            retVal = retVal.SetDebugLocation( CurDILocation );
            return new _Value( Module, val.Type, retVal, true );
        }

        public _Value InsertAtomicCmpXchg( _Value ptr, _Value cmp, _Value val )
        {
            // LLVM cmpxchg instruction returns { ty, i1 } for { oldValue, isSuccess }
            var retVal = IrBuilder.AtomicCmpXchg( ptr.LlvmValue, cmp.LlvmValue, val.LlvmValue );

            // And we only want the old value
            var oldVal = IrBuilder.ExtractValue( retVal, 0 );

            oldVal = oldVal.SetDebugLocation( CurDILocation );
            return new _Value( Module, val.Type, oldVal, true );
        }

        internal InstructionBuilder IrBuilder { get; }

        internal BasicBlock LlvmBasicBlock { get; }

        public DISubProgram CurDISubProgram => CurDILocation?.Scope as DISubProgram;

        internal DILocation CurDILocation;

        readonly _Module Module;
        readonly _Function Owner;
    }
}

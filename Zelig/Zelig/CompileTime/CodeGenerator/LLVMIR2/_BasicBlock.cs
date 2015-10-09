using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Instructions;
using Llvm.NET.Types;
using Llvm.NET.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TS = Microsoft.Zelig.Runtime.TypeSystem;

namespace Microsoft.Zelig.LLVM
{
    public interface IModuleManager
    {
        string GetMangledNameFor( TS.MethodRepresentation method );
        _Type LookupNativeTypeFor( TS.TypeRepresentation type );
        Debugging.DebugInfo GetDebugInfoFor( TS.MethodRepresentation method );
    }

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
                                 .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            llvmValue = IrBuilder.ExtractValue( llvmValue, 0 )
                                 .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            var resultValue = IrBuilder.BitCast( llvmValue
                                               , timpl.GetLLVMObjectForStorage( )
                                               ).RegisterName( "indirect_function_pointer_cast" )
                                                .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, timpl, resultValue, false );
       }

        public _Value LoadToImmediate( _Value val )
        {
            if( val.IsImmediate )
                return val;

            var resultValue = IrBuilder.Load( val.LlvmValue )
                                       .RegisterName( "LoadToImmediate" )
                                       .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, val.Type, resultValue, true );
        }

        private _Value RevertImmediate( _Value val )
        {
            if( !val.IsImmediate )
                return val;

            var loadInst = val.LlvmValue as Load;
            if( loadInst == null )
                return null;

            return new _Value( Module, val.Type, loadInst.Operands[ 0 ], false );
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

        //void InsertWarning(string msg);

        public void SetDebugInfo( int curLine, int curCol, string srcFile, IModuleManager manager, TS.MethodRepresentation method )
        {
            string mangledName = manager.GetMangledNameFor( method );
            if( srcFile != null )
            {
                DebugCurLine = curLine;
                DebugCurCol = curCol;
                Module.SetCurrentDIFile( srcFile );
            }

            CurDiSubProgram = Module.GetDISubprogram( mangledName );

            if( CurDiSubProgram == null )
            {
                CreateDiSubProgram( manager, method, mangledName );
            }
        }

        private void CreateDiSubProgram( IModuleManager manager, TS.MethodRepresentation method, string mangledName )
        {
            var diFile = Module.CurDiFile;
            var functionType = Owner.Type;

            // Create the DiSupprogram info
            CurDiSubProgram = Module.LlvmModule.DIBuilder.CreateFunction( diFile
                                                                        , method.Name
                                                                        , mangledName
                                                                        , diFile
                                                                        , ( uint )DebugCurLine
                                                                        , (DICompositeType)functionType.DIType
                                                                        , true
                                                                        , true
                                                                        , ( uint )DebugCurLine
                                                                        , 0U
                                                                        , false
                                                                        , ( Function )( Owner.LlvmValue )
                                                                        );

            Module.SetDISubprogram( mangledName, CurDiSubProgram );
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
            var ptrType = llvmDst.Type as IPointerType;

            if( ptrType == null )
            {
                IrBuilder.InsertValue( llvmDst, llvmSrc, 0 );
            }
            else
            {
                llvmDst = IrBuilder.GetStructElementPointer( llvmDst, 0 );
                InsertStore(llvmSrc, llvmDst);
            }
        }

        public _Value LoadIndirect( _Value val, _Type ptrTy )
        {
            val = LoadToImmediate( val );

            if( val.Type.IsBoxed )
            {
                return new _Value( Module
                                 , val.Type.UnderlyingBoxedType
                                 , IrBuilder.GetStructElementPointer( val.LlvmValue, 1 )
                                 , false
                                 );
            }

            var value = IrBuilder.BitCast( val.LlvmValue
                                         , ptrTy.GetLLVMObjectForStorage( ).CreatePointerType( )
                                         );

            return new _Value( Module, ptrTy, value, false );
        }

        public void InsertMemCpy( _Value dst, _Value src )
        {
            dst = RevertImmediate( dst );
            _Value riSrc = RevertImmediate( src );
            Context ctx = Module.LlvmModule.Context;

            //I can't do a memcopy because I can't find a
            //src address, so I do a copy by copy instead
            if( riSrc == null )
            {
                Value llvmDst = dst.LlvmValue;
                Value llvmSrc = src.LlvmValue;

                //INSERT FIELD BY FIELD LOAD/STORE AND RETURN
                var ptrType = ( IPointerType )llvmDst.Type;
                var numFields = ( ( IStructType )ptrType.ElementType ).Members.Count;
                for( uint i = 0; i < numFields; ++i )
                {
                    Value[] idxs = { ctx.CreateConstant( 0 ), ctx.CreateConstant( (int)i ) };

                    Value tmpSrc = IrBuilder.ExtractValue( llvmSrc, i)
                                            .RegisterName( "fieldByFieldSrcExtract" )
                                            .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                    Value tmpDst = IrBuilder.GetElementPtrInBounds( llvmDst, idxs)
                                            .RegisterName( "fieldByFieldDstGep" )
                                            .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                    IrBuilder.Store( tmpSrc, tmpDst )
                             .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
                }

                return;
            }

            src = riSrc;

            Debug.Assert( src != null && dst != null );
            Debug.Assert( src.IsPointer );
            Debug.Assert( dst.IsPointer );
            Debug.Assert( src.Type == dst.Type );

            IrBuilder.MemCpy( Module.LlvmModule
                            , dst.LlvmValue
                            , src.LlvmValue
                            , ctx.CreateConstant( dst.Type.SizeInBits / 8)
                            , 0
                            , false
                            );
        }

        public void InsertMemCpy( _Value dst, _Value src, _Value size, bool overlapping )
        {
            dst = LoadToImmediate( dst );
            src = LoadToImmediate( src );

            Debug.Assert( src != null && dst != null && size != null );
            Debug.Assert( src.IsPointer );
            Debug.Assert( dst.IsPointer );
            Debug.Assert( size.IsInteger );

            if( overlapping )
            {
                IrBuilder.MemMove( Module.LlvmModule
                                 , dst.LlvmValue
                                 , src.LlvmValue
                                 , size.LlvmValue
                                 , 0
                                 , false
                                 );
            }
            else
            {
                IrBuilder.MemCpy( Module.LlvmModule
                                , dst.LlvmValue
                                , src.LlvmValue
                                , size.LlvmValue
                                , 0
                                , false
                                );
            }
        }

        public void InsertMemSet( _Value dst, byte value )
        {
            dst = RevertImmediate( dst );
            Debug.Assert( dst != null );
            Debug.Assert( dst.IsPointer );
            Context ctx = Module.LlvmModule.Context;

            IrBuilder.MemSet( Module.LlvmModule
                            , dst.LlvmValue
                            , ctx.CreateConstant( value )
                            , ctx.CreateConstant( dst.Type.SizeInBits / 8 )
                            , 0
                            , false
                            );
        }

        
        public void InsertMemSet( _Value dst, _Value src, _Value size )
        {
            dst = LoadToImmediate( dst );
            src = LoadToImmediate( src );

            Debug.Assert( src != null && dst != null && size != null );
            Debug.Assert( src.IsPointer );
            Debug.Assert( dst.IsPointer );
            Debug.Assert( size.IsInteger );

            IrBuilder.MemSet( Module.LlvmModule
                                , dst.LlvmValue
                                , src.LlvmValue
                                , size.LlvmValue
                                , 4
                                , false
                                );
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

            retVal = retVal.SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
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
            retVal = retVal.SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
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

            _Type booleanImpl = Module.GetType( "LLVM.System.Boolean" );

            if( valA.IsInteger && valB.IsInteger )
            {
                Predicate p = PredicateMap[ predicate + ( isSigned ? SignedBase : 0 ) ];
                var icmp = IrBuilder.Compare( ( IntPredicate )p, llvmValA, llvmValB )
                                    .RegisterName("icmp" );

                var inst = IrBuilder.ZeroExtendOrBitCast( icmp, booleanImpl.DebugType )
                                    .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                return new _Value( Module, booleanImpl, inst, true );
            }

            if( valA.IsFloatingPoint && valB.IsFloatingPoint )
            {
                Predicate p = PredicateMap[ predicate + FloatBase ];

                var cmp = IrBuilder.Compare( ( RealPredicate )p, llvmValA, llvmValB )
                                   .RegisterName("fcmp" );

                var value = IrBuilder.ZeroExtendOrBitCast( cmp, booleanImpl.DebugType )
                                     .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

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

            var retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, Module.LlvmModule.Context.GetIntType( ( uint )significantBits ) )
                                  .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            retVal = IrBuilder.ZeroExtendOrBitCast( retVal, ty.GetLLVMObjectForStorage( ) )
                              .RegisterName( "zext" )
                              .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, retVal, true );
        }

        public _Value InsertSExt( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );

            var retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, Module.LlvmModule.Context.GetIntType( ( uint )significantBits ) )
                                  .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            retVal = IrBuilder.SignExtendOrBitCast( retVal, ty.GetLLVMObjectForStorage( ) )
                              .RegisterName( "sext" )
                              .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, retVal, true );
        }

        public _Value InsertTrunc( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );

            Value retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, ty.GetLLVMObjectForStorage( ) )
                                    .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, retVal, true );
        }

        public _Value InsertPointerToInt( _Value val, bool skipObjectHeader )
        {
            Debug.Assert( val.IsPointer );
            val = LoadToImmediate( val );
            _Type ty = Module.GetType( "LLVM.System.UInt32" );

            Value llvmVal = IrBuilder.PointerToInt( val.LlvmValue, ty.GetLLVMObjectForStorage( ) );

            if( skipObjectHeader && !val.Type.IsValueType && val.Type.Name != "Microsoft.Zelig.Runtime.ObjectHeader" )
            {
                _Type ohTy = Module.GetType( "Microsoft.Zelig.Runtime.ObjectHeader" );
                var constantInt = Module.LlvmModule.Context.CreateConstant( ohTy.SizeInBits / 8 );
                llvmVal = IrBuilder.Add( llvmVal, constantInt )
                                   .RegisterName( "headerOffsetAdd" )
                                   .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
            }

            return new _Value( Module, ty, llvmVal, true );
        }

        public _Value InsertIntToPointer( _Value val, _Type ptrTy )
        {
            Debug.Assert( val.IsInteger );
            val = LoadToImmediate( val );

            Value llvmVal = val.LlvmValue;

            if (!ptrTy.IsValueType && ptrTy.Name != "Microsoft.Zelig.Runtime.ObjectHeader")
            {
                _Type ohTy = Module.GetType( "Microsoft.Zelig.Runtime.ObjectHeader" );
                llvmVal = IrBuilder.Sub( llvmVal, Module.LlvmModule.Context.CreateConstant( ohTy.SizeInBits / 8 ) )
                                   .RegisterName("headerOffsetSub" )
                                   .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
            }

            llvmVal = IrBuilder.IntToPointer( llvmVal, (IPointerType)ptrTy.GetLLVMObjectForStorage( ) )
                               .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ptrTy, llvmVal, true );
        }

        public _Value GetAddressAsUIntPtr( _Value val )
        {
            _Type ptrTy = Module.GetType( "LLVM.System.UIntPtr" );
            val = RevertImmediate( val );

            Debug.Assert( val != null );

            return new _Value( Module, ptrTy, val.LlvmValue, true );
        }

        public _Value GetBTCast( _Value val, _Type ty )
        {
            if( val.Type.GetBTUnderlyingType( ) == null )
                return val;

            if( ty.GetBTUnderlyingType( ) == null )
                return val;

            if( val.Type.GetSizeInBitsForStorage( ) != ty.GetSizeInBitsForStorage( ) )
            {
                //Needs integer cast:
                val = LoadToImmediate( val );
                _Value newVal = Owner.GetLocalStackValue( "ForParameterIntegerCast", ty );

                Value llvmVal = IrBuilder.ExtractValue( val.LlvmValue, 0 )
                                         .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                var structType = ( IStructType )newVal.Type.DebugType;

                llvmVal = IrBuilder.IntCast( llvmVal, structType.Members[0], false )
                                   .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                var gep = IrBuilder.GetStructElementPointer( newVal.LlvmValue, 0 )
                                   .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
                IrBuilder.Store( llvmVal, gep )
                         .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                return newVal;
            }

                _Value retVal = RevertImmediate( val );
                if( retVal == null )
                    return val;

            var bitCast = IrBuilder.BitCast( val.LlvmValue, ty.DebugType.CreatePointerType() )
                                   .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, bitCast, false );
        }

        public _Value ExtractFirstElementFromBasicType( _Value val )
        {
            _Type ty = Module.GetType( $"LLVM.{val.Type.Name}" );

            Debug.Assert( ty != null );

            Value llvmVal = val.LlvmValue;

            if( llvmVal.Type.IsPointer )
            {
                llvmVal = IrBuilder.Load( llvmVal );
            }

            llvmVal = IrBuilder.ExtractValue( llvmVal, 0)
                               .RegisterName( ".m_value" )
                               .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, llvmVal, true );
        }

        public _Value InsertSIToFPFloat( _Value val )
        {
            Debug.Assert( val.IsInteger );
            _Type ty = Module.GetType( "LLVM.System.Single" );
            val = LoadToImmediate( val );

            var value = IrBuilder.SIToFPCast( val.LlvmValue, ty.DebugType )
                                 .RegisterName( "sitofp" )
                                 .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, value, true );
        }

        public _Value InsertSIToFPDouble( _Value val )
        {
            Debug.Assert( val.IsInteger );
            _Type ty = Module.GetType( "LLVM.System.Double" );
            val = LoadToImmediate( val );

            var value = IrBuilder.SIToFPCast( val.LlvmValue, ty.DebugType)
                                 .RegisterName( "sitofp" )
                                 .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, value, true );
        }

        public _Value InsertFPFloatToFPDouble( _Value val )
        {
            Debug.Assert( val.IsFloatingPoint );
            _Type ty = Module.GetType( "LLVM.System.Double" );
            val = LoadToImmediate( val );

            var value = IrBuilder.FPExt( val.LlvmValue, ty.DebugType)
                                 .RegisterName( "fpext" )
                                 .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, ty, value, true );
        }

        public _Value InsertFPToUI( _Value val, _Type ty )
        {
            Debug.Assert( val.IsFloatingPoint );
            _Type intType = ty;
            val = LoadToImmediate( val );

            var value = IrBuilder.FPToUICast( val.LlvmValue, intType.DebugType)
                                 .RegisterName( "fptoui" )
                                 .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            return new _Value( Module, intType, value, true );
        }

        public void InsertUnconditionalBranch( _BasicBlock bb )
        {
            IrBuilder.Branch( bb.LlvmBasicBlock )
                     .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
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
                                     .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            IrBuilder.Branch( condVal, trueBB.LlvmBasicBlock, falseBB.LlvmBasicBlock )
                     .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
        }

        public void InsertSwitchAndCases( _Value cond, _BasicBlock defaultBB, List<int> casesValues, List<_BasicBlock> casesBBs )
        {
            Debug.Assert( cond.IsInteger );
            cond = LoadToImmediate( cond );
            var si = IrBuilder.Switch( cond.LlvmValue, defaultBB.LlvmBasicBlock, ( uint )casesBBs.Count )
                              .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            for( int i = 0; i < casesBBs.Count; ++i )
            {
                si.AddCase( Module.LlvmModule.Context.CreateConstant( casesValues[ i ] ), casesBBs[ i ].LlvmBasicBlock );
            }
        }

        void LoadParams( _Function func, IList<_Value> args, IList<Value> parameters )
        {
            for( int i = 0; i < args.Count; ++i )
            {
                args[ i ] = LoadToImmediate( args[ i ] );

                Value llvmValue = args[ i ].LlvmValue;
                ITypeRef pty = ((Function)func.LlvmValue).Parameters[ i ].Type;
                _Type tiPty = _Type.GetTypeImpl( pty );

                //IntPtr/UIntPtr can be casted to anything
                if( args[ i ].Type.Name == "System.IntPtr"
                    || args[ i ].Type.Name == "System.UIntPtr" )
                {
                    args[ i ] = RevertImmediate( args[ i ] );
                    Debug.Assert( args[ i ] != null );
                    llvmValue = args[ i ].LlvmValue;
                    llvmValue = IrBuilder.Load( IrBuilder.BitCast( llvmValue, pty.CreatePointerType() ) );
                }

                //Anything can be casted to IntPtr/UIntPtr
                if( tiPty.Name == "System.IntPtr"
                    || tiPty.Name == "System.UIntPtr" )
                {
                    args[ i ] = RevertImmediate( args[ i ] );
                    Debug.Assert( args[ i ] != null );
                    llvmValue = args[ i ].LlvmValue;
                    llvmValue = IrBuilder.Load( IrBuilder.BitCast( llvmValue, pty.CreatePointerType() ) );
                }

                if( args[ i ].Type.Name.StartsWith( "LLVM." ) )
                {
                    var constVal = llvmValue as Constant;
                    if( constVal != null )
                    {
                        llvmValue = Module.LlvmModule.AddGlobal( llvmValue.Type, true, Linkage.Internal, constVal );
                    }
                    else
                    {
                        args[ i ] = RevertImmediate( args[ i ] );
                        Debug.Assert( args[ i ] != null );
                        llvmValue = args[ i ].LlvmValue;
                    }

                    llvmValue = IrBuilder.Load( IrBuilder.BitCast( llvmValue, pty.CreatePointerType( ) ) );
                }

                parameters.Add( llvmValue );
            }
        }

        public _Value InsertCall( _Function func, List<_Value > args )
        {
            List< Value> parameters = new List<Value>();
            LoadParams( func, args, parameters );

            Value retVal = IrBuilder.Call( func.LlvmValue, parameters )
                                    .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

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
                                    .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            var llvmFunc = ( Function )( func.LlvmValue );
            if( llvmFunc.ReturnType.IsVoid )
                return null;

            return new _Value( Module, _Type.GetTypeImpl( llvmFunc.ReturnType ), retVal, true );
        }

        static _Type SetValuesForByteOffsetAccess( _Type ty, List<uint> values, int offset, ref string fieldName )
        {
            for( int i = 0; i < ty.Fields.Count; ++i )
            {
                TypeField thisField = ty.Fields[ i ];

                // The first field of a managed object is either its object header or a super-class.
                bool fieldIsParent = ( i == 0 && !ty.IsValueType && ty != _Type.GetTypeImpl( "Microsoft.Zelig.Runtime.ObjectHeader" ) );
                int thisFieldSize = thisField.MemberType.GetSizeInBitsForStorage( ) / 8;
                if( fieldIsParent )
                {
                    thisFieldSize = thisField.MemberType.SizeInBits / 8;
                }

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
                        return SetValuesForByteOffsetAccess( thisField.MemberType, values, offset - curOffset, ref fieldName );
                    }

                    return thisField.MemberType;
                }
            }

            throw new ApplicationException( "Invalid offset for field access!!" );
        }

        public _Value GetField( _Value obj, _Type zTy, _Type fieldType, int offset )
        {
            obj = LoadToImmediate( obj );
            Context ctx = Module.LlvmModule.Context;

            //Special case for boxed types
            if( obj.Type.IsBoxed )
            {
                var structType = ( IStructType )( obj.Type.DebugType );
                Value[] indexValues = { ctx.CreateConstant( 0 ), ctx.CreateConstant( 1 ) };

                var gep = IrBuilder.GetElementPtr( obj.LlvmValue, indexValues )
                                   .RegisterName( "BoxedFieldAccessGep" )
                                   .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

                _Value rVal = new _Value( Module, _Type.GetTypeImpl( structType.Members[ 1 ] ), gep, false );
                obj = rVal;
            }

            _Type finalTimpl = obj.Type;

            //Special case for indexing into pointer to value types
            if( obj.Type.UnderlyingPointerType != null &&
                obj.Type.UnderlyingPointerType.IsValueType )
            {
                finalTimpl = obj.Type.UnderlyingPointerType;
            }

            List<uint> values = new List<uint>();
            string fieldName = string.Empty;
            _Type timpl = SetValuesForByteOffsetAccess( finalTimpl, values, offset, ref fieldName );

            List<Value> valuesForGep = new List<Value>();
            valuesForGep.Add( ctx.CreateConstant( 0 ) );

            for( int i = 0; i < values.Count; ++i )
            {
                valuesForGep.Add( ctx.CreateConstant( values[ i ] ) );
            }

            var gep2 = IrBuilder.GetElementPtr( obj.LlvmValue, valuesForGep )
                                .RegisterName( finalTimpl.Name + "." + fieldName )
                                .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            _Value retVal = new _Value( Module, timpl, gep2, false );

            return retVal;
        }

        public _Value IndexLLVMArray( _Value obj, _Value idx )
        {
            idx = LoadToImmediate( idx );

            Value[] idxs = { Module.LlvmModule.Context.CreateConstant( 0 ), idx.LlvmValue };
            Value retVal = IrBuilder.GetElementPtr( obj.LlvmValue, idxs )
                                    .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );

            var arrayType = ( IArrayType )( obj.Type.DebugType );
            return new _Value( Module, _Type.GetTypeImpl( arrayType.ElementType ), retVal, false );
        }

        public void InsertRet( _Value val )
        {
            if( val == null )
            {
                IrBuilder.Return( )
                         .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
            }
            else
            {
                IrBuilder.Return( LoadToImmediate( val ).LlvmValue )
                         .SetDebugLocation( ( uint )DebugCurLine, ( uint )DebugCurCol, CurDiSubProgram );
            }
        }

        internal DISubProgram CurDiSubProgram { get; private set; }

        internal int DebugCurCol { get; private set; }

        internal int DebugCurLine { get; private set; }

        internal InstructionBuilder IrBuilder { get; }

        internal BasicBlock LlvmBasicBlock { get; }

        readonly _Module Module;
        readonly _Function Owner;
    }
}

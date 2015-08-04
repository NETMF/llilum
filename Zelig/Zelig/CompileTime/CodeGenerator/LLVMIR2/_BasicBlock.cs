using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Zelig.LLVM
{
    // mostly empty shell at the moment to enable testing type and function signature generation
    // In order to ease the migration this is a direct transliteration of the hierarchy and naming
    // from the original C++/CLI code base
    public class _BasicBlock
    {
        internal _BasicBlock( _Function owner, BasicBlockImpl impl )
        {
            Impl = impl;
            Owner = owner;
            Module = Owner.Module();
        }

        private _Value CastToFunctionPointer( _Value val, _Type funcTy )
        {
            TypeImpl timpl = Module.GetOrInsertPointerType( funcTy ).Impl;

            val = LoadToImmediate( val );

            Llvm.NET.Value llvmValue = val.Impl.GetLLVMObject( );

            llvmValue = Impl.IrBuilder.ExtractValue( llvmValue, 0 );
            llvmValue = Impl.IrBuilder.ExtractValue( llvmValue, 0 );

            var resultValue = Impl.IrBuilder.BitCast( llvmValue
                                                    , timpl.GetLLVMObjectForStorage( )
                                                    , "indirect_function_pointer_cast"
                                                    );

            var valueImpl = new ValueImpl( timpl, DecorateInstructionWithDebugInfo( resultValue ), false );
            return new _Value( Module, valueImpl );
       }

        public _Value LoadToImmediate( _Value val )
        {
            if( val.IsImmediate( ) )
                return val;

            var resultValue = Impl.IrBuilder.Load( val.Impl.GetLLVMObject( ), "LoadToImmediate" );
            var valueImpl = new ValueImpl( val.Type( ).Impl, DecorateInstructionWithDebugInfo( resultValue ), true );
            return new _Value( Module, valueImpl );
        }

        private _Value RevertImmediate( _Value val )
        {
            if( !val.IsImmediate( ) )
                return val;

            var loadInst = val.Impl.GetLLVMObject() as Llvm.NET.Instructions.Load;
            if( loadInst == null )
                return null;

            return new _Value( Module, new ValueImpl( val.Type( ).Impl, loadInst.Operands[ 0 ], false ) );
        }

        Llvm.NET.Value DecorateInstructionWithDebugInfo( Llvm.NET.Value value )
        {
            var instruction = value as Llvm.NET.Instructions.Instruction;
            if( instruction != null )
            {
                if( CurDiSubProgram != null )
                    instruction.SetDebugLocation( (uint)DebugCurLine, (uint)DebugCurCol, CurDiSubProgram );
            }
            return value;
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

        public void SetDebugInfo( int curLine, int curCol, string srcFile, string mangledName )
        {
            DebugCurLine = curLine;
            DebugCurCol = curCol;

            CurDiSubProgram = Module.Impl.GetDISubprogram( mangledName );

            if( CurDiSubProgram == null )
            {
                Module.SetCurrentDIFile( srcFile );

                Llvm.NET.DebugInfo.TypeArray dita = Module.Impl.DiBuilder.CreateTypeArray();
                Llvm.NET.DebugInfo.CompositeType funcType = Module.Impl.DiBuilder.CreateSubroutineType( Module.Impl.CurDiFile, dita );
                CurDiSubProgram = Module.Impl.DiBuilder.CreateFunction( Module.Impl.CurDiFile
                                                                      , mangledName
                                                                      , mangledName
                                                                      , Module.Impl.CurDiFile
                                                                      , ( uint )curLine
                                                                      , funcType
                                                                      , true
                                                                      , true
                                                                      , ( uint )curLine
                                                                      , 0U
                                                                      , true
                                                                      , ( Llvm.NET.Function )( Owner.Impl.GetLLVMObject( ) )
                                                                      );

                Module.Impl.SetDISubprogram( mangledName, CurDiSubProgram );
            }
        }

        // review: Is there a good reason the order of params here is reversed from classic src,dst? (same as LLVM API)
        public void InsertStore( _Value dst, _Value src )
        {
            Llvm.NET.Value llvmSrc = src.Impl.GetLLVMObject( );
            Llvm.NET.Value llvmDst = dst.Impl.GetLLVMObject( );
            var ptrType = ( ( Llvm.NET.PointerType )llvmDst.Type );
            if( llvmSrc.Type != ptrType.ElementType )
            {
                Console.WriteLine( "For \"Ptr must be a pointer to Val type!\" Assert." );
                Console.WriteLine( "getOperand(0).getType()" );
                Console.WriteLine( llvmSrc.Type.ToString() );
                Console.WriteLine( "" );
                Console.WriteLine( "cast<PointerType>(getOperand(1).getType()).getElementType()" );
                Console.WriteLine( ptrType.ElementType.ToString() );
                Console.WriteLine( "" );
                throw new ApplicationException( );
            }

            Impl.IrBuilder.Store( llvmSrc, llvmDst );
        }

        public void InsertStoreIntoBT( _Value dst, _Value src )
        {
            Llvm.NET.Value llvmSrc = src.Impl.GetLLVMObject( );
            Llvm.NET.Value llvmDst = dst.Impl.GetLLVMObject( );
            var ptrType = llvmDst.Type as Llvm.NET.PointerType;

            if( ptrType == null )
            {
                Impl.IrBuilder.InsertValue( llvmDst, llvmSrc, 0 );
            }
            else
            {
                llvmDst = Impl.IrBuilder.GetStructElementPointer( llvmDst, 0 );
                ptrType = llvmDst.Type as Llvm.NET.PointerType;
                if( llvmSrc.Type != ptrType.ElementType )
                {
                    Console.WriteLine( "For \"Ptr must be a pointer to Val type!\" Assert." );
                    Console.WriteLine( "getOperand(0).getType()" );
                    Console.WriteLine( llvmSrc.Type.ToString() );
                    Console.WriteLine( "" );
                    Console.WriteLine( "cast<PointerType>(getOperand(1).getType()).getElementType()" );
                    Console.WriteLine( ptrType.ElementType );
                    Console.WriteLine( "" );
                    throw new ArgumentException( );
                }

                Impl.IrBuilder.Store( llvmSrc, llvmDst );
            }
        }

        public _Value LoadIndirect( _Value val, _Type ptrTy )
        {
            val = LoadToImmediate( val );

            if( val.Type( ).Impl.IsBoxed( ) )
            {
                var valueImpl = new ValueImpl( val.Type( ).Impl.UnderlyingBoxedType
                                             , Impl.IrBuilder.GetStructElementPointer( val.Impl.GetLLVMObject( ), 1 )
                                             , false
                                             );
                return new _Value( Module, valueImpl );
            }

            var value = Impl.IrBuilder.BitCast( val.Impl.GetLLVMObject( )
                                              , ptrTy.Impl.GetLLVMObjectForStorage( ).CreatePointerType( )
                                              );

            return new _Value( Module, new ValueImpl( ptrTy.Impl, value, false ) );
        }

        public void InsertMemCpy( _Value dst, _Value src )
        {
            dst = RevertImmediate( dst );
            _Value riSrc = RevertImmediate( src );

            //I can't do a memcopy because I can't find a
            //src address, so I do a copy by copy instead
            if( riSrc == null )
            {
                Llvm.NET.Value llvmDst = dst.Impl.GetLLVMObject( );
                Llvm.NET.Value llvmSrc = src.Impl.GetLLVMObject( );

                //INSERT FIELD BY FIELD LOAD/STORE AND RETURN
                var ptrType = ( Llvm.NET.PointerType )llvmDst.Type;
                var numFields = ( ( Llvm.NET.StructType )ptrType.ElementType ).Members.Count;
                for( uint i = 0; i < numFields; ++i )
                {
                    Llvm.NET.Value[] idxs = { Llvm.NET.ConstantInt.From( 0 ), Llvm.NET.ConstantInt.From( (int)i ) };

                    Llvm.NET.Value tmpSrc = DecorateInstructionWithDebugInfo( Impl.IrBuilder.ExtractValue( llvmSrc, i, "fieldByFieldSrcExtrsact" ) );
                    Llvm.NET.Value tmpDst = DecorateInstructionWithDebugInfo( Impl.IrBuilder.GetElementPtrInBounds( llvmDst, idxs, "fieldByFieldDstGep" ) );

                    DecorateInstructionWithDebugInfo( Impl.IrBuilder.Store( tmpSrc, tmpDst ) );
                }

                return;
            }

            src = riSrc;

            Debug.Assert( src != null && dst != null );
            Debug.Assert( src.IsPointer( ) );
            Debug.Assert( dst.IsPointer( ) );
            Debug.Assert( src.Type( ).__op_Equality( dst.Type( ) ) );

            Impl.IrBuilder.MemCpy( Module.Impl.GetLLVMObject()
                                 , dst.Impl.GetLLVMObject( )
                                 , src.Impl.GetLLVMObject( )
                                 , Llvm.NET.ConstantInt.From( dst.Type( ).GetSizeInBits( ) / 8)
                                 , 0
                                 , false
                                 );
        }

        public void InsertMemCpy( _Value dst, _Value src, _Value size, bool overlapping )
        {
            dst = LoadToImmediate( dst );
            src = LoadToImmediate( src );

            Debug.Assert( src != null && dst != null && size != null );
            Debug.Assert( src.IsPointer( ) );
            Debug.Assert( dst.IsPointer( ) );
            Debug.Assert( size.IsInteger( ) );

            if( overlapping )
            {
                Impl.IrBuilder.MemMove( Module.Impl.GetLLVMObject( )
                                      , dst.Impl.GetLLVMObject( )
                                      , src.Impl.GetLLVMObject( )
                                      , size.Impl.GetLLVMObject( )
                                      , 0
                                      , false
                                      );
            }
            else
            {
                Impl.IrBuilder.MemCpy( Module.Impl.GetLLVMObject()
                                     , dst.Impl.GetLLVMObject( )
                                     , src.Impl.GetLLVMObject( )
                                     , size.Impl.GetLLVMObject( )
                                     , 0
                                     , false
                                     );
            }
        }


        public void InsertMemSet( _Value dst, byte value )
        {
            dst = RevertImmediate( dst );
            Debug.Assert( dst != null );
            Debug.Assert( dst.IsPointer( ) );

            Impl.IrBuilder.MemSet( Module.Impl.GetLLVMObject()
                                 , dst.Impl.GetLLVMObject( )
                                 , Llvm.NET.ConstantInt.From( value )
                                 , Llvm.NET.ConstantInt.From( dst.Type( ).GetSizeInBits( ) / 8 )
                                 , 0
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
            Debug.Assert( a.IsInteger( ) || a.IsFloatingPoint( ) );
            Debug.Assert( b.IsInteger( ) || b.IsFloatingPoint( ) );
            Llvm.NET.Value retVal;

            a = LoadToImmediate( a );
            b = LoadToImmediate( b );

            Llvm.NET.Value loadedA = a.Impl.GetLLVMObject( );
            Llvm.NET.Value loadedB = b.Impl.GetLLVMObject( );
            var bldr = Impl.IrBuilder;

            if( a.IsInteger( ) && b.IsInteger( ) )
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
            else if( a.IsFloatingPoint( ) && b.IsFloatingPoint( ) )
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

            retVal = DecorateInstructionWithDebugInfo( retVal );
            return new _Value( Module, new ValueImpl( a.Type( ).Impl, retVal, true ) );
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

            Debug.Assert( val.IsInteger( ) || val.IsFloatingPoint() );
            val = LoadToImmediate( val );

            Llvm.NET.Value retVal = val.Impl.GetLLVMObject( );

            switch( unOp )
            {
            case UnaryOperator.NEG:
                if( val.IsInteger( ) )
                {
                    retVal = Impl.IrBuilder.Neg( retVal );
                }            
                else
                {
                    retVal = Impl.IrBuilder.FNeg( retVal );
                }
                break;
            case UnaryOperator.NOT:
                retVal = Impl.IrBuilder.Not( retVal );
                break;
            }
            retVal = DecorateInstructionWithDebugInfo( retVal );
            return new _Value( Module, new ValueImpl( val.Type( ).Impl, retVal, true ) );
        }
        const int SignedBase = 10;
        const int FloatBase = SignedBase + 10;

        static Dictionary<int, Llvm.NET.Predicate> PredicateMap = new Dictionary<int, Llvm.NET.Predicate>( )
        {
            [ 0 ] = Llvm.NET.Predicate.Equal,                  //llvm::CmpInst::ICMP_EQ;
            [ 1 ] = Llvm.NET.Predicate.UnsignedGreaterOrEqual, //llvm::CmpInst::ICMP_UGE;
            [ 2 ] = Llvm.NET.Predicate.UnsignedGreater,        //llvm::CmpInst::ICMP_UGT;
            [ 3 ] = Llvm.NET.Predicate.UnsignedLessOrEqual,    //llvm::CmpInst::ICMP_ULE;
            [ 4 ] = Llvm.NET.Predicate.UnsignedLess,           //llvm::CmpInst::ICMP_ULT;
            [ 5 ] = Llvm.NET.Predicate.NotEqual,               //llvm::CmpInst::ICMP_NE;

            [ SignedBase + 0 ] = Llvm.NET.Predicate.Equal,                //llvm::CmpInst::ICMP_EQ;
            [ SignedBase + 1 ] = Llvm.NET.Predicate.SignedGreaterOrEqual, //llvm::CmpInst::ICMP_SGE;
            [ SignedBase + 2 ] = Llvm.NET.Predicate.SignedGreater,        //llvm::CmpInst::ICMP_SGT;
            [ SignedBase + 3 ] = Llvm.NET.Predicate.SignedLessOrEqual,    //llvm::CmpInst::ICMP_SLE;
            [ SignedBase + 4 ] = Llvm.NET.Predicate.SignedLess,           //llvm::CmpInst::ICMP_SLT;
            [ SignedBase + 5 ] = Llvm.NET.Predicate.NotEqual,              //llvm::CmpInst::ICMP_NE;

            [ FloatBase + 0 ] = Llvm.NET.Predicate.OrderedAndEqual,              //llvm::CmpInst::FCMP_OEQ;
            [ FloatBase + 1 ] = Llvm.NET.Predicate.OrderedAndGreaterThanOrEqual, //llvm::CmpInst::FCMP_OGE;
            [ FloatBase + 2 ] = Llvm.NET.Predicate.OrderedAndGreaterThan,        //llvm::CmpInst::FCMP_OGT;
            [ FloatBase + 3 ] = Llvm.NET.Predicate.OrderedAndLessThanOrEqual,    //llvm::CmpInst::FCMP_OLE;
            [ FloatBase + 4 ] = Llvm.NET.Predicate.OrderedAndLessThan,           //llvm::CmpInst::FCMP_OLT;
            [ FloatBase + 5 ] = Llvm.NET.Predicate.OrderedAndNotEqual            //llvm::CmpInst::FCMP_ONE;
        };

        public _Value InsertCmp( int predicate, bool isSigned, _Value valA, _Value valB )
        {
            Debug.Assert( valA.IsInteger( ) || valA.IsFloatingPoint( ) );
            Debug.Assert( valB.IsInteger( ) || valB.IsFloatingPoint( ) );

            valA = LoadToImmediate( valA );
            valB = LoadToImmediate( valB );

            Llvm.NET.Value llvmValA = valA.Impl.GetLLVMObject( );
            Llvm.NET.Value llvmValB = valB.Impl.GetLLVMObject( );

            TypeImpl booleanImpl = Module.GetType( "LLVM.System.Boolean" ).Impl;

            if( valA.IsInteger( ) && valB.IsInteger( ) )
            {
                Llvm.NET.Predicate p = PredicateMap[ predicate + ( isSigned ? SignedBase : 0 ) ];
                var icmp = Impl.IrBuilder.Compare( ( Llvm.NET.IntPredicate )p, llvmValA, llvmValB, "icmp" );

                var inst = Impl.IrBuilder.ZeroExtendOrBitCast( icmp, booleanImpl.GetLLVMObject( ) );
                inst = DecorateInstructionWithDebugInfo( inst );
                return new _Value( Module, new ValueImpl( booleanImpl, inst, true ) );
            }
            else if( valA.IsFloatingPoint( ) && valB.IsFloatingPoint( ) )
            {
                Llvm.NET.Predicate p = PredicateMap[ predicate + FloatBase ];
                var cmp = Impl.IrBuilder.Compare( ( Llvm.NET.RealPredicate )p, llvmValA, llvmValB, "fcmp" );
                var value = Impl.IrBuilder.ZeroExtendOrBitCast( cmp, booleanImpl.GetLLVMObject( ) );
                value = DecorateInstructionWithDebugInfo( value );
                return new _Value( Module, new ValueImpl( booleanImpl, value, true ) );
            }
            else
            {
                Console.WriteLine( "valA:" );
                Console.WriteLine( valA.ToString( ) );
                Console.WriteLine( "valB:" );
                Console.WriteLine( valB.ToString( ) );
                throw new ApplicationException( "Parameter combination not supported for CMP Operator." );
            }
        }

        public _Value InsertZExt( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger( ) );
            val = LoadToImmediate( val );

            var retVal = Impl.IrBuilder.TruncOrBitCast( val.Impl.GetLLVMObject( ), Module.Impl.LlvmContext.GetIntType( ( uint )significantBits ) );
            retVal = DecorateInstructionWithDebugInfo( retVal );

            retVal = Impl.IrBuilder.ZeroExtendOrBitCast( retVal, ty.Impl.GetLLVMObjectForStorage( ), "zext" );
            retVal = DecorateInstructionWithDebugInfo( retVal );

            return new _Value( Module, new ValueImpl( ty.Impl, retVal, true ) );
        }

        public _Value InsertSExt( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger( ) );
            val = LoadToImmediate( val );

            var retVal = Impl.IrBuilder.TruncOrBitCast( val.Impl.GetLLVMObject( ), Module.Impl.LlvmContext.GetIntType( ( uint )significantBits ) );
            retVal = DecorateInstructionWithDebugInfo( retVal );

            retVal = Impl.IrBuilder.SignExtendOrBitCast( retVal, ty.Impl.GetLLVMObjectForStorage( ), "sext" );
            retVal = DecorateInstructionWithDebugInfo( retVal );

            return new _Value( Module, new ValueImpl( ty.Impl, retVal, true ) );
        }

        public _Value InsertTrunc( _Value val, _Type ty, int significantBits )
        {
            Debug.Assert( val.IsInteger() );
            val = LoadToImmediate( val );

            Llvm.NET.Value retVal = Impl.IrBuilder.TruncOrBitCast( val.Impl.GetLLVMObject( ), ty.Impl.GetLLVMObjectForStorage( ) );
            retVal = DecorateInstructionWithDebugInfo( retVal );

            return new _Value( Module, new ValueImpl( ty.Impl, retVal, true ) );
        }

        public _Value InsertPointerToInt( _Value val, bool skipObjectHeader )
        {
            Debug.Assert( val.IsPointer() );
            val = LoadToImmediate( val );
            _Type ty = Module.GetType( "LLVM.System.UInt32" );

            Llvm.NET.Value llvmVal = Impl.IrBuilder.PointerToInt( val.Impl.GetLLVMObject( ), ty.Impl.GetLLVMObjectForStorage( ) );

            if( skipObjectHeader && !val.Type( ).IsValueType( ) && val.Type( ).Impl.GetName( ) != "Microsoft.Zelig.Runtime.ObjectHeader" )
            {
                _Type ohTy = Module.GetType( "Microsoft.Zelig.Runtime.ObjectHeader" );
                var constantInt = Llvm.NET.ConstantInt.From( ohTy.GetSizeInBits( ) / 8 );
                llvmVal = Impl.IrBuilder.Add( llvmVal, constantInt, "headerOffsetAdd" );
            }

            return new _Value( Module, new ValueImpl( ty.Impl, llvmVal, true ) );
        }

        public _Value InsertIntToPointer( _Value val, _Type ptrTy )
        {
            Debug.Assert( val.IsInteger( ) );
            val = LoadToImmediate( val );

            Llvm.NET.Value llvmVal = val.Impl.GetLLVMObject( );

            if (!ptrTy.IsValueType( ) && ptrTy.Impl.GetName( ) != "Microsoft.Zelig.Runtime.ObjectHeader")
            {
                _Type ohTy = Module.GetType( "Microsoft.Zelig.Runtime.ObjectHeader" );
                llvmVal = Impl.IrBuilder.Sub( llvmVal, Llvm.NET.ConstantInt.From( ohTy.GetSizeInBits( ) / 8 ), "headerOffsetSub" );
            }

            llvmVal = Impl.IrBuilder.IntToPointer( llvmVal, (Llvm.NET.PointerType)ptrTy.Impl.GetLLVMObjectForStorage( ) );

            return new _Value( Module, new ValueImpl( ptrTy.Impl, llvmVal, true ) );
        }

        public _Value GetAddressAsUIntPtr( _Value val )
        {
            _Type ptrTy = Module.GetType( "LLVM.System.UIntPtr" );
            val = RevertImmediate( val );

            Debug.Assert( val != null );

            return new _Value( Module, new ValueImpl( ptrTy.Impl, val.Impl.GetLLVMObject( ), true ) );
        }

        public _Value GetBTCast( _Value val, _Type ty )
        {
            if( val.Type( ).GetBTUnderlyingType( ) == null )
                return val;

            if( ty.GetBTUnderlyingType( ) == null )
                return val;

            if( val.Type( ).GetSizeInBitsForStorage( ) != ty.GetSizeInBitsForStorage( ) )
            {
                //Needs integer cast:
                val = LoadToImmediate( val );
                _Value newVal = Owner.GetLocalStackValue( "ForParameterIntegerCast", ty );

                Llvm.NET.Value llvmVal = Impl.IrBuilder.ExtractValue( val.Impl.GetLLVMObject( ), 0 );
                var structType = ( Llvm.NET.StructType )newVal.Type( ).Impl.GetLLVMObject( );

                llvmVal = Impl.IrBuilder.IntCast( llvmVal, structType.Members[0], false );

                Impl.IrBuilder.Store( llvmVal,  Impl.IrBuilder.GetStructElementPointer( newVal.Impl.GetLLVMObject( ), 0 )  );

                return newVal;
            }
            else
            {
                _Value retVal = RevertImmediate( val );
                if( retVal == null )
                    return val;

                return new _Value( Module, new ValueImpl( ty.Impl, Impl.IrBuilder.BitCast( val.Impl.GetLLVMObject( ), ty.Impl.GetLLVMObject( ).CreatePointerType() ), false ) );
            }
        }

        public _Value ExtractFirstElementFromBasicType( _Value val )
        {
            _Type ty = Module.GetType( $"LLVM.{val.Type().Impl.GetName()}" );

            Debug.Assert( ty != null );

            Llvm.NET.Value llvmVal = val.Impl.GetLLVMObject( );

            if( llvmVal.Type.IsPointer )
            {
                llvmVal = Impl.IrBuilder.Load( llvmVal );
            }

            llvmVal = Impl.IrBuilder.ExtractValue( llvmVal, 0, ".m_value" );

            return new _Value( Module, new ValueImpl( ty.Impl, llvmVal, true ) );
        }

        public _Value InsertSIToFPFloat( _Value val )
        {
            Debug.Assert( val.IsInteger() );
            _Type ty = Module.GetType( "LLVM.System.Single" );
            val = LoadToImmediate( val );

            var value = Impl.IrBuilder.SIToFPCast( val.Impl.GetLLVMObject( ), ty.Impl.GetLLVMObject( ), "sitofp" );
            return new _Value( Module, new ValueImpl( ty.Impl, DecorateInstructionWithDebugInfo( value ), true ) );
        }

        public _Value InsertSIToFPDouble( _Value val )
        {
            Debug.Assert( val.IsInteger( ) );
            _Type ty = Module.GetType( "LLVM.System.Double" );
            val = LoadToImmediate( val );

            var value = Impl.IrBuilder.SIToFPCast( val.Impl.GetLLVMObject( ), ty.Impl.GetLLVMObject( ), "sitofp" );
            return new _Value( Module, new ValueImpl( ty.Impl, DecorateInstructionWithDebugInfo( value ), true ) );
        }

        public _Value InsertFPFloatToFPDouble( _Value val )
        {
            Debug.Assert( val.IsFloatingPoint() );
            _Type ty = Module.GetType( "LLVM.System.Double" );
            val = LoadToImmediate( val );

            var value = Impl.IrBuilder.FPExt( val.Impl.GetLLVMObject( ), ty.Impl.GetLLVMObject( ), "fpext" );
            return new _Value( Module, new ValueImpl( ty.Impl, DecorateInstructionWithDebugInfo( value ), true ) );
        }

        public _Value InsertFPToUI( _Value val, _Type ty )
        {
            Debug.Assert( val.IsFloatingPoint( ) );
            _Type intType = ty;
            val = LoadToImmediate( val );

            var value = Impl.IrBuilder.FPToUICast( val.Impl.GetLLVMObject( ), intType.Impl.GetLLVMObject( ), "fptoui" );
            return new _Value( Module, new ValueImpl( intType.Impl, DecorateInstructionWithDebugInfo( value ), true ) );
        }

        public void InsertUnconditionalBranch( _BasicBlock bb )
        {
            DecorateInstructionWithDebugInfo( Impl.IrBuilder.Branch( bb.Impl.GetLLVMObject( ) ) );
        }

        public void InsertConditionalBranch( _Value cond, _BasicBlock trueBB, _BasicBlock falseBB )
        {
            Debug.Assert( cond.IsInteger( ) );
            cond = LoadToImmediate( cond );

            //Review: Testing all the bits for now. We need to check its always valid to trunc the condition
            // to the first bit if we want to change it. That being said, most of the times this should be
            // get rid on the instructions conv pass from LLVM.
            Llvm.NET.Value vA = cond.Impl.GetLLVMObject( );
            Llvm.NET.Value vB = Llvm.NET.ConstantInt.From( cond.Type( ).Impl.GetLLVMObject( ), 0, false );

            Llvm.NET.Value condVal = DecorateInstructionWithDebugInfo( Impl.IrBuilder.Compare(Llvm.NET.IntPredicate.NotEqual, vA, vB, "icmpe" ) );

            DecorateInstructionWithDebugInfo( Impl.IrBuilder.Branch( condVal, trueBB.Impl.GetLLVMObject(), falseBB.Impl.GetLLVMObject() ) );
        }

        public void InsertSwitchAndCases( _Value cond, _BasicBlock defaultBB, List<int> casesValues, List<_BasicBlock> casesBBs )
        {
            Debug.Assert( cond.IsInteger( ) );
            cond = LoadToImmediate( cond );
            var si = Impl.IrBuilder.Switch( cond.Impl.GetLLVMObject( ), defaultBB.Impl.GetLLVMObject( ), ( uint )casesBBs.Count );
            si = (Llvm.NET.Instructions.Switch)( DecorateInstructionWithDebugInfo( si ) );

            for( int i = 0; i < casesBBs.Count; ++i )
            {
                si.AddCase( Llvm.NET.ConstantInt.From( casesValues[ i ] ), casesBBs[ i ].Impl.GetLLVMObject() );
            }
        }

        void LoadParams( _Function func, IList<_Value> args, IList<Llvm.NET.Value> parameters )
        {
            for( int i = 0; i < args.Count; ++i )
            {
                args[ i ] = LoadToImmediate( args[ i ] );

                Llvm.NET.Value llvmValue = args[ i ].Impl.GetLLVMObject( );
                Llvm.NET.TypeRef pty = ((Llvm.NET.Function)func.Impl.GetLLVMObject( )).Parameters[ i ].Type;
                TypeImpl tiPty = TypeImpl.GetTypeImpl( pty );

                //IntPtr/UIntPtr can be casted to anything
                if( args[ i ].Type( ).Impl.GetName( ) == "System.IntPtr"
                    || args[ i ].Type( ).Impl.GetName( ) == "System.UIntPtr" )
                {
                    args[ i ] = RevertImmediate( args[ i ] );
                    Debug.Assert( args[ i ] != null );
                    llvmValue = args[ i ].Impl.GetLLVMObject( );
                    llvmValue = Impl.IrBuilder.Load( Impl.IrBuilder.BitCast( llvmValue, pty.CreatePointerType() ) );
                }

                //Anything can be casted to IntPtr/UIntPtr
                if( tiPty.GetName( ) == "System.IntPtr"
                    || tiPty.GetName( ) == "System.UIntPtr" )
                {
                    args[ i ] = RevertImmediate( args[ i ] );
                    Debug.Assert( args[ i ] != null );
                    llvmValue = args[ i ].Impl.GetLLVMObject( );
                    llvmValue = Impl.IrBuilder.Load( Impl.IrBuilder.BitCast( llvmValue, pty.CreatePointerType() ) );
                }

                if( args[ i ].Type( ).Impl.GetName( ).StartsWith( "LLVM." ) )
                {
                    var constVal = llvmValue as Llvm.NET.Constant;
                    if( constVal != null )
                    {
                        llvmValue = Module.Impl.GetLLVMObject().AddGlobal( llvmValue.Type, true, Llvm.NET.Linkage.Internal, constVal );
                    }
                    else
                    {
                        args[ i ] = RevertImmediate( args[ i ] );
                        Debug.Assert( args[ i ] != null );
                        llvmValue = args[ i ].Impl.GetLLVMObject( );
                    }

                    llvmValue = Impl.IrBuilder.Load( Impl.IrBuilder.BitCast( llvmValue, pty.CreatePointerType( ) ) );
                }

                parameters.Add( llvmValue );
            }
        }

        public _Value InsertCall( _Function func, List<_Value > args )
        {
            List< Llvm.NET.Value> parameters = new List<Llvm.NET.Value>();
            LoadParams( func, args, parameters );

            Llvm.NET.Value retVal = DecorateInstructionWithDebugInfo( Impl.IrBuilder.Call( func.Impl.GetLLVMObject( ), parameters ) );

            var llvmFunc = (Llvm.NET.Function)(func.Impl.GetLLVMObject( ));
            if( llvmFunc.ReturnType.IsVoid )
                return null;

            return new _Value( Module, new ValueImpl( TypeImpl.GetTypeImpl( llvmFunc.ReturnType ), retVal, true ) );
        }

        public _Value InsertIndirectCall( _Function func, _Value ptr, List<_Value> args )
        {
            List< Llvm.NET.Value > parameters = new List<Llvm.NET.Value>();
            LoadParams( func, args, parameters );

            ptr = CastToFunctionPointer( ptr, func.Type( ) );

            Llvm.NET.Value retVal = DecorateInstructionWithDebugInfo( Impl.IrBuilder.Call( ptr.Impl.GetLLVMObject( ), parameters ) );

            var llvmFunc = ( Llvm.NET.Function )( func.Impl.GetLLVMObject( ) );
            if( llvmFunc.ReturnType.IsVoid )
                return null;

            return new _Value( Module, new ValueImpl( TypeImpl.GetTypeImpl( llvmFunc.ReturnType ), retVal, true ) );
        }

        public _Value GetFunctionArgument( _Function func, int n )
        {
            var llvmFunc = ( Llvm.NET.Function )func.Impl.GetLLVMObject( );
            Llvm.NET.Argument AI = llvmFunc.Parameters[ n ];
            return new _Value( Module, new ValueImpl( func.Type( ).Impl.FunctionArgs[ n ], AI, true ) );
        }

        static TypeImpl SetValuesForByteOffsetAccess( TypeImpl ty, List<uint> values, int offset, ref string fieldName )
        {
            for( int i = 0; i < ty.Fields.Count; ++i )
            {
                bool firstFieldOfManagedObject = ( i == 0 && !ty.IsValueType( ) && ty != TypeImpl.GetTypeImpl( "Microsoft.Zelig.Runtime.ObjectHeader" ) );
                int thisFieldSize = (int)ty.Fields[ i ].MemberType.GetSizeInBitsForStorage( ) / 8;
                if( firstFieldOfManagedObject )
                    thisFieldSize = (int)ty.Fields[ i ].MemberType.GetSizeInBits( ) / 8;
                int curOffset = offset - ( int )( ty.Fields[ i ].Offset + thisFieldSize );

                if( curOffset < 0 )
                {
                    values.Add( ty.Fields[ i ].FinalIdx );
                    //On non value types (nor objectheader which is special) we force inspection of the
                    //super class
                    fieldName = ty.Fields[ i ].Name;
                    if( !firstFieldOfManagedObject && ( -curOffset ) == thisFieldSize )
                        return ty.Fields[ i ].MemberType;
                    return SetValuesForByteOffsetAccess( ty.Fields[ i ].MemberType, values, curOffset + thisFieldSize, ref fieldName );
                }
            }

            throw new ApplicationException( "Invalid offset for field access!!" );
        }

        public _Value GetField( _Value obj, _Type zTy, _Type fieldType, int offset )
        {
            obj = LoadToImmediate( obj );
            Debug.Assert( Module.Impl.LlvmContext == Llvm.NET.Context.CurrentContext );

            //Special case for boxed types
            if( obj.Type( ).Impl.IsBoxed( ) )
            {
                var structType = ( Llvm.NET.StructType )( obj.Type( ).Impl.GetLLVMObject( ) );
                Llvm.NET.Value[] indexValues = { Llvm.NET.ConstantInt.From( 0 ), Llvm.NET.ConstantInt.From( 1 ) };

                var gep = Impl.IrBuilder.GetElementPtr( obj.Impl.GetLLVMObject( ), indexValues, "BoxedFieldAccessGep" );
                _Value rVal = new _Value( Module, new ValueImpl( TypeImpl.GetTypeImpl( structType.Members[ 1 ] ), DecorateInstructionWithDebugInfo( gep ), false ) );

                obj = rVal;
            }

            TypeImpl finalTimpl = obj.Type( ).Impl;

            //Special case for indexing into pointer to value types
            if( obj.Type( ).Impl.UnderlyingPointerType != null &&
                obj.Type( ).Impl.UnderlyingPointerType.IsValueType( ) )
            {
                finalTimpl = obj.Type( ).Impl.UnderlyingPointerType;
            }

            List<uint> values = new List<uint>();
            string fieldName = string.Empty;
            TypeImpl timpl = SetValuesForByteOffsetAccess( finalTimpl, values, offset, ref fieldName );

            List<Llvm.NET.Value> valuesForGep = new List<Llvm.NET.Value>();
            valuesForGep.Add( Llvm.NET.ConstantInt.From( 0 ) );

            for( int i = 0; i < values.Count; ++i )
            {
                valuesForGep.Add( Llvm.NET.ConstantInt.From( values[ i ] ) );
            }

            var gep2 = Impl.IrBuilder.GetElementPtr( obj.Impl.GetLLVMObject( ), valuesForGep, finalTimpl.GetName( ) + "." + fieldName );
            _Value retVal = new _Value( Module, new ValueImpl( timpl, DecorateInstructionWithDebugInfo( gep2 ), false ) );

            return retVal;
        }

        public _Value IndexLLVMArray( _Value obj, _Value idx )
        {
            Debug.Assert( Module.Impl.LlvmContext == Llvm.NET.Context.CurrentContext );
            idx = LoadToImmediate( idx );

            Llvm.NET.Value[] idxs = { Llvm.NET.ConstantInt.From( 0 ), idx.Impl.GetLLVMObject( ) };
            Llvm.NET.Value retVal = DecorateInstructionWithDebugInfo( Impl.IrBuilder.GetElementPtr( obj.Impl.GetLLVMObject( ), idxs ) );

            var arrayType = ( Llvm.NET.ArrayType )( obj.Type( ).Impl.GetLLVMObject( ) );
            return new _Value( Module, new ValueImpl( TypeImpl.GetTypeImpl( arrayType.ElementType ), retVal, false ) );
        }

        public void InsertRet( _Value val )
        {
            if( val == null )
            {
                DecorateInstructionWithDebugInfo( Impl.IrBuilder.Return() );
            }
            else
            {
                DecorateInstructionWithDebugInfo( Impl.IrBuilder.Return( LoadToImmediate( val ).Impl.GetLLVMObject() ) );
            }
        }

        internal Llvm.NET.BasicBlock Block { get; }

        _Module Module;
        int DebugCurCol;
        int DebugCurLine;
        _Function Owner;
        Llvm.NET.DebugInfo.SubProgram CurDiSubProgram;
        internal BasicBlockImpl Impl;
    }

    internal class BasicBlockImpl
    {
        internal BasicBlockImpl( Llvm.NET.BasicBlock block )
        {
            LlvmBasicBlock = block;
            IrBuilder = new Llvm.NET.InstructionBuilder( block );
        }

        internal Llvm.NET.InstructionBuilder IrBuilder { get; }
        internal Llvm.NET.BasicBlock GetLLVMObject( ) => LlvmBasicBlock;
        private readonly Llvm.NET.BasicBlock LlvmBasicBlock;
    }

}

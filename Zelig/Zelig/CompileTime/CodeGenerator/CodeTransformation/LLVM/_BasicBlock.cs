using Llvm.NET;
using Llvm.NET.DebugInfo;
using Llvm.NET.Instructions;
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
            var func = manager.GetOrInsertFunction( method );
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
            // from, which the Zelig inlining doesn't currently provide - all we get
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

        /// <summary>
        /// Ensure that at least default debug info has been set for this block. If debug info has already been created,
        /// this is a no-op.
        /// </summary>
        /// <param name="manager">Owner of the LLVM module.</param>
        /// <param name="method">Representation of the method in which this block is defined.</param>
        public void EnsureDebugInfo( LLVMModuleManager manager, MethodRepresentation method )
        {
            if( CurDILocation == null )
            {
                SetDebugInfo( manager, method, null );
            }

            Debug.Assert( CurDILocation != null );
            Debug.Assert( CurDISubProgram != null );
        }

        public _Value GetMethodArgument( int index, _Type type )
        {
            var llvmFunc = (Function)Owner.LlvmValue;
            Value value = llvmFunc.Parameters[ index ];
            return new _Value( Module, type, value );
        }

        public _Value InsertAlloca( string name, _Type type )
        {
            Value value = IrBuilder.Alloca( type.DebugType ).RegisterName( name );
            _Type pointerType = Module.GetOrInsertPointerType( type );
            return new _Value( Module, pointerType, value );
        }

        public _Value InsertLoad( _Value value )
        {
            var resultValue = IrBuilder.Load( value.LlvmValue )
                                       .SetDebugLocation( CurDILocation );
            return new _Value( Module, value.Type.UnderlyingType, resultValue );
        }

        private void InsertStore(Value src, Value dst)
        {
            var ptrType = dst.NativeType as IPointerType;
            if (src.NativeType != ptrType.ElementType)
            {
                Console.WriteLine("For \"Ptr must be a pointer to Val type!\" Assert.");
                Console.WriteLine("getOperand(0).getType()");
                Console.WriteLine(src.NativeType );
                Console.WriteLine("");
                Console.WriteLine("cast<IPointerType>(getOperand(1).getType()).getElementType()");
                Console.WriteLine(ptrType.ElementType);
                Console.WriteLine("");
                throw new ApplicationException();
            }

            IrBuilder.Store(src, dst);
        }

        public void InsertStore(_Value src, _Value dst)
        {
            InsertStore(src.LlvmValue, dst.LlvmValue);
        }

        public _Value LoadIndirect( _Value val, _Type loadedType )
        {
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

            return new _Value( Module, pointerType, resultVal );
        }

        public _Value InsertPhiNode( _Type type )
        {
            var builder = IrBuilder;

            // Ensure phi nodes are grouped at the top of the block.
            var lastInstruction = LlvmBasicBlock.LastInstruction;
            if( ( lastInstruction != null ) && !( lastInstruction is PhiNode ) )
            {
                // We have an instruction that isn't a phi node, so move the insertion point to just
                // after any existing phi nodes.
                foreach( Instruction instruction in LlvmBasicBlock.Instructions )
                {
                    if( !( instruction is PhiNode ) )
                    {
                        builder = new InstructionBuilder( LlvmBasicBlock );
                        builder.PositionBefore( instruction );
                        break;
                    }
                }
            }

            var phiNode = builder.PhiNode( type.DebugType );
            return new _Value( Module, type, phiNode );
        }

        public void AddPhiIncomingValue( _Value phiNode, _Value value, _BasicBlock origin )
        {
            PhiNode node = (PhiNode)phiNode.LlvmValue;
            node.AddIncoming( value.LlvmValue, origin.LlvmBasicBlock );
        }

        public void InsertMemCpy( _Value dst, _Value src, _Value size, bool overlapping )
        {
            if( overlapping )
            {
                IrBuilder.MemMove( Module.LlvmModule, dst.LlvmValue, src.LlvmValue, size.LlvmValue, 0, false );
            }
            else
            {
                IrBuilder.MemCpy( Module.LlvmModule, dst.LlvmValue, src.LlvmValue, size.LlvmValue, 0, false );
            }
        }

        public void InsertMemSet( _Value dst, _Value value, _Value size )
        {
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
            return new _Value( Module, a.Type, retVal );
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
            return new _Value( Module, val.Type, retVal );
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
            _Type booleanImpl = Module.GetNativeBoolType();

            if( (valA.IsInteger && valB.IsInteger) ||
                (valA.IsPointer && valB.IsPointer) )
            {
                Predicate p = PredicateMap[ predicate + ( isSigned ? SignedBase : 0 ) ];
                var cmp = IrBuilder.Compare( ( IntPredicate )p, valA.LlvmValue, valB.LlvmValue )
                                   .SetDebugLocation( CurDILocation );
                return new _Value( Module, booleanImpl, cmp );
            }

            if( valA.IsFloatingPoint && valB.IsFloatingPoint )
            {
                Predicate p = PredicateMap[ predicate + FloatBase ];
                var cmp = IrBuilder.Compare( ( RealPredicate )p, valA.LlvmValue, valB.LlvmValue )
                                   .SetDebugLocation( CurDILocation );
                return new _Value( Module, booleanImpl, cmp );
            }

            Console.WriteLine( "valA:" );
            Console.WriteLine( valA.ToString( ) );
            Console.WriteLine( "valB:" );
            Console.WriteLine( valB.ToString( ) );
            throw new ApplicationException( "Parameter combination not supported for CMP Operator." );
        }

        public _Value InsertZExt( _Value val, _Type ty, int significantBits )
        {
            Value retVal = val.LlvmValue;

            // TODO: Remove this workaround once issue #123 has been resolved.
            if ( significantBits != val.Type.SizeInBits )
            {
                retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, Module.LlvmModule.Context.GetIntType( ( uint )significantBits ) )
                                  .SetDebugLocation( CurDILocation );
            }

            retVal = IrBuilder.ZeroExtendOrBitCast( retVal, ty.DebugType )
                              .SetDebugLocation( CurDILocation );

            return new _Value( Module, ty, retVal );
        }

        public _Value InsertSExt( _Value val, _Type ty, int significantBits )
        {
            Value retVal = val.LlvmValue;

            // TODO: Remove this workaround once issue #123 has been resolved.
            if ( significantBits != val.Type.SizeInBits )
            {
                retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, Module.LlvmModule.Context.GetIntType( ( uint )significantBits ) )
                                  .SetDebugLocation( CurDILocation );
            }

            retVal = IrBuilder.SignExtendOrBitCast( retVal, ty.DebugType )
                              .SetDebugLocation( CurDILocation );

            return new _Value( Module, ty, retVal );
        }

        public _Value InsertTrunc( _Value val, _Type type, int significantBits )
        {
            // TODO: Remove this workaround once issue #123 has been resolved.
            if (significantBits < type.SizeInBits)
            {
                return InsertZExt(val, type, significantBits);
            }

            Value retVal = IrBuilder.TruncOrBitCast( val.LlvmValue, type.DebugType )
                                    .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, retVal );
        }

        public _Value InsertBitCast( _Value val, _Type type )
        {
            var bitCast = IrBuilder.BitCast( val.LlvmValue, type.DebugType )
                                   .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, bitCast );
        }

        public _Value InsertPointerToInt( _Value val, _Type intType )
        {
            Value llvmVal = IrBuilder.PointerToInt( val.LlvmValue, intType.DebugType )
                                     .SetDebugLocation( CurDILocation );
            return new _Value( Module, intType, llvmVal );
        }

        public _Value InsertIntToPointer( _Value val, _Type pointerType )
        {
            Value llvmVal = IrBuilder.IntToPointer( val.LlvmValue, (IPointerType)pointerType.DebugType )
                                     .SetDebugLocation( CurDILocation );
            return new _Value( Module, pointerType, llvmVal );
        }

        public _Value InsertIntToFP( _Value val, _Type type )
        {
            Value result;
            if( val.Type.IsSigned )
            {
                result = IrBuilder.SIToFPCast( val.LlvmValue, type.DebugType )
                                  .SetDebugLocation( CurDILocation );
            }
            else
            {
                result = IrBuilder.UIToFPCast( val.LlvmValue, type.DebugType )
                                  .SetDebugLocation( CurDILocation );
            }

            return new _Value( Module, type, result );
        }

        public _Value InsertFPExt( _Value val, _Type type )
        {
            var value = IrBuilder.FPExt( val.LlvmValue, type.DebugType )
                                 .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, value );
        }

        public _Value InsertFPTrunc( _Value val, _Type type )
        {
            var value = IrBuilder.FPTrunc( val.LlvmValue, type.DebugType )
                                 .SetDebugLocation( CurDILocation );

            return new _Value( Module, type, value );
        }

        public _Value InsertFPToInt( _Value val, _Type intType )
        {
            Value result;
            if( val.Type.IsSigned )
            {
                result = IrBuilder.FPToSICast( val.LlvmValue, intType.DebugType )
                                  .SetDebugLocation( CurDILocation );
            }
            else
            {
                result = IrBuilder.FPToUICast( val.LlvmValue, intType.DebugType )
                                  .SetDebugLocation( CurDILocation );
            }

            return new _Value( Module, intType, result );
        }

        public void InsertUnreachable()
        {
            IrBuilder.Unreachable( ).SetDebugLocation( CurDILocation );
        }

        public void InsertUnconditionalBranch( _BasicBlock bb )
        {
            IrBuilder.Branch( bb.LlvmBasicBlock )
                     .SetDebugLocation( CurDILocation );
        }

        public void InsertConditionalBranch( _Value cond, _BasicBlock trueBB, _BasicBlock falseBB )
        {
            IrBuilder.Branch( cond.LlvmValue, trueBB.LlvmBasicBlock, falseBB.LlvmBasicBlock )
                     .SetDebugLocation( CurDILocation );
        }

        public void InsertSwitchAndCases( _Value cond, _BasicBlock defaultBB, List<int> casesValues, List<_BasicBlock> casesBBs )
        {
            Debug.Assert( cond.IsInteger );

            var si = IrBuilder.Switch( cond.LlvmValue, defaultBB.LlvmBasicBlock, ( uint )casesBBs.Count )
                              .SetDebugLocation( CurDILocation );

            for( int i = 0; i < casesBBs.Count; ++i )
            {
                si.AddCase( Module.LlvmModule.Context.CreateConstant( casesValues[ i ] ), casesBBs[ i ].LlvmBasicBlock );
            }
        }

        public _Value InsertCall(_Function func, _Type returnType, List<_Value> args)
        {
            List<Value> parameters = args.Select((_Value value) => { return value.LlvmValue; }).ToList();

            Value retVal = IrBuilder.Call(func.LlvmValue, parameters)
                                    .SetDebugLocation(CurDILocation);

            // Don't bother wrapping the return if we won't use it.
            if (returnType.DebugType.IsVoid)
            {
                return null;
            }

            return new _Value(Module, returnType, retVal);
        }

        public _Value InsertIndirectCall(_Value ptr, _Type returnType, List<_Value> args)
        {
            List<Value> parameters = args.Select((_Value value) => { return value.LlvmValue; }).ToList();

            // Manufacture a function prototype.
            var paramTypes = parameters.Select((Value value) => { return value.NativeType; });
            ITypeRef llvmFuncType = Module.LlvmContext.GetFunctionType(returnType.DebugType.NativeType, paramTypes);
            ITypeRef funcPointerType = llvmFuncType.CreatePointerType();

            // Build a call to the code pointer.
            Value extracted = IrBuilder.ExtractValue(ptr.LlvmValue, 0)
                                       .SetDebugLocation(CurDILocation);
            Value pointer = IrBuilder.BitCast(extracted, funcPointerType)
                                     .SetDebugLocation(CurDILocation);
            Value retVal = IrBuilder.Call(pointer, parameters)
                                    .SetDebugLocation(CurDILocation);

            // Don't bother wrapping the return if we won't use it.
            if (returnType.DebugType.IsVoid)
            {
                return null;
            }

            return new _Value(Module, returnType, retVal);
        }

        static _Type SetValuesForByteOffsetAccess( _Type ty, List<uint> values, int offset, out string fieldName )
        {
            for( int i = 0; i < ty.Fields.Count; ++i )
            {
                TypeField thisField = ty.Fields[ i ];

                // The first field of a reference type is always a super-class, except for the root System.Object.
                bool fieldIsParent = ( i == 0 && !ty.IsValueType && ty != _Type.GetOrInsertTypeImpl( ty.Module, ty.Module.TypeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ObjectHeader ) );
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

        public _Value GetFieldAddress( _Value objAddress, int offset, _Type fieldType )
        {
            Context ctx = Module.LlvmModule.Context;

            Debug.Assert( objAddress.Type.IsPointer, "Cannot get field address from a loaded value type." );

            _Type underlyingType = objAddress.Type.UnderlyingType;
            List<Value> valuesForGep = new List<Value>();

            // Add an initial 0 value to index into the address.
            valuesForGep.Add( ctx.CreateConstant( 0 ) );

            string fieldName = string.Empty;

            // Special case: For boxed types, index into the wrapped value type.
            if ( ( underlyingType != null ) && underlyingType.IsBoxed )
            {
                fieldName = underlyingType.Fields[1].Name;
                underlyingType = underlyingType.UnderlyingType;
                valuesForGep.Add(ctx.CreateConstant(1));
            }

            // Don't index into the type's fields if we want the outer struct. This is always true for primitive values,
            // and usually true for boxed values.
            if (underlyingType.IsPrimitiveType || (underlyingType == fieldType))
            {
                Debug.Assert( offset == 0, "Primitive and boxed types can only have one member, and it must be at offset zero." );
            }
            else
            {
                List<uint> values = new List<uint>();
                underlyingType = SetValuesForByteOffsetAccess( underlyingType, values, offset, out fieldName );

                for( int i = 0; i < values.Count; ++i )
                {
                    valuesForGep.Add( ctx.CreateConstant( values[ i ] ) );
                }
            }

            // Special case: No-op trivial GEP instructions, and just return the original address.
            if( valuesForGep.Count == 1 )
            {
                return objAddress;
            }

            var gep = IrBuilder.GetElementPtr( objAddress.LlvmValue, valuesForGep )
                               .RegisterName( $"{objAddress.LlvmValue.Name}.{fieldName}" )
                               .SetDebugLocation( CurDILocation );

            _Type pointerType = Module.GetOrInsertPointerType( underlyingType );
            return new _Value( Module, pointerType, gep );
        }

        public _Value IndexLLVMArray(_Value obj, _Value idx, _Type elementType)
        {
            Value[] idxs = { Module.LlvmModule.Context.CreateConstant( 0 ), idx.LlvmValue };
            Value retVal = IrBuilder.GetElementPtr( obj.LlvmValue, idxs )
                                    .SetDebugLocation( CurDILocation );

            _Type pointerType = Module.GetOrInsertPointerType(elementType);
            return new _Value(Module, pointerType, retVal);
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
                IrBuilder.Return( val.LlvmValue )
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
            return new _Value( Module, val.Type, retVal );
        }

        public _Value InsertAtomicCmpXchg( _Value ptr, _Value cmp, _Value val )
        {
            // LLVM cmpxchg instruction returns { ty, i1 } for { oldValue, isSuccess }
            var retVal = IrBuilder.AtomicCmpXchg( ptr.LlvmValue, cmp.LlvmValue, val.LlvmValue );

            // And we only want the old value
            var oldVal = IrBuilder.ExtractValue( retVal, 0 );

            oldVal = oldVal.SetDebugLocation( CurDILocation );
            return new _Value( Module, val.Type, oldVal );
        }

        public void SetVariableName( _Value value, VariableExpression expression )
        {
            string name = expression.DebugName?.Name;
            if( !string.IsNullOrWhiteSpace( name ) )
            {
                value.LlvmValue.RegisterName( name );
            }
        }

        internal InstructionBuilder IrBuilder { get; }

        internal BasicBlock LlvmBasicBlock { get; }

        internal DISubProgram CurDISubProgram => CurDILocation?.Scope as DISubProgram;

        internal DILocation CurDILocation;

        private readonly _Module Module;
        private readonly _Function Owner;
    }
}

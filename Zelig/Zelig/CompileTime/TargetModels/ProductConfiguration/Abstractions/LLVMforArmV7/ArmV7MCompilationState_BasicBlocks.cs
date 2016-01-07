//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define GENERATE_ONLY_TYPESYSTEM_AND_FUNCTION_DEFINITIONS

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Zelig.LLVM;

    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public partial class ArmV7MCompilationState
    {
        private _BasicBlock m_basicBlock;

        private LLVM._BasicBlock GetOrInsertBasicBlock( IR.BasicBlock block )
        {
            _BasicBlock llvmBlock;
            if( m_blocks.TryGetValue( block, out llvmBlock ) )
            {
                return llvmBlock;
            }

            llvmBlock = m_function.GetOrInsertBasicBlock( block.ToShortString( ) );
            m_blocks.Add( block, llvmBlock );
            return llvmBlock;
        }

        public override void EmitCodeForBasicBlock( IR.BasicBlock bb )
        {
#if GENERATE_ONLY_TYPESYSTEM_AND_FUNCTION_DEFINITIONS
            m_manager.TurnOffCompilationAndValidation( );
            m_function.SetExternalLinkage( );
            m_basicBlock = null;
#else // GENERATE_ONLY_TYPESYSTEM_AND_FUNCTION_DEFINITIONS
            m_basicBlock = GetOrInsertBasicBlock( bb );

            // If this is the entry block, set up local variable storage.
            if( bb == m_basicBlocks[ 0 ] )
            {
                var defChains = m_cfg.DataFlow_DefinitionChains;
                var useChains = m_cfg.DataFlow_UseChains;

                foreach( IR.VariableExpression exp in m_variables )
                {
                    Debug.Assert( exp.SpanningTreeIndex >= 0, "Encountered unreachable expression; expected removal in prior phase: " + exp );

                    // Skip unused variables. These can exist when a value is aliased, but not otherwise referenced.
                    if( ( defChains[exp.SpanningTreeIndex].Length == 0 ) &&
                        ( useChains[exp.SpanningTreeIndex].Length == 0 ) )
                    {
                        continue;
                    }

                    Debug.Assert( !m_localValues.ContainsKey( exp ), "Found multiple definitions for expression: " + exp );
                    CreateValueCache( exp, defChains, useChains );
                }
            }

            foreach( var op in bb.Operators )
            {
                if ( EmitCodeForBasicBlock_ShouldSkip( op ) )
                {
                    continue;
                }

                TranslateOperator( op, bb );
            }
#endif // GENERATE_ONLY_TYPESYSTEM_AND_FUNCTION_DEFINITIONS
        }

        protected override bool EmitCodeForBasicBlock_ShouldSkip( IR.Operator op )
        {
            return base.EmitCodeForBasicBlock_ShouldSkip( op );
        }

        protected override uint EmitCodeForBasicBlock_EstimateMinimumSize( IR.Operator op )
        {
            return sizeof(uint);
        }

        protected override void EmitCodeForBasicBlock_EmitOperator( IR.Operator op )
        {
            throw new Exception( "EmitCodeForBasicBlock_EmitOperator not implemented." );
        }

        protected override void EmitCodeForBasicBlock_FlushOperators( )
        {
        }

        private void CreateValueCache(IR.VariableExpression expr, IR.Operator[][] defChains,  IR.Operator[][] useChains)
        {
            Debug.Assert(expr.AliasedVariable == expr, "Aliased variables are currently unsupported.");

            // Get or create a slot for the backing variable.
            if (!m_localValues.ContainsKey(expr))
            {
                ValueCache valueCache;
                if (RequiresAddress(expr, defChains, useChains))
                {
                    _Value address = m_function.GetLocalStackValue(m_method, m_basicBlock, expr, m_manager);
                    valueCache = new ValueCache(expr, address);
                }
                else
                {
                    valueCache = new ValueCache(expr, m_manager.GetOrInsertType(expr.Type));
                }

                m_localValues[expr] = valueCache;
            }
        }

        private bool RequiresAddress(IR.VariableExpression expr, IR.Operator[][] defChains, IR.Operator[][] useChains)
        {
            // LLVM needs an address to attach debug info, so give an address to all named variables.
            if (!string.IsNullOrWhiteSpace(expr.DebugName?.Name))
            {
                return true;
            }

            // If the variable has more than one definition, give it an address instead of a phi node.
            if (defChains[expr.SpanningTreeIndex].Length > 1)
            {
                return true;
            }

            // If any operator takes the variable's address, it must be addressable.
            foreach (IR.Operator useOp in useChains[expr.SpanningTreeIndex])
            {
                // Address assignment operators take their argument's address by definition.
                if (useOp is IR.AddressAssignmentOperator)
                {
                    return true;
                }

                // Field accessors need the first argument to be an address, so value types need to stay indirected.
                if ((useOp is IR.StoreInstanceFieldOperator) ||
                    (useOp is IR.LoadInstanceFieldOperator) ||
                    (useOp is IR.LoadInstanceFieldAddressOperator))
                {
                    if ((expr == useOp.FirstArgument) && (expr.Type is TS.ValueTypeRepresentation))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private _Value GetImmediate( IR.BasicBlock block, IR.Expression expression )
        {
            return GetValue( block, expression, wantImmediate: true, allowLoad: true );
        }

        private _Value GetAddress( IR.BasicBlock block, IR.Expression expression )
        {
            return GetValue( block, expression, wantImmediate: false, allowLoad: false );
        }

        private _Value GetValue( IR.BasicBlock block, IR.Expression exp, bool wantImmediate, bool allowLoad )
        {
            if( !IsSuitableForLLVMTranslation( exp ) )
            {
                throw new System.InvalidOperationException( "Llilum output too low level." );
            }

            if( exp is IR.VariableExpression )
            {
                _BasicBlock llvmBlock = GetOrInsertBasicBlock( block );
                ValueCache valueCache = m_localValues[ exp ];

                // If we don't need to load the value, just return the address.
                if( !wantImmediate )
                {
                    Debug.Assert( valueCache.IsAddressable );
                    return valueCache.Address;
                }

                // Never cache addressable values since we don't easily know when they'll be modified.
                // Instead, reload at each operator. Unnecessary loads will be optimized out by LLVM.
                if( valueCache.IsAddressable )
                {
                    return m_basicBlock.InsertLoad( valueCache.Address );
                }

                // If we already have a cached value for this expression there's no need to load it again.
                _Value value = valueCache.GetValueFromBlock( llvmBlock );
                if( value != null )
                {
                    return value;
                }

                // This value isn't addressable so it must have been set in a previous block.
                IR.BasicBlock idom = m_immediateDominators[ block.SpanningTreeIndex ];
                if( idom == block )
                {
                    // This is the entry block so there's no predecessor to search.
                    throw new InvalidOperationException( $"Encountered use of expression with no definition. Expression {exp} in {block.Owner.Method}" );
                }

                value = GetValue( idom, exp, wantImmediate, allowLoad );
                valueCache.SetValueForBlock( llvmBlock, value );
                return value;
            }

            if ( exp is IR.ConstantExpression )
            {
                IR.ConstantExpression ce = ( IR.ConstantExpression )exp;

                if( ce.Value == null )
                {
                    Debug.Assert( wantImmediate, "Cannot take the address of a null constant." );
                    return m_manager.Module.GetNullValue( m_manager.GetOrInsertType( ce.Type ) );
                }

                if( ce.Type.IsInteger || ce.Type.IsFloatingPoint )
                {
                    if( ce.SizeOfValue == 0 )
                    {
                        throw new System.InvalidOperationException( "Scalar constant with 0 bits width." );
                    }

                    Debug.Assert( wantImmediate, "Cannot take the address of a scalar constant." );
                    _Type scalarType = m_manager.GetOrInsertType( ce.Type );

                    object value = ce.Value;
                    if( ce.Type.IsInteger )
                    {
                        // Ensure integer types are converted to ulong. This will also catch enums and IntPtr/UIntPtr.
                        ulong intValue;
                        ce.GetAsRawUlong(out intValue);
                        value = intValue;
                    }

                    return m_manager.Module.GetScalarConstant( scalarType, value );
                }

                IR.DataManager.DataDescriptor dd = ce.Value as IR.DataManager.DataDescriptor;
                if( dd != null )
                {
                    Debug.Assert( wantImmediate || ( dd.Context is TS.ValueTypeRepresentation ), "Cannot take the address of a global object reference." );

                    // This call always returns a pointer to global storage, so value types may need to be loaded.
                    _Value value = m_manager.GlobalValueFromDataDescriptor( dd, true );
                    if( wantImmediate && ( dd.Context is TS.ValueTypeRepresentation ) )
                    {
                        value = m_basicBlock.InsertLoad( value );
                    }

                    return value;
                }

                throw new Exception( "Constant type not supported." );
            }

            throw new Exception( "Expression type not supported." + exp );
        }

        private void WarnUnimplemented( string msg )
        {
            msg = "Unimplemented operator: " + msg;

            var color = Console.ForegroundColor; 
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine( msg );
            Console.ForegroundColor = color; 

            //Trick to list in-code missing operators.
            //m_basicBlock.InsertASMString( "; " + msg );
        }

        private void OutputStringInline( string str )
        {
            m_basicBlock.InsertASMString( "####################################################" );
            m_basicBlock.InsertASMString( "# " + str );
            m_basicBlock.InsertASMString( "####################################################" );
        }

        private void TranslateOperator( IR.Operator op, IR.BasicBlock bb )
        {
            if( ShouldSkipOperator( op ) )
            {
                return;
            }

            // Load Debug metadata
            // Miguel: (Hack to remove processor.cs epilogue/prologue debug data)
            if( op.DebugInfo != null && !op.DebugInfo.SrcFileName.EndsWith( "ProcessorARMv7M.cs" ) )
            {
                m_basicBlock.SetDebugInfo( m_manager, m_method, op );
            }

            OutputStringInline( op.ToString( ) );

            //ALU
            if( op is IR.AbstractBinaryOperator )
            {
                Translate_AbstractBinaryOperator( ( IR.AbstractBinaryOperator )op );
            }
            else if( op is IR.AbstractUnaryOperator )
            {
                //Todo: Add unary "Finite" operation, which in fact is "CKfinite" from the checks.
                Translate_AbstractUnaryOperator( ( IR.AbstractUnaryOperator )op );
            }
            //Conversions
            else if( op is IR.ConversionOperator )
            {
                Translate_ConversionOperator( ( IR.ConversionOperator )op );
            }
            else if( op is IR.ConvertOperator )
            {
                Translate_ConvertOperator( ( IR.ConvertOperator )op );
            }
            //Store-Load operators
            else if( op is IR.SingleAssignmentOperator )
            {
                Translate_SingleAssignmentOperator( ( IR.SingleAssignmentOperator )op );
            }
            else if( op is IR.AddressAssignmentOperator )
            {
                Translate_AddressAssignmentOperator( ( IR.AddressAssignmentOperator )op );
            }
            else if( op is IR.InitialValueOperator )
            {
                Translate_InitialValueOperator( ( IR.InitialValueOperator )op );
            }
            else if( op is IR.CompareAndSetOperator )
            {
                Translate_CompareAndSetOperator( ( IR.CompareAndSetOperator )op );
            }
            else if( op is IR.LoadIndirectOperator )
            {
                Translate_LoadIndirectOperator( ( IR.LoadIndirectOperator )op );
            }
            else if( op is IR.StoreIndirectOperator )
            {
                Translate_StoreIndirectOperator( ( IR.StoreIndirectOperator )op );
            }
            else if( op is IR.StoreInstanceFieldOperator )
            {
                Translate_StoreInstanceFieldOperator( ( IR.StoreInstanceFieldOperator )op );
            }
            else if( op is IR.LoadInstanceFieldOperator )
            {
                Translate_LoadInstanceFieldOperator( ( IR.LoadInstanceFieldOperator )op );
            }
            else if( op is IR.LoadInstanceFieldAddressOperator )
            {
                Translate_LoadInstanceFieldAddressOperator( ( IR.LoadInstanceFieldAddressOperator )op );
            }
            else if( op is IR.StoreElementOperator )
            {
                Translate_StoreElementOperator( ( IR.StoreElementOperator )op );
            }
            else if( op is IR.LoadElementOperator )
            {
                Translate_LoadElementOperator( ( IR.LoadElementOperator )op );
            }
            else if( op is IR.LoadElementAddressOperator )
            {
                Translate_LoadElementAddressOperator( ( IR.LoadElementAddressOperator )op );
            }
            //Control flow operators
            else if( op is IR.UnconditionalControlOperator )
            {
                Translate_UnconditionalControlOperator( ( IR.UnconditionalControlOperator )op );
            }
            else if( op is IR.BinaryConditionalControlOperator )
            {
                Translate_BinaryConditionalControlOperator( ( IR.BinaryConditionalControlOperator )op );
            }
            else if( op is IR.CompareConditionalControlOperator )
            {
                Translate_CompareConditionalControlOperator( ( IR.CompareConditionalControlOperator )op );
            }
            else if( op is IR.MultiWayConditionalControlOperator )
            {
                Translate_MultiWayConditionalControlOperator( ( IR.MultiWayConditionalControlOperator )op );
            }
            else if( op is IR.ReturnControlOperator )
            {
                Translate_ReturnControlOperator( ( IR.ReturnControlOperator )op );
            }
            else if( op is IR.LeaveControlOperator )
            {
                Translate_LeaveControlOperator( ( IR.LeaveControlOperator )op );
            }
            else if( op is IR.DeadControlOperator )
            {
                Translate_DeadControlOperator( ( IR.DeadControlOperator )op );
            }
            //Calls
            else if( op is IR.StaticCallOperator )
            {
                Translate_StaticCallOperator( ( IR.StaticCallOperator )op );
            }
            else if( op is IR.InstanceCallOperator )
            {
                Translate_InstanceCallOperator( ( IR.InstanceCallOperator )op );
            }
            else if( op is IR.IndirectCallOperator )
            {
                Translate_IndirectCallOperator( ( IR.IndirectCallOperator )op );
            }
            //Other
            else
            {
                WarnUnimplemented( op.ToString( ) );
            }
        }

        private static bool ShouldSkipOperator( IR.Operator op )
        {
            if( ( op is IR.NullCheckOperator ) ||
                ( op is IR.OutOfBoundCheckOperator ) ||
                ( op is IR.CompilationConstraintsOperator ) ||
                ( op is IR.NopOperator ) )
            {
                return true;
            }

            return false;
        }

        private bool IsSuitableForLLVMTranslation( IR.Expression varExp )
        {
            if( varExp is IR.PhysicalRegisterExpression ||
                varExp is IR.StackLocationExpression )
            {
                return false;
            }

            return true;
        }

        private void EnsureSameType( ref _Value valA, ref _Value valB )
        {
            if( valA.IsPointer != valB.IsPointer )
            {
                // Convert the pointer parameter to an integer to match the other.
                valA = ConvertValueToALUOperableType( valA );
                valB = ConvertValueToALUOperableType( valB );
            }
            else if( valA.IsPointer )
            {
                // Ensure pointer types match.
                valB = m_basicBlock.InsertBitCast( valB, valA.Type );
            }

            if ( valA.IsInteger && valB.IsInteger )
            {
                if (valA.Type.SizeInBits < valB.Type.SizeInBits )
                {
                    if( valA.Type.IsSigned )
                    {
                        valA = m_basicBlock.InsertSExt( valA, valB.Type, valA.Type.SizeInBits );
                    }
                    else
                    {
                        valA = m_basicBlock.InsertZExt( valA, valB.Type, valA.Type.SizeInBits );
                    }
                }
                else if( valA.Type.SizeInBits > valB.Type.SizeInBits )
                {
                    if( valA.Type.IsSigned )
                    {
                        valB = m_basicBlock.InsertSExt( valB, valA.Type, valB.Type.SizeInBits );
                    }
                    else
                    {
                        valB = m_basicBlock.InsertZExt( valB, valA.Type, valB.Type.SizeInBits );
                    }
                }
            }
        }

        private _Value ConvertValueToALUOperableType( _Value val )
        {
            // LLVM doesn't accept pointers as operands for arithmetic operations, so convert them to integers.
            if( val.IsPointer )
            {
                _Type intPtrType = m_manager.GetOrInsertType( m_wkt.System_UInt32 );
                return m_basicBlock.InsertPointerToInt( val, intPtrType );
            }

            return val;
        }

        private _Value ConvertValueToStoreToTarget( _Value val, LLVM._Type targetType )
        {
            // Trivial case: Value is already the desired type. We compare the inner types directly because the outer
            // types will be the same between pointer types for arrays and strings, while the inner types won't.
            if ( val.LlvmValue.NativeType == targetType.DebugType.NativeType )
            {
                return val;
            }

            if( val.Type.IsPointer && targetType.IsPointer )
            {
                return m_basicBlock.InsertBitCast( val, targetType );
            }

            if( val.Type.IsInteger && targetType.IsInteger )
            {
                if( val.Type.SizeInBits < targetType.SizeInBits )
                {
                    if( val.Type.IsSigned )
                    {
                        return m_basicBlock.InsertSExt( val, targetType, val.Type.SizeInBits );
                    }
                    else
                    {
                        return m_basicBlock.InsertZExt( val, targetType, val.Type.SizeInBits );
                    }
                }

                if( val.Type.SizeInBits > targetType.SizeInBits )
                {
                    return m_basicBlock.InsertTrunc( val, targetType, val.Type.SizeInBits );
                }

                return m_basicBlock.InsertBitCast( val, targetType );
            }

            if( val.Type.IsInteger && targetType.IsPointer )
            {
                return m_basicBlock.InsertIntToPointer( val, targetType );
            }

            if( val.Type.IsPointer && targetType.IsInteger )
            {
                return m_basicBlock.InsertPointerToInt( val, targetType );
            }

            if( val.Type.IsFloatingPoint && targetType.IsFloatingPoint )
            {
                if( val.Type.SizeInBits > targetType.SizeInBits )
                {
                    return m_basicBlock.InsertFPTrunc( val, targetType );
                }
                else
                {
                    return m_basicBlock.InsertFPExt( val, targetType );
                }
            }

            if( val.Type.IsFloatingPoint && targetType.IsInteger )
            {
                return m_basicBlock.InsertFPToInt( val, targetType );
            }

            if( val.Type.IsInteger && targetType.IsFloatingPoint )
            {
                return m_basicBlock.InsertIntToFP( val, targetType );
            }

            throw new InvalidOperationException( "Invalid type cast." );
        }

        private void StoreValue( ValueCache dst, _Value src )
        {
            _Value convertedSrc = ConvertValueToStoreToTarget( src, dst.Type );
            m_basicBlock.SetVariableName( convertedSrc, dst.Expression );

            if( dst.IsAddressable )
            {
                StoreValue( dst.Address, convertedSrc );
            }
            else
            {
                dst.SetValueForBlock( m_basicBlock, convertedSrc );
            }
        }

        private void StoreValue( _Value dst, _Value src )
        {
            m_basicBlock.InsertStore( ConvertValueToStoreToTarget( src, dst.Type.UnderlyingType ), dst );
        }

        private void Translate_AbstractBinaryOperator( IR.AbstractBinaryOperator op )
        {
            if( !(op is IR.BinaryOperator) )
            {
                throw new Exception( "Unhandled Binary Op: " + op );
            }

            _Value valA = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value valB = GetImmediate( op.BasicBlock, op.SecondArgument );
            valA = ConvertValueToALUOperableType( valA );
            valB = ConvertValueToALUOperableType( valB );

            // TODO: Add support for overflow exceptions.
            EnsureSameType( ref valA, ref valB );
            _Value result = m_basicBlock.InsertBinaryOp( ( int )op.Alu, valA, valB, op.Signed );
            StoreValue( m_localValues[ op.FirstResult ], result );
        }

        private void Translate_AbstractUnaryOperator( IR.AbstractUnaryOperator op )
        {
            if( !( op is IR.UnaryOperator ) )
            {
                throw new Exception( "Unhandled Unary Op: " + op );
            }

            // TODO: Add support for overflow exceptions.
            _Value argument = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value value = ConvertValueToALUOperableType( argument );
            _Value result = m_basicBlock.InsertUnaryOp( ( int )op.Alu, value, op.Signed );
            StoreValue( m_localValues[ op.FirstResult ], result );
        }

        private void Translate_ConversionOperator( IR.ConversionOperator op )
        {
            // TODO: Add support for overflow exceptions
            _Value argument = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value value = ConvertValueToALUOperableType( argument );
            _Type resultType = m_manager.GetOrInsertType( op.FirstResult.Type );

            if( op.FirstResult.Type == m_wkt.System_IntPtr ||
                op.FirstResult.Type == m_wkt.System_UIntPtr ||
                op.FirstResult.Type is TS.PointerTypeRepresentation )
            {
                resultType = m_manager.GetOrInsertType( m_wkt.System_UInt32 );
            }

            if( op is IR.ZeroExtendOperator )
            {
                value = m_basicBlock.InsertZExt( value, resultType, 8 * ( int )op.SignificantSize );
            }
            else if( op is IR.SignExtendOperator )
            {
                value = m_basicBlock.InsertSExt( value, resultType, 8 * ( int )op.SignificantSize );
            }
            else if( op is IR.TruncateOperator )
            {
                value = m_basicBlock.InsertTrunc( value, resultType, 8 * ( int )op.SignificantSize );
            }
            else
            {
                throw new Exception( "Unimplemented Conversion Operator: " + op.ToString( ) );
            }

            StoreValue( m_localValues[ op.FirstResult ], value );
        }

        private void Translate_ConvertOperator( IR.ConvertOperator op )
        {
            // TODO: Add support for overflow exceptions
            _Value argument = GetImmediate( op.BasicBlock, op.FirstArgument );
            StoreValue( m_localValues[ op.FirstResult ], argument );
        }

        private void Translate_SingleAssignmentOperator( IR.SingleAssignmentOperator op )
        {
            _Value value = GetImmediate( op.BasicBlock, op.FirstArgument );
            StoreValue( m_localValues[ op.FirstResult ], value );
        }

        private void Translate_AddressAssignmentOperator( IR.AddressAssignmentOperator op )
        {
            _Value address = GetAddress( op.BasicBlock, op.FirstArgument );
            StoreValue( m_localValues[ op.FirstResult ], address);
        }

        private void Translate_InitialValueOperator( IR.InitialValueOperator op )
        {
            int index = op.FirstResult.Number;
            if (m_method is TS.StaticMethodRepresentation)
            {
                --index;
            }

            _Type type = m_manager.GetOrInsertType( op.FirstResult.Type );
            _Value argument = m_basicBlock.GetMethodArgument( index, type );
            StoreValue( m_localValues[ op.FirstResult ], argument );
        }

        private void Translate_CompareAndSetOperator( IR.CompareAndSetOperator op )
        {
            _Value left = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value right = GetImmediate( op.BasicBlock, op.SecondArgument );
            EnsureSameType( ref left, ref right );

            _Value value = m_basicBlock.InsertCmp( ( int )op.Condition, op.Signed, left, right );
            StoreValue( m_localValues[ op.FirstResult ], value );
        }

        private void Translate_LoadIndirectOperator( IR.LoadIndirectOperator op )
        {
            _Value argument = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value address = m_basicBlock.LoadIndirect( argument, m_manager.GetOrInsertType( op.Type ) );
            _Value value = m_basicBlock.InsertLoad( address );
            StoreValue( m_localValues[ op.FirstResult ], value );
        }

        private void Translate_StoreIndirectOperator( IR.StoreIndirectOperator op )
        {
            _Value argument = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value value = GetImmediate( op.BasicBlock, op.SecondArgument );
            _Value address = m_basicBlock.LoadIndirect( argument, m_manager.GetOrInsertType( op.Type ) );
            StoreValue( address, value );
        }

        private _Value GetInstanceAddress( IR.FieldOperator op )
        {
            IR.Expression instance = op.FirstArgument;
            if ( instance.Type is TS.ValueTypeRepresentation )
            {
                return GetAddress( op.BasicBlock, instance );
            }
            else
            {
                return GetImmediate( op.BasicBlock, instance );
            }
        }

        private void Translate_StoreInstanceFieldOperator( IR.StoreInstanceFieldOperator op )
        {
            _Value objAddress = GetInstanceAddress( op );
            _Value value = GetImmediate( op.BasicBlock, op.SecondArgument );
            _Type fieldType = m_manager.GetOrInsertType( op.Field.FieldType );
            _Value fieldAddress = m_basicBlock.GetFieldAddress( objAddress, op.Field.Offset, fieldType );
            StoreValue( fieldAddress, value );
        }

        private void Translate_LoadInstanceFieldOperator( IR.LoadInstanceFieldOperator op )
        {
            _Value objAddress = GetInstanceAddress( op );
            _Type fieldType = m_manager.GetOrInsertType( op.Field.FieldType );
            _Value fieldAddress = m_basicBlock.GetFieldAddress( objAddress, op.Field.Offset, fieldType );
            _Value value = m_basicBlock.InsertLoad( fieldAddress );
            StoreValue( m_localValues[ op.FirstResult ], value );
        }

        private void Translate_LoadInstanceFieldAddressOperator( IR.LoadInstanceFieldAddressOperator op )
        {
            _Value objAddress = GetInstanceAddress( op );
            _Type fieldType = m_manager.GetOrInsertType( op.Field.FieldType );
            _Value fieldAddress = m_basicBlock.GetFieldAddress( objAddress, op.Field.Offset, fieldType );
            StoreValue( m_localValues[ op.FirstResult ], fieldAddress );
        }

        private _Value GetElementAddress(IR.ElementOperator op)
        {
            var arrayType = (TS.ArrayReferenceTypeRepresentation)op.FirstArgument.Type;
            _Type elementType = m_manager.GetOrInsertType(arrayType.ContainedType);

            _Value array = GetImmediate(op.BasicBlock, op.FirstArgument);
            _Value nativeArray = m_basicBlock.GetFieldAddress(array, (int)m_wkt.System_Array.Size, null);
            _Value index = GetImmediate(op.BasicBlock, op.SecondArgument);
            return m_basicBlock.IndexLLVMArray(nativeArray, index, elementType);
        }

        private void Translate_LoadElementOperator(IR.LoadElementOperator op)
        {
            _Value elementAddress = GetElementAddress(op);
            _Value value = m_basicBlock.InsertLoad(elementAddress);
            StoreValue(m_localValues[op.FirstResult], value);
        }

        private void Translate_LoadElementAddressOperator(IR.LoadElementAddressOperator op)
        {
            _Value elementAddress = GetElementAddress(op);
            StoreValue(m_localValues[op.FirstResult], elementAddress);
        }

        private void Translate_StoreElementOperator(IR.StoreElementOperator op)
        {
            _Value elementAddress = GetElementAddress(op);
            _Value value = GetImmediate(op.BasicBlock, op.ThirdArgument);
            StoreValue(elementAddress, value);
        }

        private void Translate_UnconditionalControlOperator( IR.UnconditionalControlOperator op )
        {
            m_basicBlock.InsertUnconditionalBranch( GetOrInsertBasicBlock( op.TargetBranch ) );
        }

        private void Translate_BinaryConditionalControlOperator( IR.BinaryConditionalControlOperator op )
        {
            _Value left = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value zero = m_manager.Module.GetNullValue( left.Type );
            _Value condition = m_basicBlock.InsertCmp( (int)IR.CompareAndSetOperator.ActionCondition.NE, false, left, zero );
            _BasicBlock taken = GetOrInsertBasicBlock( op.TargetBranchTaken );
            _BasicBlock notTaken = GetOrInsertBasicBlock( op.TargetBranchNotTaken );
            m_basicBlock.InsertConditionalBranch( condition, taken, notTaken );
        }

        private void Translate_CompareConditionalControlOperator( IR.CompareConditionalControlOperator op )
        {
            _Value left = GetImmediate( op.BasicBlock, op.FirstArgument );
            _Value right = GetImmediate( op.BasicBlock, op.SecondArgument );
            EnsureSameType( ref left, ref right );

            _Value condition = m_basicBlock.InsertCmp( ( int )op.Condition, op.Signed, left, right );
            _BasicBlock taken = GetOrInsertBasicBlock( op.TargetBranchTaken );
            _BasicBlock notTaken = GetOrInsertBasicBlock( op.TargetBranchNotTaken );
            m_basicBlock.InsertConditionalBranch( condition, taken, notTaken );
        }

        private void Translate_MultiWayConditionalControlOperator( IR.MultiWayConditionalControlOperator op )
        {
            List<int> caseValues = new List<int>( );
            List<LLVM._BasicBlock> caseBBs = new List<LLVM._BasicBlock>( );

            for( int i = 0; i < op.Targets.Length; ++i )
            {
                caseValues.Add( i );
                caseBBs.Add( GetOrInsertBasicBlock( op.Targets[ i ] ) );
            }

            _Value argument = GetImmediate( op.BasicBlock, op.FirstArgument );
            _BasicBlock defaultBlock = GetOrInsertBasicBlock( op.TargetBranchNotTaken );
            m_basicBlock.InsertSwitchAndCases( argument, defaultBlock, caseValues, caseBBs );
        }

        private void Translate_ReturnControlOperator( IR.ReturnControlOperator op )
        {
            switch( op.Arguments.Length )
            {
            case 0:
                m_basicBlock.InsertRet( null );
                break;

            case 1:
                _Value result = GetImmediate( op.BasicBlock, op.FirstArgument );
                m_basicBlock.InsertRet( result );
                break;

            default:
                throw new System.InvalidOperationException( "ReturnControlOperator with more than one arg not supported. " + op );
            }
        }

        private void Translate_LeaveControlOperator( IR.LeaveControlOperator op )
        {
            // Jump to a finally block.
            m_basicBlock.InsertUnconditionalBranch( GetOrInsertBasicBlock( op.TargetBranch ) );
        }

        private void Translate_DeadControlOperator( IR.DeadControlOperator op )
        {
            // Basic Block marked as dead code by Zelig.
            m_basicBlock.InsertUnreachable();
        }

        private bool ReplaceMethodCallWithIntrinsic(
            TS.MethodRepresentation method,
            List<_Value> convertedArgs,
            out _Value result )
        {
            TS.WellKnownMethods wkm = m_cfg.TypeSystem.WellKnownMethods;
            result = null;

            // System.Buffer.InternalMemoryCopy(byte*, byte*, int) => llvm.memcpy
            // System.Buffer.InternalBackwardMemoryCopy(byte*, byte*, int) => llvm.memmove
            if ((method == wkm.System_Buffer_InternalMemoryCopy) ||
                (method == wkm.System_Buffer_InternalBackwardMemoryCopy))
            {
                bool overlapping = method != wkm.System_Buffer_InternalMemoryCopy;

                _Value src = convertedArgs[ 0 ];
                _Value dst = convertedArgs[ 1 ];
                _Value size = convertedArgs[ 2 ];
                m_basicBlock.InsertMemCpy( dst, src, size, overlapping );
                return true;
            }

            // Microsoft.Zelig.Runtime.Memory.Fill(byte*, int, byte) => llvm.memset
            if( method == wkm.Microsoft_Zelig_Runtime_Memory_Fill )
            {
                _Value dst = convertedArgs[ 0 ];
                _Value size = convertedArgs[ 1 ];
                _Value value = convertedArgs[ 2 ];
                m_basicBlock.InsertMemSet( dst, value, size );
                return true;
            }

            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalAdd(ref int, int) => llvm.atomicrmw add
            // Note: there's no built-in support for 64bit interlocked methods at the moment.
            if( method == wkm.InterlockedImpl_InternalAdd_int )
            {
                _Value ptr = convertedArgs[ 0 ];
                _Value val = convertedArgs[ 1 ];
                _Value oldVal = m_basicBlock.InsertAtomicAdd( ptr, val );
                // Because LLVM atomicrmw returns the old value, in order to match the behavior of Interlocked.Add
                // We need to calculate the new value to return.
                result = m_basicBlock.InsertBinaryOp( (int)IR.BinaryOperator.ALU.ADD, oldVal, val, true );
                return true;
            }

            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref int, int)       => llvm.atomicrmw xchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref float, float)   => llvm.atomicrmw xchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref IntPtr, IntPtr) => llvm.atomicrmw xchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref T, T)           => llvm.atomicrmw xchg
            // Note: there's no built-in support for 64bit interlocked methods at the moment.
            if ((method == wkm.InterlockedImpl_InternalExchange_int) ||
                (method == wkm.InterlockedImpl_InternalExchange_float) ||
                (method == wkm.InterlockedImpl_InternalExchange_IntPtr) ||
                (method.IsGenericInstantiation && method.GenericTemplate == wkm.InterlockedImpl_InternalExchange_Template))
            {
                _Value ptr = convertedArgs[ 0 ];
                _Value val = convertedArgs[ 1 ];
                _Type type = val.Type;

                // AtomicXchg only supports integer types, so need to convert them if necessary
                _Type intType = m_manager.GetOrInsertType( m_wkt.System_Int32 );
                _Type intPtrType = m_manager.Module.GetOrInsertPointerType( intType );

                if(!type.IsInteger)
                {
                    ptr = m_basicBlock.InsertBitCast( ptr, intPtrType );

                    if(type.IsPointer)
                    {
                        val = m_basicBlock.InsertPointerToInt( val, intType );
                    }
                    else
                    {
                        val = m_basicBlock.InsertBitCast( val, intType );
                    }
                }

                result = m_basicBlock.InsertAtomicXchg( ptr, val );
                if(result.Type != type)
                {
                    if(type.IsPointer)
                    {
                        result = m_basicBlock.InsertIntToPointer( result, type );
                    }
                    else
                    {
                        result = m_basicBlock.InsertBitCast( result, type );
                    }
                }

                return true;
            }

            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref int, int, int)          => llvm.compxchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref float, float, float)    => llvm.compxchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref IntPtr, IntPtr, IntPtr) => llvm.compxchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref T, T, T)                => llvm.compxchg
            // Note: there's no built-in support for 64bit interlocked methods at the moment.
            if ((method == wkm.InterlockedImpl_InternalCompareExchange_int) ||
                (method == wkm.InterlockedImpl_InternalCompareExchange_float) ||
                (method == wkm.InterlockedImpl_InternalCompareExchange_IntPtr) ||
                (method.IsGenericInstantiation && method.GenericTemplate == wkm.InterlockedImpl_InternalCompareExchange_Template))
            {
                _Value ptr = convertedArgs[ 0 ];
                _Value val = convertedArgs[ 1 ];
                _Value cmp = convertedArgs[ 2 ];
                _Type type = val.Type;

                // AtomicCmpXchg only supports integer types, so need to convert them if necessary
                _Type intType = m_manager.GetOrInsertType( m_wkt.System_Int32 );
                _Type intPtrType = m_manager.Module.GetOrInsertPointerType( intType );

                if(!type.IsInteger)
                {
                    ptr = m_basicBlock.InsertBitCast( ptr, intPtrType );

                    if(type.IsPointer)
                    {
                        val = m_basicBlock.InsertPointerToInt( val, intType );
                        cmp = m_basicBlock.InsertPointerToInt( cmp, intType );
                    }
                    else
                    {
                        val = m_basicBlock.InsertBitCast( val, intType );
                        cmp = m_basicBlock.InsertBitCast( cmp, intType );
                    }
                }

                result = m_basicBlock.InsertAtomicCmpXchg( ptr, cmp, val );
                if(result.Type != type)
                {
                    if(type.IsPointer)
                    {
                        result = m_basicBlock.InsertIntToPointer( result, type );
                    }
                    else
                    {
                        result = m_basicBlock.InsertBitCast( result, type );
                    }
                }

                return true;
            }

            return false;
        }

        private void BuildMethodCallInstructions( IR.CallOperator op, bool callIndirect )
        {
            List<_Value> args = new List<_Value>( );

            TS.MethodRepresentation method = op.TargetMethod;
            int firstArgument = ( method is TS.StaticMethodRepresentation ) ? 1 : 0;
            int indirectAdjust = 0;

            if (callIndirect)
            {
                firstArgument = ((IR.IndirectCallOperator)op).IsInstanceCall ? 0 : 1;
                indirectAdjust = 1;
            }

            for( int i = firstArgument; i < method.ThisPlusArguments.Length; ++i )
            {
                TS.TypeRepresentation typeRep = method.ThisPlusArguments[ i ];
                _Type type = m_manager.GetOrInsertType( typeRep );
                _Value argument = GetImmediate( op.BasicBlock, op.Arguments[ i + indirectAdjust ] );
                _Value convertedArg = ConvertValueToStoreToTarget( argument, type );
                args.Add( convertedArg );
            }

            _Value result;
            if( !ReplaceMethodCallWithIntrinsic( method, args, out result ) )
            {
                _Function targetFunc = m_manager.GetOrInsertFunction( method );
                _Type returnType = m_manager.GetOrInsertType(method.ReturnType);

                if( method.Flags.HasFlag( TS.MethodRepresentation.Attributes.PinvokeImpl ) )
                {
                    targetFunc.SetExternalLinkage( );
                }

                if (callIndirect)
                {
                    _Value callAddress = GetImmediate(op.BasicBlock, op.Arguments[0]);
                    result = m_basicBlock.InsertIndirectCall(callAddress, returnType, args);
                }
                else
                {
                    result = m_basicBlock.InsertCall(targetFunc, returnType, args);
                }
            }

            if( op.Results.Length == 1 )
            {
                StoreValue( m_localValues[ op.FirstResult ], result );
            }
            else if( op.Results.Length > 1 )
            {
                throw new System.InvalidOperationException( "More than one return values are not handled." );
            }
        }

        private void Translate_StaticCallOperator( IR.StaticCallOperator op )
        {
            BuildMethodCallInstructions( op, callIndirect: false );
        }

        private void Translate_InstanceCallOperator( IR.InstanceCallOperator op )
        {
            BuildMethodCallInstructions( op, callIndirect: false );
        }

        private void Translate_IndirectCallOperator( IR.IndirectCallOperator op )
        {
            BuildMethodCallInstructions( op, callIndirect: true );
        }
    }
}

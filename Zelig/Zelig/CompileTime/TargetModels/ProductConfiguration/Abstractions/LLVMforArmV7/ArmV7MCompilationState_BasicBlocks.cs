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

    public partial class ArmV7MCompilationState : IR.ImageBuilders.CompilationState
    {
        public LLVM._BasicBlock GetOrInsertBasicBlock( IR.BasicBlock bb )
        {
            return m_function.GetOrInsertBasicBlock( bb.ToShortString( ) );
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

        private _BasicBlock m_basicBlock;
        private List<_Value> m_arguments;
        private List<_Value> m_results;

        private _Value GetValue( IR.Expression exp, bool wantImmediate )
        {
            if( exp is IR.VariableExpression )
            {
                _Value value = m_localValues[ ((IR.VariableExpression)exp).AliasedVariable ];
                if( wantImmediate )
                {
                    value = m_basicBlock.LoadToImmediate( value );
                }

                return value;
            }
            else if( exp is IR.ConstantExpression )
            {
                IR.ConstantExpression ce = ( IR.ConstantExpression )exp;

                if( ce.Value == null )
                {
                    Debug.Assert( wantImmediate, "Cannot take the address of a null constant." );
                    return m_manager.Module.GetNullPointer( m_manager.GetOrInsertType( ce.Type ) );
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
                        value = m_basicBlock.LoadToImmediate( value );
                    }

                    return value;
                }

                throw new Exception( "Constant type not supported." );
            }
            else
            {
                throw new Exception( "Expression type not supported." + exp );
            }
        }

        private void WarnUnimplemented( string msg )
        {
            msg = "Unimplemented operator: " + msg;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine( msg );
            Console.ResetColor( );

            //Trick to list in-code missing operators.
            //m_basicBlock.InsertASMString( "; " + msg );
        }

        private void OutputStringInline( string str )
        {
            m_basicBlock.InsertASMString( "####################################################" );
            m_basicBlock.InsertASMString( "# " + str );
            m_basicBlock.InsertASMString( "####################################################" );
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
                foreach( IR.VariableExpression exp in m_variables )
                {
                    var aliased = exp.AliasedVariable;
                    if( !m_localValues.ContainsKey( aliased ) )
                    {
                        m_localValues[ aliased ] = m_function.GetLocalStackValue( m_method, m_basicBlock, exp, m_manager );
                    }
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

            m_arguments = new List<_Value>( );
            m_results = new List<_Value>( );

            foreach( IR.Expression varExp in op.Arguments )
            {
                if( !IsSuitableForLLVMTranslation( varExp ) )
                {
                    //throw new System.InvalidOperationException( "ZELIG TOO NEAR ARM: Op argument in physical register" );
                }

                // REVIEW: Is there a cleaner way to determine which operators want addresses? Long term, we'll want to
                // get this information from the code flow graph that's already been analyzed in an earlier phase.
                bool wantImmediate = true;
                if( op is IR.AddressAssignmentOperator )
                {
                    wantImmediate = false;
                }
                else if( ( op is IR.StoreInstanceFieldOperator ) ||
                         ( op is IR.LoadInstanceFieldOperator ) ||
                         ( op is IR.LoadInstanceFieldAddressOperator ) )
                {
                    // Field accessors need the first argument to be an address, so value types need to stay indirected.
                    if( ( varExp == op.FirstArgument ) && ( varExp.Type is TS.ValueTypeRepresentation ) )
                    {
                        wantImmediate = false;
                    }
                }

                Debug.Assert( wantImmediate || !( varExp is IR.TemporaryVariableExpression ), "Cannot take the address of a temporary variable." );
                _Value value = GetValue( varExp, wantImmediate );

                m_arguments.Add( value );
            }

            foreach( IR.VariableExpression varExp in op.Results )
            {
                if( !IsSuitableForLLVMTranslation( varExp ) )
                {
                    //throw new System.InvalidOperationException( "ZELIG TOO NEAR ARM: Op result to physical register" );
                }

                m_results.Add( GetValue( varExp, wantImmediate: false ) );
            }

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
            else if( op is IR.PhiOperator )
            {
                Translate_PhiOperator( ( IR.PhiOperator )op );
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

        void MatchIntegerLengths( ref _Value valA, ref _Value valB )
        {
            valA = ConvertValueToALUOperableType( valA );
            valB = ConvertValueToALUOperableType( valB );

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

        _Value ConvertValueToALUOperableType( _Value val )
        {
            // LLVM doesn't accept pointers as operands for arithmetic operations, so convert them to integers.
            if( val.IsPointer )
            {
                _Type intPtrType = m_manager.GetOrInsertType( m_wkt.System_UInt32 );
                return m_basicBlock.InsertPointerToInt( val, intPtrType );
            }

            return val;
        }

        _Value ConvertValueToStoreToTarget( _Value val, LLVM._Type targetType )
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

        void StoreValue( _Value dst, _Value src )
        {
            m_basicBlock.InsertStore( dst, ConvertValueToStoreToTarget( src, dst.Type.UnderlyingType ) );
        }

        private void Translate_AbstractBinaryOperator( IR.AbstractBinaryOperator op )
        {
            if( !(op is IR.BinaryOperator) )
            {
                throw new Exception( "Unhandled Binary Op: " + op );
            }

            _Value valA = m_arguments[ 0 ];
            _Value valB = m_arguments[ 1 ];

            // TODO: Add support for overflow exceptions.
            MatchIntegerLengths( ref valA, ref valB );
            _Value result = m_basicBlock.InsertBinaryOp( ( int )op.Alu, valA, valB, op.Signed );
            StoreValue( m_results[ 0 ], result );
        }

        private void Translate_AbstractUnaryOperator( IR.AbstractUnaryOperator op )
        {
            if( !( op is IR.UnaryOperator ) )
            {
                throw new Exception( "Unhandled Unary Op: " + op );
            }

            // TODO: Add support for overflow exceptions.
            _Value val = ConvertValueToALUOperableType( m_arguments[ 0 ] );
            _Value result = m_basicBlock.InsertUnaryOp( ( int )op.Alu, val, op.Signed );
            StoreValue( m_results[ 0 ], result );
        }

        private void Translate_ConversionOperator( IR.ConversionOperator op )
        {
            _Value value = ConvertValueToALUOperableType( m_arguments[ 0 ] );
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

            StoreValue( m_results[ 0 ], value );
        }

        private void Translate_ConvertOperator( IR.ConvertOperator op )
        {
            // TODO: Add support for overflow exceptions
            _Value value = ConvertValueToALUOperableType( m_arguments[ 0 ] );
            StoreValue( m_results[ 0 ], value );
        }

        private void Translate_SingleAssignmentOperator( IR.SingleAssignmentOperator op )
        {
            StoreValue( m_results[ 0 ], m_arguments[ 0 ] );
        }

        private void Translate_AddressAssignmentOperator( IR.AddressAssignmentOperator op )
        {
            StoreValue( m_results[ 0 ], m_arguments[ 0 ] );
        }

        private void Translate_InitialValueOperator( IR.InitialValueOperator op )
        {
            IR.VariableExpression exp = op.FirstResult;
            if( exp is IR.PhiVariableExpression )
            {
                exp = exp.AliasedVariable;
            }

            int index = exp.Number;
            if (m_method is TS.StaticMethodRepresentation)
            {
                --index;
            }

            m_basicBlock.InsertStoreArgument( m_results[0], index );
        }

        private void Translate_PhiOperator( IR.PhiOperator op )
        {
            // FUTURE: Nothing to do here since all variables are stored on the stack. In the future, this operator may
            // reference immediate values, at which time we'll need to build a proper phi node.
        }

        _Value DoCmpOp( _Value valA, _Value valB, int cond, bool signed )
        {
            valA = ConvertValueToALUOperableType( valA );
            valB = ConvertValueToALUOperableType( valB );
            MatchIntegerLengths( ref valA, ref valB );
            return m_basicBlock.InsertCmp( cond, signed, valA, valB );
        }

        private void Translate_CompareAndSetOperator( IR.CompareAndSetOperator op )
        {
            _Value value = DoCmpOp( m_arguments[ 0 ], m_arguments[ 1 ], ( int )op.Condition, op.Signed );
            StoreValue( m_results[ 0 ], value );
        }

        private void Translate_LoadIndirectOperator( IR.LoadIndirectOperator op )
        {
            _Value address = m_basicBlock.LoadIndirect( m_arguments[ 0 ], m_manager.GetOrInsertType( op.Type ) );
            _Value value = m_basicBlock.LoadToImmediate( address );
            StoreValue( m_results[ 0 ], value );
        }

        private void Translate_StoreIndirectOperator( IR.StoreIndirectOperator op )
        {
            _Value address = m_basicBlock.LoadIndirect( m_arguments[ 0 ], m_manager.GetOrInsertType( op.Type ) );
            StoreValue( address, m_arguments[ 1 ] );
        }

        private void Translate_StoreInstanceFieldOperator( IR.StoreInstanceFieldOperator op )
        {
            _Value address = m_basicBlock.GetFieldAddress( m_arguments[ 0 ], op.Field.Offset );
            StoreValue( address, m_arguments[ 1 ] );
        }

        private void Translate_LoadInstanceFieldOperator( IR.LoadInstanceFieldOperator op )
        {
            _Value address = m_basicBlock.GetFieldAddress( m_arguments[ 0 ], op.Field.Offset );
            _Value value = m_basicBlock.LoadToImmediate( address );
            StoreValue( m_results[ 0 ], value );
        }

        private void Translate_LoadInstanceFieldAddressOperator( IR.LoadInstanceFieldAddressOperator op )
        {
            _Value address = m_basicBlock.GetFieldAddress( m_arguments[ 0 ], op.Field.Offset );
            StoreValue( m_results[ 0 ], address );
        }

        private _Value ArrayAccessByIDX( _Value array, _Value idx )
        {
            array = m_basicBlock.GetFieldAddress( array, ( int )m_wkt.System_Array.Size );
            return m_basicBlock.IndexLLVMArray( array, ConvertValueToALUOperableType( idx ) );
        }

        private void Translate_LoadElementOperator( IR.LoadElementOperator op )
        {
            _Value address = ArrayAccessByIDX( m_arguments[ 0 ], m_arguments[ 1 ] );
            _Value value = m_basicBlock.LoadToImmediate( address );
            StoreValue( m_results[ 0 ], value );
        }

        private void Translate_LoadElementAddressOperator( IR.LoadElementAddressOperator op )
        {
            _Value address = ArrayAccessByIDX( m_arguments[ 0 ], m_arguments[ 1 ] );
            StoreValue( m_results[ 0 ], address );
        }

        private void Translate_StoreElementOperator( IR.StoreElementOperator op )
        {
            _Value address = ArrayAccessByIDX( m_arguments[ 0 ], m_arguments[ 1 ] );
            StoreValue( address, m_arguments[ 2 ] ); 
        }

        private void Translate_UnconditionalControlOperator( IR.UnconditionalControlOperator op )
        {
            m_basicBlock.InsertUnconditionalBranch( GetOrInsertBasicBlock( op.TargetBranch ) );
        }

        private void Translate_BinaryConditionalControlOperator( IR.BinaryConditionalControlOperator op )
        {
            m_basicBlock.InsertConditionalBranch(
                ConvertValueToALUOperableType( m_arguments[ 0 ] ),
                GetOrInsertBasicBlock( op.TargetBranchTaken ),
                GetOrInsertBasicBlock( op.TargetBranchNotTaken ) );
        }

        private void Translate_CompareConditionalControlOperator( IR.CompareConditionalControlOperator op )
        {
            _Value value = DoCmpOp( m_arguments[ 0 ], m_arguments[ 1 ], ( int )op.Condition, op.Signed );
            _BasicBlock taken = GetOrInsertBasicBlock( op.TargetBranchTaken );
            _BasicBlock notTaken = GetOrInsertBasicBlock( op.TargetBranchNotTaken );
            m_basicBlock.InsertConditionalBranch( value, taken, notTaken );
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

            _Value argument = ConvertValueToALUOperableType( m_arguments[ 0 ] );
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
                    m_basicBlock.InsertRet( m_arguments[ 0 ] );
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

        private bool ReplaceMethodCallWithIntrinsic( TS.MethodRepresentation method, List<_Value> convertedArgs )
        {
            TS.WellKnownMethods wkm = m_cfg.TypeSystem.WellKnownMethods;

            // System.Buffer.InternalMemoryCopy(byte*, byte*, int) => llvm.memcpy
            // System.Buffer.InternalBackwardMemoryCopy(byte*, byte*, int) => llvm.memmove
            if( ( method == wkm.System_Buffer_InternalMemoryCopy ) ||
                ( method == wkm.System_Buffer_InternalBackwardMemoryCopy ) )
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
                _Value newVal = m_basicBlock.InsertBinaryOp( (int)IR.BinaryOperator.ALU.ADD, oldVal, val, true );
                StoreValue( m_results[ 0 ], newVal );
                return true;
            }

            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref int, int) => llvm.atomicrmw xchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref float, float) => llvm.atomicrmw xchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref IntPtr, IntPtr) => llvm.atomicrmw xchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalExchange(ref T, T) => llvm.atomicrmw xchg
            // Note: there's no built-in support for 64bit interlocked methods at the moment.
            if(( method == wkm.InterlockedImpl_InternalExchange_int ) ||
                ( method == wkm.InterlockedImpl_InternalExchange_float ) ||
                ( method == wkm.InterlockedImpl_InternalExchange_IntPtr ) ||
                ( method.IsGenericInstantiation && method.GenericTemplate == wkm.InterlockedImpl_InternalExchange_Template ))
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

                _Value oldVal = m_basicBlock.InsertAtomicXchg( ptr, val );

                if(oldVal.Type != type)
                {
                    if(type.IsPointer)
                    {
                        oldVal = m_basicBlock.InsertIntToPointer( oldVal, type );
                    }
                    else
                    {
                        oldVal = m_basicBlock.InsertBitCast( oldVal, type );
                    }
                }

                StoreValue( m_results[ 0 ], oldVal );
                return true;
            }

            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref int, int, int) => llvm.compxchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref float, float, float) => llvm.compxchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref IntPtr, IntPtr, IntPtr) => llvm.compxchg
            // Microsoft.Zelig.Runtime.InterlockedImpl.InternalCompareExchange(ref T, T, T) => llvm.compxchg
            // Note: there's no built-in support for 64bit interlocked methods at the moment.
            if(( method == wkm.InterlockedImpl_InternalCompareExchange_int ) ||
                ( method == wkm.InterlockedImpl_InternalCompareExchange_float ) ||
                ( method == wkm.InterlockedImpl_InternalCompareExchange_IntPtr ) ||
                ( method.IsGenericInstantiation && method.GenericTemplate == wkm.InterlockedImpl_InternalCompareExchange_Template ))
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

                _Value oldVal = m_basicBlock.InsertAtomicCmpXchg( ptr, cmp, val );

                if(oldVal.Type != type)
                {
                    if(type.IsPointer)
                    {
                        oldVal = m_basicBlock.InsertIntToPointer( oldVal, type );
                    }
                    else
                    {
                        oldVal = m_basicBlock.InsertBitCast( oldVal, type );
                    }
                }

                StoreValue( m_results[ 0 ], oldVal );
                return true;
            }

            return false;
        }

        private void BuildMethodCallInstructions( TS.MethodRepresentation method, bool callIndirect )
        {
            List<_Value> args = new List<_Value>( );

            int firstArgument = ( method is TS.StaticMethodRepresentation ) ? 1 : 0;
            int indirectAdjust = callIndirect ? 1 : 0;

            for( int i = firstArgument; i < method.ThisPlusArguments.Length; ++i )
            {
                var opArgTypeRep = method.ThisPlusArguments[ i ];
                var opArgType = m_manager.GetOrInsertType( opArgTypeRep );
                var convertedArg = ConvertValueToStoreToTarget( m_arguments[ i + indirectAdjust ], opArgType );
                args.Add( convertedArg );
            }

            if( ReplaceMethodCallWithIntrinsic( method, args ) )
            {
                return;
            }

            LLVM._Function targetFunc = m_manager.GetOrInsertFunction( method );

            if( method.Flags.HasFlag( TS.MethodRepresentation.Attributes.PinvokeImpl ) )
            {
                targetFunc.SetExternalLinkage( );
            }

            _Value ret;

            if( callIndirect )
            {
                ret = m_basicBlock.InsertIndirectCall( targetFunc, m_arguments[ 0 ], args );
            }
            else
            {
                ret = m_basicBlock.InsertCall( targetFunc, args );
            }

            if( m_results.Count == 1 )
            {
                StoreValue( m_results[ 0 ], ret );
            }
            else if( m_results.Count > 1 )
            {
                throw new System.InvalidOperationException( "More than one return values are not handled." );
            }
        }

        private void Translate_StaticCallOperator( IR.StaticCallOperator op )
        {
            BuildMethodCallInstructions( op.TargetMethod, callIndirect: false );
        }

        private void Translate_InstanceCallOperator( IR.InstanceCallOperator op )
        {
            BuildMethodCallInstructions( op.TargetMethod, callIndirect: false );
        }

        private void Translate_IndirectCallOperator( IR.IndirectCallOperator op )
        {
            BuildMethodCallInstructions( op.TargetMethod, callIndirect: true );
        }
    }
}

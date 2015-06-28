//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    /////////////////////////////////////////////////////////////////////////////////////////////
    //
    // Method analysis:
    //
    // 1) Discover basic blocks (starting points: exception handlers and first bytecode).
    // 2) Operators to zero-out all local variables.
    // 3) Build state of evaluation stack, inferring types of every slot.
    // 4) Convert the evaluation stack to variables.
    // 5) Convert every bytecode to a set of operators (input: set of variables, output: a variable).
    // 6) Optimize.
    // 7) Code-generation.
    //

    public sealed partial class ByteCodeConverter
    {
        //
        // State
        //

        private readonly TypeSystemForIR                     m_typeSystem;
        private readonly ConversionContext                   m_context;
        private readonly MethodRepresentation                m_md;
        private readonly MetaData.Normalized.SignatureType[] m_mdLocals;
        private readonly Debugging.MethodDebugInfo           m_mdDebugInfo;
        private readonly Instruction[]                       m_instructions;
        private readonly EHClause[]                          m_ehTable;
                                                
        private          List< ByteCodeBlock >               m_byteCodeBlocks;
        private          ByteCodeBlock[]                     m_instructionToByteCodeBlock;
        private          object[]                            m_byteCodeArguments;
        private          TypeRepresentation[]                m_localsTypes;

        //
        // Constructor Methods
        //

        public ByteCodeConverter(     TypeSystemForIR                     typeSystem  ,
                                  ref ConversionContext                   context     ,
                                      MethodRepresentation                md          ,
                                      MetaData.Normalized.SignatureType[] mdLocals    ,
                                      Debugging.MethodDebugInfo           mdDebugInfo )
        {
            MetaDataMethodBase metadata = typeSystem.GetAssociatedMetaData( md );

            m_typeSystem   = typeSystem;
            m_context      = context;
            m_md           = md;
            m_mdLocals     = mdLocals;
            m_mdDebugInfo  = mdDebugInfo;
            m_instructions = metadata.Instructions;
            m_ehTable      = metadata.EHTable;
        }

        //--//

        public void PerformDelayedByteCodeAnalysis()
        {
            m_typeSystem.Report( "Converting byte code for {0}...", m_md );

            BuildByteCodeBlocks( AnalyzeControlFlow() );

            SetExceptionHandlingDomains();

            PopulateBasicBlocks();
        }

        //--//

        #region ExpandByteCodeArguments

        public void ExpandByteCodeArguments()
        {
            int numInstructions = m_instructions.Length;

            m_byteCodeArguments = new object[numInstructions];

            for(int i = 0; i < numInstructions; i++)
            {
                object o = m_instructions[i].Argument;

                if(o is MetaDataTypeDefinitionAbstract)
                {
                    o = m_typeSystem.ConvertToIR( (MetaDataTypeDefinitionAbstract)o, m_context );
                }
                else if(o is MetaDataMethodAbstract)
                {
                    o = m_typeSystem.ConvertToIR( (MetaDataMethodAbstract)o, m_context );
                }
                else if(o is MetaDataFieldAbstract)
                {
                    o = m_typeSystem.ConvertToIR( null, (MetaDataFieldAbstract)o, m_context );
                }
                else if(o is MetaDataObject)
                {
                    throw TypeConsistencyErrorException.Create( "Found unexpected metadata object {0} in method {1}", o, m_md );
                }

                m_byteCodeArguments[i] = o;
            }

            if(m_mdLocals != null)
            {
                m_localsTypes = new TypeRepresentation[m_mdLocals.Length];

                //
                // We need to convert the types of the local variables here, even if we don't keep the result around.
                // The ByteCodeConverter will actually pass the values to the FlowGraphState object.
                //
                for(int i = 0; i < m_mdLocals.Length; i++)
                {
                    m_localsTypes[i] = m_typeSystem.ConvertToIR( m_mdLocals[i].Type, m_context );
                }
            }
            else
            {
                m_localsTypes = TypeRepresentation.SharedEmptyArray;
            }
        }

        #endregion ExpandByteCodeArguments

        //--//

        #region AnalyzeControlFlow

        private ByteCodeBlock.Flags[] AnalyzeControlFlow()
        {
            int numInstructions = m_instructions.Length;

            ByteCodeBlock.Flags[] byteCodeBlockKinds = new ByteCodeBlock.Flags[numInstructions];

            try
            {
                //
                // The first instruction is for sure the start of a new basic block.
                //
                byteCodeBlockKinds[0] |= ByteCodeBlock.Flags.NormalEntryPoint;

                for(int i = 0; i < m_ehTable.Length; i++)
                {
                    byteCodeBlockKinds[ m_ehTable[i].HandlerOffset ] |= ByteCodeBlock.Flags.ExceptionHandlerEntryPoint;

                    //
                    // Force a basic block boundary on entry to a try block.
                    //
                    byteCodeBlockKinds[ m_ehTable[i].TryOffset ] |= ByteCodeBlock.Flags.JoinNode;
                }

                for(int outerIndex = 0; outerIndex < numInstructions; outerIndex++)
                {
                    if(byteCodeBlockKinds[outerIndex] != 0 && (byteCodeBlockKinds[outerIndex] & ByteCodeBlock.Flags.Reached) == 0)
                    {
                        for(int innerIndex = outerIndex; (byteCodeBlockKinds[innerIndex] & ByteCodeBlock.Flags.Reached) == 0; innerIndex++)
                        {
                            byteCodeBlockKinds[innerIndex] |= ByteCodeBlock.Flags.Reached;

                            Instruction                   instr = m_instructions[innerIndex];
                            Instruction.OpcodeInfo        oi    = instr.Operator;
                            Instruction.OpcodeFlowControl ctrl  = oi.Control;

                            if(ctrl != Instruction.OpcodeFlowControl.None)
                            {
                                switch(oi.OperandFormat)
                                {
                                    case Instruction.OpcodeOperand.Branch:
                                        {
                                            int target = (int)instr.Argument;

                                            byteCodeBlockKinds[innerIndex] |= ByteCodeBlock.Flags.BranchNode;

                                            outerIndex = AnalyzeControlFlow_ProcessBranch( byteCodeBlockKinds, target, innerIndex, outerIndex );
                                        }
                                        break;

                                    case Instruction.OpcodeOperand.Switch:
                                        {
                                            byteCodeBlockKinds[innerIndex] |= ByteCodeBlock.Flags.BranchNode;

                                            foreach(int target in (int[])instr.Argument)
                                            {
                                                outerIndex = AnalyzeControlFlow_ProcessBranch( byteCodeBlockKinds, target, innerIndex, outerIndex );
                                            }
                                        }
                                        break;
                                }

                                if(ctrl != Instruction.OpcodeFlowControl.ConditionalControl)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                for(int outerIndex = 0; outerIndex < numInstructions; outerIndex++)
                {
                    if(byteCodeBlockKinds[outerIndex] == 0)
                    {
                        m_typeSystem.Report( "Unreachable bytecode {0} for {1}", m_instructions[outerIndex], m_md );
                    }
                }
            }
            catch
            {
                throw TypeConsistencyErrorException.Create( "Invalid byte code for '{0}'", m_md );
            }

            return byteCodeBlockKinds;
        }

        private static int AnalyzeControlFlow_ProcessBranch( ByteCodeBlock.Flags[] byteCodeBlockKinds ,
                                                             int                   target             ,
                                                             int                   innerIndex         ,
                                                             int                   outerIndex         )
        {
            byteCodeBlockKinds[target] |= ByteCodeBlock.Flags.JoinNode;

            if(target < innerIndex)
            {
                byteCodeBlockKinds[target] |= ByteCodeBlock.Flags.BackEdge;
            }

            //
            // Backtrack external loop.
            //
            if((byteCodeBlockKinds[target] & ByteCodeBlock.Flags.Reached) == 0 && outerIndex > target)
            {
                outerIndex = target - 1;
            }

            return outerIndex;
        }

        #endregion AnalyzeControlFlow

        //--//

        #region BuildByteCodeBlocks

        private void BuildByteCodeBlocks( ByteCodeBlock.Flags[] byteCodeBlockKinds )
        {
            int           numInstructions = m_instructions.Length;
            ByteCodeBlock currentBCB      = null;

            m_byteCodeBlocks             = new List<ByteCodeBlock>();
            m_instructionToByteCodeBlock = new ByteCodeBlock[numInstructions];

            for(int i = 0; i < numInstructions; i++)
            {
                if((byteCodeBlockKinds[i] & ByteCodeBlock.Flags.JoinNode) != 0)
                {
                    currentBCB = null;
                }

                if(currentBCB == null)
                {
                    currentBCB = BuildByteCodeBlocks_Fetch( i, byteCodeBlockKinds );
                }
                else
                {
                    m_instructionToByteCodeBlock[i] = currentBCB;
                }

                Instruction instr         = m_instructions[i];
                bool        fJoinWithNext = false;

                currentBCB.IncrementInstructionCount();

                switch(instr.Operator.OperandFormat)
                {
                    case Instruction.OpcodeOperand.Branch:
                        {
                            int target = (int)instr.Argument;

                            BuildByteCodeBlocks_Link( currentBCB, target, byteCodeBlockKinds );

                            fJoinWithNext = (instr.Operator.Control == Instruction.OpcodeFlowControl.ConditionalControl);
                        }
                        break;

                    case Instruction.OpcodeOperand.Switch:
                        {
                            foreach(int target in (int[])instr.Argument)
                            {
                                BuildByteCodeBlocks_Link( currentBCB, target, byteCodeBlockKinds );
                            }

                            fJoinWithNext = (instr.Operator.Control == Instruction.OpcodeFlowControl.ConditionalControl);
                        }
                        break;

                    default:
                        {
                            if(instr.Operator.Control == Instruction.OpcodeFlowControl.None)
                            {
                                int target = i + 1;

                                if(target < numInstructions && (byteCodeBlockKinds[target] & ByteCodeBlock.Flags.JoinNode) != 0)
                                {
                                    fJoinWithNext = true;
                                }
                            }
                        }
                        break;
                }

                if(fJoinWithNext)
                {
                    BuildByteCodeBlocks_Link( currentBCB, i+1, byteCodeBlockKinds );
                }

                if((byteCodeBlockKinds[i] & ByteCodeBlock.Flags.BranchNode) != 0)
                {
                    currentBCB = null;
                }
            }
        }

        private ByteCodeBlock BuildByteCodeBlocks_Fetch( int                   i                  ,
                                                         ByteCodeBlock.Flags[] byteCodeBlockKinds )
        {
            ByteCodeBlock current = m_instructionToByteCodeBlock[i];
            if(current == null)
            {
                current = new ByteCodeBlock( byteCodeBlockKinds[i], i );

                m_byteCodeBlocks.Add( current );

                m_instructionToByteCodeBlock[i] = current;
            }

            return current;
        }

        private void BuildByteCodeBlocks_Link( ByteCodeBlock         currentBCB         ,
                                               int                   target             ,
                                               ByteCodeBlock.Flags[] byteCodeBlockKinds )
        {
            ByteCodeBlock targetBCB = BuildByteCodeBlocks_Fetch( target, byteCodeBlockKinds );

            targetBCB .SetPredecessor( currentBCB );
            currentBCB.SetSuccessor  ( targetBCB  );
        }

        #endregion BuildByteCodeBlocks

        //--//

        #region SetExceptionHandlingDomains

        private void SetExceptionHandlingDomains()
        {
            for(int i = 0; i < m_ehTable.Length; i++)
            {
                EHClause      eh         = m_ehTable[i];
                ByteCodeBlock bcbLast    = null;
                ByteCodeBlock bcbHandler = m_instructionToByteCodeBlock[eh.HandlerOffset];

                bcbHandler.SetAsHandlerFor( eh );

                for(int j = eh.TryOffset; j < eh.TryEnd; j++)
                {
                    ByteCodeBlock bcb = m_instructionToByteCodeBlock[j];

                    if(bcb != bcbLast)
                    {
                        bcb.SetProtectedBy( bcbHandler );

                        bcbLast = bcb;
                    }
                }
            }
        }

        #endregion SetExceptionHandlingDomains

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

    }
}

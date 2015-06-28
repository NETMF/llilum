//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public partial class MetaDataMethod : MetaDataObject
    {
        // Internal Types
        private enum MethodFormats : byte
        {
            Tiny  = 2,
            Fat   = 3,
            Tiny1 = 6
        }

        private enum SectionKind : byte
        {
            Reserved   = 0,
            EHTable    = 1,
            OptILTable = 2
        }

        private static readonly EHClause[] s_sharedEmptyTable = new EHClause[0];

        //
        // Constructor Methods
        //

        private void getInstructions( Parser              mdParser    ,
                                      PdbInfo.PdbFunction pdbFunction )
        {
            ArrayReader reader = mdParser.ImageReaderAtVirtualAddress( m_rva );

            byte headerByte = reader.ReadUInt8();
            byte formatMask = (byte)(headerByte & 0x7);

            int         codeSize;
            ArrayReader codeReader;

            m_ehTable = s_sharedEmptyTable;

            switch((MethodFormats)formatMask)
            {
                case MethodFormats.Tiny:
                case MethodFormats.Tiny1:
                    {
                        m_initLocals = false;
                        m_maxStack   = 8;
                        m_locals     = null;
                        codeSize     = headerByte >> 2;
                        codeReader   = reader.CreateSubsetAndAdvance( codeSize );
                        break;
                    }

                case MethodFormats.Fat:
                    {
                        //
                        // for Fat format, check for CorILMethod_InitLocals
                        // Section 25.4.3 of ECMA spec, Partition II
                        //
                        m_initLocals = ((headerByte & 0x10) != 0);

                        byte headerByte2 = reader.ReadUInt8();

                        int size = headerByte2 >> 4;
                        if(size != 3)
                        {
                            throw new IllegalInstructionStreamException( "Unexpected FAT size: " + size );
                        }

                        m_maxStack = reader.ReadUInt16();
                        codeSize   = reader.ReadInt32();

                        int localVarSignatureToken = reader.ReadInt32();
                        if(localVarSignatureToken == 0)
                        {
                            m_locals = null;
                        }
                        else
                        {
                            MetaDataStandAloneSig standAloneSig = (MetaDataStandAloneSig)mdParser.getObjectFromToken( localVarSignatureToken );

                            SignatureLocalVar localVarSignature = (SignatureLocalVar)standAloneSig.Signature;

                            m_locals = localVarSignature.Locals;
                        }

                        codeReader = reader.CreateSubsetAndAdvance( codeSize );
                        break;
                    }

                default:
                    {
                        throw new IllegalInstructionStreamException( "Unknown format: " + formatMask.ToString( "x" ) );
                    }
            }

            int[] byteToInstrMapping = new int[codeSize + 1];
            int   instructionCount   = 0;

            while(true)
            {
                byteToInstrMapping[codeReader.Position] = instructionCount;

                if(codeReader.Position >= codeSize) break;

                instructionCount++;

                Normalized.Instruction.Opcode opcode = (Normalized.Instruction.Opcode)codeReader.ReadUInt8();
                switch(opcode)
                {
                    case Normalized.Instruction.Opcode.PREFIX1:
                        opcode = (Normalized.Instruction.Opcode)((int)codeReader.ReadUInt8() + 256);
                        if(opcode < 0 || (Normalized.Instruction.Opcode)opcode >= Normalized.Instruction.Opcode.COUNT)
                        {
                            throw new IllegalInstructionStreamException( "Saw prefixed opcode of " + opcode );
                        }
                        break;

                    case Normalized.Instruction.Opcode.PREFIXREF:
                    case Normalized.Instruction.Opcode.PREFIX2:
                    case Normalized.Instruction.Opcode.PREFIX3:
                    case Normalized.Instruction.Opcode.PREFIX4:
                    case Normalized.Instruction.Opcode.PREFIX5:
                    case Normalized.Instruction.Opcode.PREFIX6:
                    case Normalized.Instruction.Opcode.PREFIX7:
                        throw new IllegalInstructionStreamException( "Saw unexpected prefix opcode " + opcode );
                }

                Normalized.Instruction.OpcodeInfo opcodeInfo  = Normalized.Instruction.OpcodeInfoTable[(int)opcode];
                int                               operandSize = opcodeInfo.OperandSize;

                if(operandSize == -1)
                {
                    switch(opcodeInfo.OperandFormat)
                    {
                        case Normalized.Instruction.OpcodeOperand.Switch:
                            {
                                int caseCount = codeReader.ReadInt32();

                                codeReader.Seek( 4 * caseCount );
                                break;
                            }
                    }
                }
                else
                {
                    codeReader.Seek( operandSize );
                }
            }

            //--//

            // Check whether or not there is an EH section
            if(formatMask == (byte)MethodFormats.Fat && (headerByte & 0x08) != 0)
            {
                int sectionPadding = (m_rva + codeSize) % 4;

                if(sectionPadding != 0)
                {
                    while(sectionPadding < 4)
                    {
                        sectionPadding++;
                        byte padding = reader.ReadUInt8();
                    }
                }

                byte sectionKind = reader.ReadUInt8();
                if((sectionKind & 0x80) != 0)
                {
                    throw new IllegalInstructionStreamException( "More than one section after the code" );
                }
                if((sectionKind & 0x3F) != (int)SectionKind.EHTable)
                {
                    throw new IllegalInstructionStreamException( "Expected EH table section, got " + sectionKind.ToString( "x" ) );
                }

                //--//

                bool smallSection = ((sectionKind & 0x40) == 0);
                int  dataSize;

                if(smallSection)
                {
                    // Small section
                    int ehByteCount = reader.ReadUInt8();
                    switch((ehByteCount % 12))
                    {
                        case 0: // Some compilers generate the wrong value here, they forget to add the size of the header.
                        case 4:
                            break;

                        default:
                            throw new IllegalInstructionStreamException( "Unexpected byte count for small EH table: " + ehByteCount );
                    }

                    int sectSmallReserved = reader.ReadInt16();

                    dataSize = (ehByteCount) / 12;
                }
                else
                {
                    // Fat section
                    int ehByteCount;

                    ehByteCount  = reader.ReadUInt8();
                    ehByteCount |= reader.ReadUInt8() << 8;
                    ehByteCount |= reader.ReadUInt8() << 16;

                    switch((ehByteCount % 24))
                    {
                        case 0: // Some compilers generate the wrong value here, they forget to add the size of the header.
                        case 4:
                            break;

                        default:
                            throw new IllegalInstructionStreamException( "Unexpected byte count for fat EH table: " + ehByteCount );
                    }

                    dataSize = ehByteCount / 24;
                }

                m_ehTable = new EHClause[dataSize];
                for(int i = 0; i < dataSize; i++)
                {
                    int flags;
                    int tryOffset;
                    int tryLength;
                    int handlerOffset;
                    int handlerLength;
                    int tokenOrOffset;

                    if(smallSection)
                    {
                        flags         = reader.ReadUInt16();
                        tryOffset     = reader.ReadUInt16();
                        tryLength     = reader.ReadUInt8();
                        handlerOffset = reader.ReadUInt16();
                        handlerLength = reader.ReadUInt8();
                        tokenOrOffset = reader.ReadInt32();
                    }
                    else
                    {
                        flags         = reader.ReadInt32();
                        tryOffset     = reader.ReadInt32();
                        tryLength     = reader.ReadInt32();
                        handlerOffset = reader.ReadInt32();
                        handlerLength = reader.ReadInt32();
                        tokenOrOffset = reader.ReadInt32();
                    }

                    int tryInstrOffset     = byteToInstrMapping[tryOffset                    ];
                    int tryInstrEnd        = byteToInstrMapping[tryOffset     + tryLength    ];
                    int handlerInstrOffset = byteToInstrMapping[handlerOffset                ];
                    int handlerInstrEnd    = byteToInstrMapping[handlerOffset + handlerLength];

                    IMetaDataTypeDefOrRef classObject = null;
                    if((flags & (int)EHClause.ExceptionFlag.Filter) != 0)
                    {
                        tokenOrOffset = byteToInstrMapping[tokenOrOffset];
                    }
                    else if(flags == (int)EHClause.ExceptionFlag.None)
                    {
                        classObject   = (IMetaDataTypeDefOrRef)mdParser.getObjectFromToken( tokenOrOffset );
                        tokenOrOffset = 0;
                    }
                    else
                    {
                        //
                        // Managed C++ emits finally blocks with a token. So we cannot enforce this check.
                        //
                        //if(MetaData.UnpackTokenAsIndex( tokenOrOffset ) != 0)
                        //{
                        //    throw new IllegalInstructionStreamException( "Unknown token or offset " + tokenOrOffset.ToString( "x8" ) );
                        //}
                    }

                    m_ehTable[i] = new EHClause( flags                                ,
                                                 tryInstrOffset                       ,
                                                 tryInstrEnd     - tryInstrOffset     ,
                                                 handlerInstrOffset                   ,
                                                 handlerInstrEnd - handlerInstrOffset ,
                                                 tokenOrOffset                        ,
                                                 classObject                          );
                }
            }

            m_instructions = new Instruction[instructionCount];

            int                 instructionCounter = 0;
            Debugging.DebugInfo debugInfo          = null;

            codeReader.Rewind();

            while(codeReader.Position < codeSize)
            {
                int pc = codeReader.Position;

                if(pc > 0 && byteToInstrMapping[pc] == 0)
                {
                    throw new IllegalInstructionStreamException( "Out of sync at " + pc.ToString( "x" ) );
                }

                Normalized.Instruction.Opcode opcode = (Normalized.Instruction.Opcode)codeReader.ReadUInt8();
                if(opcode == Normalized.Instruction.Opcode.PREFIX1)
                {
                    opcode = (Normalized.Instruction.Opcode)((int)codeReader.ReadUInt8() + 256);
                }

                Normalized.Instruction.OpcodeInfo opcodeInfo  = Normalized.Instruction.OpcodeInfoTable[(int)opcode];
                int                               operandSize = opcodeInfo.OperandSize;
                Instruction.Operand               operand;

                switch(opcodeInfo.OperandFormat)
                {
                    case Normalized.Instruction.OpcodeOperand.None:
                        operand = null;
                        break;

                    case Normalized.Instruction.OpcodeOperand.Var:
                        operand = new Instruction.OperandInt( (int)ReadOperandUInt( opcodeInfo, codeReader ) );
                        break;

                    case Normalized.Instruction.OpcodeOperand.Int:
                        if(operandSize == 8)
                        {
                            operand = new Instruction.OperandLong( codeReader.ReadInt64() );
                        }
                        else
                        {
                            operand = new Instruction.OperandInt( ReadOperandInt( opcodeInfo, codeReader ) );
                        }
                        break;

                    case Normalized.Instruction.OpcodeOperand.Float:
                        if(operandSize == 8)
                        {
                            operand = new Instruction.OperandDouble( codeReader.ReadDouble() );
                        }
                        else
                        {
                            operand = new Instruction.OperandSingle( codeReader.ReadSingle() );
                        }
                        break;

                    case Normalized.Instruction.OpcodeOperand.Branch:
                        {
                            int target = ReadOperandInt( opcodeInfo, codeReader );

                            int instrTarget = byteToInstrMapping[codeReader.Position + target];

                            operand = new Instruction.OperandTarget( instrTarget );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.Method:
                        {
                            int token = codeReader.ReadInt32();

                            MetaDataObject mdMethod = mdParser.getObjectFromToken( token );
                            operand = new Instruction.OperandObject( mdMethod );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.Field:
                        {
                            int token = codeReader.ReadInt32();

                            MetaDataObject mdField = mdParser.getObjectFromToken( token );
                            operand = new Instruction.OperandObject( mdField );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.Type:
                        {
                            int token = codeReader.ReadInt32();

                            MetaDataObject type = mdParser.getObjectFromToken( token );
                            operand = new Instruction.OperandObject( type );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.Token:
                        {
                            int token = codeReader.ReadInt32();

                            MetaDataObject mdToken = mdParser.getObjectFromToken( token );
                            operand = new Instruction.OperandObject( mdToken );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.String:
                        {
                            int token = codeReader.ReadInt32();

                            if(MetaData.UnpackTokenAsType( token ) != TokenType.String)
                            {
                                throw new IllegalInstructionStreamException( "Unexpected string token " + token.ToString( "x" ) );
                            }

                            int index = MetaData.UnpackTokenAsIndex( token );

                            operand = new Instruction.OperandString( mdParser.getUserString( index ) );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.Sig:
                        {
                            int token = codeReader.ReadInt32();

                            MetaDataObject calleeDescr = mdParser.getObjectFromToken( token );

                            operand = new Instruction.OperandObject( calleeDescr );
                            break;
                        }

                    case Normalized.Instruction.OpcodeOperand.Switch:
                        {
                            int caseCount = codeReader.ReadInt32();

                            int[] branchArray = new int[caseCount];
                            int   nextPC      = codeReader.Position + 4 * caseCount;

                            for(int j = 0; j < caseCount; j++)
                            {
                                int target = codeReader.ReadInt32();

                                int instrTarget = byteToInstrMapping[nextPC + target];

                                branchArray[j] = instrTarget;
                            }

                            operand = new Instruction.OperandTargetArray( branchArray );
                            break;
                        }

                    default:
                        throw new IllegalInstructionStreamException( "Invalid opcode " + opcodeInfo );
                }

                // read in the line number information.
                PdbInfo.PdbSource file;
                PdbInfo.PdbLine   line;

                if(pdbFunction != null && pdbFunction.FindOffset( (uint)pc, out file, out line ))
                {
                    if(line.LineBegin != 0xFEEFEE)
                    {
                        debugInfo = new Debugging.DebugInfo( file.Name, (int)line.LineBegin, (int)line.ColumnBegin, (int)line.LineEnd, (int)line.ColumnEnd );
                    }
                    else
                    {
                        // No debugging info.
                    }
                }

                m_instructions[instructionCounter] = new Instruction( opcodeInfo, operand, debugInfo );
                instructionCounter++;
            }
        }

        private static int ReadOperandInt( Normalized.Instruction.OpcodeInfo opcodeInfo, ArrayReader codeReader )
        {
            switch(opcodeInfo.OperandSize)
            {
                case 0: return opcodeInfo.ImplicitOperandValue;
                case 1: return codeReader.ReadInt8();
                case 2: return codeReader.ReadInt16();
                case 4: return codeReader.ReadInt32();
            }

            throw new IllegalInstructionStreamException( "Unknown operand size " + opcodeInfo.OperandSize );
        }

        private static uint ReadOperandUInt( Normalized.Instruction.OpcodeInfo opcodeInfo, ArrayReader codeReader )
        {
            switch(opcodeInfo.OperandSize)
            {
                case 0: return (uint)opcodeInfo.ImplicitOperandValue;
                case 1: return codeReader.ReadUInt8 ();
                case 2: return codeReader.ReadUInt16();
                case 4: return codeReader.ReadUInt32();
            }

            throw new IllegalInstructionStreamException( "Unknown operand size " + opcodeInfo.OperandSize );
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Internal classes

        public class IllegalInstructionStreamException : Exception
        {
            //
            // Constructor Methods
            //

            internal IllegalInstructionStreamException( String message ) : base( message )
            {
            }
        }
    }
}

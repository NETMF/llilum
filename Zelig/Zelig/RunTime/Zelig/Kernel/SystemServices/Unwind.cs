//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

// Note: This implementation was written to match the Itanium ABI. For further details, please
// reference the Itanium ABI (http://mentorembedded.github.io/cxx-abi/abi-eh.html) and LLVM's
// libc++abi (http://libcxxabi.llvm.org/).

#define ARM_EABI

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Microsoft.Zelig.Runtime.TypeSystem;

    public static class Unwind
    {
        /// <summary>
        /// Status flags describing the unwind phase and options. Should match _Unwind_Action in ABI.
        /// </summary>
        [Flags]
        public enum UnwindActions
        {
            SearchPhase = 0x01, // Mutually exclusive with CleanupPhase
            CleanupPhase = 0x02, // Mutually exclusive with SearchPhase
            HandlerFrame = 0x04,
            ForceUnwind = 0x08,
            EndOfStack = 0x16,
        }

        /// <summary>
        /// Result of any given unwind operation; values should match _Unwind_Reason_Code.
        /// </summary>
        public enum UnwindReasonCode
        {
            NoReason = 0,
            ForeignExceptionCaught,
            Phase2Error,
            Phase1Error,
            NormalStop,
            EndOfStack,
            HandlerFound,
            InstallContext,
            ContinueUnwind,
            Failure,
        }

        /// <summary>
        /// DWARF encoding types for variable-length data.
        /// </summary>
        [Flags]
        private enum DwarfEncoding : byte
        {
            // Encoding types:
            Pointer             = 0x00,
            Uleb128             = 0x01,
            Udata2              = 0x02,
            Udata4              = 0x03,
            Udata8              = 0x04,
            Sleb128             = 0x09,
            Sdata2              = 0x0a,
            Sdata4              = 0x0b,
            Sdata8              = 0x0c,
            TypeMask            = 0x0f,

            // Encoding modifiers:
            Absolute            = 0x00,
            PcRelative          = 0x10,
            TextRelative        = 0x20,
            DataRelative        = 0x30,
            FunctionRelative    = 0x40,
            Aligned             = 0x50,
            ModifierMask        = 0x70,

            // Special values:
            Indirect            = 0x80,
            Omit                = 0xff,
        }

        internal const ulong ExceptionClass = 0x000023435446534d; // "MSFTC#\0\0"

        [ExportedMethod]
        static public unsafe UnwindReasonCode LLOS_Unwind_Personality(
            UnwindActions actions,
            UInt64 exceptionClass,
            UIntPtr exceptionObject,
            UIntPtr context)
        {
            // TODO: Should we execute cleanup pads?
            if (exceptionClass != ExceptionClass)
            {
                // We have been notified of a foreign exception being thrown, and we therefore need to
                // execute cleanup landing pads.
                return UnwindReasonCode.ContinueUnwind;
            }

            byte* lsda = (byte*)LLOS_Unwind_GetLanguageSpecificData(context);
            if (lsda == null)
            {
                return UnwindReasonCode.ContinueUnwind;
            }

            // Get the current instruction pointer and offset it before next instruction in the current
            // frame which threw the exception.
            ulong pc = (ulong)LLOS_Unwind_GetIP(context) - 1;

            // Get beginning current frame's code (as defined by the emitted dwarf code)
            ulong funcStart = (ulong)LLOS_Unwind_GetRegionStart(context);
            ulong pcOffset = pc - funcStart;

            // Get the landing pad's base address; defaults to the start of the function.
            DwarfEncoding landingPadBaseEncoding = (DwarfEncoding)(*lsda);
            ++lsda;

            if (landingPadBaseEncoding != DwarfEncoding.Omit)
            {
                funcStart = ReadEncodedPointer(ref lsda, landingPadBaseEncoding);
            }

            DwarfEncoding typeEncoding = (DwarfEncoding)(*lsda);
            ++lsda;

            // Get the type info list; this is an array of pointers to type info, in our case VTable*.
            // It points to the end of the table and expects a one-based index.
            UIntPtr classInfo = UIntPtr.Zero;
            if (typeEncoding != DwarfEncoding.Omit)
            {
                ulong classInfoOffset = ReadULEB128(ref lsda);
                classInfo = (UIntPtr)(lsda + classInfoOffset);
            }

            DwarfEncoding callSiteEncoding = (DwarfEncoding)(*lsda);
            ++lsda;

            uint callSiteTableLength = (uint)ReadULEB128(ref lsda);
            byte* callSiteTableStart = lsda;
            byte* callSiteTableEnd = callSiteTableStart + callSiteTableLength;
            byte* actionTableStart = callSiteTableEnd;
            byte* callSitePtr = callSiteTableStart;

            UIntPtr landingPad = UIntPtr.Zero;
            ulong actionEntry = 0;

            // Walk the call sites to find which region the PC falls in.
            while (callSitePtr < callSiteTableEnd)
            {
                // These values are offsets from the function start.
                ulong start = ReadEncodedPointer(ref callSitePtr, callSiteEncoding);
                ulong length = ReadEncodedPointer(ref callSitePtr, callSiteEncoding);
                ulong pad = ReadEncodedPointer(ref callSitePtr, callSiteEncoding);

                // One-based currentByte offset into the action table.
                actionEntry = ReadULEB128(ref callSitePtr);

                if ((start <= pcOffset) && (pcOffset < (start + length)))
                {
                    // Landing pad may be zero to indicate this region has no handlers.
                    landingPad = (UIntPtr)pad;
                    break;
                }
            }

            if (landingPad == UIntPtr.Zero)
            {
                // No landing pad for this frame.
                return UnwindReasonCode.ContinueUnwind;
            }

            landingPad = AddressMath.Increment(landingPad, (uint)funcStart);

            // Action entry of zero means this is a cleanup pad.
            if (actionEntry == 0)
            {
                if (((actions & UnwindActions.CleanupPhase) != 0) &&
                    ((actions & UnwindActions.HandlerFrame) == 0))
                {
                    return UnwindReasonCode.HandlerFound;
                }

                return UnwindReasonCode.ContinueUnwind;
            }

            object thrownException = LLOS_GetExceptionObject(exceptionObject);
            byte* action = actionTableStart + actionEntry - 1;

            for (int i = 0; true; ++i)
            {
                ulong typeIndex = ReadSLEB128(ref action);
                if (typeIndex > 0)
                {
                    // This is a catch clause. Get the associated vtable and see if it matches the thrown exception.
                    bool foundMatch = false;

                    VTable entryVTable = GetEntryVTable(typeIndex, typeEncoding, classInfo);
                    if (entryVTable == null)
                    {
                        // Null clause means we should match anything.
                        foundMatch = true;
                    }
                    else if (TypeSystemManager.CastToTypeNoThrow(thrownException, entryVTable) != null)
                    {
                        // Thrown exception is a subclass of the clause's vtable.
                        foundMatch = true;
                    }

                    if (foundMatch)
                    {
                        if ((actions & UnwindActions.SearchPhase) != 0)
                        {
                            return UnwindReasonCode.HandlerFound;
                        }

                        if ((actions & UnwindActions.HandlerFrame) != 0)
                        {
                            LLOS_Unwind_SetRegisters(context, landingPad, exceptionObject, (UIntPtr)(i + 1));
                            return UnwindReasonCode.InstallContext;
                        }

                        // If this isn't a search or a handler phase, then it must be a force unwind.
                        if ((actions & UnwindActions.ForceUnwind) == 0)
                        {
                            LLOS_Terminate();
                        }
                    }
                }
                else if (typeIndex == 0)
                {
                    // This is a cleanup pad. If this is the cleanup phase, handle it. We intentionally
                    // pass an invalid (zero) selector so the landing pad doesn't execute any catch handlers.
                    if (((actions & UnwindActions.CleanupPhase) != 0) &&
                        ((actions & UnwindActions.HandlerFrame) == 0))
                    {
                        LLOS_Unwind_SetRegisters(context, landingPad, exceptionObject, UIntPtr.Zero);
                        return UnwindReasonCode.InstallContext;
                    }
                }
                else
                {
                    // This is a filter clause; ignore it.
                }

                // Move to the next handler. If there isn't one, we didn't find an appropriate clause.
                byte* tempAction = action;
                ulong actionOffset = ReadSLEB128(ref tempAction);
                if (actionOffset == 0)
                {
                    return UnwindReasonCode.ContinueUnwind;
                }

                action += actionOffset;
            }
        }

        private static unsafe VTable GetEntryVTable(
            ulong typeIndex,
            DwarfEncoding typeEncoding,
            UIntPtr classInfo)
        {
            UIntPtr typePointer = AddressMath.Decrement(classInfo, (uint)typeIndex * GetEncodingSize(typeEncoding));

#if ARM_EABI
            UIntPtr offset = *(UIntPtr*)typePointer;
            if (offset == UIntPtr.Zero)
            {
                return null;
            }

            UIntPtr vtablePointer = AddressMath.Increment(typePointer, offset.ToUInt32());
#else // ARM_EABI
            byte* tempTypePointer = (byte*)typePointer.ToPointer();
            UIntPtr vtablePointer = (UIntPtr)ReadEncodedPointer(ref tempTypePointer, typeEncoding);
#endif // ARM_EABI

            // Note: We need to adjust the VTable pointer past the object header due to the LLVM bug cited
            // in Translate_LandingPadOperator. When this issue is resolved we can remove the adjustment.
            return (VTable)(object)ObjectHeader.CastAsObjectHeader(vtablePointer).Pack();
        }

        // Decode an unsigned leb128 value and advance the data pointer.
        // See (7.6) Variable Length Data in: http://dwarfstd.org/doc/DWARF4.pdf
        private static unsafe ulong ReadULEB128(ref byte* data)
        {
            ulong result = 0;
            int shift = 0;
            byte currentByte;

            do
            {
                currentByte = *(data++);
                result |= ((ulong)(currentByte & 0x7f)) << shift;
                shift += 7;
            } while ((currentByte & 0x80) != 0);

            return result;
        }

        // Decode a signed leb128 value and advance the data pointer.
        // See (7.6) Variable Length Data in: http://dwarfstd.org/doc/DWARF4.pdf
        private static unsafe ulong ReadSLEB128(ref byte* data)
        {
            ulong result = 0;
            int shift = 0;
            byte currentByte;

            do
            {
                currentByte = *(data++);
                result |= ((ulong)(currentByte & 0x7f)) << shift;
                shift += 7;
            } while ((currentByte & 0x80) != 0);

            // If the high bit is set on the last (highest order) byte, sign-extend the entire value.
            if (((currentByte & 0x40) != 0) && (shift < (sizeof(ulong) * 8)))
            {
                result |= (ulong.MaxValue << shift);
            }

            return result;
        }

        // Decode a pointer value and advance the data pointer.
        private static unsafe ulong ReadEncodedPointer(ref byte* data, DwarfEncoding encoding)
        {
            if (encoding == DwarfEncoding.Omit)
            {
                return 0;
            }

            ulong result = 0;

            switch (encoding & DwarfEncoding.TypeMask)
            {
            case DwarfEncoding.Pointer:
                result = (*(UIntPtr*)data).ToUInt64();
                data += sizeof(UIntPtr);
                break;

            case DwarfEncoding.Udata2:
            case DwarfEncoding.Sdata2:
                result = *(UInt16*)data;
                data += sizeof(UInt16);
                break;

            case DwarfEncoding.Udata4:
            case DwarfEncoding.Sdata4:
                result = *(UInt32*)data;
                data += sizeof(UInt32);
                break;

            case DwarfEncoding.Udata8:
            case DwarfEncoding.Sdata8:
                result = *(UInt64*)data;
                data += sizeof(UInt64);
                break;

            case DwarfEncoding.Uleb128:
                result = ReadULEB128(ref data);
                break;

            case DwarfEncoding.Sleb128:
                result = ReadSLEB128(ref data);
                break;

            default:
                LLOS_Terminate();
                break;
            }

            // Adjust the value.
            switch (encoding & DwarfEncoding.ModifierMask)
            {
            case DwarfEncoding.Absolute:
                break;

            case DwarfEncoding.PcRelative:
                if (result != 0)
                {
                    result += (ulong)data;
                }
                break;

            default:
                LLOS_Terminate();
                break;
            }

            // Indirect the value if necessary.
            if ((encoding & DwarfEncoding.Indirect) != 0)
            {
                result = *(ulong*)result;
            }

            return result;
        }

        private static uint GetEncodingSize(DwarfEncoding encoding)
        {
            if (encoding == DwarfEncoding.Omit)
            {
                return 0;
            }

            switch (encoding & DwarfEncoding.TypeMask)
            {
            case DwarfEncoding.Pointer:
                return (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(UIntPtr));

            case DwarfEncoding.Udata2:
            case DwarfEncoding.Sdata2:
                return 2;

            case DwarfEncoding.Udata4:
            case DwarfEncoding.Sdata4:
                return 4;

            case DwarfEncoding.Udata8:
            case DwarfEncoding.Sdata8:
                return 8;

            default:
                LLOS_Terminate();
                return 0;
            }
        }

        [DllImport("C")]
        internal static extern UIntPtr LLOS_AllocateException(object exception, UInt64 exceptionClass);

        [DllImport("C")]
        internal static extern object LLOS_GetExceptionObject(UIntPtr exception);

        [DllImport("C")]
        internal static extern UIntPtr LLOS_Unwind_GetIP(UIntPtr context);

        [DllImport("C")]
        internal static extern UIntPtr LLOS_Unwind_GetLanguageSpecificData(UIntPtr context);

        [DllImport("C")]
        internal static extern UIntPtr LLOS_Unwind_GetRegionStart(UIntPtr context);

        [DllImport("C")]
        internal static extern void LLOS_Unwind_SetRegisters(
            UIntPtr context,
            UIntPtr landingPad,
            UIntPtr exceptionObject,
            UIntPtr selector);

        [NoReturn]
        [DllImport("C")]
        internal static extern void LLOS_Unwind_RaiseException(UIntPtr exceptionObject);

        [NoReturn]
        [DllImport("C")]
        internal static extern void LLOS_Terminate();
    }
}

using System;

namespace Microsoft.binutils.elflib
{
    //
    // Enum Definitions
    //

    public enum ElfType
    {
        Linking   = e_type.ET_REL,
        Execution = e_type.ET_EXEC,
    }

    public enum ElfElementStatus
    {
        Added       ,
        Removed     ,
        IndexChanged,
        AddressChanged,
    }

    //
    // Interface Definitions
    //

    internal interface IElfElement
    {
        void BuildReferences();
    }

    internal interface IElfElementStatusPublisher<T>
    {
        event Action<T, ElfElementStatus> ElementStatusChangedEvent;
    }

    //
    // Exception Definitions
    //

    public class ElfConsistencyException : Exception
    {
        public ElfConsistencyException( string message ) : base( message ) { }
    }
}

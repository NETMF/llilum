using System.Collections.Generic;
using System.Text;

namespace Llvm.NET
{
    /// <summary>Name mangling mode for target output</summary>
    public enum NameMangling
    {
        Elf,
        Mips,
        MachO,
        PeCoff
    }

    /// <summary>Interface for components describing a particular target's data layout rules</summary>
    public interface ITargetDataLayout
    {
        /// <summary>List of natively supported integer bit widths</summary>
        IReadOnlyList<int> NativeIntWidths { get; }
        
        /// <summary>Name mangling to use when generating symbol names for the linker</summary>
        NameMangling NameMangling { get; }

        /// <summary>Default Alignment info for aggregate data types</summary>
        AlignmentInfo AggregateAlignmentInfo { get; }

        /// <summary>Default alignment information for floating point types</summary>
        IReadOnlyList<SizeAndAlignment> FloatAlignmentInfo { get; }

        /// <summary>Default alignment information for vector types</summary>
        IReadOnlyList<SizeAndAlignment> VectorAlignmentInfo { get; }

        /// <summary>Default alignment information for integer types</summary>
        IReadOnlyList<SizeAndAlignment> IntegerAlignmentInfo { get; }

        /// <summary>Default alignment information for pointer types</summary>
        IReadOnlyList<SizeAndAlignment> PointerInfo { get; }

        /// <summary>Default alignment information for the stack</summary>
        int StackAlignment { get; }

        /// <summary>Byte Ordering</summary>
        ByteOrdering ByteOrder { get; }
    }

    /// <summary>Base class with default information for creating implementations of <see cref="ITargetDataLayout"/></summary>
    public class TargetDataLayout
        : ITargetDataLayout
    {
        public IReadOnlyList<int> NativeIntWidths => NativeIntWidths_.AsReadOnly( );
        readonly List<int> NativeIntWidths_ = new List<int>( );

        public NameMangling NameMangling { get; protected set; }

        public AlignmentInfo AggregateAlignmentInfo { get; protected set; }

        public IReadOnlyList<SizeAndAlignment> FloatAlignmentInfo => FloatAlignmentInfo_.AsReadOnly( );
        readonly List<SizeAndAlignment> FloatAlignmentInfo_ = new List<SizeAndAlignment>( );

        public IReadOnlyList<SizeAndAlignment> VectorAlignmentInfo => VectorAlignmentInfo_.AsReadOnly( );
        readonly List<SizeAndAlignment> VectorAlignmentInfo_ = new List<SizeAndAlignment>( );

        public IReadOnlyList<SizeAndAlignment> IntegerAlignmentInfo => IntegerAlignmentInfo_.AsReadOnly( );
        readonly List<SizeAndAlignment> IntegerAlignmentInfo_ = new List<SizeAndAlignment>( );

        public IReadOnlyList<SizeAndAlignment> PointerInfo => PointerInfo_.AsReadOnly( );
        readonly List<SizeAndAlignment> PointerInfo_ = new List<SizeAndAlignment>( );

        public int StackAlignment { get; protected set; }

        public ByteOrdering ByteOrder { get; protected set; }

        protected TargetDataLayout( int nativeIntWidth, int stackAlignment, int aggregateAlignment, ByteOrdering endianMode, NameMangling nameMangling )
        {
            ByteOrder = endianMode;
            StackAlignment = stackAlignment;
            AggregateAlignmentInfo = new AlignmentInfo( aggregateAlignment, aggregateAlignment );
            NameMangling = nameMangling;
            PointerInfo_.Add( new SizeAndAlignment( nativeIntWidth, nativeIntWidth, nativeIntWidth ) );
            NativeIntWidths_.Add( nativeIntWidth );
        }

        public TargetDataLayout( )
            : this( 64, 0, 64, ByteOrdering.BigEndian, NameMangling.Elf )
        {
            IntegerAlignmentInfo_.Add( new SizeAndAlignment( 1, 8, 8 ) );
            IntegerAlignmentInfo_.Add( new SizeAndAlignment( 8, 8, 8 ) );
            IntegerAlignmentInfo_.Add( new SizeAndAlignment( 16, 16, 16 ) );
            IntegerAlignmentInfo_.Add( new SizeAndAlignment( 32, 32, 32 ) );
            IntegerAlignmentInfo_.Add( new SizeAndAlignment( 64, 64, 64 ) );
            FloatAlignmentInfo_.Add( new SizeAndAlignment( 16, 16, 16 ) );
            FloatAlignmentInfo_.Add( new SizeAndAlignment( 32, 32, 32 ) );
            FloatAlignmentInfo_.Add( new SizeAndAlignment( 128, 128, 128 ) );
            VectorAlignmentInfo_.Add( new SizeAndAlignment( 64, 64, 64 ) );
            VectorAlignmentInfo_.Add( new SizeAndAlignment( 128, 128, 128 ) );
        }

        /// <summary>Builds an LLVM target layout string from the information in this layout</summary>
        /// <returns>LLVM target layout formatted string representation of this layout</returns>
        public override string ToString( )
        {
            StringBuilder bldr = new StringBuilder( );
            string value;
            if( ByteOrder == ByteOrdering.LittleEndian )
                value = "e";
            else
                value = "E";

            bldr.Append( value );
            switch( NameMangling )
            {
            case NameMangling.Elf:
                bldr.Append( "-m:e" );
                break;
            case NameMangling.Mips:
                bldr.Append( "-m:m" );
                break;
            case NameMangling.MachO:
                bldr.Append( "-m:o" );
                break;
            case NameMangling.PeCoff:
                bldr.Append( "-m:w" );
                break;
            }

            for( int i = 0; i < PointerInfo_.Count; i++ )
            {
                bldr.Append( "-p" );
                if( i > 0 )
                    bldr.Append( i );

                bldr.AppendFormat( ":{0}", PointerInfo_[ i ] );
            }
            
            foreach( var info in IntegerAlignmentInfo )
                bldr.AppendFormat( "-i{0}", info );

            foreach( var info in VectorAlignmentInfo )
                bldr.AppendFormat( "-v{0}", info );

            foreach( var info in FloatAlignmentInfo )
                bldr.AppendFormat( "-f{0}", info );

            bldr.AppendFormat( "-a:{0}", AggregateAlignmentInfo );
            for( int j = 0; j < NativeIntWidths_.Count; j++ )
            {
                bldr.Append( "-n" );
                if( j > 0 )
                    bldr.Append( ":" );

                bldr.Append( NativeIntWidths_[ j ] );
            }

            if( StackAlignment > 0 )
                bldr.AppendFormat( "-S{0}", StackAlignment );

            return bldr.ToString( );
        }

        protected void AddIntegerAlignment( SizeAndAlignment alignmentInfo )
        {
            IntegerAlignmentInfo_.Add( alignmentInfo );
        }

        protected void AddIntegerAlignment( IEnumerable<SizeAndAlignment> alignmentInfo )
        {
            IntegerAlignmentInfo_.AddRange( alignmentInfo );
        }

        protected void AddFloatAlignment( SizeAndAlignment alignmentInfo )
        {
            FloatAlignmentInfo_.Add( alignmentInfo );
        }

        protected void AddFloatAlignment( IEnumerable<SizeAndAlignment> alignmentInfo )
        {
            FloatAlignmentInfo_.AddRange( alignmentInfo );
        }

        protected void AddVectorAlignment( SizeAndAlignment alignmentInfo )
        {
            VectorAlignmentInfo_.Add( alignmentInfo );
        }

        protected void AddVectorAlignment( IEnumerable<SizeAndAlignment> alignmentInfo )
        {
            VectorAlignmentInfo_.AddRange( alignmentInfo );
        }
    }
}

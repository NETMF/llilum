using System.ComponentModel;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    public class XpsrRegisterDetailsViewModel
        : INotifyPropertyChanged 
    {
        public XpsrRegisterDetailsViewModel( CoreRegisterViewModel vm )
        {
            Register = vm;
            PropertyChangedEventManager.AddHandler( vm, RegValueChanged, nameof( vm.Value ) );
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RegValueChanged( object sender, PropertyChangedEventArgs e )
        {
            var val = Register.Value;
            N = ( val & NBitMask ) != 0;
            Z = ( val & ZBitMask ) != 0;
            C = ( val & CBitMask ) != 0;
            V = ( val & VBitMask ) != 0;
            Q = ( val & QBitMask ) != 0;
            ExceptionNum = ( int )( val & ExceptionMask );
            PropertyChanged( this, AllPropertiesChangedEventArgs );
        }

        public bool N { get; private set; }
        public bool Z { get; private set; }
        public bool C { get; private set; }
        public bool V { get; private set; }
        public bool Q { get; private set; }
        public int ExceptionNum { get; private set; }

        private readonly CoreRegisterViewModel Register;
        
        // see ARM-V7M Arch manual (B1.4.2)
        private const uint NBitMask = 1u << 31;
        private const uint ZBitMask = 1u << 30;
        private const uint CBitMask = 1u << 29;
        private const uint VBitMask = 1u << 28;
        private const uint QBitMask = 1u << 27;
        private const uint ExceptionMask = 0x1FF;

        private static readonly PropertyChangedEventArgs AllPropertiesChangedEventArgs = new PropertyChangedEventArgs( string.Empty );
    }
}

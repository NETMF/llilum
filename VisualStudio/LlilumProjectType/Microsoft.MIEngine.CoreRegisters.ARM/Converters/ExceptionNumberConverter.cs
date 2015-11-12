using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{

    public class ExceptionNumberConverter
        : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if( targetType != typeof( string ) )
                return null;

            var exceptionNum = ( int )value;
            switch( exceptionNum )
            {
            case 0:
                return "<none>";
            case 1:
                return "Reset";
            case 2:
                return "NMI";
            case 3:
                return "HardFault";
            case 4:
                return "MemManage";
            case 5:
                return "BusFault";
            case 6:
                return "UsageFault";
            case 7:
                return "Reserved7";
            case 8:
                return "Reserved8";
            case 9:
                return "Reserved9";
            case 10:
                return "Reserved10";
            case 11:
                return "SvcCall";
            case 12:
                return "DebugMonitor";
            case 13:
                return "Reserved13";
            case 14:
                return "PendSV";
            case 15:
                return "SysTick";
            default:
                // TODO: make this extensible so that the UI can show actual SOC interrupt source names 
                return $"ExternalInt{exceptionNum - 16}";
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException( );
        }
    }
}

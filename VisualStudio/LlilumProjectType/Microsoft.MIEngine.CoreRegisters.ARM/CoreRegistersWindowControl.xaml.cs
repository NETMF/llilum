// The MIT License( MIT)
// 
// Copyright( c) 2015 Microsoft
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    /// <summary>Interaction logic for CoreRegistersWindowControl. </summary>
    public partial class CoreRegistersWindowControl
    {
        /// <summary>Initializes a new instance of the <see cref="CoreRegistersWindowControl"/> class.</summary>
        internal CoreRegistersWindowControl( CoreRegistersViewModel context )
        {
            DataContext = context;
            InitializeComponent( );
            ICollectionView view = CollectionViewSource.GetDefaultView( RegistersView.ItemsSource );
            view.GroupDescriptions.Add( new PropertyGroupDescription( "Group" ) );
        }
    }

    public class RegisterDetailsTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate( object item, DependencyObject container )
        {
            var element = container as FrameworkElement;
            var register = item as CoreRegisterViewModel;
            if( element == null || register == null )
                return null;

            var templateName = register.Name == "xpsr" ? "XpsrRegisterDetailsViewModelTemplate" : "DefaultRegisterDetailsTemplate";
            return element.FindResource( templateName ) as DataTemplate;
        }
    }
}
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

using System;
using System.ComponentModel;

namespace Microsoft.MIEngine.CoreRegisters.ARM
{
    public class CoreRegisterViewModel
        : INotifyPropertyChanged
    {
        public CoreRegisterViewModel( string name, string group, int id )
            : this( name, group, id, null )
        {
        }

        public CoreRegisterViewModel( string name, string group, int id, Func<CoreRegisterViewModel, object> detailFactory )
        {
            Name = name;
            Group = group;
            Id = id;
            Value = 0;
            if( detailFactory != null )
                Details = detailFactory( this );
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        public string Name { get; }

        public uint Value
        {
            get { return Value_; }
            internal set
            {
                Value_ = value;
                PropertyChanged( this, ValueChangedEventArgs );
                IsChanged = true;
            }
        }
        uint Value_;

        public object Details { get; }

        public string Group { get; }

        public int Id { get; }

        public bool IsChanged
        {
            get { return IsChanged_; }
            internal set
            {
                IsChanged_ = value;
                PropertyChanged( this, IsChangedEventArgs );
            }
        }
        private bool IsChanged_;

        private static readonly PropertyChangedEventArgs IsChangedEventArgs = new PropertyChangedEventArgs( nameof( IsChanged ) );
        private static readonly PropertyChangedEventArgs ValueChangedEventArgs = new PropertyChangedEventArgs( nameof( Value ) );
    }
}

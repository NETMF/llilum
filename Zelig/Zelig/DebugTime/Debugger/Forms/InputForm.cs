using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    public partial class InputForm : Form
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        public InputForm( string title ,
                          string text  )
        {
            InitializeComponent();

            //--//

            this.Text = title;

            textBoxInput.Text = text;
        }

        //
        // Helper Methods
        //

        //
        // Access Methods
        //

        public string Result
        {
            get
            {
                return textBoxInput.Text;
            }
        }

        //
        // Event Methods
        //

        private void buttonOk_Click( object    sender ,
                                     EventArgs e      )
        {
            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private void buttonCancel_Click( object    sender ,
                                         EventArgs e      )
        {
            this.DialogResult = DialogResult.Cancel;

            this.Close();
        }
    }
}
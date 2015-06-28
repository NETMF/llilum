using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Microsoft.Zelig.Tools.IRViewer
{
    public partial class MainForm : Form
    {
        //
        // State
        //

        string m_phaseName;

        Dictionary< string, Method > m_lookupMethod;

        //
        // Constructor Methods
        //

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click( object    sender ,
                                                  EventArgs e      )
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

                    doc.Load( openFileDialog1.FileName );

                    m_phaseName = Path.GetFileNameWithoutExtension( openFileDialog1.FileName );

                    int idx = m_phaseName.LastIndexOf( '.' );

                    if(idx >= 0)
                    {
                        m_phaseName = m_phaseName.Substring( idx + 1, m_phaseName.Length - idx - 1 );
                    }

                    this.Text = "IRViewer - " + m_phaseName;

                    foreach(System.Xml.XmlNode methods in doc.SelectNodes( "Methods" ))
                    {
                        Parser parser = new Parser( methods );

                        Dictionary< string, Method > lookup = parser.Methods;

                        m_lookupMethod = lookup;

                        UpdateListBox();
                    }
                }
                catch
                {
                }
            }
        }

        private void textBoxFilter_TextChanged( object    sender ,
                                                EventArgs e      )
        {
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            if(m_lookupMethod != null)
            {
                string filter = textBoxFilter.Text;

                List< string > lst = new List<string>();

                foreach(string id in m_lookupMethod.Keys)
                {
                    if(string.IsNullOrEmpty( filter ) || id.Contains( filter ))
                    {
                        lst.Add( id );
                    }
                }

                lst.Sort();

                ListBox.ObjectCollection col = listBoxMethods.Items;

                listBoxMethods.SuspendLayout();

                col.Clear();

                col.AddRange( lst.ToArray() );

                listBoxMethods.ResumeLayout();
            }
        }

        private void listBoxMethods_DoubleClick( object    sender ,
                                                 EventArgs e      )
        {
            if(m_lookupMethod != null)
            {
                if(listBoxMethods.SelectedItem != null)
                {
                    Method method = m_lookupMethod[(string)listBoxMethods.SelectedItem];

                    MethodViewer mv = new MethodViewer( m_lookupMethod, method, m_phaseName );

                    mv.Show();
                }
            }
        }
    }
}
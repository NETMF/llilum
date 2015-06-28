//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Tools.IRCompare
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    using IR  = Microsoft.Zelig.CodeGeneration.IR;
    using TS  = Microsoft.Zelig.Runtime.TypeSystem;
    using System.IO;

////using ARM = Microsoft.Zelig.Emulation.ArmProcessor;

    public partial class MainForm : Form
    {
        //
        // State
        //

        Dictionary< string, TS.MethodRepresentation > m_lookupMethod;
        IR.TypeSystemForCodeTransformation            m_typeSystem;
        RenderMethod                                  m_renderer;

        string m_IRName;


        //
        // Constructor Methods
        //

        public MainForm()
        {
            InitializeComponent();

            m_renderer = new RenderMethod();
        }

        //--/

        private object CreateInstanceForType( Type t )
        {
            if(t == typeof(IR.TypeSystemForCodeTransformation))
            {
                return new IR.TypeSystemForCodeTransformation( null );
            }

            return null;
        }

        //--//

        private void openToolStripMenuItem_Click( object    sender ,
                                                  EventArgs e      )
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_IRName = Path.GetFileNameWithoutExtension( openFileDialog1.FileName );

                    int idx = m_IRName.LastIndexOf( '.' );

                    if(idx >= 0)
                    {
                        m_IRName = m_IRName.Substring( idx + 1, m_IRName.Length - idx - 1 );
                    }

                    this.Text = "IR Compare - " + m_IRName;

                    using(System.IO.FileStream stream = new System.IO.FileStream( openFileDialog1.FileName, System.IO.FileMode.Open ))
                    {
                        m_typeSystem = IR.TypeSystemSerializer.Deserialize( stream, CreateInstanceForType, null, 0 );

                        m_lookupMethod = new Dictionary< string, TS.MethodRepresentation >();

                        m_typeSystem.EnumerateMethods( delegate( TS.MethodRepresentation md )
                        {
                            m_lookupMethod[md.ToShortString()] = md;
                        } );
                    }

                    UpdateListBox();
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
            if(m_lookupMethod != null && listBoxMethods.SelectedItem != null)
            {
                TS.MethodRepresentation method = m_lookupMethod[ (string)listBoxMethods.SelectedItem ];

                MethodViewer mv = new MethodViewer( m_lookupMethod, method, m_renderer, m_IRName );

                mv.Show();
            }
        }
    }
}
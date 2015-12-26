//
// Copyright (c) Microsoft Corporation.     All rights reserved.
//

namespace Microsoft.Zelig.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Forms;

    using Microsoft.Zelig.Debugger.ArmProcessor;
    using Microsoft.Zelig.Emulation.Hosting;
    using Microsoft.Zelig.Runtime;
    
    using Cfg = Microsoft.Zelig.Configuration.Environment;

    using Microsoft.Zelig.Elf;

    public partial class Loader : Form, IMainForm
    {
        Cfg.Manager         m_configurationManager;
        Cfg.ProductCategory m_product;
        Cfg.EngineCategory  m_engine;

        EnvironmentForm     m_environmentForm;
        
        String              m_imageFile;

        Thread              m_worker;

        public Loader()
        {
            InitializeComponent();

            //--//

            m_configurationManager = new Cfg.Manager();

            m_configurationManager.AddAllAssemblies( null );
            m_configurationManager.ComputeAllPossibleValuesForFields();

            //--//

            m_environmentForm = new EnvironmentForm( this );
        }

        //--//
        
        public Cfg.Manager ConfigurationManager
        {
            get
            {
                return m_configurationManager;
            }
        }

        //--//

        private void btnConfig_Click(object sender, EventArgs e)
        {
            DialogResult res = m_environmentForm.ShowDialog();
            if (res == DialogResult.OK)
            {
                m_engine  = m_environmentForm.SelectedEngine;
                m_product = m_environmentForm.SelectedProduct;
            }
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                m_imageFile = openFileDialog1.FileName;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            m_worker = new Thread(LoadImageAndExecute);

            m_worker.IsBackground = true;

            m_worker.Start();
        }

        //--//

        private void LoadImageAndExecute()
        {
            //
            // Open ELF and see if it is valid
            //

            ElfObject[] elfs = ElfObject.FileUtil.Parse( m_imageFile );
            uint entryPoint = 0;


            if(elfs == null)
            {
                throw TypeConsistencyErrorException.Create( "Unrecognized file: {0}", m_imageFile );
            }

            var sections = new List<Cfg.ImageSection>();

            foreach(ElfObject elf in elfs)
            {
                if(elf.Header.e_type != e_type.ET_EXEC)
                {
                    throw TypeConsistencyErrorException.Create( "ELf is not an exec file: {0}", m_imageFile );
                }

                //
                // Create list of ImageSections
                //

                foreach(var segment in elf.Segments)
                {
                    if(segment.Type == SegmentType.PT_LOAD)
                    {
                        foreach(var section in segment.ReferencedSections)
                        {
                            sections.Add( new Cfg.ImageSection( section.AddressInMemory, section.Raw, section.Name, 0, MemoryUsage.Relocation ) );
                        }
                    }
                }

                entryPoint = elf.EntryPoint;
            }

            //--//

            var engine = m_engine.Instantiate(null) as AbstractHost;

            if(engine == null)
            {
                throw TypeConsistencyErrorException.Create( "Unrecognized engine: {0}", m_engine );
            }

            ProcessorControl svcPC; engine.GetHostingService( out svcPC );

            svcPC.ResetState( m_product );
            svcPC.PrepareHardwareModels( m_product );

            ProcessorStatus svcPS; engine.GetHostingService( out svcPS );

            svcPC.DeployImage( sections, delegate( string format, object[] args )
            {
                this.BeginInvoke( (MethodInvoker)delegate()
                {
                    StatusLabel.Text = string.Format( format, args );
                } );

            } );

            svcPS.ProgramCounter = entryPoint;

            this.BeginInvoke( (MethodInvoker)delegate()
            {
                StatusLabel.Text = "Deployed and running";
            } );

            svcPC.Execute( new List<Breakpoint>() );
        }

        //--//
    }
}

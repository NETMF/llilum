//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;


    public partial class DisplayForm : Emulation.Hosting.Forms.BaseDebuggerForm
    {
        class SED15E0SinkImpl : Emulation.Hosting.SED15E0Sink
        {
            //
            // State
            //

            DisplayForm m_owner;

            //
            // Constructor Methods
            //

            internal SED15E0SinkImpl( DisplayForm owner )
            {
                m_owner = owner;

                //--//

                owner.Host.RegisterService( typeof(Emulation.Hosting.SED15E0Sink), this );
            }

            //
            // Helper Methods
            //

            public override void NewScreenShot( uint[] buffer ,
                                                int    width  ,
                                                int    height )
            {

                Emulation.Hosting.ProcessorControl svcPC; m_owner.Host.GetHostingService( out svcPC );

                if(svcPC.StopExecution) return;

                m_owner.SendScreenShot( buffer, width, height );
            }
        }

        //
        // State
        //

        uint[]          m_buffer;
        int             m_buffer_width;
        int             m_buffer_height;
        SED15E0SinkImpl m_implSED15E0Sink;

        //
        // Constructor Methods
        //

        public DisplayForm( Emulation.Hosting.Forms.HostingSite site ) : base( site )
        {
            InitializeComponent();

            //--//

            m_implSED15E0Sink = new SED15E0SinkImpl( this );

            //--//

            timer1.Tick += new EventHandler( UpdateTimerCallback );

            //--//

            site.RegisterView( this, Emulation.Hosting.Forms.HostingSite.PublicationMode.Tools );
        }

        //
        // Helper Methods
        //

        protected override void NotifyChangeInVisibility( bool fVisible )
        {
            if(fVisible)
            {
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        protected override bool ProcessKeyDown( KeyEventArgs e )
        {
            uint mask = 0;

            switch(e.KeyCode)
            {
                case Keys.Down : mask = Emulation.Hosting.HalButtons.BUTTON_B5; break;
                case Keys.Up   : mask = Emulation.Hosting.HalButtons.BUTTON_B2; break;
                case Keys.Left : mask = Emulation.Hosting.HalButtons.BUTTON_B1; break;
                case Keys.Right: mask = Emulation.Hosting.HalButtons.BUTTON_B4; break;
            }

            if(mask != 0)
            {
                Emulation.Hosting.HalButtons svc;
                
                if(this.Host.GetHostingService( out svc ))
                {
                    svc.QueueNextStateChange( mask, 0 );
                }

                return true;
            }

            return false;
        }

        protected override bool ProcessKeyUp( KeyEventArgs e )
        {
            uint mask = 0;

            switch(e.KeyCode)
            {
                case Keys.Down : mask = Emulation.Hosting.HalButtons.BUTTON_B5; break;
                case Keys.Up   : mask = Emulation.Hosting.HalButtons.BUTTON_B2; break;
                case Keys.Left : mask = Emulation.Hosting.HalButtons.BUTTON_B1; break;
                case Keys.Right: mask = Emulation.Hosting.HalButtons.BUTTON_B4; break;
            }

            if(mask != 0)
            {
                Emulation.Hosting.HalButtons svc;
                
                if(this.Host.GetHostingService( out svc ))
                {
                    svc.QueueNextStateChange( 0, mask );
                }

                return true;
            }

            return false;
        }

        //--//

        public void SendScreenShot( uint[] buffer ,
                                    int    width  ,
                                    int    height )
        {
            lock(this)
            {
                m_buffer        = buffer;
                m_buffer_width  = width;
                m_buffer_height = height;
            }
        }

        private void UpdateTimerCallback( object    sender ,
                                          EventArgs e      )
        {
            uint[] buffer;
            int    width;
            int    height;

            lock(this)
            {
                buffer = m_buffer;
                width  = m_buffer_width;
                height = m_buffer_height;

                m_buffer = null;
            }

            if(buffer != null)
            {
                UpdateScreen( buffer, width, height );
            }
        }

        private void UpdateScreen( uint[] buffer ,
                                   int    width  ,
                                   int    height )
        {
            pictureBox_LCD.Image = ConvertBufferToBitmap( buffer, width, height );
        }

        //--//

        private static uint[] Adjust1bppOrientation( uint[] buf )
        {
            //CLR_GFX_Bitmap::AdjustBitOrientation
            //The TinyCLR treats 1bpp bitmaps reversed from Windows
            //And most likely every other 1bpp format as well
            byte[] reverseTable = new byte[]
            {
                0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0,
                0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
                0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8,
                0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
                0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4,
                0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
                0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC,
                0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
                0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2,
                0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
                0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA,
                0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
                0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6,
                0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
                0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE,
                0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
                0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1,
                0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
                0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9,
                0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
                0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5,
                0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
                0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED,
                0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
                0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3,
                0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
                0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB,
                0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
                0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7,
                0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
                0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF,
                0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F,0xFF,
                };

            unsafe
            {
                uint[] res = new uint[buf.Length];

                fixed(uint* pSrc = buf)
                {
                    fixed(uint* pDst = res)
                    {
                        byte* ptrSrc = (byte*)pSrc;
                        byte* ptrDst = (byte*)pDst;

                        for(int i = buf.Length * 4; i > 0; i--)
                        {
                            *ptrDst++ = reverseTable[*ptrSrc++];
                        }
                    }
                }

                return res;
            }
        }

        private static Bitmap ConvertBufferToBitmap( uint[] buffer ,
                                                     int    width  ,
                                                     int    height )
        {
            buffer = Adjust1bppOrientation( buffer );

            Bitmap                            bmp        = null;
            System.Drawing.Imaging.BitmapData bitmapData = null;

            try
            {
                bmp = new Bitmap( width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed );

                Rectangle rect = new Rectangle( 0, 0, width, height );

                System.Drawing.Imaging.ColorPalette palette = bmp.Palette;

                palette.Entries[0] = System.Drawing.Color.White;
                palette.Entries[1] = System.Drawing.Color.Black;

                bmp.Palette = palette;

                bitmapData = bmp.LockBits( rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed  );

                IntPtr data = bitmapData.Scan0;

                unsafe
                {
                    fixed(uint* pbuf = buffer)
                    {
                        uint* src = (uint*)pbuf;
                        uint* dst = (uint*)data.ToPointer();

                        for(int i = buffer.Length; i > 0; i--)
                        {
                            *dst = *src;
                            dst++;
                            src++;
                        }
                    }
                }
            }
            finally
            {
                if(bitmapData != null)
                {
                    bmp.UnlockBits( bitmapData );
                }
            }

            return bmp;
        }

        private void QueueButton( uint mask )
        {
            Emulation.Hosting.HalButtons svc;
            
            if(this.Host.GetHostingService( out svc ))
            {
                svc.QueueNextStateChange( mask, 0 );
            }
        }

        //
        // Access Methods
        //

        public override string ViewTitle
        {
            get
            {
                return "&Watch UI";
            }
        }

        //
        // Event Methods
        //


        private void button_Up_Click( object    sender ,
                                      EventArgs e      )
        {
            QueueButton( Emulation.Hosting.HalButtons.BUTTON_B2 );
        }

        private void button_Down_Click( object    sender ,
                                        EventArgs e      )
        {
            QueueButton( Emulation.Hosting.HalButtons.BUTTON_B5 );
        }

        private void button_Enter_Click( object    sender ,
                                         EventArgs e      )
        {
            QueueButton( Emulation.Hosting.HalButtons.BUTTON_B4 );
        }

        private void button_Channel_Click( object    sender ,
                                           EventArgs e      )
        {
            QueueButton( Emulation.Hosting.HalButtons.BUTTON_B1 );
        }

        private void button_Backlight_Click( object    sender ,
                                             EventArgs e      )
        {
            QueueButton( Emulation.Hosting.HalButtons.BUTTON_B0 );
        }

        private void button_Spare_Click( object    sender ,
                                         EventArgs e      )
        {
            QueueButton( Emulation.Hosting.HalButtons.BUTTON_B3 );
        }
    }
}
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
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg                = Microsoft.Zelig.Configuration.Environment;


    public class Session
    {
        //
        // State
        //

        public string              m_settingsFile;
        public bool                m_dirty;
        public bool                m_temporary;

        public Guid                m_id;
        public string              m_displayName;
        public DateTime            m_lastModified;

        public Cfg.EngineCategory  m_selectedEngine;
        public Cfg.ProductCategory m_selectedProduct;

        public string              m_imageToLoad;

        public bool                m_displayDisassembly;
        public uint                m_pastOpcodesToDisassembly   = 5;
        public uint                m_futureOpcodesToDisassembly = 5;

        //
        // Constructor Methods
        //

        public Session()
        {
            m_id           = Guid.NewGuid();
            m_lastModified = DateTime.Now;
        }

        public Session( Session session ) : this()
        {
            m_selectedEngine             = session.m_selectedEngine;
            m_selectedProduct            = session.m_selectedProduct;

            m_imageToLoad                = session.m_imageToLoad;
                                                   
            m_displayDisassembly         = session.m_displayDisassembly;
            m_pastOpcodesToDisassembly   = session.m_pastOpcodesToDisassembly;
            m_futureOpcodesToDisassembly = session.m_futureOpcodesToDisassembly;
        }

        //
        // Helper Methods
        //

        public byte[] ToArray()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                Save( stream );

                return stream.ToArray();
            }
        }

        public void Save( string file    ,
                          bool   fUpdate )
        {
            using(Stream stream = new FileStream( file, FileMode.Create, FileAccess.Write ))
            {
                if(fUpdate)
                {
                    m_settingsFile = file;
                }

                Save( stream );
            }
        }

        public void Save( Stream stream )
        {
            System.Xml.XmlDocument doc  = new System.Xml.XmlDocument();
            System.Xml.XmlElement  root = XmlHelper.AddElement( doc, "Session" );
            System.Xml.XmlElement  node;

            XmlHelper.AddAttribute( root, "Id"          , m_id           );
            XmlHelper.AddAttribute( root, "Name"        , m_displayName  );
            XmlHelper.AddAttribute( root, "LastModified", m_lastModified );

            XmlHelper.AddAttribute( root, "DisplayDisassembly"        , m_displayDisassembly         );
            XmlHelper.AddAttribute( root, "PastOpcodesToDisassembly"  , m_pastOpcodesToDisassembly   );
            XmlHelper.AddAttribute( root, "FutureOpcodesToDisassembly", m_futureOpcodesToDisassembly );

            node = XmlHelper.AddElement( root, "Engine" );
            Cfg.Manager.Serialize( node, m_selectedEngine );

            node = XmlHelper.AddElement( root, "Product" );
            Cfg.Manager.Serialize( node, m_selectedProduct );

            node = XmlHelper.AddElement( root, "Image" );
            XmlHelper.AddAttribute( node, "File", m_imageToLoad );

            doc.Save( stream );

            m_dirty = false;
        }

        //--//

        public static Session Load( string file )
        {
            using(Stream stream = new FileStream( file, FileMode.Open, FileAccess.Read ))
            {
                Session session = new Session();

                session.Load( stream );

                return session;
            }
        }

        public static Session LoadAndSetOrigin( string file )
        {
            Session session = Load( file );

            session.SettingsFile = file;

            return session;
        }

        public static Session Load( byte[] blob )
        {
            using(Stream stream = new MemoryStream( blob ))
            {
                Session session = new Session();

                session.Load( stream );

                return session;
            }
        }

        public void Load( Stream stream )
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlNode     root;

            m_dirty = false;

            doc.Load( stream );

            root = doc.SelectSingleNode( "Session" );
            if(root != null)
            {
                m_id                         =                      XmlHelper  .GetAttribute( root, "Id"                        , Guid.NewGuid() );
                m_displayName                =                      XmlHelper  .GetAttribute( root, "Name"                                       );
                m_lastModified               =                      XmlHelper  .GetAttribute( root, "LastModified"              , DateTime.Now   );
                                                                             
                m_displayDisassembly         =                      XmlHelper  .GetAttribute( root, "DisplayDisassembly"        , false          );
                m_pastOpcodesToDisassembly   =                      XmlHelper  .GetAttribute( root, "PastOpcodesToDisassembly"  , 5u             );
                m_futureOpcodesToDisassembly =                      XmlHelper  .GetAttribute( root, "FutureOpcodesToDisassembly", 5u             );

                m_selectedEngine             = (Cfg.EngineCategory )Cfg.Manager.Deserialize ( root.SelectSingleNode( "Engine"  )         );
                m_selectedProduct            = (Cfg.ProductCategory)Cfg.Manager.Deserialize ( root.SelectSingleNode( "Product" )         );
                m_imageToLoad                =                      XmlHelper  .GetAttribute( root.SelectSingleNode( "Image"   ), "File" );
            }
        }

        //
        // Access Methods
        //

        public string SettingsFile
        {
            get
            {
                return m_settingsFile;
            }

            set
            {
                m_settingsFile = value;

                this.Dirty = true;
            }
        }

        public bool Dirty
        {
            get
            {
                return m_dirty;
            }

            set
            {
                m_dirty = value;

                if(value)
                {
                    m_lastModified = DateTime.Now;
                }
            }
        }

        public bool IsTemporary
        {
            get
            {
                return m_temporary;
            }

            set
            {
                m_temporary = value;

                if(m_temporary)
                {
                    m_id = Guid.NewGuid();
                }
            }
        }

        public Guid Id
        {
            get
            {
                return m_id;
            }
        }

        public string DisplayName
        {
            get
            {
                return m_displayName;
            }

            set
            {
                m_displayName = value;

                this.Dirty = true;
            }
        }

        public DateTime LastModified
        {
            get
            {
                return m_lastModified;
            }
        }

        public Cfg.EngineCategory SelectedEngine
        {
            get
            {
                return m_selectedEngine;
            }

            set
            {
                m_selectedEngine = value;

                this.Dirty = true;
            }
        }

        public Cfg.ProductCategory SelectedProduct
        {
            get
            {
                return m_selectedProduct;
            }

            set
            {
                m_selectedProduct = value;

                this.Dirty = true;
            }
        }

        public string ImageToLoad
        {
            get
            {
                return m_imageToLoad;
            }

            set
            {
                m_imageToLoad = value;

                this.Dirty = true;
            }
        }


        public bool DisplayDisassembly
        {
            get
            {
                return m_displayDisassembly;
            }

            set
            {
                m_displayDisassembly = value;

                this.Dirty = true;
            }
        }

        public uint PastOpcodesToDisassembly
        {
            get
            {
                return m_pastOpcodesToDisassembly;
            }
        }

        public uint FutureOpcodesToDisassembly
        {
            get
            {
                return m_futureOpcodesToDisassembly;
            }
        }
    }
}
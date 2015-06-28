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

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public partial class EnvironmentForm : Form
    {
        internal class SelectionHolder
        {
            //
            // State
            //

            internal List< Cfg.AbstractCategory.ValueContext[] > m_choices;
            internal SelectionCombination[]                      m_combinations;
            internal int[]                                       m_selected;

            //
            // Constructor Methods
            //

            internal SelectionHolder( Cfg.ProductCategory selectedProduct ,
                                      Type                filter          )
            {
                m_choices = new List< Cfg.AbstractCategory.ValueContext[] >();

                foreach(Cfg.AbstractCategory.ValueContext ctx in selectedProduct.SearchPossibleValues( filter ))
                {
                    bool fGot = false;

                    for(int i = 0; i < m_choices.Count; i++)
                    {
                        var ctxArray = m_choices[i];
                        var ctxOld   = ctxArray[0];

                        if(ctxOld.Holder == ctx.Holder &&
                           ctxOld.Field  == ctx.Field   )
                        {
                            m_choices[i] = ArrayUtility.AppendToNotNullArray( ctxArray, ctx );

                            fGot = true;
                            break;
                        }
                    }

                    if(fGot == false)
                    {
                        m_choices.Add( new Cfg.AbstractCategory.ValueContext[] { ctx } );
                    }
                }
            }

            //
            // Helper Methods
            //

            internal void ApplyToComboBox( ComboBox comboBox )
            {
                ComboBox.ObjectCollection items = comboBox.Items;

                items.Clear();

                if(m_choices.Count == 0)
                {
                    items.Add( "<No Options>" );

                    comboBox.Enabled = false;

                    m_combinations = null;
                    m_selected     = null;
                }
                else
                {
                    int cols = m_choices.Count;
                    int rows = 1;

                    for(int col = 0; col < cols; col++)
                    {
                        rows *= m_choices[col].Length;
                    }

                    m_combinations = new SelectionCombination[rows];

                    for(int row = 0; row < rows; row++)
                    {
                        int[] indices = new int[cols];

                        int val = row;

                        for(int col = 0; col < cols; col++)
                        {
                            int num = m_choices[col].Length;

                            indices[col] = val % num;

                            val /= num;
                        }

                        SelectionCombination comb = new SelectionCombination( this, indices );

                        m_combinations[row] = comb;

                        items.Add( comb );
                    }

                    if(rows == 1)
                    {
                        if(comboBox.SelectedIndex != 0)
                        {
                            comboBox.SelectedIndex = 0;
                        }

                        comboBox.Enabled = false;
                    }
                    else
                    {
                        comboBox.Enabled = true;
                    }
                }
            }

            internal bool SameSelections( SelectionHolder other )
            {
                if(this.m_choices.Count != other.m_choices.Count)
                {
                    return false;
                }

                for(int i = 0; i < this.m_choices.Count; i++)
                {
                    Cfg.AbstractCategory.ValueContext[] arrayThis  = this .m_choices[i];
                    Cfg.AbstractCategory.ValueContext[] arrayOther = other.m_choices[i];

                    if(arrayThis.Length != arrayOther.Length)
                    {
                        return false;
                    }

                    for(int j = 0; j < arrayThis.Length; j++)
                    {
                        if(arrayThis[j].Equals( arrayOther[j] ) == false)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            //
            // Access Methods
            //

            internal bool IsSelected
            {
                get
                {
                    return m_combinations == null || m_selected != null;
                }
            }
        }

        internal class SelectionCombination
        {
            //
            // State
            //

            private SelectionHolder m_holder;
            private int[]           m_indices;

            //
            // Constructor Methods
            //

            internal SelectionCombination( SelectionHolder holder  ,
                                           int[]           indices )
            {
                m_holder  = holder;
                m_indices = indices;
            }

            //
            // Helper Methods
            //

            internal void ApplySelection()
            {
                Cfg.AbstractCategory.ValueContext[] res = new Cfg.AbstractCategory.ValueContext[m_indices.Length];

                for(int i = 0; i < m_indices.Length; i++)
                {
                    Cfg.AbstractCategory.ValueContext ctx = m_holder.m_choices[i][m_indices[i]];

                    Cfg.AbstractCategory holder = ctx.Holder;
                    if(holder != null)
                    {
                        ctx.Field.SetValue( holder, ctx.Value );
                    }

                    Cfg.AbstractCategory selected = ctx.Value as Cfg.AbstractCategory;
                    if(selected != null)
                    {
                        selected.ApplyDefaultValues();
                    }
                }

                m_holder.m_selected = m_indices;
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                for(int i = 0; i < m_indices.Length; i++)
                {
                    if(i != 0)
                    {
                        sb.Append( " , " );
                    }

                    sb.Append( m_holder.m_choices[i][m_indices[i]].ToString() );
                }

                return sb.ToString();
            }
        }

        //
        // State
        //

        IMainForm               m_owner;
                                                 
        Cfg.EngineCategory      m_selectedEngine;
        Cfg.ProductCategory     m_selectedProduct;
        SelectionHolder         m_selectedProcessor;
        SelectionHolder         m_selectedDisplay;
        SelectionHolder         m_selectedRAM;
        SelectionHolder         m_selectedFLASH;

        //
        // Constructor Methods
        //

        public EnvironmentForm( IMainForm owner )
        {
            m_owner = owner;

            InitializeComponent();

            //--//

            Cfg.Manager manager = owner.ConfigurationManager;

            foreach(Cfg.AbstractCategory value in manager.AllValues)
            {
                if(value is Cfg.EngineCategory)
                {
                    comboBox_Engine.Items.Add( value );
                }

                if(value is Cfg.ProductCategory)
                {
                    comboBox_Product.Items.Add( value );
                }
            }

            SetStatus();
        }

        //
        // Helper Methods
        //

        private void SetStatus()
        {
            if(m_selectedProduct != null)
            {
                UpdateChoices();
            }

            SetComboBoxStatus( comboBox_Engine  );
            SetComboBoxStatus( comboBox_Product );

            button_Ok.Enabled = this.IsConfigured;
        }

        private void UpdateChoices()
        {
            UpdateIfChanged( comboBox_Processor, ref m_selectedProcessor, new SelectionHolder( m_selectedProduct, typeof(Cfg.ProcessorCategory  ) ) );
            UpdateIfChanged( comboBox_Display  , ref m_selectedDisplay  , new SelectionHolder( m_selectedProduct, typeof(Cfg.DisplayCategory    ) ) );
            UpdateIfChanged( comboBox_RAM      , ref m_selectedRAM      , new SelectionHolder( m_selectedProduct, typeof(Cfg.RamMemoryCategory  ) ) );
            UpdateIfChanged( comboBox_FLASH    , ref m_selectedFLASH    , new SelectionHolder( m_selectedProduct, typeof(Cfg.FlashMemoryCategory) ) );
        }

        private void UpdateIfChanged(     ComboBox        comboBox  ,
                                      ref SelectionHolder holderOld ,
                                          SelectionHolder holderNew )
        {
            if(holderOld != null)
            {
                if(holderOld.SameSelections( holderNew ))
                {
                    return;
                }
            }

            holderOld = holderNew;

            holderOld.ApplyToComboBox( comboBox );
        }

        private void SetComboBoxStatus( ComboBox comboBox )
        {
            switch(comboBox.Items.Count)
            {
                case 0:
                    comboBox.Enabled = false;
                    break;

                case 1:
                    if(comboBox.SelectedIndex != 0)
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    comboBox.Enabled = false;
                    break;

                default:
                    comboBox.Enabled = true;
                    break;
            }
        }

        //--//

        private void ApplyChoice( ComboBox comboBox )
        {
            SelectionCombination combination = (SelectionCombination)comboBox.SelectedItem;

            combination.ApplySelection();

            SetStatus();
        }

        //
        // Access Methods
        //

        public bool IsConfigured
        {
            get
            {
                if(m_selectedEngine  != null &&
                   m_selectedProduct != null  )
                {
                    if(m_selectedProcessor.IsSelected &&
                       m_selectedDisplay  .IsSelected &&
                       m_selectedRAM      .IsSelected &&
                       m_selectedFLASH    .IsSelected  )
                    {
                        return true;
                    }
                }

                return false;
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

                SetStatus();
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

                SetStatus();
            }
        }

        //
        // Event Methods
        //

        private void comboBox_Engine_SelectedIndexChanged( object    sender ,
                                                           EventArgs e      )
        {
            m_selectedEngine = (Cfg.EngineCategory)comboBox_Engine.SelectedItem;

            SetStatus();
        }

        private void comboBox_Product_SelectedIndexChanged( object    sender ,
                                                            EventArgs e      )
        {
            m_selectedProduct = (Cfg.ProductCategory)comboBox_Product.SelectedItem;
            m_selectedProduct.ApplyDefaultValues();

            SetStatus();
        }

        private void comboBox_Processor_SelectedIndexChanged( object    sender ,
                                                              EventArgs e      )
        {
            ApplyChoice( comboBox_Processor );
        }

        private void comboBox_Display_SelectedIndexChanged( object    sender ,
                                                            EventArgs e      )
        {
            ApplyChoice( comboBox_Display );
        }

        private void comboBox_RAM_SelectedIndexChanged( object    sender ,
                                                        EventArgs e      )
        {
            ApplyChoice( comboBox_RAM );
        }

        private void comboBox_FLASH_SelectedIndexChanged( object    sender ,
                                                          EventArgs e      )
        {
            ApplyChoice( comboBox_FLASH );
        }

        //--//

        private void button_Cancel_Click( object sender, EventArgs e )
        {
            this.DialogResult = DialogResult.Cancel;

            this.Close();
        }

        private void button_Ok_Click( object sender, EventArgs e )
        {
            this.DialogResult = DialogResult.OK;

            this.Close();
        }
   }
}
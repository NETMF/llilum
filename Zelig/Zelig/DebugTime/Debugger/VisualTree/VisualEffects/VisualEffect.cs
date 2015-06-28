//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Text;
    using System.Windows.Forms;

    using IR = Microsoft.Zelig.CodeGeneration.IR;


    public abstract partial class VisualEffect
    {
        //
        // State
        //

        public int                Version;
        public object             Context;
        public VisualTreeInfo     Owner;
        public List< VisualItem > Items;

        //
        // Constructor Methods
        //

        protected VisualEffect( VisualTreeInfo owner )
        {
            this.Owner = owner;
            this.Items = new List< VisualItem >();

            owner.VisualEffects.Add( this );
        }

        //
        // Helper Methods
        //

        public virtual void Clear()
        {
            foreach(VisualItem item in this.Items)
            {
                item.Delete();
            }

            this.Items.Clear();
        }
    }
}

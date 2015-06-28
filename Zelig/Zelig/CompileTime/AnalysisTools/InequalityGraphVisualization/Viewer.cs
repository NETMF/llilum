//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Tools.InequalityGraphVisualization
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using Microsoft.Zelig.CodeGeneration.IR;
    using Microsoft.Zelig.CodeGeneration.IR.Transformations;

    public class Viewer
    {
        //
        // State
        //

        static bool s_Initialized;
        //
        // Helper Methods
        //

        [STAThread]
        public static void Show( ConstraintSystemCollector.GraphState gs )
        {
            if(!s_Initialized)
            {
                s_Initialized = true;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
            }

            Application.Run( new GraphForm( gs ) );
        }
    }
}

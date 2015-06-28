//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime.TypeSystem;

    public class ExternalDataDescriptor : DataManager.DataDescriptor
    {
        public interface IExternalDataContext
        {
            byte[] RawData { get; }
            void WriteData( ImageBuilders.SequentialRegion region );
            object DataSection { get; }
        }

        private IExternalDataContext m_externContext;

        internal ExternalDataDescriptor()
        {
        }

        public ExternalDataDescriptor( DataManager                        owner   ,
                                       IExternalDataContext               context ,
                                       DataManager.Attributes             flags   ,
                                       Abstractions.PlacementRequirements pr      ) : base( owner, null, flags, pr )
        {
            m_externContext = context;
        }

        public IExternalDataContext ExternContext
        {
            get { return m_externContext; }
        }

        public override object GetDataAtOffset( Runtime.TypeSystem.FieldRepresentation[] accessPath, int accessPathIndex, int offset )
        {
            throw new NotImplementedException();
        }

        internal override void IncludeExtraTypes( Runtime.TypeSystem.TypeSystem.Reachability reachability, CompilationSteps.PhaseDriver phase )
        {
            throw new NotImplementedException();
        }

        internal override void Reduce( GrowOnlySet<DataManager.DataDescriptor> visited, Runtime.TypeSystem.TypeSystem.Reachability reachability, bool fApply )
        {
            
        }

        internal override void RefreshValues( CompilationSteps.PhaseDriver phase )
        {
            
        }

        internal override void Write( ImageBuilders.SequentialRegion region )
        {
            m_externContext.WriteData( region );
        }

        protected override string ToString( bool fVerbose )
        {
            return "ExternalDataDescriptor";
        }
    }
}

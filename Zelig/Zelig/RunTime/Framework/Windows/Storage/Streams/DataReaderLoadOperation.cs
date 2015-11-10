using System;
using Windows.Foundation;
using Windows.Internal;

namespace Windows.Storage.Streams
{
    /// <summary>Loads data from an input stream.</summary>
    public sealed class DataReaderLoadOperation : IAsyncOperation<uint>
    {
        private readonly SynchronousOperation<uint> m_internalOperation;

        internal DataReaderLoadOperation(uint result)
        {
            m_internalOperation = new SynchronousOperation<uint>(result);
        }

        internal DataReaderLoadOperation(Exception error)
        {
            m_internalOperation = new SynchronousOperation<uint>(error);
        }

        public AsyncOperationCompletedHandler<uint> Completed
        {
            get
            {
                return m_internalOperation.Completed;
            }

            set
            {
                m_internalOperation.Completed = value;
            }
        }

        public Exception ErrorCode => m_internalOperation.ErrorCode;
        
        public uint Id => m_internalOperation.Id;

        public AsyncStatus Status => m_internalOperation.Status;

        public void Cancel() => m_internalOperation.Cancel();

        public void Close() => m_internalOperation.Close();

        public uint GetResults() => m_internalOperation.GetResults();
    }
}

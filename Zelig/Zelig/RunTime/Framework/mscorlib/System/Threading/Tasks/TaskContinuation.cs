using System;

namespace System.Threading.Tasks
{
    // Task type used to implement: Task ContinueWith(Action<Task>,...>)
    internal sealed class ContinuationTaskFromTask : Task
    {
        private Task m_antecedant;

        public ContinuationTaskFromTask(
            Task antecedent,
            Delegate action,
            object state,
            TaskCreationOptions creationOptions) :
                base(action, state, creationOptions, InternalTaskOptions.ContinuationTask)
        {
        }

        internal override void Invoke()
        {
            // Get and null out the antecedent. This is crucial to avoid a memory leak with long chains of continuations.
            Task antecedent = m_antecedant;
            m_antecedant = null;

#if ENABLE_CONTRACTS
            Contract.Assert(antecedent != null, "No antecedent was set for the task continuation.");
#endif // ENABLE_CONTRACTS

            var action = m_action as Action<Task>;
            if (action != null)
            {
                action(antecedent);
                return;
            }

            var actionWithState = m_action as Action<Task, object>;
            if (actionWithState != null)
            {
                actionWithState(antecedent, m_stateObject);
                return;
            }

#if ENABLE_CONTRACTS
            Contract.Assert(false, "Invalid m_action in ContinuationTaskFromTask.");
#endif // ENABLE_CONTRACTS
        }
    }

    // Task type used to implement: Task ContinueWith(Action<Task<TAntecedentResult>,...>)
    internal sealed class ContinuationTaskFromResultTask<TAntecedentResult> : Task
    {
        private Task<TAntecedentResult> m_antecedant;

        public ContinuationTaskFromResultTask(
            Task<TAntecedentResult> antecedent,
            Delegate action,
            object state,
            TaskCreationOptions creationOptions) :
                base(action, state, creationOptions, InternalTaskOptions.ContinuationTask)
        {
        }

        internal override void Invoke()
        {
            // Get and null out the antecedent. This is crucial to avoid a memory leak with long chains of continuations.
            Task<TAntecedentResult> antecedent = m_antecedant;
            m_antecedant = null;

#if ENABLE_CONTRACTS
            Contract.Assert(antecedent != null, "No antecedent was set for the task continuation.");
#endif // ENABLE_CONTRACTS

            var action = m_action as Action<Task<TAntecedentResult>>;
            if (action != null)
            {
                action(antecedent);
                return;
            }

            var actionWithState = m_action as Action<Task<TAntecedentResult>, object>;
            if (actionWithState != null)
            {
                actionWithState(antecedent, m_stateObject);
                return;
            }

#if ENABLE_CONTRACTS
            Contract.Assert(false, "Invalid m_action in ContinuationTaskFromResultTask");
#endif // ENABLE_CONTRACTS
        }
    }

    // Task type used to implement: Task<TResult> ContinueWith(Func<Task<TAntecedentResult>,...>)
    internal sealed class ContinuationResultTaskFromTask<TResult> : Task<TResult>
    {
        private Task m_antecedant;

        public ContinuationResultTaskFromTask(
            Task antecedent,
            Delegate function,
            object state,
            TaskCreationOptions creationOptions) :
                base(function, state, creationOptions, InternalTaskOptions.ContinuationTask)
        {
        }

        internal override void Invoke()
        {
            // Get and null out the antecedent. This is crucial to avoid a memory leak with long chains of continuations.
            Task antecedent = m_antecedant;
            m_antecedant = null;

#if ENABLE_CONTRACTS
            Contract.Assert(antecedent != null, "No antecedent was set for the task continuation.");
#endif // ENABLE_CONTRACTS

            var func = m_action as Func<Task, TResult>;
            if (func != null)
            {
                m_result = func(antecedent);
                return;
            }

            var funcWithState = m_action as Func<Task, object, TResult>;
            if (funcWithState != null)
            {
                m_result = funcWithState(antecedent, m_stateObject);
                return;
            }

#if ENABLE_CONTRACTS
            Contract.Assert(false, "Invalid m_action in ContinuationResultTaskFromResultTask");
#endif // ENABLE_CONTRACTS
        }
    }

    // Task type used to implement: Task<TResult> ContinueWith(Func<Task<TAntecedentResult>,...>)
    internal sealed class ContinuationResultTaskFromResultTask<TAntecedentResult, TResult> : Task<TResult>
    {
        private Task<TAntecedentResult> m_antecedant;

        public ContinuationResultTaskFromResultTask(
            Task<TAntecedentResult> antecedent,
            Delegate function,
            object state,
            TaskCreationOptions creationOptions) :
                base(function, state, creationOptions, InternalTaskOptions.ContinuationTask)
        {
        }

        internal override void Invoke()
        {
            // Get and null out the antecedent. This is crucial to avoid a memory leak with long chains of continuations.
            Task<TAntecedentResult> antecedent = m_antecedant;
            m_antecedant = null;

#if ENABLE_CONTRACTS
            Contract.Assert(antecedent != null, "No antecedent was set for the task continuation.");
#endif // ENABLE_CONTRACTS

            var func = m_action as Func<Task<TAntecedentResult>, TResult>;
            if (func != null)
            {
                m_result = func(antecedent);
                return;
            }

            var funcWithState = m_action as Func<Task<TAntecedentResult>, object, TResult>;
            if (funcWithState != null)
            {
                m_result = funcWithState(antecedent, m_stateObject);
                return;
            }

#if ENABLE_CONTRACTS
            Contract.Assert(false, "Invalid m_action in ContinuationResultTaskFromResultTask");
#endif // ENABLE_CONTRACTS
        }
    }

    /// <summary>
    /// Wrapper class to simplify invocation of continuation tasks.
    /// </summary>
    internal class TaskContinuation
    {
        private readonly Task m_task;
        private readonly TaskContinuationOptions m_continuationOptions;

        public TaskContinuation(Task continuationTask, TaskContinuationOptions continuationOptions)
        {
            m_task = continuationTask;
            m_continuationOptions = continuationOptions;
        }

        public void RunOrSchedule(Task antecedent)
        {
            TaskStatus status = antecedent.Status;
            switch (status)
            {
            case TaskStatus.RanToCompletion:
                if ((m_continuationOptions & TaskContinuationOptions.NotOnRanToCompletion) != 0)
                {
                    // TODO: ENABLE_CANCELLATION: Cancel the task.
                    return;
                }
                break;

            case TaskStatus.Canceled:
                if ((m_continuationOptions & TaskContinuationOptions.NotOnCanceled) != 0)
                {
                    // TODO: ENABLE_CANCELLATION: Cancel the task.
                    return;
                }
                break;

            case TaskStatus.Faulted:
                if ((m_continuationOptions & TaskContinuationOptions.NotOnFaulted) != 0)
                {
                    // TODO: ENABLE_CANCELLATION: Cancel the task.
                    return;
                }
                break;
            }

            bool runSynchronously =
                ((m_continuationOptions & TaskContinuationOptions.ExecuteSynchronously) != 0) &&
                ((antecedent.CreationOptions & TaskCreationOptions.RunContinuationsAsynchronously) == 0);
            m_task.Start(runSynchronously);
        }
    }
}

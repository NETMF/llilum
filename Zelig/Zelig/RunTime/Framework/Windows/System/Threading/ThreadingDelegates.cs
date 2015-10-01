//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using Windows.Foundation;

namespace Windows.System.Threading
{
    /// <summary>
    /// Represents a method that is called when a timer created with CreateTimer or CreatePeriodicTimer is completed.
    /// </summary>
    /// <param name="timer">The timer to associate with this method.</param>
    public delegate void TimerDestroyedHandler(ThreadPoolTimer timer);

    /// <summary>
    /// Represents a method that is called when a timer created with CreateTimer or CreatePeriodicTimer expires.
    /// </summary>
    /// <param name="timer">The timer to associate with this method. When this timer expires, the method is called.</param>
    /// <remarks>
    /// When a timer is canceled, pending TimerElapsedHandler delegates are also canceled. TimerElapsedHandler delegates
    /// that are already running are allowed to finish.
    /// </remarks>
    public delegate void TimerElapsedHandler(ThreadPoolTimer timer);

    /// <summary>
    /// Represents a method that is called when a work item runs.
    /// </summary>
    /// <param name="operation">The work item to associate with the callback method.</param>
    /// <remarks>
    /// The thread pool calls a work item's WorkItemHandler delegate when a thread becomes available to run the work
    /// item. If a work item is canceled, WorkItemHandler delegates that have not yet started running are not called.
    /// WorkItemHandler delegates that are already running are allowed to finish unless the application stops them. If a
    /// work item might run for a relatively long time, the application should check if cancellation has been requested
    /// and stop the handler in an orderly way.
    /// </remarks>
    public delegate void WorkItemHandler(IAsyncAction operation);
}

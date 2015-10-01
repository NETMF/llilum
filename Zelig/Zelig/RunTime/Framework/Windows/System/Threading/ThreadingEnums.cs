//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.System.Threading
{
    /// <summary>
    /// Specifies how work items should be run.
    /// </summary>
    public enum WorkItemOptions
    {
        /// <summary>
        /// The work item should be run when the thread pool has an available worker thread.
        /// </summary>
        None = 0,

        /// <summary>
        /// The work items should be run simultaneously with other work items sharing a processor.
        /// </summary>
        TimeSliced,
    }

    /// <summary>
    /// Specifies the priority of a work item relative to other work items in the thread pool.
    /// </summary>
    public enum WorkItemPriority
    {
        /// <summary>
        /// The work item should run at normal priority. This is the default value.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The work item should run at low priority.
        /// </summary>
        Low = -1,

        /// <summary>
        /// The work item should run at high priority.
        /// </summary>
        High = 1,
    }
}

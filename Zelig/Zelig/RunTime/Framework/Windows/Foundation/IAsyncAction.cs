//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.Foundation
{
    /// <summary>
    /// Represents an asynchronous action. This is the return type for many Windows Runtime asynchronous methods that
    /// don't have a result object, and don't report ongoing progress.
    /// </summary>
    public interface IAsyncAction : IAsyncInfo
    {
        /// <summary>
        /// Returns the results of the action.
        /// </summary>
        /// <remarks>
        /// <para>The interface definition of this method has a void return, and void is what methods that use the
        ///     default IAsyncAction behavior will return after completing, when an awaitable syntax is used.</para>
        /// <para>If you want the method to return a result you probably should be using IAsyncOperation{TResult}
        ///     instead. For IAsyncAction, any added logic should be in the Completed implementation, not GetResults.
        /// </remarks>
        void GetResults();

        /// <summary>
        /// Gets or sets the method that handles the action completed notification.
        /// </summary>
        /// <value>The method that handles the notification.</value>
        /// <remarks>
        /// <para>The Windows Runtime enforces that this property can only be set once on an action.</para>
        /// <para>Generally, a completed IAsyncAction method called using language-specific awaitable syntax does
        ///     nothing further than to return null when it completes.</para>
        /// <para>If you're implementing IAsyncAction, then the set implementation of Completed should store the
        ///     handler, and the surrounding logic should invoke it when Close is called. The implementation should set
        ///     the asyncStatus parameter of invoked callbacks appropriately if there is a Cancel call, Status is not
        ///     Completed, errors occurred, and so on.</para>
        /// </remarks>
        AsyncActionCompletedHandler Completed
        {
            get;
            set;
        }
    }
}

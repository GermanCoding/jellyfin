#pragma warning disable CS1591

using System;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Querying;

namespace MediaBrowser.Model.Activity
{
    public interface IActivityManager
    {
        event EventHandler<GenericEventArgs<ActivityLogEntry>> EntryCreated;

        void Create(ActivityLog entry);

        Task CreateAsync(ActivityLog entry);

        /// <summary>
        /// Remove all activity logs before the specified date.
        /// </summary>
        /// <param name="startDate">Activity log start date.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CleanAsync(DateTime startDate);

        QueryResult<ActivityLogEntry> GetPagedResult(int? startIndex, int? limit);

        QueryResult<ActivityLogEntry> GetPagedResult(
            Func<IQueryable<ActivityLog>, IQueryable<ActivityLog>> func,
            int? startIndex,
            int? limit);
    }
}

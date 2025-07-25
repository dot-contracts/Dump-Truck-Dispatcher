using System.Collections.Generic;
using System.Linq;
using DispatcherWeb.SyncRequests.Dto;
using DispatcherWeb.SyncRequests.Entities;

namespace DispatcherWeb.SyncRequests
{
    public class SyncRequest
    {
        public SyncRequest()
        {
        }

        public SyncRequest UpdateChangesFromReferences()
        {
            foreach (var change in Changes.OfType<ISyncRequestChangeDetail>())
            {
                if (change.Entity is IChangedDriverAppEntity changedDriverAppEntity)
                {
                    changedDriverAppEntity.UpdateFromEntityReference();
                }
            }

            return this;
        }

        public List<SyncRequestChangeDetailAbstract> Changes { get; set; } = new List<SyncRequestChangeDetailAbstract>();

        //[JsonIgnore]
        public bool IgnoreForCurrentUser { get; set; }

        //[JsonIgnore]
        public int? IgnoreForDeviceId { get; set; }

        //[JsonIgnore]
        public bool SuppressTenantFilter { get; set; }

        //[JsonIgnore]
        public string LogMessage { get; set; }

        public SyncRequest AddChange<TEntity>(EntityEnum entityType, TEntity changedEntity, ChangeType changeType = ChangeType.Modified) where TEntity : ChangedEntityAbstract
        {
            if (!Changes.OfType<SyncRequestChangeDetail<TEntity>>().Any(x => x.Entity.IsSame(changedEntity)))
            {
                Changes.Add(new SyncRequestChangeDetail<TEntity>(entityType, changedEntity, changeType));
            }
            return this;
        }

        //public SyncRequest AddChange<TKey>(EntityEnum entity, TKey id, ChangeType changeType = ChangeType.Modified)
        //{
        //    Changes.Add(new SyncRequestChangeDetail<TKey>(entity, id, changeType));
        //    return this;
        //}

        public SyncRequest AddChanges<TEntity>(EntityEnum entityType, IEnumerable<TEntity> changedEntities, ChangeType changeType = ChangeType.Modified) where TEntity : ChangedEntityAbstract
        {
            foreach (var changedEntity in changedEntities.ToList())
            {
                if (!Changes.OfType<SyncRequestChangeDetail<TEntity>>().Any(x => x.Entity.IsSame(changedEntity)))
                {
                    Changes.Add(new SyncRequestChangeDetail<TEntity>(entityType, changedEntity, changeType));
                }
            }
            return this;
        }

        //public SyncRequest AddChangesById<TKey>(EntityEnum entity, IEnumerable<TKey> ids, ChangeType changeType = ChangeType.Modified)
        //{
        //    foreach (var id in ids.Distinct().ToList())
        //    {
        //        Changes.Add(new SyncRequestChangeDetail<TKey>(entity, id, changeType));
        //    }
        //    return this;
        //}

        //public SyncRequest AddChangeById(EntityEnum entity, int id, ChangeType changeType = ChangeType.Modified)
        //{
        //    Changes.Add(new SyncRequestChangeDetail<int>(entity, id, changeType));
        //    return this;
        //}

        //public SyncRequest AddChangesById(EntityEnum entity, IEnumerable<int> ids, ChangeType changeType = ChangeType.Modified)
        //{
        //    return AddChangesById<int>(entity, ids, changeType);
        //}

        //public SyncRequest AddChange(SyncRequestChangeDetailAbstract change)
        //{
        //    Changes.Add(change);
        //    return this;
        //}

        public bool HasChangesWithEntityType<TEntity>() where TEntity : ChangedEntityAbstract
        {
            return Changes.OfType<ISyncRequestChangeDetail>().Any(x => x.Entity is TEntity);
        }

        public List<TEntity> GetChangedEntitiesOfType<TEntity>() where TEntity : ChangedEntityAbstract
        {
            return Changes
                .OfType<ISyncRequestChangeDetail>()
                .Where(x => x.Entity is TEntity)
                .Select(x => (TEntity)x.Entity)
                .ToList();
        }

        public List<IChangedDriverAppEntity> GetDriverRelatedChanges()
        {
            return Changes
                .OfType<ISyncRequestChangeDetail>()
                .Where(x => x.Entity is IChangedDriverAppEntity)
                .Select(x => (IChangedDriverAppEntity)x.Entity)
                .ToList();
        }

        public List<int> GetDriverIds()
        {
            var driverRelatedChanges = GetDriverRelatedChanges();

            var driverIds = driverRelatedChanges.Select(x => x.DriverId)
                .Union(driverRelatedChanges.Select(x => x.OldDriverIdToNotify))
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Union(driverRelatedChanges.SelectMany(x => x.DriverIds ?? new List<int>()))
                .Distinct()
                .ToList();

            return driverIds;
        }

        public List<long> GetUserIds()
        {
            var driverRelatedChanges = GetDriverRelatedChanges();

            var userIds = driverRelatedChanges.Select(x => x.UserId)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            return userIds;
        }

        /// <summary>
        /// Set this flag to send the sync request to everyone except the user that made the change
        /// </summary>
        public SyncRequest SetIgnoreForCurrentUser(bool val)
        {
            IgnoreForCurrentUser = val;
            return this;
        }

        public SyncRequest SetIgnoreForDeviceId(int? val)
        {
            IgnoreForDeviceId = val;
            return this;
        }

        public SyncRequest SetSuppressTenantFilter(bool val)
        {
            SuppressTenantFilter = val;
            return this;
        }

        public SyncRequest AddLogMessage(string val)
        {
            if (!string.IsNullOrEmpty(LogMessage))
            {
                LogMessage += "\n";
            }
            LogMessage += val;
            return this;
        }

        public SyncRequestDto ToDto()
        {
            return new SyncRequestDto
            {
                Changes = Changes.ToList(),
            };
        }
    }
}

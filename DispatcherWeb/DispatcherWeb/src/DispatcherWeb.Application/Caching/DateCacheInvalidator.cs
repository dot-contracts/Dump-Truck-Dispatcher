using System;
using System.Collections.Generic;
using Abp.Dependency;
using Abp.Domain.Uow;
using DispatcherWeb.Drivers;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Caching
{
    public class DateCacheInvalidator : ISingletonDependency
    {
        private readonly ListCacheCollection _listCaches;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DateCacheInvalidator(
            ListCacheCollection listCaches,
            IUnitOfWorkManager unitOfWorkManager
        )
        {
            _listCaches = listCaches;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public void ChangeDateOrShift(Order order, DateTime newDate, Shift? newShift)
        {
            var oldDate = order.DeliveryDate;
            var oldShift = order.Shift;
            if (oldDate != newDate
                || oldShift != newShift)
            {
                InvalidateIfNeeded(order.Id, order.TenantId, oldDate, newDate, oldShift, newShift);
                InvalidateOrderIfNeeded(order);

                order.DeliveryDate = newDate;
                order.Shift = newShift;
            }
        }

        public void ChangeDateOrShift(Order order, DateTime newDate)
        {
            ChangeDateOrShift(order, newDate, order.Shift);
        }

        public void ChangeDateOrShift(Order order, Shift? newShift)
        {
            ChangeDateOrShift(order, order.DeliveryDate, newShift);
        }

        public void ChangeDateOrShift(DriverAssignment driverAssignment, DateTime newDate, Shift? newShift)
        {
            var oldDate = driverAssignment.Date;
            var oldShift = driverAssignment.Shift;
            if (oldDate != newDate
                || oldShift != newShift)
            {
                InvalidateIfNeeded(driverAssignment.Id, driverAssignment.TenantId, oldDate, newDate, oldShift, newShift);

                driverAssignment.Date = newDate;
                driverAssignment.Shift = newShift;
            }
        }

        public void ChangeDateOrShift(DriverAssignment order, DateTime newDate)
        {
            ChangeDateOrShift(order, newDate, order.Shift);
        }

        public void ChangeDateOrShift(DriverAssignment order, Shift? newShift)
        {
            ChangeDateOrShift(order, order.Date, newShift);
        }

        public void InvalidateIfNeeded(int id, int tenantId, DateTime? oldDate, DateTime? newDate, Shift? oldShift, Shift? newShift)
        {
            if (id > 0)
            {
                var affectedKeys = new List<ListCacheDateKey>();
                if (oldDate.HasValue)
                {
                    affectedKeys.Add(new ListCacheDateKey(tenantId, oldDate.Value, oldShift));
                    if (oldShift != null)
                    {
                        affectedKeys.Add(new ListCacheDateKey(tenantId, oldDate.Value, null));
                    }
                }
                if (newDate.HasValue)
                {
                    affectedKeys.Add(new ListCacheDateKey(tenantId, newDate.Value, newShift));
                    if (newShift != null)
                    {
                        affectedKeys.Add(new ListCacheDateKey(tenantId, newDate.Value, null));
                    }
                }

                if (affectedKeys.Count > 0)
                {
                    if (_unitOfWorkManager.Current == null)
                    {
                        throw new InvalidOperationException("Unit of Work must be active to change date or shift.");
                    }

                    _unitOfWorkManager.Current.Completed += (sender, args) =>
                    {
                        foreach (var cache in _listCaches.AllDateCaches)
                        {
                            foreach (var key in affectedKeys)
                            {
                                cache.HardInvalidateCache(key);
                            }
                        }
                    };
                }
            }
        }

        public void InvalidateOrderIfNeeded(Order order)
        {
            if (order.Id > 0)
            {
                if (_unitOfWorkManager.Current == null)
                {
                    throw new InvalidOperationException("Unit of Work must be active to invalidate order cache.");
                }
                _unitOfWorkManager.Current.Completed += async (sender, args) =>
                {
                    await _listCaches.DateKeyLookup.InvalidateKeyForOrder(order);
                };
            }
        }
    }
}

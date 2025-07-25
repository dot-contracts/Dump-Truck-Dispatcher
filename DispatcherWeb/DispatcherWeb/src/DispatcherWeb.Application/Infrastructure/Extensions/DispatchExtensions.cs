using System.Linq;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DispatchExtensions
    {
        public static IQueryable<RawDispatchDto> ToRawDispatchDto(this IQueryable<Dispatch> query)
        {
            return DispatchingAppService.ToRawDispatchDto(query);
        }

    }
}

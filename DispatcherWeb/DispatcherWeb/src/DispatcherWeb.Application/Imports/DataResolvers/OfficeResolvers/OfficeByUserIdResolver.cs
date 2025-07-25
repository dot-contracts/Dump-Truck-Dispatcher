using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Authorization.Users;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public class OfficeByUserIdResolver : OfficeResolverBase, ITransientDependency, IOfficeResolver
    {
        private readonly UserManager _userManager;

        public OfficeByUserIdResolver(
            UserManager userManager
        )
        {
            _userManager = userManager;
        }

        protected override async Task<Dictionary<string, int>> GetOfficeStringValueIdDictionaryAsync()
        {
            return _officeStringValueIdDictionary = await (await _userManager.GetQueryAsync())
                .Where(u => u.OfficeId.HasValue)
                .Select(u => new { UserId = u.Id, OfficeId = u.OfficeId.Value })
                .ToDictionaryAsync(o => o.UserId.ToString(), o => o.OfficeId);
        }
    }
}

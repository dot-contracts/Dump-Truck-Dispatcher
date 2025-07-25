using System.Threading.Tasks;
using Abp;
using Abp.Domain.Services;

namespace DispatcherWeb.Authorization.Delegation
{
    public interface IUserDelegationManager : IDomainService
    {
        Task<bool> HasActiveDelegationAsync(long sourceUserId, long targetUserId);

        Task RemoveDelegationAsync(long userDelegationId, UserIdentifier currentUser);

        Task<UserDelegation> GetAsync(long userDelegationId);
    }
}

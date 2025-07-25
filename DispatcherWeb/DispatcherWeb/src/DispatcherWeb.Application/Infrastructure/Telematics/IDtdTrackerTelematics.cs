using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface IDtdTrackerTelematics : ITruckTelematicsService
    {
        Task<DtdTrackerSettings> GetAccountDetailsFromAccessToken(string accessToken);
        Task<string> TestDtd();
        Task<WialonDeviceTypesResult> GetDeviceTypes();
        Task<List<UnitDto>> GetAllUnits(TokenLoginResult loginResult = null);
        Task<TokenLoginResult> LoginToApi(string accessToken = null);
        Task<WialonResult> LogoutFromApi(TokenLoginResult loginResult);
        Task CreateUnit(UnitDto unit, TokenLoginResult loginResult = null);
        Task DeleteItem(int itemId, TokenLoginResult loginResult = null);
        Task ImportMessages(int unitId, List<GpsMessageDto> messages, TokenLoginResult loginResult);
        Task<UnitDto> GetUnitByUniqueId(string uniqueId, TokenLoginResult loginResult);
        bool AreHostSettingsEmpty();
    }
}

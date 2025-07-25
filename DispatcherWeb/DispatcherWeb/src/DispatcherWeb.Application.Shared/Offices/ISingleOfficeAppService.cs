using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.Offices
{
    public interface ISingleOfficeAppService
    {
        Task<bool> IsSingleOffice();
        Task<KeyValuePair<int, string>> GetSingleOfficeIdName();
        Task Reset();
        Task FillSingleOffice(IOfficeIdNameDto officeIdNameDto);
    }
}
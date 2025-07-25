using System.Threading.Tasks;
using Abp.Dependency;

namespace DispatcherWeb.MultiTenancy.Accounting
{
    public interface IInvoiceNumberGenerator : ITransientDependency
    {
        Task<string> GetNewInvoiceNumber();
    }
}
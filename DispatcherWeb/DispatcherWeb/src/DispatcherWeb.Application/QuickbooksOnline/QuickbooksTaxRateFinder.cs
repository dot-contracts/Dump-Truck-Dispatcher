/*
using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.QueryFilter;

namespace DispatcherWeb.QuickbooksOnline
{
    public class QuickbooksTaxRateFinder
    {
        private readonly ServiceContext _serviceContext;
        private List<TaxCode> _taxCodes = null;
        private List<TaxRate> _taxRates = null;

        public QuickbooksTaxRateFinder(ServiceContext serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public string GetTaxCodeId(decimal taxRate)
        {
            if (_taxCodes == null)
            {
                _taxCodes = new QueryService<TaxCode>(_serviceContext).ExecuteIdsQuery("SELECT * FROM TaxCode where Active = true").ToList();
            }
            if (_taxRates == null)
            {
                _taxRates = new QueryService<TaxRate>(_serviceContext).ExecuteIdsQuery("SELECT * FROM TaxRate where Active = true").ToList();
            }

            var matchingTaxRates = _taxRates.Where(x => x.RateValueSpecified && x.RateValue == taxRate && x.SpecialTaxType == SpecialTaxTypeEnum.NONE).Select(x => x.Id).ToList();
            if (!matchingTaxRates.Any())
            {
                return null;
            }

            foreach (var taxCode in _taxCodes)
            {
                var detailList = taxCode.SalesTaxRateList.TaxRateDetail;
                if (detailList.Length != 1)
                {
                    continue;
                }
                if (!matchingTaxRates.Contains(detailList[0].TaxRateRef.Value))
                {
                    continue;
                }
                return taxCode.Id;
            }
            return null;
        }
    }
}
*/

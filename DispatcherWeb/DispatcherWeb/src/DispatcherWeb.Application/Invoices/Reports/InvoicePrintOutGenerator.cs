using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using DispatcherWeb.Configuration;
using DispatcherWeb.Invoices.Dto;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Invoices.Reports
{
    public class InvoicePrintOutGenerator : ITransientDependency
    {
        private readonly InvoicePrintOutGenerator1 _invoicePrintOutGenerator1;
        private readonly InvoicePrintOutGenerator2 _invoicePrintOutGenerator2;
        private readonly InvoicePrintOutGenerator3 _invoicePrintOutGenerator3;
        private readonly InvoicePrintOutGenerator4 _invoicePrintOutGenerator4;
        private readonly InvoicePrintOutGenerator5 _invoicePrintOutGenerator5;
        private readonly InvoicePrintOutGenerator6 _invoicePrintOutGenerator6;
        private readonly InvoicePrintOutGenerator7 _invoicePrintOutGenerator7;
        private readonly InvoicePrintOutGenerator8 _invoicePrintOutGenerator8;
        private readonly InvoicePrintOutGenerator9 _invoicePrintOutGenerator9;
        private readonly InvoicePrintOutGenerator10 _invoicePrintOutGenerator10;
        private readonly InvoicePrintOutGeneratorMinimalDescription _invoicePrintOutGeneratorMinimalDescription;
        private readonly ISettingManager _settingManager;

        public InvoicePrintOutGenerator(
            InvoicePrintOutGenerator1 invoicePrintOutGenerator1,
            InvoicePrintOutGenerator2 invoicePrintOutGenerator2,
            InvoicePrintOutGenerator3 invoicePrintOutGenerator3,
            InvoicePrintOutGenerator4 invoicePrintOutGenerator4,
            InvoicePrintOutGenerator5 invoicePrintOutGenerator5,
            InvoicePrintOutGenerator6 invoicePrintOutGenerator6,
            InvoicePrintOutGenerator7 invoicePrintOutGenerator7,
            InvoicePrintOutGenerator8 invoicePrintOutGenerator8,
            InvoicePrintOutGenerator9 invoicePrintOutGenerator9,
            InvoicePrintOutGenerator10 invoicePrintOutGenerator10,
            InvoicePrintOutGeneratorMinimalDescription invoicePrintOutGeneratorMinimalDescription,
            ISettingManager settingManager
        )
        {

            _invoicePrintOutGenerator1 = invoicePrintOutGenerator1;
            _invoicePrintOutGenerator2 = invoicePrintOutGenerator2;
            _invoicePrintOutGenerator3 = invoicePrintOutGenerator3;
            _invoicePrintOutGenerator4 = invoicePrintOutGenerator4;
            _invoicePrintOutGenerator5 = invoicePrintOutGenerator5;
            _invoicePrintOutGenerator6 = invoicePrintOutGenerator6;
            _invoicePrintOutGenerator7 = invoicePrintOutGenerator7;
            _invoicePrintOutGenerator8 = invoicePrintOutGenerator8;
            _invoicePrintOutGenerator9 = invoicePrintOutGenerator9;
            _invoicePrintOutGenerator10 = invoicePrintOutGenerator10;
            _invoicePrintOutGeneratorMinimalDescription = invoicePrintOutGeneratorMinimalDescription;
            _settingManager = settingManager;
        }

        public async Task<Document> GenerateReport(List<InvoicePrintOutDto> data)
        {
            var invoiceTemplate = (InvoiceTemplateEnum)await _settingManager.GetSettingValueAsync<int>(AppSettings.Invoice.InvoiceTemplate);
            switch (invoiceTemplate)
            {
                case InvoiceTemplateEnum.Invoice1:
                    return await _invoicePrintOutGenerator1.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice2:
                    return await _invoicePrintOutGenerator2.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice3:
                    return await _invoicePrintOutGenerator3.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice4:
                    return await _invoicePrintOutGenerator4.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice5:
                    return await _invoicePrintOutGenerator5.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice6:
                    return await _invoicePrintOutGenerator6.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice7:
                    return await _invoicePrintOutGenerator7.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice8:
                    return await _invoicePrintOutGenerator8.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice9:
                    return await _invoicePrintOutGenerator9.GenerateReport(data);

                case InvoiceTemplateEnum.Invoice10:
                    return await _invoicePrintOutGenerator10.GenerateReport(data);

                case InvoiceTemplateEnum.MinimalDescription:
                    return await _invoicePrintOutGeneratorMinimalDescription.GenerateReport(data);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}

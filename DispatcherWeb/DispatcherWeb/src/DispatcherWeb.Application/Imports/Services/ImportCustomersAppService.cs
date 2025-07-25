using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Customers;
using DispatcherWeb.Imports.RowReaders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public class ImportCustomersAppService : ImportDataBaseAppService<CustomerImportRow>, IImportCustomersAppService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerContact> _customerContactRepository;
        private HashSet<string> _existingCustomerNames;

        public ImportCustomersAppService(
            IRepository<Customer> customerRepository,
            IRepository<CustomerContact> customerContactRepository
        )
        {
            _customerRepository = customerRepository;
            _customerContactRepository = customerContactRepository;
        }

        protected override async Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            _existingCustomerNames = new HashSet<string>((await _customerRepository.GetQueryAsync()).Select(x => x.Name));

            return await base.CacheResourcesBeforeImportAsync(reader);
        }

        protected override async Task<bool> ImportRowAsync(CustomerImportRow row)
        {
            Customer customer;
            if (_existingCustomerNames.Contains(row.Name))
            {
                customer = (await _customerRepository.GetQueryAsync())
                    .Include(x => x.CustomerContacts)
                    .First(x => x.Name == row.Name);
            }
            else
            {
                customer = new Customer
                {
                    Name = row.Name,
                };
                await _customerRepository.InsertAsync(customer);
            }

            customer.IsActive = row.IsActive;
            customer.AccountNumber = row.AccountNumber;
            customer.IsCod = row.IsCod;
            customer.Address1 = row.Address1;
            customer.Address2 = row.Address2;
            customer.City = row.City;
            customer.State = row.State;
            customer.ZipCode = row.ZipCode;
            customer.CountryCode = row.CountryCode;
            customer.BillingAddress1 = row.BillingAddress1 ?? row.Address1;
            customer.BillingAddress2 = row.BillingAddress2 ?? row.Address2;
            customer.BillingCity = row.BillingCity ?? row.City;
            customer.BillingState = row.BillingState ?? row.State;
            customer.BillingZipCode = row.BillingZipCode ?? row.ZipCode;
            customer.BillingCountryCode = row.BillingCountryCode ?? row.CountryCode;
            customer.InvoiceEmail = row.InvoiceEmail;
            customer.Terms = ParseTerms(row.Terms);
            customer.IsInQuickBooks = true;

            if (!row.ContactName.IsNullOrEmpty() || !row.ContactPhone.IsNullOrEmpty())
            {
                var contactName = row.ContactName.IsNullOrEmpty() ? "Main contact" : row.ContactName;
                CustomerContact customerContact = null;
                if (customer.Id > 0)
                {
                    customerContact = customer.CustomerContacts?.FirstOrDefault(x => x.Name == contactName);
                }
                if (customerContact == null)
                {
                    customerContact = new CustomerContact
                    {
                        Customer = customer,
                        Name = contactName,
                    };
                    await _customerContactRepository.InsertAsync(customerContact);
                }
                customerContact.IsActive = true;
                customerContact.Email = row.ContactEmail;
                customerContact.Fax = row.ContactFax;
                customerContact.PhoneNumber = row.ContactPhone;
                customerContact.Title = row.ContactTitle;
            }

            if (!row.Contact2Name.IsNullOrEmpty() || !row.Contact2Phone.IsNullOrEmpty())
            {
                var contactName = row.Contact2Name.IsNullOrEmpty() ? "Alternative contact" : row.Contact2Name;
                CustomerContact customerContact = null;
                if (customer.Id > 0)
                {
                    customerContact = customer.CustomerContacts?.FirstOrDefault(x => x.Name == contactName);
                }
                if (customerContact == null)
                {
                    customerContact = new CustomerContact
                    {
                        Customer = customer,
                        Name = contactName,
                    };
                    await _customerContactRepository.InsertAsync(customerContact);
                }
                customerContact.IsActive = true;
                customerContact.PhoneNumber = row.Contact2Phone;
                customerContact.Title = row.Contact2Title;
                customerContact.Email = row.Contact2Email;
            }

            if (!_existingCustomerNames.Contains(row.Name))
            {
                _existingCustomerNames.Add(row.Name);
            }
            return true;
        }

        private BillingTermsEnum? ParseTerms(string terms)
        {
            switch (terms?.ToLower())
            {
                case "due on receipt": return BillingTermsEnum.DueOnReceipt;
                case "net 5": return BillingTermsEnum.Net5;
                case "net 10": return BillingTermsEnum.Net10;
                case "net 14": return BillingTermsEnum.Net14;
                case "net 15": return BillingTermsEnum.Net15;
                case "net 30": return BillingTermsEnum.Net30;
                case "net 60": return BillingTermsEnum.Net60;
                case "due by the first of the month": return BillingTermsEnum.DueByTheFirstOfTheMonth;
            }
            return null;
        }

        protected override bool IsRowEmpty(CustomerImportRow row)
        {
            return row.Name.IsNullOrEmpty() || row.Name.Contains(":");
        }
    }
}

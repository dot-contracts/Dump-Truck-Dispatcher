using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerContactEditDto : IValidatableObject
    {
        public int? Id { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(EntityStringFieldLengths.CustomerContact.Name)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string Fax { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string Email { get; set; }

        [StringLength(40)]
        public string Title { get; set; }

        public bool IsActive { get; set; }

        public bool HasCustomerPortalAccess { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var name = Name.Trim();
            if (HasCustomerPortalAccess
                && !string.IsNullOrEmpty(name)
                && name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                yield return new ValidationResult("This isn't a valid name for when contact is given access to customer portal. Please enter only the first and last name.");
            }
        }
    }
}

using System.Collections.Generic;
using DispatcherWeb.Dto;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public static class AccountTypes
    {
        public const string AccountsPayable = "AP";
        public const string AccountsReceivable = "AR";
        public const string CheckingOrSavings = "BANK";
        public const string CreditCardAccount = "CCARD";
        public const string CostOfGoodsSold = "COGS";
        /// <summary>
        /// Capital/Equity
        /// </summary>
        public const string Equity = "EQUITY";
        public const string OtherExpense = "EXEXP";
        public const string OtherIncome = "EXINC";
        public const string Expense = "EXP";
        public const string FixedAsset = "FIXASSET";
        public const string Income = "INC";
        public const string LongTermLiability = "LTLIAB";
        public const string NonPostingAccount = "NONPOSTING";
        public const string OtherAsset = "OASSET";
        public const string OtherCurrentAsset = "OCASSET";
        public const string OtherCurrentLiability = "OCLIAB";

        public static List<SelectListDto> GetIncomeTypesSelectList()
        {
            return new List<SelectListDto>
            {
                new SelectListDto { Id = Income, Name = "Income" },
                new SelectListDto { Id = CostOfGoodsSold, Name = "Cost of Goods Sold" },
                new SelectListDto { Id = OtherIncome, Name = "Other Income" },
            };
        }
    }
}

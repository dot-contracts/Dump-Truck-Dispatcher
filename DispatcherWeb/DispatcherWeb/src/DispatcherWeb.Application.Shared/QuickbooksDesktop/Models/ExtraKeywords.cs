namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public static class ExtraKeywords
    {
        public static class InvoiceLine
        {
            /// <summary>
            /// (Invoices, credit memos, and sales receipts only) Indicates that the distribution line is the last item of an invoice item group.
            /// </summary>
            public const string EndGroup = "ENDGRP";

            /// <summary>
            /// Identifies a sales tax item as the automatic tax rate you set up for your QuickBooks company.
            /// </summary>
            public const string AutoSalesTax = "AUTOSTAX";
        }
    }
}

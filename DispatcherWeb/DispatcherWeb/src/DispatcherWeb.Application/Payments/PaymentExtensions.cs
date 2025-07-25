using GlobalPayments.Api.Entities;

namespace DispatcherWeb.Payments
{
    public static class PaymentExtensions
    {
        public static Payment FillSummaryFieldsFrom(this Payment payment, TransactionSummary transaction)
        {
            payment.CardType ??= transaction.CardType;
            if (transaction.MaskedCardNumber?.Length >= 4)
            {
                payment.CardLast4 ??= transaction.MaskedCardNumber.Substring(transaction.MaskedCardNumber.Length - 4);
            }
            payment.TransactionType ??= transaction.TransactionType;
            payment.BatchSummaryId ??= transaction.BatchSequenceNumber;

            return payment;
        }

        public static Payment FillAuthorizationFieldsFrom(this Payment payment, TransactionSummary transaction)
        {
            payment.AuthorizationAmount = transaction.AuthorizedAmount;
            payment.AuthorizationCaptureDateTime = transaction.ResponseDate;
            payment.AuthorizationTransactionId = transaction.TransactionId;
            payment.PaymentDescription = transaction.Description;

            return payment;
        }

        public static Payment FillAuthorizationCaptureFieldsFrom(this Payment payment, TransactionSummary transaction)
        {
            payment.AuthorizationCaptureAmount = transaction.SettlementAmount;
            payment.AuthorizationCaptureTransactionId = transaction.TransactionId;
            payment.AuthorizationDateTime = transaction.ResponseDate;

            return payment;
        }
    }
}

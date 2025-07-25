namespace DispatcherWeb.PayStatements.Dto
{
    public class PayStatementReportTimeDto : PayStatementReportItemDto
    {
        public override PayStatementItemKind ItemKind
        {
            get => PayStatementItemKind.Time;
            set { }
        }
    }
}

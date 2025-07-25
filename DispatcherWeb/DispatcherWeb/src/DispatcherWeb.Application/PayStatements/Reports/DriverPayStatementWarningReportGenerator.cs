using System.Collections.Generic;
using System.Linq;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Dependency;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Localization;
using DispatcherWeb.PayStatements.Dto;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Orders.Reports
{
    public class DriverPayStatementWarningReportGenerator : ITransientDependency
    {
        public ISettingManager SettingManager { get; }
        public IFeatureChecker FeatureChecker { get; }
        public LocalizationHelper LocalizationHelper { get; }

        public DriverPayStatementWarningReportGenerator(
            ISettingManager settingManager,
            IFeatureChecker featureChecker,
            LocalizationHelper localizationHelper
            )
        {
            SettingManager = settingManager;
            FeatureChecker = featureChecker;
            LocalizationHelper = localizationHelper;
        }

        private string GetFilename(PayStatementWarningReportDto model)
        {
            return $"Warnings For Pay Statement Through {model.EndDate:yyyy-MM-dd}".SanitizeFilename();
        }

        public FileBytesDto GenerateReport(PayStatementWarningReportDto model, EntityDto input)
        {
            var result = new List<FileBytesDto>();
            Document document;
            Section section;
            Paragraph paragraph;

            void InitNewDocument()
            {
                document = new Document();
                section = document.AddSection();
                section.PageSetup = document.DefaultPageSetup.Clone();
                //section.PageSetup.Orientation = Orientation.Landscape;
                section.PageSetup.PageFormat = PageFormat.Letter;
                section.PageSetup.PageHeight = Unit.FromInch(11);
                section.PageSetup.PageWidth = Unit.FromInch(8.5);
                section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

                Style style = document.Styles[StyleNames.Normal];
                style.Font.Name = "Times New Roman";
                style.Font.Size = Unit.FromPoint(12);
                style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

                var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
                tableStyle.Font.Name = "Times New Roman";
                tableStyle.Font.Size = Unit.FromPoint(9);
                tableStyle.ParagraphFormat.SpaceAfter = 0;

                var headerStyle = document.Styles[StyleNames.Header];
                headerStyle.Font.Name = "Times New Roman";
                headerStyle.Font.Size = Unit.FromPoint(10);
                paragraph = new Paragraph();
                paragraph.AddText("Page ");
                paragraph.AddPageField();
                paragraph.AddText(" of ");
                paragraph.AddNumPagesField();
                section.Headers.Primary.Add(paragraph);
                section.Headers.EvenPage.Add(paragraph.Clone());
            }
            InitNewDocument();

            var filename = GetFilename(model);

            var productionPayTimeButNoTickets = model.DriverDateConflicts.Where(d => d.ConflictKind == DriverDateConflictKind.ProductionPayTimeButNoTickets);
            var bothProductionAndHourlyPay = model.DriverDateConflicts.Where(d => d.ConflictKind == DriverDateConflictKind.BothProductionAndHourlyPay);

            if (productionPayTimeButNoTickets.Any())
            {
                foreach (var conflict in productionPayTimeButNoTickets)
                {
                    paragraph = document.LastSection.AddParagraph(LocalizationHelper.L("{0}HasProductionPayTimeOn{1}ButNoTickets", conflict.DriverName, conflict.Date.ToString("d")));
                }

                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);
            }

            if (bothProductionAndHourlyPay.Any())
            {
                paragraph = document.LastSection.AddParagraph(LocalizationHelper.L("ListOfDriverTimeConflicts"));
                //paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);
                foreach (var conflict in bothProductionAndHourlyPay)
                {
                    paragraph = document.LastSection.AddParagraph(conflict.DriverName + " - " + conflict.Date.ToString("d"));
                }
            }
            return new FileBytesDto
            {
                FileBytes = document.SaveToBytesArray(),
                FileName = filename + ".pdf",
                MimeType = "application/pdf",
            };
        }
    }
}

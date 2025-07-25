using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DispatcherWeb.Infrastructure.Templates;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceTokens(this string template, Dictionary<string, string> tokenDictionary)
        {
            foreach (var keyValuePair in tokenDictionary)
            {
                template = template.Replace(keyValuePair.Key, keyValuePair.Value, true, CultureInfo.InvariantCulture);
            }

            return template;
        }

        public static string ReplaceTokensInTemplate(this string template, TemplateTokenDto tokenDto)
        {
            return template.ReplaceTokens(new Dictionary<string, string>
            {
                { TemplateTokens.DeliveryDate, tokenDto.DeliveryDate },
                { TemplateTokens.Shift, tokenDto.Shift },
                { TemplateTokens.OrderNumber, tokenDto.OrderNumber },
                { TemplateTokens.Customer, tokenDto.Customer },
                { TemplateTokens.Directions, tokenDto.Directions },
                { TemplateTokens.Comments, tokenDto.Directions },
                { TemplateTokens.Note, tokenDto.Note },
                //{ TemplateTokens.TimeOnJob, tokenDto.TimeOnJob },
                //{ TemplateTokens.StartTime, tokenDto.StartTime },
                { TemplateTokens.Item, tokenDto.Item },
                { TemplateTokens.Supplier, tokenDto.LoadAt },
                { TemplateTokens.LoadAt, tokenDto.LoadAt },
                { TemplateTokens.Quantity, tokenDto.Designation.HasMaterial() ? tokenDto.MaterialQuantity : tokenDto.FreightQuantity },
                { TemplateTokens.MaterialQuantity, tokenDto.MaterialQuantity },
                { TemplateTokens.FreightQuantity, tokenDto.FreightQuantity },
                { TemplateTokens.Uom, tokenDto.Designation.HasMaterial() ? tokenDto.MaterialUom : tokenDto.FreightUom },
                { TemplateTokens.MaterialUom, tokenDto.MaterialUom },
                { TemplateTokens.FreightUom, tokenDto.FreightUom },
                { TemplateTokens.UserFirstName, tokenDto.UserFirstName },
                { TemplateTokens.UserLastName, tokenDto.UserLastName },
                { TemplateTokens.UserPhoneNumber, tokenDto.UserPhoneNumber },
                { TemplateTokens.CompanyName, tokenDto.CompanyName },
                { TemplateTokens.DeliverTo, tokenDto.DeliverTo },
                { TemplateTokens.ChargeTo, tokenDto.ChargeTo },
            });
        }

        public static string SanitizeFilename(this string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return string.Empty;
            }

            return Regex.Replace(filename, @"[^a-zA-Z0-9_\-\.]", "");
        }
    }
}

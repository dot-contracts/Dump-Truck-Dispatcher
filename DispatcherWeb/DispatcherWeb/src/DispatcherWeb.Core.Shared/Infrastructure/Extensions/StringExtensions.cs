using Abp.Extensions;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value;
        }

        public static string TruncateWithPostfixAsFilename(this string value, int maxLength)
        {
            return value.TruncateWithPostfixAsFilename(maxLength, "...");
        }

        public static string TruncateWithPostfixAsFilename(this string value, int maxLength, string postfix)
        {

            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (maxLength == 0)
            {
                return string.Empty;
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            if (maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            var lastDotPosition = value.LastIndexOf('.');
            if (lastDotPosition <= 0)
            {
                return value.TruncateWithPostfix(maxLength, postfix);
            }

            var filename = value.Substring(0, lastDotPosition);
            var extension = value.Substring(lastDotPosition + 1);

            var newFilenameLength = maxLength - 1 - extension.Length;
            if (newFilenameLength <= 0)
            {
                return postfix;
            }

            filename = filename.TruncateWithPostfix(newFilenameLength, postfix);

            return $"{filename}.{extension}";
        }
    }
}

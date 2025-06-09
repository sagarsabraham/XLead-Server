using System.Text.RegularExpressions;

namespace XLead_Server.Helpers
{
    public class NormalizationHelper
    {
        public static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Removes all non-digit characters
            return Regex.Replace(phoneNumber, @"[^\d]", "");
        }

        public static string NormalizeWebsite(string website)
        {
            if (string.IsNullOrWhiteSpace(website))
                return string.Empty;

            // Simple normalization: lowercase and remove trailing slash
            var normalized = website.Trim().ToLowerInvariant();
            if (normalized.EndsWith("/"))
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }
            return normalized;
        }
    }
}

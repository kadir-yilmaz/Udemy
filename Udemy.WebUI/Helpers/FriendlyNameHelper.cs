using System.Text.RegularExpressions;

namespace Udemy.WebUI.Helpers
{
    public static class FriendlyNameHelper
    {
        public static string GetFriendlyTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";
            
            // 1. To Lower
            title = title.ToLowerInvariant();
            
            // 2. Turkish Characters
            title = title.Replace("ı", "i")
                         .Replace("ğ", "g")
                         .Replace("ü", "u")
                         .Replace("ş", "s")
                         .Replace("ö", "o")
                         .Replace("ç", "c")
                         .Replace(" ", "-");

            // 3. Remove Invalid Chars (Keep only a-z, 0-9, -)
            title = Regex.Replace(title, @"[^a-z0-9\-]", "");

            // 4. Remove duplicate hyphens
            title = Regex.Replace(title, @"\-{2,}", "-");
            
            // 5. Trim hyphens
            return title.Trim('-');
        }
    }
}

using System.Text.Json.Serialization;

namespace Udemy.WebUI.Models.PhotoStocks
{
    public class PhotoViewModel
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}

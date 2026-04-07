using System.ComponentModel.DataAnnotations;

namespace Udemy.WebUI.Models.Catalogs
{
    public class FeatureViewModel
    {
        [Display(Name = "Kurs süre")]
        public int Duration { get; set; }
    }
}

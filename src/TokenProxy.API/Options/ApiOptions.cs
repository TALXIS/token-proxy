using System.ComponentModel.DataAnnotations;

namespace TokenProxy.API.Options
{
    public class ApiOptions
    {
        /// <summary>
        /// Proxied API base url
        /// </summary>
        [Required]
        public string BaseUrl { get; set; }
    }
}

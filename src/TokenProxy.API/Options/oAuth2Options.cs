using System.ComponentModel.DataAnnotations;

namespace TokenProxy.API.Options
{
    public class oAuth2Options
    {
        /// <summary>
        /// Token endpoint. Usually ends with oauth/token
        /// </summary>
        [Required]
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// API Scopes, e.g. api.read api.write
        /// </summary>
        [Required]
        public string Scope { get; set; }
    }
}

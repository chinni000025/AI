using System.ComponentModel.DataAnnotations;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineConnectivity.DTOs
{
    public class DataBaseConfiguration
    {

        [Required]
        public DataBaseProvider DataBaseType { get; set; }

        [Required]
        public string Server { get; set; } = string.Empty;
        [Required]
        public int Port { get; set; }
        [Required]
        public string DatabaseName { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

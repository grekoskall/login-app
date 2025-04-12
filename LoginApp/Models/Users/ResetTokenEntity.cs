using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApp.Models
{
    [Table("RESET_TOKEN")]
    public class ResetTokenEntity : ModelBase
    {
        [Key]
        public string email { get; set; }
        public string token { get; set; }
        public string expiresAt { get; set; }
    }
}

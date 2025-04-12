using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApp.Models.Users
{
    [Table("USERS_SESSION")]
    public class UserSessionEntity : ModelBase
    {
        [Key]
        public string email { get; set; }
        public string sessionToken { get; set; }
        public string expirationDatetime { get; set; }
    }
}

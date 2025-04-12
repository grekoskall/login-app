using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LoginApp.Models.Users
{
    [Table("USERS_TEMP_TOKEN")]
    public class UserTempTokenEntity : ModelBase
    {
        [Key]
        public string email { get; set; }
        public string tempToken { get; set; }
        public string expirationDatetime { get; set; }
        public string codeFa { get; set; }
        public string expirationFa { get; set; }
    }
}

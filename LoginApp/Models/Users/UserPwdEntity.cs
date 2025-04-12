using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApp.Models.Users
{
    [Table("USERS_PWD")]
    public class UserPwdEntity : ModelBase
    {
        [Key]
        public string email { get; set; }
        public string hash { get; set; }
        public string salt { get; set; }
    }
}

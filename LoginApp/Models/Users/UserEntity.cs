using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginApp.Models.Users
{
    [Table("USERS")]
    public class UserEntity : ModelBase
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string telephone { get; set; }

        [Key]
        public string email { get; set; }
        public string photoPath { get; set; }

        [NotMapped]
        public string password { get; set; }
    }

    public class UpdateUserDTO
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string telephone { get; set; }
        public string photoPath { get; set; }
    }

    public class PasswordResetRequest
    {
        public string oldPassword { get; set; }
        public string password { get; set; }
    }
}

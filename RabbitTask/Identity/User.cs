using Microsoft.AspNetCore.Identity;

namespace RabbitTask.Identity
{
    public class User:IdentityUser
    {
        public string FirstName  { get; set; }
        public string LastName { get; set; }
    }
}

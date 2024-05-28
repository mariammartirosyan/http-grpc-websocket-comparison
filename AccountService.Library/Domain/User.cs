using System;
using Microsoft.AspNetCore.Identity;

namespace AccountService.Library.Domain
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}


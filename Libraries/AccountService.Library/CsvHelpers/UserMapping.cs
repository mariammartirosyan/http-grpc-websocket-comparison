using System;
using AccountService.Library.Domain;
using AccountService.Library.DTOs;
using CsvHelper.Configuration;

namespace AccountService.Library.CsvHelpers
{
	public class UserMapping : ClassMap<UserDTO>
    {
		public UserMapping()
		{
            Map(x => x.FirstName).Index(0);
            Map(x => x.LastName).Index(1);
            Map(x => x.Email).Index(2);
            Map(x => x.UserName).Index(3);
            Map(x => x.Password).Index(4);
            Map(x => x.Role).Index(5);

        }

	}
}


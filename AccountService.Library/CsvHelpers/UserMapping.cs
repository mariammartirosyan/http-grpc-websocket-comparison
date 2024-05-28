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
            Map(x => x.Id).Index(0);
            Map(x => x.FirstName).Index(1);
            Map(x => x.LastName).Index(2);
            Map(x => x.Email).Index(3);
            Map(x => x.UserName).Index(4);
            Map(x => x.Password).Index(5);
            Map(x => x.Role).Index(6);

        }

	}
}


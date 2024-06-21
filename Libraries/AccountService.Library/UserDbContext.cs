using System;
using AccountService.Library.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using AccountService.Library.Constants;
using AccountService.Library.CsvHelpers;
using AccountService.Library.DTOs;
using Microsoft.Extensions.Logging;

namespace AccountService.Library
{
    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
           
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var logger = _loggerFactory.CreateLogger<UserDbContext>();
        //    optionsBuilder.LogTo(log => logger.LogError(log));

        //    var dbConnectionString = "Server=127.0.0.1;Database=P2AccountDB;User ID=root;Password=pass;Port=3306";
        //    dbConnectionString = "Server=db;Database=TrailerStreamingSystemDB;User ID=root;Password=pass;Port=3306";
        //    optionsBuilder.UseMySql(dbConnectionString, new MySqlServerVersion(ServerVersion.AutoDetect(dbConnectionString)));
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // create roles
            var adminRoleId = "";
            var userRoleId = "";
            foreach (var role in Enum.GetValues(typeof(Roles)))
            {
                var identityRole = new IdentityRole
                {
                    Name = role.ToString(),
                    NormalizedName = role.ToString().ToUpper()
                };

                modelBuilder.Entity<IdentityRole>().HasData(identityRole);

                switch (role.ToString())
                {
                    case "Admin":
                        adminRoleId = identityRole.Id;
                        break;
                    case "User":
                        userRoleId = identityRole.Id;
                        break;

                }
            }

            // add users
            var usersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seed Data", "Users.csv");
            var usersData = CsvDataReader<UserDTO, UserMapping>.GetData(usersPath);

            foreach (var userDTO in usersData)
            {
                var user = new User
                {
                    UserName = userDTO.UserName,
                    NormalizedUserName = userDTO.UserName.ToUpper(),
                    Email = userDTO.Email,
                    NormalizedEmail = userDTO.Email.ToUpper(),
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    EmailConfirmed = true,
                    SecurityStamp = string.Empty
                };

                user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "pass");

                modelBuilder.Entity<User>().HasData(user);

                modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
                {
                    RoleId = userDTO.Role == "Admin" ? adminRoleId : userRoleId,
                    UserId = user.Id
                });
            }
        }

    }
}


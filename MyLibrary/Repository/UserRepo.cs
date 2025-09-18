using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyLibrary.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyLibrary.Repository
{
    public class UserRepo
    {
        private readonly IConfiguration _contex;

        public UserRepo(IConfiguration configuration)
        {
            _contex = configuration;
        }


        public IEnumerable<User> ShowUsers()
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_contex.GetConnectionString("SqlConnection")))
            {
                conn.Open();

                var sql = 
                    "SELECT Id, Username, Email FROM Users";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2)
                        });
                    }
                }
            }

            return users;
        }

    }
}

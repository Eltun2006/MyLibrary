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


        public List<User> ShowUsers()
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_contex.GetConnectionString("SqlConnection")))
            {
                conn.Open();

                var sql = @"
                            SELECT 
                                CAST(ROW_NUMBER() OVER (ORDER BY Id) AS INT) AS DisplayId,
                                Id,
                                Username,
                                LastName,
                                Email
                            FROM Users
                            ORDER BY Id
                        ";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            DisplayId = reader.GetInt32(0), // ekran üçün sıra
                            Id = reader.GetInt32(1),        // real database Id
                            Username = reader.GetString(2),
                            LastName = reader.GetString(3),
                            Email = reader.GetString(4)
                        });
                    }
                }
            }

            return users;
        }


        public List<User> DeleteUser(int id)
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_contex.GetConnectionString("SqlConnection")))
            {
                conn.Open();

                // 1️⃣ Useri sil
                using (var deleteCmd = new SqlCommand("DELETE FROM Users WHERE Id = @Id", conn))
                {
                    deleteCmd.Parameters.AddWithValue("@Id", id);
                    deleteCmd.ExecuteNonQuery();
                }

                // 2️⃣ Reseed et (növbəti ID max + 1 olsun)
                string reseedSql = @"
            DECLARE @maxId INT;
            SELECT @maxId = ISNULL(MAX(Id), 0) FROM Users;
            DBCC CHECKIDENT ('Users', RESEED, @maxId);
        ";
                using (var reseedCmd = new SqlCommand(reseedSql, conn))
                {
                    reseedCmd.ExecuteNonQuery();
                }

                // 3️⃣ Yenilənmiş userləri ardıcıl sıra ilə qaytar (ekran üçün)
                string selectSql = @"
            SELECT 
                CAST(ROW_NUMBER() OVER (ORDER BY Id) AS INT) AS DisplayId,
                Id,
                Username,
                LastName,
                Email
            FROM Users
            ORDER BY Id
        ";
                using (var selectCmd = new SqlCommand(selectSql, conn))
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            DisplayId = reader.GetInt32(0),   // ekran üçün ardıcıl sıra
                            Id = reader.GetInt32(1),          // həqiqi database Id
                            Username = reader.GetString(2),
                            LastName = reader.GetString(3),
                            Email = reader.GetString(4)
                        });
                    }
                }
            }

            return users;
        }

        
    }
}

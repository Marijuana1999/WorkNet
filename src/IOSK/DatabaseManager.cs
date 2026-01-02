using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace IOSK
{
    internal static class UserDatabaseManager
    {
        private static readonly string dbPath = "users.db";
        private static readonly string connectionString = $"Data Source={dbPath};Version=3;";

        public static void InitializeUserDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Family TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    Tell TEXT,
                    NationalCode TEXT,
                    Address TEXT,
                    CardNum TEXT,
                    Email TEXT
                );";

                using (var cmd = new SQLiteCommand(createUsersTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string insertDefaultUser = @"
                INSERT OR IGNORE INTO Users (Username, Family, Password)
                VALUES ('ali', 'AdminFamily', 'ali123');";

                using (var cmd = new SQLiteCommand(insertDefaultUser, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool AddUser(string username, string password, string family, string nationalCode, string tell, string cardNum, string address, string email)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                using (var checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@username", username);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0) return false;
                }

                string insertQuery = @"
                INSERT INTO Users (Username, Family, Password, Tell, NationalCode, Address, CardNum, Email)
                VALUES (@Username, @Family, @Password, @Tell, @NationalCode, @Address, @CardNum, @Email);";

                using (var insertCmd = new SQLiteCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@Username", username);
                    insertCmd.Parameters.AddWithValue("@Family", family);
                    insertCmd.Parameters.AddWithValue("@Password", password);
                    insertCmd.Parameters.AddWithValue("@Tell", tell);
                    insertCmd.Parameters.AddWithValue("@NationalCode", nationalCode);
                    insertCmd.Parameters.AddWithValue("@Address", address);
                    insertCmd.Parameters.AddWithValue("@CardNum", cardNum);
                    insertCmd.Parameters.AddWithValue("@Email", email);
                    insertCmd.ExecuteNonQuery();
                }
                try
                {
                    // اجرای INSERT
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error in register: " + ex.Message);
                    return false;
                }
                return true;
            }
        }
        public static class CurrentUser
        {
            public static int UserId { get; set; }
            public static string Username { get; set; }
        }

        public static Dictionary<string, string> GetUserDetails(int userId)
        {
            var userData = new Dictionary<string, string>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE UserID = @uid";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userData["Username"] = reader["Username"].ToString();
                            userData["Family"] = reader["Family"].ToString();
                            userData["Tell"] = reader["Tell"].ToString();
                            userData["Email"] = reader["Email"].ToString();
                            userData["NationalCode"] = reader["NationalCode"].ToString();
                            userData["Address"] = reader["Address"].ToString();
                            userData["CardNum"] = reader["CardNum"].ToString();
                        }
                    }
                }
            }

            return userData;
        }

        public static bool UpdateUserInfo(int userId, string username, string family, string tell, string email, string nationalCode, string address, string cardNum)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = @"
                UPDATE Users SET 
                    Username = @Username,
                    Family = @Family,
                    Tell = @Tell,
                    Email = @Email,
                    NationalCode = @NationalCode,
                    Address = @Address,
                    CardNum = @CardNum
                WHERE UserID = @UserID;";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Family", family);
                    cmd.Parameters.AddWithValue("@Tell", tell);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@NationalCode", nationalCode);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@CardNum", cardNum);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
        }

        public static bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE UserID = @UserID AND Password = @OldPassword";
                using (var checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    checkCmd.Parameters.AddWithValue("@OldPassword", oldPassword);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count == 0) return false;
                }

                string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE UserID = @UserID";
                using (var updateCmd = new SQLiteCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    updateCmd.Parameters.AddWithValue("@UserID", userId);
                    updateCmd.ExecuteNonQuery();
                }

                return true;
            }
        }

        public static bool CheckLogin(string username, string password)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT UserID FROM Users WHERE Username = @Username AND Password = @Password";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }
    }
}
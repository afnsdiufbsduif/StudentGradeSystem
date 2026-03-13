using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public static class Database
    {
        // Подставь свои данные: Server, Database, User, Password
        private static string connectionString = @"Server=localhost;Database=GradeeeSystem;Trusted_Connection=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Проверяет, существует ли пользователь с таким email
        public static bool UserExists(string email)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string query = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        // Проверяет логин и пароль и возвращает роль пользователя, если он найден
        public static string AuthenticateUser(string login, string password)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string query = "SELECT Role FROM Users WHERE Login = @Login AND (PasswordHash = @Password OR PasswordHash = @PasswordHash)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@PasswordHash", ComputeHash(password));

                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        private static string ComputeHash(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

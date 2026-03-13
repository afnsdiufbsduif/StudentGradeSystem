using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class AddUserAccountWindow : Window
    {
        public AddUserAccountWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Users (Login, PasswordHash, Role, StudentID, TeacherID) VALUES (@log, @pass, @role, @sid, @tid)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@log", tbLogin.Text);
                        cmd.Parameters.AddWithValue("@pass", tbPassword.Text); // Plain for now, hash in real world
                        cmd.Parameters.AddWithValue("@role", cbRole.Text);
                        cmd.Parameters.AddWithValue("@sid", string.IsNullOrWhiteSpace(tbStudentID.Text) ? DBNull.Value : (object)int.Parse(tbStudentID.Text));
                        cmd.Parameters.AddWithValue("@tid", string.IsNullOrWhiteSpace(tbTeacherID.Text) ? DBNull.Value : (object)int.Parse(tbTeacherID.Text));
                        cmd.ExecuteNonQuery();
                    }
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class AddGroupWindow : Window
    {
        public AddGroupWindow()
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
                    string query = "INSERT INTO Groups (GroupName, Faculty, Course) VALUES (@name, @faculty, @course)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", tbGroupName.Text);
                        cmd.Parameters.AddWithValue("@faculty", tbFaculty.Text);
                        cmd.Parameters.AddWithValue("@course", int.Parse(tbCourse.Text));
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
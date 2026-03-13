using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class AddEditTeacherWindow : Window
    {
        private int? _teacherId = null;

        public AddEditTeacherWindow(int? teacherId = null)
        {
            InitializeComponent();
            _teacherId = teacherId;

            if (_teacherId.HasValue)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT FirstName, LastName, Patronymic, Department, Email FROM Teachers WHERE TeacherID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _teacherId.Value);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tbFirstName.Text = reader["FirstName"].ToString();
                                tbLastName.Text = reader["LastName"].ToString();
                                tbPatronymic.Text = reader["Patronymic"].ToString();
                                tbDepartment.Text = reader["Department"].ToString();
                                tbEmail.Text = reader["Email"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query;
                    if (_teacherId.HasValue)
                    {
                        query = "UPDATE Teachers SET FirstName=@fn, LastName=@ln, Patronymic=@pat, Department=@dep, Email=@email WHERE TeacherID=@id";
                    }
                    else
                    {
                        query = "INSERT INTO Teachers (FirstName, LastName, Patronymic, Department, Email) VALUES (@fn, @ln, @pat, @dep, @email)";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fn", tbFirstName.Text);
                        cmd.Parameters.AddWithValue("@ln", tbLastName.Text);
                        cmd.Parameters.AddWithValue("@pat", tbPatronymic.Text);
                        cmd.Parameters.AddWithValue("@dep", tbDepartment.Text);
                        cmd.Parameters.AddWithValue("@email", tbEmail.Text);

                        if (_teacherId.HasValue)
                            cmd.Parameters.AddWithValue("@id", _teacherId.Value);

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
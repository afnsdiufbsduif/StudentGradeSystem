using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class AddEditStudentWindow : Window
    {
        private int? _studentId = null;

        public AddEditStudentWindow(int? studentId = null)
        {
            InitializeComponent();
            _studentId = studentId;

            if (_studentId.HasValue)
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
                    string query = "SELECT FirstName, LastName, Patronymic, Gender, GroupID, Email FROM Students WHERE StudentID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _studentId.Value);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tbFirstName.Text = reader["FirstName"].ToString();
                                tbLastName.Text = reader["LastName"].ToString();
                                tbPatronymic.Text = reader["Patronymic"].ToString();
                                cbGender.Text = reader["Gender"].ToString();
                                tbGroupID.Text = reader["GroupID"].ToString();
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
                    if (_studentId.HasValue)
                    {
                        query = "UPDATE Students SET FirstName=@fn, LastName=@ln, Patronymic=@pat, Gender=@gen, GroupID=@gid, Email=@email WHERE StudentID=@id";
                    }
                    else
                    {
                        query = "INSERT INTO Students (FirstName, LastName, Patronymic, Gender, GroupID, Email) VALUES (@fn, @ln, @pat, @gen, @gid, @email)";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fn", tbFirstName.Text);
                        cmd.Parameters.AddWithValue("@ln", tbLastName.Text);
                        cmd.Parameters.AddWithValue("@pat", tbPatronymic.Text);
                        cmd.Parameters.AddWithValue("@gen", cbGender.Text);
                        cmd.Parameters.AddWithValue("@gid", string.IsNullOrWhiteSpace(tbGroupID.Text) ? DBNull.Value : (object)int.Parse(tbGroupID.Text));
                        cmd.Parameters.AddWithValue("@email", tbEmail.Text);

                        if (_studentId.HasValue)
                            cmd.Parameters.AddWithValue("@id", _studentId.Value);

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
using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class EditGradeWindow : Window
    {
        private readonly int _gradeId;

        public EditGradeWindow(int gradeId)
        {
            InitializeComponent();
            _gradeId = gradeId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Grade, GradeDate FROM Grades WHERE GradeID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", _gradeId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tbGrade.Text = reader.GetInt32(0).ToString();
                                dpGradeDate.SelectedDate = reader.GetDateTime(1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE Grades SET Grade=@grade, GradeDate=@date WHERE GradeID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@grade", int.Parse(tbGrade.Text));
                        cmd.Parameters.AddWithValue("@date", dpGradeDate.SelectedDate ?? DateTime.Today);
                        cmd.Parameters.AddWithValue("@id", _gradeId);
                        cmd.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
    }
}

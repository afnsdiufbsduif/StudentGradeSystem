using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class AddGradeWindow : Window
    {
        private int _teacherId;

        public AddGradeWindow(int teacherId)
        {
            InitializeComponent();
            _teacherId = teacherId;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Grades (StudentID, SubjectID, TeacherID, Grade, GradeDate) VALUES (@sid, @subid, @tid, @grade, @date)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", int.Parse(tbStudentID.Text));
                        cmd.Parameters.AddWithValue("@subid", int.Parse(tbSubjectID.Text));
                        cmd.Parameters.AddWithValue("@tid", _teacherId);
                        cmd.Parameters.AddWithValue("@grade", int.Parse(tbGrade.Text));
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения оценки: " + ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
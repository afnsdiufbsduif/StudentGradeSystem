using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp3
{
    public partial class AssignTeacherWindow : Window
    {
        private ObservableCollection<TeacherItem> _teachers = new ObservableCollection<TeacherItem>();
        private ObservableCollection<SubjectItem> _subjects = new ObservableCollection<SubjectItem>();

        public AssignTeacherWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string queryTeachers = "SELECT TeacherID, LastName + ' ' + FirstName + ISNULL(' ' + Patronymic, '') FROM Teachers";
                    using (SqlCommand cmd = new SqlCommand(queryTeachers, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _teachers.Add(new TeacherItem { TeacherID = reader.GetInt32(0), FullName = reader.GetString(1).Trim() });
                        }
                    }

                    string querySubjects = "SELECT SubjectID, SubjectName FROM Subjects";
                    using (SqlCommand cmd = new SqlCommand(querySubjects, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _subjects.Add(new SubjectItem { SubjectID = reader.GetInt32(0), SubjectName = reader.GetString(1) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }

            cbTeachers.ItemsSource = _teachers;
            cbSubjects.ItemsSource = _subjects;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbTeachers.SelectedItem is not TeacherItem teacher || cbSubjects.SelectedItem is not SubjectItem subject)
            {
                MessageBox.Show("Выберите преподавателя и предмет.");
                return;
            }

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string insert = @"
                        INSERT INTO Reports (ReportName, ReportDate, CreatedBy, ReportContent)
                        VALUES (@name, @date, @createdBy, @content)";
                    using (SqlCommand cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", "Назначение преподавателя");
                        cmd.Parameters.AddWithValue("@date", DateTime.Today);
                        cmd.Parameters.AddWithValue("@createdBy", teacher.TeacherID);
                        cmd.Parameters.AddWithValue("@content", $"Назначен на предмет: {subject.SubjectName} (ID={subject.SubjectID})");
                        cmd.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class TeacherItem { public int TeacherID { get; set; } public string FullName { get; set; } }
        public class SubjectItem { public int SubjectID { get; set; } public string SubjectName { get; set; } }
    }
}

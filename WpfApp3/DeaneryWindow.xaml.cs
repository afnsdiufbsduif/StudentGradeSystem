using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace WpfApp3
{
    public partial class DeaneryWindow : Window
    {
        public ObservableCollection<StudentVM> StudentsList { get; set; }
        public ObservableCollection<TeacherVM> TeachersList { get; set; }
        public ObservableCollection<GroupVM> GroupsList { get; set; }
        public ObservableCollection<SubjectVM> SubjectsList { get; set; }
        public ObservableCollection<UserAccountsVM> UsersList { get; set; }

        public DeaneryWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadStudents();
            LoadTeachers();
            LoadGroups();
            LoadSubjects();
            LoadUsers();
        }


        private void LoadStudents()
        {
            StudentsList = new ObservableCollection<StudentVM>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT s.StudentID, 
                               s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.Patronymic, '') AS FullName, 
                               g.GroupName, 
                               g.Faculty, 
                               'Обучается' AS Status 
                        FROM Students s
                        LEFT JOIN Groups g ON s.GroupID = g.GroupID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StudentsList.Add(new StudentVM 
                            { 
                                StudentID = reader.GetInt32(0), 
                                FullName = reader.GetString(1).Trim(), 
                                GroupName = reader.IsDBNull(2) ? "" : reader.GetString(2), 
                                Speciality = reader.IsDBNull(3) ? "" : reader.GetString(3), 
                                Status = reader.GetString(4) 
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore exception if tables don't exist yet
            }
            dgStudents.ItemsSource = StudentsList;
        }

        private void LoadTeachers()
        {
            TeachersList = new ObservableCollection<TeacherVM>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT TeacherID, 
                               LastName + ' ' + FirstName + ' ' + ISNULL(Patronymic, '') AS FullName, 
                               Department,
                               Email 
                        FROM Teachers";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TeachersList.Add(new TeacherVM 
                            { 
                                TeacherID = reader.GetInt32(0), 
                                FullName = reader.GetString(1).Trim(), 
                                Department = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { }
            dgTeachers.ItemsSource = TeachersList;
        }

        private void LoadGroups()
        {
            GroupsList = new ObservableCollection<GroupVM>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT g.GroupName, 
                               g.Course, 
                               (SELECT COUNT(*) FROM Students s WHERE s.GroupID = g.GroupID) AS StudentCount
                        FROM Groups g";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GroupsList.Add(new GroupVM 
                            { 
                                GroupName = reader.IsDBNull(0) ? "" : reader.GetString(0), 
                                Course = reader.IsDBNull(1) ? 0 : reader.GetInt32(1), 
                                StudentCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { }
            dgGroups.ItemsSource = GroupsList;
        }

        private void LoadSubjects()
        {
            SubjectsList = new ObservableCollection<SubjectVM>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT SubjectName, CreditHours FROM Subjects";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SubjectsList.Add(new SubjectVM 
                            { 
                                SubjectName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                CreditHours = reader.IsDBNull(1) ? 0 : reader.GetInt32(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { }
            dgSubjects.ItemsSource = SubjectsList;
        }

        private void LoadUsers()
        {
            UsersList = new ObservableCollection<UserAccountsVM>();
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT Login, Role, 
                               COALESCE(CAST(StudentID AS VARCHAR), CAST(TeacherID AS VARCHAR), '-') AS RecordID
                        FROM Users";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UsersList.Add(new UserAccountsVM 
                            { 
                                Login = reader.IsDBNull(0) ? "" : reader.GetString(0), 
                                Role = reader.IsDBNull(1) ? "" : reader.GetString(1), 
                                RecordID = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { }
            dgUsers.ItemsSource = UsersList;
        }

        private void BtnAssignTeacher_Click(object sender, RoutedEventArgs e)
        {
            AssignTeacherWindow window = new AssignTeacherWindow();
            if (window.ShowDialog() == true)
            {
                LoadSubjects();
            }
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string query = GetDeaneryReportQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            ReportPreviewWindow window = new ReportPreviewWindow("Отчет деканата", query);
            window.ShowDialog();
        }

        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            string query = GetDeaneryReportQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            ReportGenerator.GenerateCsv(query, "deanery-report.csv");
        }

        private void BtnPrintReport_Click(object sender, RoutedEventArgs e)
        {
            string query = GetDeaneryReportQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            string tempFile = Path.Combine(Path.GetTempPath(), "deanery-report.csv");
            ReportGenerator.GenerateCsvToFile(query, tempFile);
            Process.Start(new ProcessStartInfo
            {
                FileName = tempFile,
                Verb = "print",
                UseShellExecute = true
            });
        }

        private string GetDeaneryReportQuery()
        {
            if (cbDeaneryReportType.SelectedItem is ComboBoxItem item)
            {
                string type = item.Content?.ToString() ?? string.Empty;

                if (type.Contains("успеваемости"))
                {
                    return @"
                        SELECT s.LastName + ' ' + s.FirstName AS Student,
                               sub.SubjectName AS Subject,
                               g.Grade,
                               g.GradeDate
                        FROM Grades g
                        JOIN Students s ON s.StudentID = g.StudentID
                        JOIN Subjects sub ON sub.SubjectID = g.SubjectID";
                }

                if (type.Contains("посещаемости"))
                {
                    return @"
                        SELECT s.LastName + ' ' + s.FirstName AS Student,
                               sub.SubjectName AS Subject,
                               a.Date,
                               a.Status
                        FROM Attendance a
                        JOIN Students s ON s.StudentID = a.StudentID
                        JOIN Subjects sub ON sub.SubjectID = a.SubjectID";
                }

                if (type.Contains("сессии"))
                {
                    return @"
                        SELECT s.LastName + ' ' + s.FirstName AS Student,
                               AVG(CAST(g.Grade AS FLOAT)) AS AvgGrade
                        FROM Grades g
                        JOIN Students s ON s.StudentID = g.StudentID
                        GROUP BY s.LastName, s.FirstName";
                }

                if (type.Contains("группе"))
                {
                    if (dgGroups.SelectedItem is GroupVM group)
                    {
                        return $@"
                            SELECT s.LastName + ' ' + s.FirstName AS Student,
                                   sub.SubjectName AS Subject,
                                   g.Grade,
                                   g.GradeDate
                            FROM Grades g
                            JOIN Students s ON s.StudentID = g.StudentID
                            JOIN Subjects sub ON sub.SubjectID = g.SubjectID
                            JOIN Groups gr ON gr.GroupID = s.GroupID
                            WHERE gr.GroupName = '{group.GroupName.Replace("'", "''")}'";
                    }

                    MessageBox.Show("Выберите группу в списке слева для отчета.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }

            MessageBox.Show("Выберите тип отчета.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        // --- ОБРАБОТЧИКИ СТУДЕНТОВ ---
        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            AddEditStudentWindow window = new AddEditStudentWindow();
            if (window.ShowDialog() == true)
            {
                LoadStudents();
                LoadGroups();
            }
        }

        private void BtnEditStudent_Click(object sender, RoutedEventArgs e)
        {
            if (dgStudents.SelectedItem is StudentVM selected)
            {
                AddEditStudentWindow window = new AddEditStudentWindow(selected.StudentID);
                if (window.ShowDialog() == true)
                {
                    LoadStudents();
                    LoadGroups();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (dgStudents.SelectedItem is StudentVM selected)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить студента {selected.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = Database.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM Students WHERE StudentID = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", selected.StudentID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        StudentsList.Remove(selected);
                        LoadGroups(); // Update students count in groups
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить студента (возможно, есть связанные оценки): {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TbSearchStudent_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearchStudent.Text == "Поиск по ФИО или Группе...")
            {
                tbSearchStudent.Text = "";
                tbSearchStudent.Foreground = Brushes.Black;
            }
        }

        private void TbSearchStudent_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSearchStudent.Text))
            {
                tbSearchStudent.Text = "Поиск по ФИО или Группе...";
                tbSearchStudent.Foreground = Brushes.Gray;
            }
        }

        private void BtnSearchStudent_Click(object sender, RoutedEventArgs e)
        {
            string query = tbSearchStudent.Text.ToLower();
            if (query != "поиск по фио или группе..." && !string.IsNullOrWhiteSpace(query))
            {
                var filtered = StudentsList.Where(s => s.FullName.ToLower().Contains(query) || s.GroupName.ToLower().Contains(query)).ToList();
                dgStudents.ItemsSource = filtered;
            }
            else
            {
                dgStudents.ItemsSource = StudentsList;
            }
        }

        // --- ОБРАБОТЧИКИ ПРЕПОДАВАТЕЛЕЙ ---
        private void BtnAddTeacher_Click(object sender, RoutedEventArgs e)
        {
            AddEditTeacherWindow window = new AddEditTeacherWindow();
            if (window.ShowDialog() == true)
            {
                LoadTeachers();
            }
        }

        private void BtnEditTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachers.SelectedItem is TeacherVM selected)
            {
                AddEditTeacherWindow window = new AddEditTeacherWindow(selected.TeacherID);
                if (window.ShowDialog() == true)
                {
                    LoadTeachers();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите преподавателя для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachers.SelectedItem is TeacherVM selected)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить преподавателя {selected.FullName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = Database.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM Teachers WHERE TeacherID = @id";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", selected.TeacherID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        TeachersList.Remove(selected);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить преподавателя (возможно, есть связанные оценки или отчеты): {ex.Message}", "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите преподавателя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            AddGroupWindow window = new AddGroupWindow();
            if (window.ShowDialog() == true)
            {
                LoadGroups();
            }
        }

        private void BtnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (dgGroups.SelectedItem is GroupVM selected)
            {
                var result = MessageBox.Show($"Удалить группу {selected.GroupName}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = Database.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM Groups WHERE GroupName = @name";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", selected.GroupName);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadGroups();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить группу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите группу для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            AddUserAccountWindow window = new AddUserAccountWindow();
            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is UserAccountsVM user)
            {
                var result = MessageBox.Show($"Сбросить пароль для {user.Login} на '1234'?", "Сброс пароля", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = Database.GetConnection())
                        {
                            conn.Open();
                            string query = "UPDATE Users SET PasswordHash = '1234' WHERE Login = @login";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@login", user.Login);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Пароль сброшен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось сбросить пароль: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAccessRights_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is UserAccountsVM user)
            {
                var result = MessageBox.Show($"Удалить аккаунт {user.Login}?", "Удаление", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = Database.GetConnection())
                        {
                            conn.Open();
                            string query = "DELETE FROM Users WHERE Login = @login";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@login", user.Login);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        // Модели представления для таблиц (ViewModels)
        public class StudentVM { public int StudentID { get; set; } public string FullName { get; set; } public string GroupName { get; set; } public string Speciality { get; set; } public string Status { get; set; } }
        public class TeacherVM { public int TeacherID { get; set; } public string FullName { get; set; } public string Department { get; set; } public string Email { get; set; } }
        public class GroupVM { public string GroupName { get; set; } public int Course { get; set; } public int StudentCount { get; set; } }
        public class SubjectVM { public string SubjectName { get; set; } public int CreditHours { get; set; } }
        public class UserAccountsVM { public string Login { get; set; } public string Role { get; set; } public string RecordID { get; set; } }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp3
{
    public partial class StudentWindow : Window
    {
        private int _studentId = 1; // Example logged-in student ID
        public ObservableCollection<GradeVM> GradesList { get; set; }
        public ObservableCollection<AttendanceVM> AttendanceList { get; set; }
        public ObservableCollection<SubjectItem> SubjectFilters { get; set; }

        public StudentWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadSubjects();
            EnsureCollections();
            LoadGrades();
            LoadAttendance();
            UpdateNotificationCount();
        }

        private void LoadSubjects()
        {
            SubjectFilters = new ObservableCollection<SubjectItem>
            {
                new SubjectItem { SubjectID = 0, SubjectName = "Все предметы" }
            };

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT DISTINCT sub.SubjectID, sub.SubjectName
                                    FROM Subjects sub
                                    LEFT JOIN Grades g ON g.SubjectID = sub.SubjectID AND g.StudentID = @sid
                                    LEFT JOIN Attendance a ON a.SubjectID = sub.SubjectID AND a.StudentID = @sid
                                    WHERE g.SubjectID IS NOT NULL OR a.SubjectID IS NOT NULL
                                    ORDER BY sub.SubjectName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", _studentId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SubjectFilters.Add(new SubjectItem
                                {
                                    SubjectID = reader.GetInt32(0),
                                    SubjectName = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметов: {ex.Message}", "Предметы", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            cbGradesSubject.ItemsSource = SubjectFilters;
            cbAttendanceSubject.ItemsSource = SubjectFilters;

            if (cbGradesSubject.Items.Count > 0) cbGradesSubject.SelectedIndex = 0;
            if (cbAttendanceSubject.Items.Count > 0) cbAttendanceSubject.SelectedIndex = 0;
        }

        private void EnsureCollections()
        {
            if (GradesList == null)
            {
                GradesList = new ObservableCollection<GradeVM>();
                dgGrades.ItemsSource = GradesList;
            }

            if (AttendanceList == null)
            {
                AttendanceList = new ObservableCollection<AttendanceVM>();
                dgAttendance.ItemsSource = AttendanceList;
            }
        }

        private void LoadGrades()
        {
            EnsureCollections();
            GradesList.Clear();

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string queryGrades = @"SELECT sub.SubjectName, t.LastName + ' ' + t.FirstName AS Teacher, g.Grade, g.GradeDate
                                           FROM Grades g 
                                           JOIN Subjects sub ON g.SubjectID = sub.SubjectID
                                           JOIN Teachers t ON g.TeacherID = t.TeacherID
                                           WHERE g.StudentID = @sid";

                    string semester = GetSelectedText(cbSemester);
                    if (semester.Contains("1 Семестр", StringComparison.OrdinalIgnoreCase))
                    {
                        queryGrades += " AND MONTH(g.GradeDate) BETWEEN 9 AND 12";
                    }
                    else if (semester.Contains("2 Семестр", StringComparison.OrdinalIgnoreCase))
                    {
                        queryGrades += " AND MONTH(g.GradeDate) BETWEEN 1 AND 6";
                    }

                    string subject = GetSelectedText(cbGradesSubject);
                    if (!string.IsNullOrWhiteSpace(subject) && !subject.Contains("Все", StringComparison.OrdinalIgnoreCase))
                    {
                        queryGrades += " AND sub.SubjectName = @subject";
                    }

                    queryGrades += " ORDER BY g.GradeDate DESC";

                    using (SqlCommand cmd = new SqlCommand(queryGrades, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", _studentId);
                        if (!string.IsNullOrWhiteSpace(subject) && !subject.Contains("Все", StringComparison.OrdinalIgnoreCase))
                        {
                            cmd.Parameters.AddWithValue("@subject", subject);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GradesList.Add(new GradeVM
                                {
                                    Subject = reader.GetString(0),
                                    Teacher = reader.GetString(1),
                                    GradeValue = reader.GetInt32(2).ToString(),
                                    Date = reader.GetDateTime(3).ToShortDateString(),
                                    WorkType = "Рубежный контроль",
                                    Comment = string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оценок: {ex.Message}", "Мои оценки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAttendance()
        {
            EnsureCollections();
            AttendanceList.Clear();

            int presentCount = 0;
            int absentCount = 0;

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string queryAtt = @"SELECT a.Date, sub.SubjectName, a.Status
                                        FROM Attendance a 
                                        JOIN Subjects sub ON a.SubjectID = sub.SubjectID
                                        WHERE a.StudentID = @sid";

                    string period = GetSelectedText(cbAttendancePeriod);
                    if (period.Contains("Этот месяц", StringComparison.OrdinalIgnoreCase))
                    {
                        queryAtt += " AND YEAR(a.Date) = @year AND MONTH(a.Date) = @month";
                    }
                    else if (period.Contains("Прошлый месяц", StringComparison.OrdinalIgnoreCase))
                    {
                        queryAtt += " AND YEAR(a.Date) = @year AND MONTH(a.Date) = @month";
                    }
                    else if (period.Contains("Весь семестр", StringComparison.OrdinalIgnoreCase))
                    {
                        queryAtt += " AND a.Date >= @fromDate";
                    }

                    string subject = GetSelectedText(cbAttendanceSubject);
                    if (!string.IsNullOrWhiteSpace(subject) && !subject.Contains("Все", StringComparison.OrdinalIgnoreCase))
                    {
                        queryAtt += " AND sub.SubjectName = @subject";
                    }

                    queryAtt += " ORDER BY a.Date DESC";

                    using (SqlCommand cmd = new SqlCommand(queryAtt, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", _studentId);

                        if (period.Contains("Этот месяц", StringComparison.OrdinalIgnoreCase))
                        {
                            cmd.Parameters.AddWithValue("@year", DateTime.Today.Year);
                            cmd.Parameters.AddWithValue("@month", DateTime.Today.Month);
                        }
                        else if (period.Contains("Прошлый месяц", StringComparison.OrdinalIgnoreCase))
                        {
                            DateTime prevMonth = DateTime.Today.AddMonths(-1);
                            cmd.Parameters.AddWithValue("@year", prevMonth.Year);
                            cmd.Parameters.AddWithValue("@month", prevMonth.Month);
                        }
                        else if (period.Contains("Весь семестр", StringComparison.OrdinalIgnoreCase))
                        {
                            cmd.Parameters.AddWithValue("@fromDate", DateTime.Today.AddMonths(-6));
                        }

                        if (!string.IsNullOrWhiteSpace(subject) && !subject.Contains("Все", StringComparison.OrdinalIgnoreCase))
                        {
                            cmd.Parameters.AddWithValue("@subject", subject);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string status = reader.IsDBNull(2) ? "Неизвестно" : reader.GetString(2);
                                if (status.Contains("присутств", StringComparison.OrdinalIgnoreCase) || status.Contains("был", StringComparison.OrdinalIgnoreCase))
                                {
                                    presentCount++;
                                }
                                else if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Неизвестно", StringComparison.OrdinalIgnoreCase))
                                {
                                    absentCount++;
                                }

                                AttendanceList.Add(new AttendanceVM
                                {
                                    Date = reader.GetDateTime(0).ToShortDateString(),
                                    Subject = reader.GetString(1),
                                    Type = "Лекция",
                                    Status = status,
                                    Note = string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки посещаемости: {ex.Message}", "Посещаемость", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            tbPresentCount.Text = $"Присутствий: {presentCount}";
            tbAbsentCount.Text = $"Пропусков: {absentCount}";
        }

        private void UpdateNotificationCount()
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Grades WHERE StudentID = @sid AND GradeDate >= DATEADD(day, -7, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", _studentId);
                        int count = (int)cmd.ExecuteScalar();
                        btnNotifications.Content = $"Уведомления ({count})";
                    }
                }
            }
            catch
            {
                btnNotifications.Content = "Уведомления";
            }
        }

        private static string GetSelectedText(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString() ?? string.Empty;
            }

            if (comboBox.SelectedItem is SubjectItem subject)
            {
                return subject.SubjectName ?? string.Empty;
            }

            return string.Empty;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnRefreshGrades_Click(object sender, RoutedEventArgs e)
        {
            LoadGrades();
            MessageBox.Show("Данные об оценках обновлены.", "Мои оценки", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDownloadReport_Click(object sender, RoutedEventArgs e)
        {
            string query = GetStudentReportQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            ReportGenerator.GenerateCsv(query, "student-report.csv");
        }

        private void BtnNotifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Grades WHERE StudentID = @sid AND GradeDate >= DATEADD(day, -7, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", _studentId);
                        int count = (int)cmd.ExecuteScalar();
                        MessageBox.Show($"Новых оценок за неделю: {count}", "Уведомления", MessageBoxButton.OK, MessageBoxImage.Information);
                        btnNotifications.Content = $"Уведомления ({count})";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения уведомлений: {ex.Message}");
            }
        }

        private string GetStudentReportQuery()
        {
            if (cbStudentReportType.SelectedItem is ComboBoxItem item)
            {
                string type = item.Content?.ToString() ?? string.Empty;
                if (type.Contains("Транскрипт", StringComparison.OrdinalIgnoreCase) || type.Contains("успеваемости", StringComparison.OrdinalIgnoreCase))
                {
                    return $@"
                        SELECT sub.SubjectName AS Subject,
                               t.LastName + ' ' + t.FirstName AS Teacher,
                               g.Grade,
                               g.GradeDate
                        FROM Grades g
                        JOIN Subjects sub ON g.SubjectID = sub.SubjectID
                        JOIN Teachers t ON g.TeacherID = t.TeacherID
                        WHERE g.StudentID = {_studentId}";
                }

                if (type.Contains("посещаемости", StringComparison.OrdinalIgnoreCase))
                {
                    return $@"
                        SELECT a.Date,
                               sub.SubjectName AS Subject,
                               a.Status
                        FROM Attendance a
                        JOIN Subjects sub ON a.SubjectID = sub.SubjectID
                        WHERE a.StudentID = {_studentId}";
                }
            }

            MessageBox.Show("Выберите тип отчета.");
            return null;
        }

        public class GradeVM { public string Subject { get; set; } public string Teacher { get; set; } public string GradeValue { get; set; } public string Date { get; set; } public string WorkType { get; set; } public string Comment { get; set; } }
        public class AttendanceVM { public string Date { get; set; } public string Subject { get; set; } public string Type { get; set; } public string Status { get; set; } public string Note { get; set; } }
        public class SubjectItem { public int SubjectID { get; set; } public string SubjectName { get; set; } }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp3
{
    public partial class TeacherWindow : Window
    {
        private int _teacherId = 1;
        public ObservableCollection<GradeVM> GradesList { get; set; }
        public ObservableCollection<StudentVM> StudentsList { get; set; }
        public ObservableCollection<AttendanceVM> AttendanceList { get; set; }
        public ObservableCollection<GroupItem> Groups { get; set; }
        public ObservableCollection<GroupItem> GroupsWithAll { get; set; }
        public ObservableCollection<SubjectItem> Subjects { get; set; }

        public TeacherWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadLookups();
            LoadStudents();
            LoadGrades();
        }

        private void LoadLookups()
        {
            Groups = new ObservableCollection<GroupItem>();
            GroupsWithAll = new ObservableCollection<GroupItem>
            {
                new GroupItem { GroupID = 0, GroupName = "Все группы" }
            };
            Subjects = new ObservableCollection<SubjectItem>();

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT GroupID, GroupName FROM Groups", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GroupItem group = new GroupItem { GroupID = reader.GetInt32(0), GroupName = reader.GetString(1) };
                            Groups.Add(group);
                            GroupsWithAll.Add(group);
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT SubjectID, SubjectName FROM Subjects", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Subjects.Add(new SubjectItem { SubjectID = reader.GetInt32(0), SubjectName = reader.GetString(1) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}");
            }

            cbGradeGroup.ItemsSource = Groups;
            cbAttendanceGroup.ItemsSource = Groups;
            ComboBox teacherGroupCombo = GetTeacherStudentGroupCombo();
            if (teacherGroupCombo != null)
            {
                teacherGroupCombo.ItemsSource = GroupsWithAll;
            }
            cbGradeSubject.ItemsSource = Subjects;
            cbAttendanceSubject.ItemsSource = Subjects;

            if (cbGradeGroup.Items.Count > 0) cbGradeGroup.SelectedIndex = 0;
            if (cbAttendanceGroup.Items.Count > 0) cbAttendanceGroup.SelectedIndex = 0;
            if (teacherGroupCombo != null && teacherGroupCombo.Items.Count > 0) teacherGroupCombo.SelectedIndex = 0;
            if (cbGradeSubject.Items.Count > 0) cbGradeSubject.SelectedIndex = 0;
            if (cbAttendanceSubject.Items.Count > 0) cbAttendanceSubject.SelectedIndex = 0;
            if (dpAttendanceDate.SelectedDate == null) dpAttendanceDate.SelectedDate = DateTime.Today;
        }

        private int? GetSelectedGroupId(ComboBox combo)
        {
            if (combo.SelectedItem is GroupItem item)
            {
                return item.GroupID == 0 ? null : item.GroupID;
            }

            return null;
        }

        private int? GetSelectedSubjectId(ComboBox combo)
        {
            return combo.SelectedItem is SubjectItem item ? item.SubjectID : null;
        }

        private void LoadGrades()
        {
            GradesList = new ObservableCollection<GradeVM>();
            dgGrades.ItemsSource = GradesList;

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    int? groupId = GetSelectedGroupId(cbGradeGroup);
                    int? subjectId = GetSelectedSubjectId(cbGradeSubject);

                    string query = @"
                        SELECT g.GradeID,
                               s.LastName + ' ' + s.FirstName + ISNULL(' ' + s.Patronymic, '') AS FullName,
                               sub.SubjectName,
                               g.Grade,
                               g.GradeDate
                        FROM Grades g
                        JOIN Students s ON g.StudentID = s.StudentID
                        JOIN Subjects sub ON g.SubjectID = sub.SubjectID
                        WHERE g.TeacherID = @tid";

                    if (groupId.HasValue)
                    {
                        query += " AND s.GroupID = @gid";
                    }

                    if (subjectId.HasValue)
                    {
                        query += " AND g.SubjectID = @sid";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tid", _teacherId);
                        if (groupId.HasValue) cmd.Parameters.AddWithValue("@gid", groupId.Value);
                        if (subjectId.HasValue) cmd.Parameters.AddWithValue("@sid", subjectId.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GradesList.Add(new GradeVM
                                {
                                    GradeID = reader.GetInt32(0),
                                    FullName = reader.GetString(1).Trim(),
                                    SubjectName = reader.GetString(2),
                                    GradeValue = reader.GetInt32(3).ToString(),
                                    Date = reader.GetDateTime(4).ToShortDateString(),
                                    Type = "Оценка",
                                    Comment = ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оценок: {ex.Message}");
            }
        }

        private void LoadStudents()
        {
            StudentsList = new ObservableCollection<StudentVM>();
            dgStudents.ItemsSource = StudentsList;

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT s.StudentID,
                               s.GroupID,
                               s.LastName + ' ' + s.FirstName + ISNULL(' ' + s.Patronymic, '') AS FullName,
                               g.GroupName,
                               s.Email
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
                                GroupID = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                FullName = reader.GetString(2).Trim(),
                                GroupName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Contact = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}");
            }

            ApplyStudentFilter();
        }

        private void LoadAttendance()
        {
            AttendanceList = new ObservableCollection<AttendanceVM>();
            dgAttendance.ItemsSource = AttendanceList;

            int? groupId = GetSelectedGroupId(cbAttendanceGroup);
            int? subjectId = GetSelectedSubjectId(cbAttendanceSubject);
            DateTime? date = dpAttendanceDate.SelectedDate;

            if (!groupId.HasValue || !subjectId.HasValue || !date.HasValue)
            {
                MessageBox.Show("Выберите группу, предмет и дату.");
                return;
            }

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT s.StudentID,
                               s.LastName + ' ' + s.FirstName + ISNULL(' ' + s.Patronymic, '') AS FullName,
                               a.Status
                        FROM Students s
                        LEFT JOIN Attendance a ON a.StudentID = s.StudentID AND a.SubjectID = @sid AND a.Date = @date
                        WHERE s.GroupID = @gid";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sid", subjectId.Value);
                        cmd.Parameters.AddWithValue("@date", date.Value);
                        cmd.Parameters.AddWithValue("@gid", groupId.Value);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string status = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                AttendanceList.Add(new AttendanceVM
                                {
                                    StudentID = reader.GetInt32(0),
                                    FullName = reader.GetString(1).Trim(),
                                    IsPresent = status.Equals("Присутствовал", StringComparison.OrdinalIgnoreCase) || status.Equals("Present", StringComparison.OrdinalIgnoreCase),
                                    Reason = ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки посещаемости: {ex.Message}");
            }
        }

        private void SaveAttendance()
        {
            int? subjectId = GetSelectedSubjectId(cbAttendanceSubject);
            DateTime? date = dpAttendanceDate.SelectedDate;

            if (!subjectId.HasValue || !date.HasValue)
            {
                MessageBox.Show("Выберите предмет и дату.");
                return;
            }

            try
            {
                using (SqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    foreach (AttendanceVM item in AttendanceList)
                    {
                        string status = item.IsPresent ? "Присутствовал" : "Отсутствовал";
                        string existsQuery = "SELECT COUNT(*) FROM Attendance WHERE StudentID=@sid AND SubjectID=@subid AND Date=@date";
                        using (SqlCommand cmd = new SqlCommand(existsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@sid", item.StudentID);
                            cmd.Parameters.AddWithValue("@subid", subjectId.Value);
                            cmd.Parameters.AddWithValue("@date", date.Value);
                            int count = (int)cmd.ExecuteScalar();

                            if (count > 0)
                            {
                                string update = "UPDATE Attendance SET Status=@status WHERE StudentID=@sid AND SubjectID=@subid AND Date=@date";
                                using (SqlCommand upd = new SqlCommand(update, conn))
                                {
                                    upd.Parameters.AddWithValue("@status", status);
                                    upd.Parameters.AddWithValue("@sid", item.StudentID);
                                    upd.Parameters.AddWithValue("@subid", subjectId.Value);
                                    upd.Parameters.AddWithValue("@date", date.Value);
                                    upd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                string insert = "INSERT INTO Attendance (StudentID, SubjectID, Date, Status) VALUES (@sid, @subid, @date, @status)";
                                using (SqlCommand ins = new SqlCommand(insert, conn))
                                {
                                    ins.Parameters.AddWithValue("@sid", item.StudentID);
                                    ins.Parameters.AddWithValue("@subid", subjectId.Value);
                                    ins.Parameters.AddWithValue("@date", date.Value);
                                    ins.Parameters.AddWithValue("@status", status);
                                    ins.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                MessageBox.Show("Посещаемость сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения посещаемости: {ex.Message}");
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnAddGrade_Click(object sender, RoutedEventArgs e)
        {
            AddGradeWindow window = new AddGradeWindow(_teacherId);
            if (window.ShowDialog() == true)
            {
                LoadGrades();
            }
        }

        private void BtnEditGrade_Click(object sender, RoutedEventArgs e)
        {
            if (dgGrades.SelectedItem is GradeVM grade)
            {
                EditGradeWindow window = new EditGradeWindow(grade.GradeID);
                if (window.ShowDialog() == true)
                {
                    LoadGrades();
                }
            }
            else
            {
                MessageBox.Show("Выберите оценку для редактирования.");
            }
        }

        private void BtnDeleteGrade_Click(object sender, RoutedEventArgs e)
        {
            if (dgGrades.SelectedItem is GradeVM grade)
            {
                try
                {
                    using (SqlConnection conn = Database.GetConnection())
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Grades WHERE GradeID=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", grade.GradeID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadGrades();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите оценку для удаления.");
            }
        }

        private void BtnUpdateAttendance_Click(object sender, RoutedEventArgs e)
        {
            LoadAttendance();
        }

        private void CbGradeGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrades();
        }

        private void CbGradeSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrades();
        }

        private void CbAttendanceGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAttendance();
        }

        private void CbAttendanceSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAttendance();
        }

        private void DpAttendanceDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAttendance();
        }

        private void BtnSaveAttendance_Click(object sender, RoutedEventArgs e)
        {
            SaveAttendance();
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string query = GetTeacherReportQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            ReportPreviewWindow window = new ReportPreviewWindow("Отчет преподавателя", query);
            window.ShowDialog();
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            string query = GetTeacherReportQuery();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            ReportGenerator.GenerateCsv(query, "teacher-report.csv");
        }

        private string GetTeacherReportQuery()
        {
            if (cbTeacherReportType.SelectedItem is ComboBoxItem item)
            {
                string type = item.Content?.ToString() ?? string.Empty;
                if (type.Contains("успеваемость", StringComparison.OrdinalIgnoreCase))
                {
                    return $@"
                        SELECT s.LastName + ' ' + s.FirstName AS Student,
                               sub.SubjectName,
                               g.Grade,
                               g.GradeDate
                        FROM Grades g
                        JOIN Students s ON s.StudentID = g.StudentID
                        JOIN Subjects sub ON sub.SubjectID = g.SubjectID
                        WHERE g.TeacherID = {_teacherId}";
                }

                if (type.Contains("экзамен", StringComparison.OrdinalIgnoreCase))
                {
                    return $@"
                        SELECT s.LastName + ' ' + s.FirstName AS Student,
                               AVG(CAST(g.Grade AS FLOAT)) AS AvgGrade
                        FROM Grades g
                        JOIN Students s ON s.StudentID = g.StudentID
                        WHERE g.TeacherID = {_teacherId}
                        GROUP BY s.LastName, s.FirstName";
                }

                if (type.Contains("пропуск", StringComparison.OrdinalIgnoreCase))
                {
                    return @"
                        SELECT s.LastName + ' ' + s.FirstName AS Student,
                               COUNT(*) AS Absences
                        FROM Attendance a
                        JOIN Students s ON s.StudentID = a.StudentID
                        WHERE a.Status = 'Отсутствовал'
                        GROUP BY s.LastName, s.FirstName";
                }
            }

            MessageBox.Show("Выберите тип отчета.");
            return null;
        }

        private void BtnSearchTeacherStudent_Click(object sender, RoutedEventArgs e)
        {
            ApplyStudentFilter();
        }

        private void ApplyStudentFilter()
        {
            if (StudentsList == null)
            {
                return;
            }

            string query = tbTeacherStudentSearch.Text?.Trim() ?? string.Empty;
            if (query.Equals("Поиск по ФИО...", StringComparison.OrdinalIgnoreCase))
            {
                query = string.Empty;
            }

            ComboBox teacherGroupCombo = GetTeacherStudentGroupCombo();
            int? groupId = teacherGroupCombo == null ? null : GetSelectedGroupId(teacherGroupCombo);
            var filtered = StudentsList.Where(student =>
            {
                bool matchGroup = !groupId.HasValue || student.GroupID == groupId.Value;
                bool matchQuery = string.IsNullOrWhiteSpace(query) || student.FullName.Contains(query, StringComparison.OrdinalIgnoreCase);
                return matchGroup && matchQuery;
            }).ToList();

            dgStudents.ItemsSource = filtered;
        }

        private ComboBox GetTeacherStudentGroupCombo()
        {
            return FindName("cbTeacherStudentGroup") as ComboBox;
        }

        public class GradeVM
        {
            public int GradeID { get; set; }
            public string FullName { get; set; }
            public string SubjectName { get; set; }
            public string GradeValue { get; set; }
            public string Date { get; set; }
            public string Type { get; set; }
            public string Comment { get; set; }
        }

        public class StudentVM
        {
            public int StudentID { get; set; }
            public int? GroupID { get; set; }
            public string FullName { get; set; }
            public string GroupName { get; set; }
            public string Contact { get; set; }
        }

        public class AttendanceVM
        {
            public int StudentID { get; set; }
            public string FullName { get; set; }
            public bool IsPresent { get; set; }
            public string Reason { get; set; }
        }

        public class GroupItem { public int GroupID { get; set; } public string GroupName { get; set; } }
        public class SubjectItem { public int SubjectID { get; set; } public string SubjectName { get; set; } }
    }
}

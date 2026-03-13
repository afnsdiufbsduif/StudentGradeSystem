using System;

namespace WpfApp3
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }  // Student, Teacher, Deanery
        public int? StudentID { get; set; }
        public int? TeacherID { get; set; }
    }

    public class StudentModel
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int GroupID { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }  // Добавлен адрес
    }

    public class TeacherModel
    {
        public int TeacherID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public string Department { get; set; }  // Добавлен департамент
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class GroupModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Faculty { get; set; }
        public int Course { get; set; }
    }

    public class SubjectModel
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int CreditHours { get; set; }  // Добавлены часы для предмета
    }

    public class GradeModel
    {
        public int GradeID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int TeacherID { get; set; }
        public int Grade { get; set; }
        public DateTime GradeDate { get; set; }
    }

    public class AttendanceModel
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }  // "Present" or "Absent"
    }

    public class ReportModel
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public DateTime ReportDate { get; set; }
        public int CreatedBy { get; set; }
        public string ReportContent { get; set; }
    }
}
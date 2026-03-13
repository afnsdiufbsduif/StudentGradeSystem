using System.Text;
using System.Windows;

namespace WpfApp3
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text;
            string password = pbPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string role = Database.AuthenticateUser(login, password);

            if (role == "Student")
            {
                StudentWindow studentWindow = new StudentWindow();
                studentWindow.Show();
                this.Close();
            }
            else if (role == "Teacher")
            {
                TeacherWindow teacherWindow = new TeacherWindow();
                teacherWindow.Show();
                this.Close();
            }
            else if (role == "Deanery")
            {
                DeaneryWindow deaneryWindow = new DeaneryWindow();
                deaneryWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
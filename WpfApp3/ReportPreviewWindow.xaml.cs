using System;
using System.Data;
using System.Windows;

namespace WpfApp3
{
    public partial class ReportPreviewWindow : Window
    {
        private readonly string _query;

        public ReportPreviewWindow(string title, string query)
        {
            InitializeComponent();
            _query = query;
            tbTitle.Text = title;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DataTable table = ReportGenerator.GetDataTable(_query);
                dgReport.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

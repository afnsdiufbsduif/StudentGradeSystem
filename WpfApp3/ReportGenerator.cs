using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace WpfApp3
{
    public static class ReportGenerator
    {
        public static DataTable GetDataTable(string query)
        {
            DataTable table = new DataTable();
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }
            }
            return table;
        }

        public static void GenerateCsv(string query, string defaultFileName)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = defaultFileName;
                if (sfd.ShowDialog() == true)
                {
                    GenerateCsvToFile(query, sfd.FileName);
                    MessageBox.Show("Отчет успешно сохранен!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void GenerateCsvToFile(string query, string filePath)
        {
            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                    {
                        string header = "";
                        for (int i = 0; i < reader.FieldCount; i++)
                            header += reader.GetName(i) + (i < reader.FieldCount - 1 ? "," : "");
                        sw.WriteLine(header);

                        while (reader.Read())
                        {
                            string line = "";
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string val = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();
                                line += $"\"{val.Replace("\"", "\"\"")}\"" + (i < reader.FieldCount - 1 ? "," : "");
                            }
                            sw.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
}
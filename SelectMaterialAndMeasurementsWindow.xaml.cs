using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Atelie
{
    public partial class SelectMaterialAndMeasurementsWindow : Window
    {
        private string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;";
        public int? SelectedMaterialId { get; private set; }
        public int? SelectedMeasurementId { get; private set; }

        public SelectMaterialAndMeasurementsWindow()
        {
            InitializeComponent();
            LoadMaterials();
            LoadMeasurements();
        }

        private void LoadMaterials()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT MaterialId, MaterialName FROM Materials";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // Добавляем элементы в ComboBox
                        MaterialComboBox.Items.Add(new ComboBoxItem
                        {
                            Content = reader["MaterialName"].ToString(),
                            Tag = reader["MaterialId"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке материалов: " + ex.Message);
            }
        }

        private void LoadMeasurements()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, SetName FROM atelier_measurements";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // Добавляем элементы в ComboBox
                        MeasurementComboBox.Items.Add(new ComboBoxItem
                        {
                            Content = reader["SetName"].ToString(),
                            Tag = reader["Id"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке мерок: " + ex.Message);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedItem is ComboBoxItem materialItem &&
                MeasurementComboBox.SelectedItem is ComboBoxItem measurementItem)
            {
                SelectedMaterialId = Convert.ToInt32(materialItem.Tag);
                SelectedMeasurementId = Convert.ToInt32(measurementItem.Tag);

                // Проверка на наличие выбранных значений
                if (SelectedMaterialId.HasValue && SelectedMeasurementId.HasValue)
                {
                    DialogResult = true; // Закрыть окно с подтверждением
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите материал и набор мерок.");
                }
            }
            else
            {
                MessageBox.Show("Выберите материал и мерки.");
            }
        }
    }
}

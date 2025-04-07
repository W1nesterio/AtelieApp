using System;
using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Atelie
{
    public partial class EnterMaterial : Window
    {
        private const string connectionString = "server=localhost;user=root;password=root;database=Atelie;";
        private string selectedImagePath = string.Empty;  // Хранение выбранного пути к изображению

        public EnterMaterial()
        {
            InitializeComponent();
            LoadMaterials();  // Загружаем материалы при открытии окна
        }

        private void LoadMaterials()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT MaterialName, ImagePath FROM Materials";  // Запрос для получения всех материалов
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    List<Material> materials = new List<Material>();

                    while (reader.Read())
                    {
                        materials.Add(new Material
                        {
                            MaterialName = reader.GetString("MaterialName"),
                          
                        });
                    }

                    MaterialsListBox.ItemsSource = materials;  // Привязываем список материалов к ListBox
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке материалов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик для выбора изображения
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Открытие диалога выбора файла изображения
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;  // Сохраняем путь к изображению
            }
        }

        // Обработчик для добавления нового материала
        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            string materialName = MaterialNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(materialName) || string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Materials (MaterialName, ImagePath) VALUES (@MaterialName, @ImagePath)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaterialName", materialName);
                    cmd.Parameters.AddWithValue("@ImagePath", selectedImagePath);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Материал добавлен успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadMaterials();  // Перезагружаем список материалов

                // Очищаем поле ввода текста после добавления материала
                MaterialNameTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении материала: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Класс для представления материала
        public class Material
        {
            public string MaterialName { get; set; }
            public string ImagePath { get; set; }
        }
    }
}

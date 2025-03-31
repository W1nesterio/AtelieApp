using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace Atelie
{
    public partial class CreateCardWindow : Window
    {
        private string imagePath; // Поле для хранения пути к изображению

        public CreateCardWindow()
        {
            InitializeComponent();
        }

        // Обработчик нажатия кнопки "Добавить"
        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            decimal price;
            string description = DescriptionTextBox.Text;

            // Проверка, чтобы цена была валидной
            if (decimal.TryParse(PriceTextBox.Text, out price))
            {
                // Запрос на добавление карточки в базу данных
                string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;Charset=utf8;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "INSERT INTO Cards (Title, Price, Description, ImagePath) VALUES (@Title, @Price, @Description, @ImagePath)";

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Title", title);
                            command.Parameters.AddWithValue("@Price", price);
                            command.Parameters.AddWithValue("@Description", description);
                            command.Parameters.AddWithValue("@ImagePath", imagePath); // Путь к картинке

                            command.ExecuteNonQuery();
                            MessageBox.Show("Карточка успешно добавлена!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при добавлении карточки: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Неверный формат цены.");
            }
        }

        // Обработчик нажатия кнопки "Добавить картинку"
        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;";

            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName; // Сохраняем путь к выбранному изображению
            }
        }
    }
}

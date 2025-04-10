using System;
using System.Windows;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace Atelie
{
    public partial class CreateCards : Window
    {
        private string imagePath = null;
        private const string connectionString = "server=localhost;user=root;password=root;database=Atelie;";

        // Определяем событие CardAdded
        public event EventHandler<Card> CardAdded;

        public CreateCards()
        {
            InitializeComponent();
            DescriptionBox.TextChanged += DescriptionBox_TextChanged;
        }

        // Обработчик изменения текста в DescriptionBox (для отображения количества символов)
        private void DescriptionBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CharCount.Text = $"{DescriptionBox.Text.Length}/300";
        }

        // Обработчик клика по кнопке "Добавить изображение"
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";  // Фильтры для выбора изображений

            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName;  // Сохраняем путь к изображению
                ImagePathBlock.Text = imagePath;  // Отображаем путь в TextBlock
            }
        }

        // Обработчик клика по кнопке "Добавить карточку"
        private void AddCard_Click(object sender, RoutedEventArgs e)
        {
            bool hasError = false;

            // Проверка наличия обязательных полей
            TitleWarning.Visibility = string.IsNullOrWhiteSpace(TitleBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            PriceWarning.Visibility = string.IsNullOrWhiteSpace(PriceBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            DescWarning.Visibility = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? Visibility.Visible : Visibility.Collapsed;

            hasError = TitleWarning.Visibility == Visibility.Visible ||
                       PriceWarning.Visibility == Visibility.Visible ||
                       DescWarning.Visibility == Visibility.Visible;

            if (hasError)
                return;

            if (!decimal.TryParse(PriceBox.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Cards (Title, Price, Description, ImagePath) VALUES (@title, @price, @desc, @img)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Добавляем параметры для запроса
                    cmd.Parameters.AddWithValue("@title", TitleBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@desc", DescriptionBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@img", string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);  // Если изображения нет, передаем DBNull

                    int rowsAffected = cmd.ExecuteNonQuery();  // Выполняем запрос
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Карточка успешно добавлена в базу данных!");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении карточки в базу данных.");
                    }
                }

                // После успешного добавления, создаем объект новой карточки
                var newCard = new Card
                {
                    Title = TitleBox.Text.Trim(),
                    Price = price,
                    Description = DescriptionBox.Text.Trim(),
                    ImagePath = imagePath ?? ""  // Путь к изображению, если его нет - пустая строка
                };

                // Вызываем событие добавления карточки
                CardAdded?.Invoke(this, newCard);
                this.Close();  // Закрываем окно
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message);  // Обработка ошибок
            }
        }
    }
}

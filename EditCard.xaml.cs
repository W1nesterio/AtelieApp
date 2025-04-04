using System;
using System.Windows;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace Atelie
{
    public partial class EditCard : Window
    {
        private readonly Card _cardToEdit;
        private const string connectionString = "server=localhost;user=root;password=root;database=Atelie;";

        // Конструктор, принимающий карточку для редактирования
        public EditCard(Card card)
        {
            InitializeComponent();
            _cardToEdit = card;

            // Инициализируем поля формы значениями из карточки
            TitleTextBox.Text = _cardToEdit.Title;
            PriceTextBox.Text = _cardToEdit.Price.ToString();
            DescriptionTextBox.Text = _cardToEdit.Description;
            ImagePathTextBox.Text = _cardToEdit.ImagePath;
        }

        // Обработчик выбора изображения
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePathTextBox.Text = openFileDialog.FileName;
            }
        }

        // Обработчик сохранения изменений
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка на корректность введенных данных
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                    string.IsNullOrWhiteSpace(ImagePathTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновляем карточку в базе данных
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Cards SET Title = @Title, Price = @Price, Description = @Description, ImagePath = @ImagePath WHERE CardId = @CardId";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Title", TitleTextBox.Text);
                    cmd.Parameters.AddWithValue("@Price", decimal.Parse(PriceTextBox.Text));
                    cmd.Parameters.AddWithValue("@Description", DescriptionTextBox.Text);
                    cmd.Parameters.AddWithValue("@ImagePath", ImagePathTextBox.Text);
                    cmd.Parameters.AddWithValue("@CardId", _cardToEdit.CardId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Карточка успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true; // Закрытие окна с успешным результатом
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении карточки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

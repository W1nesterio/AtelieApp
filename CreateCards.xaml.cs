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

        private void DescriptionBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CharCount.Text = $"{DescriptionBox.Text.Length}/300";
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName;
                ImagePathBlock.Text = imagePath;
            }
        }

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


                    cmd.Parameters.AddWithValue("@title", TitleBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@desc", DescriptionBox.Text.Trim());
                    cmd.Parameters.AddWithValue("@img", string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);


                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Карточка успешно добавлена в базу данных!");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении карточки в базу данных.");
                    }
                }

                // После успешного добавления, вызываем событие и закрываем окно
                var newCard = new Card
                {
                    Title = TitleBox.Text.Trim(),
                    Price = price,
                    Description = DescriptionBox.Text.Trim(),
                    ImagePath = imagePath ?? ""
                };

                // Вызываем событие добавления карточки
                CardAdded?.Invoke(this, newCard);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении: " + ex.Message);
            }
        }
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Atelie
{
    public partial class Showcase : Window
    {
        public Showcase()
        {
            InitializeComponent();
            LoadCards(); // Загружаем карточки из базы данных при открытии окна
        }

        // Открытие окна создания карточки товара
        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем и показываем окно для добавления карточки
            CreateCardWindow createCardWindow = new CreateCardWindow();
            createCardWindow.ShowDialog();

            // После закрытия окна, загружаем карточки снова
            LoadCards();
        }

        // Метод для добавления карточки на экран
        private void AddCardToShowcase(string title, string description, decimal price, string imagePath)
        {
            // Создаем карточку с красивыми элементами
            var card = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray),
                BorderThickness = new System.Windows.Thickness(1),
                Margin = new System.Windows.Thickness(10),
                Padding = new System.Windows.Thickness(10),
                CornerRadius = new System.Windows.CornerRadius(10),  // Закругленные углы
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    Direction = 270,
                    ShadowDepth = 5,
                    Opacity = 0.3
                },
                Child = new StackPanel
                {
                    Children =
                    {
                        // Название карточки
                        new TextBlock { Text = title, FontWeight = FontWeights.Bold, FontSize = 18, Margin = new System.Windows.Thickness(0, 0, 0, 5) },
                        
                        // Описание карточки
                        new TextBlock { Text = description, FontSize = 14, Margin = new System.Windows.Thickness(0, 0, 0, 10) },

                        // Цена товара
                        new TextBlock { Text = $"Цена: {price} р.", FontSize = 14, Margin = new System.Windows.Thickness(0, 0, 0, 10) },

                        // Изображение товара (если оно есть)
                        !string.IsNullOrEmpty(imagePath) ? new Image
                        {
                            Source = new BitmapImage(new Uri(imagePath)),
                            Width = 200,
                            Height = 200,
                            Stretch = System.Windows.Media.Stretch.UniformToFill,
                            Margin = new System.Windows.Thickness(0, 10, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Center
                        } : null
                    }
                }
            };

            // Добавляем карточку в StackPanel
            CardStackPanel.Children.Add(card);
        }

        // Метод для загрузки карточек из базы данных
        private void LoadCards()
        {
            // Очистим текущие карточки в StackPanel
            CardStackPanel.Children.Clear();

            // Подключение к базе данных
            string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;Charset=utf8;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Cards";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader.GetString("Title");
                            decimal price = reader.GetDecimal("Price");
                            string description = reader.GetString("Description");
                            string imagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString("ImagePath");

                            // Добавляем карточку в интерфейс
                            AddCardToShowcase(title, description, price, imagePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке карточек: " + ex.Message);
                }
            }
        }
    }
}

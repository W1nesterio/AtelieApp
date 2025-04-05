using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Atelie
{
    public partial class AtelieWindowForUsers : Window
    {
        private string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;";
        private string currentUsername; // Глобальная переменная для хранения имени текущего пользователя

        public AtelieWindowForUsers(string username) // Конструктор для инициализации имени пользователя
        {
            InitializeComponent();
            currentUsername = username; // Сохраняем имя пользователя
            LoadCards();
        }

        // Метод для загрузки карточек из базы данных
        private void LoadCards()
        {
            List<Card> cards = new List<Card>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT CardId, Title, Price, Description, ImagePath FROM Cards";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Card card = new Card
                        {
                            CardId = reader.GetInt32("CardId"),
                            Title = reader.GetString("Title"),
                            Price = reader.GetDecimal("Price"),
                            Description = reader.GetString("Description"),
                            ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString("ImagePath")
                        };
                        cards.Add(card);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }

            // Отображение карточек в интерфейсе
            DisplayCards(cards);
        }

        // Метод для отображения карточек в WrapPanel
        private void DisplayCards(List<Card> cards)
        {
            foreach (var card in cards)
            {
                // Создаем элемент карточки
                StackPanel cardPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Width = 250,
                    Margin = new System.Windows.Thickness(10)
                };

                // Изображение
                Image cardImage = new Image
                {
                    Source = new BitmapImage(new Uri(card.ImagePath ?? "/default_image.png", UriKind.RelativeOrAbsolute)),
                    Width = 200,
                    Height = 150
                };

                // Заголовок карточки
                TextBlock titleBlock = new TextBlock
                {
                    Text = card.Title,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new System.Windows.Thickness(0, 10, 0, 5)
                };

                // Цена карточки
                TextBlock priceBlock = new TextBlock
                {
                    Text = $"Цена: {card.Price:C}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new System.Windows.Thickness(0, 10, 0, 10)
                };

                // Кнопка для отображения описания
                Button showDescriptionButton = new Button
                {
                    Content = "Показать описание",
                    Width = 200,
                    Margin = new System.Windows.Thickness(0, 5, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new System.Windows.Thickness(1),
                    Padding = new System.Windows.Thickness(10),
                    FontWeight = FontWeights.Bold
                };
                showDescriptionButton.Click += (sender, e) => ShowDescriptionWindow(card.Description);

                // Кнопка для добавления в корзину
                Button addToCartButton = new Button
                {
                    Content = "Добавить в корзину",
                    Width = 200,
                    Margin = new System.Windows.Thickness(0, 5, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new System.Windows.Thickness(1),
                    Padding = new System.Windows.Thickness(10),
                    FontWeight = FontWeights.Bold
                };
                addToCartButton.Click += (sender, e) => AddToCart(card);

                // Добавляем элементы в карточку
                cardPanel.Children.Add(cardImage);
                cardPanel.Children.Add(titleBlock);
                cardPanel.Children.Add(priceBlock);
                cardPanel.Children.Add(showDescriptionButton);
                cardPanel.Children.Add(addToCartButton);

                // Добавляем карточку в WrapPanel
                CardsWrapPanel.Children.Add(cardPanel);
            }
        }

        // Метод для отображения описания в отдельном окне
        private void ShowDescriptionWindow(string description)
        {
            DescriptionWindow descriptionWindow = new DescriptionWindow(description);
            descriptionWindow.Show();
        }

        // Метод для добавления карточки в корзину
        private void AddToCart(Card card)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Проверка, существует ли уже этот товар в корзине
                    string checkQuery = "SELECT COUNT(*) FROM CartItems WHERE UserId = (SELECT id FROM user WHERE username = @username) AND CardId = @cardId";
                    MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@username", currentUsername); // Используем текущего пользователя
                    checkCommand.Parameters.AddWithValue("@cardId", card.CardId);
                    long count = (long)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Этот товар уже в корзине!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Добавление товара в корзину
                        string insertQuery = "INSERT INTO CartItems (UserId, CardId) VALUES ((SELECT id FROM user WHERE username = @username), @cardId)";
                        MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@username", currentUsername); // Используем текущего пользователя
                        insertCommand.Parameters.AddWithValue("@cardId", card.CardId);
                        insertCommand.ExecuteNonQuery();

                        MessageBox.Show($"Карточка '{card.Title}' добавлена в корзину!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении в корзину: " + ex.Message);
                }
            }
        }

        // Обработчик для перехода на CartWindow при нажатии на корзину
        private void CartImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CartWindow cartWindow = new CartWindow();
            cartWindow.Show();
        }

        // Класс карточки
        public class Card
        {
            public int CardId { get; set; }
            public string Title { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string ImagePath { get; set; }
        }

        // Окно для отображения описания
        public class DescriptionWindow : Window
        {
            public DescriptionWindow(string description)
            {
                Title = "Описание товара";
                Width = 400;
                Height = 300;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WindowStyle = WindowStyle.None; // Убираем стандартные элементы окна
                Background = Brushes.White;

                TextBlock descriptionText = new TextBlock
                {
                    Text = description,
                    FontSize = 14,
                    Margin = new System.Windows.Thickness(20),
                    TextWrapping = TextWrapping.Wrap
                };

                Button closeButton = new Button
                {
                    Content = "Закрыть",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 100,
                    Height = 40,
                    Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    FontWeight = FontWeights.Bold
                };
                closeButton.Click += (sender, e) => this.Close();

                StackPanel panel = new StackPanel();
                panel.Children.Add(descriptionText);
                panel.Children.Add(closeButton);

                Content = panel;
            }
        }
    }
}

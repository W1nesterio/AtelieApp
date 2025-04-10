using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Atelie
{
    public partial class AtelieWindowForUsers : Window
    {
        private string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;";
        private string currentUsername;
        private DispatcherTimer searchTimer;

        public AtelieWindowForUsers(string username)
        {
            InitializeComponent();
            currentUsername = username;

            // Инициализация таймера для задержки поиска
            searchTimer = new DispatcherTimer();
            searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            searchTimer.Tick += SearchTimer_Tick;

            LoadCards();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            LoadCards(SearchTextBox.Text.Trim());
        }

        private void LoadCards(string searchText = "")
        {
            List<Card> cards = new List<Card>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT CardId, Title, Price, Description, ImagePath FROM Cards WHERE Title LIKE @search";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", $"%{searchText}%");

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
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }

            CardsWrapPanel.Children.Clear();

            if (cards.Count == 0)
            {
                TextBlock noResults = new TextBlock
                {
                    Text = "Ничего не найдено по данному запросу.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(20)
                };
                CardsWrapPanel.Children.Add(noResults);
            }
            else
            {
                DisplayCards(cards);
            }
        }

        private void DisplayCards(List<Card> cards)
        {
            foreach (var card in cards)
            {
                StackPanel cardPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Width = 250,
                    Margin = new Thickness(10)
                };

                Image cardImage = new Image
                {
                    Source = new BitmapImage(new Uri(card.ImagePath ?? "/default_image.png", UriKind.RelativeOrAbsolute)),
                    Width = 200,
                    Height = 150
                };

                TextBlock titleBlock = new TextBlock
                {
                    Text = card.Title,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                };

                var price = new TextBlock
                {
                    Text = $"Цена: {card.Price} BYN",  
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };


                Button showDescriptionButton = new Button
                {
                    Content = "Показать описание",
                    Width = 200,
                    Margin = new Thickness(0, 5, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(10),
                    FontWeight = FontWeights.Bold
                };
                showDescriptionButton.Click += (sender, e) => ShowDescriptionWindow(card.Description);

                Button addToCartButton = new Button
                {
                    Content = "Добавить в корзину",
                    Width = 200,
                    Margin = new Thickness(0, 5, 0, 5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(10),
                    FontWeight = FontWeights.Bold
                };
                addToCartButton.Click += (sender, e) => AddToCart(card);

                cardPanel.Children.Add(cardImage);
                cardPanel.Children.Add(titleBlock);
                cardPanel.Children.Add(price);
                cardPanel.Children.Add(showDescriptionButton);
                cardPanel.Children.Add(addToCartButton);

                CardsWrapPanel.Children.Add(cardPanel);
            }
        }

        private void ShowDescriptionWindow(string description)
        {
            DescriptionWindow descriptionWindow = new DescriptionWindow(description);
            descriptionWindow.Show();
        }

        private void AddToCart(Card card)
        {
            SelectMaterialAndMeasurementsWindow selectWindow = new SelectMaterialAndMeasurementsWindow();
            if (selectWindow.ShowDialog() == true)
            {
                int materialId = selectWindow.SelectedMaterialId.Value;
                int measurementId = selectWindow.SelectedMeasurementId.Value;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Используем правильное поле MeasurementId
                        string insertQuery = @"
                    INSERT INTO CartItems (UserId, CardId, MaterialId, MeasurementId)
                    VALUES ((SELECT id FROM user WHERE username = @username), @cardId, @materialId, @measurementId)";
                        MySqlCommand command = new MySqlCommand(insertQuery, connection);
                        command.Parameters.AddWithValue("@username", currentUsername);
                        command.Parameters.AddWithValue("@cardId", card.CardId);
                        command.Parameters.AddWithValue("@materialId", materialId);
                        command.Parameters.AddWithValue("@measurementId", measurementId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Товар добавлен в корзину с выбранным материалом и мерками!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при добавлении в корзину: " + ex.Message);
                    }
                }
            }
        }



        private void CartImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CartWindow cartWindow = new CartWindow(currentUsername);
            cartWindow.Show();
        }

        public class Card
        {
            public int CardId { get; set; }
            public string Title { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string ImagePath { get; set; }
        }

        public class DescriptionWindow : Window
        {
            public DescriptionWindow(string description)
            {
                Title = "Описание товара";
                Width = 400;
                Height = 300;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WindowStyle = WindowStyle.None;
                Background = Brushes.White;

                TextBlock descriptionText = new TextBlock
                {
                    Text = description,
                    FontSize = 14,
                    Margin = new Thickness(20),
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

        private void ViewCardDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            CardDetailsWindow cardDetailsWindow = new CardDetailsWindow(currentUsername);
            cardDetailsWindow.Show();
        }

        private void PromoButton_Click(object sender, RoutedEventArgs e)
        {
            MeasurementWindow measurementWindow = new MeasurementWindow();
            measurementWindow.Show();
        }

        private void MeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            CreatePromo promoWindow = new CreatePromo();
            promoWindow.Show();
        }
    }
}

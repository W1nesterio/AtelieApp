using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Windows.Threading;

namespace Atelie
{
    public partial class AtelieWindow : Window
    {
        private const string connectionString = "server=localhost;user=root;password=root;database=Atelie;";
        private DispatcherTimer _searchTimer;
        private string _lastSearchQuery = "";

        public AtelieWindow()
        {
            InitializeComponent();

            // Инициализируем таймер для задержки поиска
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // Задержка 500ms перед отправкой запроса
            };
            _searchTimer.Tick += SearchTimer_Tick;

            LoadCards();  // Загружаем карточки при инициализации окна
        }

        // Список для хранения карточек
        private List<Card> cards = new List<Card>();

        // Метод для открытия окна создания карточки
        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            var createCardWindow = new CreateCards();
            createCardWindow.CardAdded += CreateCardWindow_CardAdded;
            createCardWindow.ShowDialog();
        }

        // Метод для обработки добавления карточки
        private void CreateCardWindow_CardAdded(object sender, Card newCard)
        {
            LoadCards(); // Просто перезагружаем карточки из БД, включая только что добавленную
        }


        // Метод для загрузки карточек из базы данных с фильтрацией по названию
        private void LoadCards(string searchQuery = "")
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Cards WHERE Title LIKE @SearchQuery";  // SQL-запрос с фильтрацией
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");  // Параметр для безопасной подстановки значения

                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Очищаем текущие элементы перед добавлением новых
                    CardsWrapPanel.Children.Clear();
                    cards.Clear();  // Очищаем список карточек

                    while (reader.Read())
                    {
                        var card = new Card
                        {
                            CardId = reader.GetInt32("CardId"),
                            Title = reader.GetString("Title"),
                            Price = reader.GetDecimal("Price"),
                            Description = reader.GetString("Description"),
                            ImagePath = reader.GetString("ImagePath")
                        };

                        // Добавляем карточку в список
                        cards.Add(card);

                        // Создаем визуальный элемент для карточки
                        var cardControl = new CardControl(card, DeleteCard);

                        // Добавляем карточку в интерфейс
                        CardsWrapPanel.Children.Add(cardControl);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке карточек: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Метод для обработки клика по кнопке "Добавить материал"
        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            var enterMaterialWindow = new EnterMaterial(); // Открываем окно для добавления материала
            enterMaterialWindow.ShowDialog(); // Показать окно
        }
    
        // Метод для удаления карточки из базы данных и списка
        private void DeleteCard(Card card)
        {
            // Окно с подтверждением удаления
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту карточку?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Удаляем карточку из базы данных
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Cards WHERE CardId = @CardId"; // Запрос на удаление карточки по ID
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@CardId", card.CardId); // Добавляем параметр CardId

                        // Выполнение запроса
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Карточка удалена успешно!", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при удалении карточки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    // Удаляем карточку из списка (если успешно удалена из базы данных)
                    cards.Remove(card);

                    // Удаляем визуальное представление карточки из интерфейса
                    var cardControlToRemove = CardsWrapPanel.Children.OfType<CardControl>()
                        .FirstOrDefault(ctrl => ctrl.Card.CardId == card.CardId);
                    if (cardControlToRemove != null)
                    {
                        CardsWrapPanel.Children.Remove(cardControlToRemove);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении карточки: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик события изменения текста в поле поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();

            // Если текст изменился, запускаем таймер для отсроченного поиска
            if (_lastSearchQuery != searchQuery)
            {
                _lastSearchQuery = searchQuery;
                _searchTimer.Stop();  // Останавливаем старый таймер
                _searchTimer.Start();  // Запускаем новый таймер
            }
        }

        // Таймер для выполнения поиска с задержкой
        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();  // Останавливаем таймер после срабатывания
            LoadCards(_lastSearchQuery);  // Загружаем карточки с учетом текста поиска
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCards();  // Загружаем карточки при запуске окна
        }
    }

    // Класс карточки
    public class Card
    {
        public int CardId { get; set; }  // Идентификатор карточки
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }

    // Элемент управления для отображения карточки
    public class CardControl : UserControl
    {
        public Card Card { get; set; }
        private Action<Card> DeleteCardAction;

        public CardControl(Card card, Action<Card> deleteCardAction)
        {
            Card = card;
            DeleteCardAction = deleteCardAction;

            var stackPanel = new StackPanel
            {
                Width = 300,
                Height = 400,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var image = new Image
            {
                Source = new BitmapImage(new Uri(Card.ImagePath)),
                Width = 200,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var title = new TextBlock
            {
                Text = Card.Title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var price = new TextBlock
            {
                Text = $"Цена: {Card.Price} BYN",  
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };


            var infoButton = new Button
            {
                Content = "Информация",
                Width = 120,
                Height = 30,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White
            };

            infoButton.Click += InfoButton_Click;

            var deleteButton = new Button
            {
                Content = "Удалить",
                Width = 120,
                Height = 30,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White
            };

            deleteButton.Click += DeleteButton_Click;

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(price);
            stackPanel.Children.Add(infoButton);
            stackPanel.Children.Add(deleteButton);

            this.Content = stackPanel;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Информация о карточке:\n\n{Card.Description}", "Информация о карточке", MessageBoxButton.OK);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Редактировать карточку");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteCardAction(Card);  // Вызов делегата для удаления карточки
        }
    }
}

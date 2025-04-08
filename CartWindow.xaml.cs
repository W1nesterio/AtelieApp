using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Atelie
{
    public partial class CartWindow : Window
    {
        private string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;";
        private string currentUsername;

        public CartWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            LoadCartItems();
            LoadCardDetails();
        }

        // Метод для загрузки товаров из корзины
        private void LoadCartItems()
        {
            List<CartItem> cartItems = new List<CartItem>();
            decimal totalAmount = 0; // Для вычисления суммы товаров

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT Cards.Title, Cards.Price, CartItems.CardId
                        FROM CartItems
                        INNER JOIN Cards ON CartItems.CardId = Cards.CardId
                        WHERE CartItems.UserId = (SELECT id FROM user WHERE username = @username)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", currentUsername); // Используем текущего пользователя
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CartItem cartItem = new CartItem
                        {
                            Title = reader.GetString("Title"),
                            Price = reader.GetDecimal("Price"),
                            CardId = reader.GetInt32("CardId")
                        };
                        totalAmount += cartItem.Price; // Добавляем цену к общей сумме
                        cartItems.Add(cartItem);
                    }

                    // Отображаем общую сумму и налог
                    TotalAmountText.Text = totalAmount.ToString("C");
                    decimal taxAmount = totalAmount * 0.05M; // Налог 5%
                    TaxAmountText.Text = taxAmount.ToString("C");
                    FinalAmountText.Text = (totalAmount + taxAmount).ToString("C");

                    // Если корзина пуста, показываем сообщение
                    if (cartItems.Count == 0)
                    {
                        CartListView.Visibility = Visibility.Collapsed;
                        NoItemsText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CartListView.Visibility = Visibility.Visible;
                        NoItemsText.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке товаров из корзины: " + ex.Message);
                }
            }

            // Отображаем товары в корзине
            DisplayCartItems(cartItems);
        }

        // Метод для загрузки данных карты
        private void LoadCardDetails()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT CardNumber, CardHolderName, ExpiryDate
                        FROM usercards
                        WHERE UserId = (SELECT id FROM user WHERE username = @username)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", currentUsername);
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        CardNumberText.Text = reader.GetString("CardNumber");
                        CardHolderNameText.Text = reader.GetString("CardHolderName");
                        ExpiryDateText.Text = reader.GetDateTime("ExpiryDate").ToString("MM/yyyy");
                        CardDetailsPanel.Visibility = Visibility.Visible;
                        NoCardText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        CardDetailsPanel.Visibility = Visibility.Collapsed;
                        NoCardText.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных карты: " + ex.Message);
                }
            }
        }

        // Метод для отображения товаров в корзине
        private void DisplayCartItems(List<CartItem> cartItems)
        {
            CartListView.ItemsSource = cartItems; // Привязываем список товаров в корзине к ListView
        }

        // Класс для представления товара в корзине
        public class CartItem
        {
            public string Title { get; set; }
            public decimal Price { get; set; }
            public int CardId { get; set; }
        }

        // Обработчик для удаления товара из корзины (по кнопке "Удалить")
        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            CartItem cartItem = button.DataContext as CartItem;
            if (cartItem != null)
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string deleteQuery = @"
                            DELETE FROM CartItems
                            WHERE UserId = (SELECT id FROM user WHERE username = @username)
                            AND CardId = @cardId";
                        MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                        command.Parameters.AddWithValue("@username", currentUsername);
                        command.Parameters.AddWithValue("@cardId", cartItem.CardId);
                        command.ExecuteNonQuery();

                        MessageBox.Show($"Товар '{cartItem.Title}' удален из корзины.", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCartItems(); // Перезагружаем корзину после удаления товара
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении товара из корзины: " + ex.Message);
                    }
                }
            }
        }

        // Обработчик для оформления заказа
        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на наличие карты
            if (CardDetailsPanel.Visibility == Visibility.Collapsed)
            {
                MessageBox.Show("Пожалуйста, добавьте карту для оформления заказа.");
                return;
            }

            decimal totalAmount = 0; // Общая сумма товаров
            List<CartItem> cartItems = new List<CartItem>();

            // Сначала получим товары из корзины и рассчитаем общую сумму
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT Cards.Title, Cards.Price, CartItems.CardId
                        FROM CartItems
                        INNER JOIN Cards ON CartItems.CardId = Cards.CardId
                        WHERE CartItems.UserId = (SELECT id FROM user WHERE username = @username)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", currentUsername);
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CartItem cartItem = new CartItem
                        {
                            Title = reader.GetString("Title"),
                            Price = reader.GetDecimal("Price"),
                            CardId = reader.GetInt32("CardId")
                        };
                        totalAmount += cartItem.Price;
                        cartItems.Add(cartItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке товаров из корзины: " + ex.Message);
                    return;
                }
            }

            // Добавляем заказ и товары в таблицу Orders
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Вставляем товары и заказ в таблицу Orders
                    string insertOrderQuery = @"
                        INSERT INTO Orders (UserId, CardId, Price, TotalAmount)
                        VALUES ((SELECT id FROM user WHERE username = @username), @cardId, @price, @totalAmount)";

                    foreach (var cartItem in cartItems)
                    {
                        MySqlCommand command = new MySqlCommand(insertOrderQuery, connection);
                        command.Parameters.AddWithValue("@username", currentUsername);
                        command.Parameters.AddWithValue("@cardId", cartItem.CardId);
                        command.Parameters.AddWithValue("@price", cartItem.Price);
                        command.Parameters.AddWithValue("@totalAmount", totalAmount);
                        command.ExecuteNonQuery();
                    }

                    // После оформления заказа, очистим корзину
                    string deleteCartItemsQuery = @"
                        DELETE FROM CartItems
                        WHERE UserId = (SELECT id FROM user WHERE username = @username)";
                    MySqlCommand deleteCommand = new MySqlCommand(deleteCartItemsQuery, connection);
                    deleteCommand.Parameters.AddWithValue("@username", currentUsername);
                    deleteCommand.ExecuteNonQuery();

                    MessageBox.Show($"Заказ оформлен!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем данные после оформления заказа
                    LoadCartItems();  // Очищаем корзину
                    LoadCardDetails(); // Обновляем данные карты
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message);
                }
            }
        }
    }
}

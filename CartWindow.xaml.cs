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

        private void LoadCartItems()
        {
            List<CartItem> cartItems = new List<CartItem>();
            decimal totalAmount = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT Cards.Title, Cards.Price, CartItems.CardId, CartItems.MaterialId, CartItems.MeasurementId, 
                               Materials.MaterialName, atelier_measurements.SetName
                        FROM CartItems
                        INNER JOIN Cards ON CartItems.CardId = Cards.CardId
                        LEFT JOIN Materials ON CartItems.MaterialId = Materials.MaterialId
                        LEFT JOIN atelier_measurements ON CartItems.MeasurementId = atelier_measurements.Id
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
                            MaterialName = reader.IsDBNull(reader.GetOrdinal("MaterialName")) ? null : reader.GetString("MaterialName"),
                            SetName = reader.IsDBNull(reader.GetOrdinal("SetName")) ? null : reader.GetString("SetName"),
                            CardId = reader.GetInt32("CardId"),
                            MaterialId = reader.IsDBNull(reader.GetOrdinal("MaterialId")) ? (int?)null : reader.GetInt32("MaterialId"),
                            MeasurementId = reader.IsDBNull(reader.GetOrdinal("MeasurementId")) ? (int?)null : reader.GetInt32("MeasurementId")
                        };
                        totalAmount += cartItem.Price;
                        cartItems.Add(cartItem);
                    }

                    TotalAmountText.Text = totalAmount.ToString("C");
                    decimal taxAmount = totalAmount * 0.05M;
                    TaxAmountText.Text = taxAmount.ToString("C");
                    FinalAmountText.Text = (totalAmount + taxAmount).ToString("C");

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

            DisplayCartItems(cartItems);
        }

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

        private void DisplayCartItems(List<CartItem> cartItems)
        {
            CartListView.ItemsSource = cartItems;
        }

        public class CartItem
        {
            public string Title { get; set; }
            public decimal Price { get; set; }
            public string MaterialName { get; set; }
            public string SetName { get; set; }
            public int CardId { get; set; }
            public int? MaterialId { get; set; }
            public int? MeasurementId { get; set; }
        }

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
                        LoadCartItems();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении товара из корзины: " + ex.Message);
                    }
                }
            }
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardDetailsPanel.Visibility == Visibility.Collapsed)
            {
                MessageBox.Show("Пожалуйста, добавьте карту для оформления заказа.");
                return;
            }

            decimal totalAmount = 0;
            List<CartItem> cartItems = new List<CartItem>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT Cards.Title, Cards.Price, CartItems.CardId, CartItems.MaterialId, CartItems.MeasurementId, 
                               Materials.MaterialName, atelier_measurements.SetName
                        FROM CartItems
                        INNER JOIN Cards ON CartItems.CardId = Cards.CardId
                        LEFT JOIN Materials ON CartItems.MaterialId = Materials.MaterialId
                        LEFT JOIN atelier_measurements ON CartItems.MeasurementId = atelier_measurements.Id
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
                            MaterialName = reader.IsDBNull(reader.GetOrdinal("MaterialName")) ? null : reader.GetString("MaterialName"),
                            SetName = reader.IsDBNull(reader.GetOrdinal("SetName")) ? null : reader.GetString("SetName"),
                            CardId = reader.GetInt32("CardId"),
                            MaterialId = reader.IsDBNull(reader.GetOrdinal("MaterialId")) ? (int?)null : reader.GetInt32("MaterialId"),
                            MeasurementId = reader.IsDBNull(reader.GetOrdinal("MeasurementId")) ? (int?)null : reader.GetInt32("MeasurementId")
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

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Удалить заказы, связанные с корзиной
                    string deleteOrdersQuery = @"
                        DELETE FROM Orders
                        WHERE UserId = (SELECT id FROM user WHERE username = @username)
                        AND CardId IN (SELECT CardId FROM CartItems WHERE UserId = (SELECT id FROM user WHERE username = @username))";
                    MySqlCommand deleteOrdersCommand = new MySqlCommand(deleteOrdersQuery, connection);
                    deleteOrdersCommand.Parameters.AddWithValue("@username", currentUsername);
                    deleteOrdersCommand.ExecuteNonQuery();

                    // Теперь удалить товары из корзины
                    string deleteCartItemsQuery = @"
                        DELETE FROM CartItems
                        WHERE UserId = (SELECT id FROM user WHERE username = @username)";
                    MySqlCommand deleteCartItemsCommand = new MySqlCommand(deleteCartItemsQuery, connection);
                    deleteCartItemsCommand.Parameters.AddWithValue("@username", currentUsername);
                    deleteCartItemsCommand.ExecuteNonQuery();

                    MessageBox.Show($"Заказ оформлен!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadCartItems();
                    LoadCardDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message);
                }
            }
        }
    }
}

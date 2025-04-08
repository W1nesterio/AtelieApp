using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Atelie
{
    public partial class CardDetailsWindow : Window
    {
        private string connectionString = "Server=localhost;Database=Atelie;User ID=root;Password=root;";
        private string currentUsername;
        private bool isCardLinked = false; // Флаг для проверки, привязана ли карта

        public CardDetailsWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            LoadCardDetails();
        }

        // Метод для загрузки данных карты пользователя
        private void LoadCardDetails()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT CardNumber, ExpiryDate, CardHolderName FROM UserCards WHERE UserId = (SELECT id FROM user WHERE username = @username)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", currentUsername);
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isCardLinked = true; // Карта привязана
                        string cardNumber = reader.GetString("CardNumber");
                        string formattedCardNumber = FormatCardNumber(cardNumber); // Форматируем номер карты

                        CardNumberTextBox.Text = formattedCardNumber;
                        ExpiryDateTextBox.Text = reader.GetDateTime("ExpiryDate").ToString("MM/yy");
                        CardHolderNameTextBox.Text = reader.GetString("CardHolderName");

                        // Меняем текст кнопки на "Отвязать карту"
                        SaveCardDetailsButton.Content = "Отвязать карту";

                        // Делаем поля только для чтения, если карта привязана
                        SetFieldsReadOnly(true);
                    }
                    else
                    {
                        // Если карты нет, кнопка "Сохранить"
                        SaveCardDetailsButton.Content = "Сохранить";
                        SetFieldsReadOnly(false); // Делаем поля доступными для ввода
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке реквизитов: " + ex.Message);
                }
            }
        }

       
        private string FormatCardNumber(string cardNumber)
        {
            return string.Join(" ", cardNumber.ToCharArray()
                .Select((c, i) => i % 4 == 0 && i != 0 ? " " + c : c.ToString()));
        }

      
        private void SetFieldsReadOnly(bool isReadOnly)
        {
            CardNumberTextBox.IsReadOnly = isReadOnly;
            ExpiryDateTextBox.IsReadOnly = isReadOnly;
            CardHolderNameTextBox.IsReadOnly = isReadOnly;
        }

       
        private void SaveCardDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            string cardNumber = CardNumberTextBox.Text.Replace(" ", ""); 
            string expiryDate = ExpiryDateTextBox.Text;
            string cardHolderName = CardHolderNameTextBox.Text;

            if (isCardLinked)
            {
          
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите отвязать карту?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            string deleteQuery = "DELETE FROM UserCards WHERE UserId = (SELECT id FROM user WHERE username = @username)";
                            MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);
                            deleteCommand.Parameters.AddWithValue("@username", currentUsername);
                            deleteCommand.ExecuteNonQuery();

                            MessageBox.Show("Карта отвязана.");
                        }
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при отвязке карты: " + ex.Message);
                    }
                }
            }
            else
            {
          
                if (!Regex.IsMatch(cardNumber, @"^\d{16}$"))
                {
                    MessageBox.Show("Номер карты должен состоять из 16 цифр.");
                    return;
                }

                if (!Regex.IsMatch(expiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    MessageBox.Show("Дата истечения должна быть в формате MM/YY (например, 01/23).");
                    return;
                }

                if (!Regex.IsMatch(cardHolderName, @"^[A-Z\s]+$"))
                {
                    MessageBox.Show("Имя держателя должно быть написано заглавными буквами.");
                    return;
                }

             
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO UserCards (UserId, CardNumber, ExpiryDate, CardHolderName) " +
                                             "VALUES ((SELECT id FROM user WHERE username = @username), @cardNumber, @expiryDate, @cardHolderName)";
                        MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@username", currentUsername);
                        insertCommand.Parameters.AddWithValue("@cardNumber", cardNumber);
                        insertCommand.Parameters.AddWithValue("@expiryDate", DateTime.ParseExact(expiryDate, "MM/yy", null));
                        insertCommand.Parameters.AddWithValue("@cardHolderName", cardHolderName);
                        insertCommand.ExecuteNonQuery();

                        MessageBox.Show("Реквизиты успешно сохранены!");
                    }
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении реквизитов: " + ex.Message);
                }
            }
        }

        private void CardNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = CardNumberTextBox.Text.Replace(" ", ""); 
            if (text.Length > 16)
            {
                text = text.Substring(0, 16); 
            }

            text = string.Join(" ", Regex.Matches(text, @"(\d{1,4})")
                .Cast<Match>()
                .Select(m => m.Value));


            CardNumberTextBox.Text = text;


            CardNumberTextBox.SelectionStart = CardNumberTextBox.Text.Length;
        }

    }
}

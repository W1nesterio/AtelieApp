using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Atelie
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString = "Server=localhost;Database=Atelie;Uid=root;Pwd=root;"; 

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Registration registrationWindow = new Registration();
            registrationWindow.Show();
            this.Close();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string checkUserQuery = "SELECT password FROM user WHERE username = @username";
                using (var cmd = new MySqlCommand(checkUserQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    var dbPassword = cmd.ExecuteScalar()?.ToString();

                    if (dbPassword == null)
                    {
                        MessageBox.Show("Пользователь с таким логином не найден.");
                        return;
                    }

                    string hashedPassword = HashPassword(password);

                    if (dbPassword == hashedPassword)
                    {
                        Showcase showcaseWindow = new Showcase();
                        showcaseWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный пароль.");
                    }
                }
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}

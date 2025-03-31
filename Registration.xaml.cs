using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Atelie
{
    public partial class Registration : Window
    {
        private readonly string connectionString = "Server=localhost;Port=3306;Database=Atelie;Uid=root;Pwd=root;";

        public Registration()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми.");
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        string checkUserQuery = "SELECT COUNT(*) FROM user WHERE username = @username";
                        using (var cmd = new MySqlCommand(checkUserQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Transaction = transaction;

                            int userCount = Convert.ToInt32(cmd.ExecuteScalar());
                            if (userCount > 0)
                            {
                                MessageBox.Show("Пользователь с таким логином уже существует.");
                                return;
                            }
                        }

                        string hashedPassword = HashPassword(password);

                        string insertQuery = "INSERT INTO user (username, password) VALUES (@username, @password)";
                        using (var cmd = new MySqlCommand(insertQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password", hashedPassword);
                            cmd.Transaction = transaction;

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Регистрация прошла успешно!");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}");
                    }
                }


                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении к базе данных: {ex.Message}");
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

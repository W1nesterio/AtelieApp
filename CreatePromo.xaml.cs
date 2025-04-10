using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;

namespace Atelie
{
    public partial class CreatePromo : Window
    {
        string connectionString = "server=localhost;user=root;password=root;database=Atelie;";

        public CreatePromo()
        {
            InitializeComponent();
            LoadMeasurements();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string setName = SetNameBox.Text.Trim();
            double waist = double.TryParse(WaistBox.Text, out double w) ? w : 0;
            double chest = double.TryParse(ChestBox.Text, out double c) ? c : 0;
            double hips = double.TryParse(HipsBox.Text, out double h) ? h : 0;
            double shoulders = double.TryParse(ShouldersBox.Text, out double s) ? s : 0;
            double neck = double.TryParse(NeckBox.Text, out double n) ? n : 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO atelier_measurements (SetName, Waist, Chest, Hips, Shoulders, Neck) " +
                               "VALUES (@SetName, @Waist, @Chest, @Hips, @Shoulders, @Neck)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@SetName", setName);
                cmd.Parameters.AddWithValue("@Waist", waist);
                cmd.Parameters.AddWithValue("@Chest", chest);
                cmd.Parameters.AddWithValue("@Hips", hips);
                cmd.Parameters.AddWithValue("@Shoulders", shoulders);
                cmd.Parameters.AddWithValue("@Neck", neck);
                cmd.ExecuteNonQuery();
            }

            ClearInputs();
            LoadMeasurements();
        }

        private void LoadMeasurements()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM atelier_measurements";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                DataGridMeasurements.ItemsSource = table.DefaultView;
            }
        }

        private void ClearInputs()
        {
            SetNameBox.Text = "";
            WaistBox.Text = "";
            ChestBox.Text = "";
            HipsBox.Text = "";
            ShouldersBox.Text = "";
            NeckBox.Text = "";
        }
    }
}

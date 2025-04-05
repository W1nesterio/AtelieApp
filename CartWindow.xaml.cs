using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Atelie
{
    public partial class CartWindow : Window
    {
        public ObservableCollection<CartItem> CartItems { get; set; }

        public CartWindow()
        {
            InitializeComponent();
            CartItems = new ObservableCollection<CartItem>
            {
                
            };
            CartListView.ItemsSource = CartItems;
        }

        // Обработчик для удаления товара из корзины
        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.DataContext as CartItem;
            if (item != null)
            {
                CartItems.Remove(item);
            }
        }

        // Обработчик для оформления заказа
        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ваш заказ оформлен!");
            this.Close();  // Закрытие окна корзины после оформления заказа
        }
    }

    public class CartItem
    {
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
    }
}

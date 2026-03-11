using System.Windows;
using RPM.Pages; // Подключаем новые View

namespace RPM
{
    public partial class CashierWindow : Window
    {
        public CashierWindow()
        {
            InitializeComponent();
            // Можно сразу открыть Новый заказ при старте
            MainContent.Content = new NewOrderView();
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new NewOrderView();
        }

        private void OrdersList_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new OrdersView();
        }

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			new LoginWindow().Show();
			this.Close();
		}
	}
}

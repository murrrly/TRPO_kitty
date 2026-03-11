using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RPM.Pages;

namespace RPM
{
	public partial class AdminWindow : Window
	{
		private Button _selectedButton;

		public AdminWindow()
		{
			InitializeComponent();
		}

		private void SetSelectedButton(Button btn)
		{
			if (_selectedButton != null && _selectedButton != btn)
			{
				// Сброс старой кнопки
				_selectedButton.Background = (_selectedButton.Content.ToString() == "Выход")
					? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E84C34"))
					: new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1D8"));
			}

			_selectedButton = btn;

			if (_selectedButton.Content.ToString() != "Выход")
			{
				_selectedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
			}
		}

		private void Orders_Click(object sender, RoutedEventArgs e)
		{
			SetSelectedButton(sender as Button);
			MainContent.Content = new OrdersView();
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			SetSelectedButton(sender as Button);
			MainContent.Content = new MenuView();
		}

		private void Users_Click(object sender, RoutedEventArgs e)
		{
			SetSelectedButton(sender as Button);
			MainContent.Content = new UsersView();
		}

		private void Stats_Click(object sender, RoutedEventArgs e)
		{
			SetSelectedButton(sender as Button);
			MainContent.Content = new StatisticsView();
		}

		private void Promo_Click(object sender, RoutedEventArgs e)
		{
			SetSelectedButton(sender as Button);
			MainContent.Content = new PromotionsView();
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			new LoginWindow().Show();
			this.Close();
		}
	}
}

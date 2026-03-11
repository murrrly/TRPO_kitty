using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace RPM.Pages
{
	public partial class AdvertisementsView : UserControl
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		DataTable adsTable = new DataTable();

		public AdvertisementsView()
		{
			InitializeComponent();
			LoadAds();
		}

		private void LoadAds()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT ID, ImageURL, Title, Description, IsActive FROM Advertisements ORDER BY ID DESC";
					MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
					adsTable.Clear();
					adapter.Fill(adsTable);
					AdsDataGrid.ItemsSource = adsTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке рекламы: " + ex.Message);
			}
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			AdvertisementEditWindow win = new AdvertisementEditWindow();
			if (win.ShowDialog() == true)
				LoadAds();
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			if (AdsDataGrid.SelectedItem == null) return;

			DataRowView row = (DataRowView)AdsDataGrid.SelectedItem;
			int id = Convert.ToInt32(row["ID"]);

			if (MessageBox.Show("Удалить выбранную рекламу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				try
				{
					using (MySqlConnection conn = new MySqlConnection(connectionString))
					{
						conn.Open();
						string query = "DELETE FROM Advertisements WHERE ID=@id";
						MySqlCommand cmd = new MySqlCommand(query, conn);
						cmd.Parameters.AddWithValue("@id", id);
						cmd.ExecuteNonQuery();
						LoadAds();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Ошибка при удалении рекламы: " + ex.Message);
				}
			}
		}
	}
}

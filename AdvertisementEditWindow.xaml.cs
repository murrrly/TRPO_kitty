using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace RPM
{
	public partial class AdvertisementEditWindow : Window
	{
		string connectionString = "server=localhost;user=root;password=;database=catcafe_db;";

		public AdvertisementEditWindow()
		{
			InitializeComponent();
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			string title = TitleTextBox.Text.Trim();
			string desc = DescriptionTextBox.Text.Trim();
			string url = ImageUrlTextBox.Text.Trim();
			bool isActive = IsActiveCheckBox.IsChecked ?? false;

			if (string.IsNullOrEmpty(url))
			{
				MessageBox.Show("Введите URL картинки");
				return;
			}

			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = @"INSERT INTO Advertisements (Title, Description, ImageURL, IsActive) 
                                     VALUES (@title,@desc,@url,@active)";
					MySqlCommand cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@title", title);
					cmd.Parameters.AddWithValue("@desc", desc);
					cmd.Parameters.AddWithValue("@url", url);
					cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);
					cmd.ExecuteNonQuery();

					MessageBox.Show("Реклама добавлена");
					DialogResult = true;
					Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при добавлении рекламы: " + ex.Message);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

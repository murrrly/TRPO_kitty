using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace RPM
{
	public partial class MenuEditWindow : Window
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		int menuId = 0; // 0 = новый, >0 = редактировать

		public MenuEditWindow(int id = 0)
		{
			InitializeComponent();
			menuId = id;
			if (menuId > 0)
				LoadMenu();
		}

		private void LoadMenu()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT * FROM Menu WHERE ID=@id";
					MySqlCommand cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@id", menuId);
					var reader = cmd.ExecuteReader();
					if (reader.Read())
					{
						NameTextBox.Text = reader["Name"].ToString();
						CompositionTextBox.Text = reader["Composition"].ToString();
						DescriptionTextBox.Text = reader["Description"].ToString();
						PriceTextBox.Text = reader["Price"].ToString();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке: " + ex.Message);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			string name = NameTextBox.Text.Trim();
			string comp = CompositionTextBox.Text.Trim();
			string desc = DescriptionTextBox.Text.Trim();
			if (!decimal.TryParse(PriceTextBox.Text.Trim(), out decimal price))
			{
				MessageBox.Show("Некорректная цена");
				return;
			}

			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query;
					if (menuId == 0)
					{
						query = "INSERT INTO Menu (Name, Composition, Description, Price) VALUES (@name,@comp,@desc,@price)";
					}
					else
					{
						query = "UPDATE Menu SET Name=@name, Composition=@comp, Description=@desc, Price=@price WHERE ID=@id";
					}

					MySqlCommand cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@name", name);
					cmd.Parameters.AddWithValue("@comp", comp);
					cmd.Parameters.AddWithValue("@desc", desc);
					cmd.Parameters.AddWithValue("@price", price);
					if (menuId > 0)
						cmd.Parameters.AddWithValue("@id", menuId);

					cmd.ExecuteNonQuery();
				}

				DialogResult = true;
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при сохранении: " + ex.Message);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

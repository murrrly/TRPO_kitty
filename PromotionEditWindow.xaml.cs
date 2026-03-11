using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace RPM
{
	public partial class PromotionEditWindow : Window
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		DataTable menuTable = new DataTable();

		public PromotionEditWindow()
		{
			InitializeComponent();
			LoadMenu();
			StartDatePicker.SelectedDate = DateTime.Today;
			EndDatePicker.SelectedDate = DateTime.Today.AddDays(7);
		}

		private void LoadMenu()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT ID, Name FROM Menu";
					MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
					menuTable.Clear();
					adapter.Fill(menuTable);

					MenuListBox.Items.Clear();
					foreach (DataRow row in menuTable.Rows)
					{
						MenuListBox.Items.Add(new MenuItem
						{
							ID = Convert.ToInt32(row["ID"]),
							Name = row["Name"].ToString()
						});
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке меню: " + ex.Message);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			string name = NameTextBox.Text.Trim();
			string desc = DescriptionTextBox.Text.Trim();
			DateTime start = StartDatePicker.SelectedDate ?? DateTime.Today;
			DateTime end = EndDatePicker.SelectedDate ?? DateTime.Today;
			if (!decimal.TryParse(DiscountPriceTextBox.Text.Trim(), out decimal discountPrice))
			{
				MessageBox.Show("Некорректная цена по акции");
				return;
			}

			if (MenuListBox.SelectedItems.Count == 0)
			{
				MessageBox.Show("Выберите хотя бы одно блюдо");
				return;
			}

			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					MySqlTransaction transaction = conn.BeginTransaction();

					// Вставляем акцию
					string insertPromo = @"
                        INSERT INTO Promotions (Name, Description, StartDate, EndDate, DiscountPrice)
                        VALUES (@name,@desc,@start,@end,@price);
                        SELECT LAST_INSERT_ID();";

					MySqlCommand cmd = new MySqlCommand(insertPromo, conn, transaction);
					cmd.Parameters.AddWithValue("@name", name);
					cmd.Parameters.AddWithValue("@desc", desc);
					cmd.Parameters.AddWithValue("@start", start);
					cmd.Parameters.AddWithValue("@end", end);
					cmd.Parameters.AddWithValue("@price", discountPrice);

					long promoId = Convert.ToInt64(cmd.ExecuteScalar());

					// Вставляем блюда акции
					foreach (MenuItem item in MenuListBox.SelectedItems)
					{
						string insertMenu = "INSERT INTO PromotionMenu (PromotionID, MenuID) VALUES (@promo,@menu)";
						MySqlCommand menuCmd = new MySqlCommand(insertMenu, conn, transaction);
						menuCmd.Parameters.AddWithValue("@promo", promoId);
						menuCmd.Parameters.AddWithValue("@menu", item.ID);
						menuCmd.ExecuteNonQuery();
					}

					transaction.Commit();
					MessageBox.Show("Акция создана успешно!");
					DialogResult = true;
					Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при создании акции: " + ex.Message);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}

	class MenuItem
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public override string ToString() => Name;
	}
}

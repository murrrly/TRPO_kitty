using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace RPM.Pages
{
	public partial class PromotionsView : UserControl
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		DataTable promotionsTable = new DataTable();
		DataTable promotionsTableOriginal = new DataTable(); // копия для отката

		public PromotionsView()
		{
			InitializeComponent();
			LoadPromotions();
		}

		private void LoadPromotions()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT ID, Name, Description, StartDate, EndDate, DiscountPrice FROM Promotions ORDER BY StartDate DESC";
					MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);

					promotionsTable.Clear();
					adapter.Fill(promotionsTable);

					// сохраняем копию исходных данных
					promotionsTableOriginal = promotionsTable.Copy();

					PromotionsDataGrid.ItemsSource = promotionsTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке акций: " + ex.Message);
			}
		}

		private void CreateButton_Click(object sender, RoutedEventArgs e)
		{
			PromotionEditWindow win = new PromotionEditWindow();
			if (win.ShowDialog() == true)
			{
				LoadPromotions();
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var changesList = new List<string>();

				foreach (DataRow row in promotionsTable.Rows)
				{
					if (row.RowState == DataRowState.Modified)
					{
						foreach (DataColumn col in promotionsTable.Columns)
						{
							var oldVal = row[col, DataRowVersion.Original]?.ToString();
							var newVal = row[col, DataRowVersion.Current]?.ToString();

							if (!Equals(oldVal, newVal))
								changesList.Add($"ID {row["ID"]}: {col.ColumnName} \"{oldVal}\" → \"{newVal}\"");
						}
					}
				}

				if (changesList.Count == 0)
				{
					MessageBox.Show("Нет изменений для сохранения.");
					return;
				}

				// Подтверждение
				string message = "Вы хотите сохранить следующие изменения?\n\n" + string.Join("\n", changesList);
				var result = MessageBox.Show(message, "Подтверждение сохранения", MessageBoxButton.YesNo, MessageBoxImage.Question);

				if (result != MessageBoxResult.Yes)
				{
					// Откат изменений
					promotionsTable.Clear();
					promotionsTable.Merge(promotionsTableOriginal);
					PromotionsDataGrid.ItemsSource = promotionsTable.DefaultView;
					MessageBox.Show("Изменения отменены.");
					return;
				}

				// Сохраняем изменения в БД
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();

					foreach (DataRow row in promotionsTable.Rows)
					{
						if (row.RowState == DataRowState.Modified)
						{
							string updateQuery = @"UPDATE Promotions 
                                                   SET Name=@Name, Description=@Description, StartDate=@StartDate, EndDate=@EndDate, DiscountPrice=@DiscountPrice 
                                                   WHERE ID=@ID";

							MySqlCommand cmd = new MySqlCommand(updateQuery, conn);
							cmd.Parameters.AddWithValue("@Name", row["Name"]);
							cmd.Parameters.AddWithValue("@Description", row["Description"]);
							cmd.Parameters.AddWithValue("@StartDate", row["StartDate"]);
							cmd.Parameters.AddWithValue("@EndDate", row["EndDate"]);
							cmd.Parameters.AddWithValue("@DiscountPrice", row["DiscountPrice"]);
							cmd.Parameters.AddWithValue("@ID", row["ID"]);
							cmd.ExecuteNonQuery();

							row.AcceptChanges();
						}
					}
				}

				// Обновляем копию после успешного сохранения
				promotionsTableOriginal = promotionsTable.Copy();

				MessageBox.Show("Изменения успешно сохранены!");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при сохранении: " + ex.Message);
			}
		}
	}
}
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace RPM.Pages
{
	public partial class UsersView : UserControl
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		DataTable usersTable = new DataTable();

		public UsersView()
		{
			InitializeComponent();
			LoadUsers();
		}

		private void LoadUsers()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = @"
                        SELECT u.ID, u.FullName, u.Login, p.PositionName, u.IsActive, u.RegistrationDate
                        FROM Users u
                        LEFT JOIN Positions p ON u.PositionID = p.ID
                        ORDER BY u.ID";
					MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
					usersTable.Clear();
					adapter.Fill(usersTable);
					UsersDataGrid.ItemsSource = usersTable.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке сотрудников: " + ex.Message);
			}
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			UsersEditWindow win = new UsersEditWindow();
			if (win.ShowDialog() == true)
				LoadUsers();
		}

		private void ChangeRoleButton_Click(object sender, RoutedEventArgs e)
		{
			if (UsersDataGrid.SelectedItem == null) return;

			DataRowView row = (DataRowView)UsersDataGrid.SelectedItem;
			int id = Convert.ToInt32(row["ID"]);

			UsersEditWindow win = new UsersEditWindow(id);
			if (win.ShowDialog() == true)
				LoadUsers();
		}

		private void BlockButton_Click(object sender, RoutedEventArgs e)
		{
			if (UsersDataGrid.SelectedItem == null) return;

			DataRowView row = (DataRowView)UsersDataGrid.SelectedItem;
			int id = Convert.ToInt32(row["ID"]);
			bool isActive = Convert.ToBoolean(row["IsActive"]);

			string newStatus = isActive ? "0" : "1";

			if (MessageBox.Show(isActive ? "Заблокировать пользователя?" : "Разблокировать пользователя?",
				"Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				try
				{
					using (MySqlConnection conn = new MySqlConnection(connectionString))
					{
						conn.Open();
						string query = "UPDATE Users SET IsActive=@status WHERE ID=@id";
						MySqlCommand cmd = new MySqlCommand(query, conn);
						cmd.Parameters.AddWithValue("@status", newStatus);
						cmd.Parameters.AddWithValue("@id", id);
						cmd.ExecuteNonQuery();
						LoadUsers();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Ошибка при обновлении статуса: " + ex.Message);
				}
			}
		}
	}
}

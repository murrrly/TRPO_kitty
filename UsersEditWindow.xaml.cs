using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace RPM
{
	public partial class UsersEditWindow : Window
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		int userId = 0;

		public UsersEditWindow(int id = 0)
		{
			InitializeComponent();
			userId = id;
			LoadPositions();

			if (userId > 0)
				LoadUser();
		}

		private void LoadPositions()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT ID, PositionName FROM Positions";
					MySqlCommand cmd = new MySqlCommand(query, conn);

					var positions = new List<Position>();

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							positions.Add(new Position
							{
								ID = Convert.ToInt32(reader["ID"]),
								PositionName = reader["PositionName"].ToString()
							});
						}
					}

					PositionCombo.ItemsSource = positions;
					PositionCombo.DisplayMemberPath = "PositionName";
					PositionCombo.SelectedValuePath = "ID";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке ролей: " + ex.Message);
			}
		}

		private void LoadUser()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT FullName, Login, PositionID FROM Users WHERE ID=@id";
					MySqlCommand cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@id", userId);
					var reader = cmd.ExecuteReader();
					if (reader.Read())
					{
						FullNameTextBox.Text = reader["FullName"].ToString();
						LoginTextBox.Text = reader["Login"].ToString();
						PositionCombo.SelectedValue = Convert.ToInt32(reader["PositionID"]);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке пользователя: " + ex.Message);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (PositionCombo.SelectedItem == null)
			{
				RoleErrorText.Visibility = Visibility.Visible;
				PositionCombo.Style = (Style)FindResource("ErrorComboStyle");
				return;
			}

			int positionId = Convert.ToInt32(PositionCombo.SelectedValue);
			string fullName = FullNameTextBox.Text.Trim();
			string login = LoginTextBox.Text.Trim();

			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query;

					if (userId == 0)
					{
						// Добавляем нового пользователя
						query = "INSERT INTO Users (FullName, Login, Password, PositionID) VALUES (@full,@login,'1234',@pos)";
					}
					else
					{
						// Редактируем существующего
						query = "UPDATE Users SET FullName=@full, Login=@login, PositionID=@pos WHERE ID=@id";
					}

					MySqlCommand cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@full", fullName);
					cmd.Parameters.AddWithValue("@login", login);
					cmd.Parameters.AddWithValue("@pos", positionId);
					if (userId > 0)
						cmd.Parameters.AddWithValue("@id", userId);

					cmd.ExecuteNonQuery();
				}

				RoleErrorText.Visibility = Visibility.Collapsed;
				PositionCombo.ClearValue(StyleProperty);

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

		public class Position
		{
			public int ID { get; set; }
			public string PositionName { get; set; }
			public override string ToString() => PositionName;
		}
	}
}
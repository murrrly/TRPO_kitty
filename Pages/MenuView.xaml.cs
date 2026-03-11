using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace RPM.Pages
{
	public partial class MenuView : UserControl
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		DataTable menuTable = new DataTable();
		private List<MenuItem> AllItems;

		public MenuView()
		{
			InitializeComponent();
			LoadMenu();
		}

		private void LoadMenu()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();

					string query = @"
                        SELECT m.ID, m.Name, m.Composition, m.Description, m.Price, m.ImageURL, c.Name AS CategoryName
                        FROM Menu m
                        LEFT JOIN Categories c ON m.CategoryID = c.ID";
					MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
					menuTable.Clear();
					adapter.Fill(menuTable);

					AllItems = menuTable.AsEnumerable().Select(r => new MenuItem
					{
						ID = Convert.ToInt32(r["ID"]),
						Name = r["Name"].ToString(),
						Composition = r["Composition"].ToString(),
						Description = r["Description"].ToString(),
						Price = Convert.ToDecimal(r["Price"]),
						ImageURL = GetAbsolutePath("/Images/" + r["ImageURL"].ToString()),
						CategoryName = r["CategoryName"] == DBNull.Value ? "Без категории" : r["CategoryName"].ToString()
					}).ToList();

					ItemsControlMenu.ItemsSource = AllItems;
					InitializeCategoryButtons();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке меню: " + ex.Message);
			}
		}

		private string GetAbsolutePath(string path)
		{
			if (Path.IsPathRooted(path))
				return path;

			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			MenuEditWindow win = new MenuEditWindow();
			if (win.ShowDialog() == true)
				LoadMenu();
		}

		private void EditItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.DataContext is MenuItem item)
			{
				MenuEditWindow win = new MenuEditWindow(item.ID);
				if (win.ShowDialog() == true)
					LoadMenu();
			}
		}

		private void DeleteItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn && btn.DataContext is MenuItem item)
			{
				if (MessageBox.Show("Удалить выбранное блюдо?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						using (MySqlConnection conn = new MySqlConnection(connectionString))
						{
							conn.Open();
							string query = "DELETE FROM Menu WHERE ID=@id";
							MySqlCommand cmd = new MySqlCommand(query, conn);
							cmd.Parameters.AddWithValue("@id", item.ID);
							cmd.ExecuteNonQuery();
							LoadMenu();
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show("Ошибка при удалении: " + ex.Message);
					}
				}
			}
		}

		private void InitializeCategoryButtons()
		{
			CategoryPanel.Children.Clear();

			var categories = AllItems.Select(i => i.CategoryName).Distinct().ToList();
			categories.Insert(0, "Все");

			foreach (var cat in categories)
			{
				var btn = new Button
				{
					Content = cat,
					Width = 100,
					Height = 32,
					Margin = new Thickness(4, 0, 4, 0),
					Background = Brushes.Transparent,
					BorderBrush = Brushes.Gray,
					BorderThickness = new Thickness(0),
					Tag = cat
				};
				btn.Click += FilterButton_Click;
				CategoryPanel.Children.Add(btn);

			}

			if (CategoryPanel.Children.Count > 0 && CategoryPanel.Children[0] is Button first)
				first.Background = Brushes.White;
		}

		private void FilterButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button btn)
			{
				string category = btn.Tag.ToString();

				foreach (Button b in CategoryPanel.Children)
					b.Background = Brushes.Transparent;

				btn.Background = Brushes.White;

				ApplyFilter(category, SearchBox.Text.Trim());
			}
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string searchText = SearchBox.Text.Trim();
			string selectedCategory = CategoryPanel.Children
										 .OfType<Button>()
										 .FirstOrDefault(b => b.Background == Brushes.White)?.Tag.ToString() ?? "Все";

			ApplyFilter(selectedCategory, searchText);
		}

		private void ApplyFilter(string category, string searchText)
		{
			var filtered = AllItems.AsEnumerable();

			if (!string.IsNullOrEmpty(searchText))
				filtered = filtered.Where(i => i.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

			if (category != "Все")
				filtered = filtered.Where(i => i.CategoryName == category);

			ItemsControlMenu.ItemsSource = filtered.ToList();
		}
	}

	public class MenuItem
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Composition { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string ImageURL { get; set; }
		public string CategoryName { get; set; }
	}
}

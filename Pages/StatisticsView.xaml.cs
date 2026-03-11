using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace RPM.Pages
{
	public partial class StatisticsView : UserControl
	{
		string connectionString = "server=localhost;user=root;password=cat12345;database=catcafe_db;";
		private int? SelectedBranchID = null;
		public ObservableCollection<BranchViewModel> Branches { get; set; } = new ObservableCollection<BranchViewModel>();

		public StatisticsView()
		{
			InitializeComponent();
			LoadBranches();
			LoadStatistics();
		}

		private void LoadBranches()
		{
			Branches.Clear();
			try
			{
				using (var conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					string query = "SELECT ID, Address FROM Branches ORDER BY Address";
					using (var cmd = new MySqlCommand(query, conn))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Branches.Add(new BranchViewModel
							{
								ID = reader.GetInt32("ID"),
								Address = reader["Address"].ToString()
							});
						}
					}
				}

				BranchFilterComboBox.ItemsSource = Branches;
				BranchFilterComboBox.SelectedIndex = -1; // "Все филиалы"
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке филиалов: " + ex.Message);
			}
		}

		private void BranchFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (BranchFilterComboBox.SelectedItem is BranchViewModel branch)
				SelectedBranchID = branch.ID;
			else
				SelectedBranchID = null;

			LoadStatistics();
		}

		private void LoadStatistics()
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(connectionString))
				{
					conn.Open();

					// Выручка за день
					string queryDay = @"
                        SELECT IFNULL(SUM(TotalAmount),0) 
                        FROM Orders o
                        JOIN OrderMenu om ON om.OrderID = o.ID
                        WHERE DATE(o.OrderDate) = CURDATE()
                        " + (SelectedBranchID.HasValue ? " AND om.BranchID=@branchID" : "");
					MySqlCommand cmdDay = new MySqlCommand(queryDay, conn);
					if (SelectedBranchID.HasValue)
						cmdDay.Parameters.AddWithValue("@branchID", SelectedBranchID.Value);
					RevenueDayText.Text = cmdDay.ExecuteScalar().ToString() + " ₽";

					// Выручка за месяц
					string queryMonth = @"
                        SELECT IFNULL(SUM(TotalAmount),0) 
                        FROM Orders o
                        JOIN OrderMenu om ON om.OrderID = o.ID
                        WHERE MONTH(o.OrderDate)=MONTH(CURDATE()) AND YEAR(o.OrderDate)=YEAR(CURDATE())
                        " + (SelectedBranchID.HasValue ? " AND om.BranchID=@branchID" : "");
					MySqlCommand cmdMonth = new MySqlCommand(queryMonth, conn);
					if (SelectedBranchID.HasValue)
						cmdMonth.Parameters.AddWithValue("@branchID", SelectedBranchID.Value);
					RevenueMonthText.Text = cmdMonth.ExecuteScalar().ToString() + " ₽";

					// Количество заказов
					string queryOrders = @"
                        SELECT COUNT(DISTINCT o.ID) 
                        FROM Orders o
                        JOIN OrderMenu om ON om.OrderID = o.ID
                        WHERE MONTH(o.OrderDate)=MONTH(CURDATE()) AND YEAR(o.OrderDate)=YEAR(CURDATE())
                        " + (SelectedBranchID.HasValue ? " AND om.BranchID=@branchID" : "");
					MySqlCommand cmdOrders = new MySqlCommand(queryOrders, conn);
					if (SelectedBranchID.HasValue)
						cmdOrders.Parameters.AddWithValue("@branchID", SelectedBranchID.Value);
					OrdersCountText.Text = cmdOrders.ExecuteScalar().ToString();

					// Средний чек
					string queryAvg = @"
                        SELECT IFNULL(AVG(TotalAmount),0) 
                        FROM Orders o
                        JOIN OrderMenu om ON om.OrderID = o.ID
                        WHERE MONTH(o.OrderDate)=MONTH(CURDATE()) AND YEAR(o.OrderDate)=YEAR(CURDATE())
                        " + (SelectedBranchID.HasValue ? " AND om.BranchID=@branchID" : "");
					MySqlCommand cmdAvg = new MySqlCommand(queryAvg, conn);
					if (SelectedBranchID.HasValue)
						cmdAvg.Parameters.AddWithValue("@branchID", SelectedBranchID.Value);
					AverageCheckText.Text = Convert.ToDecimal(cmdAvg.ExecuteScalar()).ToString("N2") + " ₽";

					// Топ блюд
					string queryTop = @"
                        SELECT m.Name, SUM(om.Quantity) AS Quantity
                        FROM OrderMenu om
                        JOIN Menu m ON om.MenuID = m.ID
                        JOIN Orders o ON om.OrderID = o.ID
                        WHERE 1=1 " + (SelectedBranchID.HasValue ? " AND om.BranchID=@branchID" : "") + @"
                        GROUP BY om.MenuID
                        ORDER BY Quantity DESC
                        LIMIT 10";
					MySqlCommand cmdTop = new MySqlCommand(queryTop, conn);
					if (SelectedBranchID.HasValue)
						cmdTop.Parameters.AddWithValue("@branchID", SelectedBranchID.Value);

					MySqlDataAdapter adapter = new MySqlDataAdapter(cmdTop);
					DataTable dt = new DataTable();
					adapter.Fill(dt);
					TopMenuDataGrid.ItemsSource = dt.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при загрузке статистики: " + ex.Message);
			}
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			LoadStatistics();
		}
	}

	public class BranchViewModel
	{
		public int ID { get; set; }
		public string Address { get; set; }
		public override string ToString() => Address;
	}
}

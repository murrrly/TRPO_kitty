using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace RPM.Pages
{
	public partial class OrdersView : UserControl
	{
		public ObservableCollection<OrderViewModel> Orders { get; set; } = new ObservableCollection<OrderViewModel>();
		public ObservableCollection<BranchViewModel> Branches { get; set; } = new ObservableCollection<BranchViewModel>();

		private string connectionString = "server=localhost;database=catcafe_db;uid=root;pwd=cat12345;charset=utf8mb4;";
		private int? SelectedBranchID = null; // выбранный филиал

		public OrdersView()
		{
			InitializeComponent();
			DataContext = this; // Важно для привязки ItemsSource

			LoadBranches(); // Загружаем филиалы
			LoadOrders();   // Загружаем заказы
		}

		// ===============================
		// Загрузка филиалов
		// ===============================
		private void LoadBranches()
		{
			Branches.Clear();

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

			// НЕ назначаем ItemsSource вручную, используем привязку
			// BranchComboBox.ItemsSource = Branches;
		}

		// ===============================
		// Событие выбора филиала
		// ===============================
		private void BranchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (BranchComboBox.SelectedItem is BranchViewModel selectedBranch)
			{
				SelectedBranchID = selectedBranch.ID;
				LoadOrders();
			}
			else
			{
				SelectedBranchID = null;
				LoadOrders();
			}
		}



		// ===============================
		// Модель филиала
		// ===============================
		public class BranchViewModel
		{
			public int ID { get; set; }
			public string Address { get; set; }

			// ВАЖНО: чтобы ComboBox показывал Address, а не имя класса
			public override string ToString() => Address;
		}



		// ===============================
		// Кнопка "Обновить"
		// ===============================
		private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			LoadOrders();
		}

		// ===============================
		// Загрузка заказов и их позиций
		// ===============================
		private void LoadOrders()
		{
			Orders.Clear();

			using (var conn = new MySqlConnection(connectionString))
			{
				conn.Open();

				// -------------------------------
				// Загружаем заказы с фильтром по филиалу
				// -------------------------------
				string ordersQuery = @"
                    SELECT DISTINCT o.ID, o.OrderNumber, o.OrderDate, o.TotalAmount, o.PaymentType,
                                    u.FullName AS CashierName
                    FROM Orders o
                    LEFT JOIN Users u ON o.CashierUserID = u.ID
                    LEFT JOIN OrderMenu om ON om.OrderID = o.ID
                    WHERE (@BranchID IS NULL OR om.BranchID = @BranchID)
                    ORDER BY o.OrderDate DESC;
                ";

				using (var cmd = new MySqlCommand(ordersQuery, conn))
				{
					cmd.Parameters.AddWithValue("@BranchID", SelectedBranchID.HasValue ? (object)SelectedBranchID.Value : DBNull.Value);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Orders.Add(new OrderViewModel
							{
								ID = reader.GetInt32("ID"),
								OrderNumber = reader["OrderNumber"].ToString(),
								OrderDate = Convert.ToDateTime(reader["OrderDate"]),
								TotalAmount = reader.GetDecimal("TotalAmount"),
								PaymentType = reader["PaymentType"]?.ToString() ?? "",
								CashierName = reader["CashierName"]?.ToString() ?? "",
								Items = new ObservableCollection<OrderItemViewModel>()
							});
						}
					}
				}

				// -------------------------------
				// Загружаем позиции каждого заказа
				// -------------------------------
				foreach (var order in Orders)
				{
					string itemsQuery = @"
                        SELECT m.Name AS MenuName, om.Quantity, om.UnitPrice, p.Name AS PromotionName
                        FROM OrderMenu om
                        LEFT JOIN Menu m ON om.MenuID = m.ID
                        LEFT JOIN Promotions p ON om.PromotionID = p.ID
                        WHERE om.OrderID = @OrderID;
                    ";

					using (var cmd = new MySqlCommand(itemsQuery, conn))
					{
						cmd.Parameters.AddWithValue("@OrderID", order.ID);

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								order.Items.Add(new OrderItemViewModel
								{
									MenuName = reader["MenuName"].ToString(),
									Quantity = reader.GetInt32("Quantity"),
									UnitPrice = reader.GetDecimal("UnitPrice"),
									PromotionName = reader["PromotionName"]?.ToString() ?? ""
								});
							}
						}
					}
				}
			}
		}
	}

	// ===============================
	// Модели
	// ===============================
	public class OrderViewModel
	{
		public int ID { get; set; }
		public string OrderNumber { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentType { get; set; }
		public string CashierName { get; set; }
		public ObservableCollection<OrderItemViewModel> Items { get; set; } = new ObservableCollection<OrderItemViewModel>();
	}

	public class OrderItemViewModel
	{
		public string MenuName { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public string PromotionName { get; set; }
	}

}

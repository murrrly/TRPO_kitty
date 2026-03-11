using MySql.Data.MySqlClient;

namespace RPM.Data
{
	public static class Db
	{
		private static string connectionString =
			"server=localhost;user=root;password=cat12345;database=catcafe_db;";

		public static MySqlConnection GetConnection()
		{
			return new MySqlConnection(connectionString);
		}
	}
}

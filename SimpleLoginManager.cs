using System;
using System.IO;

public static class SimpleLoginManager
{
	private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastlogin.txt");

	public static void Save(string username, string password)
	{
		try
		{
			File.WriteAllText(filePath, username + "|" + password);
		}
		catch { }
	}

	public static (string username, string password) Load()
	{
		if (!File.Exists(filePath))
			return (string.Empty, string.Empty);

		try
		{
			string content = File.ReadAllText(filePath);
			string[] parts = content.Split('|');
			if (parts.Length != 2) return (string.Empty, string.Empty);
			return (parts[0], parts[1]);
		}
		catch
		{
			return (string.Empty, string.Empty);
		}
	}

	public static void Delete()
	{
		if (File.Exists(filePath))
			File.Delete(filePath);
	}
}

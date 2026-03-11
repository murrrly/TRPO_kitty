using System;
using System.Globalization;
using System.Windows.Data;

namespace RPM.Converters
{
	public class ShortNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string fullName && !string.IsNullOrWhiteSpace(fullName))
			{
				var parts = fullName.Split(' ');
				if (parts.Length >= 3)
				{
					// Имя + первая буква отчества + первая буква фамилии (или по желанию)
					return $"{parts[1]} {parts[2][0]}.";
				}
				else if (parts.Length == 2)
				{
					return $"{parts[1]} {parts[0][0]}.";
				}
				else
				{
					return fullName;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

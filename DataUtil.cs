using System;
using System.Data.SqlClient;

namespace Nunesoft.DataBinder {
	public class DataUtil {
		public static string connectionString { get; set; }

		public static SqlConnection CreateConnection() {
			return new SqlConnection(connectionString);
		}

		public static DateTime ToDateTime(object value) {
			DateTime result = DateTime.MinValue;
			try {
				result = Convert.ToDateTime(value, System.Globalization.CultureInfo.InvariantCulture);
			} catch {
			}
			return result;
		}

		public static Int32 ToInt32(object value) {
			Int32 result = 0;
			try {
				result = Convert.ToInt32(value);
			} catch {
			}
			return result;
		}

		public static Int64 ToInt64(object value) {
			Int64 result = 0;
			try {
				result = Convert.ToInt64(value);
			} catch {
			}
			return result;
		}

		public static string ToString(object value) {
			string result = String.Empty;
			try {
				result = Convert.ToString(value);
			} catch {
			}
			return result;
		}
	}
}

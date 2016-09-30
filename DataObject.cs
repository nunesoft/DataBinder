using System;
using System.Data.SqlClient;
using System.Text;

namespace Nunesoft.DataBinder {

	public interface IDataObject {
		void Insert();
		void Delete();
		void Update();
	}
	
	public class DataObject: IDataObject {
		public void Insert() {
			bool first = true;
			DataBinder binder = DataBinder.GetByType(this.GetType());
			StringBuilder sb = new StringBuilder();
			sb.Append("INSERT INTO ");
			sb.Append(binder.tableName);
			sb.Append("\n( ");
			foreach(IDataBinderField ff in binder.keys) {
				if(!ff.isAutoIncrement) {
					if(first) {
						first = false;
					} else {
						sb.Append(",  ");
					}
					sb.Append(ff.fieldName);
				}
			}
			foreach(IDataBinderField ff in binder.fields) {
				if(first) {
					first = false;
				} else { 
					sb.Append(",  ");
				}
				sb.Append(ff.fieldName);
			}
			sb.Append(")\nVALUES\n(");
			first = true;
			foreach(IDataBinderField ff in binder.keys) {
				if(!ff.isAutoIncrement) {
					if(first) {
						first = false;
					} else {
						sb.Append(",  ");
					}
					sb.Append('@');
					sb.Append(ff.fieldName);
				}
			}
			foreach(IDataBinderField ff in binder.fields) {
				if(first) {
					first = false;
				} else {
					sb.Append(",  ");
				}
				sb.Append('@');
				sb.Append(ff.fieldName);
			}
			sb.Append(")");

			using(SqlConnection conn = DataUtil.CreateConnection()) {
				conn.Open();
				using(SqlCommand cmd = new SqlCommand(sb.ToString(), conn)) {
					foreach(IDataBinderField ff in binder.keys) {
						if(!ff.isAutoIncrement) {
							cmd.Parameters.AddWithValue("@" + ff.fieldName, ff.Getter(this));
						}
					}
					foreach(IDataBinderField ff in binder.fields) {
						cmd.Parameters.AddWithValue("@" + ff.fieldName, ff.Getter(this));
					}
					cmd.ExecuteNonQuery();
				}
				if(binder.keys.Count == 1 && binder.keys[0].isAutoIncrement) {
					using(SqlCommand cmd = new SqlCommand("SELECT @@IDENTITY", conn)) {
						object newKey = cmd.ExecuteScalar();
						binder.keys[0].Setter(this, newKey);
					}
				}
			}
		}

		public void Delete() {
			DataBinder binder = DataBinder.GetByType(this.GetType());
			StringBuilder sb = new StringBuilder();
			sb.Append("DELETE FROM ");
			sb.Append(binder.tableName);
			sb.Append(" WHERE ");
			for(int i = 0; i < binder.keys.Count; i++) {
				if(i > 0) {
					sb.Append(" AND  ");
				}
				sb.Append("(");
				sb.Append(binder.keys[i].fieldName);
				sb.Append(" = @");
				sb.Append(binder.keys[i].fieldName);
				sb.Append(")");
			}

			using(SqlConnection conn = DataUtil.CreateConnection()) {
				conn.Open();
				using(SqlCommand cmd = new SqlCommand(sb.ToString(), conn)) {
					foreach(IDataBinderField ff in binder.keys) {
						cmd.Parameters.AddWithValue("@" + ff.fieldName, ff.Getter(this));
					}
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Update() {
			DataBinder binder = DataBinder.GetByType(this.GetType());
			StringBuilder sb = new StringBuilder();
			sb.Append("UPDATE ");
			sb.Append(binder.tableName);
			sb.Append(" SET ");
			for(int i = 0; i < binder.fields.Count; i++) {
				if(i > 0) {
					sb.Append(",  ");
				}
				sb.Append(binder.fields[i].fieldName);
				sb.Append(" = @");
				sb.Append(binder.fields[i].fieldName);
			}
			sb.Append(" WHERE ");
			for(int i = 0; i < binder.keys.Count; i++) {
				if(i > 0) {
					sb.Append(" AND  ");
				}
				sb.Append("(");
				sb.Append(binder.keys[i].fieldName);
				sb.Append(" = @");
				sb.Append(binder.keys[i].fieldName);
				sb.Append(")");
			}

			using(SqlConnection conn = DataUtil.CreateConnection()) {
				conn.Open();
				using(SqlCommand cmd = new SqlCommand(sb.ToString(), conn)) {
					foreach(IDataBinderField ff in binder.keys) {
						cmd.Parameters.AddWithValue("@" + ff.fieldName, ff.Getter(this));
					}
					foreach(IDataBinderField ff in binder.fields) {
						cmd.Parameters.AddWithValue("@" + ff.fieldName, ff.Getter(this));
					}
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static T GetByKey<T>(object k1) {
			return GetByKey<T>(new object[] { k1 });
		}
		public static T GetByKey<T>(object k1, object k2) {
			return GetByKey<T>(new object[] { k1, k2 });
		}
		public static T GetByKey<T>(object k1, object k2, object k3) {
			return GetByKey<T>(new object[] { k1, k2, k3 });
		}
		public static T GetByKey<T>(object k1, object k2, object k3, object k4) {
			return GetByKey<T>(new object[] { k1, k2, k3, k4 });
		}
		public static T GetByKey<T>(object[] keys) {
			DataBinder binder = DataBinder.GetByType(typeof(T));

			if(binder.keys.Count > keys.Length) {
				throw new ArgumentException("Length of keys parameter must by greater or equal than key fields.");
			}
			T result = default(T);
			StringBuilder sb = new StringBuilder();
			bool first = true;
			sb.Append("SELECT\n\t");
			foreach(IDataBinderField ff in binder.keys) {
				if(first) {
					first = false;
				} else {
					sb.Append(", ");
				}
				sb.Append(ff.fieldName);
			}
			foreach(IDataBinderField ff in binder.fields) {
				if(first) {
					first = false;
				} else {
					sb.Append(", ");
				}
				sb.Append(ff.fieldName);
			}
			sb.Append("\nFROM ");
			sb.Append(binder.tableName);
			sb.AppendLine("\nWHERE\n\t");
			for(int i = 0; i < binder.keys.Count; i++) {
				if(i > 0) {
					sb.AppendLine("\nAND\t");
				}
				sb.Append("(");
				sb.Append(binder.keys[i].fieldName);
				sb.Append(" = @");
				sb.Append(binder.keys[i].fieldName);
				sb.Append(")");
			}

			using(SqlConnection conn = DataUtil.CreateConnection()) {
				conn.Open();
				using(SqlCommand cmd = new SqlCommand(sb.ToString(), conn)) {
					for(int i = 0; i < binder.keys.Count; i++) { 
						cmd.Parameters.AddWithValue("@" + binder.keys[i].fieldName, keys[i]);
					}
					using(SqlDataReader dr = cmd.ExecuteReader()) {
						if(dr.Read()) {
							result = (T)binder.funcConstructor();

							foreach(IDataBinderField ff in binder.keys) {
								object dbValue = dr[ff.fieldName];
								ff.Setter(result, dbValue);
							}
							foreach(IDataBinderField ff in binder.fields) {
								object dbValue = dr[ff.fieldName];
								ff.Setter(result, dbValue);
							}
						}
					}
				}
			}
			return result;
		}
	}
}

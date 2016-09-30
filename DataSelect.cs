using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Nunesoft.DataBinder {
	public class DataSelect<T> {
		DataBinder m_binder;
		string m_command;
		Dictionary<string, object> m_params;

		public DataSelect(string command) {
			m_binder = DataBinder.GetByType(typeof(T));
			m_command = command;
			m_params = new Dictionary<string, object>();
		}

		public void AddParam(string key, object value) {
			m_params.Add(key, value);
		}

		public List<T> ExecuteSelect() {
			List<T> result = new List<T>();

			using(SqlConnection conn = DataUtil.CreateConnection()) {
				conn.Open();
				using(SqlCommand cmd = new SqlCommand(m_command, conn)) {
					foreach(KeyValuePair<string, object> pp in m_params) {
						cmd.Parameters.AddWithValue(pp.Key, pp.Value);
					}
					using(SqlDataReader dr = cmd.ExecuteReader()) {
						while(dr.Read()) {
							T item = (T)m_binder.funcConstructor();

							foreach(IDataBinderField ff in m_binder.keys) {
								object dbValue = dr[ff.fieldName];
								ff.Setter(item, dbValue);
							}
							foreach(IDataBinderField ff in m_binder.fields) {
								object dbValue = dr[ff.fieldName];
								ff.Setter(item, dbValue);
							}

							result.Add(item);
						}
					}
				}
			}

			return result;
		}
	}
}

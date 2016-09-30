using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nunesoft.DataBinder {

	public interface IDataBinderField {
		string fieldName { get; }
		bool isAutoIncrement { get; }
		object Getter(object obj);
		void Setter(object obj, object value);
	}

	public class DataBinderField<T>: IDataBinderField {
		string m_fieldName;
		Func<T, object> m_getter;
		Action<T, object> m_setter;
		bool m_isAutoIncrement;

		public string fieldName {
			get { return m_fieldName; }
		}

		public bool isAutoIncrement {
			get { return m_isAutoIncrement; }
		}

		public DataBinderField(string name, Func<T, object> getter, Action<T, object> setter, bool isAutoIncrement = false) {
			m_fieldName = name;
			m_getter = getter;
			m_setter = setter;
			m_isAutoIncrement = isAutoIncrement;
		}

		public object Getter(object obj) {
			return m_getter((T)obj);
		}

		public void Setter(object obj, object value) {
			m_setter((T)obj, value);
		}
	}

	public class DataBinder {
		string m_tableName;
		public Func<IDataObject> funcConstructor;
		public List<IDataBinderField> keys;
		public List<IDataBinderField> fields;

		public string tableName {
			get { return m_tableName; }
		}

		public DataBinder(string tableName) {
			m_tableName = tableName;
			funcConstructor = null;
			keys = new List<IDataBinderField>();
			fields = new List<IDataBinderField>();
		}

		static Dictionary<int, DataBinder> x_binders = new Dictionary<int, DataBinder>();
		static object x_locker = new object();

		public static DataBinder GetByType(Type t) {
			DataBinder result = null;
			int hash = t.GetHashCode();
			lock(x_locker) {
				if(x_binders.ContainsKey(hash)) {
					return x_binders[hash];
				}

				MethodInfo mi = t.GetMethod("BuildDataBinder");
				if(mi == null || !mi.IsStatic) {
					throw new Exception("There is no static method called BuildDataBinder");
				}
				result = (DataBinder)mi.Invoke(null, null);

				x_binders.Add(hash, result);
			}
			return result;
		}
	}
}

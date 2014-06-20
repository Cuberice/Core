using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data;
using Core.Service;
using Models;

namespace Core.Models
{
	public static class ModelExtensions
	{
		public static T CreateInstance<T>(IAdapterReader reader) where T : new()
		{
			List<Column> columns = Column.GetAll(typeof(T));
			object t = new T();
			foreach (Column c in columns)
			{
				try
				{
					object value = reader.GetValue(c.Name);
					c.Property.SetValue(t, value);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			return (T)t;
		}

		public static void InsertTestObject<T>(this T item, IDataService dataService)
		{
			PerformInsertTestObject(item, dataService);
		}

		public static void PerformInsertTestObject<T>(T item, IDataService dataService)
		{
			Table t = Table.Get(typeof(T));
			t.GetTableTypeColumns().ForEach(c =>
			{
				dynamic value = c.Property.GetValue(item);
				PerformInsertTestObject(value, dataService);
			});

			dataService.InsertModel(item);
		}
	}
}

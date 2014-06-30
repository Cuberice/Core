using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
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
					if (Table.IsTable(c.PropertyType))
					{
						
					}
					c.Property.SetValue(t, value);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			return (T)t;
		}

		public static List<T> CreateTestInstances<T>(int amount) where T : ITestObject, new()
		{
			return Enumerable.Range(1, amount).Cast<object>().Select(i => new T().CreateTestObject()).Cast<T>().ToList();
		}
	}
}

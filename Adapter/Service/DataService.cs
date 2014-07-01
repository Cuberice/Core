using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel;
using Core.Data;
using Core.Models;
using Models;

namespace Core.Service
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class DataService : IDataService
	{
		public IDbAdapter Adapter { get; set; }
		public static Dictionary<string, IEnumerable> Cache; 

		public DataService()
		{
			Cache = new Dictionary<string, IEnumerable>();

			if(Adapter.IsNull())
				GetAdapter();
		}		
		private void GetAdapter()
		{
			switch (Properties.Settings.Default.IDataAdapterType)
			{
				case "SQLiteDataAdapter": Adapter = new SQLiteDataAdapter(); break;
				case "MySqlDataAdapter": Adapter = new MySqlDataAdapter(); break;

				default: Adapter = new SQLiteDataAdapter();
					break;
			}
		}
		
		public List<T> GetAllForModelNoCache<T>() where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(), r => list.Add(ModelExtensions.CreateInstance<T>(r)));
			return list;
		}

		protected IEnumerable SelectForModel(Type type)
		{
			MethodInfo method = typeof(DataService).GetMethod("SelectForModel", new Type[]{} );
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(this, null) as IEnumerable;
		}
		public List<T> SelectForModel<T>() where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(), r => list.Add(CreateInstance<T>(r)));
			return list;
		}
		public List<T> SelectForModel<T>(Expression<Func<T, bool>> f) where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(f), r => list.Add(CreateInstance<T>(r)));
			return list;
		}

		public void InsertModel<T>(T t)
		{
			Adapter.ExecuteNonQuery(() => Adapter.CreateInsertCommand(t));
		}
		public void UpdateModel<T>(T t)
		{
			Adapter.ExecuteNonQuery(() => Adapter.CreateUpdateCommand(t));
		}

		private T CreateInstance<T>(IAdapterReader reader) where T : new()
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
						if (!Cache.ContainsKey(c.PropertyTypeName))
							AddToCache(c.PropertyType);

						SetFromCache(t, c, (Guid)value);
					}
					else if (c.Conversion != Column.ConversionType.None)
					{
						c.Property.SetValue(t, GetFromConversion(value, c.Conversion));
					}
					else
					{
						c.Property.SetValue(t, value);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			return (T)t;
		}		
		private void AddToCache(Type t)
		{
			IEnumerable list = SelectForModel(t);
			Cache.Add(t.Name, list);
		}
		private void SetFromCache(object t, Column c, Guid pKvalue)
		{
			List<object> l = Cache[c.PropertyTypeName].Cast<object>().ToList();
			Column pk = Table.GetPrimaryKey(c.PropertyType);

			foreach (object o in l)
			{
				Guid cachePk = (Guid)pk.GetColumnValue(o);

				if (cachePk == pKvalue)
				{
					c.Property.SetValue(t, o);
					break;
				}
			}
		}

		private object GetFromConversion(object value, Column.ConversionType conversion)
		{
			if (value.IsNull())
				return null;

			if(conversion == Column.ConversionType.DirectoryInfo)
				return new DirectoryInfo(value as string);

			throw new NotImplementedException();
		}
		
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
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
		
		public List<T> GetAllForModelCache<T>() where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(), r => list.Add(CreateInstanceFromCache<T>(r)));
			return list;
		}
		protected IEnumerable GetAllForModelCache(Type type)
		{
			MethodInfo method = typeof(DataService).GetMethod("GetAllForModelCache");
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(this, null) as IEnumerable;
		}

		public List<T> SelectForModel<T>() where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(), r => list.Add(ModelExtensions.CreateInstance<T>(r)));
			return list;
		}
		public List<T> SelectForModel<T>(Expression<Func<T, bool>> f) where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(f), r => list.Add(ModelExtensions.CreateInstance<T>(r)));
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

		private T CreateInstanceFromCache<T>(IAdapterReader reader) where T : new()
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
						if (!Cache.ContainsKey(c.TypeName))
							AddToCache(c.PropertyType);

						SetFromCache(t, c, (Guid)value);
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
			IEnumerable list = GetAllForModelCache(t);
			Cache.Add(t.Name, list);
		}
		private void SetFromCache(object t, Column c, Guid pKvalue)
		{
			List<object> l = Cache[c.TypeName].Cast<object>().ToList();
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

		public T CreateInstance<T>(IAdapterReader reader) where T : new()
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
						Adapter.CreateSelectCommand<CoreEquipmentMake>(eq => eq.ID == (Guid)value);
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
	}
}

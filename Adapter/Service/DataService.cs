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
using Models;

namespace Core.Service
{
	public class DataService : IDataService
	{
		public IDbAdapter Adapter { get; set; }
		public static Dictionary<string, IList> Cache; 

		public DataService()
		{
			Cache = new Dictionary<string, IList>();

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

		/// <summary>
		/// Using Reflection - Only use if the Type is not available at Compile Time 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		protected IList SelectForModel(Type type)
		{
			MethodInfo method = typeof(DataService).GetMethod("SelectForModel", new Type[]{} );
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(this, null) as IList;
		}
		/// <summary>
		/// Using Reflection - Only use if the Type is not available at Compile Time 
		/// </summary>
		/// <typeparam name="R"></typeparam>
		/// <param name="type"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		protected IList SelectForModel<R>(Type type, Expression<Func<R, bool>> f)
		{
			MethodInfo method = typeof(DataService).GetMethod("SelectForModel", new Type[]{typeof(Expression)} );
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(this, new object[] { f }) as IList;
		}
		/// <summary>
		/// Using Reflection - Only use if the Type is not available at Compile Time 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="cmd"></param>
		/// <returns></returns>
		protected IList SelectForModel(Type type, Command cmd)
		{
			MethodInfo method = typeof(DataService).GetMethod("SelectForModel", new Type[] { typeof(Command) });
			MethodInfo generic = method.MakeGenericMethod(type);
			return generic.Invoke(this, new object[] { cmd }) as IList;
		}

		/// <summary>
		/// Select All data for Model given the Model
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public List<T> SelectForModel<T>() where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(), r => list.Add(CreateInstance<T>(r)));
			return list;
		}
		/// <summary>
		/// Select data for given Model with WHERE Clause as Expression
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="f">WHERE Clause as Expression of T</param>
		/// <returns></returns>
		public List<T> SelectForModel<T>(Expression<Func<T, bool>> f) where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(f), r => list.Add(CreateInstance<T>(r)));
			return list;
		}
		/// <summary>
		/// Select data for given Model with WHERE Clause as Command
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public List<T> SelectForModel<T>(Command cmd) where T : new()
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(() => Adapter.CreateSelectCommand<T>(cmd), r => list.Add(CreateInstance<T>(r)));
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
			Type type = typeof (T);
			Table table = Table.Get(type);
			
			object t = new T();
			foreach (Column c in table.Columns)
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
					else if (c.PropertyType.IsNullableType())
					{
						c.Property.SetValue(t, value.IsNullOrDbNull() ? null : c.PropertyType.GetNullableValue(value));
					}
					else if (c.PropertyType != c.ColumnSystemType)
					{
						c.Property.SetValue(t, GetFromConversion(c, value));
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

			Guid pkVal = (Guid)table.GetPrimaryKeyValue(t);
			foreach (Containment c in table.Containments)
			{
				try
				{
					Command cmd = new Command();
					cmd.AddExpression(c.ReferenceColumn, OperatorType.Equal, pkVal);					
					IEnumerable list = SelectForModel(c.ReferenceTableType, cmd);

					foreach (BackReference br in BackReference.GetAll(c.ReferenceTableType))
					{
						Column vc = table.GetColumn(br.ValueColumn);
						object vcValue = vc.GetColumnValue(t);

						if(vc.PropertyType != br.PropertyType)
							throw new ArgumentException("BackReference PropertyType must be the same as the Type being Referenced");

						foreach (object lobj in list)
						{
							br.Property.SetValue(lobj, vcValue);
						}
					}

					c.Property.SetValue(t, list);
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
			IList list = SelectForModel(t);
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

			throw new NotImplementedException();
		}
		private object GetFromConversion(Column c, object value)
		{
			if (value.IsNull())
				return value;
			
			if (c.ColumnType == Column.DataType.String)
			{
				string s = value as string;

				if (s.IsNull())
					return null;

				if (c.PropertyType == typeof(DirectoryInfo))
					return new DirectoryInfo(s);

				if (c.PropertyType == typeof(FileInfo))
					return new FileInfo(s); 
			}

			throw new NotImplementedException();
		}
	}
}

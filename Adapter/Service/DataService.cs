using System;
using System.Collections.Generic;
using System.ServiceModel;
using Core.Data;

namespace Core.Service
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class DataService : IDataService
	{
		public IDbAdapter Adapter { get; set; }

		public DataService()
		{
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
		
		public string SelectCommandString<T>()
		{
			return Adapter.CreateSelectCommand<T>();
		}
		
		public List<T> GetAllForModel<T>(Func<IAdapterReader, T> CreateInstance)
		{
			List<T> list = new List<T>();
			Adapter.PerformWithDataReader(SelectCommandString<T>(), r => list.Add(CreateInstance(r)));

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
	}
}

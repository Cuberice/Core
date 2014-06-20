using System;
using System.Collections.Generic;
using Core.Adapters;
using Core.Data;
using Core.Models;
using Models;


namespace Core.Service
{
	public class DataService : IDataService
	{
		public IDbAdapter Adapter { get; set; }

		public string SelectCommandString<T>()
		{
			return Adapter.CreateSelectCommand<T>();
		}

		public List<User> GetAllUsers()
		{
			List<User> list = new List<User>();
			Adapter.PerformWithDataReader(SelectCommandString<User>(), r => list.Add(ModelExtensions.CreateInstance<User>(r)));

			return list;
		}

		public List<Equipment> GetAllEquipment()
		{
			List<Equipment> list = new List<Equipment>();
			Adapter.PerformWithDataReader(SelectCommandString<Equipment>(), r => list.Add(ModelExtensions.CreateInstance<Equipment>(r)));

			return list;
		}

		public void InsertUser(User user)
		{
			Adapter.ExecuteNonQuery(() => Adapter.CreateInsertCommand(user));
		}
		public void InsertEquipment(Equipment equipment)
		{
			Adapter.ExecuteNonQuery(() => Adapter.CreateInsertCommand(equipment));
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

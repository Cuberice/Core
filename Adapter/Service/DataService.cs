using System.Collections.Generic;
using Core.Data;
using Core.Extensions;
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
			Adapter.PerformWithDataReader(SelectCommandString<User>(), reader =>
				{
					list.Add(ModelExtensions.CreateInstance<User>(reader));
					return null;
				});

			return list;
		}

		public List<Equipment> GetAllEquipment()
		{
			List<Equipment> list = new List<Equipment>();
			Adapter.PerformWithDataReader(SelectCommandString<Equipment>(), reader =>
			{
				list.Add(ModelExtensions.CreateInstance<Equipment>(reader));
				return null;
			});

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

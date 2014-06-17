using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Common;

namespace RegalWorxData
{
	[ServiceContract]
	public interface IRWDataService
	{
		IDbAdapter Adapter { get; set; }

		[OperationContract]
		string SelectCommandString<T>();
	
		[OperationContract]
		List<User> GetAllUsers();		
		
		[OperationContract]
		List<Equipment> GetAllEquipment();

		[OperationContract]
		void InsertUser(User user);

		[OperationContract]
		void InsertEquipment(Equipment equipment);
		
		[OperationContract]
		void InsertModel<T>(T t);

		[OperationContract]
		void UpdateModel<T>(T t);
	}
}

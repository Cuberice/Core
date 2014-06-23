using System;
using System.Collections.Generic;
using System.ServiceModel;
using Core.Data;

namespace Core.Service
{
	[ServiceContract]
	public interface IDataService
	{
		IDbAdapter Adapter { get; set; }

		[OperationContract]
		string SelectCommandString<T>();

		[OperationContract]
		List<T> GetAllForModel<T>(Func<IAdapterReader, T> CreateInstance);
		
		[OperationContract]
		void InsertModel<T>(T t);

		[OperationContract]
		void UpdateModel<T>(T t);
	}
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ServiceModel;
using Core.Data;

namespace Core.Service
{
	[ServiceContract]
	public interface IDataService
	{
		IDbAdapter Adapter { get; set; }
		
		[OperationContract]
		List<T> GetAllForModelNoCache<T>() where T : new();

		[OperationContract(Name = "SelectForModel")]
		List<T> SelectForModel<T>() where T : new();

		[OperationContract(Name = "SelectForModelWhereExpression")]
		List<T> SelectForModel<T>(Expression<Func<T, bool>> f) where T : new();

		[OperationContract(Name = "SelectForModelWhereCommand")]
		List<T> SelectForModel<T>(Command cmd) where T : new();
		
		[OperationContract]
		void InsertModel<T>(T t);

		[OperationContract]
		void UpdateModel<T>(T t);
	}
}

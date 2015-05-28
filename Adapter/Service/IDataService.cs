using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Core.Data;

namespace Core.Service
{
	public interface IDataService
	{
		IDbAdapter Adapter { get; set; }
		
		List<T> GetAllForModelNoCache<T>() where T : new();

		List<T> SelectForModel<T>() where T : new();

		List<T> SelectForModel<T>(Expression<Func<T, bool>> f) where T : new();

		List<T> SelectForModel<T>(Command cmd) where T : new();
		
		void InsertModel<T>(T t);

		void UpdateModel<T>(T t); 
	}
}

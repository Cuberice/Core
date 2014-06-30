using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Core.Common;

namespace Core.Data
{
	public interface IDbAdapter
	{
		#region Command Building
		string SPECIAL(Column c);
		string DATATYPE(Column c);
		string CREATETABLE();

		string INSERTINTO();
		string UPDATE();
		string SELECT();
		string OPERATOR(ExpressionType t);
		#endregion

		#region Data Execution
		
		IAdapterCommand CreateCommand(string commandstring);
		void PerformWithDataReader(string cmdSelect, Action<IAdapterReader> perform);
		void PerformWithDataReader(Func<IAdapterCommand> command, Action<IAdapterReader> perform); 

		bool ExecuteNonQuery(Func<IAdapterCommand> command);
		bool ExecuteNonQuery(string commandstring);
		#endregion
	}
	
	public interface IAdapterParameter
	{
		string Name { get; set; }
		object Value { get; set; }
	}
	public interface IAdapterConnection : IDisposable
	{
		string GetConnectionString();
		ConnectionState State { get;}
		object ConnObject { get; set; }
		void Open();
	}
	public interface IAdapterCommand : IDisposable, IDebuggerObject
	{
		IAdapterReader ExecuteReader();
		void SetConnection(IAdapterConnection conn);
		void AddParameter(string name, object value);
		void AddParameter(List<Parameter> parameters);
		int ExecuteNonQuery();
	}
	public interface IAdapterReader : IDisposable
	{
		bool HasRows();
		bool Read();

		object GetValue(string columnname);
	}
}

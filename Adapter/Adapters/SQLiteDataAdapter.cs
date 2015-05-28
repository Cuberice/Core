using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Core.Properties;

namespace Core.Data
{
	public class SQLiteDataAdapter : IDbAdapter
	{
		#region Command Building
		public string CREATETABLE()
		{
			return "CREATE TABLE {0} ({1});";
		}
		public string SPECIAL(Column c)
		{
			if (c.PrimaryKey)
				return "PRIMARY KEY";

			string str = c.Unique ? "UNIQUE" : string.Empty;

			if (c.NotNull)
				str += "NOT NULL";

			return str;
		}
		public string DATATYPE(Column c)
		{
			switch (c.ColumnType) //Use propertyType to determine ColumnType if Null
			{
				case Column.DataType.Guid: return "GUID";
				case Column.DataType.Integer: return "INT";
				case Column.DataType.String: return "NVARCHAR(30)";
				case Column.DataType.Text: return "NVARCHAR(100)";
				case Column.DataType.LargeText: return "NVARCHAR(500)";
				case Column.DataType.Double: return "DOUBLE";
				case Column.DataType.DateTime: return "DATETIME";
				case Column.DataType.Boolean: return "BIT";
			}

			return "UNDEFINED DATATYPE";
		}

		public string INSERTINTO()
		{
			return "INSERT INTO {0}({1}) VALUES({2});";
		}
		public string UPDATE()
		{
			return "UPDATE {0} SET {1} WHERE {2};";
		}

		public string SELECT()
		{
			return "SELECT {0} FROM {1} {2};";
		}
		public string OPERATOR(OperatorType t)
		{
			switch (t)
			{
				case OperatorType.Equal: return "=";
				case OperatorType.GreaterThan: return ">";
				case OperatorType.GreaterThanOrEqual: return ">=";
				case OperatorType.LessThan: return "<";
				case OperatorType.LessThanOrEqual: return "<=";
				case OperatorType.And: return "AND";
				case OperatorType.Or: return "OR";
				case OperatorType.In: return "IN";

			}
			return "Symbol Not Implemented";
		}
		
		#endregion

		#region Data Execution
		public IAdapterCommand CreateCommand(string commandstring)
		{
			return new SQLiteAdapterCommand(commandstring);
		}
		
		public void PerformWithDataReader(string cmdSelect, Action<IAdapterReader> perform)
		{
			using (IAdapterConnection conn = new SQLiteAdapterConnection())
			{
				using (IAdapterCommand cmd = new SQLiteAdapterCommand(cmdSelect, conn))
				{
					using (IAdapterReader r = cmd.ExecuteReader())
					{
						if (!r.HasRows())
							return;

						while (r.Read())
						{
							perform(r);
						}
					}
				}
			}
		}
		public void PerformWithDataReader(Func<IAdapterCommand> command, Action<IAdapterReader> perform)
		{
			using (IAdapterConnection conn = new SQLiteAdapterConnection())
			{
				using (IAdapterCommand cmd = command())
				{
					cmd.SetConnection(conn);
					using (IAdapterReader r = cmd.ExecuteReader())
					{
						if (!r.HasRows())
							return;

						while (r.Read())
						{
							perform(r);
						}
					}
				}
			}
		}
		
		public bool ExecuteNonQuery(Func<IAdapterCommand> command)
		{
			using (IAdapterConnection conn = new SQLiteAdapterConnection())
			{
				using (IAdapterCommand cmd = command())
				{
					try
					{
						cmd.SetConnection(conn);
						return cmd.ExecuteNonQuery() > 0;
					}
					catch (SQLiteException e)
					{
						Console.WriteLine(e);
						return false;
					}
				}
			}
		}
		public bool ExecuteNonQuery(string commandstring)
		{
			return ExecuteNonQuery(() => new SQLiteAdapterCommand(commandstring));
		}

		#endregion

		#region Adapter Classes

		protected class SQLiteAdapterConnection : IAdapterConnection
		{
			public object ConnObject { get; set; }
			protected SQLiteConnection Connection
			{
				get { return ConnObject as SQLiteConnection; }
				set { ConnObject = value; }
			}
			public ConnectionState State { get { return Connection.State; } }

			public string GetConnectionString()
			{
				return Settings.Default.DbConnection;
			}
			public SQLiteAdapterConnection()
			{
				Connection = new SQLiteConnection(GetConnectionString());

				while (Connection.State != ConnectionState.Closed)
				{
					Thread.Sleep(100);
				}
				Open();
			}
			public void Open()
			{
				Connection.Open();
			}
			public void Dispose()
			{
				Connection.Dispose();
			}
		}
		protected class SQLiteAdapterReader : IAdapterReader
		{
			public SQLiteDataReader Reader { get; set; }

			public bool HasRows()
			{
				return Reader.HasRows;
			}

			public bool Read()
			{
				return Reader.Read();
			}

			public object GetValue(string columnname)
			{
				return Reader.GetValue(Reader.GetOrdinal(columnname));
			}

			public void Dispose()
			{
				Reader.Dispose();
			}
		}

		[DebuggerDisplay("{DebugString()}")]
		protected class SQLiteAdapterCommand : IAdapterCommand
		{
			public SQLiteCommand Command { get; set; }

			public SQLiteAdapterCommand(string commandText)
			{
				Command = new SQLiteCommand(commandText);
			}

			public SQLiteAdapterCommand(string commandText, IAdapterConnection conn)
			{
				Command = new SQLiteCommand(commandText, conn.ConnObject as SQLiteConnection);
			}

			public void SetConnection(IAdapterConnection conn)
			{
				Command.Connection = conn.ConnObject as SQLiteConnection;
			}

			public void AddParameter(string name, object value)
			{
				Command.Parameters.AddWithValue(name, value);
			}			
			public void AddParameter(List<Parameter> parameters )
			{
				parameters.ForEach(p => AddParameter(p.Name, p.Value));
			}

			public IAdapterReader ExecuteReader()
			{
				return new SQLiteAdapterReader {Reader = Command.ExecuteReader()};
			}

			public int ExecuteNonQuery()
			{
				Trace.WriteLine(string.Format("Executing Command:[{0}]", Command.CommandText));
				return Command.ExecuteNonQuery();
			}

			public void Dispose()
			{
				Command.Dispose();
			}
			
			public string DebugString()
			{
				return Command == null ? "Empty" : Command.CommandText.Take(20).ToString();
			}
		}

		#endregion
	}

	[DebuggerDisplay("{DebugString()}")]
	public class Parameter : IDebuggerObject
	{
		private string name;
		public string Name { get { return "@" + name; } set { name = value.ToLower(); } }
		public object Value { get; set; }

		public string DebugString()
		{
			return string.Format("{0}-{1}", Name, Value);
		}
	}
}

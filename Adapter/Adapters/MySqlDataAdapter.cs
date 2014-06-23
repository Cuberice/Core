using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq.Expressions;
using System.Threading;

namespace Core.Data
{
	public class MySqlDataAdapter : IDbAdapter
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
			}

			return "UNDEFINED DATATYPE";
		}

		public string INSERTINTO()
		{
			throw new NotImplementedException();
		}
		public string UPDATE()
		{
			throw new NotImplementedException();
		}
		public string SELECT()
		{
			throw new NotImplementedException();
		}

		public string OPERATOR(ExpressionType t)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Data Execution
		public IAdapterCommand CreateCommand(string commandstring)
		{
			throw new NotImplementedException();
		}

		public void PerformWithDataReader<T>(string cmdSelect, Func<IAdapterReader, T> perform)
		{
			
		}

		public void PerformWithDataReader(string cmdSelect, Action<IAdapterReader> perform)
		{
			
		}

		public bool ExecuteNonQuery(Func<IAdapterCommand> command)
		{
			using (IAdapterConnection conn = new MySQLAdapterConnection())
			{
				using (IAdapterCommand cmd = command())
				{
					try
					{
						cmd.SetConnection(conn);
						return cmd.ExecuteNonQuery() > 0;
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						return false;
					}
				}
			}
		}
		public bool ExecuteNonQuery(string commandstring)
		{
			return ExecuteNonQuery(() => new MySQLAdapterCommand(commandstring));
		}

		#endregion

		#region Adapter Classes
		protected class MySQLAdapterConnection : IAdapterConnection
		{
			public object ConnObject { get; set; }
			protected MySqlConnection Connection
			{
				get { return ConnObject as MySqlConnection; }
				set { ConnObject = value; }
			}
			public ConnectionState State { get; set; }

			public MySQLAdapterConnection()
			{
				Connection = new MySqlConnection(Properties.Settings.Default.DbConnection);

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
		protected class MySQLAdapterReader : IAdapterReader
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
		protected class MySQLAdapterCommand : IAdapterCommand
		{
			private MySqlCommand Command { get; set; }

			public MySQLAdapterCommand(string commandText)
			{
				Command = new MySqlCommand(commandText);
			}

			public MySQLAdapterCommand(string commandText, IAdapterConnection conn)
			{
				Command = new MySqlCommand(commandText, conn.ConnObject as MySqlConnection);
			}

			public void SetConnection(IAdapterConnection conn)
			{
				Command.Connection = conn.ConnObject as MySqlConnection;
			}
			
			public void AddParameter(string name, object value)
			{
				
			}

			public void AddParameter(List<Parameter> parameters)
			{
				throw new NotImplementedException();
			}

			public IAdapterReader ExecuteReader()
			{
				return new MySQLAdapterReader { Reader = Command.ExecuteReader() };
			}
			public int ExecuteNonQuery()
			{
				throw new NotImplementedException();
			}

			public void Dispose()
			{
				Command.Dispose();
			}

			public string DebugString()
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}

	#region Mock MySQL Classes

	public class MySqlCommand : IDisposable
	{
		public MySqlCommand(string commandText)
		{
			throw new NotImplementedException();
		}		
		public MySqlCommand(string commandText, MySqlConnection connection)
		{
			throw new NotImplementedException();
		}

		public MySqlConnection Connection { get; set; }
		public object Parameters { get; set; }

		public SQLiteDataReader ExecuteReader()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}

	public class MySqlConnection : IDisposable
	{
		public MySqlConnection(string dbConnection)
		{
			throw new NotImplementedException();
		}

		public ConnectionState State { get; set; }

		public void Open()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}

	#endregion

}

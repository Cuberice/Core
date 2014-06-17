using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using RegalWorxData;
using RegalWorxData.Properties;

namespace RegalWorxData
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
		public string OPERATOR(ExpressionType t)
		{
			switch (t)
			{
				case ExpressionType.Equal: return "=";
				case ExpressionType.GreaterThan: return ">";
				case ExpressionType.GreaterThanOrEqual: return ">=";
				case ExpressionType.LessThan: return "<";
				case ExpressionType.LessThanOrEqual: return "<=";
				case ExpressionType.And | ExpressionType.AndAlso: return "AND";
				case ExpressionType.Or | ExpressionType.OrElse: return "OR";
			}
			return "Symbol Not Implemented";
		}
		#endregion

		#region Data Execution
		public IAdapterCommand CreateCommand(string commandstring)
		{
			return new SQLiteAdapterCommand(commandstring);
		}
		
		public void PerformWithDataReader(string cmdSelect, Func<IAdapterReader, object> perform)
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

			public SQLiteAdapterConnection()
			{
				Connection = new SQLiteConnection(Settings.Default.DbConnection);

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
		public string Name { get; set; }
		public object Value { get; set; }

		public string DebugString()
		{
			return string.Format("{0}-{1}", Name, Value);
		}
	}
}

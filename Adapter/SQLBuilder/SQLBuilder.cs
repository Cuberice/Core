using System;
using System.Collections.Generic;
using System.Linq;


namespace Core.Data
{
	public class SQLBuilder
	{
		private IDbAdapter Adapter;

		public SQLBuilder(IDbAdapter adapter)
		{
			Adapter = adapter;
		}

		public void CreateStructure()
		{
			List<Type> tables = Table.GetAllTableTypes();
			string cmd = tables.Aggregate(string.Empty, (current, table) => current + CreateScript(table));

			Adapter.ExecuteNonQuery(cmd);
		}
		private string CreateScript(Type type)
		{
			Table t = Table.Get(type);
			List<Column> columns = Column.GetAll(type);

			if (!columns.Any())
				throw new ArgumentException("Empty Column Definition");

			string cs = columns.Aggregate(string.Empty, (current, c) => current + string.Format("{0} {1} {2},", c.Name, Adapter.DATATYPE(c), Adapter.SPECIAL(c)));
			return string.Format(Adapter.CREATETABLE(), t.TableName, cs.TrimEnd(','));
		}
	}
}

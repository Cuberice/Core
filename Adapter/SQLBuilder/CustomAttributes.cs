using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Core.Common;


namespace Core.Data
{
	[DebuggerDisplay("{DebugString()}")]
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public class Table : Attribute, IDebuggerObject
	{
		public string TableName;
		public Type ClassType;
		public List<Column> Columns;
    public Table(string table)
    {
			TableName = table;
    }

		public static bool IsTable(Type t)
		{
			return IsDefined(t, typeof (Table));
		}
		public static List<Type> GetAllTableTypes()
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(IsTable).ToList();
		}
		
		public static Table Get(Type t)
		{
			Table table = (Table)t.GetCustomAttribute(typeof (Table));
			table.ClassType = t;
			table.Columns = Column.GetAll(t);
			return table;
		}
		public Column GetPrimaryKey()
		{
			return Columns.First(c => c.PrimaryKey); // Column.GetAll(ClassType).First(c => c.PrimaryKey);
		}
		/// <summary>
		/// Returns a collection of Columns which has a PropertyType for which a Table is defined
		/// </summary>
		/// <returns></returns>
		public List<Column> GetTableTypeColumns()
		{
			return Columns.Where(c => IsTable(c.Property.PropertyType)).ToList();
		}

		public string DebugString()
		{
			return string.Format("{0} - Type [{1}]", TableName, ClassType.Name);
		}
	}

	[DebuggerDisplay("{DebugString()}")]
	[AttributeUsage(AttributeTargets.Property)]
	public class Column : Attribute, IDebuggerObject
	{
		public string Name;
		public DataType ColumnType;
		public bool PrimaryKey = false;
		public bool Unique = false;
		public bool NotNull = false;
		public string OldName;

		public PropertyInfo Property;
		public Type ColumnSystemType{ get{ return GetSystemType(); }}

    public Column(string column, DataType columnType)
    {
	    Name = column;
	    ColumnType = columnType;
    }

		/// <summary>
		/// Returns all Column Data (Incl PropertyInfo) for each defined Property of given Type
		/// </summary>
		/// <param name="t">Model Class with defined Table and Column decorations</param>
		/// <returns></returns>
		public static List<Column> GetAll(Type t)
		{
			return t.GetProperties().Where(p => p.IsDefined(typeof(Column))).Select(Get).ToList();
		}

		public static Column Get(PropertyInfo p)
		{
			Column c = (Column) p.GetCustomAttribute(typeof (Column));
			c.Property = p;
			return c;
		}

		public object GetColumnValue<T>(T obj)
		{
			try
			{
				Type propType = Property.PropertyType;
				object propValue = Property.GetValue(obj);

				if (propType.IsPrimitive)
					return propValue;

				if (propType.IsEnum)
					return (int)propValue;

				if (propType.Namespace.StartsWith("System"))
					return propValue;

				if (Table.IsTable(propType))
				{
					Table propTable = Table.Get(propType);
					Column propColumn = propTable.GetPrimaryKey();

					return propColumn.GetColumnValue(propValue);
				}

				return null;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}

		public enum DataType
		{
			Guid, Integer, String, Double, DateTime, Text, LargeText
		}

		protected Type GetSystemType()
		{
			switch (ColumnType)
			{
				case DataType.Guid : return typeof(Guid);
				case DataType.Integer : return typeof(int);
				case DataType.String : return typeof(string);
				case DataType.Double : return typeof(double);
				case DataType.DateTime : return typeof(DateTime);
				case DataType.Text : return typeof(string);
				case DataType.LargeText : return typeof(string);
			}
			return null;
		}

		public string DebugString()
		{
			return string.Format("{0}:[{1}]", PrimaryKey ? "Pk."+Name : Name, ColumnType);
		}
	}

	public static class AttributeExtensions
	{

	}
}

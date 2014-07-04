using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;


namespace Core.Data
{

	#region Table

	[DebuggerDisplay("{DebugString()}")]
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public class Table : Attribute, IDebuggerObject
	{
		public string TypeName
		{
			get { return ClassType.Name; }
		}

		public string TableName;
		public Type ClassType;

		public Column PrimaryKey
		{
			get { return Columns.First(c => c.PrimaryKey); }
		}

		public List<Column> Columns;
		public List<Containment> Containments;

		public Table(string table)
		{
			TableName = table;
		}

		public static List<Type> GetNamespaceTableTypes()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(t => t.GetTypes())
				.Where(t => t.Namespace == "Models" && IsTable(t)).ToList();
		}

		public static List<Type> GetAllTableTypes()
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(IsTable).ToList();
		}

		public static Table Get(Type t)
		{
			Table table = (Table) t.GetCustomAttribute(typeof (Table));
			table.ClassType = t;
			table.Columns = Column.GetAll(t);
			table.Containments = Containment.GetAll(t);
			return table;
		}

		public static bool IsTable(Type t)
		{
			return IsDefined(t, typeof (Table));
		}

		/// <summary>
		/// Returns a collection of Columns which has a PropertyType for which a Table is defined
		/// </summary>
		/// <returns></returns>
		public List<Column> GetModelTypeColumns()
		{
			return Columns.Where(c => IsTable(c.PropertyType)).ToList();
		}

		public Column GetColumn(string name)
		{
			return Columns.First(c => c.Name == name);
		}

		public static Column GetPrimaryKey(Type type)
		{
			Table table = Get(type);
			return table.PrimaryKey;
		}

		public static object GetPrimaryKeyValue(Type type, object value)
		{
			Column pk = GetPrimaryKey(type);
			return pk.GetColumnValue(value);
		}

		public object GetPrimaryKeyValue(object value)
		{
			return PrimaryKey.GetColumnValue(value);
		}

		public string GetColumnString()
		{
			return
				Columns.Aggregate(string.Empty, (current, c) => current + string.Format("{0}, ", c.Name)).TrimEnd(' ').TrimEnd(',');
		}

		public string DebugString()
		{
			return string.Format("{0} - Type [{1}]", TableName, ClassType.IsNull() ? "ClassType Not Set" : ClassType.Name);
		}
	}

	#endregion

	#region Column

	[DebuggerDisplay("{DebugString()}")]
	[AttributeUsage(AttributeTargets.Property)]
	public class Column : ColumnBase, IDebuggerObject
	{
		public string Name;
		public DataType ColumnType;
		public ValueType Type;
		public ConversionType Conversion;
		public bool PrimaryKey = false;
		public bool Unique = false;
		public bool NotNull = false;
		public string OldName;

		public Type ColumnSystemType
		{
			get { return GetColumnType(); }
		}

		public Column(string column, DataType columnType)
		{
			Name = column;
			ColumnType = columnType;
			Conversion = ConversionType.None;
		}

		/// <summary>
		/// Returns all Column Data (Incl PropertyInfo) for each defined Property of given Type
		/// </summary>
		/// <param name="t">Model Class with defined Table and Column decorations</param>
		/// <returns></returns>
		public static List<Column> GetAll(Type t)
		{
			return t.GetProperties().Where(p => p.IsDefined(typeof (Column))).Select(Get).ToList();
		}

		public static Column Get(PropertyInfo p)
		{
			Column c = (Column) p.GetCustomAttribute(typeof (Column));
			c.Property = p;
			return c;
		}

		protected Type GetColumnType()
		{
			switch (ColumnType)
			{
				case DataType.Guid:
					return typeof (Guid);
				case DataType.Integer:
					return typeof (int);
				case DataType.String:
					return typeof (string);
				case DataType.Double:
					return typeof (double);
				case DataType.Boolean:
					return typeof (bool);
				case DataType.DateTime:
					return typeof (DateTime);
				case DataType.Text:
					return typeof (string);
				case DataType.LargeText:
					return typeof (string);
			}
			return null;
		}

		public object GetColumnValue<T>(T obj)
		{
			try
			{
				Type propType = PropertyType;
				object propValue = Property.GetValue(obj);

				if (propType.IsPrimitive)
					return propValue;

				if (propType.IsEnum)
					return (int) propValue;

				if (propType.Namespace.StartsWith("System"))
					return propValue;

				if (Table.IsTable(propType)) //Returns the Pk value of the Lookup Table
				{
					return Table.GetPrimaryKeyValue(propType, propValue);
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
			Guid,
			Integer,
			String,
			Double,
			DateTime,
			Text,
			LargeText,
			Boolean
		}

		public enum ValueType
		{
			Value,
			Lookup
		}

		public enum ConversionType
		{
			None,
			DirectoryInfo,
			FileInfo
		}

		public string DebugString()
		{
			return string.Format("{0}:[{1}]", PrimaryKey ? "Pk." + Name : Name, ColumnType);
		}
	}

	#endregion

	#region Containment

	[DebuggerDisplay("{DebugString()}")]
	[AttributeUsage(AttributeTargets.Property)]
	public class Containment : ColumnBase, IDebuggerObject
	{
		public Type ReferenceTableType { get; set; }
		public string ReferenceColumnName { get; set; }
		public Table ReferenceTable { get; set; }
		public Column ReferenceColumn { get; set; }

		public Containment(Type refTableType, string columnname)
		{
			ReferenceTableType = refTableType;
			ReferenceColumnName = columnname;

			ReferenceTable = Table.Get(ReferenceTableType);
			ReferenceColumn = ReferenceTable.GetColumn(ReferenceColumnName);
		}

		public static List<Containment> GetAll(Type t)
		{
			return t.GetProperties().Where(p => p.IsDefined(typeof (Containment))).Select(Get).ToList();
		}

		public static Containment Get(PropertyInfo p)
		{
			Containment r = (Containment) p.GetCustomAttribute(typeof (Containment));
			r.Property = p;
			return r;
		}

		public string DebugString()
		{
			return string.Format("References [{0}] on Column [{1}]",
				ReferenceTable.ClassType.IsNull() ? "Ref Table ClassType not set" : ReferenceTable.ClassType.Name,
				ReferenceColumnName);
		}
	}

	#endregion

	[DebuggerDisplay("{DebugString()}")]
	[AttributeUsage(AttributeTargets.Property)]
	public class BackReference : ColumnBase, IDebuggerObject
	{
		public string ValueColumn { get; set; }

		public BackReference(string valuecolumn)
		{
			ValueColumn = valuecolumn;
		}

		public static List<BackReference> GetAll(Type t)
		{
			return t.GetProperties().Where(p => p.IsDefined(typeof(BackReference))).Select(Get).ToList();
		}

		public static BackReference Get(PropertyInfo p)
		{
			BackReference r = (BackReference)p.GetCustomAttribute(typeof(BackReference));
			r.Property = p;
			return r;
		}

		public string DebugString()
		{
			return "BackRef";
		}
	}




	/// <summary>
	/// Base Class for Property Types
	/// </summary>
	public class ColumnBase : Attribute
	{
		public PropertyInfo Property;
		public Type PropertyType { get { return Property.PropertyType; } }
		public string PropertyTypeName { get { return Property.IsNull() ? "EmptyType" : PropertyType.Name; } }
	}

	public static class AttributeExtensions
	{
		public static List<Table> DebugGetNamespaceTableTypes()
		{
			List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
			List<Type> types = assemblies.SelectMany(t => t.GetTypes()).ToList();
			List<Type> models = types.Where(t => t.Namespace == "Models").ToList();
			List<Type> tables = models.Where(Table.IsTable).ToList();

			return tables.Select(Table.Get).ToList();
		}
		public static void LoadAllAssemblies()
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "");
			List<string> assemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName()).Select(a => a.Name + ".dll").ToList();

			foreach (string f in Directory.GetFiles(path, "*.dll"))
			{
				FileInfo fi = new FileInfo(f);

				if (!assemblyNames.Contains(fi.Name))
				{
					Assembly.LoadFile(fi.FullName);
					assemblyNames.Add(fi.Name);
				}
			}
		}
	}
}

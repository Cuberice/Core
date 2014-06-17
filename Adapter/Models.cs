using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Common;


namespace RegalWorxData
{
	[DebuggerDisplay("{DebugString()}")]
	[Table("TBL_USER")]
	public struct User : ITestObject, IDebuggerObject
	{
		[Column("ID", Column.DataType.Guid, PrimaryKey = true)]
		public Guid ID { get; private set; }

		[Column("NAME", Column.DataType.String, NotNull = true)]
		public string Name { get; set; }

		[Column("TYPE", Column.DataType.Integer, NotNull = true)]
		public UserType Type { get; set; }

		public User(Guid id, string name, int typeid) : this()
		{
			ID = id;
			Name = "";
			Type = UserType.Admin;
		}
		
		public Object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Name = Extentions.GetRandomString(8);
			Type = UserType.Normal;

			return this;
		}
		public string DebugString()
		{
			return ToString();
		}
		public override string ToString()
		{
			return string.Format("User ID: {0}, Name: {1}, Type: {2}", ID, Name, Type);
		}
		public static List<User> CreateTestInstances(int amount)
		{
			return Enumerable.Range(1, amount).Cast<object>().Select(i => new User().CreateTestObject()).Cast<User>().ToList();
		}
	}
	public enum UserType
	{
		Admin = 1, 
		Supervisor = 2,
		Normal = 3
	}

	[DebuggerDisplay("{DebugString()}")]
	[Table("TBL_EQUIPMENT")]
	public struct Equipment : ITestObject, IDebuggerObject
	{
		[Column("ID", Column.DataType.Guid, PrimaryKey = true)]
		public Guid ID { get; private set; }

		[Column("NAME", Column.DataType.String, NotNull = true)]
		public string Name { get; set; }

		[Column("MODEL", Column.DataType.String)]
		public string Model { get; set; }

		[Column("SERIALNUMBER", Column.DataType.String)]
		public string SerialNumber { get; set; }

		[Column("COST", Column.DataType.Double)]
		public double Cost { get; set; }

		[Column("PURCHASEDATE", Column.DataType.DateTime)]
		public DateTime PurchaseDate { get; set; }

		[Column("DECOMMISIONDATE", Column.DataType.DateTime)]
		public DateTime DecommisionDate { get; set; }

		[Column("MAKE", Column.DataType.Guid)]
		public EquipmentMake Make { get; set; }

		[Column("TYPE", Column.DataType.Guid)]
		public EquipmentType Type { get; set; }

		public object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Name = Extentions.GetRandomString(8);
			PurchaseDate = DateTime.Now;
			Make = (EquipmentMake)new EquipmentMake().CreateTestObject();
			Type = (EquipmentType)new EquipmentType().CreateTestObject();

			return this;
		}
		
		public string DebugString()
		{
			return ToString();
		}

		public override string ToString()
		{
			return string.Format("Equipment Name: {0}, Type: {1}, Make: {2}", Name, Type, Make);
		}
		public static List<Equipment> CreateTestInstances(int amount)
		{
			return Enumerable.Range(1, amount).Cast<object>().Select(i => new Equipment().CreateTestObject()).Cast<Equipment>().ToList();
		}
	}

	[Table("TBL_EQUIPMENT_MAKE")]
	public struct EquipmentMake : ITestObject
	{
		[Column("ID", Column.DataType.Guid, PrimaryKey = true)]
		public Guid ID { get; private set; }

		[Column("NAME", Column.DataType.String, NotNull = true)]
		public string Name { get; set; }

		public object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Name = "Test Equipment Make";

			return this;
		}
		public override string ToString()
		{
			return Name;
		}
		public static List<EquipmentMake> CreateTestInstances(int amount)
		{
			return Enumerable.Range(1, amount).Cast<object>().Select(i => new EquipmentMake().CreateTestObject()).Cast<EquipmentMake>().ToList();
		}
	}

	[Table("TBL_EQUIPMENT_TYPE")]
	public struct EquipmentType : ITestObject
	{
		[Column("ID", Column.DataType.Guid, PrimaryKey = true)]
		public Guid ID { get; private set; }

		[Column("DESCRIPTION", Column.DataType.String, NotNull = true)]
		public string Description { get; set; }

		public object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Description = "Test Equipment Type";

			return this;
		}
		public override string ToString()
		{
			return Description;
		}
	}

	public static class ModelExtensions
	{
		public static T CreateInstance<T>(IAdapterReader reader) where T : new()
		{
			List<Column> columns = Column.GetAll(typeof(T));
			object t = new T();
			foreach (Column c in columns)
			{
				try
				{
					object value = reader.GetValue(c.Name);
					c.Property.SetValue(t, value);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			return (T)t;
		}

		public static void InsertTestObject<T>(this T item, IRWDataService dataService)
		{
			PerformInsertTestObject(item, dataService);
		}

		public static void PerformInsertTestObject<T>(T item, IRWDataService dataService)
		{
			Table t = Table.Get(typeof(T));
			t.GetTableTypeColumns().ForEach(c =>
			{
				dynamic value = c.Property.GetValue(item);
				PerformInsertTestObject(value, dataService);
			});

			dataService.InsertModel(item);
		}
	}
}

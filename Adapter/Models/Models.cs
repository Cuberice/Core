using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core;
using Core.Common;
using Core.Data;
using Core.Service;


namespace Models
{
	[DebuggerDisplay("{DebugString()}")]
	[Table("TBL_CORE_EQUIPMENT")]
	public struct CoreEquipment : ITestObject, IDebuggerObject
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
		public CoreEquipmentMake Make { get; set; }

		public object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Name = Extentions.GetRandomString(8);
			PurchaseDate = DateTime.Now;
			Make = (CoreEquipmentMake)new CoreEquipmentMake().CreateTestObject();
			
			return this;
		}

		public string DebugString()
		{
			return ToString();
		}

		public override string ToString()
		{
			return string.Format("Equipment Name: {0}, Make: {1}", Name, Make);
		}
		public static List<CoreEquipmentMake> CreateTestInstances(int amount)
		{
			return Enumerable.Range(1, amount).Cast<object>().Select(i => new CoreEquipmentMake().CreateTestObject()).Cast<CoreEquipmentMake>().ToList();
		}
	}

	[Table("TBL_CORE_EQUIPMENT_MAKE")]
	public struct CoreEquipmentMake : ITestObject
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
		public static List<CoreEquipmentMake> CreateTestInstances(int amount)
		{
			return Enumerable.Range(1, amount).Cast<object>().Select(i => new CoreEquipmentMake().CreateTestObject()).Cast<CoreEquipmentMake>().ToList();
		}
	}
}
